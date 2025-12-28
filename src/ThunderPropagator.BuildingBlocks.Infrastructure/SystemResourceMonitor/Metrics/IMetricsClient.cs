namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics;

public interface IMetricsClient<TMetrics>
    where TMetrics : class
{
    public Task<TMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}