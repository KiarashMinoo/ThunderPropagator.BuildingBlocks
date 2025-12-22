namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;

/// <summary>
/// Represents GPU temperature and utilization metrics.
/// </summary>
public record GpuMetrics : IMetrics
{
    /// <summary>
    /// GPU identifier/index.
    /// </summary>
    public int GpuIndex { get; init; }

    /// <summary>
    /// GPU name/model.
    /// </summary>
    public string? GpuName { get; init; }

    /// <summary>
    /// GPU temperature in Celsius. Null if not supported.
    /// </summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>
    /// GPU utilization percentage (0-100). Null if not supported.
    /// </summary>
    public double? UtilizationPercent { get; init; }

    /// <summary>
    /// GPU memory utilization percentage (0-100). Null if not supported.
    /// </summary>
    public double? MemoryUtilizationPercent { get; init; }

    /// <summary>
    /// Total GPU memory in MB. Null if not supported.
    /// </summary>
    public long? TotalMemoryMB { get; init; }

    /// <summary>
    /// Used GPU memory in MB. Null if not supported.
    /// </summary>
    public long? UsedMemoryMB { get; init; }

    /// <summary>
    /// GPU power usage in watts. Null if not supported.
    /// </summary>
    public double? PowerUsageWatts { get; init; }

    /// <summary>
    /// GPU fan speed percentage (0-100). Null if not supported.
    /// </summary>
    public double? FanSpeedPercent { get; init; }

    /// <summary>
    /// Active GPU processes.
    /// </summary>
    public List<GpuProcessInfo> ActiveProcesses { get; init; } = new();

    /// <summary>
    /// Whether GPU is available and metrics can be collected.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents a process using GPU resources.
/// </summary>
public record GpuProcessInfo
{
    /// <summary>
    /// Process ID.
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Process name.
    /// </summary>
    public string? ProcessName { get; init; }

    /// <summary>
    /// GPU memory used by this process in MB. Null if not supported.
    /// </summary>
    public long? UsedMemoryMB { get; init; }

    /// <summary>
    /// GPU utilization by this process (0-100). Null if not supported.
    /// </summary>
    public double? UtilizationPercent { get; init; }
}