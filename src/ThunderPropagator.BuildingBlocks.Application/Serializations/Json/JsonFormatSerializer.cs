using System.Text.Json;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Json
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by <c>System.Text.Json</c>.
    /// </summary>
    public sealed class JsonFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        /// <summary>
        /// System.Text.Json — application/json
        /// </summary>
        public static readonly SerializerType Json = 1;

        /// <summary>application/json</summary>
        public const string JsonMediaType = "application/json";

        public SerializerType SerializerType => Json;
        public string MediaType => JsonMediaType;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(JsonFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return instance.ToJson();
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(JsonFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return instance.ToJsonBytes();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(JsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return data.FromJson<T>();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(JsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            return bytes.FromJsonBytes<T>();
        }
    }
}
