using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        services.TryAddSingleton<CpuMetricsClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SystemResourceMonitorOptions>>().Value;
            return new CpuMetricsClient(options.DefaultSamplingWindowMs, options.CollectAllProcesses);
        });
        services.TryAddSingleton<ICpuMetricsClient>(sp => sp.GetRequiredService<CpuMetricsClient>());

        services.TryAddSingleton<IMemoryMetricsClient, MemoryMetricsClient>();
        services.TryAddSingleton<ISystemDriveMetricsClient, SystemDriveMetricsClient>();

        // Register new metric clients
        services.TryAddSingleton<ICpuTemperatureMetricsClient, CpuTemperatureMetricsClient>();
        services.TryAddSingleton<IDiskHealthMetricsClient, DiskHealthMetricsClient>();
        services.TryAddSingleton<IDiskSpeedMetricsClient, DiskSpeedMetricsClient>();

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SystemResourceMonitorOptions>>();
            return new GpuMetricsClient(options.Value.MaxGpuProcesses);
        });
        services.TryAddSingleton<IGpuMetricsClient>(sp => sp.GetRequiredService<GpuMetricsClient>());

        services.TryAddSingleton<IBatteryMetricsClient, BatteryMetricsClient>();

        // Register main monitor
        services.TryAddSingleton<ISystemResourceMonitor, SystemResourceMonitorImpl>();

        return services;
    }
}