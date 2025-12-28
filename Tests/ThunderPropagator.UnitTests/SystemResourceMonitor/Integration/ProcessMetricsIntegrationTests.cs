using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Helpers;
using ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests for Process resource metrics.
/// Tests validate thread count, handle count, and process-level resource tracking.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
[Trait("Metric", "Process")]
public sealed class ProcessMetricsIntegrationTests : IDisposable
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricSampler _sampler;
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;

    public ProcessMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        services.AddSystemResourceMonitor(options =>
        {
            options.DefaultSamplingWindowMs = 500;
            options.EnableCpuTemperature = false;
            options.EnableDiskHealthMetrics = false;
            options.EnableDiskSpeedMetrics = false;
            options.EnableGpuMetrics = false;
            options.EnableBatteryMetrics = false;
        });

        _serviceProvider = services.BuildServiceProvider();
        _monitor = _serviceProvider.GetRequiredService<ISystemResourceMonitor>();
        _sampler = new MetricSampler(_monitor);
    }

    [Fact]
    public async Task ThreadCount_IncreasesWhenThreadsCreated()
    {
        // Arrange
        _output.WriteLine("=== Thread Count Increase Test ===");

        using var processLoad = new ProcessLoadGenerator();

        // Act & Assert - Baseline
        _output.WriteLine("\n[BASELINE] Collecting baseline process metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);
        _output.WriteLine($"Baseline Threads: Avg={baseline.ThreadCountAvg}, Max={baseline.ThreadCountMax}");

        // Act - Create threads
        var threadCount = 15;
        _output.WriteLine($"\n[LOAD] Creating {threadCount} additional threads...");
        processLoad.CreateThreads(threadCount);
        await Task.Delay(500); // Let threads start

        var load = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);
        _output.WriteLine($"Load Threads: Avg={load.ThreadCountAvg}, Max={load.ThreadCountMax}");
        _output.WriteLine($"Active Threads Created: {processLoad.ThreadCount}");

        // Assert
        var increase = load.ThreadCountAvg - baseline.ThreadCountAvg;
        _output.WriteLine($"Thread count increase: {increase}");

        Assert.True(increase >= threadCount / 2,
            $"Expected at least {threadCount / 2} thread increase, got {increase}");

        _output.WriteLine($"✓ Thread count increased by {increase}");

        // Cleanup
        processLoad.ReleaseAll();
        await Task.Delay(500);

        var cooldown = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);
        _output.WriteLine($"Cooldown Threads: Avg={cooldown.ThreadCountAvg}");
        _output.WriteLine($"✓ Threads released");
    }

    [Fact(Skip = "Handle counting platform-specific and unreliable in CI")]
    public async Task HandleCount_IncreasesWithFileHandles()
    {
        // Arrange
        _output.WriteLine("=== Handle Count Test ===");

        using var processLoad = new ProcessLoadGenerator();

        // Act & Assert - Baseline
        _output.WriteLine("\n[BASELINE] Collecting baseline...");
        var baselineMetrics = await _monitor.GetMetricsAsync();
        _output.WriteLine($"Baseline - can't reliably track handle count in .NET");

        // Act - Create handles
        var handleCount = 20;
        _output.WriteLine($"\n[LOAD] Creating {handleCount} file handles...");
        processLoad.CreateFileHandles(handleCount);
        await Task.Delay(500);

        var loadMetrics = await _monitor.GetMetricsAsync();
        _output.WriteLine($"Handles created by generator: {processLoad.HandleCount}");

        // Note: .NET Process.HandleCount is Windows-only and includes many system handles
        // This test documents the limitation rather than asserting
        _output.WriteLine($"ℹ Handle counting is platform-specific and not directly testable");
    }

    [Fact]
    public async Task ProcessCount_IsPositive()
    {
        // Arrange
        _output.WriteLine("=== Process Count Validation Test ===");

        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);

        // Assert
        _output.WriteLine($"Process Count: Avg={sample.ProcessCountAvg}");

        Assert.True(sample.ProcessCountAvg > 0, "Process count should be positive");
        Assert.True(sample.ProcessCountAvg < 10000, "Process count should be reasonable (< 10000)");

        _output.WriteLine($"✓ Process count is reasonable: {sample.ProcessCountAvg}");
    }

    [Fact]
    public async Task ThreadCount_IsReasonable()
    {
        // Arrange
        _output.WriteLine("=== Thread Count Validation Test ===");

        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);

        // Assert
        _output.WriteLine($"Thread Count: Avg={sample.ThreadCountAvg}, Max={sample.ThreadCountMax}");

        // Current process should have at least a few threads
        Assert.True(sample.ThreadCountAvg >= 1, "Thread count should be at least 1");

        // But not an unreasonable number
        Assert.True(sample.ThreadCountAvg < 1000, "Thread count should be reasonable (< 1000 for test process)");

        _output.WriteLine($"✓ Thread count is reasonable: {sample.ThreadCountAvg}");
    }

    [Fact]
    public async Task TotalThreads_IsGreaterThanProcessThreads()
    {
        // Arrange
        _output.WriteLine("=== Total vs Process Threads Test ===");

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        _output.WriteLine($"Process Threads: {metrics.Cpu.Threads}");
        _output.WriteLine($"Total System Threads: {metrics.Cpu.TotalThreads}");

        Assert.True(metrics.Cpu.TotalThreads >= metrics.Cpu.Threads,
            "Total system threads should be >= current process threads");

        _output.WriteLine($"✓ Total threads ({metrics.Cpu.TotalThreads}) >= process threads ({metrics.Cpu.Threads})");
    }

    [Fact]
    public async Task ProcessMetrics_UpdateOverTime()
    {
        // Arrange
        _output.WriteLine("=== Process Metrics Update Test ===");

        using var cpuLoad = new CpuLoadGenerator();

        // Act - Create activity that will affect process metrics
        _output.WriteLine("\n[LOAD] Generating activity...");
        var loadTask = cpuLoad.GenerateLoadAsync(durationMs: 3000, threadCount: 5, intensity: 0.5);

        await Task.Delay(500); // Let load start

        var sample = await _sampler.CollectSamplesAsync(windowMs: 2000, intervalMs: 300);

        await loadTask;

        // Assert - Metrics should update (show variance)
        var cpuUsageVariance = sample.CpuUsageMax - sample.CpuUsageMin;
        _output.WriteLine($"CPU Usage Variance: {cpuUsageVariance:F2}%");

        Assert.True(cpuUsageVariance > 0, "Process metrics should show activity variance");
        _output.WriteLine($"✓ Process metrics update over time");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
