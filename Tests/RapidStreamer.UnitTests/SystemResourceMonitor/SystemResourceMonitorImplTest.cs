using System.Globalization;
using JetBrains.Annotations;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using Xunit.Abstractions;

namespace RapidStreamer.UnitTests.SystemResourceMonitor;

[TestSubject(typeof(SystemResourceMonitorImpl))]
public class SystemResourceMonitorImplTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SystemResourceMonitorImplTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SystemResources_Must_Returns_CpuUsage_For_1_Second()
    {
        //Arrange
        CpuMetricsClient cpuMetricsClient = new();
        MemoryMetricsClient memoryMetricsClient = new();
        SystemDriveMetricsClient systemDriveMetricsClient = new();
        SystemResourceMonitorImpl systemResourceMonitorImpl = new(cpuMetricsClient, memoryMetricsClient, systemDriveMetricsClient);

        //Act
        var metrics = systemResourceMonitorImpl.GetMetrics(1000, true);

        //Assert
        Assert.NotNull(metrics);
        Assert.True(metrics.Cpu.Usage >= 0);
        _testOutputHelper.WriteLine(metrics.Cpu.Usage.ToString(CultureInfo.InvariantCulture));
    }
}