# ChangeTrackingItem\<TValue>

The `ChangeTrackingItem<TValue>` class is the core data structure in the RapidStreamer BuildingBlocks change tracking system that encapsulates information about a single change operation, including the type of change and the before/after values.

## Purpose

This class serves as:
- A immutable record of a specific change operation
- A container for change metadata (type, previous value, new value)
- The fundamental unit of information in the change tracking system
- A strongly-typed wrapper for change data

## Properties

### ChangeType
- **Type**: [`ChangeType`](ChangeType.md)
- **Access**: Read-only
- **Description**: Indicates the type of change (Added, Modified, or Removed)

### PreviousValue  
- **Type**: `TValue?`
- **Access**: Read-only
- **Description**: The value before the change occurred (null for Added operations)

### NewValue
- **Type**: `TValue?`
- **Access**: Read-only  
- **Description**: The value after the change occurred (null for Removed operations)

## Constructor

```csharp
internal ChangeTrackingItem(ChangeType changeType, TValue? previousValue, TValue? newValue)
```

**Note**: The constructor is internal - instances are typically created through the [`ChangeTrackingObjectAdapter`](ChangeTrackingObjectAdapter.md).

## Usage Examples

### Basic Creation (via Adapter)

```csharp
using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;

var adapter = new ChangeTrackingObjectAdapter<string, string>();
adapter.BeginTracking();

// These operations create ChangeTrackingItem instances internally
adapter.ReportAdded("key1", "newValue");
adapter.ReportModified("key2", "oldValue", "newValue");  
adapter.ReportRemoved("key3", "deletedValue");

var changes = adapter.EndTracking();
foreach (var change in changes)
{
    ChangeTrackingItem<string> item = change.Value;
    Console.WriteLine($"Change Type: {item.ChangeType}");
    Console.WriteLine($"Previous: {item.PreviousValue}");
    Console.WriteLine($"New: {item.NewValue}");
}
```

### Examining Change Details

```csharp
public void AnalyzeChange(ChangeTrackingItem<string> item)
{
    switch (item.ChangeType)
    {
        case ChangeType.Added:
            Console.WriteLine($"Added new item: '{item.NewValue}'");
            // PreviousValue will be null for additions
            Debug.Assert(item.PreviousValue == null);
            break;
            
        case ChangeType.Modified:
            Console.WriteLine($"Changed from '{item.PreviousValue}' to '{item.NewValue}'");
            // Both values should be present for modifications
            Debug.Assert(item.PreviousValue != null && item.NewValue != null);
            break;
            
        case ChangeType.Removed:
            Console.WriteLine($"Removed item: '{item.PreviousValue}'");
            // NewValue will be null for removals
            Debug.Assert(item.NewValue == null);
            break;
    }
}
```

### Working with Different Value Types

```csharp
// String values
var stringChange = new ChangeTrackingItem<string>(ChangeType.Modified, "old", "new");

// Numeric values  
var intChange = new ChangeTrackingItem<int>(ChangeType.Added, 0, 42);

// Nullable values
var nullableChange = new ChangeTrackingItem<int?>(ChangeType.Removed, 100, null);

// Complex objects
var objectChange = new ChangeTrackingItem<Person>(
    ChangeType.Modified, 
    new Person { Name = "John", Age = 30 }, 
    new Person { Name = "John", Age = 31 }
);

// Collections
var listChange = new ChangeTrackingItem<List<string>>(
    ChangeType.Added,
    null,
    new List<string> { "item1", "item2" }
);
```

### Processing Collections of Changes

```csharp
public void ProcessChanges(IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> changes)
{
    var addedItems = changes.Where(c => c.Value.ChangeType == ChangeType.Added);
    var modifiedItems = changes.Where(c => c.Value.ChangeType == ChangeType.Modified);
    var removedItems = changes.Where(c => c.Value.ChangeType == ChangeType.Removed);
    
    Console.WriteLine($"Added: {addedItems.Count()} items");
    Console.WriteLine($"Modified: {modifiedItems.Count()} items");
    Console.WriteLine($"Removed: {removedItems.Count()} items");
    
    // Process each type of change
    foreach (var addition in addedItems)
    {
        var item = addition.Value;
        HandleAddition(addition.Key, item.NewValue);
    }
    
    foreach (var modification in modifiedItems)
    {
        var item = modification.Value;
        HandleModification(modification.Key, item.PreviousValue, item.NewValue);
    }
    
    foreach (var removal in removedItems)
    {
        var item = removal.Value;
        HandleRemoval(removal.Key, item.PreviousValue);
    }
}
```

## Change Patterns and Value Expectations

### Added Items
```csharp
// Pattern: PreviousValue = null, NewValue = actual value
var addedItem = new ChangeTrackingItem<string>(ChangeType.Added, null, "new item");

Debug.Assert(addedItem.ChangeType == ChangeType.Added);
Debug.Assert(addedItem.PreviousValue == null);
Debug.Assert(addedItem.NewValue == "new item");
```

### Modified Items
```csharp
// Pattern: PreviousValue = old value, NewValue = new value
var modifiedItem = new ChangeTrackingItem<int>(ChangeType.Modified, 10, 20);

Debug.Assert(modifiedItem.ChangeType == ChangeType.Modified);
Debug.Assert(modifiedItem.PreviousValue == 10);
Debug.Assert(modifiedItem.NewValue == 20);
```

### Removed Items
```csharp
// Pattern: PreviousValue = actual value, NewValue = null  
var removedItem = new ChangeTrackingItem<string>(ChangeType.Removed, "deleted item", null);

Debug.Assert(removedItem.ChangeType == ChangeType.Removed);
Debug.Assert(removedItem.PreviousValue == "deleted item");
Debug.Assert(removedItem.NewValue == null);
```

## Real-World Applications

### Audit Trail Generation

```csharp
public class AuditLogger
{
    public void LogChange<T>(string entityId, ChangeTrackingItem<T> change, string userId)
    {
        var auditEntry = new AuditEntry
        {
            EntityId = entityId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            ChangeType = change.ChangeType.ToString(),
            PreviousValue = SerializeValue(change.PreviousValue),
            NewValue = SerializeValue(change.NewValue)
        };
        
        SaveAuditEntry(auditEntry);
    }
    
    private string? SerializeValue<T>(T? value)
    {
        return value?.ToString(); // Or use JSON serialization
    }
}
```

### Undo/Redo Operations

```csharp
public class UndoRedoManager<T>
{
    private readonly Stack<ChangeTrackingItem<T>> _undoStack = new();
    private readonly Stack<ChangeTrackingItem<T>> _redoStack = new();
    
    public void RecordChange(ChangeTrackingItem<T> change)
    {
        _undoStack.Push(change);
        _redoStack.Clear(); // Clear redo stack when new change is made
    }
    
    public ChangeTrackingItem<T>? CreateUndoOperation()
    {
        if (!_undoStack.TryPop(out var lastChange))
            return null;
            
        // Create reverse operation
        var undoChange = lastChange.ChangeType switch
        {
            ChangeType.Added => new ChangeTrackingItem<T>(ChangeType.Removed, lastChange.NewValue, null),
            ChangeType.Removed => new ChangeTrackingItem<T>(ChangeType.Added, null, lastChange.PreviousValue),
            ChangeType.Modified => new ChangeTrackingItem<T>(ChangeType.Modified, lastChange.NewValue, lastChange.PreviousValue),
            _ => throw new InvalidOperationException($"Unknown change type: {lastChange.ChangeType}")
        };
        
        _redoStack.Push(lastChange);
        return undoChange;
    }
}
```

### Data Synchronization

```csharp
public class DataSynchronizer
{
    public async Task SynchronizeChanges(IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> changes)
    {
        var tasks = changes.Select(change => ProcessChangeAsync(change.Key, change.Value));
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessChangeAsync(string key, ChangeTrackingItem<object> change)
    {
        switch (change.ChangeType)
        {
            case ChangeType.Added:
                await CreateItemAsync(key, change.NewValue);
                break;
                
            case ChangeType.Modified:
                await UpdateItemAsync(key, change.NewValue, change.PreviousValue);
                break;
                
            case ChangeType.Removed:
                await DeleteItemAsync(key, change.PreviousValue);
                break;
        }
    }
}
```

## Implementation Details

### Build Configuration
- In **DEBUG** builds: The class is not sealed, allowing inheritance for testing purposes
- In **RELEASE** builds: The class is sealed for performance optimization

### Immutability
The class is designed to be immutable:
- All properties are read-only after construction
- Values cannot be modified after creation
- Provides thread-safe access to change information

### Generic Type Support
The class supports any type for `TValue`:
- Value types (int, DateTime, etc.)
- Reference types (string, objects, etc.)  
- Nullable types (int?, string?, etc.)
- Collections and complex objects

## Best Practices

### Value Handling

✅ **Recommended practices:**
- Follow the value patterns for each change type
- Use null appropriately (null for PreviousValue in additions, null for NewValue in removals)
- Ensure type consistency between PreviousValue and NewValue
- Consider deep copying for reference types to avoid unintended mutations

### Type Safety

✅ **Type considerations:**
- Use strongly-typed generics for compile-time safety
- Consider nullable reference types for better null handling
- Validate change patterns match expected semantics

### Performance

✅ **Performance tips:**
- Items are lightweight - no concern about memory overhead for typical usage
- Consider the cost of value storage for large objects
- Use appropriate generic type parameters to avoid boxing

## Error Handling

```csharp
public void ValidateChangeItem<T>(ChangeTrackingItem<T> item)
{
    switch (item.ChangeType)
    {
        case ChangeType.Added:
            if (item.NewValue == null)
                throw new InvalidOperationException("Added items must have a NewValue");
            break;
            
        case ChangeType.Removed:  
            if (item.PreviousValue == null)
                throw new InvalidOperationException("Removed items must have a PreviousValue");
            break;
            
        case ChangeType.Modified:
            if (item.PreviousValue == null || item.NewValue == null)
                throw new InvalidOperationException("Modified items must have both PreviousValue and NewValue");
            break;
            
        default:
            throw new ArgumentException($"Unknown change type: {item.ChangeType}");
    }
}
```

## Related Components

- [`ChangeType`](ChangeType.md) - Enum defining the types of changes this class can represent
- [`ChangeTrackingItemCollection<TKey, TValue>`](ChangeTrackingItemCollection.md) - Collection that stores multiple ChangeTrackingItem instances
- [`ChangeTrackingObjectAdapter<TKey, TValue>`](ChangeTrackingObjectAdapter.md) - Factory class that creates ChangeTrackingItem instances
- [`IChangeTrackingObject<TKey, TValue>`](ChangeTrackingObject.md) - Interface that defines change tracking contracts

## Testing

The class behavior is thoroughly tested in `ChangeTrackingItemTests.cs`, which verifies:
- Proper constructor parameter assignment
- Correct property accessibility
- Nullable value handling
- Integration with different generic types
- Thread-safe immutability guarantees