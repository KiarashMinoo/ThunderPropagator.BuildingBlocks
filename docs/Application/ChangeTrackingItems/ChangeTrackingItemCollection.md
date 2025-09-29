# ChangeTrackingItemCollection\<TKey, TValue>

The `ChangeTrackingItemCollection<TKey, TValue>` class is a specialized collection in the RapidStreamer BuildingBlocks change tracking system that manages and provides access to [`ChangeTrackingItem`](ChangeTrackingItem.md) instances with thread-safe operations and convenient filtering capabilities.

## Purpose

This collection serves as:
- A centralized store for change tracking items
- A thread-safe collection using `ConcurrentDictionary` internally
- A provider of filtered views based on [`ChangeType`](ChangeType.md)
- A converter between change tracking data and standard dictionaries
- An enumerable interface for iterating over tracked changes

## Key Features

- **Thread-Safe Operations**: Built on `ConcurrentDictionary` for concurrent access
- **Change Type Filtering**: Built-in methods to filter by specific change types
- **Dictionary Conversion**: Convert tracked changes to standard dictionaries
- **Enumerable Interface**: Implements `IEnumerable` for easy iteration
- **Flexible Querying**: Support for both filtered and unfiltered data access

## Properties

The collection internally uses a `ConcurrentDictionary<TKey, ChangeTrackingItem<TValue>>` to store the changes, providing thread-safe access patterns.

## Methods

### Collection Management

#### Clear()
```csharp
internal void Clear()
```
Removes all items from the collection. This method is internal and typically called when beginning a new tracking session.

#### Add()
```csharp
internal bool Add(TKey key, ChangeTrackingItem<TValue> value, bool forceToUpdate = false)
```
Adds a new change tracking item or updates an existing one.
- **Parameters:**
  - `key`: The key to associate with the change
  - `value`: The change tracking item to store
  - `forceToUpdate`: Whether to update if the key already exists
- **Returns:** `true` if the item was added/updated successfully

### Data Access and Filtering

#### ToDictionary()
```csharp
public Dictionary<TKey, TValue> ToDictionary(ChangeType? changeType = null)
```
Converts the collection to a standard dictionary, optionally filtering by change type.
- **Parameters:**
  - `changeType`: Optional filter for specific change types
- **Returns:** Dictionary containing the `NewValue` from matching items

#### GetItems()
```csharp
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetItems(ChangeType changeType)
```
Retrieves all items matching a specific change type.

#### Convenience Filtering Methods

```csharp
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetAddedItems()
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetModifiedItems()
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetRemovedItems()
```
Shorthand methods for filtering by specific change types.

## Usage Examples

### Basic Collection Operations

```csharp
using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;

// Collection is typically used internally by ChangeTrackingObjectAdapter
var adapter = new ChangeTrackingObjectAdapter<string, string>();
adapter.BeginTracking();

// Add various types of changes
adapter.ReportAdded("item1", "newValue1");
adapter.ReportModified("item2", "oldValue2", "newValue2");
adapter.ReportRemoved("item3", "deletedValue3");

// Get the collection through EndTracking
var changes = adapter.EndTracking(); // Returns the internal collection

// Iterate through all changes
foreach (var change in changes)
{
    Console.WriteLine($"Key: {change.Key}, Type: {change.Value.ChangeType}");
}
```

### Filtering by Change Type

```csharp
public void ProcessChangesByType(ChangeTrackingItemCollection<string, string> collection)
{
    // Get only added items
    var addedItems = collection.GetAddedItems();
    Console.WriteLine($"Added {addedItems.Count()} new items:");
    foreach (var item in addedItems)
    {
        Console.WriteLine($"  {item.Key}: {item.Value.NewValue}");
    }
    
    // Get only modified items  
    var modifiedItems = collection.GetModifiedItems();
    Console.WriteLine($"Modified {modifiedItems.Count()} items:");
    foreach (var item in modifiedItems)
    {
        Console.WriteLine($"  {item.Key}: {item.Value.PreviousValue} → {item.Value.NewValue}");
    }
    
    // Get only removed items
    var removedItems = collection.GetRemovedItems();
    Console.WriteLine($"Removed {removedItems.Count()} items:");
    foreach (var item in removedItems)
    {
        Console.WriteLine($"  {item.Key}: {item.Value.PreviousValue}");
    }
}
```

### Dictionary Conversion

```csharp
public void ConvertToStandardDictionaries(ChangeTrackingItemCollection<string, object> collection)
{
    // Get all new values as a dictionary (regardless of change type)
    var allValues = collection.ToDictionary();
    
    // Get only added items as a dictionary
    var addedValues = collection.ToDictionary(ChangeType.Added);
    
    // Get only modified items as a dictionary  
    var modifiedValues = collection.ToDictionary(ChangeType.Modified);
    
    Console.WriteLine($"All values: {allValues.Count}");
    Console.WriteLine($"Added values: {addedValues.Count}");
    Console.WriteLine($"Modified values: {modifiedValues.Count}");
    
    // Note: Removed items won't appear in ToDictionary since their NewValue is null
}
```

### Advanced Filtering and Querying

```csharp
public void AdvancedQuerying(ChangeTrackingItemCollection<string, decimal> collection)
{
    // Combine with LINQ for complex queries
    var significantChanges = collection
        .Where(item => item.Value.ChangeType == ChangeType.Modified)
        .Where(item => Math.Abs(item.Value.NewValue.GetValueOrDefault() - 
                              item.Value.PreviousValue.GetValueOrDefault()) > 100)
        .ToList();
    
    Console.WriteLine($"Found {significantChanges.Count} significant changes (>100 difference)");
    
    // Get changes for specific keys
    var specificKeys = new[] { "key1", "key2", "key3" };
    var keySpecificChanges = collection
        .Where(item => specificKeys.Contains(item.Key))
        .ToList();
    
    // Group changes by type
    var changesByType = collection
        .GroupBy(item => item.Value.ChangeType)
        .ToDictionary(g => g.Key, g => g.ToList());
        
    foreach (var group in changesByType)
    {
        Console.WriteLine($"{group.Key}: {group.Value.Count} items");
    }
}
```

## Real-World Applications

### Audit Trail Generation

```csharp
public class AuditService
{
    public async Task GenerateAuditTrail(ChangeTrackingItemCollection<string, object> changes, 
                                        string userId, string entityId)
    {
        var auditEntries = new List<AuditEntry>();
        
        foreach (var change in changes)
        {
            var auditEntry = new AuditEntry
            {
                EntityId = entityId,
                PropertyName = change.Key,
                ChangeType = change.Value.ChangeType.ToString(),
                OldValue = SerializeValue(change.Value.PreviousValue),
                NewValue = SerializeValue(change.Value.NewValue),
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow
            };
            auditEntries.Add(auditEntry);
        }
        
        await SaveAuditEntriesAsync(auditEntries);
    }
}
```

### Database Synchronization

```csharp
public class DatabaseSynchronizer
{
    public async Task SynchronizeChanges(ChangeTrackingItemCollection<string, object> changes, 
                                       string tableName, string primaryKey)
    {
        // Process additions
        var additions = changes.GetAddedItems();
        if (additions.Any())
        {
            await ProcessAdditions(tableName, additions);
        }
        
        // Process modifications
        var modifications = changes.GetModifiedItems();
        if (modifications.Any())
        {
            await ProcessModifications(tableName, primaryKey, modifications);
        }
        
        // Process removals
        var removals = changes.GetRemovedItems();
        if (removals.Any())
        {
            await ProcessRemovals(tableName, primaryKey, removals);
        }
    }
    
    private async Task ProcessAdditions(string tableName, 
        IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> additions)
    {
        foreach (var addition in additions)
        {
            var sql = $"INSERT INTO {tableName} ({addition.Key}) VALUES (@value)";
            await ExecuteSqlAsync(sql, new { value = addition.Value.NewValue });
        }
    }
}
```

### Change Validation

```csharp
public class ChangeValidator
{
    public ValidationResult ValidateChanges(ChangeTrackingItemCollection<string, object> changes)
    {
        var errors = new List<string>();
        
        // Validate additions
        var additions = changes.GetAddedItems();
        foreach (var addition in additions)
        {
            if (addition.Value.NewValue == null)
            {
                errors.Add($"Added item '{addition.Key}' cannot have null value");
            }
        }
        
        // Validate modifications
        var modifications = changes.GetModifiedItems();
        foreach (var modification in modifications)
        {
            if (Equals(modification.Value.PreviousValue, modification.Value.NewValue))
            {
                errors.Add($"Modified item '{modification.Key}' has identical old and new values");
            }
        }
        
        // Validate removals
        var removals = changes.GetRemovedItems();
        foreach (var removal in removals)
        {
            if (removal.Value.PreviousValue == null)
            {
                errors.Add($"Removed item '{removal.Key}' must have a previous value");
            }
        }
        
        return new ValidationResult 
        { 
            IsValid = !errors.Any(), 
            Errors = errors 
        };
    }
}
```

## Thread Safety

The collection is built on `ConcurrentDictionary`, providing thread-safe operations:

```csharp
public class ThreadSafeChangeProcessor
{
    private readonly ChangeTrackingObjectAdapter<string, string> _adapter = new();
    
    public async Task ProcessChangesAsync()
    {
        _adapter.BeginTracking();
        
        // Multiple threads can safely report changes
        var tasks = Enumerable.Range(0, 10).Select(i =>
            Task.Run(() => _adapter.ReportAdded($"key{i}", $"value{i}"))
        );
        
        await Task.WhenAll(tasks);
        
        var changes = _adapter.EndTracking();
        
        // Safe to enumerate while other operations might be occurring
        Console.WriteLine($"Processed {changes.Count()} changes safely");
    }
}
```

## Performance Considerations

### Memory Usage
- Uses `ConcurrentDictionary` internally for optimal concurrent performance
- Items are stored by reference, not copied
- Consider the memory footprint of stored values for large collections

### Iteration Performance
```csharp
// Efficient: Use specific filtering methods
var additions = collection.GetAddedItems();

// Less efficient: Manual filtering with LINQ
var additions2 = collection.Where(c => c.Value.ChangeType == ChangeType.Added);

// Most efficient: Convert to dictionary for value-only access
var valueDict = collection.ToDictionary(ChangeType.Added);
```

## Best Practices

### Filtering and Access Patterns

✅ **Recommended practices:**
- Use built-in filtering methods (`GetAddedItems()`, etc.) instead of manual LINQ filtering
- Use `ToDictionary()` when you only need the values, not the full change information
- Leverage specific change type filtering to avoid processing irrelevant changes

### Thread Safety

✅ **Concurrent usage:**
- The collection is thread-safe for reads and writes
- Enumeration is safe during concurrent modifications
- Consider the thread safety of the stored values themselves

### Performance Optimization

✅ **Efficiency tips:**
- Batch process changes by type rather than processing one-by-one
- Use appropriate filtering to minimize data processing
- Consider memory usage when storing large objects as values

## Error Handling

```csharp
public void SafeCollectionAccess(ChangeTrackingItemCollection<string, object> collection)
{
    try
    {
        // Safe enumeration
        foreach (var item in collection)
        {
            ProcessItem(item.Key, item.Value);
        }
        
        // Safe filtering
        var additions = collection.GetAddedItems();
        if (additions.Any())
        {
            ProcessAdditions(additions);
        }
    }
    catch (InvalidOperationException ex)
    {
        // Handle collection modification during enumeration
        Console.WriteLine($"Collection was modified during access: {ex.Message}");
    }
}
```

## Related Components

- [`ChangeTrackingItem<TValue>`](ChangeTrackingItem.md) - The individual items stored in this collection
- [`ChangeType`](ChangeType.md) - Enum used for filtering operations
- [`ChangeTrackingObjectAdapter<TKey, TValue>`](ChangeTrackingObjectAdapter.md) - Primary creator and manager of this collection
- [`IChangeTrackingObject<TKey, TValue>`](ChangeTrackingObject.md) - Interface that returns instances of this collection

## Testing

The collection behavior is tested to verify:
- Thread-safe concurrent operations
- Correct filtering by change type
- Proper dictionary conversion
- Enumeration safety
- Integration with the change tracking workflow