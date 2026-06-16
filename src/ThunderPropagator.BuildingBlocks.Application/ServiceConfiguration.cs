using Ardalis.GuardClauses;
using CaseConverter;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public interface IServiceConfiguration : IEnumerable<KeyValuePair<string, string>>;

    [JsonConverter(typeof(ServiceConfigurationJsonConverter))]
    public abstract class ServiceConfiguration : IServiceConfiguration,
        INotifyPropertyChanged,
        INotifyPropertyChanging,
        IEquatable<ServiceConfiguration>
    {
        private
#if !DEBUG
            sealed
#endif
            class ServiceConfigurationJsonConverter : JsonConverter<ServiceConfiguration>
        {
            // One allowlist per concrete subclass, built once via reflection and reused.
            private static readonly ConcurrentDictionary<Type, IReadOnlySet<string>> _allowedKeysCache = new();

            private static IReadOnlySet<string> GetAllowedKeys(Type type)
            {
                return _allowedKeysCache.GetOrAdd(type, static t =>
                    t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => p.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase));
            }

            public override void WriteJson(JsonWriter writer, ServiceConfiguration? value, JsonSerializer serializer)
            {
                if (value is null)
                    return;

                writer.WriteStartObject();

                foreach (var item in value._properties)
                {
                    writer.WritePropertyName(item.Key.ToCamelCase());
                    writer.WriteValue(item.Value);
                }

                writer.WriteEndObject();
            }

            public override ServiceConfiguration? ReadJson(JsonReader reader, Type objectType, ServiceConfiguration? existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                var rtn = existingValue ?? Activator.CreateInstance(objectType) as ServiceConfiguration;

                if (rtn is not null)
                {
                    var allowedKeys = GetAllowedKeys(objectType);
                    var jObject = JObject.Load(reader);
                    foreach (var property in jObject)
                    {
                        var key = property.Key.ToPascalCase();
                        if (allowedKeys.Count > 0 && !allowedKeys.Contains(key))
                            continue;
                        rtn._properties[key] = property.Value!.ToString();
                    }
                }

                return rtn;
            }
        }

        private ConcurrentDictionary<string, string> _properties = null!;

        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected ServiceConfiguration() => _properties = new ConcurrentDictionary<string, string>();

        protected ServiceConfiguration(IEnumerable<KeyValuePair<string, string>> properties) => Bind(properties);

        protected ServiceConfiguration(ServiceConfiguration serviceConfiguration) => Bind(serviceConfiguration);

        protected void Bind(IEnumerable<KeyValuePair<string, string>> properties) => _properties = new ConcurrentDictionary<string, string>(properties);

        protected void Bind(ServiceConfiguration serviceConfiguration) => _properties = new ConcurrentDictionary<string, string>(serviceConfiguration._properties);

        protected void Set<T>(T? value, [CallerMemberName] string? key = null)
        {
            var propertyName = Guard.Against.NullOrWhiteSpace(key, nameof(key));

            var type = typeof(T);
            var stringValue = !type.IsClass ? value?.ToString() : value as string ?? value?.ToNJson();

            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                if (_properties.TryGetValue(propertyName, out var previousValue) && stringValue.Equals(previousValue))
                {
                    return;
                }

                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

                _properties[propertyName] = stringValue;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected T? Get<T>([CallerMemberName] string? key = null) => Get(default(T), key);

        protected T Get<T>(T defaultValue, [CallerMemberName] string? key = null)
        {
            var value = _properties.GetValueOrDefault(Guard.Against.NullOrWhiteSpace(key, nameof(key)));
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            var type = typeof(T);

            var rtn = type.IsClass switch
            {
                true when !ReferenceEquals(type, typeof(string)) => value.FromNJson<T>(),
                _ => type.IsEnum switch
                {
                    true when Enum.TryParse(type, value, out var enumValue) => Convert.ChangeType(enumValue, type),
                    _ => type.GetTypeInfo().Name switch
                    {
                        nameof(TimeSpan) => TimeSpan.Parse(value),
                        nameof(Guid) => Guid.Parse(value),
                        _ => Convert.ChangeType(value, type)
                    }
                }
            };

            return rtn is not null ? (T)rtn : defaultValue;
        }

        [MustDisposeResource]
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _properties.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(ServiceConfiguration? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (_properties.Count != other._properties.Count)
                return false;

            using var enumerator = _properties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!other._properties.TryGetValue(enumerator.Current.Key, out var value) || value != enumerator.Current.Value)
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj);

        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        public static implicit operator Dictionary<string, string>(ServiceConfiguration serviceConfiguration)
            => new(serviceConfiguration._properties);

        public static TServiceConfiguration CreateNew<TServiceConfiguration>()
            where TServiceConfiguration : ServiceConfiguration, new()
            => new();

        public static TServiceConfiguration CreateNew<TServiceConfiguration>(IEnumerable<KeyValuePair<string, string>> properties)
            where TServiceConfiguration : ServiceConfiguration, new()
            => new() { _properties = new ConcurrentDictionary<string, string>(properties) };

        public static TServiceConfiguration CreateNew<TServiceConfiguration>(ServiceConfiguration serviceConfiguration)
            where TServiceConfiguration : ServiceConfiguration, new()
            => new() { _properties = new ConcurrentDictionary<string, string>(serviceConfiguration._properties) };
    }
}
