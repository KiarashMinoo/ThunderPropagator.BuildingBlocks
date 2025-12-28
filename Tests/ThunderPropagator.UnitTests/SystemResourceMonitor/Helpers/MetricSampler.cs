using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Helpers;

/// <summary>
/// Helper for sampling and validating Resource Monitor metrics over time windows.
/// </summary>
public sealed class MetricSampler
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly List<SystemResourceMonitorMetrics> _samples = [];

    public MetricSampler(ISystemResourceMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    /// <summary>
    /// Collects metrics samples over a time window.
    /// </summary>
    /// <param name="windowMs">Total sampling window in milliseconds.</param>
    /// <param name="intervalMs">Interval between samples in milliseconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collected samples.</returns>
    public async Task<MetricSample> CollectSamplesAsync(
        int windowMs,
        int intervalMs = 100,
        CancellationToken cancellationToken = default)
    {
        _samples.Clear();
        var endTime = DateTime.UtcNow.AddMilliseconds(windowMs);
        var sampleCount = 0;

        // Collect first sample immediately
        var metrics = await _monitor.GetMetricsAsync(window: intervalMs, cancellationToken: cancellationToken);
        _samples.Add(metrics);
        sampleCount++;

        // Continue collecting samples at intervals until window expires
        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(intervalMs, cancellationToken);
            
            // Only collect if we're still within the window
            if (DateTime.UtcNow < endTime)
            {
                metrics = await _monitor.GetMetricsAsync(window: intervalMs, cancellationToken: cancellationToken);
                _samples.Add(metrics);
                sampleCount++;
            }
        }

        return new MetricSample(_samples.ToArray(), windowMs, intervalMs);
    }

    /// <summary>
    /// Clears collected samples.
    /// </summary>
    public void Clear()
    {
        _samples.Clear();
    }
}

/// <summary>
/// Represents a collection of metric samples with aggregation functions.
/// </summary>
public sealed class MetricSample
{
    private readonly SystemResourceMonitorMetrics[] _samples;

    public MetricSample(SystemResourceMonitorMetrics[] samples, int windowMs, int intervalMs)
    {
        _samples = samples ?? throw new ArgumentNullException(nameof(samples));
        WindowMs = windowMs;
        IntervalMs = intervalMs;
    }

    public int WindowMs { get; }
    public int IntervalMs { get; }
    public int Count => _samples.Length;
    public SystemResourceMonitorMetrics[] Samples => _samples;

    // CPU Aggregations
    public double CpuUsageAvg => _samples.Average(s => s.Cpu.Usage);
    public double CpuUsageMax => _samples.Max(s => s.Cpu.Usage);
    public double CpuUsageMin => _samples.Min(s => s.Cpu.Usage);
    public long ThreadCountAvg => (long)_samples.Average(s => s.Cpu.Threads);
    public long ThreadCountMax => _samples.Max(s => s.Cpu.Threads);
    public long ProcessCountAvg => (long)_samples.Average(s => s.Cpu.Processes);

    // Memory Aggregations
    public double MemoryUsedAvg => _samples.Average(s => s.Memory.Used);
    public double MemoryUsedMax => _samples.Max(s => s.Memory.Used);
    public double MemoryUsedMin => _samples.Min(s => s.Memory.Used);
    public double MemoryFreeAvg => _samples.Average(s => s.Memory.Free);
    public double MemoryFreeMin => _samples.Min(s => s.Memory.Free);
    public double MemoryUsagePercentAvg => _samples.Average(s => s.Memory.UsagePercentage);

    // Disk Speed Aggregations (for first drive if available)
    public double? DiskReadThroughputAvg =>
        _samples.Where(s => s.DiskSpeed.Length > 0 && s.DiskSpeed[0].ReadThroughputMBps.HasValue)
            .Select(s => s.DiskSpeed[0].ReadThroughputMBps!.Value)
            .DefaultIfEmpty()
            .Average();

    public double? DiskWriteThroughputAvg =>
        _samples.Where(s => s.DiskSpeed.Length > 0 && s.DiskSpeed[0].WriteThroughputMBps.HasValue)
            .Select(s => s.DiskSpeed[0].WriteThroughputMBps!.Value)
            .DefaultIfEmpty()
            .Average();

    public double? DiskReadIOPSAvg =>
        _samples.Where(s => s.DiskSpeed.Length > 0 && s.DiskSpeed[0].ReadIOPS.HasValue)
            .Select(s => s.DiskSpeed[0].ReadIOPS!.Value)
            .DefaultIfEmpty()
            .Average();

    public double? DiskWriteIOPSAvg =>
        _samples.Where(s => s.DiskSpeed.Length > 0 && s.DiskSpeed[0].WriteIOPS.HasValue)
            .Select(s => s.DiskSpeed[0].WriteIOPS!.Value)
            .DefaultIfEmpty()
            .Average();

    // Drive Space Aggregations (for first drive if available)
    public double? DriveUsedAvg =>
        _samples.Where(s => s.Drives.Length > 0)
            .Select(s => s.Drives[0].Used)
            .DefaultIfEmpty()
            .Average();

    public double? DriveFreeMin =>
        _samples.Where(s => s.Drives.Length > 0)
            .Select(s => s.Drives[0].Free)
            .DefaultIfEmpty()
            .Min();

    /// <summary>
    /// Gets percentile value from samples.
    /// </summary>
    public double GetPercentile(Func<SystemResourceMonitorMetrics, double> selector, double percentile)
    {
        if (percentile is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Must be 0-100");

        var values = _samples.Select(selector).OrderBy(v => v).ToArray();
        if (values.Length == 0) return 0;

        var index = (int)Math.Ceiling(percentile / 100.0 * values.Length) - 1;
        return values[Math.Max(0, Math.Min(index, values.Length - 1))];
    }
}
