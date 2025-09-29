# ActiveMQHealthCheckExtensions

## Overview

The `ActiveMQHealthCheckExtensions` class provides extension methods for the `IHealthChecksBuilder` interface to simplify registration and configuration of ActiveMQ health checks in .NET applications. This static class follows the standard .NET extension pattern for dependency injection configuration.

## Purpose

- **Simplified Registration**: Easy health check registration with fluent API
- **Dependency Injection**: Proper service registration and lifetime management
- **Configuration Flexibility**: Support for various configuration patterns
- **Framework Integration**: Seamless integration with .NET health check framework

## Class Declaration

```csharp
public static class ActiveMQHealthCheckExtensions
{
    public static IHealthChecksBuilder AddActiveMQHealthCheck(
        this IHealthChecksBuilder builder,
        ActiveMQHealthCheckOptions options,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        // Implementation
    }
}
```

## Extension Method

### AddActiveMQHealthCheck

Registers the ActiveMQ health check with the dependency injection container and configures it for use within the health check framework.

#### Parameters

- **builder**: `IHealthChecksBuilder` - The health checks builder to extend
- **options**: `ActiveMQHealthCheckOptions` - Configuration options for the health check
- **name**: `string?` - Optional name for the health check (defaults to "ActiveMQHealthCheck")
- **failureStatus**: `HealthStatus?` - Health status to return on failure (defaults to Unhealthy)
- **tags**: `IEnumerable<string>?` - Optional tags for categorizing the health check
- **timeout**: `TimeSpan?` - Optional timeout for the health check execution

#### Return Value
Returns the `IHealthChecksBuilder` to enable method chaining.

## Usage Examples

### Basic Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://localhost:61616",
            Queue = "health.check"
        });
}
```

### Comprehensive Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(
            options: new ActiveMQHealthCheckOptions
            {
                BrokerUri = "tcp://activemq.production.com:61616",
                ClientId = "web-api-health-checker",
                UserName = "monitoring_user",
                Password = "secure_password",
                Queue = "system.health.web"
            },
            name: "ActiveMQ Production Broker",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "messaging", "activemq", "critical" },
            timeout: TimeSpan.FromSeconds(15)
        );
}
```

### Multiple Health Checks
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(
            new ActiveMQHealthCheckOptions
            {
                BrokerUri = "tcp://activemq-primary.com:61616",
                Queue = "health.primary"
            },
            name: "Primary ActiveMQ",
            tags: new[] { "messaging", "primary" })
        .AddActiveMQHealthCheck(
            new ActiveMQHealthCheckOptions
            {
                BrokerUri = "tcp://activemq-secondary.com:61616",
                Queue = "health.secondary"
            },
            name: "Secondary ActiveMQ",
            tags: new[] { "messaging", "secondary", "backup" });
}
```

### Configuration-Based Setup
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Load from appsettings.json
    var activeMQOptions = new ActiveMQHealthCheckOptions();
    Configuration.GetSection("HealthChecks:ActiveMQ").Bind(activeMQOptions);

    services.AddHealthChecks()
        .AddActiveMQHealthCheck(
            activeMQOptions,
            name: "Configured ActiveMQ",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "messaging", "configured" },
            timeout: TimeSpan.FromSeconds(10)
        );
}
```

## Implementation Details

### Service Registration
The extension method performs the following registrations:

1. **Options Registration**: Registers the `ActiveMQHealthCheckOptions` as a singleton
2. **Health Check Registration**: Adds the `ActiveMQHealthCheck` to the health checks builder
3. **Configuration Binding**: Associates configuration with the health check instance

```csharp
builder.Services.TryAddSingleton(options);
builder.AddCheck<ActiveMQHealthCheck>(name ?? nameof(ActiveMQHealthCheck), failureStatus, tags, timeout);
```

### Service Lifetime Management
- **Options**: Registered as `Singleton` for configuration immutability
- **Health Check**: Managed by the health check framework (typically scoped)
- **Dependencies**: Properly injected through constructor injection

## Configuration Patterns

### Environment-Specific Configuration
```csharp
// Development
services.AddHealthChecks()
    .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
    {
        BrokerUri = "tcp://localhost:61616",
        Queue = "dev.health"
    }, 
    name: "Development ActiveMQ");

// Production
services.AddHealthChecks()
    .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
    {
        BrokerUri = "tcp://prod-activemq.company.com:61616",
        ClientId = "prod-health-checker",
        UserName = Configuration["ActiveMQ:Username"],
        Password = Configuration["ActiveMQ:Password"],
        Queue = "prod.health"
    },
    name: "Production ActiveMQ",
    failureStatus: HealthStatus.Unhealthy,
    timeout: TimeSpan.FromSeconds(30));
```

### Configuration File Integration
```json
// appsettings.json
{
  "HealthChecks": {
    "ActiveMQ": {
      "BrokerUri": "tcp://activemq.example.com:61616",
      "ClientId": "health-checker-web",
      "UserName": "health_user",
      "Password": "health_password",
      "Queue": "system.health"
    }
  }
}
```

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<ActiveMQHealthCheckOptions>(
        Configuration.GetSection("HealthChecks:ActiveMQ"));

    services.AddHealthChecks()
        .AddActiveMQHealthCheck(
            services.BuildServiceProvider()
                .GetRequiredService<IOptions<ActiveMQHealthCheckOptions>>().Value,
            name: "Configured ActiveMQ");
}
```

## Advanced Usage Scenarios

### Conditional Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks();

    // Only add ActiveMQ health check if enabled in configuration
    if (Configuration.GetValue<bool>("HealthChecks:ActiveMQ:Enabled"))
    {
        var options = Configuration.GetSection("HealthChecks:ActiveMQ")
            .Get<ActiveMQHealthCheckOptions>();
        
        services.AddHealthChecks()
            .AddActiveMQHealthCheck(options, "ActiveMQ");
    }
}
```

### Custom Failure Handling
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(
        activeMQOptions,
        name: "Critical ActiveMQ",
        failureStatus: HealthStatus.Unhealthy,  // Fail the entire application
        tags: new[] { "critical", "messaging" })
    .AddActiveMQHealthCheck(
        secondaryOptions,
        name: "Secondary ActiveMQ",
        failureStatus: HealthStatus.Degraded,  // Mark as degraded only
        tags: new[] { "secondary", "messaging" });
```

### Tag-Based Filtering
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(options1, tags: new[] { "messaging", "primary" })
    .AddActiveMQHealthCheck(options2, tags: new[] { "messaging", "secondary" })
    .AddActiveMQHealthCheck(options3, tags: new[] { "messaging", "analytics" });

// In controller or middleware, filter by tags
app.UseHealthChecks("/health/messaging", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("messaging")
});
```

## Integration with ASP.NET Core

### Health Check Endpoints
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Basic health check endpoint
    app.UseHealthChecks("/health");

    // Detailed health check with custom response
    app.UseHealthChecks("/health/detailed", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(x => new
                {
                    name = x.Key,
                    status = x.Value.Status.ToString(),
                    exception = x.Value.Exception?.Message,
                    duration = x.Value.Duration.ToString()
                })
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });

    // ActiveMQ-specific health checks
    app.UseHealthChecks("/health/activemq", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("activemq")
    });
}
```

### Background Service Integration
```csharp
public class HealthCheckBackgroundService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckBackgroundService> _logger;

    public HealthCheckBackgroundService(
        HealthCheckService healthCheckService,
        ILogger<HealthCheckBackgroundService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await _healthCheckService.CheckHealthAsync(stoppingToken);
            
            if (result.Status != HealthStatus.Healthy)
            {
                _logger.LogWarning("Health check failed: {Status}", result.Status);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

## Related Components

- **[ActiveMQ Health Check](ActiveMQHealthCheck.md)** - The main health check implementation
- **[ActiveMQ Health Check Options](ActiveMQHealthCheckOptions.md)** - Configuration settings
- **[Health Checks Overview](README.md)** - Complete health checks documentation

## Best Practices

### Configuration Management
1. **External Configuration**: Use configuration files or environment variables
2. **Secret Management**: Store sensitive data (passwords) securely
3. **Environment Isolation**: Different configurations per environment
4. **Validation**: Validate configuration at startup

### Performance Optimization
1. **Appropriate Timeouts**: Set realistic timeout values
2. **Resource Management**: Ensure proper cleanup of connections
3. **Frequency Control**: Balance monitoring frequency with resource usage
4. **Caching**: Consider caching strategies for expensive operations

### Monitoring and Alerting
1. **Comprehensive Tags**: Use meaningful tags for filtering and grouping
2. **Custom Endpoints**: Create specific endpoints for different audiences
3. **Alert Integration**: Connect health checks to monitoring systems
4. **Failure Analysis**: Log detailed information for troubleshooting

### Security Considerations
1. **Credential Protection**: Secure storage and transmission of credentials
2. **Network Security**: Use encrypted connections where appropriate
3. **Access Control**: Limit access to health check endpoints
4. **Audit Logging**: Track health check activities and failures

## Troubleshooting

### Common Registration Issues
1. **Missing Dependencies**: Ensure all required packages are installed
2. **Service Conflicts**: Check for duplicate service registrations
3. **Configuration Errors**: Validate configuration values at startup
4. **Lifetime Issues**: Verify proper service lifetime configuration

### Runtime Problems
1. **Connection Failures**: Check network connectivity and broker status
2. **Authentication Issues**: Verify credentials and permissions
3. **Timeout Problems**: Adjust timeout values for network conditions
4. **Resource Leaks**: Monitor connection and memory usage