namespace RapidStreamer.BuildingBlocks.Application
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}