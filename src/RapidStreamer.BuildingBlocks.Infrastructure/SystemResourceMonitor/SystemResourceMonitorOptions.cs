namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

/// <summary>
/// Configuration options for the system resource monitor.
/// </summary>
public sealed class SystemResourceMonitorOptions
{
    /// <summary>
    /// Enable/disable CPU metrics collection (usage, threads, processes).
    /// Default: true
    /// </summary>
    public bool EnableCpuMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable CPU temperature metrics collection.
    /// Default: true
    /// </summary>
    public bool EnableCpuTemperature { get; set; } = true;

    /// <summary>
    /// Enable/disable memory metrics collection.
    /// Default: true
    /// </summary>
    public bool EnableMemoryMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable disk space metrics collection.
    /// Default: true
    /// </summary>
    public bool EnableDiskSpaceMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable disk health metrics collection (SMART status, wear level).
    /// Default: true
    /// </summary>
    public bool EnableDiskHealthMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable disk speed metrics collection (throughput, IOPS, latency).
    /// Default: true
    /// </summary>
    public bool EnableDiskSpeedMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable GPU metrics collection (temperature, utilization, processes).
    /// Default: true
    /// </summary>
    public bool EnableGpuMetrics { get; set; } = true;

    /// <summary>
    /// Enable/disable battery metrics collection.
    /// Default: true
    /// </summary>
    public bool EnableBatteryMetrics { get; set; } = true;

    /// <summary>
    /// Default sampling window in milliseconds for CPU usage calculation.
    /// Default: 500ms
    /// </summary>
    public long DefaultSamplingWindowMs { get; set; } = 500;

    /// <summary>
    /// Whether to collect metrics for all processes or just current process.
    /// Default: false (current process only)
    /// </summary>
    public bool CollectAllProcesses { get; set; } = false;

    /// <summary>
    /// Maximum number of GPU processes to track per GPU.
    /// Default: 10
    /// </summary>
    public int MaxGpuProcesses { get; set; } = 10;

    /// <summary>
    /// Cache duration for hardware metrics that don't change frequently (in seconds).
    /// Default: 60 seconds
    /// </summary>
    public int HardwareMetricsCacheDurationSeconds { get; set; } = 60;
}

