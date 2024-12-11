using JetBrains.Annotations;
using Newtonsoft.Json;
using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;
using RapidStreamer.BuildingBlocks.Application.Objects;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace RapidStreamer.BuildingBlocks.Application.Collections
{
    public delegate void DictionaryCleared(object sender);

    public delegate void DictionaryKeyChanged<in TKey>(object sender, TKey key, NotifiableObject.NotifiableChangeType changeType)
        where TKey : notnull;

    public delegate void DictionaryValueChanged<in TKey, in TValue>(object sender, TKey key, TValue value, NotifiableObject.NotifiableChangeType changeType)
        where TKey : notnull;

    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class BindingDictionary<TKey, TValue> :
        NotifiableObject,
        IDictionary,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IEquatable<IDictionary<TKey, TValue>>,
        IChangeTrackingObject<TKey, TValue>
        where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly ChangeTrackingObjectAdapter<TKey, TValue> _changeTrackingObjectAdapter = new();

        //Properties
        public bool ConcurrentSupport => _dictionary is ConcurrentDictionary<TKey, TValue>;

        public TValue this[TKey key]
        {
            get => GetValueOrDefault(key);
            set => AddOrUpdate(key, value);
        }

        public object? this[object key]
        {
            get => this[AsTKey(key)];
            set => this[AsTKey(key)] = AsTValue(value);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        ICollection IDictionary.Keys => ((IDictionary)_dictionary).Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        ICollection IDictionary.Values => ((IDictionary)_dictionary).Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;


        public int Count => _dictionary.Count;
        public bool IsSynchronized => ((ICollection)_dictionary).IsSynchronized;
        public object SyncRoot => ((ICollection)_dictionary).SyncRoot;
        public bool IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
        public bool IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;

        //Events
        public event DictionaryCleared? Cleared;
        public event DictionaryKeyChanged<TKey>? KeyChanged;
        public event DictionaryValueChanged<TKey, TValue>? ValueChanged;


        [JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        public BindingDictionary() : this(false)
        {
        }

        public BindingDictionary(bool concurrentSupport) => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>() : new Dictionary<TKey, TValue>();

        public BindingDictionary(int capacity, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity) : new Dictionary<TKey, TValue>(capacity);

        public BindingDictionary(IEqualityComparer<TKey>? comparer, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(comparer) : new Dictionary<TKey, TValue>(comparer);

        public BindingDictionary(int capacity, IEqualityComparer<TKey>? comparer, bool concurrentSupport = false)
            => _dictionary = concurrentSupport
                ? new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity, comparer)
                : new Dictionary<TKey, TValue>(capacity, comparer);

        public BindingDictionary(IDictionary<TKey, TValue> dictionary, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(dictionary) : new Dictionary<TKey, TValue>(dictionary);

        public BindingDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(dictionary, comparer) : new Dictionary<TKey, TValue>(dictionary, comparer);

        public BindingDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(collection) : new Dictionary<TKey, TValue>(collection);

        public BindingDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer, bool concurrentSupport = false)
            => _dictionary = concurrentSupport ? new ConcurrentDictionary<TKey, TValue>(collection, comparer) : new Dictionary<TKey, TValue>(collection, comparer);

        private void OnCleared()
        {
            Cleared?.Invoke(this);
        }

        private void OnKeyChanged(TKey key, NotifiableChangeType changeType)
        {
            KeyChanged?.Invoke(this, key, changeType);
        }

        private void OnValueChanged(TKey key, TValue value, NotifiableChangeType changeType)
        {
            ValueChanged?.Invoke(this, key, value, changeType);
        }

        private static TKey AsTKey(object key) => key is not TKey keyVal ? throw new InconvertibleException($"key: {key} is not convertable to type {typeof(TKey)}") : keyVal;

        private static TValue AsTValue(object? value)
            => value switch
            {
                null => default!,
                _ => value is not TValue valueVal ? throw new InconvertibleException($"value: {value} is not convertable to type {typeof(TValue)}") : valueVal
            };

        public void Clear()
        {
            _dictionary.Clear();
            _changeTrackingObjectAdapter.Clear();
            OnCleared();
        }

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MustDisposeResource]
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_dictionary).GetEnumerator();

        [MustDisposeResource]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        public void CopyTo(Array array, int index) => ((IDictionary)_dictionary).CopyTo(array, index);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

        public bool Contains(object key) => ContainsKey(AsTKey(key));
        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public void Remove(object key)
        {
            Remove(AsTKey(key));
        }

        public bool Remove(TKey key)
        {
            var value = GetValue(key);

            if (!_dictionary.Remove(key))
            {
                return false;
            }

            OnKeyChanged(key, NotifiableChangeType.Removed);
            OnValueChanged(key, value, NotifiableChangeType.Removed);
            _changeTrackingObjectAdapter.ReportRemoved(key, value);

            if (Count == 0)
            {
                OnCleared();
            }

            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        void IDictionary.Add(object key, object? value) => TryAddInternal(AsTKey(key), AsTValue(value), true);
        public void Add(TKey key, TValue value) => TryAddInternal(key, value, true);
        public void Add(KeyValuePair<TKey, TValue> item) => TryAddInternal(item.Key, item.Value, true);
        public bool TryAdd(TKey key, TValue value) => TryAddInternal(key, value, false);

        private bool TryAddInternal(TKey key, TValue value, bool raiseException)
        {
            var isAdded = _dictionary switch
            {
                ConcurrentDictionary<TKey, TValue> concurrentDictionary => concurrentDictionary.TryAdd(key, value),
                _ => _dictionary.TryAdd(key, value)
            };

            if (isAdded)
            {
                OnKeyChanged(key, NotifiableChangeType.Added);
                OnValueChanged(key, value, NotifiableChangeType.Added);
                _changeTrackingObjectAdapter.ReportAdded(key, value);
            }
            else if (raiseException)
                throw new InvalidOperationException(
                    $"There’s already an entry with the key '{key}' in the dictionary. Each key must be unique, so please check and use a different key");

            return isAdded;
        }

        public TValue? AddOrUpdate(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var previousValue))
                TryAdd(key, value);
            else
                TryUpdate(key, value, previousValue);

            return previousValue;
        }

        public TValue AddOrUpdate(TKey key, Func<TValue> addValueFactory, Func<TValue, TValue> updateValueFactory)
            => AddOrUpdate(key, _ => addValueFactory.Invoke(), (_, previousValue) => updateValueFactory.Invoke(previousValue));

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (!TryGetValue(key, out var previousValue))
            {
                var value = addValueFactory.Invoke(key);
                TryAdd(key, value);
                return value;
            }
            else
            {
                var value = updateValueFactory(key, previousValue);

                if (!Equals(previousValue, value))
                {
                    _dictionary[key] = value;

                    OnValueChanged(key, value, NotifiableChangeType.Modified);
                    _changeTrackingObjectAdapter.ReportModified(key, previousValue, value);
                }

                return value;
            }
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            var isUpdated = true;
            if (_dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary)
                isUpdated = concurrentDictionary.TryUpdate(key, newValue, comparisonValue);
            else
            {
                if (TryGetValue(key, out comparisonValue!) && !Equals(comparisonValue, newValue))
                    _dictionary[key] = newValue;
                else
                    isUpdated = false;
            }

            if (isUpdated)
            {
                OnValueChanged(key, newValue, NotifiableChangeType.Modified);
                _changeTrackingObjectAdapter.ReportModified(key, comparisonValue, newValue);
            }

            return isUpdated;
        }

        public TValue GetValue(TKey key)
            => !TryGetValue(key, out var value)
                ? throw new KeyNotFoundException("The key you’re looking for does not exist in the dictionary. Please check the key and try again.")
                : value;

        public TValue? GetValueOrNull(TKey key) => TryGetValue(key, out var value) ? value : default;
        public TValue GetValueOrDefault(TKey key, TValue @default = default!) => TryGetValue(key, out var value) ? value : @default;
        public TValue GetValueOrAdd(TKey key, TValue value) => GetValueOrAdd(key, () => value);
        public TValue GetValueOrAdd(TKey key, Func<TValue> valueFactory) => GetValueOrAdd(key, _ => valueFactory.Invoke());

        public TValue GetValueOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!TryGetValue(key, out var value))
            {
                value = valueFactory.Invoke(key);
                TryAdd(key, value);
            }

            return value;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

        public bool TryGetValue(Func<TKey, bool> keyExpression, [MaybeNullWhen(false)] out TValue value)
        {
            var key = Keys.FirstOrDefault(keyExpression);

            if (key is not null)
            {
                value = _dictionary[key];
                return true;
            }

            value = default!;
            return false;
        }

        bool IEquatable<IDictionary<TKey, TValue>>.Equals(IDictionary<TKey, TValue>? other) => Equals(other);

        protected bool Equals(IDictionary<TKey, TValue>? other)
            => other switch
            {
                null => false,
                _ => Count == other.Count &&
                     Keys.Any(left => other.Keys.Any(right => right.Equals(left))) &&
                     Values.Any(left => other.Values.Any(right => right?.Equals(left) == true))
            };

        public override bool Equals(object? obj) => Equals(obj as IDictionary<TKey, TValue>);

        public override int GetHashCode() => _dictionary.Aggregate(0, HashCode.Combine);

        bool IChangeTrackingObject<TKey, TValue>.BeginTracking() => _changeTrackingObjectAdapter.BeginTracking();
        IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> IChangeTrackingObject<TKey, TValue>.EndTracking() => _changeTrackingObjectAdapter.EndTracking();

        public static implicit operator Dictionary<TKey, TValue>(BindingDictionary<TKey, TValue> highPerformanceDictionary) => new(highPerformanceDictionary._dictionary);
        public static implicit operator ConcurrentDictionary<TKey, TValue>(BindingDictionary<TKey, TValue> highPerformanceDictionary) => new(highPerformanceDictionary._dictionary);
    }
}