using ThunderPropagator.BuildingBlocks.Application.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class JsonHelper
    {
        internal static JsonSerializerOptions BuildDefaultSerializerOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        internal static JsonSerializerOptions JsonSerializerOptions<T>(JsonSerializerOptions? serializerOptions = null) => JsonSerializerOptions(typeof(T), serializerOptions);

        internal static JsonSerializerOptions JsonSerializerOptions(Type type, JsonSerializerOptions? serializerOptions = null)
        {
            var jsonSerializationAttribute = JsonSerializationAttributeCache.Get(type);

            serializerOptions ??= BuildDefaultSerializerOptions();

            if (serializerOptions is { IsReadOnly: false, PropertyNamingPolicy: not null } && jsonSerializationAttribute?.CamelCase == false)
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
            const string activityName = $"{nameof(JsonHelper)}_{nameof(ToJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            JsonSerializerOptions? serializerOptions = null;

            if (options is not null)
            {
                serializerOptions = BuildDefaultSerializerOptions();
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
            where T : notnull
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
                serializerOptions = BuildDefaultSerializerOptions();
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
                serializerOptions = BuildDefaultSerializerOptions();
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
                serializerOptions = BuildDefaultSerializerOptions();
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