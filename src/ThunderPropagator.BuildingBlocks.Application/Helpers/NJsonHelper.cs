using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class NJsonHelper
    {
        private static JsonSerializerSettings BuildDefaultNSerializerSettings() => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static JsonSerializerSettings NJsonSerializerSettings<T>(JsonSerializerSettings? serializerSettings = null)
            => NJsonSerializerSettings(typeof(T), serializerSettings);

        private static JsonSerializerSettings NJsonSerializerSettings(Type type, JsonSerializerSettings? serializerSettings = null)
        {
            serializerSettings ??= BuildDefaultNSerializerSettings();

            var jsonSerializationAttribute = JsonSerializationAttributeCache.Get(type);

            if (jsonSerializationAttribute?.CamelCase == false)
                serializerSettings.ContractResolver = null;

            return serializerSettings;
        }

        public static string ToNJson<T>(this T instance, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

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
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            using var jsonWriter = new JsonTextWriter(streamWriter);

            var serializerSettings = NJsonSerializerSettings<T>(settings is not null ? BuildDefaultNSerializerSettings() : null);
            if (settings is not null)
            {
                settings(serializerSettings);
            }

            var serializer = JsonSerializer.Create(serializerSettings);

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                serializer.Serialize(jsonWriter, exceptionInfo);
            }
            else
            {
                serializer.Serialize(jsonWriter, instance);
            }

            jsonWriter.Flush();
            return memoryStream.ToArray();
        }

        public static string ToNJsonBase64<T>(this T instance, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
            where T : notnull
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJsonBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = instance.ToNJsonBytes(settings);
            return Convert.ToBase64String(bytes);
        }

        public static T? FromNJson<T>(this string json, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

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
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

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

            using var memoryStream = new MemoryStream(bytes);
            using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);

            var serializerSettings = NJsonSerializerSettings<T>(settings is not null ? BuildDefaultNSerializerSettings() : null);
            if (settings is not null)
            {
                settings(serializerSettings);
            }

            var serializer = JsonSerializer.Create(serializerSettings);
            return serializer.Deserialize<T>(jsonReader);
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