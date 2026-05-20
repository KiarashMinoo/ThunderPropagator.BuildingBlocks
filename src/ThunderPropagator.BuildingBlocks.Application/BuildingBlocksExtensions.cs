using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;
using ThunderPropagator.BuildingBlocks.Application.Serializations.MessagePack;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Protobuf;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Xml;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// Extension methods for registering ThunderPropagator BuildingBlocks services.
    /// </summary>
    public static class BuildingBlocksExtensions
    {
        /// <summary>
        /// Registers the format serializer registry and all built-in format implementations
        /// (<see cref="SerializerType.Json"/>, <see cref="SerializerType.NJson"/>,
        /// <see cref="SerializerType.NetJson"/>, <see cref="SerializerType.Protobuf"/>,
        /// <see cref="SerializerType.MessagePack"/>, <see cref="SerializerType.Xml"/>,
        /// <see cref="SerializerType.Yaml"/>) with the DI container.
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
            services.TryAddSingleton<JsonFormatSerializer>();
            services.TryAddSingleton<NJsonFormatSerializer>();
            services.TryAddSingleton<NetJsonFormatSerializer>();
            services.TryAddSingleton<ProtobufFormatSerializer>();
            services.TryAddSingleton<MessagePackFormatSerializer>();
            services.TryAddSingleton<XmlFormatSerializer>();
            services.TryAddSingleton<YamlFormatSerializer>();

            // Register as IFormatSerializer (order determines media-type priority: Json wins for application/json)
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<JsonFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<NJsonFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<NetJsonFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<ProtobufFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<MessagePackFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<XmlFormatSerializer>());
            services.AddSingleton<IFormatSerializer>(sp => sp.GetRequiredService<YamlFormatSerializer>());

            // Register as IFormatDeserializer (same ordering)
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<JsonFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<NJsonFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<NetJsonFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<ProtobufFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<MessagePackFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<XmlFormatSerializer>());
            services.AddSingleton<IFormatDeserializer>(sp => sp.GetRequiredService<YamlFormatSerializer>());

            services.TryAddSingleton<IFormatSerializerRegistry, FormatSerializerRegistry>();

            return services;
        }
    }
}
