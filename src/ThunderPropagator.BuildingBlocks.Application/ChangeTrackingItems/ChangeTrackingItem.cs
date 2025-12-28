namespace ThunderPropagator.BuildingBlocks.Application.ChangeTrackingItems
{
    public
#if !DEBUG
        sealed
#endif
        class ChangeTrackingItem<TValue>
    {
        public ChangeType ChangeType { get; }
        public TValue? PreviousValue { get; }
        public TValue? NewValue { get; }

        internal ChangeTrackingItem(ChangeType changeType, TValue? previousValue, TValue? newValue)
        {
            ChangeType = changeType;
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}