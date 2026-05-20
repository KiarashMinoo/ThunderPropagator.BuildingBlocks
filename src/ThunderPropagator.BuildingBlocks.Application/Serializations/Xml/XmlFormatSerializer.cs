using System.Collections.Concurrent;
using System.Text;
using System.Xml.Serialization;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Xml
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by <c>System.Xml.Serialization</c>.
    /// </summary>
    public sealed class XmlFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();

        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.Xml;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.Xml;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(XmlFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var writer = new StringWriter();
            GetSerializer(typeof(T)).Serialize(writer, instance);
            return writer.ToString();
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(XmlFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            GetSerializer(typeof(T)).Serialize(streamWriter, instance);
            streamWriter.Flush();
            return memoryStream.ToArray();
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(XmlFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            using var reader = new StringReader(data);
            return (T?)GetSerializer(typeof(T)).Deserialize(reader);
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(XmlFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            using var memoryStream = new MemoryStream(bytes);
            return (T?)GetSerializer(typeof(T)).Deserialize(memoryStream);
        }

        private static XmlSerializer GetSerializer(Type type)
        {
            return _serializerCache.GetOrAdd(type, static t => new XmlSerializer(t));
        }
    }
}
