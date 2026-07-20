using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// Extension methods for registering ThunderPropagator BuildingBlocks services.
    /// </summary>
    public static class BuildingBlocksExtensions
    {
        public static IServiceCollection AddFormatSerializer<TFormatSerializer>(this IServiceCollection services) where TFormatSerializer : class, IFormatSerializer
        {
            Guard.Against.Null(services);

            // Register each concrete implementation as a singleton so the same instance
            // is reused across both IFormatSerializer lookups.
            services.TryAddSingleton<TFormatSerializer>();

            // Register as IFormatSerializer (order determines media-type priority: Json wins for application/json)
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<TFormatSerializer>());

            return services;
        }

        public static IServiceCollection AddFormatDeserializer<TFormatDeserializer>(this IServiceCollection services) where TFormatDeserializer : class, IFormatDeserializer
        {
            Guard.Against.Null(services);

            // Register each concrete implementation as a singleton so the same instance
            // is reused across both IFormatDeserializer lookups.
            services.TryAddSingleton<TFormatDeserializer>();

            // Register as IFormatDeserializer (order determines media-type priority: Json wins for application/json)
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<TFormatDeserializer>());

            return services;
        }

        /// <summary>
        /// Registers the format serializer registry and all built-in format implementations
        /// (<see cref="JsonFormatSerializer.Json"/>, <see cref="NJsonFormatSerializer.NJson"/>) with the DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddBuildingBlocks(this IServiceCollection services)
        {
            Guard.Against.Null(services);

            if (services.Any(sd => sd.ServiceType == typeof(IFormatSerializerRegistry)))
            {
                return services;
            }

            // Register each concrete implementation as a singleton so the same instance
            // is reused across both IFormatSerializer and IFormatDeserializer lookups.
            services.AddFormatSerializer<JsonFormatSerializer>().AddFormatDeserializer<JsonFormatSerializer>();
            services.AddFormatSerializer<NJsonFormatSerializer>().AddFormatDeserializer<NJsonFormatSerializer>();

            services.TryAddSingleton<IFormatSerializerRegistry, FormatSerializerRegistry>();

            return services;
        }
    }
}
