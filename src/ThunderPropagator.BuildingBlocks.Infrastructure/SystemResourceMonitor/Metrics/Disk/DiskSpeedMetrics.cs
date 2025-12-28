namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

/// <summary>
/// Represents disk performance metrics (throughput, IOPS, latency).
/// </summary>
public record DiskSpeedMetrics : IMetrics
{
    /// <summary>
    /// Drive identifier (e.g., "C:", "/dev/sda")
    /// </summary>
    public required string DriveId { get; init; }

    /// <summary>
    /// Read throughput in MB/s. Null if not supported.
    /// </summary>
    public double? ReadThroughputMBps { get; init; }

    /// <summary>
    /// Write throughput in MB/s. Null if not supported.
    /// </summary>
    public double? WriteThroughputMBps { get; init; }

    /// <summary>
    /// Read IOPS (Input/Output Operations Per Second). Null if not supported.
    /// </summary>
    public double? ReadIOPS { get; init; }

    /// <summary>
    /// Write IOPS (Input/Output Operations Per Second). Null if not supported.
    /// </summary>
    public double? WriteIOPS { get; init; }

    /// <summary>
    /// Average read latency in milliseconds. Null if not supported.
    /// </summary>
    public double? AverageReadLatencyMs { get; init; }

    /// <summary>
    /// Average write latency in milliseconds. Null if not supported.
    /// </summary>
    public double? AverageWriteLatencyMs { get; init; }

    /// <summary>
    /// Queue depth (number of pending I/O operations). Null if not supported.
    /// </summary>
    public long? QueueDepth { get; init; }

    /// <summary>
    /// Disk active time percentage (0-100). Null if not supported.
    /// </summary>
    public double? ActiveTimePercent { get; init; }

    /// <summary>
    /// Whether performance counters are available.
    /// </summary>
    public bool PerformanceCountersAvailable { get; init; }

    /// <summary>
    /// Error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}