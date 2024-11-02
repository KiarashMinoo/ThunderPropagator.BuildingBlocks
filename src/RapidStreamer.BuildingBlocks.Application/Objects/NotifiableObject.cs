namespace RapidStreamer.BuildingBlocks.Application.Objects
{
    public abstract class NotifiableObject
    {
        public enum NotifiableChangeType
        {
            Added = 0,
            Modified,
            Removed
        }
    }
}