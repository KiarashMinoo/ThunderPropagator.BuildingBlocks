namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;

/// <summary>
/// Generates disk I/O load for testing disk metrics.
/// </summary>
public sealed class DiskIoGenerator : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();
    private CancellationTokenSource? _cts;
    private Task? _ioTask;
    private volatile bool _isRunning;

    public DiskIoGenerator()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"DiskIoTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Starts continuous disk I/O operations.
    /// </summary>
    /// <param name="blockSizeKb">Block size in KB for each operation</param>
    /// <param name="operationsPerSecond">Target operations per second</param>
    /// <param name="readWriteRatio">Ratio of reads to writes (0.5 = 50% reads, 50% writes)</param>
    public void Start(int blockSizeKb = 64, int operationsPerSecond = 100, double readWriteRatio = 0.5)
    {
        if (_isRunning)
            throw new InvalidOperationException("Disk I/O generator is already running");

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _ioTask = Task.Run(async () => await GenerateIoAsync(blockSizeKb, operationsPerSecond, readWriteRatio, _cts.Token), _cts.Token);
    }

    /// <summary>
    /// Performs a single large write operation for testing write throughput.
    /// </summary>
    /// <param name="sizeInMb">Size of file to write in MB</param>
    /// <returns>Path to the created file</returns>
    public async Task<string> WriteLargeFileAsync(int sizeInMb = 100)
    {
        var filePath = Path.Combine(_testDirectory, $"largefile_{Guid.NewGuid():N}.tmp");
        var buffer = new byte[1024 * 1024]; // 1 MB buffer
        var random = new Random();

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 
            buffer.Length, FileOptions.WriteThrough);

        for (var i = 0; i < sizeInMb; i++)
        {
            random.NextBytes(buffer);
            await fs.WriteAsync(buffer);
        }

        await fs.FlushAsync();
        _createdFiles.Add(filePath);
        
        return filePath;
    }

    /// <summary>
    /// Reads a file multiple times to generate read I/O.
    /// </summary>
    /// <param name="filePath">Path to file to read</param>
    /// <param name="iterations">Number of times to read the file</param>
    public async Task ReadFileAsync(string filePath, int iterations = 5)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var buffer = new byte[1024 * 1024]; // 1 MB buffer

        for (var i = 0; i < iterations; i++)
        {
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 
                buffer.Length, FileOptions.SequentialScan);

            while (await fs.ReadAsync(buffer) > 0)
            {
                // Just read, don't process
            }
        }
    }

    /// <summary>
    /// Stops continuous disk I/O generation.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning || _cts == null || _ioTask == null)
            return;

        _isRunning = false;
        _cts.Cancel();

        try
        {
            await _ioTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            // Ignore timeout
        }
    }

    private async Task GenerateIoAsync(int blockSizeKb, int operationsPerSecond, double readWriteRatio, CancellationToken cancellationToken)
    {
        var buffer = new byte[blockSizeKb * 1024];
        var random = new Random();
        var delayMs = 1000 / operationsPerSecond;

        // Create initial files for reading
        for (var i = 0; i < 5; i++)
        {
            var filePath = Path.Combine(_testDirectory, $"testfile_{i}.tmp");
            await File.WriteAllBytesAsync(filePath, buffer, cancellationToken);
            _createdFiles.Add(filePath);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (random.NextDouble() < readWriteRatio && _createdFiles.Count > 0)
                {
                    // Read operation
                    var readFile = _createdFiles[random.Next(_createdFiles.Count)];
                    if (File.Exists(readFile))
                    {
                        await File.ReadAllBytesAsync(readFile, cancellationToken);
                    }
                }
                else
                {
                    // Write operation
                    var writeFile = Path.Combine(_testDirectory, $"testfile_{random.Next(10)}.tmp");
                    random.NextBytes(buffer);
                    await File.WriteAllBytesAsync(writeFile, buffer, cancellationToken);
                    
                    if (!_createdFiles.Contains(writeFile))
                        _createdFiles.Add(writeFile);
                }

                await Task.Delay(delayMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();

        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }

        _cts?.Dispose();
    }
}

