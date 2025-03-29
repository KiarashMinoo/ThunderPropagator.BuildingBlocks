using System.Collections.Concurrent;
using System.Diagnostics;
using RapidStreamer.BuildingBlocks.Application.Attributes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class JsonHelper
    {
        private static readonly ConcurrentDictionary<Type, JsonSerializationAttribute?> JsonSerializationAttributes = new();

        private static JsonSerializerOptions BuildDefaultSerializerOptions()
            => new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

        private static JsonSerializerOptions JsonSerializerOptions<T>(JsonSerializerOptions? serializerOptions = null) => JsonSerializerOptions(typeof(T), serializerOptions);

        private static JsonSerializerOptions JsonSerializerOptions(Type type, JsonSerializerOptions? serializerOptions = null)
        {
            var jsonSerializationAttribute = JsonSerializationAttributes.GetOrAdd(type, key =>
            {
                var jsonSerializationAttributes = key.GetCustomAttributes(typeof(JsonSerializationAttribute), true);
                if (jsonSerializationAttributes.Length == 0)
                    return null;

                return jsonSerializationAttributes.First() as JsonSerializationAttribute;
            });

            serializerOptions ??= BuildDefaultSerializerOptions();

            if (serializerOptions is { IsReadOnly: false, PropertyNamingPolicy: not null } && jsonSerializationAttribute?.CamelCase == false)
                serializerOptions.PropertyNamingPolicy = null;

            return serializerOptions;
        }

        public static string ToJson<T>(this T instance, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(ToJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = BuildDefaultSerializerOptions();
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
            where T : notnull
        {
            var jsonStr = instance.ToJson(options);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            return bytes;
        }

        public static string ToJsonBase64<T>(this T instance, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
            where T : notnull
        {
            var bytes = instance.ToJsonBytes(options);
            return Convert.ToBase64String(bytes)[..^2];
        }

        public static T? FromJson<T>(this string json, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(FromJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = BuildDefaultSerializerOptions();
                options(serializerOptions);
            }

            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions<T>(serializerOptions));
        }

        public static object? FromJson(this string json, Type type, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
        {
            const string activityName = $"{nameof(JsonHelper)}_{nameof(FromJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = BuildDefaultSerializerOptions();
                options(serializerOptions);
            }

            return JsonSerializer.Deserialize(json, type, JsonSerializerOptions(type, serializerOptions));
        }

        public static T? FromJsonBytes<T>(this byte[] bytes, Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
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

            return jsonStr.FromJson<T>(options);
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