using System.Diagnostics;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;

/// <summary>
/// Generates CPU load for testing CPU metrics.
/// </summary>
public sealed class CpuLoadGenerator : IDisposable
{
    private CancellationTokenSource? _cts;
    private readonly List<Task> _tasks = new();
    private volatile bool _isRunning;

    /// <summary>
    /// Starts generating CPU load with specified parameters.
    /// </summary>
    /// <param name="threadCount">Number of threads to spawn (default: number of processors)</param>
    /// <param name="targetUtilizationPercent">Target CPU utilization percent (0-100, default: 80)</param>
    public void Start(int? threadCount = null, int targetUtilizationPercent = 80)
    {
        if (_isRunning)
            throw new InvalidOperationException("Load generator is already running");

        _cts = new CancellationTokenSource();
        _isRunning = true;

        var threads = threadCount ?? Environment.ProcessorCount;
        var cyclesPerInterval = CalculateCyclesForUtilization(targetUtilizationPercent);

        for (var i = 0; i < threads; i++)
        {
            var task = Task.Factory.StartNew(
                () => GenerateLoad(cyclesPerInterval, _cts.Token),
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            
            _tasks.Add(task);
        }
    }

    /// <summary>
    /// Stops the CPU load generation and waits for all threads to complete.
    /// </summary>
    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (!_isRunning || _cts == null)
            return;

        _isRunning = false;
        _cts.Cancel();

        var timeoutTs = timeout ?? TimeSpan.FromSeconds(5);
        await Task.WhenAny(
            Task.WhenAll(_tasks),
            Task.Delay(timeoutTs));

        _tasks.Clear();
    }

    private static void GenerateLoad(int cyclesPerInterval, CancellationToken cancellationToken)
    {
        const int intervalMs = 100;
        var sw = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            sw.Restart();

            // Busy work
            var dummy = 0.0;
            for (var i = 0; i < cyclesPerInterval; i++)
            {
                dummy += Math.Sqrt(i);
            }

            // Prevent optimization
            if (dummy < 0)
                Console.WriteLine(dummy);

            // Sleep for the remaining time to target the desired utilization
            var elapsed = sw.ElapsedMilliseconds;
            var sleepTime = intervalMs - elapsed;
            if (sleepTime > 0)
            {
                Thread.Sleep((int)sleepTime);
            }
        }
    }

    private static int CalculateCyclesForUtilization(int targetPercent)
    {
        // Calibrate cycles based on target utilization
        // Higher utilization = more cycles
        var baseCycles = 1_000_000;
        return baseCycles * targetPercent / 100;
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
    }
}

