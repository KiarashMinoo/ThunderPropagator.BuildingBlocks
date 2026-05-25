using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor;

[TestSubject(typeof(SystemResourceMonitorImpl))]
public class SystemResourceMonitorImplTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SystemResourceMonitorImplTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SystemResources_Must_Return_CpuUsage_For_1_Second_And_NotThrow_For_New_Metrics()
    {
        // Arrange
        var cpuMetricsClient = new CpuMetricsClient();
        var cpuTemperatureMetricsClient = new CpuTemperatureMetricsClient();
        var memoryMetricsClient = new MemoryMetricsClient();
        var systemDriveMetricsClient = new SystemDriveMetricsClient();
        var diskHealthMetricsClient = new DiskHealthMetricsClient();
        var diskSpeedMetricsClient = new DiskSpeedMetricsClient();
        var gpuMetricsClient = new GpuMetricsClient();
        var batteryMetricsClient = new BatteryMetricsClient();

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 1000,
            CollectAllProcesses = true,
            // keep defaults enabled; providers must still behave gracefully on unsupported platforms
        });

        var systemResourceMonitorImpl = new SystemResourceMonitorImpl(
            cpuMetricsClient,
            cpuTemperatureMetricsClient,
            memoryMetricsClient,
            systemDriveMetricsClient,
            diskHealthMetricsClient,
            diskSpeedMetricsClient,
            gpuMetricsClient,
            batteryMetricsClient,
            options);

        // Act
        var metrics = await systemResourceMonitorImpl.GetMetricsAsync(window: 1000, all: true);

        // Assert
        Assert.NotNull(metrics);
        Assert.NotNull(metrics.Cpu);
        Assert.True(metrics.Cpu.Usage >= 0);

        // New metric groups should always be safe to read
        Assert.NotNull(metrics.Drives);
        Assert.NotNull(metrics.DiskHealth);
        Assert.NotNull(metrics.DiskSpeed);
        Assert.NotNull(metrics.Gpus);
        // Battery is optional (only present if battery exists)
        // CpuTemperature is optional (platform-dependent)

        _testOutputHelper.WriteLine(metrics.Cpu.Usage.ToString(CultureInfo.InvariantCulture));

        if (metrics.CpuTemperature != null)
            _testOutputHelper.WriteLine($"CPU temp sensors available: {metrics.CpuTemperature.TemperatureSensorsAvailable}");

        if (metrics.Battery != null)
            _testOutputHelper.WriteLine($"Battery: {metrics.Battery.ChargePercent}% ({metrics.Battery.Status})");
    }

    [Fact]
    public async Task Options_Disabling_Metric_Groups_Should_Return_Empty_Or_Null_As_Expected()
    {
        // Arrange
        var cpuMetricsClient = new CpuMetricsClient();
        var cpuTemperatureMetricsClient = new CpuTemperatureMetricsClient();
        var memoryMetricsClient = new MemoryMetricsClient();
        var systemDriveMetricsClient = new SystemDriveMetricsClient();
        var diskHealthMetricsClient = new DiskHealthMetricsClient();
        var diskSpeedMetricsClient = new DiskSpeedMetricsClient();
        var gpuMetricsClient = new GpuMetricsClient();
        var batteryMetricsClient = new BatteryMetricsClient();

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            EnableCpuTemperature = false,
            EnableDiskHealthMetrics = false,
            EnableDiskSpeedMetrics = false,
            EnableGpuMetrics = false,
            EnableBatteryMetrics = false,
        });

        var sut = new SystemResourceMonitorImpl(
            cpuMetricsClient,
            cpuTemperatureMetricsClient,
            memoryMetricsClient,
            systemDriveMetricsClient,
            diskHealthMetricsClient,
            diskSpeedMetricsClient,
            gpuMetricsClient,
            batteryMetricsClient,
            options);

        // Act
        var metrics = await sut.GetMetricsAsync(window: 10, all: false);

        // Assert
        Assert.NotNull(metrics);
        Assert.NotNull(metrics.Cpu);
        Assert.NotNull(metrics.Memory);

        Assert.Null(metrics.CpuTemperature);
        Assert.Empty(metrics.DiskHealth);
        Assert.Empty(metrics.DiskSpeed);
        Assert.Empty(metrics.Gpus);
        Assert.Null(metrics.Battery);
    }

    [Fact]
    public async Task CpuTemperature_Windows_Should_Attempt_To_Read_Temperature()
    {
        // Arrange
        var client = new CpuTemperatureMetricsClient();

        // Act
        var metrics = await client.GetMetricsAsync();

        // Assert
        Assert.NotNull(metrics);

        // On Windows, thermal sensors may or may not be available
        // The implementation should handle both cases gracefully
        if (metrics.TemperatureSensorsAvailable)
        {
            Assert.NotNull(metrics.CoreTemperatures);
            Assert.True(metrics.MaxTemperatureCelsius >= 0);
            Assert.True(metrics.AverageTemperatureCelsius >= 0);
            Assert.True(metrics.PackageTemperatureCelsius >= 0);
        }
        else
        {
            Assert.False(string.IsNullOrEmpty(metrics.ErrorMessage));
            _testOutputHelper.WriteLine($"CPU Temperature not available: {metrics.ErrorMessage}");
        }
    }
}