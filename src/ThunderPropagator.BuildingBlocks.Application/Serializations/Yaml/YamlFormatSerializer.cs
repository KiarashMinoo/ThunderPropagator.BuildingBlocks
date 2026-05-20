using System.Text;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by YamlDotNet.
    /// </summary>
    public sealed class YamlFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.Yaml;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.Yaml;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(YamlFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return instance.ToYaml();
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(YamlFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Encoding.UTF8.GetBytes(instance.ToYaml());
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(YamlFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return data.FromYaml<T>();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(YamlFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            var yaml = Encoding.UTF8.GetString(bytes);
            return yaml.FromYaml<T>();
        }
    }
}
