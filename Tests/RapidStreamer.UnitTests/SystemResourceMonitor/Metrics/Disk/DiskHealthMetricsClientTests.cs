using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

namespace RapidStreamer.UnitTests.SystemResourceMonitor.Metrics.Disk;

public class DiskHealthMetricsClientTests
{
    private readonly IDiskHealthProvider _mockProvider;
    private readonly DiskHealthMetricsClient _client;

    public DiskHealthMetricsClientTests()
    {
        _mockProvider = Substitute.For<IDiskHealthProvider>();
        _client = new DiskHealthMetricsClient(_mockProvider);
    }

    [Fact]
    public async Task GetMetricsAsync_Returns_Metrics_From_Provider()
    {
        // Arrange
        var expectedMetrics = new[]
        {
            new DiskHealthMetrics
            {
                DriveId = "C:",
                Status = DiskHealthStatus.Healthy
            }
        };
        _mockProvider.GetDiskHealthMetricsAsync(Arg.Any<CancellationToken>())
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
            .GetDiskHealthMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}