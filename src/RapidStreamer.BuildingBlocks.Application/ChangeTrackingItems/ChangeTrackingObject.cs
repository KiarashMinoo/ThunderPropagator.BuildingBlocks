namespace RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems
{
    public interface IChangeTrackingObject<TKey, TValue>
        where TKey : notnull
    {
        bool BeginTracking();
        IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> EndTracking();
    }
}