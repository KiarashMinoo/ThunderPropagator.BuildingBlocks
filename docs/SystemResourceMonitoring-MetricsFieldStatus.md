# System Resource Monitoring - Metrics Field Status

This document provides a comprehensive overview of all metrics fields in the system resource monitoring module, indicating which fields are populated and which are placeholders for future implementation.

## Overview

The metrics system is designed to collect hardware health and performance metrics across different platforms (Windows, Linux, macOS). Many fields are defined in the metrics classes but may not be populated due to:
- Platform limitations (API not available)
- Hardware limitations (sensors not available)
- Implementation in progress (placeholder for future development)

## Battery Metrics

### BatteryMetrics

| Field | Type | Windows | Linux | macOS | Notes |
|-------|------|---------|-------|-------|-------|
| BatteryPresent | bool | ✅ | ✅ | ✅ | Always populated |
| ChargePercent | double? | ✅ | ✅ | ⚠️ | macOS: Requires additional parsing |
| Status | BatteryStatus | ✅ | ✅ | ⚠️ | macOS: Requires additional parsing |
| RemainingTimeMinutes | int? | ✅ | ✅ | ❌ | macOS: Not available via pmset |
| HealthPercent | double? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| DesignCapacityMWh | long? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| FullChargeCapacityMWh | long? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| ChargeRateMW | long? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| VoltageMV | long? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| TemperatureCelsius | double? | ❌ | ❌ | ❌ | Not commonly available |
| CycleCount | int? | ❌ | ✅ | ❌ | Windows: Requires WMI implementation |
| OnACPower | bool | ✅ | ✅ | ✅ | Always populated |
| ErrorMessage | string? | ✅ | ✅ | ✅ | Populated on errors |

**Legend:**
- ✅ Fully implemented and populated
- ⚠️ Partially implemented (basic support)
- ❌ Not implemented (returns null)

## GPU Metrics

### GpuMetrics

| Field | Type | Windows | Linux | macOS | Notes |
|-------|------|---------|-------|-------|-------|
| GpuIndex | int | ✅ | ✅ | ✅ | Always populated |
| GpuName | string? | ✅ | ⚠️ | ✅ | Linux: Generic names only |
| TemperatureCelsius | double? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| UtilizationPercent | double? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| MemoryUtilizationPercent | double? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| TotalMemoryMB | double? | ❌ | ❌ | ✅ | macOS: Via system_profiler |
| UsedMemoryMB | long? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| PowerUsageWatts | double? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| FanSpeedPercent | double? | ❌ | ❌ | ❌ | Requires NVML/AMD Display Library |
| ActiveProcesses | List<GpuProcessInfo> | ✅ | ✅ | ✅ | Always initialized (empty list) |
| IsAvailable | bool | ✅ | ✅ | ✅ | Always populated |
| ErrorMessage | string? | ✅ | ✅ | ✅ | Populated with status/errors |

### GpuProcessInfo

| Field | Type | Status | Notes |
|-------|------|--------|-------|
| ProcessId | int | ❌ | Requires NVML/AMD Display Library implementation |
| ProcessName | string? | ❌ | Requires NVML/AMD Display Library implementation |
| UsedMemoryMB | long? | ❌ | Requires NVML/AMD Display Library implementation |
| UtilizationPercent | double? | ❌ | Requires NVML/AMD Display Library implementation |

**Implementation Notes:**
- Full GPU metrics require vendor-specific libraries (NVIDIA NVML, AMD Display Library, DirectX/DXGI)
- Current implementation only detects GPU presence and model
- nvidia-smi and rocm-smi detection is implemented on Linux but parsing is not yet complete

## CPU Temperature Metrics

### CpuTemperatureMetrics

| Field | Type | Windows | Linux | macOS | Notes |
|-------|------|---------|-------|-------|-------|
| PackageTemperatureCelsius | double? | ⚠️ | ✅ | ⚠️ | Windows: Requires WMI thermal sensors |
| CoreTemperatures | Dictionary<int, double> | ⚠️ | ✅ | ⚠️ | Windows: Requires WMI thermal sensors |
| MaxTemperatureCelsius | double? | ⚠️ | ✅ | ⚠️ | Windows: Requires WMI thermal sensors |
| AverageTemperatureCelsius | double? | ⚠️ | ✅ | ⚠️ | Windows: Requires WMI thermal sensors |
| TemperatureSensorsAvailable | bool | ✅ | ✅ | ✅ | Always populated |
| ErrorMessage | string? | ✅ | ✅ | ✅ | Populated on errors |

**Implementation Notes:**
- Linux: Reads from `/sys/class/thermal/thermal_zone*`
- Windows: Uses WMI MSAcpi_ThermalZoneTemperature (may not be available on all systems)
- macOS: Requires IOKit implementation (placeholder currently)

## Disk Health Metrics

### DiskHealthMetrics

| Field | Type | Windows | Linux | macOS | Notes |
|-------|------|---------|-------|-------|-------|
| DriveId | string | ✅ | ✅ | ✅ | Always populated |
| Status | DiskHealthStatus | ⚠️ | ⚠️ | ⚠️ | Returns Unknown (SMART not implemented) |
| WearLevelPercent | double? | ❌ | ❌ | ❌ | Requires SMART data parsing |
| TemperatureCelsius | double? | ❌ | ❌ | ❌ | Requires SMART data parsing |
| ReallocatedSectorsCount | long? | ❌ | ❌ | ❌ | Requires SMART data parsing |
| PowerOnHours | long? | ❌ | ❌ | ❌ | Requires SMART data parsing |
| SmartAvailable | bool | ✅ | ✅ | ✅ | Always populated (currently false) |
| ErrorMessage | string? | ✅ | ✅ | ✅ | Populated with status/errors |

**Implementation Notes:**
- SMART data collection requires:
  - Windows: WMI queries or vendor-specific APIs
  - Linux: smartctl (smartmontools package)
  - macOS: diskutil smartdata
- Current implementation only detects drives and marks SMART as unavailable

## Disk Speed Metrics

### DiskSpeedMetrics

| Field | Type | Windows | Linux | macOS | Notes |
|-------|------|---------|-------|-------|-------|
| DriveId | string | ✅ | ✅ | ✅ | Always populated |
| ReadThroughputMBps | double? | ❌ | ❌ | ❌ | Requires performance counters |
| WriteThroughputMBps | double? | ❌ | ❌ | ❌ | Requires performance counters |
| ReadIOPS | double? | ❌ | ❌ | ❌ | Requires performance counters |
| WriteIOPS | double? | ❌ | ❌ | ❌ | Requires performance counters |
| AverageReadLatencyMs | double? | ❌ | ❌ | ❌ | Requires performance counters |
| AverageWriteLatencyMs | double? | ❌ | ❌ | ❌ | Requires performance counters |
| QueueDepth | long? | ❌ | ❌ | ❌ | Requires performance counters |
| ActiveTimePercent | double? | ❌ | ❌ | ❌ | Requires performance counters |
| PerformanceCountersAvailable | bool | ✅ | ✅ | ✅ | Always populated (currently false) |
| ErrorMessage | string? | ✅ | ✅ | ✅ | Populated with status/errors |

**Implementation Notes:**
- Performance metrics require:
  - Windows: System.Diagnostics.PerformanceCounter package (Windows-only)
  - Linux: Parsing `/proc/diskstats`
  - macOS: iostat parsing
- Current implementation only detects drives and marks performance counters as unavailable

## Future Implementation Priorities

### High Priority
1. **Windows Battery Extended Metrics**: Implement WMI queries to collect health, capacity, voltage, and cycle count
2. **Disk Performance Counters**: Implement actual disk speed monitoring on all platforms
3. **CPU Temperature Windows**: Improve WMI thermal zone detection

### Medium Priority
1. **SMART Data Collection**: Implement full SMART parsing for disk health metrics
2. **GPU Detailed Metrics**: Integrate NVML for NVIDIA GPUs on Windows and Linux
3. **macOS Battery Details**: Enhance pmset parsing or use IOKit

### Low Priority
1. **AMD GPU Support**: Integrate AMD Display Library for detailed AMD GPU metrics
2. **GPU Process Monitoring**: Implement per-process GPU usage tracking
3. **Battery Temperature**: Add battery temperature where hardware supports it

## Testing Status

All metrics clients have unit tests using NSubstitute for mocking:
- ✅ BatteryMetricsClientTests
- ✅ GpuMetricsClientTests
- ✅ CpuTemperatureMetricsClientTests
- ✅ DiskHealthMetricsClientTests
- ✅ DiskSpeedMetricsClientTests

Tests verify:
- Proper error handling
- Field initialization
- Provider interaction
- Cancellation token support

## Configuration

All metric groups can be enabled/disabled through configuration:
- Battery metrics
- CPU temperature
- GPU metrics
- Disk health
- Disk speed

See `SystemResourceMonitorOptions` for configuration details.

---

**Last Updated**: December 24, 2025

