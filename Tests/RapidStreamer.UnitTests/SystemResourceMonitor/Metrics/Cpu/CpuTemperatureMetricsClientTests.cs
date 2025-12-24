using System.Runtime.InteropServices;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

namespace RapidStreamer.UnitTests.SystemResourceMonitor.Metrics.Cpu;

public class CpuTemperatureMetricsClientTests
{
    private readonly ICpuTemperatureProvider _mockProvider;
    private readonly CpuTemperatureMetricsClient _client;

    public CpuTemperatureMetricsClientTests()
    {
        _mockProvider = Substitute.For<ICpuTemperatureProvider>();
        _client = new CpuTemperatureMetricsClient(_mockProvider);
    }

    [Fact]
    public async Task GetMetricsAsync_Returns_Metrics_From_Provider()
    {
        // Arrange
        var expectedMetrics = new CpuTemperatureMetrics { TemperatureSensorsAvailable = true, PackageTemperatureCelsius = 60.5 };
        _mockProvider.GetCpuTemperatureMetricsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedMetrics, result);
        Assert.True(result.TemperatureSensorsAvailable);
        Assert.Equal(60.5, result.PackageTemperatureCelsius);
    }

    [Fact]
    public async Task GetMetricsAsync_Handles_Provider_Exception()
    {
        // Arrange
        _mockProvider.GetCpuTemperatureMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.TemperatureSensorsAvailable);
        Assert.Contains("Provider error", result.ErrorMessage);
    }

    [Fact]
    public void CreatePlatformProvider_Returns_Correct_Provider_For_Windows()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var provider = CpuTemperatureMetricsClient.CreatePlatformProvider();
            Assert.IsType<WindowsCpuTemperatureProvider>(provider);
        }
    }

    [Fact]
    public void CreatePlatformProvider_Returns_Correct_Provider_For_Linux()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var provider = CpuTemperatureMetricsClient.CreatePlatformProvider();
            Assert.IsType<LinuxCpuTemperatureProvider>(provider);
        }
    }

    [Fact]
    public void CreatePlatformProvider_Returns_Correct_Provider_For_MacOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var provider = CpuTemperatureMetricsClient.CreatePlatformProvider();
            Assert.IsType<MacOsCpuTemperatureProvider>(provider);
        }
    }
}
