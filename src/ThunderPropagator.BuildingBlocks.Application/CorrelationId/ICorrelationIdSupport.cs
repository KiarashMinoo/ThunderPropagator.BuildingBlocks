namespace ThunderPropagator.BuildingBlocks.Application.CorrelationId
{
    public interface ICorrelationIdSupport
    {
        string CorrelationId { get; protected internal set; }
    }
}