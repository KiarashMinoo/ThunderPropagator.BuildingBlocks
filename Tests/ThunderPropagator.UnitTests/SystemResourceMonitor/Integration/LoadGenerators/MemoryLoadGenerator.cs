namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;

/// <summary>
/// Generates memory load for testing memory metrics.
/// </summary>
public sealed class MemoryLoadGenerator : IDisposable
{
    private readonly List<byte[]> _retainedBuffers = new();
    private readonly List<byte[]> _churnBuffers = new();
    private CancellationTokenSource? _cts;
    private Task? _churnTask;
    private volatile bool _isRunning;

    /// <summary>
    /// Allocates memory and retains it (no churn).
    /// </summary>
    /// <param name="sizeInMB">Amount of memory to allocate in MB</param>
    public void AllocateAndRetain(int sizeInMB)
    {
        const int oneMB = 1024 * 1024;
        var buffer = new byte[sizeInMB * oneMB];
        
        // Touch the memory to ensure it's committed
        for (var i = 0; i < buffer.Length; i += 4096)
        {
            buffer[i] = 1;
        }

        _retainedBuffers.Add(buffer);
    }

    /// <summary>
    /// Starts continuous memory allocation/deallocation (churn) to generate GC pressure.
    /// </summary>
    /// <param name="churnSizePerIterationMb">Size to allocate/release per iteration</param>
    /// <param name="intervalMs">Interval between allocations in milliseconds</param>
    public void StartChurn(int churnSizePerIterationMb = 10, int intervalMs = 100)
    {
        if (_isRunning)
            throw new InvalidOperationException("Churn is already running");

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _churnTask = Task.Run(async () =>
        {
            const int oneMb = 1024 * 1024;

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Allocate
                    var buffer = new byte[churnSizePerIterationMb * oneMb];
                    for (var i = 0; i < buffer.Length; i += 4096)
                    {
                        buffer[i] = (byte)(i % 256);
                    }

                    // Keep some buffers to maintain pressure
                    _churnBuffers.Add(buffer);

                    // Release older buffers to create churn
                    if (_churnBuffers.Count > 5)
                    {
                        _churnBuffers.RemoveAt(0);
                    }

                    await Task.Delay(intervalMs, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, _cts.Token);
    }

    /// <summary>
    /// Stops memory churn.
    /// </summary>
    public async Task StopChurnAsync()
    {
        if (!_isRunning || _cts == null || _churnTask == null)
            return;

        _isRunning = false;
        _cts.Cancel();

        try
        {
            await _churnTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            // Ignore timeout
        }

        _churnBuffers.Clear();
    }

    /// <summary>
    /// Releases all retained memory.
    /// </summary>
    public void ReleaseAll()
    {
        _retainedBuffers.Clear();
        _churnBuffers.Clear();
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
    }

    public void Dispose()
    {
        StopChurnAsync().GetAwaiter().GetResult();
        ReleaseAll();
        _cts?.Dispose();
    }
}

