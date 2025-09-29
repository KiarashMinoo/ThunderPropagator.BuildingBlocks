# IChangeTrackingObject\<TKey, TValue>

The `IChangeTrackingObject<TKey, TValue>` interface defines the contract for objects that support change tracking functionality in the RapidStreamer BuildingBlocks library. This interface establishes a standardized pattern for beginning and ending change tracking sessions.

## Purpose

This interface serves as:
- A contract for implementing change tracking in custom objects
- A standardized API for change tracking session management
- An abstraction layer for different change tracking implementations
- A way to ensure consistent behavior across change tracking objects

## Interface Definition

```csharp
public interface IChangeTrackingObject<TKey, TValue>
    where TKey : notnull
{
    bool BeginTracking();
    IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> EndTracking();
}
```

## Methods

### BeginTracking()
```csharp
bool BeginTracking();
```
**Purpose**: Starts a new change tracking session.

**Returns**: 
- `true` if tracking was successfully started
- `false` if tracking could not be started (e.g., already active)

**Behavior**:
- Enables change tracking for the implementing object
- Clears any previously tracked changes
- Prepares the object to capture subsequent changes

### EndTracking()
```csharp
IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> EndTracking();
```
**Purpose**: Ends the current change tracking session and returns all tracked changes.

**Returns**: An enumerable collection of key-value pairs where:
- `Key`: The identifier for the changed item
- `Value`: A [`ChangeTrackingItem<TValue>`](ChangeTrackingItem.md) containing change details

**Behavior**:
- Disables change tracking for the implementing object
- Returns all changes captured since `BeginTracking()` was called
- May clear the internal change collection (implementation dependent)

## Usage Patterns

### Basic Implementation Pattern

```csharp
public class MyChangeTrackingClass : IChangeTrackingObject<string, object>
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    
    public bool BeginTracking()
    {
        return _adapter.BeginTracking();
    }
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> EndTracking()
    {
        return _adapter.EndTracking();
    }
    
    // Your business methods that report changes
    public void UpdateProperty(string name, object value)
    {
        var oldValue = GetCurrentValue(name);
        SetValue(name, value);
        
        // Report the change if tracking is active
        _adapter.ReportModified(name, oldValue, value);
    }
}
```

### Usage Example

```csharp
// Create an object that implements IChangeTrackingObject
var trackingObject = new MyChangeTrackingClass();

// Start tracking changes
bool trackingStarted = trackingObject.BeginTracking();
if (!trackingStarted)
{
    Console.WriteLine("Failed to start tracking");
    return;
}

// Make some changes to the object
trackingObject.UpdateProperty("Name", "John Doe");
trackingObject.UpdateProperty("Age", 30);
trackingObject.DeleteProperty("TempData");

// End tracking and get all changes
var changes = trackingObject.EndTracking();

// Process the changes
foreach (var change in changes)
{
    var key = change.Key;
    var changeItem = change.Value;
    
    Console.WriteLine($"Property '{key}' {changeItem.ChangeType}:");
    Console.WriteLine($"  Previous: {changeItem.PreviousValue}");
    Console.WriteLine($"  New: {changeItem.NewValue}");
}
```

## Implementation Strategies

### Using ChangeTrackingObjectAdapter (Recommended)

The most common implementation leverages the [`ChangeTrackingObjectAdapter`](ChangeTrackingObjectAdapter.md):

```csharp
public class ConfigurationManager : IChangeTrackingObject<string, string>
{
    private readonly ChangeTrackingObjectAdapter<string, string> _changeAdapter = new();
    private readonly Dictionary<string, string> _settings = new();
    
    public bool BeginTracking() => _changeAdapter.BeginTracking();
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<string>>> EndTracking() 
        => _changeAdapter.EndTracking();
    
    public void SetSetting(string key, string value)
    {
        var oldValue = _settings.TryGetValue(key, out var existing) ? existing : null;
        
        if (oldValue == null)
        {
            _settings[key] = value;
            _changeAdapter.ReportAdded(key, value);
        }
        else if (oldValue != value)
        {
            _settings[key] = value;
            _changeAdapter.ReportModified(key, oldValue, value);
        }
    }
    
    public void RemoveSetting(string key)
    {
        if (_settings.TryGetValue(key, out var oldValue))
        {
            _settings.Remove(key);
            _changeAdapter.ReportRemoved(key, oldValue);
        }
    }
}
```

### Custom Implementation

For specialized scenarios, you might implement the interface directly:

```csharp
public class CustomChangeTracker : IChangeTrackingObject<int, decimal>
{
    private bool _isTracking;
    private readonly List<KeyValuePair<int, ChangeTrackingItem<decimal>>> _changes = new();
    private readonly Dictionary<int, decimal> _values = new();
    
    public bool BeginTracking()
    {
        if (_isTracking) return false;
        
        _isTracking = true;
        _changes.Clear();
        return true;
    }
    
    public IEnumerable<KeyValuePair<int, ChangeTrackingItem<decimal>>> EndTracking()
    {
        _isTracking = false;
        return _changes.ToList(); // Return a copy
    }
    
    public void SetValue(int key, decimal value)
    {
        var hasOldValue = _values.TryGetValue(key, out var oldValue);
        _values[key] = value;
        
        if (!_isTracking) return;
        
        if (!hasOldValue)
        {
            var addedItem = new ChangeTrackingItem<decimal>(ChangeType.Added, default, value);
            _changes.Add(new KeyValuePair<int, ChangeTrackingItem<decimal>>(key, addedItem));
        }
        else if (oldValue != value)
        {
            var modifiedItem = new ChangeTrackingItem<decimal>(ChangeType.Modified, oldValue, value);
            _changes.Add(new KeyValuePair<int, ChangeTrackingItem<decimal>>(key, modifiedItem));
        }
    }
}
```

## Real-World Applications

### Entity Framework Change Tracking

```csharp
public class TrackedEntity<TId> : IChangeTrackingObject<string, object>
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    private readonly Dictionary<string, object> _originalValues = new();
    private readonly Dictionary<string, object> _currentValues = new();
    
    public TId Id { get; set; }
    
    public bool BeginTracking()
    {
        var result = _adapter.BeginTracking();
        if (result)
        {
            // Capture current state as baseline
            _originalValues.Clear();
            foreach (var kvp in _currentValues)
            {
                _originalValues[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> EndTracking()
    {
        return _adapter.EndTracking();
    }
    
    public void SetProperty(string propertyName, object value)
    {
        var hasOldValue = _currentValues.TryGetValue(propertyName, out var oldValue);
        _currentValues[propertyName] = value;
        
        if (!hasOldValue)
        {
            _adapter.ReportAdded(propertyName, value);
        }
        else if (!Equals(oldValue, value))
        {
            _adapter.ReportModified(propertyName, oldValue, value);
        }
    }
}
```

### Document Version Control

```csharp
public class VersionedDocument : IChangeTrackingObject<string, string>
{
    private readonly ChangeTrackingObjectAdapter<string, string> _adapter = new();
    private readonly Dictionary<string, string> _content = new();
    
    public string DocumentId { get; }
    public int Version { get; private set; }
    
    public VersionedDocument(string documentId)
    {
        DocumentId = documentId;
        Version = 1;
    }
    
    public bool BeginTracking() => _adapter.BeginTracking();
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<string>>> EndTracking()
    {
        var changes = _adapter.EndTracking().ToList();
        if (changes.Any())
        {
            Version++; // Increment version if there were changes
        }
        return changes;
    }
    
    public void UpdateSection(string sectionName, string content)
    {
        var oldContent = _content.TryGetValue(sectionName, out var existing) ? existing : null;
        _content[sectionName] = content;
        
        if (oldContent == null)
        {
            _adapter.ReportAdded(sectionName, content);
        }
        else if (oldContent != content)
        {
            _adapter.ReportModified(sectionName, oldContent, content);
        }
    }
    
    public void RemoveSection(string sectionName)
    {
        if (_content.TryGetValue(sectionName, out var oldContent))
        {
            _content.Remove(sectionName);
            _adapter.ReportRemoved(sectionName, oldContent);
        }
    }
}
```

### Batch Processing with Change Tracking

```csharp
public class BatchProcessor
{
    public async Task ProcessWithTracking(IChangeTrackingObject<string, object> trackingObject)
    {
        // Start tracking
        if (!trackingObject.BeginTracking())
        {
            throw new InvalidOperationException("Could not begin change tracking");
        }
        
        try
        {
            // Perform operations that may modify the object
            await PerformBatchOperations(trackingObject);
        }
        finally
        {
            // Always end tracking and process changes
            var changes = trackingObject.EndTracking();
            await ProcessChanges(changes);
        }
    }
    
    private async Task ProcessChanges(
        IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> changes)
    {
        foreach (var change in changes)
        {
            switch (change.Value.ChangeType)
            {
                case ChangeType.Added:
                    await LogChange($"Added {change.Key}: {change.Value.NewValue}");
                    break;
                case ChangeType.Modified:
                    await LogChange($"Modified {change.Key}: {change.Value.PreviousValue} → {change.Value.NewValue}");
                    break;
                case ChangeType.Removed:
                    await LogChange($"Removed {change.Key}: {change.Value.PreviousValue}");
                    break;
            }
        }
    }
}
```

## Best Practices

### Implementation Guidelines

✅ **Recommended practices:**
- Use [`ChangeTrackingObjectAdapter`](ChangeTrackingObjectAdapter.md) for standard implementations
- Ensure `BeginTracking()` clears previous changes and returns appropriate status
- Make `EndTracking()` disable tracking and return all captured changes
- Handle concurrent access appropriately if needed
- Consider performance implications of change tracking in high-frequency scenarios

### Error Handling

✅ **Robust implementations:**
```csharp
public class RobustTrackingObject : IChangeTrackingObject<string, object>
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    
    public bool BeginTracking()
    {
        try
        {
            return _adapter.BeginTracking();
        }
        catch (Exception ex)
        {
            // Log error but don't throw - return false to indicate failure
            Console.WriteLine($"Failed to begin tracking: {ex.Message}");
            return false;
        }
    }
    
    public IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> EndTracking()
    {
        try
        {
            return _adapter.EndTracking();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ending tracking: {ex.Message}");
            return Enumerable.Empty<KeyValuePair<string, ChangeTrackingItem<object>>>();
        }
    }
}
```

### Usage Patterns

✅ **Effective usage:**
- Always check the return value of `BeginTracking()`
- Use try-finally or using pattern to ensure `EndTracking()` is called
- Process returned changes immediately after `EndTracking()`
- Consider creating extension methods for common change tracking patterns

## Generic Type Constraints

The interface has specific constraints:
- `TKey : notnull` - Keys cannot be null, ensuring reliable dictionary operations
- No constraints on `TValue` - Values can be any type, including nullable types

## Threading Considerations

The interface doesn't specify thread safety requirements, so implementations should:
- Document their thread safety guarantees
- Use appropriate synchronization if supporting concurrent access
- Consider using [`ChangeTrackingObjectAdapter`](ChangeTrackingObjectAdapter.md) which is built on thread-safe collections

## Related Components

- [`ChangeTrackingObjectAdapter<TKey, TValue>`](ChangeTrackingObjectAdapter.md) - Recommended implementation helper
- [`ChangeTrackingItem<TValue>`](ChangeTrackingItem.md) - The change data structure returned by `EndTracking()`
- [`ChangeTrackingItemCollection<TKey, TValue>`](ChangeTrackingItemCollection.md) - Collection type used internally
- [`ChangeType`](ChangeType.md) - Enum defining the types of changes that can be tracked

## Testing

When implementing this interface, test:
- `BeginTracking()` return values under various conditions
- `EndTracking()` returns appropriate changes
- Proper change capture during tracking sessions
- Behavior when methods are called in unexpected orders
- Thread safety if concurrent access is supported