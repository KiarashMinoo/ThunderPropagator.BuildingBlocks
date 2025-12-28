namespace ThunderPropagator.BuildingBlocks.Application.ChangeTrackingItems
{
    internal
#if !DEBUG
        sealed
#endif
        class ChangeTrackingObjectAdapter<TKey, TValue>
        where TKey : notnull
    {
        private readonly ChangeTrackingItemCollection<TKey, TValue> _changeTrackingItemCollection = new();

        public bool Enabled { get; private set; }

        public bool BeginTracking()
        {
            Enabled = true;
            _changeTrackingItemCollection.Clear();
            return true;
        }

        public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> EndTracking()
        {
            Enabled = false;
            return _changeTrackingItemCollection;
        }

        public void Clear() => _changeTrackingItemCollection.Clear();

        public bool Report(TKey key, ChangeType changeType, TValue? previousValue, TValue? newValue, bool forceToUpdate = false)
            => Enabled ? _changeTrackingItemCollection.Add(key, new ChangeTrackingItem<TValue>(changeType, previousValue, newValue), forceToUpdate) : Enabled;

        public bool ReportAdded(TKey key, TValue? newValue, bool forceToUpdate = false) => Report(key, ChangeType.Added, default, newValue, forceToUpdate);

        public bool ReportModified(TKey key, TValue? previousValue, TValue? newValue, bool forceToUpdate = false)
            => Report(key, ChangeType.Modified, previousValue, newValue, forceToUpdate);

        public bool ReportRemoved(TKey key, TValue? previousValue, bool forceToUpdate = false) => Report(key, ChangeType.Removed, previousValue, default, forceToUpdate);
    }
}