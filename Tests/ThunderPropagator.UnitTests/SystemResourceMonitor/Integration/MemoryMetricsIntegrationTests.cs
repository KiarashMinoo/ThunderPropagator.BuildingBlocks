using Microsoft.Extensions.Options;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.Helpers;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests that validate memory metrics under real load.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
public class MemoryMetricsIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricsSampler _sampler;
    private readonly MemoryLoadGenerator _loadGenerator;

    public MemoryMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 500,
            CollectAllProcesses = true
        });

        _monitor = new SystemResourceMonitorImpl(
            new CpuMetricsClient(),
            new CpuTemperatureMetricsClient(),
            new MemoryMetricsClient(),
            new SystemDriveMetricsClient(),
            new DiskHealthMetricsClient(),
            new DiskSpeedMetricsClient(),
            new GpuMetricsClient(),
            new BatteryMetricsClient(),
            options);

        _sampler = new MetricsSampler(_monitor);
        _loadGenerator = new MemoryLoadGenerator();
    }

    [Fact]
    public async Task MemoryUsage_Should_Increase_When_Allocating_Memory()
    {
        // Baseline
        _output.WriteLine("Collecting baseline memory metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var (baselineMin, baselineMax, baselineAvg) = baseline.MemoryUsed();
        var baselineMb = baselineAvg / (1024 * 1024);
        _output.WriteLine($"Baseline memory: {baselineMb:F2} MB");

        // Act - Allocate memory
        _output.WriteLine("Allocating 100 MB of memory...");
        _loadGenerator.AllocateAndRetain(100);
        await Task.Delay(1000); // Allow time for metrics to reflect

        var load = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var (loadMin, loadMax, loadAvg) = load.MemoryUsed();
        var loadMb = loadAvg / (1024 * 1024);
        _output.WriteLine($"Load memory: {loadMb:F2} MB");

        // Assert
        MetricsValidator.AssertMemoryUsageIncreased(baseline, load, minIncreaseMB: 50.0);
        Assert.True(loadAvg > baselineAvg, "Memory usage should increase");

        var increaseMb = loadMb - baselineMb;
        _output.WriteLine($"Memory increase: {increaseMb:F2} MB");
        _output.WriteLine("✓ Memory usage increased as expected");
    }

    [Fact]
    public async Task MemoryUsage_Should_Decrease_After_Release()
    {
        // Allocate
        _output.WriteLine("Allocating memory...");
        _loadGenerator.AllocateAndRetain(100);
        await Task.Delay(1000);

        var beforeRelease = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);
        var beforeMb = beforeRelease.MemoryUsed().Avg / (1024 * 1024);
        _output.WriteLine($"Memory before release: {beforeMb:F2} MB");

        // Act - Release
        _output.WriteLine("Releasing memory and forcing GC...");
        _loadGenerator.ReleaseAll();
        await Task.Delay(2000); // Allow GC to run

        var afterRelease = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);
        var afterMb = afterRelease.MemoryUsed().Avg / (1024 * 1024);
        _output.WriteLine($"Memory after release: {afterMb:F2} MB");

        // Assert - Memory should decrease (though not necessarily back to original due to GC behavior)
        Assert.True(afterMb < beforeMb, "Memory usage should decrease after release");

        var decreaseMb = beforeMb - afterMb;
        _output.WriteLine($"Memory decrease: {decreaseMb:F2} MB");
        _output.WriteLine("✓ Memory usage decreased after release");
    }

    [Fact]
    public async Task MemoryMetrics_Should_Report_Valid_Total_And_Free()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var metrics = sample.Samples.First().Memory;

        Assert.True(metrics.Total > 0, "Total memory should be positive");
        Assert.True(metrics.Free >= 0, "Free memory should be non-negative");
        Assert.True(metrics.Free <= metrics.Total, "Free memory should not exceed total");
        Assert.True(metrics.Used >= 0, "Used memory should be non-negative");
        Assert.True(metrics.Used <= metrics.Total, "Used memory should not exceed total");
        Assert.Equal(metrics.Total - metrics.Free, metrics.Used);

        var totalGb = metrics.Total / (1024 * 1024 * 1024);
        var usedGb = metrics.Used / (1024 * 1024 * 1024);
        var freeGb = metrics.Free / (1024 * 1024 * 1024);

        _output.WriteLine($"Total memory: {totalGb:F2} GB");
        _output.WriteLine($"Used memory: {usedGb:F2} GB ({metrics.UsagePercentage:F2}%)");
        _output.WriteLine($"Free memory: {freeGb:F2} GB");
        _output.WriteLine("✓ Memory metrics are valid and consistent");
    }

    [Fact]
    public async Task MemoryUsagePercentage_Should_Be_Within_Valid_Range()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var (min, max, avg) = sample.MemoryUsagePercent();

        // Assert
        MetricsValidator.AssertInRange(min, 0, 100, "Memory Usage % Min");
        MetricsValidator.AssertInRange(max, 0, 100, "Memory Usage % Max");
        MetricsValidator.AssertInRange(avg, 0, 100, "Memory Usage % Avg");

        _output.WriteLine($"Memory usage %: Min={min:F2}%, Max={max:F2}%, Avg={avg:F2}%");
        _output.WriteLine("✓ Memory usage percentage is within valid range [0-100%]");
    }

    [Fact]
    public async Task MemoryChurn_Should_Generate_GC_Activity()
    {
        // Baseline
        _output.WriteLine("Collecting baseline...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);
        var baselineMb = baseline.MemoryUsed().Avg / (1024 * 1024);
        _output.WriteLine($"Baseline memory: {baselineMb:F2} MB");

        // Act - Start memory churn
        _output.WriteLine("Starting memory churn...");
        _loadGenerator.StartChurn(churnSizePerIterationMb: 20, intervalMs: 100);
        await Task.Delay(3000); // Let churn run

        var churn = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var churnMb = churn.MemoryUsed().Avg / (1024 * 1024);
        _output.WriteLine($"Churn memory: {churnMb:F2} MB");

        await _loadGenerator.StopChurnAsync();

        // Assert - Memory should show activity (allocation/deallocation)
        // During churn, we expect memory to be elevated compared to baseline
        Assert.True(churnMb >= baselineMb, "Memory during churn should be at least baseline");

        _output.WriteLine("✓ Memory churn completed successfully");
    }

    [Fact]
    public async Task MemoryMetrics_Should_Update_Consistently_Over_Time()
    {
        // Act - Collect multiple samples
        var samples = new List<double>();
        for (var i = 0; i < 5; i++)
        {
            var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 500);
            samples.Add(sample.MemoryUsed().Avg);
            _output.WriteLine($"Sample {i + 1}/5: {sample.MemoryUsed().Avg / (1024 * 1024):F2} MB");
        }

        // Assert - All samples should be valid and non-zero
        Assert.All(samples, s => Assert.True(s > 0, "Memory should always be positive"));
        
        _output.WriteLine("✓ Memory metrics update consistently");
    }

    public void Dispose()
    {
        _loadGenerator?.Dispose();
    }
}

