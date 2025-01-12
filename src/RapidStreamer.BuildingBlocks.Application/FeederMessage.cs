using Ardalis.GuardClauses;
using JetBrains.Annotations;
using RapidStreamer.BuildingBlocks.Application.Attributes;
using RapidStreamer.BuildingBlocks.Application.CorrelationId;
using RapidStreamer.BuildingBlocks.Application.Enums;
using RapidStreamer.BuildingBlocks.Application.Objects;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RapidStreamer.BuildingBlocks.Application
{
    [JsonSerialization(CamelCase = false)]
    public abstract class FeederMessage : DisposableObject,
        IDictionary<string, object?>,
        IReadOnlyDictionary<string, object?>,
        ICorrelationIdSupport,
        ICloneable,
        ICloneable<IDictionary<string, object?>>
    {
        private readonly ConcurrentDictionary<string, object?> _dictionary = [];

        public object? this[string key]
        {
            get => GetValueOrNull<object>(key)!;
            set => SetValue(value!, key);
        }

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        int ICollection<KeyValuePair<string, object?>>.Count => _dictionary.Count;
        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => _dictionary.Count;

        ICollection<string> IDictionary<string, object?>.Keys => _dictionary.Keys;
        IEnumerable<string> IReadOnlyDictionary<string, object?>.Keys => _dictionary.Keys;
        ICollection<object?> IDictionary<string, object?>.Values => _dictionary.Values;
        IEnumerable<object?> IReadOnlyDictionary<string, object?>.Values => _dictionary.Values;

        internal int? HashKey
        {
            get => GetValueOrNull<int>();
            set => SetValue(value!);
        }

        public CastType CastType
        {
            get => GetValueOrDefault(CastType.Multicast);
            set => SetValue(value);
        }

        public bool IsDeleted
        {
            get => GetValueOrDefault(false);
            set => SetValue(value);
        }

        public string CorrelationId
        {
            get => GetValueOrDefault(string.Empty);
            set => SetValue(value);
        }

        protected FeederMessage() => CastType = CastType.Multicast;

        protected void SetValue(object? value, [CallerMemberName] string? key = null)
        {
            _dictionary[Guard.Against.NullOrWhiteSpace(key)] = value;
        }

        protected T GetValue<T>([CallerMemberName] string? key = null) => (T)_dictionary[Guard.Against.NullOrWhiteSpace(key)]!;

        protected T? GetValueOrNull<T>([CallerMemberName] string? key = null)
            => _dictionary.TryGetValue(Guard.Against.NullOrWhiteSpace(key), out var value) && value is T t ? t : default;

        protected T GetValueOrDefault<T>(T @default, [CallerMemberName] string? key = null) => GetValueOrNull<T>(key) ?? @default;

        object ICloneable.Clone() => MemberwiseClone();
        IDictionary<string, object?> ICloneable<IDictionary<string, object?>>.Clone() => _dictionary;

        void IDictionary<string, object?>.Add(string key, object? value) => _dictionary.TryAdd(key, value);
        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => _dictionary.TryAdd(item.Key, item.Value);
        void ICollection<KeyValuePair<string, object?>>.Clear() => throw new NotImplementedException();
        bool IDictionary<string, object?>.Remove(string key) => throw new NotImplementedException();
        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => throw new NotImplementedException();
        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) => _dictionary.Contains(item);
        bool IDictionary<string, object?>.ContainsKey(string key) => _dictionary.ContainsKey(key);
        bool IReadOnlyDictionary<string, object?>.ContainsKey(string key) => _dictionary.ContainsKey(key);
        bool IDictionary<string, object?>.TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);
        bool IReadOnlyDictionary<string, object?>.TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<string, object?>>)_dictionary).CopyTo(array, arrayIndex);

        [MustDisposeResource]
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _dictionary.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override int GetHashCode() => _dictionary.Keys.Aggregate(0, HashCode.Combine);

        protected override void DisposeManagedResources() => _dictionary.Clear();
    }
}