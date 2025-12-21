using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;

/// <summary>
/// Client for collecting GPU metrics.
/// </summary>
internal sealed class GpuMetricsClient(int maxProcesses = 10)
{
    private readonly IGpuMetricsProvider _provider = CreatePlatformProvider();

    public GpuMetrics[] GetMetrics()
    {
        try
        {
            return _provider.GetGpuMetrics(maxProcesses);
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
    GpuMetrics[] GetGpuMetrics(int maxProcesses);
}

/// <summary>
/// Windows GPU metrics provider using WMI and command-line tools.
/// </summary>
internal sealed class WindowsGpuMetricsProvider : IGpuMetricsProvider
{
    public GpuMetrics[] GetGpuMetrics(int maxProcesses)
    {
        var metrics = new List<GpuMetrics>();

        try
        {
            // Try to detect GPUs using WMIC
            var gpuInfo = TryGetWindowsGpuInfo();

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
                    ErrorMessage = "GPU metrics require NVML (NVIDIA), AMD Display Library, or DirectX/DXGI implementation"
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows GPU metrics provider error: {ex.Message}");
            metrics.Add(new GpuMetrics
            {
                GpuIndex = 0,
                IsAvailable = false,
                ErrorMessage = ex.Message
            });
        }

        return metrics.ToArray();
    }

    private static List<string>? TryGetWindowsGpuInfo()
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

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                return null;

            // Parse GPU names from output
            var gpuNames = new List<string>();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
                {
                    var name = line.Substring(5).Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        gpuNames.Add(name);
                }
            }

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
    public GpuMetrics[] GetGpuMetrics(int maxProcesses)
    {
        var metrics = new List<GpuMetrics>();

        try
        {
            // Try NVIDIA first
            var nvidiaMetrics = TryGetNvidiaMetrics();
            if (nvidiaMetrics is { Length: > 0 })
            {
                metrics.AddRange(nvidiaMetrics);
                return metrics.ToArray();
            }

            // Try AMD/ROCm
            var amdMetrics = TryGetAmdMetrics(maxProcesses);
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
                            ErrorMessage = "No GPU detected or nvidia-smi/rocm-smi not available"
                        }
                    ];
            }
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
                    ErrorMessage = ex.Message
                }
            ];
        }
    }

    private static GpuMetrics[]? TryGetNvidiaMetrics()
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

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

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
                    ErrorMessage = "nvidia-smi parsing not yet implemented"
                }
            ];
        }
        catch
        {
            return null;
        }
    }

    private static GpuMetrics[]? TryGetAmdMetrics(int maxProcesses)
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

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

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
    public GpuMetrics[] GetGpuMetrics(int maxProcesses)
    {
        try
        {
            // macOS GPU metrics require Metal framework or system_profiler
            return
            [
                new GpuMetrics
                {
                    GpuIndex = 0,
                    GpuName = "macOS GPU",
                    IsAvailable = false,
                    ErrorMessage = "GPU metrics on macOS require Metal framework or system_profiler parsing"
                }
            ];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"macOS GPU metrics provider error: {ex.Message}");
            return
            [
                new GpuMetrics
                {
                    GpuIndex = 0,
                    IsAvailable = false,
                    ErrorMessage = ex.Message
                }
            ];
        }
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedGpuMetricsProvider : IGpuMetricsProvider
{
    public GpuMetrics[] GetGpuMetrics(int maxProcesses)
    {
        return Array.Empty<GpuMetrics>();
    }
}