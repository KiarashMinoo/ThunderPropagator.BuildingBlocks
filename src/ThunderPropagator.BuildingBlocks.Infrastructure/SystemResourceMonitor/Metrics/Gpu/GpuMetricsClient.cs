using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;

public interface IGpuMetricsClient : IMetricsClient<GpuMetrics[]>
{
    Task<GpuMetrics[]> GetMetricsAsync(int maxProcesses = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Client for collecting GPU metrics.
/// </summary>
internal sealed class GpuMetricsClient : IGpuMetricsClient
{
    private readonly IGpuMetricsProvider _provider;
    private readonly int _maxProcesses;

    internal GpuMetricsClient(IGpuMetricsProvider provider, int maxProcesses = 10)
    {
        _provider = provider;
        _maxProcesses = maxProcesses;
    }

    /// <summary>
    /// Client for collecting GPU metrics.
    /// </summary>
    public GpuMetricsClient(int maxProcesses = 10) : this(CreatePlatformProvider(), maxProcesses)
    {
    }

    public Task<GpuMetrics[]> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return GetMetricsAsync(_maxProcesses, cancellationToken);
    }

    public async Task<GpuMetrics[]> GetMetricsAsync(int maxProcesses = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _provider.GetGpuMetricsAsync(maxProcesses, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting GPU metrics: {ex.Message}");
            return Array.Empty<GpuMetrics>();
        }
    }

    private static IGpuMetricsProvider CreatePlatformProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsGpuMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxGpuMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsGpuMetricsProvider();

        return new UnsupportedGpuMetricsProvider();
    }
}

/// <summary>
/// Platform-specific GPU metrics provider interface.
/// </summary>
internal interface IGpuMetricsProvider
{
    Task<GpuMetrics[]> GetGpuMetricsAsync(int maxProcesses, CancellationToken cancellationToken);
}

/// <summary>
/// Windows GPU metrics provider using WMI and command-line tools.
/// </summary>
internal sealed class WindowsGpuMetricsProvider : IGpuMetricsProvider
{
    public async Task<GpuMetrics[]> GetGpuMetricsAsync(int maxProcesses, CancellationToken cancellationToken)
    {
        var metrics = new List<GpuMetrics>();

        try
        {
            // Try to detect GPUs using WMIC
            var gpuInfo = await TryGetWindowsGpuInfoAsync(cancellationToken).ConfigureAwait(false);

            if (gpuInfo == null || gpuInfo.Count == 0)
            {
                // No GPU detected
                return
                [
                    new GpuMetrics
                    {
                        GpuIndex = 0,
                        GpuName = "Unknown",
                        IsAvailable = false,
                        ActiveProcesses = new List<GpuProcessInfo>(),
                        ErrorMessage = "No GPU detected. GPU drivers may not be installed or accessible."
                    }
                ];
            }

            // Create metrics for each detected GPU
            for (int i = 0; i < gpuInfo.Count; i++)
            {
                metrics.Add(new GpuMetrics
                {
                    GpuIndex = i,
                    GpuName = gpuInfo[i],
                    IsAvailable = false,
                    ActiveProcesses = new List<GpuProcessInfo>(),
                    ErrorMessage = "GPU metrics require NVML (NVIDIA), AMD Display Library, or DirectX/DXGI implementation"
                });
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows GPU metrics provider error: {ex.Message}");
            metrics.Add(new GpuMetrics
            {
                GpuIndex = 0,
                IsAvailable = false,
                ActiveProcesses = new List<GpuProcessInfo>(),
                ErrorMessage = ex.Message
            });
        }

        return metrics.ToArray();
    }

    private static async Task<List<string>?> TryGetWindowsGpuInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "path win32_VideoController get name /format:list",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                return null;

            // Parse GPU names from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var gpuNames = (
                    from line in lines
                    where line.StartsWith("Name=", StringComparison.OrdinalIgnoreCase)
                    select line[5..].Trim()
                    into name
                    where !string.IsNullOrWhiteSpace(name)
                    select name)
                .ToList();

            return gpuNames.Count > 0 ? gpuNames : null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Linux GPU metrics provider using nvidia-smi or rocm-smi.
/// </summary>
internal sealed class LinuxGpuMetricsProvider : IGpuMetricsProvider
{
    public async Task<GpuMetrics[]> GetGpuMetricsAsync(int maxProcesses, CancellationToken cancellationToken)
    {
        var metrics = new List<GpuMetrics>();

        try
        {
            // Try NVIDIA first
            var nvidiaMetrics = await TryGetNvidiaMetricsAsync(cancellationToken).ConfigureAwait(false);
            if (nvidiaMetrics is { Length: > 0 })
            {
                metrics.AddRange(nvidiaMetrics);
                return metrics.ToArray();
            }

            // Try AMD/ROCm
            var amdMetrics = await TryGetAmdMetricsAsync(maxProcesses, cancellationToken).ConfigureAwait(false);
            switch (amdMetrics)
            {
                case { Length: > 0 }:
                    metrics.AddRange(amdMetrics);
                    return metrics.ToArray();
                default:
                    // No GPU found
                    return
                    [
                        new GpuMetrics
                        {
                            GpuIndex = 0,
                            IsAvailable = false,
                            ActiveProcesses = new List<GpuProcessInfo>(),
                            ErrorMessage = "No GPU detected or nvidia-smi/rocm-smi not available"
                        }
                    ];
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Linux GPU metrics provider error: {ex.Message}");
            return
            [
                new GpuMetrics
                {
                    GpuIndex = 0,
                    IsAvailable = false,
                    ActiveProcesses = new List<GpuProcessInfo>(),
                    ErrorMessage = ex.Message
                }
            ];
        }
    }

    private static async Task<GpuMetrics[]?> TryGetNvidiaMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check if nvidia-smi exists
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "nvidia-smi",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                return null;

            // nvidia-smi is available, but we need to implement the actual query
            // This is a placeholder
            return
            [
                new GpuMetrics
                {
                    GpuIndex = 0,
                    GpuName = "NVIDIA GPU",
                    IsAvailable = false,
                    ActiveProcesses = new List<GpuProcessInfo>(),
                    ErrorMessage = "nvidia-smi parsing not yet implemented"
                }
            ];
        }
        catch
        {
            return null;
        }
    }

    private static async Task<GpuMetrics[]?> TryGetAmdMetricsAsync(int maxProcesses, CancellationToken cancellationToken)
    {
        try
        {
            // Check if rocm-smi exists
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "rocm-smi",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                return null;

            // rocm-smi is available
            // Full implementation would parse rocm-smi output
            return
            [
                new GpuMetrics
                {
                    GpuIndex = 0,
                    GpuName = "AMD GPU",
                    IsAvailable = false,
                    ActiveProcesses = new List<GpuProcessInfo>(),
                    ErrorMessage = $"rocm-smi detected but full parsing not yet implemented. Max processes: {maxProcesses}"
                }
            ];
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// macOS GPU metrics provider using Metal performance counters.
/// </summary>
internal sealed class MacOsGpuMetricsProvider : IGpuMetricsProvider
{
    public async Task<GpuMetrics[]> GetGpuMetricsAsync(int maxProcesses, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var psi = new ProcessStartInfo("system_profiler", "SPDisplaysDataType -json")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return [new GpuMetrics { GpuIndex = 0, IsAvailable = false, ActiveProcesses = new List<GpuProcessInfo>(), ErrorMessage = "Failed to start system_profiler." }];
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                return [new GpuMetrics { GpuIndex = 0, IsAvailable = false, ActiveProcesses = new List<GpuProcessInfo>(), ErrorMessage = "system_profiler exited with a non-zero code." }];
            }

            return ParseSystemProfilerOutput(output);
        }
        catch (Exception ex)
        {
            return [new GpuMetrics { GpuIndex = 0, IsAvailable = false, ActiveProcesses = new List<GpuProcessInfo>(), ErrorMessage = $"Failed to get GPU metrics on macOS: {ex.Message}" }];
        }
    }

    private static GpuMetrics[] ParseSystemProfilerOutput(string jsonOutput)
    {
        try
        {
            var json = JsonDocument.Parse(jsonOutput);
            var displays = json.RootElement.GetProperty("SPDisplaysDataType");
            var metrics = new List<GpuMetrics>();
            var index = 0;

            foreach (var display in displays.EnumerateArray())
            {
                var name = display.TryGetProperty("sppci_model", out var model) ? model.GetString() : "Unknown GPU";
                var vram = display.TryGetProperty("spdisplays_vram", out var vramProp) ? vramProp.GetString() : "N/A";

                // VRAM is often reported as "X GB", so we parse it.
                double.TryParse(vram?.Split(' ')[0], out var totalMemoryMb);
                if (vram != null && vram.Contains("GB"))
                {
                    totalMemoryMb *= 1024;
                }

                metrics.Add(new GpuMetrics
                {
                    GpuIndex = index++,
                    GpuName = name,
                    TotalMemoryMB = totalMemoryMb,
                    IsAvailable = true,
                    ActiveProcesses = new List<GpuProcessInfo>(),
                    ErrorMessage = "Temperature and utilization not available via system_profiler."
                });
            }

            return metrics.ToArray();
        }
        catch (Exception ex)
        {
            return [new GpuMetrics { GpuIndex = 0, IsAvailable = false, ActiveProcesses = new List<GpuProcessInfo>(), ErrorMessage = $"Failed to parse system_profiler output: {ex.Message}" }];
        }
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedGpuMetricsProvider : IGpuMetricsProvider
{
    public Task<GpuMetrics[]> GetGpuMetricsAsync(int maxProcesses, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.Empty<GpuMetrics>());
    }
}