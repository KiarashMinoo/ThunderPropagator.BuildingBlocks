namespace RapidStreamer.UnitTests.SystemResourceMonitor.Integration.Helpers;

/// <summary>
/// Validates metric changes between baseline and load samples with configurable tolerances.
/// </summary>
public static class MetricsValidator
{
    /// <summary>
    /// Asserts that CPU usage increased significantly under load.
    /// </summary>
    public static void AssertCpuUsageIncreased(
        MetricsSample baseline,
        MetricsSample load,
        double minIncreasePct = 20.0)
    {
        var baselineAvg = baseline.CpuUsage().Avg;
        var loadAvg = load.CpuUsage().Avg;

        var increase = loadAvg - baselineAvg;

        if (increase < minIncreasePct)
        {
            throw new AssertionException(
                $"CPU usage did not increase sufficiently. " +
                $"Baseline: {baselineAvg:F2}%, Load: {loadAvg:F2}%, " +
                $"Increase: {increase:F2}% (expected >= {minIncreasePct}%)");
        }
    }

    /// <summary>
    /// Asserts that memory usage increased under load.
    /// </summary>
    public static void AssertMemoryUsageIncreased(
        MetricsSample baseline,
        MetricsSample load,
        double minIncreaseMB = 10.0)
    {
        var baselineAvg = baseline.MemoryUsed().Avg / (1024 * 1024); // Convert to MB
        var loadAvg = load.MemoryUsed().Avg / (1024 * 1024);

        var increaseMb = loadAvg - baselineAvg;

        if (increaseMb < minIncreaseMB)
        {
            throw new AssertionException(
                $"Memory usage did not increase sufficiently. " +
                $"Baseline: {baselineAvg:F2} MB, Load: {loadAvg:F2} MB, " +
                $"Increase: {increaseMb:F2} MB (expected >= {minIncreaseMB} MB)");
        }
    }

    /// <summary>
    /// Asserts that thread count increased under load.
    /// </summary>
    public static void AssertThreadCountIncreased(
        MetricsSample baseline,
        MetricsSample load,
        int minIncrease = 5)
    {
        var baselineAvg = baseline.ThreadCount().Avg;
        var loadAvg = load.ThreadCount().Avg;

        var increase = loadAvg - baselineAvg;

        if (increase < minIncrease)
        {
            throw new AssertionException(
                $"Thread count did not increase sufficiently. " +
                $"Baseline: {baselineAvg:F1}, Load: {loadAvg:F1}, " +
                $"Increase: {increase:F1} (expected >= {minIncrease})");
        }
    }

    /// <summary>
    /// Asserts that disk throughput increased under load.
    /// </summary>
    public static void AssertDiskThroughputIncreased(
        MetricsSample baseline,
        MetricsSample load,
        string driveId,
        bool checkRead = true,
        bool checkWrite = true,
        double minIncreaseMBps = 1.0)
    {
        if (checkRead)
        {
            var baselineRead = baseline.DiskReadThroughput(driveId).Avg ?? 0;
            var loadRead = load.DiskReadThroughput(driveId).Avg ?? 0;
            var readIncrease = loadRead - baselineRead;

            if (readIncrease < minIncreaseMBps)
            {
                throw new AssertionException(
                    $"Disk read throughput did not increase sufficiently for {driveId}. " +
                    $"Baseline: {baselineRead:F2} MB/s, Load: {loadRead:F2} MB/s, " +
                    $"Increase: {readIncrease:F2} MB/s (expected >= {minIncreaseMBps} MB/s)");
            }
        }

        if (checkWrite)
        {
            var baselineWrite = baseline.DiskWriteThroughput(driveId).Avg ?? 0;
            var loadWrite = load.DiskWriteThroughput(driveId).Avg ?? 0;
            var writeIncrease = loadWrite - baselineWrite;

            if (writeIncrease < minIncreaseMBps)
            {
                throw new AssertionException(
                    $"Disk write throughput did not increase sufficiently for {driveId}. " +
                    $"Baseline: {baselineWrite:F2} MB/s, Load: {loadWrite:F2} MB/s, " +
                    $"Increase: {writeIncrease:F2} MB/s (expected >= {minIncreaseMBps} MB/s)");
            }
        }
    }

    /// <summary>
    /// Asserts that a metric remains stable (within tolerance).
    /// </summary>
    public static void AssertMetricStable(
        double baselineValue,
        double currentValue,
        double maxChangePercent = 10.0,
        string metricName = "Metric")
    {
        var changePercent = Math.Abs((currentValue - baselineValue) / baselineValue * 100);

        if (changePercent > maxChangePercent)
        {
            throw new AssertionException(
                $"{metricName} changed more than expected. " +
                $"Baseline: {baselineValue:F2}, Current: {currentValue:F2}, " +
                $"Change: {changePercent:F2}% (max allowed: {maxChangePercent}%)");
        }
    }

    /// <summary>
    /// Asserts that a value is within expected range.
    /// </summary>
    public static void AssertInRange(
        double value,
        double min,
        double max,
        string metricName = "Metric")
    {
        if (value < min || value > max)
        {
            throw new AssertionException(
                $"{metricName} is out of expected range. " +
                $"Value: {value:F2}, Expected range: [{min:F2}, {max:F2}]");
        }
    }

    /// <summary>
    /// Asserts that temperature increased (if supported).
    /// </summary>
    public static void AssertTemperatureIncreased(
        MetricsSample baseline,
        MetricsSample load,
        double minIncreaseCelsius = 1.0)
    {
        var baselineTemp = baseline.CpuTemperature().Avg;
        var loadTemp = load.CpuTemperature().Avg;

        if (!baselineTemp.HasValue || !loadTemp.HasValue)
        {
            // Temperature not supported, skip validation
            return;
        }

        var increase = loadTemp.Value - baselineTemp.Value;

        if (increase < minIncreaseCelsius)
        {
            throw new AssertionException(
                $"CPU temperature did not increase sufficiently. " +
                $"Baseline: {baselineTemp:F2}°C, Load: {loadTemp:F2}°C, " +
                $"Increase: {increase:F2}°C (expected >= {minIncreaseCelsius}°C)");
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}

