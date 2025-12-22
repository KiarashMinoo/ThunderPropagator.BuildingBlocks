using Microsoft.Extensions.Options;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

/// <summary>
/// Interface for system resource monitoring with comprehensive hardware health and performance metrics.
/// </summary>
public interface ISystemResourceMonitor : IMetricsClient<SystemResourceMonitorMetrics>
{
    /// <summary>
    /// Gets all configured system resource metrics asynchronously.
    /// </summary>
    /// <param name="window">Sampling window in milliseconds for CPU usage calculation. If null, uses default from options.</param>
    /// <param name="all">Whether to collect metrics for all processes or just current process.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Comprehensive system resource metrics.</returns>
    Task<SystemResourceMonitorMetrics> GetMetricsAsync(long? window = null, bool? all = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configured system resource metrics synchronously (back-compat wrapper).
    /// </summary>
    /// <param name="window">Sampling window in milliseconds for CPU usage calculation. If null, uses default from options.</param>
    /// <param name="all">Whether to collect metrics for all processes or just current process.</param>
    /// <returns>Comprehensive system resource metrics.</returns>
    SystemResourceMonitorMetrics GetMetrics(long? window = null, bool? all = null);
}

internal sealed class SystemResourceMonitorImpl(
    ICpuMetricsClient cpuMetricsClient,
    ICpuTemperatureMetricsClient cpuTemperatureMetricsClient,
    IMemoryMetricsClient memoryMetricsClient,
    ISystemDriveMetricsClient systemDriveMetricsClient,
    IDiskHealthMetricsClient diskHealthMetricsClient,
    IDiskSpeedMetricsClient diskSpeedMetricsClient,
    IGpuMetricsClient gpuMetricsClient,
    IBatteryMetricsClient batteryMetricsClient,
    IOptions<SystemResourceMonitorOptions> options
) : ISystemResourceMonitor
{
    private readonly SystemResourceMonitorOptions _options = options.Value;

    public Task<SystemResourceMonitorMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return GetMetricsAsync(_options.DefaultSamplingWindowMs, _options.CollectAllProcesses, cancellationToken);
    }

    public async Task<SystemResourceMonitorMetrics> GetMetricsAsync(long? window = null, bool? all = null, CancellationToken cancellationToken = default)
    {
        var samplingWindow = window ?? _options.DefaultSamplingWindowMs;
        var collectAll = all ?? _options.CollectAllProcesses;

        // Collect CPU metrics
        var cpuMetrics = _options.EnableCpuMetrics
            ? await cpuMetricsClient.GetMetricsAsync(samplingWindow, collectAll, cancellationToken).ConfigureAwait(false)
            : new CpuMetrics(Environment.ProcessorCount, 0, 0, 0, 0);

        // Collect CPU temperature
        CpuTemperatureMetrics? cpuTemperature = null;
        if (_options.EnableCpuTemperature)
        {
            cpuTemperature = await cpuTemperatureMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false);
        }

        // Collect memory metrics
        var memoryMetrics = _options.EnableMemoryMetrics
            ? await memoryMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false)
            : new MemoryMetrics(0, 0);

        // Collect disk space metrics
        var systemDriveMetrics = _options.EnableDiskSpaceMetrics
            ? await systemDriveMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false)
            : [];

        // Collect disk health metrics
        var diskHealthMetrics = _options.EnableDiskHealthMetrics
            ? await diskHealthMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false)
            : [];

        // Collect disk speed metrics
        var diskSpeedMetrics = _options.EnableDiskSpeedMetrics
            ? await diskSpeedMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false)
            : [];

        // Collect GPU metrics
        var gpuMetrics = _options.EnableGpuMetrics
            ? await gpuMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false)
            : [];

        // Collect battery metrics
        BatteryMetrics? batteryMetrics = null;
        if (_options.EnableBatteryMetrics)
        {
            var battery = await batteryMetricsClient.GetMetricsAsync(cancellationToken).ConfigureAwait(false);
            // Only include battery metrics if a battery is actually present
            if (battery.BatteryPresent)
            {
                batteryMetrics = battery;
            }
        }

        return new SystemResourceMonitorMetrics
        {
            Cpu = cpuMetrics,
            CpuTemperature = cpuTemperature,
            Memory = memoryMetrics,
            Drives = systemDriveMetrics,
            DiskHealth = diskHealthMetrics,
            DiskSpeed = diskSpeedMetrics,
            Gpus = gpuMetrics,
            Battery = batteryMetrics
        };
    }

    public SystemResourceMonitorMetrics GetMetrics(long? window = null, bool? all = null)
        => GetMetricsAsync(window, all).GetAwaiter().GetResult();
}