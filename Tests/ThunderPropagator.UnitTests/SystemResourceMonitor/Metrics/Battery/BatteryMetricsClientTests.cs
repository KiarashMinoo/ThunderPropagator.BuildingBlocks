using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Metrics.Battery;

public class BatteryMetricsClientTests
{
    private readonly IBatteryMetricsProvider _mockProvider;
    private readonly BatteryMetricsClient _client;

    public BatteryMetricsClientTests()
    {
        _mockProvider = Substitute.For<IBatteryMetricsProvider>();
        _client = new BatteryMetricsClient(_mockProvider);
    }

    [Fact]
    public async Task GetMetricsAsync_Returns_Metrics_From_Provider()
    {
        // Arrange
        var expectedMetrics = new BatteryMetrics
        {
            BatteryPresent = true,
            ChargePercent = 80,
            Status = BatteryStatus.Discharging,
            OnACPower = false
        };

        _mockProvider
            .GetBatteryMetricsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedMetrics);

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedMetrics, result);
        Assert.True(result.BatteryPresent);
        Assert.Equal(80, result.ChargePercent);
    }

    [Fact]
    public async Task GetMetricsAsync_Handles_Provider_Exception()
    {
        // Arrange
        _mockProvider
            .GetBatteryMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Provider error"));

        // Act
        var result = await _client.GetMetricsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.BatteryPresent);
        Assert.Contains("Provider error", result.ErrorMessage);
    }
}
