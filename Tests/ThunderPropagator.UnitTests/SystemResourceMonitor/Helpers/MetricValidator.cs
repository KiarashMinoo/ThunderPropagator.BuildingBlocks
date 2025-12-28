using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Helpers;

/// <summary>
/// Validates metric changes between baseline and load scenarios with tolerances.
/// </summary>
public static class MetricValidator
{
    /// <summary>
    /// Platform-specific tolerance configuration.
    /// </summary>
    public static class Tolerances
    {
        // CPU tolerances
        public static double CpuUsageMinIncrease => OperatingSystem.IsWindows() ? 5.0 : 3.0; // %
        public static double CpuUsageExpectedUnderLoad => 30.0; // % minimum expected during heavy load

        // Memory tolerances  
        public static double MemoryMinIncreaseMB => 10.0; // MB
        public static double MemoryUsagePercentMaxVariance => 5.0; // %

        // Disk tolerances
        public static double DiskThroughputMinIncreaseMBps => 1.0; // MB/s
        public static double DiskIOPSMinIncrease => 10.0; // IOPS

        // Thread tolerances
        public static int ThreadCountMinIncrease => 1;
        public static int ProcessCountMinVariance => 5; // processes can come and go

        // Timing tolerances
        public static double TimingTolerancePercent => 20.0; // Allow 20% variance in timing
    }

    /// <summary>
    /// Asserts that CPU usage increased under load.
    /// </summary>
    public static void AssertCpuUsageIncreased(
        MetricSample baseline,
        MetricSample load,
        double? customMinIncrease = null,
        string? message = null)
    {
        var minIncrease = customMinIncrease ?? Tolerances.CpuUsageMinIncrease;
        var increase = load.CpuUsageAvg - baseline.CpuUsageAvg;

        if (increase < minIncrease)
        {
            throw new AssertionException(
                message ?? $"CPU usage did not increase enough. " +
                $"Baseline: {baseline.CpuUsageAvg:F2}%, Load: {load.CpuUsageAvg:F2}%, " +
                $"Increase: {increase:F2}% (expected >= {minIncrease}%)");
        }
    }

    /// <summary>
    /// Asserts that CPU usage returned to near baseline after cooldown.
    /// </summary>
    public static void AssertCpuCooledDown(
        MetricSample baseline,
        MetricSample cooldown,
        double tolerancePercent = 10.0,
        string? message = null)
    {
        var difference = Math.Abs(cooldown.CpuUsageAvg - baseline.CpuUsageAvg);

        if (difference > tolerancePercent)
        {
            throw new AssertionException(
                message ?? $"CPU did not cool down to baseline. " +
                $"Baseline: {baseline.CpuUsageAvg:F2}%, Cooldown: {cooldown.CpuUsageAvg:F2}%, " +
                $"Difference: {difference:F2}% (tolerance: {tolerancePercent}%)");
        }
    }

    /// <summary>
    /// Asserts that memory usage increased under load.
    /// </summary>
    public static void AssertMemoryUsageIncreased(
        MetricSample baseline,
        MetricSample load,
        double? customMinIncreaseMB = null,
        string? message = null)
    {
        var minIncrease = customMinIncreaseMB ?? Tolerances.MemoryMinIncreaseMB;
        var increase = load.MemoryUsedAvg - baseline.MemoryUsedAvg;

        if (increase < minIncrease)
        {
            throw new AssertionException(
                message ?? $"Memory usage did not increase enough. " +
                $"Baseline: {baseline.MemoryUsedAvg:F2} MB, Load: {load.MemoryUsedAvg:F2} MB, " +
                $"Increase: {increase:F2} MB (expected >= {minIncrease} MB)");
        }
    }

    /// <summary>
    /// Asserts that thread count increased.
    /// </summary>
    public static void AssertThreadCountIncreased(
        MetricSample baseline,
        MetricSample load,
        int? customMinIncrease = null,
        string? message = null)
    {
        var minIncrease = customMinIncrease ?? Tolerances.ThreadCountMinIncrease;
        var increase = load.ThreadCountAvg - baseline.ThreadCountAvg;

        if (increase < minIncrease)
        {
            throw new AssertionException(
                message ?? $"Thread count did not increase enough. " +
                $"Baseline: {baseline.ThreadCountAvg}, Load: {load.ThreadCountAvg}, " +
                $"Increase: {increase} (expected >= {minIncrease})");
        }
    }

    /// <summary>
    /// Asserts that disk throughput increased under load.
    /// </summary>
    public static void AssertDiskThroughputIncreased(
        MetricSample baseline,
        MetricSample load,
        bool checkRead = true,
        bool checkWrite = true,
        double? customMinIncreaseMBps = null,
        string? message = null)
    {
        var minIncrease = customMinIncreaseMBps ?? Tolerances.DiskThroughputMinIncreaseMBps;

        if (checkRead && baseline.DiskReadThroughputAvg.HasValue && load.DiskReadThroughputAvg.HasValue)
        {
            var readIncrease = load.DiskReadThroughputAvg.Value - baseline.DiskReadThroughputAvg.Value;
            if (readIncrease < minIncrease)
            {
                throw new AssertionException(
                    message ?? $"Disk read throughput did not increase enough. " +
                    $"Baseline: {baseline.DiskReadThroughputAvg:F2} MB/s, Load: {load.DiskReadThroughputAvg:F2} MB/s, " +
                    $"Increase: {readIncrease:F2} MB/s (expected >= {minIncrease} MB/s)");
            }
        }

        if (checkWrite && baseline.DiskWriteThroughputAvg.HasValue && load.DiskWriteThroughputAvg.HasValue)
        {
            var writeIncrease = load.DiskWriteThroughputAvg.Value - baseline.DiskWriteThroughputAvg.Value;
            if (writeIncrease < minIncrease)
            {
                throw new AssertionException(
                    message ?? $"Disk write throughput did not increase enough. " +
                    $"Baseline: {baseline.DiskWriteThroughputAvg:F2} MB/s, Load: {load.DiskWriteThroughputAvg:F2} MB/s, " +
                    $"Increase: {writeIncrease:F2} MB/s (expected >= {minIncrease} MB/s)");
            }
        }
    }

    /// <summary>
    /// Asserts that a value is within expected range.
    /// </summary>
    public static void AssertInRange(
        double value,
        double minValue,
        double maxValue,
        string metricName,
        string? message = null)
    {
        if (value < minValue || value > maxValue)
        {
            throw new AssertionException(
                message ?? $"{metricName} out of range. " +
                $"Value: {value:F2}, Expected: [{minValue:F2}, {maxValue:F2}]");
        }
    }

    /// <summary>
    /// Asserts that a metric is stable (low variance) over time.
    /// </summary>
    public static void AssertStable(
        MetricSample sample,
        Func<SystemResourceMonitorMetrics, double> selector,
        double maxStdDevPercent = 10.0,
        string? metricName = null,
        string? message = null)
    {
        var values = sample.Samples.Select(selector).ToArray();
        var avg = values.Average();
        var variance = values.Sum(v => Math.Pow(v - avg, 2)) / values.Length;
        var stdDev = Math.Sqrt(variance);
        var stdDevPercent = avg > 0 ? (stdDev / avg) * 100.0 : 0;

        if (stdDevPercent > maxStdDevPercent)
        {
            throw new AssertionException(
                message ?? $"{metricName ?? "Metric"} not stable. " +
                $"Avg: {avg:F2}, StdDev: {stdDev:F2} ({stdDevPercent:F2}%), " +
                $"Max allowed: {maxStdDevPercent}%");
        }
    }
}

/// <summary>
/// Custom exception for metric assertion failures.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
