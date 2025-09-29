# Objects Namespace

The **Objects** namespace provides foundational classes and patterns for building robust, maintainable .NET applications. It offers core infrastructure for compression, resource management, equality, immutability, and change notification - essential building blocks for modern application development.

## Namespace Overview

```csharp
namespace RapidStreamer.BuildingBlocks.Application.Objects
```

The Objects namespace contains five primary components that address fundamental object-oriented programming patterns:

- **[CompressedObject](CompressedObject.md)** - Efficient compressed data container with multiple format support
- **[DisposableObject](DisposableObject.md)** - Comprehensive resource management with dual disposal patterns
- **[EquatableObject](EquatableObject.md)** - Reflection-based value equality for value objects
- **[ImmutableObject](ImmutableObject.md)** - Runtime-enforced immutability with performance optimization
- **[NotifiableObject](NotifiableObject.md)** - Change notification infrastructure for reactive programming

## Architectural Philosophy

### Design Principles

The Objects namespace follows these core design principles:

1. **Composition Over Inheritance**: Classes can be composed to provide multiple behaviors
2. **Single Responsibility**: Each class addresses one specific concern
3. **Performance Optimization**: Built-in caching and efficient implementations
4. **Type Safety**: Strong typing and compile-time validation where possible
5. **Extensibility**: Virtual methods and abstract bases for customization

### Inheritance Hierarchy

```
object
├── EquatableObject<T> : IEquatable<T>
│   ├── EquatableObject
│   ├── DisposableObject : IDisposable, IAsyncDisposable
│   └── ImmutableObject<T> (inherits caching and validation)
│       └── ImmutableObject
├── NotifiableObject (standalone base for notifications)
└── CompressedObject (readonly struct)
```

### Integration Patterns

The Objects classes are designed to work together and with other BuildingBlocks components:

```csharp
// Example: Combining multiple patterns
public class Product : DisposableObject, INotifyPropertyChanged
{
    // Inherits: Equality from EquatableObject, Disposal from DisposableObject
    // Implements: Change notification manually or through composition
}

public class ImmutableConfiguration : ImmutableObject<ImmutableConfiguration>
{
    // Inherits: Equality + Immutability validation + Performance optimization
}

public class CompressedData
{
    public CompressedObject Data { get; set; } // Composition for compression
}
```

## Core Components

### CompressedObject - Data Compression

A readonly struct for efficient compressed data storage and transfer.

**Key Features:**
- Multiple compression formats (GZip, Deflate, Brotli, BZip2)
- Implicit conversions between byte arrays and Base64 strings
- Memory-efficient struct design
- Integration with serialization helpers

**Primary Use Cases:**
- API data transfer optimization
- Database storage compression
- Caching compressed objects
- File processing pipelines

```csharp
// Basic usage
string data = "Large text content...";
CompressedObject compressed = data.ToByteArray().ToCompressed(CompressionType.Brotli);
string base64 = compressed; // Implicit conversion for storage
```

### DisposableObject - Resource Management

Abstract base class implementing comprehensive disposal patterns.

**Key Features:**
- Dual disposal support (IDisposable + IAsyncDisposable)
- Lifecycle events (Disposing, Disposed)
- Thread-safe implementation
- Anonymous disposable factory methods
- Finalizer protection

**Primary Use Cases:**
- Database connections and transactions
- File and stream management
- Unmanaged resource cleanup
- Composite resource disposal

```csharp
public class DatabaseService : DisposableObject
{
    protected override void DisposeManagedResources()
    {
        _connection?.Dispose();
        base.DisposeManagedResources();
    }
    
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
        await base.DisposeManagedResourcesAsync();
    }
}
```

### EquatableObject - Value Equality

Abstract base class providing reflection-based value equality.

**Key Features:**
- Automatic field and property comparison
- IgnoreMemberAttribute support for exclusions
- Complete equality implementation (IEquatable<T>, operators, GetHashCode)
- Performance-optimized hash code generation

**Primary Use Cases:**
- Value objects and DTOs
- Dictionary keys and set members
- Domain model value types
- Comparison-heavy scenarios

```csharp
public class Address : EquatableObject<Address>
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }
    
    [IgnoreMember] // Excluded from equality
    public DateTime LastValidated { get; set; }
}
```

### ImmutableObject - Immutability Enforcement

Abstract base class enforcing runtime immutability with performance optimization.

**Key Features:**
- Runtime validation of immutability constraints
- Cached atomic values and hash codes
- Thread-safe by design
- Value equality inheritance from EquatableObject
- Performance optimization through caching

**Primary Use Cases:**
- Configuration objects
- Command and event objects
- Value objects requiring immutability guarantees
- Thread-safe shared state

```csharp
public class Configuration : ImmutableObject<Configuration>
{
    public string ConnectionString { get; }
    public int MaxConnections { get; }
    public TimeSpan Timeout { get; }
    
    public Configuration(string connectionString, int maxConnections, TimeSpan timeout)
    {
        ConnectionString = connectionString;
        MaxConnections = maxConnections;
        Timeout = timeout;
        // Validation happens in base constructor
    }
    
    public Configuration WithTimeout(TimeSpan newTimeout) =>
        new(ConnectionString, MaxConnections, newTimeout);
}
```

### NotifiableObject - Change Notification

Abstract base class providing change notification infrastructure.

**Key Features:**
- NotifiableChangeType enumeration (Added, Modified, Removed)
- Foundation for INotifyPropertyChanged implementations
- MVVM and data binding support
- Observer pattern infrastructure

**Primary Use Cases:**
- MVVM ViewModels
- Data binding scenarios
- Observable collections
- Reactive programming patterns

```csharp
public class ProductViewModel : NotifiableObject, INotifyPropertyChanged
{
    private string _name = string.Empty;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }
}
```

## Integration Scenarios

### Scenario 1: Entity with Full Feature Set

```csharp
public class AuditableEntity : DisposableObject, INotifyPropertyChanged
{
    private string _name = string.Empty;
    private DateTime _lastModified;
    private readonly NotifiableCollection<AuditLogEntry> _auditLog = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public DateTime LastModified
    {
        get => _lastModified;
        private set => SetProperty(ref _lastModified, value);
    }
    
    [IgnoreMember] // Don't include in equality comparison
    public IReadOnlyCollection<AuditLogEntry> AuditLog => _auditLog;
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            var oldValue = field;
            field = value;
            LastModified = DateTime.UtcNow;
            
            // Add audit entry
            _auditLog.Add(new AuditLogEntry(propertyName!, NotifiableChangeType.Modified, oldValue, value));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }
    
    protected override void DisposeManagedResources()
    {
        _auditLog.Clear();
        base.DisposeManagedResources();
    }
}

public class AuditLogEntry : ImmutableObject<AuditLogEntry>
{
    public string PropertyName { get; }
    public NotifiableObject.NotifiableChangeType ChangeType { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public DateTime Timestamp { get; }
    
    public AuditLogEntry(string propertyName, NotifiableObject.NotifiableChangeType changeType, 
                        object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        Timestamp = DateTime.UtcNow;
    }
}
```

### Scenario 2: Compressed Immutable Messages

```csharp
public class CompressedMessage : ImmutableObject<CompressedMessage>
{
    public Guid Id { get; }
    public string MessageType { get; }
    public CompressedObject CompressedPayload { get; }
    public DateTime CreatedAt { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    
    private readonly Dictionary<string, string> _headersDict;
    
    public CompressedMessage(string messageType, object payload, IDictionary<string, string>? headers = null)
    {
        Id = Guid.NewGuid();
        MessageType = messageType;
        CreatedAt = DateTime.UtcNow;
        
        // Serialize and compress payload
        var json = payload.ToJson();
        var bytes = Encoding.UTF8.GetBytes(json);
        CompressedPayload = bytes.ToCompressed(CompressionType.Brotli);
        
        _headersDict = headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new();
        Headers = _headersDict.AsReadOnly();
    }
    
    public T? DeserializePayload<T>()
    {
        var json = CompressedPayload.DecompressString(CompressionType.Brotli);
        return json.FromJson<T>();
    }
    
    public CompressedMessage WithHeader(string key, string value)
    {
        var newHeaders = new Dictionary<string, string>(_headersDict) { [key] = value };
        
        // Create new message with same payload but different headers
        var decompressed = DeserializePayload<object>();
        return new CompressedMessage(MessageType, decompressed!, newHeaders);
    }
}
```

### Scenario 3: Observable Configuration with Validation

```csharp
public class ApplicationSettings : NotifiableObject, INotifyPropertyChanged, IDataErrorInfo
{
    private string _databaseConnection = string.Empty;
    private int _maxConnections = 100;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private bool _enableLogging = true;
    private readonly Dictionary<string, string> _validationErrors = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string DatabaseConnection
    {
        get => _databaseConnection;
        set => SetProperty(ref _databaseConnection, value);
    }
    
    public int MaxConnections
    {
        get => _maxConnections;
        set => SetProperty(ref _maxConnections, value);
    }
    
    public TimeSpan Timeout
    {
        get => _timeout;
        set => SetProperty(ref _timeout, value);
    }
    
    public bool EnableLogging
    {
        get => _enableLogging;
        set => SetProperty(ref _enableLogging, value);
    }
    
    public string Error => _validationErrors.Count > 0 ? "Validation errors exist" : string.Empty;
    
    public string this[string columnName] => _validationErrors.TryGetValue(columnName, out var error) ? error : string.Empty;
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            ValidateProperty(propertyName!, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
            return true;
        }
        return false;
    }
    
    private void ValidateProperty(string propertyName, object? value)
    {
        _validationErrors.Remove(propertyName);
        
        switch (propertyName)
        {
            case nameof(DatabaseConnection):
                if (string.IsNullOrWhiteSpace(value as string))
                    _validationErrors[propertyName] = "Database connection is required";
                break;
                
            case nameof(MaxConnections):
                if (value is int max && (max < 1 || max > 1000))
                    _validationErrors[propertyName] = "Max connections must be between 1 and 1000";
                break;
                
            case nameof(Timeout):
                if (value is TimeSpan timeout && timeout.TotalSeconds < 1)
                    _validationErrors[propertyName] = "Timeout must be at least 1 second";
                break;
        }
    }
    
    public ImmutableConfiguration ToImmutableConfiguration()
    {
        if (_validationErrors.Count > 0)
            throw new InvalidOperationException("Cannot create immutable configuration with validation errors");
            
        return new ImmutableConfiguration(DatabaseConnection, MaxConnections, Timeout, EnableLogging);
    }
}

public class ImmutableConfiguration : ImmutableObject<ImmutableConfiguration>
{
    public string DatabaseConnection { get; }
    public int MaxConnections { get; }
    public TimeSpan Timeout { get; }
    public bool EnableLogging { get; }
    
    public ImmutableConfiguration(string databaseConnection, int maxConnections, TimeSpan timeout, bool enableLogging)
    {
        DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        MaxConnections = maxConnections > 0 ? maxConnections : throw new ArgumentException("Must be positive", nameof(maxConnections));
        Timeout = timeout.TotalSeconds >= 1 ? timeout : throw new ArgumentException("Must be at least 1 second", nameof(timeout));
        EnableLogging = enableLogging;
    }
}
```

## Performance Considerations

### Memory Efficiency

```csharp
// CompressedObject - Struct design minimizes heap allocations
CompressedObject compressed = largeData.ToCompressed(); // No boxing

// ImmutableObject - Cached values prevent repeated reflection
var config = new ImmutableConfiguration(...); // Values cached after construction
var hash1 = config.GetHashCode(); // Uses cached hash
var hash2 = config.GetHashCode(); // Returns same cached hash

// EquatableObject - Efficient equality comparison
var addr1 = new Address("123 Main", "City", "12345");
var addr2 = new Address("123 Main", "City", "12345");
bool equal = addr1 == addr2; // Optimized reflection-based comparison
```

### Threading Considerations

```csharp
// ImmutableObject - Thread-safe by design
var sharedConfig = new ImmutableConfiguration(...);
// Safe to access from multiple threads without synchronization

// DisposableObject - Thread-safe disposal
public class ThreadSafeService : DisposableObject
{
    private readonly object _lock = new();
    
    protected override void DisposeManagedResources()
    {
        lock (_lock) // Ensure thread-safe cleanup
        {
            // Cleanup logic
        }
        base.DisposeManagedResources();
    }
}

// NotifiableObject - Consider synchronization context
public class ThreadSafeNotifiable : NotifiableObject, INotifyPropertyChanged
{
    private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
        }
        else
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

## Testing Strategies

### Unit Testing Approaches

```csharp
[TestFixture]
public class ObjectsIntegrationTests
{
    [Test]
    public void ImmutableConfiguration_ShouldCacheHashCode()
    {
        var config = new ImmutableConfiguration("conn", 100, TimeSpan.FromSeconds(30), true);
        
        var hash1 = config.GetHashCode();
        var hash2 = config.GetHashCode();
        
        Assert.That(hash1, Is.EqualTo(hash2));
    }
    
    [Test]
    public void CompressedMessage_ShouldRoundTripCorrectly()
    {
        var originalData = new { Name = "Test", Value = 42 };
        var message = new CompressedMessage("TestMessage", originalData);
        
        var roundTripped = message.DeserializePayload<dynamic>();
        
        Assert.That(roundTripped.Name, Is.EqualTo("Test"));
        Assert.That(roundTripped.Value, Is.EqualTo(42));
    }
    
    [Test]
    public async Task DisposableService_ShouldDisposeCleanly()
    {
        var service = new DatabaseService();
        
        Assert.That(service.IsDisposed, Is.False);
        
        await service.DisposeAsync();
        
        Assert.That(service.IsDisposed, Is.True);
    }
    
    [Test]
    public void NotifiableEntity_ShouldTrackChanges()
    {
        var entity = new AuditableEntity();
        var propertyChanges = new List<string>();
        
        entity.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName!);
        
        entity.Name = "Test";
        entity.Name = "Updated";
        
        Assert.That(propertyChanges, Contains.Item(nameof(entity.Name)));
        Assert.That(entity.AuditLog.Count, Is.EqualTo(2));
    }
}
```

## Best Practices

### 1. Choose the Right Base Class

```csharp
// Value objects - Use EquatableObject or ImmutableObject
public class Money : ImmutableObject<Money> { }

// Entities - Use DisposableObject if resources need cleanup
public class DbContext : DisposableObject { }

// ViewModels - Use NotifiableObject for UI binding
public class CustomerViewModel : NotifiableObject, INotifyPropertyChanged { }

// Data containers - Use CompressedObject for large data
public class ApiResponse 
{
    public CompressedObject Data { get; set; }
}
```

### 2. Implement Proper Validation

```csharp
public class ValidatedImmutableObject : ImmutableObject<ValidatedImmutableObject>
{
    public string Email { get; }
    
    public ValidatedImmutableObject(string email)
    {
        Email = ValidateEmail(email); // Validate before assignment
    }
    
    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Invalid email", nameof(email));
        return email.Trim().ToLowerInvariant();
    }
}
```

### 3. Use Composition When Appropriate

```csharp
public class CompositeService
{
    private readonly IDisposable _resources = DisposableObject.Create(() => CleanupResources());
    private readonly CompressedObject _cachedData;
    
    public void Dispose() => _resources.Dispose();
    
    private static void CleanupResources() { /* cleanup logic */ }
}
```

### 4. Handle Async Disposal Properly

```csharp
public class AsyncService : DisposableObject
{
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        // Dispose async resources first
        await CleanupAsyncResources();
        
        // Then call base
        await base.DisposeManagedResourcesAsync();
    }
    
    private async Task CleanupAsyncResources()
    {
        // Async cleanup logic
    }
}
```

## Integration with Other BuildingBlocks

### Helpers Integration

```csharp
// String and serialization helpers
var jsonData = myObject.ToJson();
var compressed = jsonData.ToByteArray().ToCompressed();
var base64 = compressed.ToString(); // For storage

// ObjectHelper integration
var size = await Size.Calculate(myImmutableObject);
var cloned = myDisposableObject.DeepClone();
```

### Attributes Integration

```csharp
public class AttributeIntegratedObject : EquatableObject<AttributeIntegratedObject>
{
    public string Name { get; }
    
    [IgnoreMember] // Excluded from equality comparison
    public DateTime CreatedAt { get; }
    
    [JsonSerialization(JsonSerializationAttribute.JsonSerializationType.Ignore)]
    public string InternalId { get; }
}
```

### ChangeTracking Integration

```csharp
public class TrackedEntity : DisposableObject, IChangeTrackingSupport
{
    private readonly ChangeTrackingObject _changeTracker = new();
    
    public IChangeTracker ChangeTracker => _changeTracker;
    
    protected override void DisposeManagedResources()
    {
        _changeTracker.Dispose();
        base.DisposeManagedResources();
    }
}
```

## Migration Strategies

### From Legacy Objects

```csharp
// Legacy class
public class LegacyCustomer
{
    public string Name { get; set; }
    public string Email { get; set; }
}

// Migrated with proper patterns
public class Customer : EquatableObject<Customer>, INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, ValidateEmail(value));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }
    
    private static string ValidateEmail(string email)
    {
        // Validation logic
        return email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
    }
    
    // Factory method for migration
    public static Customer FromLegacy(LegacyCustomer legacy)
    {
        return new Customer { Name = legacy.Name, Email = legacy.Email };
    }
}
```

## See Also

- **Helper Classes**: [ObjectHelper](../Helpers/ObjectHelper.md), [StringHelper](../Helpers/StringHelper.md), [JsonHelper](../Helpers/JsonHelper.md)
- **Attributes**: [IgnoreMemberAttribute](../Attributes/IgnoreMemberAttribute.md), [JsonSerializationAttribute](../Attributes/JsonSerializationAttribute.md)
- **Change Tracking**: [ChangeTrackingObject](../ChangeTrackingItems/ChangeTrackingObject.md), [ChangeType](../ChangeTrackingItems/ChangeType.md)
- **Collections**: [BindingDictionary](../Collections/BindingDictionary.md), [LinkedArray](../Collections/LinkedArray.md)

---

*The Objects namespace provides essential foundational patterns for building robust, maintainable, and performant .NET applications with proper resource management, equality semantics, immutability guarantees, and change notification infrastructure.*