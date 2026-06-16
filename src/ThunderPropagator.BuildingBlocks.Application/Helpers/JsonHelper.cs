using ThunderPropagator.BuildingBlocks.Application.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class JsonHelper
    {
        // Shared instance constructed once and reused on every default-path call.
        // Never passed to code that mutates it — clones are created for callers
        // that need customisation (options callback or CamelCase-false types).
        private static readonly JsonSerializerOptions _defaultOptions = BuildAndFreezeDefaultOptions();

        private static JsonSerializerOptions BuildAndFreezeDefaultOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(ApplySensitiveDataEncryption),
            };
        }

        private static void ApplySensitiveDataEncryption(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            foreach (var prop in typeInfo.Properties)
            {
                if (prop.PropertyType == typeof(string)
                    && prop.AttributeProvider?.GetCustomAttributes(typeof(SensitiveDataAttribute), true) is { Length: > 0 })
                {
                    prop.CustomConverter = SensitiveDataStringJsonConverter.Instance;
                }
            }
        }

        private sealed class SensitiveDataStringJsonConverter : JsonConverter<string>
        {
            internal static readonly SensitiveDataStringJsonConverter Instance = new();

            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var value = reader.GetString();
                if (value is null || !SensitiveDataEncryption.IsConfigured)
                    return value;
                return SensitiveDataEncryption.Decrypt(value);
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                if (!SensitiveDataEncryption.IsConfigured)
                {
                    writer.WriteStringValue(value);
                    return;
                }
                writer.WriteStringValue(SensitiveDataEncryption.Encrypt(value));
            }
        }

        // Returns a mutable clone of the default options for callers that need to customise
        // the settings (e.g. ToonHelper) without touching the shared frozen instance.
        internal static JsonSerializerOptions BuildDefaultSerializerOptions()
        {
            return new JsonSerializerOptions(_defaultOptions);
        }

        internal static JsonSerializerOptions JsonSerializerOptions<T>(JsonSerializerOptions? serializerOptions = null)
        {
            return JsonSerializerOptions(typeof(T), serializerOptions);
        }

        internal static JsonSerializerOptions JsonSerializerOptions(Type type, JsonSerializerOptions? serializerOptions = null)
        {
            var jsonSerializationAttribute = JsonSerializationAttributeCache.Get(type);
            var disableCamelCase = jsonSerializationAttribute?.CamelCase == false;

            if (serializerOptions == null)
            {
                // Default path: return the shared frozen instance when no override is needed.
                // If the type opts out of camelCase, return a fresh copy with the policy cleared.
                if (!disableCamelCase)
                    return _defaultOptions;

                return new JsonSerializerOptions(_defaultOptions) { PropertyNamingPolicy = null };
            }

            if (disableCamelCase && serializerOptions is { IsReadOnly: false, PropertyNamingPolicy: not null })
                serializerOptions.PropertyNamingPolicy = null;

            return serializerOptions;
        }

        public static string ToJson<T>(this T instance, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(ToJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = new JsonSerializerOptions(_defaultOptions);
                options(serializerOptions);
            }

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return JsonSerializer.Serialize(exceptionInfo, JsonSerializerOptions<T>(serializerOptions));
            }

            return JsonSerializer.Serialize(instance, JsonSerializerOptions<T>(serializerOptions));
        }

        public static byte[] ToJsonBytes<T>(this T instance, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(ToJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = new JsonSerializerOptions(_defaultOptions);
                options(serializerOptions);
            }

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return JsonSerializer.SerializeToUtf8Bytes(exceptionInfo, JsonSerializerOptions<T>(serializerOptions));
            }

            return JsonSerializer.SerializeToUtf8Bytes(instance, JsonSerializerOptions<T>(serializerOptions));
        }

        public static string ToJsonBase64<T>(this T instance, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            var bytes = instance.ToJsonBytes(options);
            return Convert.ToBase64String(bytes);
        }

        public static T? FromJson<T>(this string json, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(FromJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = new JsonSerializerOptions(_defaultOptions);
                options(serializerOptions);
            }

            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions<T>(serializerOptions));
        }

        public static object? FromJson(this string json, Type type, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(FromJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = new JsonSerializerOptions(_defaultOptions);
                options(serializerOptions);
            }

            return JsonSerializer.Deserialize(json, type, JsonSerializerOptions(type, serializerOptions));
        }

        public static T? FromJsonBytes<T>(this byte[] bytes, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(FromJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = new JsonSerializerOptions(_defaultOptions);
                options(serializerOptions);
            }

            return JsonSerializer.Deserialize<T>(bytes, JsonSerializerOptions<T>(serializerOptions));
        }

        public static T? FromJsonBase64<T>(this string str, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            var bytes = Convert.FromBase64String(str);

            return bytes.FromJsonBytes<T>(options);
        }
    }
}
