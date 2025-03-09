namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

public class SystemDriveMetricsClient
{
    public SystemDriveMetrics[] GetMetrics() => DriveInfo.GetDrives()
        .Where(drive => drive.IsReady)
        .Select(drive => new SystemDriveMetrics(drive.Name, drive.TotalSize, drive.TotalFreeSpace, drive.IsReady))
        .ToArray();
}