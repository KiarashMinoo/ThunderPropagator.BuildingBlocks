using System.Text;
using NetJSON;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Json
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by NetJSON.
    /// </summary>
    public sealed class NetJsonFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        private static readonly NetJSONSettings _defaultSettings = new() { CamelCase = true };

        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.NetJson;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.Json;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(NetJsonFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var settings = NetJsonSettings<T>();

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return NetJSON.NetJSON.Serialize(exceptionInfo, settings);
            }

            return NetJSON.NetJSON.Serialize(instance, settings);
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(NetJsonFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Encoding.UTF8.GetBytes(Serialize<T>(instance));
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(NetJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return NetJSON.NetJSON.Deserialize<T>(data, NetJsonSettings<T>());
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(NetJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return NetJSON.NetJSON.Deserialize<T>(json, NetJsonSettings<T>());
        }

        private static NetJSONSettings NetJsonSettings<T>()
        {
            var attribute = JsonSerializationAttributeCache.Get(typeof(T));
            if (attribute?.CamelCase == false)
            {
                return new NetJSONSettings { CamelCase = false };
            }

            return _defaultSettings;
        }
    }
}
