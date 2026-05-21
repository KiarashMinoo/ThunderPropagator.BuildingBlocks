using System.Collections.Concurrent;
using System.Text;
using System.Xml.Serialization;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class XmlHelper
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();

        private static XmlSerializer GetSerializer(Type type)
        {
            return _serializerCache.GetOrAdd(type, static t => new XmlSerializer(t));
        }

        public static string ToXml<T>(this T instance)
        {
            const string activityName = $"{nameof(XmlHelper)}_{nameof(ToXml)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var writer = new StringWriter();
            GetSerializer(typeof(T)).Serialize(writer, instance);
            return writer.ToString();
        }

        public static byte[] ToXmlBytes<T>(this T instance)
        {
            const string activityName = $"{nameof(XmlHelper)}_{nameof(ToXmlBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            GetSerializer(typeof(T)).Serialize(streamWriter, instance);
            streamWriter.Flush();
            return memoryStream.ToArray();
        }

        public static string ToXmlBase64<T>(this T instance)
        {
            const string activityName = $"{nameof(XmlHelper)}_{nameof(ToXmlBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = instance.ToXmlBytes();
            return Convert.ToBase64String(bytes);
        }

        public static T? FromXml<T>(this string xml)
        {
            const string activityName = $"{nameof(XmlHelper)}_{nameof(FromXml)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(xml))
            {
                return default;
            }

            using var reader = new StringReader(xml);
            return (T?)GetSerializer(typeof(T)).Deserialize(reader);
        }

        public static T? FromXmlBytes<T>(this byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            const string activityName = $"{nameof(XmlHelper)}_{nameof(FromXmlBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream(bytes);
            return (T?)GetSerializer(typeof(T)).Deserialize(memoryStream);
        }

        public static T? FromXmlBase64<T>(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            const string activityName = $"{nameof(XmlHelper)}_{nameof(FromXmlBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = Convert.FromBase64String(str);
            return bytes.FromXmlBytes<T>();
        }
    }
}

