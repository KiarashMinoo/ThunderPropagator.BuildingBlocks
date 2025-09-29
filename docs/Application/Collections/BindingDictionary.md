# BindingDictionary\<TKey, TValue>

`BindingDictionary<TKey, TValue>` is a powerful, observable dictionary implementation that combines the functionality of a standard dictionary with advanced features like change tracking, event notifications, concurrent access support, and seamless integration with data binding scenarios. This class is designed for applications that need to monitor and react to collection changes in real-time.

## Overview

The `BindingDictionary` extends the standard dictionary concept by providing:
- **Event-driven notifications** for all dictionary operations (add, remove, update, clear)
- **Change tracking capabilities** through integration with the ChangeTrackingItems system
- **Concurrent access support** with optional thread-safe operations
- **Advanced lookup methods** with flexible value retrieval patterns
- **Type-safe operations** with comprehensive interface support

## Key Features

### 1. Observable Operations
Every modification operation triggers appropriate events, making it perfect for UI binding and reactive programming patterns.

### 2. Concurrent Support
Optional thread-safe operations using `ConcurrentDictionary<TKey, TValue>` under the hood when enabled.

### 3. Change Tracking Integration
Seamless integration with the `IChangeTrackingObject<TKey, TValue>` interface for audit trails and undo functionality.

### 4. Advanced Value Retrieval
Multiple methods for safe value access: `GetValueOrDefault`, `GetValueOrNull`, `GetValueOrAdd`, etc.

### 5. Flexible Update Operations
Support for atomic `AddOrUpdate` operations with factory functions.

## Class Declaration

```csharp
[DebuggerDisplay("Count = {Count}")]
[Serializable]
public class BindingDictionary<TKey, TValue> :
    NotifiableObject,
    IDictionary,
    IDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, TValue>,
    IEquatable<IDictionary<TKey, TValue>>,
    IChangeTrackingObject<TKey, TValue>
    where TKey : notnull
```

## Constructor Options

```csharp
// Basic constructors
var dict1 = new BindingDictionary<string, int>();                           // Standard dictionary
var dict2 = new BindingDictionary<string, int>(concurrentSupport: true);   // Thread-safe version

// With initial capacity
var dict3 = new BindingDictionary<string, int>(capacity: 100);
var dict4 = new BindingDictionary<string, int>(capacity: 100, concurrentSupport: true);

// With custom comparer
var dict5 = new BindingDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var dict6 = new BindingDictionary<string, int>(capacity: 50, comparer: StringComparer.OrdinalIgnoreCase, concurrentSupport: true);

// From existing collections
var existingDict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
var dict7 = new BindingDictionary<string, int>(existingDict);
var dict8 = new BindingDictionary<string, int>(existingDict, concurrentSupport: true);

// From IEnumerable<KeyValuePair>
var keyValuePairs = new[] { new KeyValuePair<string, int>("x", 10), new KeyValuePair<string, int>("y", 20) };
var dict9 = new BindingDictionary<string, int>(keyValuePairs);
```

## Event System

### Available Events

```csharp
public event DictionaryCleared? Cleared;
public event DictionaryKeyChanged<TKey>? KeyChanged;
public event DictionaryValueChanged<TKey, TValue>? ValueChanged;
```

### Event Delegates

```csharp
public delegate void DictionaryCleared(object sender);
public delegate void DictionaryKeyChanged<in TKey>(object sender, TKey key, NotifiableObject.NotifiableChangeType changeType);
public delegate void DictionaryValueChanged<in TKey, in TValue>(object sender, TKey key, TValue value, NotifiableObject.NotifiableChangeType changeType);
```

## Usage Examples

### Basic Dictionary Operations

```csharp
using RapidStreamer.BuildingBlocks.Application.Collections;
using RapidStreamer.BuildingBlocks.Application.Objects;

// Create a binding dictionary
var userScores = new BindingDictionary<string, int>();

// Subscribe to events
userScores.KeyChanged += (sender, key, changeType) =>
    Console.WriteLine($"Key '{key}' was {changeType}");

userScores.ValueChanged += (sender, key, value, changeType) =>
    Console.WriteLine($"Value for '{key}' {changeType}: {value}");

userScores.Cleared += sender =>
    Console.WriteLine("Dictionary was cleared");

// Add items
userScores.Add("Alice", 100);      // Events: KeyChanged (Added), ValueChanged (Added)
userScores.Add("Bob", 85);

// Update items
userScores["Alice"] = 110;         // Events: ValueChanged (Modified)

// Remove items
userScores.Remove("Bob");          // Events: KeyChanged (Removed), ValueChanged (Removed)

// Clear all
userScores.Clear();                // Events: Cleared
```

### Thread-Safe Operations

```csharp
// Create thread-safe dictionary
var concurrentStats = new BindingDictionary<string, long>(concurrentSupport: true);

// Safe concurrent operations
Parallel.For(0, 1000, i =>
{
    var key = $"counter_{i % 10}";
    concurrentStats.AddOrUpdate(key, 1, current => current + 1);
});

// Check concurrent support
Console.WriteLine($"Concurrent support: {concurrentStats.ConcurrentSupport}");

// Thread-safe conditional update
string targetKey = "counter_5";
if (concurrentStats.TryGetValue(targetKey, out var currentValue))
{
    // Atomic update only if value matches expectation
    bool updated = concurrentStats.TryUpdate(targetKey, currentValue + 100, currentValue);
    Console.WriteLine($"Update successful: {updated}");
}
```

### Advanced Value Retrieval

```csharp
var productCatalog = new BindingDictionary<string, Product>();

// Safe value access patterns
Product? product1 = productCatalog.GetValueOrNull("PROD001");
Product product2 = productCatalog.GetValueOrDefault("PROD002", Product.Default);

// Get or add pattern
Product product3 = productCatalog.GetValueOrAdd("PROD003", () => new Product
{
    Id = "PROD003",
    Name = "New Product",
    Price = 0.0m
});

// Get or add with key-based factory
Product product4 = productCatalog.GetValueOrAdd("PROD004", productId => new Product
{
    Id = productId,
    Name = $"Generated Product {productId}",
    Price = 10.0m
});

// Exception-throwing access
try
{
    Product product5 = productCatalog.GetValue("NONEXISTENT"); // Throws KeyNotFoundException
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"Product not found: {ex.Message}");
}
```

### AddOrUpdate Operations

```csharp
var inventory = new BindingDictionary<string, int>();

// Simple add or update
int? previousQuantity = inventory.AddOrUpdate("WIDGET001", 50);
Console.WriteLine($"Previous quantity: {previousQuantity ?? 0}");

// Add or update with factories
int currentQuantity = inventory.AddOrUpdate("WIDGET002",
    addValueFactory: () => 25,                    // Value if key doesn't exist
    updateValueFactory: current => current + 10   // Transform existing value
);

// Add or update with key-based factories
int updatedQuantity = inventory.AddOrUpdate("WIDGET003",
    addValueFactory: key => int.Parse(key.Substring(6)), // Use key to generate initial value
    updateValueFactory: (key, current) => current + int.Parse(key.Substring(6)) // Key-aware update
);

// Conditional updates
if (inventory.TryGetValue("WIDGET001", out int existingQuantity))
{
    // Only update if current value matches expectation
    bool success = inventory.TryUpdate("WIDGET001", existingQuantity - 5, existingQuantity);
    if (success)
    {
        Console.WriteLine("Inventory decremented successfully");
    }
}
```

### Change Tracking Integration

```csharp
var trackedConfiguration = new BindingDictionary<string, string>();

// Begin tracking changes
bool trackingStarted = ((IChangeTrackingObject<string, string>)trackedConfiguration).BeginTracking();
Console.WriteLine($"Change tracking started: {trackingStarted}");

// Make some changes
trackedConfiguration["ServerUrl"] = "https://api.example.com";
trackedConfiguration["Timeout"] = "30";
trackedConfiguration["ApiKey"] = "secret123";

// Modify existing value
trackedConfiguration["Timeout"] = "60";

// Remove a value
trackedConfiguration.Remove("ApiKey");

// End tracking and get changes
var changes = ((IChangeTrackingObject<string, string>)trackedConfiguration).EndTracking();

// Process changes
foreach (var change in changes)
{
    var item = change.Value;
    Console.WriteLine($"Key: {change.Key}");
    Console.WriteLine($"  Change Type: {item.ChangeType}");
    Console.WriteLine($"  Current Value: {item.CurrentValue}");
    if (item.ChangeType == ChangeType.Modified)
    {
        Console.WriteLine($"  Previous Value: {item.PreviousValue}");
    }
}
```

### Expression-Based Key Lookup

```csharp
var userProfiles = new BindingDictionary<string, UserProfile>();

// Add some test data
userProfiles["john.doe@email.com"] = new UserProfile { Email = "john.doe@email.com", Name = "John Doe" };
userProfiles["jane.smith@email.com"] = new UserProfile { Email = "jane.smith@email.com", Name = "Jane Smith" };

// Find by expression
bool found = userProfiles.TryGetValue(
    keyExpression: email => email.StartsWith("john"),
    out UserProfile? johnProfile
);

if (found && johnProfile != null)
{
    Console.WriteLine($"Found user: {johnProfile.Name}");
}

// Find user with specific domain
bool foundDomain = userProfiles.TryGetValue(
    keyExpression: email => email.EndsWith("@email.com"),
    out UserProfile? domainUser
);
```

### Data Binding Scenarios

```csharp
// WPF/MAUI data binding example
public class InventoryViewModel : INotifyPropertyChanged
{
    public BindingDictionary<string, Product> Products { get; }

    public InventoryViewModel()
    {
        Products = new BindingDictionary<string, Product>();
        
        // Subscribe to collection changes for UI updates
        Products.KeyChanged += OnProductCollectionChanged;
        Products.ValueChanged += OnProductValueChanged;
        Products.Cleared += OnProductsCleared;
    }

    private void OnProductCollectionChanged(object sender, string productId, NotifiableChangeType changeType)
    {
        // Update UI collections, refresh views, etc.
        NotifyPropertyChanged(nameof(Products));
        NotifyPropertyChanged(nameof(TotalProducts));
        
        switch (changeType)
        {
            case NotifiableChangeType.Added:
                LogActivity($"Product {productId} added to inventory");
                break;
            case NotifiableChangeType.Removed:
                LogActivity($"Product {productId} removed from inventory");
                break;
        }
    }

    private void OnProductValueChanged(object sender, string productId, Product product, NotifiableChangeType changeType)
    {
        // Handle product updates
        if (changeType == NotifiableChangeType.Modified)
        {
            LogActivity($"Product {productId} updated: {product.Name}");
            // Trigger specific UI updates for the modified product
        }
    }

    private void OnProductsCleared(object sender)
    {
        LogActivity("All products cleared from inventory");
        NotifyPropertyChanged(nameof(TotalProducts));
    }

    public int TotalProducts => Products.Count;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Reactive Programming Integration

```csharp
// Reactive Extensions (Rx.NET) integration
public class ReactiveInventoryService
{
    private readonly BindingDictionary<string, decimal> _prices = new();
    private readonly Subject<(string ProductId, decimal Price, NotifiableChangeType ChangeType)> _priceChanges = new();

    public IObservable<(string ProductId, decimal Price, NotifiableChangeType ChangeType)> PriceChanges => _priceChanges.AsObservable();

    public ReactiveInventoryService()
    {
        _prices.ValueChanged += (sender, productId, price, changeType) =>
        {
            _priceChanges.OnNext((productId, price, changeType));
        };
    }

    public void UpdatePrice(string productId, decimal newPrice)
    {
        _prices[productId] = newPrice;
    }

    public IObservable<decimal> GetPriceUpdatesFor(string productId)
    {
        return PriceChanges
            .Where(change => change.ProductId == productId)
            .Select(change => change.Price)
            .DistinctUntilChanged();
    }

    public IObservable<string> GetProductsAbovePrice(decimal threshold)
    {
        return PriceChanges
            .Where(change => change.Price > threshold)
            .Select(change => change.ProductId)
            .Distinct();
    }
}

// Usage
var service = new ReactiveInventoryService();

// Subscribe to all price changes
service.PriceChanges.Subscribe(change =>
    Console.WriteLine($"{change.ProductId}: {change.Price:C} ({change.ChangeType})"));

// Subscribe to specific product
service.GetPriceUpdatesFor("WIDGET001").Subscribe(price =>
    Console.WriteLine($"Widget 1 price updated to {price:C}"));

// Subscribe to expensive items
service.GetProductsAbovePrice(100m).Subscribe(productId =>
    Console.WriteLine($"Expensive product detected: {productId}"));
```

### Caching Scenarios

```csharp
// Cache implementation with automatic cleanup
public class SmartCache<TKey, TValue> where TKey : notnull
{
    private readonly BindingDictionary<TKey, CacheEntry<TValue>> _cache = new(concurrentSupport: true);
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _defaultExpiry;

    public SmartCache(TimeSpan defaultExpiry)
    {
        _defaultExpiry = defaultExpiry;
        _cleanupTimer = new Timer(CleanExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        
        // Log cache operations
        _cache.KeyChanged += (sender, key, changeType) =>
        {
            Console.WriteLine($"Cache {changeType}: {key}");
        };
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        return _cache.GetValueOrAdd(key, k => new CacheEntry<TValue>
        {
            Value = valueFactory(k),
            ExpiresAt = DateTime.UtcNow.Add(_defaultExpiry)
        }).Value;
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            value = entry.Value;
            return true;
        }

        if (entry != null) // Expired entry
        {
            _cache.Remove(key);
        }

        value = default;
        return false;
    }

    private void CleanExpiredEntries(object? state)
    {
        var expiredKeys = _cache.Keys
            .Where(key => _cache.TryGetValue(key, out var entry) && entry.ExpiresAt <= DateTime.UtcNow)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.Remove(key);
        }
    }

    public void Clear() => _cache.Clear();
    public int Count => _cache.Count;
}

public class CacheEntry<T>
{
    public T Value { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}
```

### Configuration Management

```csharp
// Dynamic configuration system
public class ConfigurationManager
{
    private readonly BindingDictionary<string, object> _settings = new();
    private readonly List<IConfigurationObserver> _observers = new();

    public ConfigurationManager()
    {
        _settings.ValueChanged += OnSettingChanged;
    }

    public void RegisterObserver(IConfigurationObserver observer)
    {
        _observers.Add(observer);
    }

    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value!;
    }

    public IObservable<T> ObserveSetting<T>(string key)
    {
        return Observable.Create<T>(observer =>
        {
            // Send current value if exists
            if (_settings.TryGetValue(key, out var currentValue) && currentValue is T current)
            {
                observer.OnNext(current);
            }

            // Subscribe to changes
            void OnChange(object sender, string settingKey, object settingValue, NotifiableChangeType changeType)
            {
                if (settingKey == key && settingValue is T newValue)
                {
                    observer.OnNext(newValue);
                }
            }

            _settings.ValueChanged += OnChange;

            return Disposable.Create(() => _settings.ValueChanged -= OnChange);
        });
    }

    private void OnSettingChanged(object sender, string key, object value, NotifiableChangeType changeType)
    {
        foreach (var observer in _observers)
        {
            observer.OnSettingChanged(key, value, changeType);
        }
    }
}

public interface IConfigurationObserver
{
    void OnSettingChanged(string key, object value, NotifiableChangeType changeType);
}
```

## Advanced Features

### Type Conversions

```csharp
var bindingDict = new BindingDictionary<string, int>();

// Implicit conversion to standard Dictionary
Dictionary<string, int> standardDict = bindingDict;

// Implicit conversion to ConcurrentDictionary
ConcurrentDictionary<string, int> concurrentDict = bindingDict;

// This enables easy interoperability with existing APIs
ProcessDictionary(bindingDict); // Works seamlessly

void ProcessDictionary(Dictionary<string, int> dict)
{
    // Process dictionary normally
    foreach (var kvp in dict)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
}
```

### Serialization Support

```csharp
// JSON serialization (Newtonsoft.Json and System.Text.Json)
var originalDict = new BindingDictionary<string, int>
{
    ["apple"] = 1,
    ["banana"] = 2,
    ["cherry"] = 3
};

// Serialize
string json = JsonSerializer.Serialize(originalDict);

// Deserialize
var deserializedDict = JsonSerializer.Deserialize<BindingDictionary<string, int>>(json);

// Binary serialization (if needed)
using var stream = new MemoryStream();
var formatter = new BinaryFormatter();
formatter.Serialize(stream, originalDict);

stream.Position = 0;
var binaryDeserialized = (BindingDictionary<string, int>)formatter.Deserialize(stream);
```

## Performance Considerations

### When to Use Concurrent Support

```csharp
// Use concurrent support for multi-threaded scenarios
var highThroughput = new BindingDictionary<string, int>(concurrentSupport: true);

// For single-threaded or UI-bound scenarios, standard version is more efficient
var uiBinding = new BindingDictionary<string, int>(concurrentSupport: false);

// Check at runtime
if (Environment.ProcessorCount > 1 && ExpectingConcurrentAccess())
{
    var adaptiveDict = new BindingDictionary<string, object>(concurrentSupport: true);
}
```

### Memory and Performance Optimization

```csharp
// Pre-size for known capacity
var largeDataset = new BindingDictionary<string, DataRecord>(capacity: 10000);

// Use appropriate comparer for string keys
var caseInsensitive = new BindingDictionary<string, string>(
    comparer: StringComparer.OrdinalIgnoreCase,
    concurrentSupport: false
);

// Batch operations to reduce event overhead
using (var suppressNotifications = new NotificationSuppressor(bindingDict))
{
    // Multiple operations without individual events
    for (int i = 0; i < 1000; i++)
    {
        bindingDict[$"key_{i}"] = $"value_{i}";
    }
} // Events fire once at the end
```

## Error Handling and Validation

```csharp
public static class BindingDictionaryExtensions
{
    public static bool TrySafeAdd<TKey, TValue>(this BindingDictionary<TKey, TValue> dict, TKey key, TValue value)
        where TKey : notnull
    {
        try
        {
            return dict.TryAdd(key, value);
        }
        catch (ArgumentException)
        {
            // Handle key already exists scenario
            return false;
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            Console.WriteLine($"Unexpected error adding key {key}: {ex.Message}");
            return false;
        }
    }

    public static (bool Success, TValue? Value, string? Error) TrySafeGetValue<TKey, TValue>(
        this BindingDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        try
        {
            if (dict.TryGetValue(key, out var value))
            {
                return (true, value, null);
            }
            return (false, default, "Key not found");
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }
}

// Usage with error handling
var safeDict = new BindingDictionary<string, Customer>();

// Safe operations
bool addResult = safeDict.TrySafeAdd("CUST001", new Customer { Id = "CUST001" });
var (success, customer, error) = safeDict.TrySafeGetValue("CUST001");

if (!success)
{
    Console.WriteLine($"Failed to get customer: {error}");
}
```

## Integration Patterns

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<BindingDictionary<string, ApplicationSetting>>();
services.AddScoped<BindingDictionary<string, UserSession>>(provider =>
    new BindingDictionary<string, UserSession>(concurrentSupport: true));

// Factory pattern for different configurations
services.AddTransient<Func<bool, BindingDictionary<string, CacheItem>>>(provider =>
    concurrent => new BindingDictionary<string, CacheItem>(concurrentSupport: concurrent));
```

### Event Aggregation Pattern

```csharp
public class DictionaryEventAggregator
{
    private readonly List<WeakReference<BindingDictionary<string, object>>> _dictionaries = new();
    
    public void Subscribe<T>(BindingDictionary<string, T> dictionary) where T : class
    {
        _dictionaries.Add(new WeakReference<BindingDictionary<string, object>>(
            dictionary as BindingDictionary<string, object>));
        
        dictionary.ValueChanged += OnAnyDictionaryChanged;
    }
    
    private void OnAnyDictionaryChanged(object sender, string key, object value, NotifiableChangeType changeType)
    {
        // Aggregate events from all subscribed dictionaries
        GlobalDictionaryChanged?.Invoke(sender, key, value, changeType);
    }
    
    public event DictionaryValueChanged<string, object>? GlobalDictionaryChanged;
}
```

## Best Practices

1. **Choose the Right Constructor**: Use concurrent support only when needed for multi-threaded scenarios.

2. **Event Management**: Always unsubscribe from events to prevent memory leaks.

3. **Change Tracking**: Use change tracking for audit scenarios, not for performance-critical paths.

4. **Value Factories**: Prefer factory methods in `AddOrUpdate` and `GetValueOrAdd` for expensive object creation.

5. **Exception Handling**: Use the safe access methods (`TryGetValue`, `GetValueOrDefault`) instead of direct indexer access when key existence is uncertain.

6. **Performance Monitoring**: Monitor event handler performance as they execute synchronously and can impact dictionary operations.

## Related Components

- **[ChangeTrackingItems](../ChangeTrackingItems/README.md)**: For detailed change tracking and audit trails
- **[NotifiableObject](../Objects/NotifiableObject.md)**: Base class providing change notification infrastructure
- **Collections System**: Part of the broader Collections utilities in RapidStreamer BuildingBlocks

The `BindingDictionary<TKey, TValue>` provides a robust foundation for observable, thread-safe dictionary operations with comprehensive event notifications and change tracking capabilities, making it ideal for data binding, reactive programming, and audit-aware applications.