using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RapidStreamer.BuildingBlocks.Application.Attributes;
using RapidStreamer.BuildingBlocks.Application.Collections;
using System.Diagnostics;
using System.Text;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class NJsonHelper
    {
        private static readonly ConcurrentDictionary<Type, JsonSerializationAttribute?> JsonSerializationAttributes = new();

        private static JsonSerializerSettings BuildDefaultNSerializerSettings()
            => new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        private static JsonSerializerSettings NJsonSerializerSettings<T>(JsonSerializerSettings? serializerSettings = null)
            => NJsonSerializerSettings(typeof(T), serializerSettings);

        private static JsonSerializerSettings NJsonSerializerSettings(Type type, JsonSerializerSettings? serializerSettings = null)
        {
            serializerSettings ??= BuildDefaultNSerializerSettings();

            var jsonSerializationAttribute = JsonSerializationAttributes.GetOrAdd(type, key =>
            {
                var jsonSerializationAttributes = key.GetCustomAttributes(typeof(JsonSerializationAttribute), true);
                if (jsonSerializationAttributes.Length == 0)
                    return null;

                return jsonSerializationAttributes.First() as JsonSerializationAttribute;
            });

            if (jsonSerializationAttribute?.CamelCase == false)
                serializerSettings.ContractResolver = null;

            return serializerSettings;
        }

        public static string ToNJson<T>(this T instance, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
#if DEBUG
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            JsonSerializerSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return JsonConvert.SerializeObject(exceptionInfo, NJsonSerializerSettings<T>(serializerSettings));
            }

            return JsonConvert.SerializeObject(instance, NJsonSerializerSettings<T>(serializerSettings));
        }

        public static byte[] ToNJsonBytes<T>(this T instance, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
            where T : notnull
        {
#if DEBUG
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJsonBytes)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            try
            {
                var jsonStr = instance.ToNJson(settings);
                var bytes = Encoding.UTF8.GetBytes(jsonStr);
                return bytes;
            }
            finally
            {
#if DEBUG
                activity?.Stop();
#endif
            }
        }

        public static string ToNJsonBase64<T>(this T instance, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
            where T : notnull
        {
#if DEBUG
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJsonBase64)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            var bytes = instance.ToNJsonBytes(settings);
            return Convert.ToBase64String(bytes)[..^2];
        }

        public static T? FromNJson<T>(this string json, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
#if DEBUG
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif

            JsonSerializerSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            return JsonConvert.DeserializeObject<T>(json, NJsonSerializerSettings<T>(serializerSettings));
        }

        public static object? FromNJson(this string json, Type type, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
#if DEBUG
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            JsonSerializerSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            return JsonConvert.DeserializeObject(json, type, NJsonSerializerSettings(type, serializerSettings));
        }

        public static T? FromNJsonBytes<T>(this byte[] bytes, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
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

            return jsonStr.FromNJson<T>(settings);
        }

        public static T? FromNJsonBase64<T>(this string str, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            var bytes = Convert.FromBase64String(str);

            return bytes.FromNJsonBytes<T>(settings);
        }
    }
}