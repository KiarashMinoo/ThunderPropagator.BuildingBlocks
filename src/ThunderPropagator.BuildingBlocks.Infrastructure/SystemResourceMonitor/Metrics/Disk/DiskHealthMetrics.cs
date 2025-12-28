namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

/// <summary>
/// Represents disk health and SMART status metrics.
/// </summary>
public record DiskHealthMetrics : IMetrics
{
    /// <summary>
    /// Drive identifier (e.g., "C:", "/dev/sda")
    /// </summary>
    public required string DriveId { get; init; }

    /// <summary>
    /// Overall health status (Healthy, Warning, Critical, Unknown)
    /// </summary>
    public DiskHealthStatus Status { get; init; }

    /// <summary>
    /// Wear level percentage (0-100). Null if not supported.
    /// </summary>
    public double? WearLevelPercent { get; init; }

    /// <summary>
    /// Temperature in Celsius. Null if not supported.
    /// </summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>
    /// Number of reallocated sectors. Null if not supported.
    /// </summary>
    public long? ReallocatedSectorsCount { get; init; }

    /// <summary>
    /// Power on hours. Null if not supported.
    /// </summary>
    public long? PowerOnHours { get; init; }

    /// <summary>
    /// Whether SMART data is available for this drive.
    /// </summary>
    public bool SmartAvailable { get; init; }

    /// <summary>
    /// Error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

public enum DiskHealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Warning = 2,
    Critical = 3,
    NotSupported = 4
}