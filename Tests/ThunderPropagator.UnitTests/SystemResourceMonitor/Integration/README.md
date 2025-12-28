# Resource Monitor Integration Tests - Documentation

## Overview

This test suite provides comprehensive validation of all Resource Monitor metrics through real load scenarios. Tests generate actual system load (CPU, memory, disk, network, process activity) and verify that metrics respond correctly.

## Architecture

### Components

**Load Generators** (`LoadGenerators/`)
- `CpuLoadGenerator` - Generates CPU load with configurable intensity and thread count
- `MemoryLoadGenerator` - Allocates memory with retention or churn patterns
- `DiskIoGenerator` - Creates real disk I/O (read, write, mixed workloads)
- `NetworkIoGenerator` - Generates loopback network traffic
- `ProcessLoadGenerator` - Creates threads and handles

**Validation Helpers** (`Helpers/`)
- `MetricSampler` - Collects time-windowed samples with aggregations
- `MetricValidator` - Validates metric changes with platform-specific tolerances

**Test Suites** (`Integration/`)
- `CpuMetricsIntegrationTests` - CPU usage, thread count, processor count
- `MemoryMetricsIntegrationTests` - Memory allocation, churn, GC behavior
- `SystemDriveMetricsIntegrationTests` - Drive space, file writes
- `ProcessMetricsIntegrationTests` - Thread count, handle count, process metrics

## Test Pattern

All tests follow a **Baseline → Load → Cooldown** pattern:

```csharp
// 1. Baseline - Collect metrics under idle conditions
var baseline = await sampler.CollectSamplesAsync(windowMs: 2000);

// 2. Load - Generate load and collect metrics
using var generator = new CpuLoadGenerator();
await generator.GenerateLoadAsync(durationMs: 5000);
var load = await sampler.CollectSamplesAsync(windowMs: 3000);

// 3. Validate - Assert metrics changed as expected
MetricValidator.AssertCpuUsageIncreased(baseline, load);

// 4. Cooldown - Verify return to baseline
await Task.Delay(1000);
var cooldown = await sampler.CollectSamplesAsync(windowMs: 2000);
MetricValidator.AssertCpuCooledDown(baseline, cooldown);
```

## Metric Coverage

### ✅ CPU Metrics
- [x] **CPU Usage (%)** - Validates under load, cooldown
- [x] **Processor Count** - Validates stability
- [x] **Thread Count** - Validates increase with new threads
- [x] **Process Count** - Validates reasonable values
- [x] **Total Threads** - Validates vs process threads

### ✅ Memory Metrics
- [x] **Memory Used** - Validates increase with allocation
- [x] **Memory Free** - Validates decrease with allocation
- [x] **Usage Percentage** - Validates accuracy
- [x] **Total Memory** - Validates stability
- [x] **Memory Churn** - Validates variance under churn

### ✅ System Drive Metrics
- [x] **Drive Space (Used/Free)** - Validates change with file write
- [x] **Usage Percentage** - Validates accuracy
- [x] **Multiple Drives** - Validates multi-drive detection
- [x] **Stability** - Validates without I/O

### ⚠️ Disk Speed Metrics
- [ ] **Read Throughput** - Requires platform-specific implementation
- [ ] **Write Throughput** - Requires platform-specific implementation
- [ ] **Read IOPS** - Requires platform-specific implementation
- [ ] **Write IOPS** - Requires platform-specific implementation
- [ ] **Latency** - Requires platform-specific implementation

*Note: Disk speed metrics require OS-specific performance counters not yet implemented*

### ⚠️ Network Metrics
- [ ] **Rx Bytes/sec** - Not yet exposed in ISystemResourceMonitor
- [ ] **Tx Bytes/sec** - Not yet exposed in ISystemResourceMonitor

*Note: NetworkIoGenerator is implemented and ready when network metrics are added*

### ⚠️ Hardware Metrics (Optional)
- [ ] **GPU Metrics** - Platform-specific, may not be available
- [ ] **Battery Metrics** - Only on battery-powered systems
- [ ] **CPU Temperature** - Platform-specific, may require elevated permissions
- [ ] **Disk Health (SMART)** - Platform-specific, limited availability

## Running Tests

### Run All Integration Tests
```powershell
dotnet test --filter "Category=Integration&Category=ResourceMonitor"
```

### Run Specific Metric Tests
```powershell
# CPU tests only
dotnet test --filter "Metric=CPU"

# Memory tests only
dotnet test --filter "Metric=Memory"

# System Drive tests only
dotnet test --filter "Metric=SystemDrive"

# Process tests only
dotnet test --filter "Metric=Process"
```

### Run Without Heavy Tests
Heavy tests are marked with `Skip` attribute and won't run by default:
```powershell
dotnet test --filter "Category=Integration&Category=ResourceMonitor"
```

### Run Heavy Tests (Manual)
To run heavy tests (max CPU load, large allocations):
```powershell
# Remove Skip attribute temporarily or run:
dotnet test --filter "FullyQualifiedName~Heavy"
```

## Tolerances & Thresholds

Tolerances are platform-specific and defined in `MetricValidator.Tolerances`:

| Metric | Windows | Linux/macOS | Notes |
|--------|---------|-------------|-------|
| CPU Usage Increase | 5.0% | 3.0% | Minimum increase to detect load |
| CPU Expected Under Load | 30.0% | 30.0% | Minimum during heavy load tests |
| Memory Increase | 10.0 MB | 10.0 MB | Minimum to detect allocation |
| Disk Throughput | 1.0 MB/s | 1.0 MB/s | Minimum to detect I/O |
| Thread Count | 1 | 1 | Minimum increase |
| Timing Tolerance | 20% | 20% | Variance allowed |

## CI Compatibility

### Test Stability
Tests are designed for CI with:
- **Conservative thresholds** - Tolerances account for CI variance
- **Timeout protection** - All operations have cancellation tokens
- **Guaranteed cleanup** - `IDisposable` pattern ensures resource cleanup
- **Isolation** - Tests don't interfere with each other
- **Platform detection** - Platform-specific tolerances

### Expected Flake Rate
Target: **< 2% failure rate** over 20 runs

Current status:
- ✅ CPU tests: Stable
- ✅ Memory tests: Stable (GC timing can vary)
- ✅ Drive tests: Stable (file system caching)
- ⚠️ Process tests: Some handle tests skipped (platform-specific)

## Diagnostics

All tests output detailed diagnostics via `ITestOutputHelper`:

```
=== CPU Usage Load Test ===
Processor Count: 12

[BASELINE] Collecting baseline CPU metrics...
Baseline CPU Usage: Avg=5.23%, Min=3.12%, Max=8.45%
Baseline Threads: Avg=42, Max=45

[LOAD] Generating CPU load...
Load CPU Usage: Avg=67.89%, Min=45.23%, Max=89.12%
Load Threads: Avg=54, Max=57
✓ CPU usage increased by 62.66%

[COOLDOWN] Waiting for CPU to cool down...
Cooldown CPU Usage: Avg=6.12%, Min=4.01%, Max=9.23%
✓ CPU cooled down (within 0.89% of baseline)
```

### On Failure
Tests output:
- Baseline, load, and cooldown values
- Expected vs actual increases
- Platform information
- Tolerance thresholds
- Sample count and window sizes

## Extending Tests

### Adding a New Metric Test

1. **Identify the metric** - Check `SystemResourceMonitorMetrics`
2. **Create load generator** (if needed) - Follow existing patterns
3. **Add test method** - Follow baseline/load/cooldown pattern
4. **Add validation helper** (if needed) - Add to `MetricValidator`
5. **Document coverage** - Update this README

### Example: Adding Network Metrics

```csharp
[Fact]
public async Task NetworkRxBytes_IncreasesWithTraffic()
{
    // Arrange
    using var networkIo = new NetworkIoGenerator();
    var port = networkIo.StartServer();

    // Baseline
    var baseline = await _sampler.CollectSamplesAsync(windowMs: 1000);

    // Load
    await networkIo.GenerateTrafficAsync(port, durationMs: 3000, throughputMbps: 10);
    var load = await _sampler.CollectSamplesAsync(windowMs: 2000);

    // Assert
    MetricValidator.AssertNetworkThroughputIncreased(baseline, load);
}
```

## Known Limitations

### Platform-Specific Metrics
- **Disk Speed**: Requires OS-specific performance counters
- **Network**: Not yet exposed in ISystemResourceMonitor
- **GPU**: Requires vendor-specific APIs
- **Battery**: Only available on battery-powered systems
- **CPU Temperature**: May require elevated permissions

### Test Constraints
- **CI Variability**: Background processes can affect metrics
- **File System Caching**: Disk tests affected by OS caching
- **GC Timing**: Memory tests subject to non-deterministic GC
- **Thread Scheduling**: OS scheduler affects CPU tests

### Mitigation Strategies
- Use **windowed averages** instead of spot checks
- Apply **platform-specific tolerances**
- Allow **relative changes** not absolute values
- Implement **retry logic** for flaky assertions
- **Skip** tests when metrics unavailable

## References

- [SystemResourceMonitor Documentation](../../docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/README.md)
- [Metrics Field Status](../../docs/SystemResourceMonitoring-MetricsFieldStatus.md)
- [Hardware Metrics](../../docs/SystemResourceMonitoring-HardwareMetrics.md)

## Definition of Done

- [x] All exposed metrics have corresponding tests
- [x] Tests follow baseline/load/cooldown pattern
- [x] Load generators for CPU, memory, disk, network, process
- [x] Validation helpers with tolerances
- [x] Guaranteed cleanup on failure
- [x] CI-compatible thresholds
- [x] Comprehensive documentation
- [ ] Network metrics (blocked: not exposed yet)
- [ ] Disk speed metrics (blocked: platform-specific impl needed)

## Contributing

When adding new metrics to `ISystemResourceMonitor`:

1. Update this test suite with new test cases
2. Add validation helpers to `MetricValidator`
3. Update coverage checklist above
4. Document tolerances and platform differences
5. Ensure tests pass in CI (20-run validation)
