using System.Diagnostics;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;

/// <summary>
/// Generates deterministic CPU load for testing CPU metrics.
/// </summary>
public sealed class CpuLoadGenerator : IDisposable
{
    private readonly List<Thread> _threads = [];
    private volatile bool _shouldStop;
    private readonly object _lock = new();

    /// <summary>
    /// Generates CPU load with configurable intensity and duration.
    /// </summary>
    /// <param name="durationMs">Duration to maintain load in milliseconds.</param>
    /// <param name="threadCount">Number of worker threads. Defaults to processor count.</param>
    /// <param name="intensity">Load intensity 0.0-1.0 (1.0 = 100% busy loop).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task GenerateLoadAsync(
        int durationMs,
        int? threadCount = null,
        double intensity = 1.0,
        CancellationToken cancellationToken = default)
    {
        if (intensity is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(intensity), "Must be between 0.0 and 1.0");

        var workerCount = threadCount ?? Environment.ProcessorCount;
        var stopwatch = Stopwatch.StartNew();

        lock (_lock)
        {
            _shouldStop = false;

            for (var i = 0; i < workerCount; i++)
            {
                var thread = new Thread(() => CpuWorker(intensity, stopwatch, durationMs))
                {
                    IsBackground = true,
                    Name = $"CpuLoad-{i}"
                };
                _threads.Add(thread);
                thread.Start();
            }
        }

        // Wait for duration or cancellation
        try
        {
            await Task.Delay(durationMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled
        }
        finally
        {
            StopLoad();
        }
    }

    /// <summary>
    /// Stops all CPU load generation.
    /// </summary>
    public void StopLoad()
    {
        lock (_lock)
        {
            _shouldStop = true;

            foreach (var thread in _threads.Where(t => t.IsAlive))
            {
                if (!thread.Join(TimeSpan.FromSeconds(2)))
                {
                    // Thread didn't stop gracefully, but it's background so it won't block process exit
                }
            }

            _threads.Clear();
        }
    }

    private void CpuWorker(double intensity, Stopwatch stopwatch, int durationMs)
    {
        var random = new Random(Thread.CurrentThread.ManagedThreadId);
        var workMs = (int)(intensity * 100);
        var sleepMs = (int)((1.0 - intensity) * 100);

        while (!_shouldStop && stopwatch.ElapsedMilliseconds < durationMs)
        {
            // Busy work
            if (workMs > 0)
            {
                var start = Stopwatch.GetTimestamp();
                var targetTicks = start + (workMs * Stopwatch.Frequency / 1000);

                while (Stopwatch.GetTimestamp() < targetTicks && !_shouldStop)
                {
                    // CPU-intensive calculation
                    _ = Math.Sqrt(random.NextDouble() * Math.PI);
                }
            }

            // Sleep to control intensity
            if (sleepMs > 0 && !_shouldStop)
            {
                Thread.Sleep(sleepMs);
            }
        }
    }

    public void Dispose()
    {
        StopLoad();
    }
}
