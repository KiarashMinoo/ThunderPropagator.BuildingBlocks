# Application Building Blocks

Core application-level building blocks providing essential functionality for building robust applications with concurrency, data management, serialization, security, and more.

## Core Components

### Essential Components
- **ConcurrentStringBuilder** - Thread-safe string builder for high-performance concurrent scenarios
- **DispatcherTimer** - Enhanced timer implementation with dispatcher support
- **FeederMessage** - Standardized message structure for data feeding operations
- **Telemetry** - Comprehensive telemetry and monitoring capabilities
- **ExceptionInfo** - Detailed exception information and metadata
- **InconvertibleException** - Exception for type conversion failures
- **ICloneable** - Enhanced cloning interface with deep copy support
- **IConvertible** - Extended type conversion capabilities
- **ServiceConfiguration** - Service configuration management and validation

## Specialized Modules

### 🏷️ [Attributes](Attributes/README.md)
Custom attributes for metadata and serialization control with comprehensive JSON serialization capabilities and reflection management.

### 🔐 [Certificate Management](Certificate/README.md)
X.509 certificate handling and security operations for authentication, HTTPS clients, and monitoring services.

### 📊 [Change Tracking](ChangeTrackingItems/README.md)
Comprehensive change tracking framework with thread-safe collections, immutable change records, and audit trail capabilities.

### 🔒 [Cryptography & Security](Ciphering/README.md)
Complete cryptographic toolkit including AES encryption, RSA encryption, and secure password generation with hybrid encryption patterns.

### 📦 [Collections](Collections/README.md)
High-performance collection types including observable dictionaries, ordered dictionaries, and memory-efficient arrays with zero-copy operations.

### 🔗 [Correlation Support](CorrelationId/README.md)
Request correlation capabilities for distributed systems with unique identifier generation and fluent management APIs.

### 📋 [Enumerations](Enums/README.md)
Common enumeration types including authentication types, data types, cast types, and recovery storage options.

### 🛠️ [Helper Utilities](Helpers/README.md)
Comprehensive utility classes for collections, serialization, validation, configuration management, and data manipulation with high-performance implementations.

### 👤 [Identity Management](Identity/README.md)
Authentication and authorization components including JWT configuration and basic user configuration models.

### 🎯 [Object Models](Objects/README.md)
Foundational object patterns including compressed objects, disposable base classes, equatable objects, immutable objects, and property change notification.

### 📄 [Serialization](Serializations/README.md)
Serialization abstractions and implementations supporting JSON, YAML, Kafka, and custom serializer types with high-performance operations.

## Quick Start

### Basic Usage
```csharp
using RapidStreamer.BuildingBlocks.Application;

// Thread-safe string operations
var builder = new ConcurrentStringBuilder();
await builder.AppendLineAsync("Processing...");

// Message processing with correlation
var message = new FeederMessage { Data = "payload" }
    .GenerateCorrelationId();

// Configuration management
var config = new ServiceConfiguration
{
    ServiceName = "OrderProcessor",
    Settings = new Dictionary<string, object>()
};
```

### Advanced Integration
```csharp
// Combine multiple building blocks
public class OrderService : DisposableObject, ICorrelationIdSupport
{
    private readonly ChangeTrackingObjectAdapter<string, object> _changeTracker = new();
    private readonly BindingDictionary<string, object> _settings = new();
    
    public string CorrelationId { get; set; } = string.Empty;
    
    public async Task ProcessOrderAsync(Order order)
    {
        // Generate correlation ID
        this.GenerateCorrelationId();
        
        // Track changes
        _changeTracker.BeginTracking();
        
        // Process with telemetry
        using var activity = Telemetry.StartActivity("ProcessOrder");
        
        // Business logic here
        await ProcessOrderInternalAsync(order);
        
        // Get audit trail
        var changes = _changeTracker.EndTracking();
    }
}
```

## Architecture Patterns

### Reactive Programming
```csharp
// Observable collections with change notifications
var settings = new BindingDictionary<string, object>();
settings.ValueChanged += (sender, key, value, changeType) =>
    Console.WriteLine($"Setting {key} changed to {value}");

// Property change notification
public class ViewModel : NotifiableObject
{
    private string _status = string.Empty;
    
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
```

### Security Integration
```csharp
// Hybrid encryption for secure data
public class SecureDataProcessor
{
    public (string EncryptedData, string EncryptedKey) SecureData(string data, string publicKey)
    {
        var password = PasswordGenerator.Generate(32);
        var aesKey = EncryptionService.CreateKey(password);
        
        var encryptedData = EncryptionService.Encrypt(data, aesKey);
        var encryptedKey = RsaEncryptionService.Encrypt(password, publicKey, 2048);
        
        return (encryptedData, encryptedKey);
    }
}
```

### Data Management
```csharp
// Immutable configuration with compression
public class AppConfig : ImmutableObject<AppConfig>
{
    public string ConnectionString { get; init; } = string.Empty;
    public CompressedObject Settings { get; init; }
    
    public AppConfig(Dictionary<string, object> settings)
    {
        var json = JsonHelper.Serialize(settings);
        Settings = json.ToByteArray().ToCompressed(CompressionType.Brotli);
        MarkAsInitialized();
    }
    
    public T GetSetting<T>(string key)
    {
        var json = Settings.Data.ToUtf8String();
        var dict = JsonHelper.Deserialize<Dictionary<string, object>>(json);
        return (T)dict[key];
    }
}
```

## Integration Guidelines

### Dependency Injection
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register building block services
    services.AddSingleton<ITelemetryService, TelemetryService>();
    services.AddScoped<IChangeTrackingService, ChangeTrackingService>();
    services.AddTransient<ICorrelationIdSupport, CorrelationIdSupport>();
}
```

### ASP.NET Core Integration
```csharp
public void Configure(IApplicationBuilder app)
{
    // Add correlation ID middleware
    app.UseMiddleware<CorrelationIdMiddleware>();
    
    // Add telemetry tracking
    app.UseMiddleware<TelemetryMiddleware>();
}
```

### Configuration
```csharp
public class BuildingBlocksConfiguration
{
    public TelemetrySettings Telemetry { get; set; } = new();
    public EncryptionSettings Encryption { get; set; } = new();
    public CollectionSettings Collections { get; set; } = new();
    public ChangeTrackingSettings ChangeTracking { get; set; } = new();
}
```

## Performance Characteristics

- **High Throughput**: Optimized for processing thousands of operations per second
- **Memory Efficient**: Zero-copy operations and minimal allocations where possible
- **Thread Safe**: All components designed for concurrent access
- **Scalable**: Designed to handle enterprise-scale workloads

## Best Practices

1. **Use correlation IDs** for all distributed operations
2. **Implement change tracking** for audit-sensitive operations
3. **Leverage helper utilities** to reduce boilerplate code
4. **Apply security patterns** for sensitive data processing
5. **Use observable collections** for reactive UI scenarios
6. **Implement proper disposal** for resource management

For specific implementation details and examples, refer to the individual component documentation files.

## Related Documentation

### Infrastructure Components
- **[Infrastructure Building Blocks](../Infrastructure/README.md)** - Infrastructure-level components
  - **[Health Checks](../Infrastructure/HealthChecks/README.md)** - Health monitoring capabilities
  - **[System Resource Monitor](../Infrastructure/SystemResourceMonitor/README.md)** - System performance monitoring
  - **[System Components](../Infrastructure/System/README.md)** - Network performance monitoring

### Development Resources
- **[Project Overview](../../ReadMe.md)** - Complete project documentation
- **[Documentation Guidelines](../README.md)** - Documentation standards and patterns

### Component Quick Links
- **[🏷️ Attributes](Attributes/README.md#custom-attributes)** - Metadata and serialization control
- **[🔐 Certificate Management](Certificate/README.md#x509-certificate-handling)** - Security operations
- **[📊 Change Tracking](ChangeTrackingItems/README.md#change-tracking-framework)** - Audit trail capabilities
- **[🔒 Cryptography](Ciphering/README.md#cryptographic-operations)** - Encryption and security
- **[📦 Collections](Collections/README.md#high-performance-collections)** - Specialized collection types
- **[🔗 Correlation ID](CorrelationId/README.md#correlation-id-management)** - Request tracing
- **[📋 Enumerations](Enums/README.md#common-enumerations)** - Shared enumeration types
- **[🛠️ Helper Utilities](Helpers/README.md#utility-classes)** - Configuration and data helpers
- **[👤 Identity Management](Identity/README.md#authentication-components)** - JWT and user management
- **[🎯 Object Models](Objects/README.md#foundational-patterns)** - Base object patterns
- **[📄 Serialization](Serializations/README.md#serialization-utilities)** - JSON and YAML processing