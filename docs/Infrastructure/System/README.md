# System Infrastructure Components

## Overview

The System Infrastructure module provides comprehensive system-level monitoring, performance analysis, and resource management capabilities. These components enable applications to monitor their runtime environment, track resource utilization, and optimize performance based on real-time system metrics.

## Purpose

- **System Monitoring**: Real-time monitoring of system resources and performance
- **Performance Analysis**: Detailed analysis of application and system performance
- **Resource Management**: Intelligent resource utilization tracking and optimization
- **Operational Intelligence**: Data-driven insights for system optimization

## Components

### Network Performance Monitoring
Advanced network traffic monitoring and analysis using Event Tracing for Windows (ETW):

**[Network Performance Monitoring](Network/README.md)** - Complete network monitoring solution
- **[NetworkPerformanceData](Network/NetworkPerformanceData.md)** - Network performance metrics data model
- **[NetworkPerformanceReporter](Network/NetworkPerformanceReporter.md)** - ETW-based real-time network monitoring

### Future System Components
Planned system monitoring capabilities:

- **CPU Performance Monitor** - Real-time CPU utilization and performance tracking
- **Memory Performance Monitor** - Memory usage, allocation patterns, and GC analysis
- **Disk Performance Monitor** - Disk I/O, throughput, and storage capacity monitoring
- **System Resource Aggregator** - Combined system resource monitoring and analytics

## Key Features

### Real-Time Monitoring
- **ETW Integration**: Low-overhead Event Tracing for Windows support
- **Process-Specific Tracking**: Monitor individual applications and processes
- **Multi-Protocol Support**: TCP, UDP, and future protocol monitoring
- **Continuous Collection**: Background monitoring with configurable intervals

### Performance Analytics
- **Historical Trending**: Track performance metrics over time
- **Threshold Management**: Configurable alerts and warnings
- **Anomaly Detection**: Identify unusual performance patterns
- **Capacity Planning**: Data for infrastructure scaling decisions

### Integration Capabilities
- **Health Check Integration**: Connect to infrastructure health monitoring
- **Metrics Export**: Prometheus, Application Insights, and custom exporters
- **Background Services**: Seamless ASP.NET Core and hosted service integration
- **Configuration Management**: Flexible configuration with environment support

## Architecture

### System Monitoring Architecture
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   System APIs   │──▶│   Performance    │───▶│   Analytics &   │
│   & ETW Events  │    │   Collectors     │    │   Storage       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Raw Metrics   │    │   Processed      │    │   Monitoring    │
│   & Events      │    │   Data Models    │    │   Systems       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Network Monitoring Flow
```
Network I/O Events → ETW Session → Process Filtering → Rate Calculation → Performance Data
        ↓               ↓              ↓                    ↓                ↓
   TCP/UDP Traffic  Event Stream  Process-Specific    Bytes/Second     Monitoring APIs
```

## Getting Started

### Basic Network Monitoring
```csharp
// Simple network monitoring setup
var processId = Environment.ProcessId;
var sessionName = $"NetworkMonitor_{processId}";

using var reporter = NetworkPerformanceReporter.Create(
    processId: processId,
    sessionName: sessionName,
    enableUdp: false);

// Collect metrics periodically
while (true)
{
    await Task.Delay(TimeSpan.FromSeconds(30));
    
    var data = reporter.GetNetworkPerformanceData();
    Console.WriteLine($"Network Traffic: {data.BytesTotal:N0} bytes/second");
    Console.WriteLine($"TCP: {data.TcpBytesTotal:N0} B/s, UDP: {data.UdpBytesTotal:N0} B/s");
}
```

### ASP.NET Core Integration
```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Network monitoring
    services.AddSingleton<NetworkPerformanceReporter>(provider =>
    {
        var processId = Environment.ProcessId;
        var sessionName = $"WebApp_NetworkMonitor_{processId}";
        
        return NetworkPerformanceReporter.Create(
            processId: processId,
            sessionName: sessionName,
            enableUdp: false);
    });

    // Background monitoring service
    services.AddHostedService<SystemMonitoringBackgroundService>();
    
    // Health checks with system monitoring
    services.AddHealthChecks()
        .AddCheck<NetworkPerformanceHealthCheck>("network_performance");
}

// System monitoring endpoint
app.Map("/api/system/metrics", builder =>
{
    builder.Run(async context =>
    {
        var reporter = context.RequestServices.GetRequiredService<NetworkPerformanceReporter>();
        var data = reporter.GetNetworkPerformanceData();
        
        var metrics = new
        {
            network = new
            {
                tcp = new { received = data.TcpBytesReceived, sent = data.TcpBytesSent, total = data.TcpBytesTotal },
                udp = new { received = data.UdpBytesReceived, sent = data.UdpBytesSent, total = data.UdpBytesTotal },
                total = new { received = data.BytesReceived, sent = data.BytesSent, total = data.BytesTotal }
            },
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsJsonAsync(metrics);
    });
});
```

### Background Monitoring Service
```csharp
public class SystemMonitoringBackgroundService : BackgroundService
{
    private readonly NetworkPerformanceReporter _networkReporter;
    private readonly ILogger<SystemMonitoringBackgroundService> _logger;
    private readonly IMetricsLogger _metricsLogger;

    public SystemMonitoringBackgroundService(
        NetworkPerformanceReporter networkReporter,
        ILogger<SystemMonitoringBackgroundService> logger,
        IMetricsLogger metricsLogger)
    {
        _networkReporter = networkReporter;
        _logger = logger;
        _metricsLogger = metricsLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Collect network metrics
                var networkData = _networkReporter.GetNetworkPerformanceData();
                
                _logger.LogInformation("System Performance - Network: {NetworkTotal:N0} B/s, TCP: {TcpTotal:N0} B/s, UDP: {UdpTotal:N0} B/s",
                    networkData.BytesTotal, networkData.TcpBytesTotal, networkData.UdpBytesTotal);

                // Send to metrics system
                await _metricsLogger.GaugeAsync("system.network.tcp.bytes_received", networkData.TcpBytesReceived);
                await _metricsLogger.GaugeAsync("system.network.tcp.bytes_sent", networkData.TcpBytesSent);
                await _metricsLogger.GaugeAsync("system.network.udp.bytes_received", networkData.UdpBytesReceived);
                await _metricsLogger.GaugeAsync("system.network.udp.bytes_sent", networkData.UdpBytesSent);
                await _metricsLogger.GaugeAsync("system.network.total.bytes", networkData.BytesTotal);

                // TODO: Add CPU, Memory, Disk monitoring here
                
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System monitoring iteration failed");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
```

## Configuration Examples

### Complete System Monitoring Configuration
```json
{
  "SystemMonitoring": {
    "Network": {
      "Enabled": true,
      "EnableUDP": false,
      "CollectionIntervalSeconds": 30,
      "SessionNamePrefix": "SystemMonitor_Network",
      "Thresholds": {
        "WarningBytesPerSecond": 10485760,
        "CriticalBytesPerSecond": 52428800
      }
    },
    "CPU": {
      "Enabled": true,
      "CollectionIntervalSeconds": 15,
      "Thresholds": {
        "WarningPercentage": 75,
        "CriticalPercentage": 90
      }
    },
    "Memory": {
      "Enabled": true,
      "CollectionIntervalSeconds": 30,
      "Thresholds": {
        "WarningPercentage": 80,
        "CriticalPercentage": 95
      }
    },
    "Disk": {
      "Enabled": true,
      "CollectionIntervalSeconds": 60,
      "MonitoredDrives": ["C:", "D:"],
      "Thresholds": {
        "WarningPercentage": 85,
        "CriticalPercentage": 95
      }
    }
  }
}
```

### Dependency Injection Setup
```csharp
public static class SystemMonitoringServiceExtensions
{
    public static IServiceCollection AddSystemMonitoring(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var config = configuration.GetSection("SystemMonitoring").Get<SystemMonitoringOptions>();
        
        // Network monitoring
        if (config?.Network?.Enabled == true)
        {
            services.AddSingleton<NetworkPerformanceReporter>(provider =>
            {
                var processId = Environment.ProcessId;
                var sessionName = $"{config.Network.SessionNamePrefix}_{processId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                return NetworkPerformanceReporter.Create(
                    processId: processId,
                    sessionName: sessionName,
                    enableUdp: config.Network.EnableUDP);
            });
        }

        // Future: CPU monitoring
        // if (config?.CPU?.Enabled == true) { ... }

        // Background monitoring service
        services.AddHostedService<SystemMonitoringBackgroundService>();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck<SystemPerformanceHealthCheck>("system_performance");

        return services;
    }
}

public class SystemMonitoringOptions
{
    public NetworkMonitoringOptions? Network { get; set; }
    public CpuMonitoringOptions? CPU { get; set; }
    public MemoryMonitoringOptions? Memory { get; set; }
    public DiskMonitoringOptions? Disk { get; set; }
}

public class NetworkMonitoringOptions
{
    public bool Enabled { get; set; } = true;
    public bool EnableUDP { get; set; } = false;
    public int CollectionIntervalSeconds { get; set; } = 30;
    public string SessionNamePrefix { get; set; } = "SystemMonitor_Network";
    public NetworkThresholds Thresholds { get; set; } = new();
}
```

## Monitoring Integration

### Prometheus Metrics Export
```csharp
public class SystemMetricsExporter
{
    private readonly NetworkPerformanceReporter _networkReporter;
    // TODO: Add other system reporters

    public async Task WritePrometheusMetrics(HttpContext context)
    {
        var metrics = new StringBuilder();

        // Network metrics
        var networkData = _networkReporter.GetNetworkPerformanceData();
        
        metrics.AppendLine("# HELP system_network_tcp_bytes_received_per_second TCP bytes received per second");
        metrics.AppendLine("# TYPE system_network_tcp_bytes_received_per_second gauge");
        metrics.AppendLine($"system_network_tcp_bytes_received_per_second {networkData.TcpBytesReceived}");

        metrics.AppendLine("# HELP system_network_tcp_bytes_sent_per_second TCP bytes sent per second");
        metrics.AppendLine("# TYPE system_network_tcp_bytes_sent_per_second gauge");
        metrics.AppendLine($"system_network_tcp_bytes_sent_per_second {networkData.TcpBytesSent}");

        metrics.AppendLine("# HELP system_network_total_bytes_per_second Total network bytes per second");
        metrics.AppendLine("# TYPE system_network_total_bytes_per_second gauge");
        metrics.AppendLine($"system_network_total_bytes_per_second {networkData.BytesTotal}");

        // TODO: Add CPU, Memory, Disk metrics

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(metrics.ToString());
    }
}
```

### Health Check Integration
```csharp
public class SystemPerformanceHealthCheck : IHealthCheck
{
    private readonly NetworkPerformanceReporter _networkReporter;
    private readonly SystemMonitoringOptions _options;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var status = HealthStatus.Healthy;
            var issues = new List<string>();

            // Network performance check
            var networkData = _networkReporter.GetNetworkPerformanceData();
            healthData["network_bytes_per_second"] = networkData.BytesTotal;
            healthData["network_tcp_bytes"] = networkData.TcpBytesTotal;
            healthData["network_udp_bytes"] = networkData.UdpBytesTotal;

            if (networkData.BytesTotal > _options.Network!.Thresholds.CriticalBytesPerSecond)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical network traffic: {networkData.BytesTotal:N0} bytes/sec");
            }
            else if (networkData.BytesTotal > _options.Network.Thresholds.WarningBytesPerSecond)
            {
                status = HealthStatus.Degraded;
                issues.Add($"High network traffic: {networkData.BytesTotal:N0} bytes/sec");
            }

            // TODO: Add CPU, Memory, Disk checks

            var description = issues.Count > 0 ? string.Join("; ", issues) : "System performance normal";
            
            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("System monitoring failed", ex);
        }
    }
}
```

## Best Practices

### Performance Monitoring
1. **Appropriate Intervals**: Balance monitoring frequency with performance overhead
2. **Resource Efficiency**: Minimize the monitoring impact on system performance
3. **Selective Monitoring**: Enable only necessary monitoring components
4. **Data Retention**: Implement appropriate data retention policies

### Security Considerations
1. **Privilege Requirements**: Document administrative privilege requirements
2. **Data Sensitivity**: Consider privacy implications of system monitoring
3. **Access Control**: Restrict access to system monitoring capabilities
4. **Audit Logging**: Log system monitoring activities

### Integration Patterns
1. **Health Check Integration**: Connect to infrastructure health monitoring
2. **Metrics Export**: Support multiple monitoring platforms
3. **Alert Integration**: Connect to alerting and notification systems
4. **Dashboard Visualization**: Create operational dashboards

### Configuration Management
1. **Environment-Specific Settings**: Different configurations per environment
2. **Runtime Configuration**: Support for configuration changes without restart
3. **Validation**: Validate configuration at startup
4. **Documentation**: Document all configuration options

## Troubleshooting

### Common Issues

#### Administrative Privileges
```csharp
// Check for required privileges
if (!(TraceEventSession.IsElevated() ?? false))
{
    throw new InvalidOperationException("System monitoring requires administrator privileges");
}
```

#### Resource Conflicts
```csharp
// Handle resource conflicts gracefully
public class SafeSystemMonitor
{
    private NetworkPerformanceReporter? _networkReporter;
    private readonly ILogger _logger;

    public NetworkPerformanceData? GetNetworkData()
    {
        try
        {
            return _networkReporter?.GetNetworkPerformanceData();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect network performance data");
            return null;
        }
    }
}
```

#### Performance Impact
- Monitor the monitoring overhead itself
- Adjust collection intervals based on system load
- Use selective monitoring to reduce resource usage
- Implement circuit breakers for monitoring failures

### Diagnostic Tools
```csharp
public static class SystemMonitoringDiagnostics
{
    public static void RunDiagnostics()
    {
        Console.WriteLine("System Monitoring Diagnostics");
        Console.WriteLine("=============================");
        Console.WriteLine($"OS Version: {Environment.OSVersion}");
        Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        Console.WriteLine($"Working Set: {Environment.WorkingSet:N0} bytes");
        Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");
        Console.WriteLine($"Is Elevated: {TraceEventSession.IsElevated()}");
        
        // Test network monitoring capability
        try
        {
            var testSessionName = $"DiagnosticTest_{Guid.NewGuid():N}";
            using var testReporter = NetworkPerformanceReporter.Create(
                Environment.ProcessId, testSessionName, false);
            Console.WriteLine("Network Monitoring: AVAILABLE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network Monitoring: UNAVAILABLE - {ex.Message}");
        }
    }
}
```

## Future Roadmap

### Planned System Components
- **CPU Performance Monitor**: Real-time CPU utilization and core-level monitoring
- **Memory Performance Monitor**: Memory allocation patterns and GC analysis
- **Disk Performance Monitor**: Disk I/O, throughput, and storage monitoring
- **System Resource Aggregator**: Combined monitoring with correlation analysis

### Enhanced Features
- **Machine Learning Analytics**: Anomaly detection and predictive monitoring
- **Container Integration**: Docker and Kubernetes system monitoring
- **Cloud Platform Integration**: Azure, AWS, and GCP system monitoring
- **Cross-Platform Support**: Linux and macOS system monitoring capabilities

## Related Documentation

- **[Network Performance Monitoring](Network/README.md)** - Detailed network monitoring documentation
- **[Infrastructure Overview](../README.md)** - Complete infrastructure documentation
- **[Health Checks](../HealthChecks/README.md)** - Infrastructure health monitoring