using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

/// <summary>
/// Extension methods for registering system resource monitoring services.
/// </summary>
public static class SystemResourceMonitorExtensions
{
    /// <summary>
    /// Adds comprehensive system resource monitoring services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration action for monitoring options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSystemResourceMonitor(
        this IServiceCollection services,
        Action<SystemResourceMonitorOptions>? configureOptions = null)
    {
        // Register options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<SystemResourceMonitorOptions>(_ => { });
        }

        // Register existing metric clients
        services.TryAddSingleton<CpuMetricsClient>();
        services.TryAddSingleton<MemoryMetricsClient>();
        services.TryAddSingleton<SystemDriveMetricsClient>();

        // Register new metric clients
        services.TryAddSingleton<CpuTemperatureMetricsClient>();
        services.TryAddSingleton<DiskHealthMetricsClient>();
        services.TryAddSingleton<DiskSpeedMetricsClient>();
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SystemResourceMonitorOptions>>();
            return new GpuMetricsClient(options.Value.MaxGpuProcesses);
        });
        services.TryAddSingleton<BatteryMetricsClient>();

        // Register main monitor
        services.TryAddSingleton<ISystemResourceMonitor, SystemResourceMonitorImpl>();

        return services;
    }
}