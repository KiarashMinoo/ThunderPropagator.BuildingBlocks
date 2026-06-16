using System.Collections;
using JetBrains.Annotations;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// The user-defined field store of a <see cref="FeederMessage"/>. Keys are C# property names
    /// captured at call sites via <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/>;
    /// values are application-level objects.
    /// </summary>
    /// <remarks>
    /// All operations are protected by an internal lock so that instances may be populated from one
    /// thread and read from another (e.g., during broadcast emission). The trade-off is deliberate:
    /// a plain <see cref="Dictionary{TKey,TValue}"/> with a single <see langword="lock"/> is cheaper
    /// than <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/> for the
    /// typical uncontended, append-only lifecycle of a <see cref="FeederMessage"/>.
    /// </remarks>
    public
#if !DEBUG
        sealed
#endif
        class FeederMessagePayload : IDictionary<string, object?>, IReadOnlyDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _store = [];
        private readonly object _syncRoot = new();

        internal void SetValue(object? value, string key)
        {
            lock (_syncRoot)
            {
                _store[key] = value;
            }
        }

        internal T GetValue<T>(string key)
        {
            lock (_syncRoot)
            {
                return (T)_store[key]!;
            }
        }

        internal T? GetValueOrNull<T>(string key)
        {
            lock (_syncRoot)
            {
                return _store.TryGetValue(key, out var value) && value is T t ? t : default;
            }
        }

        internal T GetValueOrDefault<T>(T @default, string key)
        {
            lock (_syncRoot)
            {
                if (!_store.TryGetValue(key, out var value) || value is not T t)
                    return @default;
                return t;
            }
        }

        internal void Clear()
        {
            lock (_syncRoot)
            {
                _store.Clear();
            }
        }

        internal Dictionary<string, object?> ToDictionary()
        {
            lock (_syncRoot)
            {
                return new(_store);
            }
        }

        object? IDictionary<string, object?>.this[string key]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store[key];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _store[key] = value!;
                }
            }
        }

        object? IReadOnlyDictionary<string, object?>.this[string key]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store[key];
                }
            }
        }

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        int ICollection<KeyValuePair<string, object?>>.Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Count;
                }
            }
        }

        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Count;
                }
            }
        }

        ICollection<string> IDictionary<string, object?>.Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Keys.ToList();
                }
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, object?>.Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Keys.ToList();
                }
            }
        }

        ICollection<object?> IDictionary<string, object?>.Values
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Values.ToList();
                }
            }
        }

        IEnumerable<object?> IReadOnlyDictionary<string, object?>.Values
        {
            get
            {
                lock (_syncRoot)
                {
                    return _store.Values.ToList();
                }
            }
        }

        void IDictionary<string, object?>.Add(string key, object? value)
        {
            lock (_syncRoot)
            {
                _store.TryAdd(key, value);
            }
        }

        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item)
        {
            lock (_syncRoot)
            {
                _store.TryAdd(item.Key, item.Value);
            }
        }

        void ICollection<KeyValuePair<string, object?>>.Clear() => throw new NotSupportedException();

        bool IDictionary<string, object?>.Remove(string key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
        {
            lock (_syncRoot)
            {
                return ((ICollection<KeyValuePair<string, object?>>)_store).Contains(item);
            }
        }

        bool IDictionary<string, object?>.ContainsKey(string key)
        {
            lock (_syncRoot)
            {
                return _store.ContainsKey(key);
            }
        }

        bool IReadOnlyDictionary<string, object?>.ContainsKey(string key)
        {
            lock (_syncRoot)
            {
                return _store.ContainsKey(key);
            }
        }

        bool IDictionary<string, object?>.TryGetValue(string key, out object? value)
        {
            lock (_syncRoot)
            {
                return _store.TryGetValue(key, out value);
            }
        }

        bool IReadOnlyDictionary<string, object?>.TryGetValue(string key, out object? value)
        {
            lock (_syncRoot)
            {
                return _store.TryGetValue(key, out value);
            }
        }

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                ((ICollection<KeyValuePair<string, object?>>)_store).CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc/>
        [MustDisposeResource]
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            KeyValuePair<string, object?>[] snapshot;
            lock (_syncRoot)
            {
                snapshot = [.. _store];
            }
            return ((IEnumerable<KeyValuePair<string, object?>>)snapshot).GetEnumerator();
        }

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
