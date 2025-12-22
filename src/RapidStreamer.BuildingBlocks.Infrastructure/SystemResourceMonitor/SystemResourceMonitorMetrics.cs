using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

/// <summary>
/// Comprehensive system resource monitoring metrics including hardware health and performance data.
/// </summary>
public record SystemResourceMonitorMetrics : IMetrics
{
    /// <summary>
    /// CPU usage and process metrics.
    /// </summary>
    public CpuMetrics Cpu { get; init; } = null!;

    /// <summary>
    /// CPU temperature metrics (may be null if not supported or disabled).
    /// </summary>
    public CpuTemperatureMetrics? CpuTemperature { get; init; }

    /// <summary>
    /// Memory usage metrics.
    /// </summary>
    public MemoryMetrics Memory { get; init; } = null!;

    /// <summary>
    /// System drive space metrics.
    /// </summary>
    public SystemDriveMetrics[] Drives { get; init; } = Array.Empty<SystemDriveMetrics>();

    /// <summary>
    /// Disk health metrics (SMART status, wear level, etc.).
    /// </summary>
    public DiskHealthMetrics[] DiskHealth { get; init; } = Array.Empty<DiskHealthMetrics>();

    /// <summary>
    /// Disk speed/performance metrics (throughput, IOPS, latency).
    /// </summary>
    public DiskSpeedMetrics[] DiskSpeed { get; init; } = Array.Empty<DiskSpeedMetrics>();

    /// <summary>
    /// GPU metrics (temperature, utilization, processes).
    /// </summary>
    public GpuMetrics[] Gpus { get; init; } = Array.Empty<GpuMetrics>();

    /// <summary>
    /// Battery metrics (only populated if battery is present).
    /// </summary>
    public BatteryMetrics? Battery { get; init; }
}