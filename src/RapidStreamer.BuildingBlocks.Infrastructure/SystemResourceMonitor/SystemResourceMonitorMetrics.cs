using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

public record SystemResourceMonitorMetrics(CpuMetrics Cpu, MemoryMetrics Memory, SystemDriveMetrics[] Drives);