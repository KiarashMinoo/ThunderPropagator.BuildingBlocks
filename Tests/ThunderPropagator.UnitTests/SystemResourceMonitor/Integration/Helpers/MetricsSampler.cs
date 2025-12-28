using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.Helpers;

/// <summary>
/// Utility for sampling system resource metrics over time windows.
/// </summary>
public class MetricsSampler
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly List<SystemResourceMonitorMetrics> _samples = new();

    public MetricsSampler(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    /// <summary>
    /// Collects metric samples over a time window.
    /// </summary>
    /// <param name="windowMs">Sampling window in milliseconds</param>
    /// <param name="durationMs">Total duration to sample</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<MetricsSample> CollectSamplesAsync(
        int windowMs = 500,
        int durationMs = 5000,
        CancellationToken cancellationToken = default)
    {
        _samples.Clear();
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMilliseconds(durationMs);

        // Collect first sample immediately
        var metrics = await _monitor.GetMetricsAsync(windowMs, null, cancellationToken);
        _samples.Add(metrics);

        // Continue collecting samples at intervals until duration expires
        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(windowMs, cancellationToken);
            
            // Only collect if we're still within the duration
            if (DateTime.UtcNow < endTime)
            {
                metrics = await _monitor.GetMetricsAsync(windowMs, null, cancellationToken);
                _samples.Add(metrics);
            }
        }

        return new MetricsSample(new List<SystemResourceMonitorMetrics>(_samples));
    }

    /// <summary>
    /// Clears collected samples.
    /// </summary>
    public void Clear() => _samples.Clear();
}

/// <summary>
/// Represents a collection of metric samples with aggregation methods.
/// </summary>
public class MetricsSample
{
    private readonly List<SystemResourceMonitorMetrics> _samples;

    public MetricsSample(List<SystemResourceMonitorMetrics> samples)
    {
        _samples = samples;
    }

    public int Count => _samples.Count;
    public IReadOnlyList<SystemResourceMonitorMetrics> Samples => _samples;

    /// <summary>
    /// Gets CPU usage statistics.
    /// </summary>
    public (double Min, double Max, double Avg) CpuUsage()
    {
        var values = _samples.Select(s => s.Cpu.Usage).ToList();
        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets memory used statistics in bytes.
    /// </summary>
    public (double Min, double Max, double Avg) MemoryUsed()
    {
        var values = _samples.Select(s => s.Memory.Used).ToList();
        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets memory usage percentage statistics.
    /// </summary>
    public (double Min, double Max, double Avg) MemoryUsagePercent()
    {
        var values = _samples.Select(s => s.Memory.UsagePercentage).ToList();
        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets thread count statistics.
    /// </summary>
    public (long Min, long Max, double Avg) ThreadCount()
    {
        var values = _samples.Select(s => s.Cpu.Threads).ToList();
        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets disk read throughput statistics for a specific drive.
    /// </summary>
    public (double? Min, double? Max, double? Avg) DiskReadThroughput(string driveId)
    {
        var values = _samples
            .SelectMany(s => s.DiskSpeed)
            .Where(d => d.DriveId == driveId && d.ReadThroughputMBps.HasValue)
            .Select(d => d.ReadThroughputMBps!.Value)
            .ToList();

        if (values.Count == 0)
            return (null, null, null);

        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets disk write throughput statistics for a specific drive.
    /// </summary>
    public (double? Min, double? Max, double? Avg) DiskWriteThroughput(string driveId)
    {
        var values = _samples
            .SelectMany(s => s.DiskSpeed)
            .Where(d => d.DriveId == driveId && d.WriteThroughputMBps.HasValue)
            .Select(d => d.WriteThroughputMBps!.Value)
            .ToList();

        if (values.Count == 0)
            return (null, null, null);

        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets CPU temperature statistics (if available).
    /// </summary>
    public (double? Min, double? Max, double? Avg) CpuTemperature()
    {
        var values = _samples
            .Where(s => s.CpuTemperature?.AverageTemperatureCelsius.HasValue == true)
            .Select(s => s.CpuTemperature!.AverageTemperatureCelsius!.Value)
            .ToList();

        if (values.Count == 0)
            return (null, null, null);

        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets battery charge percentage statistics (if available).
    /// </summary>
    public (double? Min, double? Max, double? Avg) BatteryChargePercent()
    {
        var values = _samples
            .Where(s => s.Battery?.ChargePercent.HasValue == true)
            .Select(s => s.Battery!.ChargePercent!.Value)
            .ToList();

        if (values.Count == 0)
            return (null, null, null);

        return (values.Min(), values.Max(), values.Average());
    }

    /// <summary>
    /// Gets GPU utilization statistics for a specific GPU (if available).
    /// </summary>
    public (double? Min, double? Max, double? Avg) GpuUtilization(int gpuIndex = 0)
    {
        var values = _samples
            .SelectMany(s => s.Gpus)
            .Where(g => g.GpuIndex == gpuIndex && g.UtilizationPercent.HasValue)
            .Select(g => g.UtilizationPercent!.Value)
            .ToList();

        if (values.Count == 0)
            return (null, null, null);

        return (values.Min(), values.Max(), values.Average());
    }
}

