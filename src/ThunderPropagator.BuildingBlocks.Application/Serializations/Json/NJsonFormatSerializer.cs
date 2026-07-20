using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Json
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by Newtonsoft.Json.
    /// </summary>
    public sealed class NJsonFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.NJson;

        /// <summary>application/json</summary>
        public string MediaType => "application/json";

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return instance.ToNJson();
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return instance.ToNJsonBytes();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return data.FromNJson<T>();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            return bytes.FromNJsonBytes<T>();
        }
    }
}

