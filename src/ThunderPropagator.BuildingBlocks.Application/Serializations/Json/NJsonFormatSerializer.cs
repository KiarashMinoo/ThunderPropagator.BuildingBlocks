using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Json
{
    /// <summary>
    /// <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/> implementation
    /// backed by Newtonsoft.Json.
    /// </summary>
    public sealed class NJsonFormatSerializer : IFormatSerializer, IFormatDeserializer
    {
        private static readonly CamelCasePropertyNamesContractResolver _camelCaseResolver = new();

        private static readonly JsonSerializerSettings _defaultSettings = new()
        {
            ContractResolver = _camelCaseResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <inheritdoc/>
        public SerializerType SerializerType => SerializerType.NJson;

        /// <inheritdoc/>
        public string MediaType => SerializerMediaTypes.Json;

        /// <inheritdoc/>
        public string Serialize<T>(T instance)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Serialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var settings = NJsonSerializerSettings<T>();

            if (instance is Exception exception)
            {
                ExceptionInfo exceptionInfo = new(exception);
                return JsonConvert.SerializeObject(exceptionInfo, settings);
            }

            return JsonConvert.SerializeObject(instance, settings);
        }

        /// <inheritdoc/>
        public byte[] SerializeToBytes<T>(T instance)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(SerializeToBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Encoding.UTF8.GetBytes(Serialize<T>(instance));
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string data)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(data, NJsonSerializerSettings<T>());
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(byte[] bytes)
        {
            const string activityName = $"{nameof(NJsonFormatSerializer)}_{nameof(Deserialize)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            if (bytes.Length == 0)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, NJsonSerializerSettings<T>());
        }

        private static JsonSerializerSettings NJsonSerializerSettings<T>()
        {
            var attribute = JsonSerializationAttributeCache.Get(typeof(T));
            if (attribute?.CamelCase == false)
            {
                return new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            }

            return _defaultSettings;
        }
    }
}
