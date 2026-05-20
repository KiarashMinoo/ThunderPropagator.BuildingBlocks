using ProtoBuf;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Protobuf
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by protobuf-net.
    /// String representations are Base64-encoded protobuf bytes.
    /// </summary>
    public sealed class ProtobufFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.Protobuf;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.Protobuf;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(ProtobufFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Convert.ToBase64String(SerializeToBytes<T>(instance));
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(ProtobufFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, instance);
            return memoryStream.ToArray();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(ProtobufFormatSerializer)}_{nameof(Deserialize)}";
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
            const string activityName = $"{nameof(ProtobufFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            using var memoryStream = new MemoryStream(bytes);
            return Serializer.Deserialize<T>(memoryStream);
        }
    }
}
