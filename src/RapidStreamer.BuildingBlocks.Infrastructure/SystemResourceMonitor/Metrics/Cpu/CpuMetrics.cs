namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

public record CpuMetrics(long ProcessorCount, double Usage, long Threads, long Processes, long TotalThreads);