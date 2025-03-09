namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;

public record MemoryMetrics(double Total, double Free)
{
    public double Used => Total - Free;

    public double UsagePercentage
    {
        get
        {
            var usage = .0;
            if (Total > 0)
                usage = 100.0 - ((1.0 * Free / Total) * 100);

            return usage;
        }
    }
}