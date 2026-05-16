using System.Text;
using NetJSON;
using ThunderPropagator.BuildingBlocks.Application.Attributes;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class NetJsonHelper
    {
        private static NetJSONSettings BuildDefaultNSerializerSettings()
            => new()
            {
                CamelCase = true
            };

        private static NetJSONSettings NetJsonSettings<T>(NetJSONSettings? serializerSettings = null)
            => NetJsonSettings(typeof(T), serializerSettings);

        private static NetJSONSettings NetJsonSettings(Type type, NetJSONSettings? netJsonSettings = null)
        {
            netJsonSettings ??= BuildDefaultNSerializerSettings();

            var jsonSerializationAttribute = JsonSerializationAttributeCache.Get(type);

            if (jsonSerializationAttribute?.CamelCase == false)
                netJsonSettings.CamelCase = false;

            return netJsonSettings;
        }

        public static string ToNetJson<T>(this T instance, Func<NetJSONSettings, NetJSONSettings>? settings = null)
        {
            const string activityName = $"{nameof(NetJsonHelper)}_{nameof(ToNetJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            NetJSONSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return NetJSON.NetJSON.Serialize(exceptionInfo, NetJsonSettings<T>(serializerSettings));
            }

            return NetJSON.NetJSON.Serialize(instance, NetJsonSettings<T>(serializerSettings));
        }

        public static byte[] ToNetJsonBytes<T>(this T instance, Func<NetJSONSettings, NetJSONSettings>? settings = null)
            where T : notnull
        {
            const string activityName = $"{nameof(NetJsonHelper)}_{nameof(ToNetJsonBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var jsonStr = instance.ToNetJson(settings);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            return bytes;
        }

        public static string ToNetJsonBase64<T>(this T instance, Func<NetJSONSettings, NetJSONSettings>? settings = null)
            where T : notnull
        {
            const string activityName = $"{nameof(NetJsonHelper)}_{nameof(ToNetJsonBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            var bytes = instance.ToNetJsonBytes(settings);
            return Convert.ToBase64String(bytes);
        }

        public static T? FromNetJson<T>(this string json, Func<NetJSONSettings, NetJSONSettings>? settings = null)
        {
            const string activityName = $"{nameof(NetJsonHelper)}_{nameof(FromNetJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            NetJSONSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            return NetJSON.NetJSON.Deserialize<T>(json, NetJsonSettings<T>(serializerSettings));
        }

        public static object? FromNetJson(this string json, Type type, Func<NetJSONSettings, NetJSONSettings>? settings = null)
        {
            const string activityName = $"{nameof(NetJsonHelper)}_{nameof(FromNetJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            NetJSONSettings? serializerSettings = null;

            if (settings is not null)
            {
                serializerSettings = BuildDefaultNSerializerSettings();
                settings(serializerSettings);
            }

            return NetJSON.NetJSON.Deserialize(type, json, NetJsonSettings(type, serializerSettings));
        }

        public static T? FromNetJsonBytes<T>(this byte[] bytes, Func<NetJSONSettings, NetJSONSettings>? settings = null)
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

            return jsonStr.FromNetJson<T>(settings);
        }

        public static T? FromNetJsonBase64<T>(this string str, Func<NetJSONSettings, NetJSONSettings>? settings = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }

            var bytes = Convert.FromBase64String(str);

            return bytes.FromNetJsonBytes<T>(settings);
        }
    }
}