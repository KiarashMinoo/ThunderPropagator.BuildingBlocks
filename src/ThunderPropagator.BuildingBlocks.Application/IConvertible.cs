namespace ThunderPropagator.BuildingBlocks.Application
{
    public interface IConvertible<out T>
    {
        T Convert();
    }
}