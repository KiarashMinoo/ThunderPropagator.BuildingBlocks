namespace ThunderPropagator.BuildingBlocks.Application
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}