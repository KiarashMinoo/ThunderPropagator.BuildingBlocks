# ChangeTrackingObjectAdapter\<TKey, TValue>

The `ChangeTrackingObjectAdapter<TKey, TValue>` class is the primary implementation helper in the RapidStreamer BuildingBlocks change tracking system. It provides a ready-to-use implementation of change tracking functionality that can be easily integrated into custom classes or used standalone.

## Purpose

This adapter serves as:
- A complete implementation of change tracking logic
- A bridge between business objects and the change tracking infrastructure
- A thread-safe change reporting mechanism
- A factory for creating [`ChangeTrackingItem`](ChangeTrackingItem.md) instances
- A manager for [`ChangeTrackingItemCollection`](ChangeTrackingItemCollection.md) lifecycle

## Key Features

- **Session Management**: Controls when change tracking is active or inactive
- **Change Reporting**: Convenient methods for reporting different types of changes
- **Thread Safety**: Built on thread-safe collections for concurrent scenarios
- **Flexible Updates**: Support for forced updates and duplicate change handling
- **Easy Integration**: Simple API that fits into existing class hierarchies

## Properties

### Enabled
```csharp
public bool Enabled { get; private set; }
```
**Description**: Indicates whether change tracking is currently active.
**Access**: Read-only (controlled by `BeginTracking()` and `EndTracking()`)

## Methods

### Session Management

#### BeginTracking()
```csharp
public bool BeginTracking()
```
**Purpose**: Starts a new change tracking session.
**Returns**: `true` (always succeeds)
**Behavior**: 
- Sets `Enabled` to `true`
- Clears any existing changes from the internal collection
- Prepares the adapter to capture new changes

#### EndTracking()
```csharp
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> EndTracking()
```
**Purpose**: Ends the current tracking session and returns all captured changes.
**Returns**: Enumerable collection of all changes since `BeginTracking()`
**Behavior**:
- Sets `Enabled` to `false`
- Returns the internal [`ChangeTrackingItemCollection`](ChangeTrackingItemCollection.md)

### Change Reporting

#### Report() (General)
```csharp
public bool Report(TKey key, ChangeType changeType, TValue? previousValue, TValue? newValue, bool forceToUpdate = false)
```
**Purpose**: Reports a general change with full control over all parameters.
**Parameters**:
- `key`: The identifier for the changed item
- `changeType`: The type of change ([`ChangeType`](ChangeType.md))
- `previousValue`: The value before the change
- `newValue`: The value after the change  
- `forceToUpdate`: Whether to overwrite existing changes for the same key
**Returns**: `true` if tracking is enabled and the change was recorded

#### ReportAdded()
```csharp
public bool ReportAdded(TKey key, TValue? newValue, bool forceToUpdate = false)
```
**Purpose**: Reports that a new item was added.
**Behavior**: Calls `Report()` with `ChangeType.Added` and `null` for `previousValue`

#### ReportModified()
```csharp
public bool ReportModified(TKey key, TValue? previousValue, TValue? newValue, bool forceToUpdate = false)
```
**Purpose**: Reports that an existing item was modified.
**Behavior**: Calls `Report()` with `ChangeType.Modified`

#### ReportRemoved()
```csharp
public bool ReportRemoved(TKey key, TValue? previousValue, bool forceToUpdate = false)
```
**Purpose**: Reports that an item was removed.
**Behavior**: Calls `Report()` with `ChangeType.Removed` and `null` for `newValue`

### Collection Management

#### Clear()
```csharp
public void Clear()
```
**Purpose**: Clears all tracked changes without affecting the `Enabled` state.

## Usage Examples

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;

var adapter = new ChangeTrackingObjectAdapter<string, object>();

// Start tracking
bool started = adapter.BeginTracking();
Console.WriteLine($"Tracking started: {started}"); // True
Console.WriteLine($"Tracking enabled: {adapter.Enabled}"); // True

// Report various changes
adapter.ReportAdded("NewProperty", "New Value");
adapter.ReportModified("ExistingProperty", "Old Value", "Updated Value");  
adapter.ReportRemoved("DeletedProperty", "Deleted Value");

// End tracking and get results
var changes = adapter.EndTracking();
Console.WriteLine($"Tracking enabled: {adapter.Enabled}"); // False
Console.WriteLine($"Changes captured: {changes.Count()}"); // 3

// Process the changes
foreach (var change in changes)
{
    Console.WriteLine($"{change.Key}: {change.Value.ChangeType}");
}
```

### Integration with Custom Classes

```csharp
public class Person
{
    private readonly ChangeTrackingObjectAdapter<string, object> _changeAdapter = new();
    private string _name = string.Empty;
    private int _age;
    private string _email = string.Empty;
    
    public string Name
    {
        get => _name;
        set
        {
            var oldValue = _name;
            _name = value;
            _changeAdapter.ReportModified(nameof(Name), oldValue, value);
        }
    }
    
    public int Age  
    {
        get => _age;
        set
        {
            var oldValue = _age;
            _age = value;
            _changeAdapter.ReportModified(nameof(Age), oldValue, value);
        }
    }
    
    public string Email
    {
        get => _email;
        set
        {
            var oldValue = _email;
            _email = value;
            _changeAdapter.ReportModified(nameof(Email), oldValue, value);
        }
    }
    
    // Change tracking interface
    public bool BeginTracking() => _changeAdapter.BeginTracking();
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> EndTracking()
        => _changeAdapter.EndTracking();
        
    public void ClearChanges() => _changeAdapter.Clear();
}

// Usage
var person = new Person();
person.BeginTracking();

person.Name = "John Doe";
person.Age = 30;
person.Email = "john@example.com";

var changes = person.EndTracking();
Console.WriteLine($"Person had {changes.Count()} changes");
```

### Advanced Usage with Force Update

```csharp
var adapter = new ChangeTrackingObjectAdapter<string, string>();
adapter.BeginTracking();

// Initial change
adapter.ReportModified("Property1", "Original", "Modified1");

// This won't overwrite because forceToUpdate is false (default)
adapter.ReportModified("Property1", "Modified1", "Modified2", forceToUpdate: false);

// This will overwrite the previous change
adapter.ReportModified("Property1", "Original", "FinalValue", forceToUpdate: true);

var changes = adapter.EndTracking();
var change = changes.First(c => c.Key == "Property1");

Console.WriteLine($"Previous: {change.Value.PreviousValue}"); // "Original"
Console.WriteLine($"New: {change.Value.NewValue}"); // "FinalValue"
```

### Batch Processing Pattern

```csharp
public class BatchProcessor<T>
{
    private readonly ChangeTrackingObjectAdapter<string, T> _adapter = new();
    
    public void ProcessBatch(Dictionary<string, T> updates, Dictionary<string, T> currentValues)
    {
        _adapter.BeginTracking();
        
        try
        {
            foreach (var update in updates)
            {
                var key = update.Key;
                var newValue = update.Value;
                
                if (currentValues.TryGetValue(key, out var currentValue))
                {
                    if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
                    {
                        _adapter.ReportModified(key, currentValue, newValue);
                    }
                }
                else
                {
                    _adapter.ReportAdded(key, newValue);
                }
            }
            
            // Check for removals
            foreach (var current in currentValues)
            {
                if (!updates.ContainsKey(current.Key))
                {
                    _adapter.ReportRemoved(current.Key, current.Value);
                }
            }
        }
        finally
        {
            var changes = _adapter.EndTracking();
            ProcessChanges(changes);
        }
    }
    
    private void ProcessChanges(IEnumerable<KeyValuePair<string, ChangeTrackingItem<T>>> changes)
    {
        foreach (var change in changes)
        {
            Console.WriteLine($"Batch change - {change.Key}: {change.Value.ChangeType}");
        }
    }
}
```

## Real-World Applications

### Entity Framework-Style Change Tracking

```csharp
public class TrackedEntity
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    private readonly Dictionary<string, object> _properties = new();
    private bool _isTrackingEnabled;
    
    public T GetProperty<T>(string propertyName)
    {
        return _properties.TryGetValue(propertyName, out var value) ? (T)value : default(T);
    }
    
    public void SetProperty<T>(string propertyName, T value)
    {
        var hasOldValue = _properties.TryGetValue(propertyName, out var oldValue);
        _properties[propertyName] = value;
        
        if (!_isTrackingEnabled) return;
        
        if (!hasOldValue)
        {
            _adapter.ReportAdded(propertyName, value);
        }
        else if (!Equals(oldValue, value))
        {
            _adapter.ReportModified(propertyName, oldValue, value);
        }
    }
    
    public void RemoveProperty(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var oldValue))
        {
            _properties.Remove(propertyName);
            if (_isTrackingEnabled)
            {
                _adapter.ReportRemoved(propertyName, oldValue);
            }
        }
    }
    
    public void StartTracking()
    {
        _isTrackingEnabled = true;
        _adapter.BeginTracking();
    }
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> GetChanges()
    {
        _isTrackingEnabled = false;
        return _adapter.EndTracking();
    }
}
```

### Configuration Manager with Change Tracking

```csharp
public class ConfigurationManager
{
    private readonly ChangeTrackingObjectAdapter<string, string> _adapter = new();
    private readonly Dictionary<string, string> _settings = new();
    
    public bool IsTrackingChanges => _adapter.Enabled;
    
    public void StartChangeTracking()
    {
        _adapter.BeginTracking();
    }
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<string>>> GetChangesAndStopTracking()
    {
        return _adapter.EndTracking();
    }
    
    public void Set(string key, string value)
    {
        var hasExisting = _settings.TryGetValue(key, out var existingValue);
        _settings[key] = value;
        
        if (!hasExisting)
        {
            _adapter.ReportAdded(key, value);
        }
        else if (existingValue != value)
        {
            _adapter.ReportModified(key, existingValue, value);
        }
    }
    
    public void Remove(string key)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            _settings.Remove(key);
            _adapter.ReportRemoved(key, value);
        }
    }
    
    public string Get(string key) => _settings.GetValueOrDefault(key, string.Empty);
    
    public async Task SaveChangesToFile(string filePath)
    {
        if (!_adapter.Enabled)
        {
            throw new InvalidOperationException("Change tracking must be active to save changes");
        }
        
        var changes = _adapter.EndTracking();
        await WriteChangesToLog(filePath, changes);
    }
    
    private async Task WriteChangesToLog(string filePath, 
        IEnumerable<KeyValuePair<string, ChangeTrackingItem<string>>> changes)
    {
        var logEntries = changes.Select(c => 
            $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {c.Key}: {c.Value.ChangeType} " +
            $"('{c.Value.PreviousValue}' → '{c.Value.NewValue}')");
            
        await File.AppendAllLinesAsync(filePath, logEntries);
    }
}
```

### Multi-threaded Change Tracking

```csharp
public class ConcurrentChangeTracker
{
    private readonly ChangeTrackingObjectAdapter<int, string> _adapter = new();
    private readonly ConcurrentDictionary<int, string> _data = new();
    
    public async Task ProcessConcurrentChanges()
    {
        _adapter.BeginTracking();
        
        // Simulate multiple threads making changes
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            await Task.Delay(Random.Shared.Next(10, 100)); // Simulate work
            
            var key = i;
            var newValue = $"Value-{i}-{DateTime.Now.Ticks}";
            
            var hasOldValue = _data.TryGetValue(key, out var oldValue);
            _data[key] = newValue;
            
            if (hasOldValue)
            {
                _adapter.ReportModified(key, oldValue, newValue);
            }
            else
            {
                _adapter.ReportAdded(key, newValue);
            }
        });
        
        await Task.WhenAll(tasks);
        
        var changes = _adapter.EndTracking();
        Console.WriteLine($"Concurrent processing resulted in {changes.Count()} changes");
        
        foreach (var change in changes.OrderBy(c => c.Key))
        {
            Console.WriteLine($"Thread change - {change.Key}: {change.Value.ChangeType}");
        }
    }
}
```

## Thread Safety

The adapter is built on thread-safe collections and can handle concurrent change reporting:

```csharp
public class ThreadSafeExample
{
    private readonly ChangeTrackingObjectAdapter<string, int> _adapter = new();
    
    public void DemonstrateConcurrentReporting()
    {
        _adapter.BeginTracking();
        
        // Multiple threads can safely report changes
        Parallel.For(0, 100, i =>
        {
            _adapter.ReportAdded($"key-{i}", i);
        });
        
        var changes = _adapter.EndTracking();
        Console.WriteLine($"Safely captured {changes.Count()} concurrent changes");
    }
}
```

## Performance Considerations

### Memory Usage
- The adapter maintains an internal [`ChangeTrackingItemCollection`](ChangeTrackingItemCollection.md)
- Memory usage scales with the number of unique keys being tracked
- Consider calling `Clear()` periodically for long-running tracking sessions

### Reporting Efficiency
```csharp
// Efficient: Check if tracking is enabled before expensive operations
public void OptimizedChangeReporting(string key, object newValue)
{
    if (!_adapter.Enabled) return; // Early exit
    
    var oldValue = GetCurrentValue(key); // Potentially expensive
    _adapter.ReportModified(key, oldValue, newValue);
}

// Less efficient: Always perform expensive operations
public void InefficientChangeReporting(string key, object newValue)
{
    var oldValue = GetCurrentValue(key); // Always called, even when not tracking
    _adapter.ReportModified(key, oldValue, newValue); // May do nothing if not enabled
}
```

## Best Practices

### Integration Guidelines

✅ **Recommended practices:**
- Check `adapter.Enabled` before expensive change detection operations
- Use the type-specific reporting methods (`ReportAdded`, `ReportModified`, `ReportRemoved`) for clarity
- Always call `EndTracking()` in a try-finally or using pattern
- Consider the performance impact of tracking in high-frequency scenarios

### Change Reporting Patterns

✅ **Effective patterns:**
```csharp
// Good: Type-specific methods
_adapter.ReportAdded(key, newValue);
_adapter.ReportModified(key, oldValue, newValue);
_adapter.ReportRemoved(key, oldValue);

// Good: Early exit when not tracking
if (!_adapter.Enabled) return;
var changes = CalculateChanges(); // Expensive operation
ReportChanges(changes);

// Good: Proper session management
public void ProcessWithTracking()
{
    _adapter.BeginTracking();
    try
    {
        // ... make changes
    }
    finally
    {
        var changes = _adapter.EndTracking();
        ProcessChanges(changes);
    }
}
```

### Error Handling

✅ **Robust implementations:**
```csharp
public class SafeChangeTracker
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    
    public void SafeChangeReporting(string key, object oldValue, object newValue)
    {
        try
        {
            _adapter.ReportModified(key, oldValue, newValue);
        }
        catch (Exception ex)
        {
            // Log but don't throw - change tracking shouldn't break business logic
            Console.WriteLine($"Failed to report change for {key}: {ex.Message}");
        }
    }
}
```

## Implementation Details

### Build Configuration
- In **DEBUG** builds: The class is not sealed, allowing inheritance for testing
- In **RELEASE** builds: The class is sealed for performance optimization

### Internal Architecture
- Uses [`ChangeTrackingItemCollection`](ChangeTrackingItemCollection.md) internally
- Thread-safe through the use of `ConcurrentDictionary`
- Minimal overhead when tracking is disabled

## Related Components

- [`IChangeTrackingObject<TKey, TValue>`](ChangeTrackingObject.md) - Interface this adapter helps implement
- [`ChangeTrackingItemCollection<TKey, TValue>`](ChangeTrackingItemCollection.md) - Internal collection used by the adapter
- [`ChangeTrackingItem<TValue>`](ChangeTrackingItem.md) - Items created by the adapter's reporting methods
- [`ChangeType`](ChangeType.md) - Enum used to categorize reported changes

## Testing

The adapter behavior is thoroughly tested in `ChangeTrackingObjectAdapterTests.cs`, which verifies:
- Session management (`BeginTracking`/`EndTracking` behavior)
- Change reporting when enabled vs disabled
- Thread safety under concurrent access
- Proper integration with the collection infrastructure
- Force update behavior and duplicate handling