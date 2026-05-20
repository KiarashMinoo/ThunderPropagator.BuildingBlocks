using MessagePack;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.MessagePack
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by MessagePack-CSharp.
    /// String representations are Base64-encoded MessagePack bytes.
    /// </summary>
    public sealed class MessagePackFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.MessagePack;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.MessagePack;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(MessagePackFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Convert.ToBase64String(SerializeToBytes<T>(instance));
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(MessagePackFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return MessagePackSerializer.Serialize(instance);
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(MessagePackFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return Deserialize<T>(Convert.FromBase64String(data));
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(MessagePackFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}
