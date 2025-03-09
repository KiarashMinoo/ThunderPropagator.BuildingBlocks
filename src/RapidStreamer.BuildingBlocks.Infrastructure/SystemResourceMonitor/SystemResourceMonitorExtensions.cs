using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

public static class SystemResourceMonitorExtensions
{
    public static IServiceCollection AddSystemResourceMonitor(this IServiceCollection services)
    {
        services.TryAddSingleton<CpuMetricsClient>();
        services.TryAddSingleton<MemoryMetricsClient>();
        services.TryAddSingleton<SystemDriveMetricsClient>();
        services.TryAddSingleton<ISystemResourceMonitor, SystemResourceMonitorImpl>();

        return services;
    }
}