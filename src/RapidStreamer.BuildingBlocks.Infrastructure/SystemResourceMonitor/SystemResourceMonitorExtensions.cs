using Microsoft.Extensions.DependencyInjection;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

public static class SystemResourceMonitorExtensions
{
    public static IServiceCollection AddSystemResourceMonitor(this IServiceCollection services)
    {
        services.AddSingleton<CpuMetricsClient>();
        services.AddSingleton<MemoryMetricsClient>();
        services.AddSingleton<SystemDriveMetricsClient>();
        services.AddSingleton<ISystemResourceMonitor, SystemResourceMonitorImpl>();

        return services;
    }
}