using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class NJsonHelper
    {
        private static readonly CamelCasePropertyNamesContractResolver _camelCaseResolver = new();
        private static readonly ConcurrentDictionary<int, JsonSerializerSettings> _settingsCache = new();

        private static JsonSerializerSettings GetCachedSettings(TypeNameHandling typeNameHandling, bool camelCase)
        {
            var key = HashCode.Combine((int)typeNameHandling, camelCase);
            return _settingsCache.GetOrAdd(key, static (_, args) =>
                new JsonSerializerSettings
                {
                    ContractResolver = args.camelCase ? _camelCaseResolver : null,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = args.typeNameHandling
                }, (typeNameHandling, camelCase));
        }

        private static bool IsCamelCase(Type type)
        {
            return JsonSerializationAttributeCache.Get(type)?.CamelCase != false;
        }

        private static JsonSerializerSettings BuildDefaultNSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = _camelCaseResolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        private static JsonSerializerSettings NJsonSerializerSettings<T>(JsonSerializerSettings? serializerSettings = null)
        {
            return NJsonSerializerSettings(typeof(T), serializerSettings);
        }

        private static JsonSerializerSettings NJsonSerializerSettings(Type type, JsonSerializerSettings? serializerSettings = null)
        {
            if (serializerSettings is null)
            {
                return GetCachedSettings(TypeNameHandling.None, IsCamelCase(type));
            }

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
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(ToNJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

            if (settings is null)
            {
                return JsonConvert.DeserializeObject<T>(json, GetCachedSettings(TypeNameHandling.None, IsCamelCase(typeof(T))));
            }

            var serializerSettings = BuildDefaultNSerializerSettings();
            settings(serializerSettings);
            return JsonConvert.DeserializeObject<T>(json, NJsonSerializerSettings<T>(serializerSettings));
        }

        public static T? FromNJson<T>(this string json, TypeNameHandling typeNameHandling)
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return JsonConvert.DeserializeObject<T>(json, GetCachedSettings(typeNameHandling, IsCamelCase(typeof(T))));
        }

        public static object? FromNJson(this string json, Type type, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (settings is null)
            {
                return JsonConvert.DeserializeObject(json, type, GetCachedSettings(TypeNameHandling.None, IsCamelCase(type)));
            }

            var serializerSettings = BuildDefaultNSerializerSettings();
            settings(serializerSettings);
            return JsonConvert.DeserializeObject(json, type, NJsonSerializerSettings(type, serializerSettings));
        }

        public static object? FromNJson(this string json, Type type, TypeNameHandling typeNameHandling)
        {
            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return JsonConvert.DeserializeObject(json, type, GetCachedSettings(typeNameHandling, IsCamelCase(type)));
        }

        public static T? FromNJsonBytes<T>(this byte[] bytes, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var json = Encoding.UTF8.GetString(bytes);

            if (settings is null)
            {
                return JsonConvert.DeserializeObject<T>(json, GetCachedSettings(TypeNameHandling.None, IsCamelCase(typeof(T))));
            }

            var serializerSettings = BuildDefaultNSerializerSettings();
            settings(serializerSettings);
            return JsonConvert.DeserializeObject<T>(json, NJsonSerializerSettings<T>(serializerSettings));
        }

        public static T? FromNJsonBytes<T>(this byte[] bytes, TypeNameHandling typeNameHandling)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            const string activityName = $"{nameof(NJsonHelper)}_{nameof(FromNJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, GetCachedSettings(typeNameHandling, IsCamelCase(typeof(T))));
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

        public static T? FromNJsonBase64<T>(this string str, TypeNameHandling typeNameHandling)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            var bytes = Convert.FromBase64String(str);

            return bytes.FromNJsonBytes<T>(typeNameHandling);
        }

        public static void PopulateFromNJson<T>(this string json, T target, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            if (settings is null)
            {
                JsonConvert.PopulateObject(json, target, GetCachedSettings(TypeNameHandling.None, IsCamelCase(typeof(T))));
                return;
            }

            var serializerSettings = BuildDefaultNSerializerSettings();
            settings(serializerSettings);
            JsonConvert.PopulateObject(json, target, NJsonSerializerSettings<T>(serializerSettings));
        }

        public static void PopulateFromNJsonBytes<T>(this byte[] bytes, T target, Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
        {
            if (bytes.Length == 0)
                return;

            var json = Encoding.UTF8.GetString(bytes);
            json.PopulateFromNJson(target, settings);
        }
    }
}
