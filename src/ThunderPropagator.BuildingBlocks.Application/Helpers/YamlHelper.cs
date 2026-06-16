using ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml;
using System.Reflection;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class YamlHelper
    {
        public static YamlSerializerSettings DefaultSerializerSettings { get; set; }

        static YamlHelper()
        {
            DefaultSerializerSettings = new YamlSerializerSettings
            {
                NamingConvention = CamelCaseNamingConvention.Instance
            };
        }

        private static ISerializer YamlSerializer(Type type, YamlSerializerSettings? serializerSettings = null)
        {
            var serializerBuilder = new SerializerBuilder();

            if (serializerSettings?.JsonCompatible ?? DefaultSerializerSettings.JsonCompatible)
                serializerBuilder.JsonCompatible();

            if (serializerSettings?.IgnoreFields ?? DefaultSerializerSettings.IgnoreFields)
                serializerBuilder.IgnoreFields();

            if (serializerSettings?.IncludeNonPublicProperties ?? DefaultSerializerSettings.IncludeNonPublicProperties)
                serializerBuilder.IncludeNonPublicProperties();

            if (serializerSettings?.EnablePrivateConstructors ?? DefaultSerializerSettings.EnablePrivateConstructors)
                serializerBuilder.EnablePrivateConstructors();

            var namingConvention = serializerSettings?.NamingConvention ?? DefaultSerializerSettings.NamingConvention;
            if (namingConvention is not null)
                serializerBuilder.WithNamingConvention(namingConvention);

            var enumNamingConvention = serializerSettings?.EnumNamingConvention ?? DefaultSerializerSettings.EnumNamingConvention;
            if (enumNamingConvention is not null)
                serializerBuilder.WithEnumNamingConvention(enumNamingConvention);

            var typeResolver = serializerSettings?.TypeResolver ?? DefaultSerializerSettings.TypeResolver;
            if (typeResolver is not null)
                serializerBuilder.WithTypeResolver(typeResolver);

            List<IYamlTypeConverter> typeConverters = [];
            var typeConverter = type.GetCustomAttribute<YamlTypeConverterAttribute>()?.ConverterType;
            if (typeConverter is not null)
                typeConverters.Add((IYamlTypeConverter)Activator.CreateInstance(typeConverter)!);

            if (serializerSettings?.TypeConverters is not null)
                typeConverters.AddRange(serializerSettings.TypeConverters);

            if (DefaultSerializerSettings.TypeConverters is not null)
                typeConverters.AddRange(DefaultSerializerSettings.TypeConverters);

            if (typeConverters.Count > 0)
            {
                typeConverters.ForEach(x => serializerBuilder.WithTypeConverter(x));
            }

            var style = serializerSettings?.Style ?? DefaultSerializerSettings.Style;
            if (style is not null)
                serializerBuilder.WithDefaultScalarStyle(style.Value);

            return serializerBuilder.Build();
        }

        public static string ToYaml<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(ToYaml)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var serializer = YamlSerializer(typeof(T), serializerSettings);
            var originals = SensitiveDataEncryption.EncryptInPlace(instance);
            try
            {
                return serializer.Serialize(instance);
            }
            finally
            {
                if (originals is not null)
                    SensitiveDataEncryption.RevertEncryption(instance, originals);
            }
        }

        private static IDeserializer YamlDeserializer(Type type, YamlSerializerSettings? serializerSettings = null)
        {
            var deserializerBuilder = new DeserializerBuilder();

            if (serializerSettings?.IgnoreFields ?? DefaultSerializerSettings.IgnoreFields)
                deserializerBuilder.IgnoreFields();

            if (serializerSettings?.IncludeNonPublicProperties ?? DefaultSerializerSettings.IncludeNonPublicProperties)
                deserializerBuilder.IncludeNonPublicProperties();

            if (serializerSettings?.EnablePrivateConstructors ?? DefaultSerializerSettings.EnablePrivateConstructors)
                deserializerBuilder.EnablePrivateConstructors();

            var namingConvention = serializerSettings?.NamingConvention ?? DefaultSerializerSettings.NamingConvention;
            if (namingConvention is not null)
                deserializerBuilder.WithNamingConvention(namingConvention);

            var enumNamingConvention = serializerSettings?.EnumNamingConvention ?? DefaultSerializerSettings.EnumNamingConvention;
            if (enumNamingConvention is not null)
                deserializerBuilder.WithEnumNamingConvention(enumNamingConvention);

            var typeResolver = serializerSettings?.TypeResolver ?? DefaultSerializerSettings.TypeResolver;
            if (typeResolver is not null)
                deserializerBuilder.WithTypeResolver(typeResolver);

            List<IYamlTypeConverter> typeConverters = [];
            var typeConverter = type.GetCustomAttribute<YamlTypeConverterAttribute>()?.ConverterType;
            if (typeConverter is not null)
                typeConverters.Add((IYamlTypeConverter)Activator.CreateInstance(typeConverter)!);

            if (serializerSettings?.TypeConverters is not null)
                typeConverters.AddRange(serializerSettings.TypeConverters);

            if (DefaultSerializerSettings.TypeConverters is not null)
                typeConverters.AddRange(DefaultSerializerSettings.TypeConverters);

            if (typeConverters.Count > 0)
            {
                typeConverters.ForEach(x => deserializerBuilder.WithTypeConverter(x));
            }

            List<INodeDeserializer> nodeDeserializers = [];
            var nodeDeserializer = type.GetCustomAttribute<YamlNodeDeserializerAttribute>()?.NodeDeserializer;
            if (nodeDeserializer is not null)
                nodeDeserializers.Add((INodeDeserializer)Activator.CreateInstance(nodeDeserializer)!);

            if (serializerSettings?.NodeDeserializers is not null)
                nodeDeserializers.AddRange(serializerSettings.NodeDeserializers);

            if (DefaultSerializerSettings.NodeDeserializers is not null)
                nodeDeserializers.AddRange(DefaultSerializerSettings.NodeDeserializers);

            if (nodeDeserializers.Count > 0)
            {
                nodeDeserializers.ForEach(x => deserializerBuilder.WithNodeDeserializer(x));
            }

            return deserializerBuilder.Build();
        }

        public static T FromYaml<T>(this string yaml, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYaml)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var deserializer = YamlDeserializer(typeof(T), serializerSettings);
            var result = deserializer.Deserialize<T>(yaml);
            SensitiveDataEncryption.DecryptInPlace(result);
            return result;
        }

        public static object? FromYaml(this string yaml, Type type, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYaml)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var deserializer = YamlDeserializer(type, serializerSettings);
            var result = deserializer.Deserialize(yaml);
            SensitiveDataEncryption.DecryptInPlace(result);
            return result;
        }

        public static byte[] ToYamlBytes<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(ToYamlBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var serializer = YamlSerializer(typeof(T), serializerSettings);
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            var originals = SensitiveDataEncryption.EncryptInPlace(instance);
            try
            {
                serializer.Serialize(streamWriter, instance);
                streamWriter.Flush();
            }
            finally
            {
                if (originals is not null)
                    SensitiveDataEncryption.RevertEncryption(instance, originals);
            }
            return memoryStream.ToArray();
        }

        public static T? FromYamlBytes<T>(this byte[] bytes, YamlSerializerSettings? serializerSettings = null)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYamlBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var deserializer = YamlDeserializer(typeof(T), serializerSettings);
            using var memoryStream = new MemoryStream(bytes);
            using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
            var result = deserializer.Deserialize<T>(streamReader);
            SensitiveDataEncryption.DecryptInPlace(result);
            return result;
        }

        public static string ToYamlBase64<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(ToYamlBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = instance.ToYamlBytes(serializerSettings);
            return Convert.ToBase64String(bytes);
        }

        public static T? FromYamlBase64<T>(this string str, YamlSerializerSettings? serializerSettings = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYamlBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = Convert.FromBase64String(str);
            return bytes.FromYamlBytes<T>(serializerSettings);
        }
    }
}
