namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public interface ICorrelationIdSupport
    {
        string CorrelationId { get; protected internal set; }
    }
}