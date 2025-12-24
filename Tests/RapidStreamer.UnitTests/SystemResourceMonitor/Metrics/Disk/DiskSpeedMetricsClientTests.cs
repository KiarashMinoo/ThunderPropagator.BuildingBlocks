using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

namespace RapidStreamer.UnitTests.SystemResourceMonitor.Metrics.Disk;

public class DiskSpeedMetricsClientTests
{
    private readonly IDiskSpeedProvider _mockProvider;
    private readonly DiskSpeedMetricsClient _client;

    public DiskSpeedMetricsClientTests()
    {
        _mockProvider = Substitute.For<IDiskSpeedProvider>();
        _client = new DiskSpeedMetricsClient(_mockProvider);
    }

    [Fact]
    public async Task GetMetricsAsync_Returns_Metrics_From_Provider()
    {
        // Arrange
        var expectedMetrics = new[]
        {
            new DiskSpeedMetrics
            {
                DriveId = "C:",
                ReadThroughputMBps = 1024,
                PerformanceCountersAvailable = true
            }
        };
        _mockProvider.GetDiskSpeedMetricsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedMetrics, result);
        Assert.Single(result);
        Assert.Equal("C:", result[0].DriveId);
    }

    [Fact]
    public async Task GetMetricsAsync_Handles_Provider_Exception()
    {
        // Arrange
        _mockProvider
            .GetDiskSpeedMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}