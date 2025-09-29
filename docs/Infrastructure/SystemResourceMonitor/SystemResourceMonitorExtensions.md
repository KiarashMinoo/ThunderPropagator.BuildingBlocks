# SystemResourceMonitorExtensions

## Overview

The `SystemResourceMonitorExtensions` class provides extension methods for `IServiceCollection` to simplify registration and configuration of system resource monitoring components in .NET applications. This static class follows the standard .NET extension pattern for dependency injection configuration.

## Purpose

- **Simplified Registration**: Easy system resource monitoring setup with fluent API
- **Dependency Injection**: Proper service registration and lifetime management
- **Service Composition**: Automatic registration of all required metric clients
- **Framework Integration**: Seamless integration with .NET dependency injection

## Class Declaration

```csharp
public static class SystemResourceMonitorExtensions
{
    public static IServiceCollection AddSystemResourceMonitor(this IServiceCollection services)
    {
        services.TryAddSingleton<CpuMetricsClient>();
        services.TryAddSingleton<MemoryMetricsClient>();
        services.TryAddSingleton<SystemDriveMetricsClient>();
        services.TryAddSingleton<ISystemResourceMonitor, SystemResourceMonitorImpl>();

        return services;
    }
}
```

## Extension Method

### AddSystemResourceMonitor

Registers all system resource monitoring components with the dependency injection container.

#### Service Registrations

The method registers the following services as singletons:

1. **CpuMetricsClient** - CPU performance monitoring client
2. **MemoryMetricsClient** - Memory utilization monitoring client  
3. **SystemDriveMetricsClient** - Disk/drive monitoring client
4. **ISystemResourceMonitor** - Main monitoring interface implementation

#### Return Value
Returns the `IServiceCollection` to enable method chaining.

## Usage Examples

### Basic Registration
```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register system resource monitoring
    services.AddSystemResourceMonitor();
    
    // Optional: Add health checks
    services.AddHealthChecks()
        .AddCheck<SystemResourceHealthCheck>("system_resources");
    
    // Optional: Add background monitoring
    services.AddHostedService<SystemResourceBackgroundMonitor>();
}
```

### Complete Application Setup
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging();
        
        // Add configuration
        services.AddOptions<SystemMonitoringOptions>()
            .Bind(Configuration.GetSection("SystemMonitoring"))
            .ValidateDataAnnotations();
        
        // Add system resource monitoring
        services.AddSystemResourceMonitor();
        
        // Add health checks with system resource monitoring
        services.AddHealthChecks()
            .AddCheck<SystemResourceHealthCheck>("system_resources");
        
        // Add background monitoring service
        services.AddHostedService<SystemResourceBackgroundMonitor>();
        
        // Add controllers for API endpoints
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        
        // Health check endpoints
        app.UseHealthChecks("/health");
        app.UseHealthChecks("/health/system", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("system")
        });
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

### Advanced Configuration with Options
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure monitoring options
    services.Configure<SystemMonitoringOptions>(options =>
    {
        options.MeasurementWindowMs = 2000;  // 2-second CPU measurement window
        options.MonitorAllProcesses = false;  // Monitor only current process
        options.CollectionIntervalSeconds = 30;  // Collect metrics every 30 seconds
        
        // CPU thresholds
        options.CpuThreshold = new ThresholdOptions
        {
            Warning = 75.0,
            Critical = 90.0
        };
        
        // Memory thresholds
        options.MemoryThreshold = new ThresholdOptions
        {
            Warning = 80.0,
            Critical = 95.0
        };
        
        // Disk thresholds
        options.DiskThreshold = new ThresholdOptions
        {
            Warning = 85.0,
            Critical = 95.0
        };
    });

    // Add system resource monitoring
    services.AddSystemResourceMonitor();
    
    // Add custom metrics publisher
    services.AddSingleton<IMetricsPublisher, PrometheusMetricsPublisher>();
    
    // Add alerting service
    services.AddSingleton<IAlertingService, EmailAlertingService>();
    
    // Add background monitoring with alerting
    services.AddHostedService<SystemResourceBackgroundMonitor>();
}
```

### Environment-Specific Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add system resource monitoring
    services.AddSystemResourceMonitor();
    
    if (Environment.IsDevelopment())
    {
        // Development: More frequent monitoring, lower thresholds
        services.Configure<SystemMonitoringOptions>(options =>
        {
            options.CollectionIntervalSeconds = 15;
            options.CpuThreshold.Warning = 60;
            options.MemoryThreshold.Warning = 70;
        });
        
        // Add development-specific monitoring
        services.AddHostedService<DevelopmentSystemMonitor>();
    }
    else if (Environment.IsProduction())
    {
        // Production: Less frequent monitoring, higher thresholds
        services.Configure<SystemMonitoringOptions>(options =>
        {
            options.CollectionIntervalSeconds = 60;
            options.MonitorAllProcesses = true;  // Monitor entire system in production
            options.CpuThreshold.Warning = 80;
            options.MemoryThreshold.Warning = 85;
        });
        
        // Add production monitoring and alerting
        services.AddHostedService<ProductionSystemMonitor>();
        services.AddSingleton<IAlertingService, SlackAlertingService>();
    }
}
```

### Containerized Application Setup
```csharp
// For Docker containers or Kubernetes pods
public void ConfigureServices(IServiceCollection services)
{
    // Container-optimized system monitoring
    services.AddSystemResourceMonitor();
    
    services.Configure<SystemMonitoringOptions>(options =>
    {
        // Monitor all processes in container
        options.MonitorAllProcesses = true;
        
        // Container-appropriate thresholds
        options.CpuThreshold = new ThresholdOptions { Warning = 70, Critical = 85 };
        options.MemoryThreshold = new ThresholdOptions { Warning = 75, Critical = 90 };
        
        // More frequent monitoring for containers
        options.CollectionIntervalSeconds = 30;
    });
    
    // Add Kubernetes-compatible health checks
    services.AddHealthChecks()
        .AddCheck<SystemResourceHealthCheck>("system_resources", tags: new[] { "ready", "live" });
    
    // Add container monitoring
    services.AddHostedService<ContainerSystemMonitor>();
}
```

## Service Lifetime Management

### Singleton Registration
All components are registered as singletons because:

1. **CpuMetricsClient**: Stateless client for CPU metrics collection
2. **MemoryMetricsClient**: Stateless client for memory metrics collection
3. **SystemDriveMetricsClient**: Stateless client for drive metrics collection
4. **ISystemResourceMonitor**: Aggregator that coordinates metric collection

### Thread Safety
All registered services are designed to be thread-safe:
- Metric clients use process-level APIs that are inherently thread-safe
- No shared mutable state between service calls
- Safe for concurrent access from multiple background services

### Resource Management
- Services are automatically disposed by the DI container
- No explicit cleanup required for metric collection
- Minimal resource footprint for monitoring operations

## Integration Patterns

### Health Check Integration
```csharp
// Custom health check using system resource monitoring
public class SystemResourceHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly IOptions<SystemMonitoringOptions> _options;

    public SystemResourceHealthCheck(ISystemResourceMonitor monitor, IOptions<SystemMonitoringOptions> options)
    {
        _monitor = monitor;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            
            // Check if system resources are within acceptable limits
            var status = HealthStatus.Healthy;
            var issues = new List<string>();

            if (metrics.Cpu.Usage > _options.Value.CpuThreshold.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical CPU usage: {metrics.Cpu.Usage:F1}%");
            }

            if (metrics.Memory.UsagePercentage > _options.Value.MemoryThreshold.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical memory usage: {metrics.Memory.UsagePercentage:F1}%");
            }

            var description = issues.Any() ? string.Join("; ", issues) : "System resources normal";
            return new HealthCheckResult(status, description);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("System monitoring failed", ex);
        }
    }
}

// Registration
services.AddHealthChecks()
    .AddCheck<SystemResourceHealthCheck>("system_resources", tags: new[] { "system", "resources" });
```

### Background Service Integration
```csharp
public class SystemResourceBackgroundMonitor : BackgroundService
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly ILogger<SystemResourceBackgroundMonitor> _logger;
    private readonly IOptions<SystemMonitoringOptions> _options;

    public SystemResourceBackgroundMonitor(
        ISystemResourceMonitor monitor,
        ILogger<SystemResourceBackgroundMonitor> logger,
        IOptions<SystemMonitoringOptions> options)
    {
        _monitor = monitor;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metrics = _monitor.GetMetrics(
                    window: _options.Value.MeasurementWindowMs,
                    all: _options.Value.MonitorAllProcesses);

                _logger.LogInformation("System: CPU {CpuUsage:F1}%, Memory {MemoryUsage:F1}%, {DriveCount} drives",
                    metrics.Cpu.Usage, metrics.Memory.UsagePercentage, metrics.Drives.Length);

                await Task.Delay(TimeSpan.FromSeconds(_options.Value.CollectionIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System monitoring failed");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

// Registration
services.AddHostedService<SystemResourceBackgroundMonitor>();
```

### Web API Integration
```csharp
[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemController(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    [HttpGet("metrics")]
    public ActionResult<SystemResourceMonitorMetrics> GetSystemMetrics(
        [FromQuery] long window = 1000,
        [FromQuery] bool all = false)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window, all);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to collect metrics: {ex.Message}");
        }
    }
}
```

## Configuration Options

### SystemMonitoringOptions
```csharp
public class SystemMonitoringOptions
{
    [Range(500, 10000)]
    public int MeasurementWindowMs { get; set; } = 1000;
    
    public bool MonitorAllProcesses { get; set; } = false;
    
    [Range(10, 3600)]
    public int CollectionIntervalSeconds { get; set; } = 60;
    
    public ThresholdOptions CpuThreshold { get; set; } = new() { Warning = 75, Critical = 90 };
    public ThresholdOptions MemoryThreshold { get; set; } = new() { Warning = 80, Critical = 95 };
    public ThresholdOptions DiskThreshold { get; set; } = new() { Warning = 85, Critical = 95 };
}

public class ThresholdOptions
{
    [Range(0, 100)]
    public double Warning { get; set; }
    
    [Range(0, 100)]
    public double Critical { get; set; }
}
```

### Configuration File Example
```json
{
  "SystemMonitoring": {
    "MeasurementWindowMs": 2000,
    "MonitorAllProcesses": false,
    "CollectionIntervalSeconds": 30,
    "CpuThreshold": {
      "Warning": 75.0,
      "Critical": 90.0
    },
    "MemoryThreshold": {
      "Warning": 80.0,
      "Critical": 95.0
    },
    "DiskThreshold": {
      "Warning": 85.0,
      "Critical": 95.0
    }
  }
}
```

## Advanced Usage Scenarios

### Custom Metric Clients
```csharp
// Extend with custom metrics
public static class CustomSystemResourceMonitorExtensions
{
    public static IServiceCollection AddSystemResourceMonitorWithNetworking(this IServiceCollection services)
    {
        // Add standard system monitoring
        services.AddSystemResourceMonitor();
        
        // Add custom network monitoring
        services.TryAddSingleton<NetworkMetricsClient>();
        
        // Replace with enhanced implementation
        services.Replace(ServiceDescriptor.Singleton<ISystemResourceMonitor, EnhancedSystemResourceMonitorImpl>());
        
        return services;
    }
}
```

### Testing Configuration
```csharp
// Test-specific setup
public class SystemResourceMonitorTestFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public SystemResourceMonitorTestFixture()
    {
        var services = new ServiceCollection();
        
        // Add system resource monitoring
        services.AddSystemResourceMonitor();
        
        // Add test-specific services
        services.AddLogging(builder => builder.AddConsole());
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public ISystemResourceMonitor GetMonitor() => ServiceProvider.GetRequiredService<ISystemResourceMonitor>();
}

// Usage in tests
[Test]
public void CanCollectSystemMetrics()
{
    var fixture = new SystemResourceMonitorTestFixture();
    var monitor = fixture.GetMonitor();
    
    var metrics = monitor.GetMetrics(window: 1000, all: false);
    
    Assert.That(metrics, Is.Not.Null);
    Assert.That(metrics.Cpu.ProcessorCount, Is.GreaterThan(0));
    Assert.That(metrics.Memory.Total, Is.GreaterThan(0));
    Assert.That(metrics.Drives.Length, Is.GreaterThan(0));
}
```

## Related Components

- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregated metrics model
- **[CPU Metrics](Metrics/Cpu/CpuMetrics.md)** - CPU performance metrics
- **[Memory Metrics](Metrics/Memory/MemoryMetrics.md)** - Memory utilization metrics
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Best Practices

### Service Registration
1. **Single Registration**: Call `AddSystemResourceMonitor()` only once per application
2. **Early Registration**: Register monitoring services early in the service configuration
3. **Configuration Validation**: Use data annotations to validate configuration options
4. **Environment Awareness**: Adjust configuration based on deployment environment

### Performance Considerations
1. **Singleton Lifetime**: Services are registered as singletons for optimal performance
2. **Resource Efficiency**: Monitoring has minimal impact on application performance
3. **Measurement Windows**: Use appropriate CPU measurement windows (1-5 seconds)
4. **Collection Frequency**: Balance monitoring needs with resource usage

### Integration Guidelines
1. **Health Checks**: Include system resource health checks for operational monitoring
2. **Background Services**: Use hosted services for continuous monitoring
3. **Logging Integration**: Include system metrics in application logs
4. **Alerting**: Connect monitoring to alerting systems for threshold violations

## Troubleshooting

### Common Issues
1. **Service Registration**: Ensure `AddSystemResourceMonitor()` is called before building the service provider
2. **Configuration Errors**: Validate configuration options using data annotations
3. **Permission Issues**: Some metrics may require elevated privileges on certain systems
4. **Resource Conflicts**: Monitor the monitoring overhead in high-frequency scenarios

### Diagnostic Tips
```csharp
// Verify service registration
public void ValidateServiceRegistration(IServiceProvider serviceProvider)
{
    var monitor = serviceProvider.GetService<ISystemResourceMonitor>();
    if (monitor == null)
    {
        throw new InvalidOperationException("ISystemResourceMonitor not registered. Call AddSystemResourceMonitor().");
    }
    
    // Test basic functionality
    var metrics = monitor.GetMetrics(1000, false);
    Console.WriteLine($"System monitoring working: CPU {metrics.Cpu.Usage:F1}%, Memory {metrics.Memory.UsagePercentage:F1}%");
}
```