# ActiveMQHealthCheckOptions

## Overview

The `ActiveMQHealthCheckOptions` class provides configuration options for the ActiveMQ health check implementation. This class encapsulates all necessary connection parameters and settings required to perform health checks against Apache ActiveMQ message brokers.

## Purpose

- **Configuration Management**: Centralized configuration for ActiveMQ health check parameters
- **Connection Settings**: Broker URI, authentication credentials, and client identification
- **Queue Configuration**: Target queue specification for health check operations
- **Flexibility**: Support for both authenticated and anonymous connections

## Class Declaration

```csharp
public sealed class ActiveMQHealthCheckOptions
{
    public string BrokerUri { get; set; } = null!;
    public string? ClientId { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string Queue { get; set; } = null!;
}
```

## Properties

### BrokerUri
- **Type**: `string`
- **Required**: Yes
- **Description**: The complete URI of the ActiveMQ broker to connect to
- **Example**: `"tcp://localhost:61616"`, `"ssl://activemq.example.com:61617"`

### ClientId
- **Type**: `string?`
- **Required**: No
- **Description**: Optional client identifier for the connection
- **Usage**: Useful for connection tracking and debugging

### UserName
- **Type**: `string?`
- **Required**: No
- **Description**: Username for authenticated connections
- **Security**: Used in conjunction with Password for broker authentication

### Password
- **Type**: `string?`
- **Required**: No
- **Description**: Password for authenticated connections
- **Security**: Should be handled securely in production environments

### Queue
- **Type**: `string`
- **Required**: Yes
- **Description**: Name of the queue to use for health check message operations
- **Example**: `"health.check"`, `"system.monitoring"`

## Usage Examples

### Basic Configuration
```csharp
var options = new ActiveMQHealthCheckOptions
{
    BrokerUri = "tcp://localhost:61616",
    Queue = "health.check"
};
```

### Authenticated Configuration
```csharp
var options = new ActiveMQHealthCheckOptions
{
    BrokerUri = "tcp://activemq.production.com:61616",
    ClientId = "health-checker-01",
    UserName = "monitoring_user",
    Password = "secure_password",
    Queue = "system.health"
};
```

### SSL Configuration
```csharp
var options = new ActiveMQHealthCheckOptions
{
    BrokerUri = "ssl://secure-activemq.example.com:61617",
    ClientId = "ssl-health-checker",
    UserName = "ssl_user",
    Password = "ssl_password",
    Queue = "secure.health.check"
};
```

## Configuration Best Practices

### Security Considerations
1. **Credential Management**: Store credentials securely using configuration providers or secret management systems
2. **SSL/TLS**: Use encrypted connections (`ssl://` or `stomp+ssl://`) for production environments
3. **Least Privilege**: Create dedicated user accounts with minimal necessary permissions

### Connection Settings
1. **Client ID**: Use descriptive client IDs for easier monitoring and debugging
2. **Queue Naming**: Use dedicated queues for health checks to avoid interference with business operations
3. **URI Format**: Ensure proper URI format including protocol, host, and port

### Performance Considerations
1. **Queue Selection**: Use lightweight, non-persistent queues for health checks
2. **Connection Pooling**: Consider connection reuse patterns for frequent health checks
3. **Timeout Settings**: Configure appropriate timeouts through the health check framework

## Integration

This options class is used by:
- **[ActiveMQHealthCheck](ActiveMQHealthCheck.md)** - The main health check implementation
- **[ActiveMQHealthCheckExtensions](ActiveMQHealthCheckExtensions.md)** - Dependency injection setup methods

## Related Components

- **[ActiveMQ Health Check](ActiveMQHealthCheck.md)** - Main health check implementation
- **[ActiveMQ Health Check Extensions](ActiveMQHealthCheckExtensions.md)** - Extension methods for DI registration
- **[Health Checks Overview](README.md)** - Complete health checks documentation

## Threading and Immutability

### Thread Safety
- Properties are not thread-safe for writes after initial configuration
- Should be configured once during application startup
- Read operations are safe across multiple threads

### Immutability Pattern
- Consider creating read-only instances after initial configuration
- Use configuration binding to populate from external sources
- Validate required properties during application startup

## Validation

### Required Properties
```csharp
public void Validate()
{
    if (string.IsNullOrWhiteSpace(BrokerUri))
        throw new ArgumentException("BrokerUri is required");
    
    if (string.IsNullOrWhiteSpace(Queue))
        throw new ArgumentException("Queue is required");
    
    if (!Uri.TryCreate(BrokerUri, UriKind.Absolute, out _))
        throw new ArgumentException("BrokerUri must be a valid URI");
}
```

### Configuration Binding
```csharp
// appsettings.json
{
  "HealthChecks": {
    "ActiveMQ": {
      "BrokerUri": "tcp://localhost:61616",
      "ClientId": "health-checker",
      "Queue": "health.check"
    }
  }
}

// Startup configuration
services.Configure<ActiveMQHealthCheckOptions>(
    configuration.GetSection("HealthChecks:ActiveMQ"));
```

## Error Handling

### Common Configuration Errors
1. **Invalid URI Format**: Ensure BrokerUri follows proper URI syntax
2. **Missing Required Properties**: Validate BrokerUri and Queue are provided
3. **Authentication Mismatch**: Ensure credentials match broker configuration
4. **Network Connectivity**: Verify broker accessibility from health check location

### Troubleshooting Tips
1. **Connection Testing**: Test broker connectivity before deploying health checks
2. **Credential Validation**: Verify authentication settings with broker administrators
3. **Queue Permissions**: Ensure the configured user has send permissions on the target queue
4. **Firewall Configuration**: Verify network paths and port accessibility