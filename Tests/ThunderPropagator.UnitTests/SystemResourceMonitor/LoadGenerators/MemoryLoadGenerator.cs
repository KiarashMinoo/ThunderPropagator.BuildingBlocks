namespace ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;

/// <summary>
/// Generates deterministic memory pressure for testing memory metrics.
/// </summary>
public sealed class MemoryLoadGenerator : IDisposable
{
    private readonly List<byte[]> _allocations = [];
    private readonly object _lock = new();
    private volatile bool _shouldChurn;
    private Thread? _churnThread;

    /// <summary>
    /// Allocates and retains memory.
    /// </summary>
    /// <param name="totalMegabytes">Total memory to allocate in MB.</param>
    /// <param name="blockSizeMb">Size of each allocation block in MB. Defaults to 10MB.</param>
    public void AllocateMemory(int totalMegabytes, int blockSizeMb = 10)
    {
        if (totalMegabytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalMegabytes), "Must be positive");

        lock (_lock)
        {
            var blockSize = blockSizeMb * 1024 * 1024;
            var blocksNeeded = (totalMegabytes * 1024 * 1024) / blockSize;

            for (var i = 0; i < blocksNeeded; i++)
            {
                var buffer = new byte[blockSize];
                // Touch memory to ensure it's committed
                for (var j = 0; j < buffer.Length; j += 4096)
                {
                    buffer[j] = (byte)(i % 256);
                }
                _allocations.Add(buffer);
            }
        }
    }

    /// <summary>
    /// Allocates memory with continuous churn (allocate and release).
    /// </summary>
    /// <param name="durationMs">Duration to maintain churn.</param>
    /// <param name="churnRateMbPerSecond">MB per second to allocate and release.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ChurnMemoryAsync(
        int durationMs,
        int churnRateMbPerSecond = 50,
        CancellationToken cancellationToken = default)
    {
        _shouldChurn = true;
        var churnInterval = 100; // ms between churn cycles
        var bytesPerCycle = (churnRateMbPerSecond * 1024 * 1024 * churnInterval) / 1000;

        _churnThread = new Thread(() =>
        {
            var random = new Random();
            while (_shouldChurn)
            {
                try
                {
                    // Allocate
                    var buffer = new byte[bytesPerCycle];
                    for (var i = 0; i < buffer.Length; i += 4096)
                    {
                        buffer[i] = (byte)random.Next(256);
                    }

                    lock (_lock)
                    {
                        _allocations.Add(buffer);

                        // Release oldest if we have too many
                        if (_allocations.Count > 20)
                        {
                            _allocations.RemoveAt(0);
                        }
                    }

                    Thread.Sleep(churnInterval);
                }
                catch (OutOfMemoryException)
                {
                    // Stop churning if we hit OOM
                    break;
                }
            }
        })
        {
            IsBackground = true,
            Name = "MemoryChurn"
        };

        _churnThread.Start();

        try
        {
            await Task.Delay(durationMs, cancellationToken);
        }
        finally
        {
            StopChurn();
        }
    }

    /// <summary>
    /// Releases all allocated memory.
    /// </summary>
    public void ReleaseMemory()
    {
        lock (_lock)
        {
            _allocations.Clear();
        }

        // Force collection to ensure memory is released
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Gets current allocated memory in bytes.
    /// </summary>
    public long GetAllocatedBytes()
    {
        lock (_lock)
        {
            return _allocations.Sum(a => (long)a.Length);
        }
    }

    private void StopChurn()
    {
        _shouldChurn = false;
        if (_churnThread?.IsAlive == true)
        {
            _churnThread.Join(TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        StopChurn();
        ReleaseMemory();
    }
}
