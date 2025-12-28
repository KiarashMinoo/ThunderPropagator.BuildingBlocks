using System.Security.Cryptography;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;

/// <summary>
/// Generates deterministic disk I/O for testing disk metrics.
/// </summary>
public sealed class DiskIoGenerator : IDisposable
{
    private readonly string _tempDirectory;
    private readonly List<string> _tempFiles = [];
    private readonly object _lock = new();

    public DiskIoGenerator()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"DiskIoTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Generates disk write I/O.
    /// </summary>
    /// <param name="totalMegabytes">Total amount to write in MB.</param>
    /// <param name="blockSizeKb">Block size for each write in KB.</param>
    /// <param name="flushEachBlock">Whether to flush after each block (tests write latency).</param>
    /// <returns>Path to the written file.</returns>
    public async Task<string> GenerateWriteIoAsync(
        int totalMegabytes,
        int blockSizeKb = 64,
        bool flushEachBlock = false)
    {
        var filePath = Path.Combine(_tempDirectory, $"write_{Guid.NewGuid():N}.dat");
        lock (_lock)
        {
            _tempFiles.Add(filePath);
        }

        var blockSize = blockSizeKb * 1024;
        var totalBytes = totalMegabytes * 1024 * 1024;
        var blocksToWrite = totalBytes / blockSize;
        var buffer = new byte[blockSize];

        // Fill buffer with random data for realism
        RandomNumberGenerator.Fill(buffer);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: blockSize,
            useAsync: true);

        for (var i = 0; i < blocksToWrite; i++)
        {
            await fileStream.WriteAsync(buffer);

            if (flushEachBlock)
            {
                await fileStream.FlushAsync();
            }
        }

        await fileStream.FlushAsync();
        return filePath;
    }

    /// <summary>
    /// Generates disk read I/O.
    /// </summary>
    /// <param name="filePath">Path to file to read.</param>
    /// <param name="blockSizeKb">Block size for each read in KB.</param>
    /// <returns>Total bytes read.</returns>
    public async Task<long> GenerateReadIoAsync(string filePath, int blockSizeKb = 64)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Test file not found", filePath);

        var blockSize = blockSizeKb * 1024;
        var buffer = new byte[blockSize];
        long totalRead = 0;

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: blockSize,
            useAsync: true);

        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
        {
            totalRead += bytesRead;
        }

        return totalRead;
    }

    /// <summary>
    /// Generates mixed read/write I/O load.
    /// </summary>
    /// <param name="durationMs">Duration to maintain I/O load.</param>
    /// <param name="fileSizeMb">Size of test file in MB.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task GenerateMixedIoAsync(
        int durationMs,
        int fileSizeMb = 50,
        CancellationToken cancellationToken = default)
    {
        var endTime = DateTime.UtcNow.AddMilliseconds(durationMs);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            // Write
            var filePath = await GenerateWriteIoAsync(fileSizeMb, blockSizeKb: 64, flushEachBlock: false);

            // Read
            await GenerateReadIoAsync(filePath, blockSizeKb: 64);

            // Small delay to prevent overwhelming the disk
            await Task.Delay(100, cancellationToken);
        }
    }

    /// <summary>
    /// Gets temporary directory used for tests.
    /// </summary>
    public string TempDirectory => _tempDirectory;

    public void Dispose()
    {
        // Clean up temp files
        try
        {
            lock (_lock)
            {
                foreach (var file in _tempFiles.Where(File.Exists))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                _tempFiles.Clear();
            }

            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
