using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RapidStreamer.BuildingBlocks.Infrastructure.HealthChecks
{
    public static class ActiveMQHealthCheckExtensions
    {
        public static IHealthChecksBuilder AddActiveMQHealthCheck(this IHealthChecksBuilder builder,
            ActiveMQHealthCheckOptions options,
            string? name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
        {
            builder.Services.TryAddSingleton(options);
            builder.AddCheck<ActiveMQHealthCheck>(name ?? nameof(ActiveMQHealthCheck), failureStatus, tags, timeout);
            return builder;
        }
    }
}