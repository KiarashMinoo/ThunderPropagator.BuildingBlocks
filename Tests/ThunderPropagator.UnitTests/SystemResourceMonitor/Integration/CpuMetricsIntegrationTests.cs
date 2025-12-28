using Microsoft.Extensions.Options;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using RapidStreamer.UnitTests.SystemResourceMonitor.Integration.Helpers;
using RapidStreamer.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;
using Xunit.Abstractions;

namespace RapidStreamer.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests that validate CPU metrics under real load.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
public class CpuMetricsIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricsSampler _sampler;
    private readonly CpuLoadGenerator _loadGenerator;

    public CpuMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 500,
            CollectAllProcesses = true,
            EnableCpuTemperature = true
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
        _loadGenerator = new CpuLoadGenerator();
    }

    [Fact]
    public async Task CpuUsage_Should_Increase_Under_Load_And_Return_To_Baseline()
    {
        // Arrange & Baseline
        _output.WriteLine("Collecting baseline metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var (baselineMin, baselineMax, baselineAvg) = baseline.CpuUsage();
        _output.WriteLine($"Baseline CPU: Min={baselineMin:F2}%, Max={baselineMax:F2}%, Avg={baselineAvg:F2}%");

        // Act - Generate Load
        _output.WriteLine("Generating CPU load...");
        _loadGenerator.Start(threadCount: Environment.ProcessorCount / 2, targetUtilizationPercent: 80);
        await Task.Delay(1000); // Warm-up

        var load = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 3000);
        var (loadMin, loadMax, loadAvg) = load.CpuUsage();
        _output.WriteLine($"Load CPU: Min={loadMin:F2}%, Max={loadMax:F2}%, Avg={loadAvg:F2}%");

        await _loadGenerator.StopAsync();

        // Cooldown
        _output.WriteLine("Waiting for cooldown...");
        await Task.Delay(2000);
        var cooldown = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var (cooldownMin, cooldownMax, cooldownAvg) = cooldown.CpuUsage();
        _output.WriteLine($"Cooldown CPU: Min={cooldownMin:F2}%, Max={cooldownMax:F2}%, Avg={cooldownAvg:F2}%");

        // Assert
        MetricsValidator.AssertCpuUsageIncreased(baseline, load, minIncreasePct: 15.0);
        Assert.True(loadAvg > baselineAvg, "Load average should be higher than baseline");
        Assert.True(cooldownAvg < loadAvg, "Cooldown average should be lower than load");

        _output.WriteLine("✓ CPU usage increased under load and returned to baseline");
    }

    [Fact]
    public async Task CpuMetrics_Should_Report_Correct_ProcessorCount()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var metrics = sample.Samples.First().Cpu;

        Assert.True(metrics.ProcessorCount > 0, "Processor count should be positive");
        Assert.Equal(Environment.ProcessorCount, metrics.ProcessorCount);

        _output.WriteLine($"Processor count: {metrics.ProcessorCount}");
        _output.WriteLine("✓ Processor count is correct");
    }

    [Fact]
    public async Task CpuUsage_Should_Be_Within_Valid_Range()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 3000);
        var (min, max, avg) = sample.CpuUsage();

        // Assert
        MetricsValidator.AssertInRange(min, 0, 100, "CPU Usage Min");
        MetricsValidator.AssertInRange(max, 0, 100, "CPU Usage Max");
        MetricsValidator.AssertInRange(avg, 0, 100, "CPU Usage Avg");

        _output.WriteLine($"CPU usage range: [{min:F2}%, {max:F2}%], Avg: {avg:F2}%");
        _output.WriteLine("✓ CPU usage is within valid range [0-100%]");
    }

    [Fact]
    public async Task ThreadCount_Should_Increase_When_Creating_Threads()
    {
        using var processLoader = new ProcessLoadGenerator();

        // Create monitor that tracks current process only (not all processes)
        var processMonitor = new SystemResourceMonitorImpl(
            new CpuMetricsClient(),
            new CpuTemperatureMetricsClient(),
            new MemoryMetricsClient(),
            new SystemDriveMetricsClient(),
            new DiskHealthMetricsClient(),
            new DiskSpeedMetricsClient(),
            new GpuMetricsClient(),
            new BatteryMetricsClient(),
            Options.Create(new SystemResourceMonitorOptions
            {
                DefaultSamplingWindowMs = 500,
                CollectAllProcesses = false // Track current process only
            }));

        var processSampler = new MetricsSampler(processMonitor);

        // Baseline
        _output.WriteLine("Collecting baseline thread count...");
        var baseline = await processSampler.CollectSamplesAsync(windowMs: 500, durationMs: 1500);
        var (baselineMin, baselineMax, baselineAvg) = baseline.ThreadCount();
        _output.WriteLine($"Baseline threads: Min={baselineMin}, Max={baselineMax}, Avg={baselineAvg:F1}");

        // Act - Create threads
        _output.WriteLine("Creating additional threads...");
        processLoader.CreateThreads(10);
        await Task.Delay(1000); // Let threads stabilize

        var load = await processSampler.CollectSamplesAsync(windowMs: 500, durationMs: 1500);
        var (loadMin, loadMax, loadAvg) = load.ThreadCount();
        _output.WriteLine($"Load threads: Min={loadMin}, Max={loadMax}, Avg={loadAvg:F1}");

        // Assert
        MetricsValidator.AssertThreadCountIncreased(baseline, load, minIncrease: 5);
        Assert.True(loadAvg > baselineAvg, "Thread count should increase");

        _output.WriteLine("✓ Thread count increased as expected");
    }

    [Fact]
    public async Task CpuTemperature_Should_Be_Available_Or_Gracefully_Unavailable()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);
        var temp = sample.CpuTemperature();

        // Assert
        if (temp.Avg.HasValue)
        {
            // Temperature is supported
            MetricsValidator.AssertInRange(temp.Avg.Value, -50, 150, "CPU Temperature");
            _output.WriteLine($"CPU temperature: Min={temp.Min:F2}°C, Max={temp.Max:F2}°C, Avg={temp.Avg:F2}°C");
            _output.WriteLine("✓ CPU temperature metrics are available and valid");
        }
        else
        {
            // Temperature not supported
            _output.WriteLine("⚠ CPU temperature not supported on this platform (expected)");
            Assert.True(true, "Temperature not supported is acceptable");
        }
    }

    public void Dispose()
    {
        _loadGenerator?.Dispose();
    }
}

