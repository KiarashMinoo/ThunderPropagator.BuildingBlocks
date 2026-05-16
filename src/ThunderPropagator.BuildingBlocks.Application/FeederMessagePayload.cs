using System.Collections;
using JetBrains.Annotations;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// The user-defined field store of a <see cref="FeederMessage"/>. Keys are C# property names
    /// captured at call sites via <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/>;
    /// values are application-level objects.
    /// </summary>
    public
#if !DEBUG
        sealed
#endif
        class FeederMessagePayload : IDictionary<string, object?>, IReadOnlyDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _store = [];

        internal void SetValue(object? value, string key) => _store[key] = value;

        internal T GetValue<T>(string key) => (T)_store[key]!;

        internal T? GetValueOrNull<T>(string key)
            => _store.TryGetValue(key, out var value) && value is T t ? t : default;

        internal T GetValueOrDefault<T>(T @default, string key) => GetValueOrNull<T>(key) ?? @default;

        internal void Clear() => _store.Clear();

        internal Dictionary<string, object?> ToDictionary() => new(_store);

        object? IDictionary<string, object?>.this[string key]
        {
            get => _store[key];
            set => _store[key] = value!;
        }

        object? IReadOnlyDictionary<string, object?>.this[string key] => _store[key];

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        int ICollection<KeyValuePair<string, object?>>.Count => _store.Count;
        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => _store.Count;

        ICollection<string> IDictionary<string, object?>.Keys => _store.Keys;
        IEnumerable<string> IReadOnlyDictionary<string, object?>.Keys => _store.Keys;
        ICollection<object?> IDictionary<string, object?>.Values => _store.Values;
        IEnumerable<object?> IReadOnlyDictionary<string, object?>.Values => _store.Values;

        void IDictionary<string, object?>.Add(string key, object? value) => _store.TryAdd(key, value);
        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => _store.TryAdd(item.Key, item.Value);
        void ICollection<KeyValuePair<string, object?>>.Clear() => throw new NotSupportedException();
        bool IDictionary<string, object?>.Remove(string key) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) => ((ICollection<KeyValuePair<string, object?>>)_store).Contains(item);
        bool IDictionary<string, object?>.ContainsKey(string key) => _store.ContainsKey(key);
        bool IReadOnlyDictionary<string, object?>.ContainsKey(string key) => _store.ContainsKey(key);
        bool IDictionary<string, object?>.TryGetValue(string key, out object? value) => _store.TryGetValue(key, out value);
        bool IReadOnlyDictionary<string, object?>.TryGetValue(string key, out object? value) => _store.TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<string, object?>>)_store).CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        [MustDisposeResource]
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _store.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
