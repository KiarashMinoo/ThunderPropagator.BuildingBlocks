using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Metrics.Gpu;

public class GpuMetricsClientTests
{
    private readonly IGpuMetricsProvider _mockProvider;
    private readonly GpuMetricsClient _client;

    public GpuMetricsClientTests()
    {
        _mockProvider = Substitute.For<IGpuMetricsProvider>();
        _client = new GpuMetricsClient(_mockProvider);
    }

    [Fact]
    public async Task GetMetricsAsync_Returns_Metrics_From_Provider()
    {
        // Arrange
        var expectedMetrics = new[] 
        { 
            new GpuMetrics 
            { 
                GpuIndex = 0,
                GpuName = "NVIDIA Test GPU", 
                IsAvailable = true,
                ActiveProcesses = new List<GpuProcessInfo>()
            } 
        };
        _mockProvider
            .GetGpuMetricsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedMetrics, result);
        Assert.Single(result);
        Assert.Equal(0, result[0].GpuIndex);
        Assert.NotNull(result[0].ActiveProcesses);
    }

    [Fact]
    public async Task GetMetricsAsync_Handles_Provider_Exception()
    {
        // Arrange
        _mockProvider
            .GetGpuMetricsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}