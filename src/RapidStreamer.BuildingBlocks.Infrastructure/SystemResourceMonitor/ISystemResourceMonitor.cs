using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

public interface ISystemResourceMonitor
{
    SystemResourceMonitorMetrics GetMetrics(long window, bool all = false);
}

internal sealed class SystemResourceMonitorImpl(
    CpuMetricsClient cpuMetricsClient,
    MemoryMetricsClient memoryMetricsClient,
    SystemDriveMetricsClient systemDriveMetricsClient)
    : ISystemResourceMonitor
{
    public SystemResourceMonitorMetrics GetMetrics(long window, bool all = false)
    {
        var cpuMetrics = cpuMetricsClient.GetMetrics(window, all);
        var memoryMetrics = memoryMetricsClient.GetMetrics();
        var systemDriveMetrics = systemDriveMetricsClient.GetMetrics();

        return new SystemResourceMonitorMetrics(cpuMetrics, memoryMetrics, systemDriveMetrics);
    }
}