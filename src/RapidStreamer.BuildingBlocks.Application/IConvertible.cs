namespace RapidStreamer.BuildingBlocks.Application
{
    public interface IConvertible<out T>
    {
        T Convert();
    }
}