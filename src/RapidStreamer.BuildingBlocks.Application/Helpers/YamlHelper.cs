using RapidStreamer.BuildingBlocks.Application.Serializations.Yaml;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
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
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            var serializer = YamlSerializer(typeof(T), serializerSettings);
            return serializer.Serialize(instance);
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

            var converterType = type.GetCustomAttribute<YamlTypeConverterAttribute>()?.ConverterType;
            if (converterType is not null)
                deserializerBuilder.WithTypeConverter((IYamlTypeConverter)Activator.CreateInstance(converterType)!);

            return deserializerBuilder.Build();
        }

        public static T FromYaml<T>(this string yaml, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYaml)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            var deserializer = YamlDeserializer(typeof(T), serializerSettings);
            return deserializer.Deserialize<T>(yaml);
        }

        public static object? FromYaml(this string yaml, Type type, YamlSerializerSettings? serializerSettings = null)
        {
            const string activityName = $"{nameof(YamlHelper)}_{nameof(FromYaml)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            var deserializer = YamlDeserializer(type, serializerSettings);
            return deserializer.Deserialize(yaml);
        }

        public static byte[] ToYamlBytes<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
            where T : notnull
        {
            var jsonStr = instance.ToYaml(serializerSettings);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            return bytes;
        }

        public static T? FromYamlBytes<T>(this byte[] bytes, YamlSerializerSettings? serializerSettings = null)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            var jsonStr = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return default;
            }

            return jsonStr.FromYaml<T>(serializerSettings);
        }

        public static string ToYamlBase64<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
            where T : notnull
        {
            var jsonStr = instance.ToYaml(serializerSettings);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            return Convert.ToBase64String(bytes)[..^2];
        }

        public static T? FromYamlBase64<T>(this string str, YamlSerializerSettings? serializerSettings = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            var bytes = Convert.FromBase64String(str);
            var jsonStr = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return default;
            }

            return jsonStr.FromYaml<T>(serializerSettings);
        }
    }
}