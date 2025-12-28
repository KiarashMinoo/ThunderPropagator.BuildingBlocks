using JetBrains.Annotations;
using System.Collections;
using System.Collections.Concurrent;

namespace ThunderPropagator.BuildingBlocks.Application.ChangeTrackingItems
{
    public
#if !DEBUG
        sealed
#endif
        class ChangeTrackingItemCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, ChangeTrackingItem<TValue>> _changeTrackingItems;

        internal ChangeTrackingItemCollection() => _changeTrackingItems = [];

        internal void Clear() => _changeTrackingItems.Clear();

        internal bool Add(TKey key, ChangeTrackingItem<TValue> value, bool forceToUpdate = false)
            => !_changeTrackingItems.TryGetValue(key, out var changeTrackingItem)
                ? CollectionExtensions.TryAdd(_changeTrackingItems, key, value)
                : forceToUpdate switch
                {
                    false => false,
                    _ => _changeTrackingItems.TryUpdate(key, value, changeTrackingItem)
                };

        public Dictionary<TKey, TValue> ToDictionary(ChangeType? changeType = null)
            => (changeType switch
            {
                not null => _changeTrackingItems.Where(item => item.Value.ChangeType == changeType),
                _ => _changeTrackingItems
            }).ToDictionary(item => item.Key, item => item.Value.NewValue!);

        public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetItems(ChangeType changeType)
            => _changeTrackingItems.Where(item => item.Value.ChangeType == changeType);

        public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetAddedItems() => GetItems(ChangeType.Added);
        public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetModifiedItems() => GetItems(ChangeType.Modified);
        public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetRemovedItems() => GetItems(ChangeType.Removed);

        [MustDisposeResource]
        public IEnumerator<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetEnumerator() => _changeTrackingItems.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}