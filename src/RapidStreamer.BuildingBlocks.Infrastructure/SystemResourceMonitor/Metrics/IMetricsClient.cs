namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics;

public interface IMetricsClient<TMetrics>
    where TMetrics : class
{
    Task<TMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}