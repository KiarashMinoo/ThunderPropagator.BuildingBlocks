using Microsoft.Extensions.Options;
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
public interface ISystemResourceMonitor
{
    /// <summary>
    /// Gets all configured system resource metrics.
    /// </summary>
    /// <param name="window">Sampling window in milliseconds for CPU usage calculation. If null, uses default from options.</param>
    /// <param name="all">Whether to collect metrics for all processes or just current process.</param>
    /// <returns>Comprehensive system resource metrics.</returns>
    SystemResourceMonitorMetrics GetMetrics(long? window = null, bool? all = null);
}

internal sealed class SystemResourceMonitorImpl : ISystemResourceMonitor
{
    private readonly CpuMetricsClient _cpuMetricsClient;
    private readonly CpuTemperatureMetricsClient _cpuTemperatureMetricsClient;
    private readonly MemoryMetricsClient _memoryMetricsClient;
    private readonly SystemDriveMetricsClient _systemDriveMetricsClient;
    private readonly DiskHealthMetricsClient _diskHealthMetricsClient;
    private readonly DiskSpeedMetricsClient _diskSpeedMetricsClient;
    private readonly GpuMetricsClient _gpuMetricsClient;
    private readonly BatteryMetricsClient _batteryMetricsClient;
    private readonly SystemResourceMonitorOptions _options;

    public SystemResourceMonitorImpl(
        CpuMetricsClient cpuMetricsClient,
        CpuTemperatureMetricsClient cpuTemperatureMetricsClient,
        MemoryMetricsClient memoryMetricsClient,
        SystemDriveMetricsClient systemDriveMetricsClient,
        DiskHealthMetricsClient diskHealthMetricsClient,
        DiskSpeedMetricsClient diskSpeedMetricsClient,
        GpuMetricsClient gpuMetricsClient,
        BatteryMetricsClient batteryMetricsClient,
        IOptions<SystemResourceMonitorOptions> options)
    {
        _cpuMetricsClient = cpuMetricsClient;
        _cpuTemperatureMetricsClient = cpuTemperatureMetricsClient;
        _memoryMetricsClient = memoryMetricsClient;
        _systemDriveMetricsClient = systemDriveMetricsClient;
        _diskHealthMetricsClient = diskHealthMetricsClient;
        _diskSpeedMetricsClient = diskSpeedMetricsClient;
        _gpuMetricsClient = gpuMetricsClient;
        _batteryMetricsClient = batteryMetricsClient;
        _options = options.Value;
    }

    public SystemResourceMonitorMetrics GetMetrics(long? window = null, bool? all = null)
    {
        var samplingWindow = window ?? _options.DefaultSamplingWindowMs;
        var collectAll = all ?? _options.CollectAllProcesses;

        // Collect CPU metrics
        var cpuMetrics = _options.EnableCpuMetrics 
            ? _cpuMetricsClient.GetMetrics(samplingWindow, collectAll)
            : new CpuMetrics(Environment.ProcessorCount, 0, 0, 0, 0);

        // Collect CPU temperature
        CpuTemperatureMetrics? cpuTemperature = null;
        if (_options.EnableCpuTemperature)
        {
            cpuTemperature = _cpuTemperatureMetricsClient.GetMetrics();
        }

        // Collect memory metrics
        var memoryMetrics = _options.EnableMemoryMetrics
            ? _memoryMetricsClient.GetMetrics()
            : new MemoryMetrics(0, 0);

        // Collect disk space metrics
        var systemDriveMetrics = _options.EnableDiskSpaceMetrics
            ? _systemDriveMetricsClient.GetMetrics()
            : Array.Empty<SystemDriveMetrics>();

        // Collect disk health metrics
        var diskHealthMetrics = _options.EnableDiskHealthMetrics
            ? _diskHealthMetricsClient.GetMetrics()
            : Array.Empty<DiskHealthMetrics>();

        // Collect disk speed metrics
        var diskSpeedMetrics = _options.EnableDiskSpeedMetrics
            ? _diskSpeedMetricsClient.GetMetrics()
            : Array.Empty<DiskSpeedMetrics>();

        // Collect GPU metrics
        var gpuMetrics = _options.EnableGpuMetrics
            ? _gpuMetricsClient.GetMetrics()
            : Array.Empty<GpuMetrics>();

        // Collect battery metrics
        BatteryMetrics? batteryMetrics = null;
        if (_options.EnableBatteryMetrics)
        {
            var battery = _batteryMetricsClient.GetMetrics();
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
}