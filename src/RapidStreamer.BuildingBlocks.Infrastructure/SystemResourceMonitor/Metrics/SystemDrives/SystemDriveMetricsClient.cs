namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

public sealed class SystemDriveMetricsClient : IMetricsClient<SystemDriveMetrics[]>
{
    public Task<SystemDriveMetrics[]> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var drives = DriveInfo.GetDrives()
            .Where(drive => drive.IsReady)
            .Select(drive => new SystemDriveMetrics(drive.Name, drive.TotalSize, drive.TotalFreeSpace, drive.IsReady))
            .ToArray();

        return Task.FromResult(drives);
    }
}