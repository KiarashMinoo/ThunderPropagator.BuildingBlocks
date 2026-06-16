using Ardalis.GuardClauses;
using CaseConverter;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
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
        // One allowlist per concrete subclass, built once via reflection and reused by both
        // the JSON converter and the CreateNew factory.
        private static readonly ConcurrentDictionary<Type, IReadOnlySet<string>> _allowedKeysCache = new();

        private static IReadOnlySet<string> GetAllowedKeys(Type type)
        {
            return _allowedKeysCache.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase));
        }

        // Sensitive-key set per concrete subclass — properties marked [SensitiveData] are
        // encrypted on write and decrypted on read by the JSON converter.
        private static readonly ConcurrentDictionary<Type, IReadOnlySet<string>> _sensitiveKeysCache = new();

        private static IReadOnlySet<string> GetSensitiveKeys(Type type)
        {
            return _sensitiveKeysCache.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null)
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase));
        }

        private
#if !DEBUG
            sealed
#endif
            class ServiceConfigurationJsonConverter : JsonConverter<ServiceConfiguration>
        {
            public override void WriteJson(JsonWriter writer, ServiceConfiguration? value, JsonSerializer serializer)
            {
                if (value is null)
                    return;

                writer.WriteStartObject();

                var sensitiveKeys = GetSensitiveKeys(value.GetType());

                foreach (var item in value._properties)
                {
                    writer.WritePropertyName(item.Key.ToCamelCase());
                    var v = SensitiveDataEncryption.IsConfigured && sensitiveKeys.Contains(item.Key)
                        ? SensitiveDataEncryption.Encrypt(item.Value)
                        : item.Value;
                    writer.WriteValue(v);
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
                    var sensitiveKeys = GetSensitiveKeys(objectType);
                    var jObject = JObject.Load(reader);
                    foreach (var property in jObject)
                    {
                        var key = property.Key.ToPascalCase();
                        if (allowedKeys.Count > 0 && !allowedKeys.Contains(key))
                            continue;
                        var rawValue = property.Value!.ToString();
                        rtn._properties[key] = SensitiveDataEncryption.IsConfigured && sensitiveKeys.Contains(key)
                            ? SensitiveDataEncryption.Decrypt(rawValue)
                            : rawValue;
                    }
                }

                return rtn;
            }
        }

        private ConcurrentDictionary<string, string> _properties = null!;

        private PropertyChangingEventHandler? _propertyChanging;
        private PropertyChangedEventHandler? _propertyChanged;

        // Explicit interface implementations prevent external code from subscribing
        // directly on the concrete type and observing sensitive property writes.
        // Subscription still works via the INotifyPropertyChanging / INotifyPropertyChanged
        // interface references, which is the intended usage pattern.
        event PropertyChangingEventHandler? INotifyPropertyChanging.PropertyChanging
        {
            add => _propertyChanging += value;
            remove => _propertyChanging -= value;
        }

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }

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

                _propertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

                _properties[propertyName] = stringValue;

                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        {
            ArgumentNullException.ThrowIfNull(properties);

            var allowedKeys = GetAllowedKeys(typeof(TServiceConfiguration));
            var filtered = allowedKeys.Count > 0
                ? properties.Where(kv => allowedKeys.Contains(kv.Key))
                : properties;
            return new() { _properties = new ConcurrentDictionary<string, string>(filtered) };
        }

        public static TServiceConfiguration CreateNew<TServiceConfiguration>(ServiceConfiguration serviceConfiguration)
            where TServiceConfiguration : ServiceConfiguration, new()
            => new() { _properties = new ConcurrentDictionary<string, string>(serviceConfiguration._properties) };

        /// <summary>
        /// Configures the AES key used to encrypt properties marked with
        /// <see cref="Attributes.SensitiveDataAttribute"/> during JSON serialization and to decrypt
        /// them during JSON deserialization. Delegates to
        /// <see cref="SensitiveDataEncryption.Configure"/>; subsequent calls are silently ignored.
        /// </summary>
        /// <param name="key">The AES key bytes (16, 24, or 32 bytes).</param>
        public static void ConfigureEncryption(byte[] key)
        {
            SensitiveDataEncryption.Configure(key);
        }
    }
}
