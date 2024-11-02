using Ardalis.GuardClauses;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Application.Collections
{
    public interface IOrderedEqualityComparer<in TKey> : IEqualityComparer,
        IEqualityComparer<TKey>;

    [SuppressMessage("ReSharper", "RedundantCast")]
    public
#if !DEBUG
        sealed
#endif
        class GenericOrderedDictionary<TKey, TValue> :
        IOrderedDictionary,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly OrderedDictionary _dictionary;


        public GenericOrderedDictionary() : this(0)
        {
        }

        public GenericOrderedDictionary(IOrderedEqualityComparer<TKey>? comparer) : this(0, comparer)
        {
        }

        public GenericOrderedDictionary(int capacity, IOrderedEqualityComparer<TKey>? comparer = null)
        {
            _dictionary = new OrderedDictionary(capacity, comparer);
        }

        public GenericOrderedDictionary(IDictionary<TKey, TValue> dictionary, IOrderedEqualityComparer<TKey>? comparer = null) :
            this(dictionary.Count, comparer)
            => AddRange(Guard.Against.Null(dictionary, nameof(dictionary)));

        public GenericOrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IOrderedEqualityComparer<TKey>? comparer = null) :
            this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
            => AddRange(Guard.Against.Null(collection, nameof(collection)));

        private static void ThrowIfIsNotTKey(object key)
        {
            if (key is not TKey)
            {
                throw new InvalidCastException(nameof(key));
            }
        }

        private static void ThrowIfIsNotTValue(object? value)
        {
            if (value is not TValue)
            {
                throw new InvalidCastException(nameof(TValue));
            }
        }

        private static TKey AsTKey(object input)
        {
            return (TKey)input;
        }

        private static TValue AsTValue(object? input)
        {
            return input is not null ? (TValue)input : default!;
        }

        private void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            if (enumerable is IDictionary<TKey, TValue> dictionary)
            {
                if (dictionary.Count == 0)
                {
                    return;
                }

                foreach (var (key, value) in dictionary)
                {
                    Add(key, value);
                }

                return;
            }

            ReadOnlySpan<KeyValuePair<TKey, TValue>> span;
            if (enumerable is KeyValuePair<TKey, TValue>[] array)
            {
                span = array;
            }
            else if (enumerable.GetType() == typeof(List<KeyValuePair<TKey, TValue>>))
            {
                span = CollectionsMarshal.AsSpan((List<KeyValuePair<TKey, TValue>>)enumerable);
            }
            else
            {
                foreach (KeyValuePair<TKey, TValue> item in enumerable)
                {
                    Add(item.Key, item.Value);
                }

                return;
            }

            foreach (KeyValuePair<TKey, TValue> item in span)
            {
                Add(item.Key, item.Value);
            }
        }

        #region "IDictionary<TKey, TValue>"

        public TValue this[TKey key]
        {
            get => AsTValue(_dictionary[(object)key]);
            set => _dictionary[key] = value;
        }

        public ICollection<TKey> Keys => _dictionary.Keys.Cast<TKey>().ToArray();
        public ICollection<TValue> Values => _dictionary.Values.Cast<TValue>().ToArray();

        public bool ContainsKey(TKey key)
        {
            return _dictionary.Contains((object)key);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            _dictionary.Remove((object)key);
            return !ContainsKey(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return (value = AsTValue(_dictionary[(object)key])) is not null;
        }

        #region "ICollection<KeyValuePair<TKey, TValue>>"

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array, nameof(array));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, _dictionary.Count, nameof(arrayIndex));
            ArgumentOutOfRangeException.ThrowIfLessThan(array.Length - arrayIndex, _dictionary.Count, nameof(array));

            var keys = (IList)_dictionary.Keys;
            var values = (IList)_dictionary.Values;

            for (var index = 0; index < _dictionary.Count - arrayIndex; index++)
            {
                var key = AsTKey(keys[arrayIndex]!);
                var value = AsTValue(values[arrayIndex]);

                array[index] = new KeyValuePair<TKey, TValue>(key, value);
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #endregion

        #region "IOrderedDictionary"

        object? IOrderedDictionary.this[int index]
        {
            get => _dictionary[index];
            set => _dictionary[index] = value;
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        void IOrderedDictionary.Insert(int index, object key, object? value)
        {
            ThrowIfIsNotTKey(key);
            ThrowIfIsNotTValue(value);
            _dictionary.Insert(index, key, value);
        }

        void IOrderedDictionary.RemoveAt(int index)
        {
            _dictionary.RemoveAt(index);
        }

        #region "IDictionary"

        object? IDictionary.this[object key]
        {
            get
            {
                ThrowIfIsNotTKey(key);
                return _dictionary[key];
            }
            set
            {
                ThrowIfIsNotTKey(key);
                ThrowIfIsNotTValue(value);
                _dictionary[key] = value;
            }
        }

        ICollection IDictionary.Keys => _dictionary.Keys;
        ICollection IDictionary.Values => _dictionary.Values;
        bool IDictionary.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;
        bool IDictionary.IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;

        bool IDictionary.Contains(object key)
        {
            ThrowIfIsNotTKey(key);
            return _dictionary.Contains(key);
        }

        void IDictionary.Add(object key, object? value)
        {
            ThrowIfIsNotTKey(key);
            ThrowIfIsNotTValue(value);
            Add(AsTKey(key), AsTValue(value));
        }

        void IDictionary.Clear()
        {
            _dictionary.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            _dictionary.Remove(key);
        }

        #endregion

        #region "ICollection"

        int ICollection.Count => ((IDictionary)_dictionary).Count;
        object ICollection.SyncRoot => ((IDictionary)_dictionary).SyncRoot;
        bool ICollection.IsSynchronized => ((IDictionary)_dictionary).IsSynchronized;

        void ICollection.CopyTo(Array array, int index)
        {
            _dictionary.CopyTo(array, index);
        }

        #endregion

        #endregion

        #region "IReadOnlyDictionary<TKey, TValue>"

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => AsTValue(_dictionary[(object)key]);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys.Cast<TKey>().ToArray().AsReadOnly();
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values.Cast<TValue>().ToArray().AsReadOnly();

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return ContainsKey(key);
        }

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return TryGetValue(key, out value);
        }

        #region "IReadOnlyCollection<KeyValuePair<TKey, TValue>>"

        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => _dictionary.Count;

        #endregion

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new GenericOrderedDictionaryEnumerator(this);
        }

        [SuppressMessage("ReSharper", "NotDisposedResource")]
        private
#if !DEBUG
            sealed
#endif
            class GenericOrderedDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IDictionaryEnumerator _enumerator;

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    var key = AsTKey(_enumerator.Entry.Key);
                    var value = AsTValue(_enumerator.Value);
                    return new KeyValuePair<TKey, TValue>(key, value);
                }
            }

            object IEnumerator.Current => Current;

            [MustDisposeResource]
            internal GenericOrderedDictionaryEnumerator(GenericOrderedDictionary<TKey, TValue> dictionary)
            {
                _enumerator = dictionary._dictionary.GetEnumerator();
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public void Dispose()
            {
                (_enumerator as IDisposable)?.Dispose();
            }
        }
    }
}