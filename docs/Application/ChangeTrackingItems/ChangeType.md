# ChangeType

The `ChangeType` enum is a foundational component of the RapidStreamer BuildingBlocks change tracking system that defines the different types of changes that can occur to tracked objects or values.

## Purpose

This enum categorizes the nature of changes in the change tracking system:
- Identifies whether an item was added, modified, or removed
- Provides a standardized way to classify change operations
- Enables filtering and querying of changes by type
- Used throughout the change tracking infrastructure

## Enum Values

| Value | Description | Use Case |
|-------|-------------|----------|
| `Added` | Represents a newly created or inserted item | New items added to collections or objects |
| `Modified` | Represents an item that was changed | Existing items with updated values |
| `Removed` | Represents an item that was deleted | Items removed from collections or objects |

## Usage Examples

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;

// Define change types
ChangeType addOperation = ChangeType.Added;
ChangeType updateOperation = ChangeType.Modified; 
ChangeType deleteOperation = ChangeType.Removed;
```

### With ChangeTrackingItem

```csharp
// Create change tracking items with different types
var addedItem = new ChangeTrackingItem<string>(ChangeType.Added, null, "New Value");
var modifiedItem = new ChangeTrackingItem<string>(ChangeType.Modified, "Old Value", "Updated Value");
var removedItem = new ChangeTrackingItem<string>(ChangeType.Removed, "Deleted Value", null);
```

### Filtering Changes by Type

```csharp
var collection = new ChangeTrackingItemCollection<string, string>();

// Filter changes by specific type
var addedItems = collection.GetAddedItems();     // Gets ChangeType.Added items
var modifiedItems = collection.GetModifiedItems(); // Gets ChangeType.Modified items  
var removedItems = collection.GetRemovedItems();  // Gets ChangeType.Removed items

// Convert to dictionary with specific change type
var addedDict = collection.ToDictionary(ChangeType.Added);
```

### Switch Pattern Matching

```csharp
public void ProcessChange(ChangeTrackingItem<string> item)
{
    switch (item.ChangeType)
    {
        case ChangeType.Added:
            Console.WriteLine($"Item added: {item.NewValue}");
            break;
            
        case ChangeType.Modified:
            Console.WriteLine($"Item changed from '{item.PreviousValue}' to '{item.NewValue}'");
            break;
            
        case ChangeType.Removed:
            Console.WriteLine($"Item removed: {item.PreviousValue}");
            break;
            
        default:
            throw new ArgumentOutOfRangeException();
    }
}
```

### Reporting Changes with Adapter

```csharp
var adapter = new ChangeTrackingObjectAdapter<string, string>();
adapter.BeginTracking();

// Report different types of changes
adapter.ReportAdded("key1", "newValue");                    // ChangeType.Added
adapter.ReportModified("key2", "oldValue", "newValue");     // ChangeType.Modified  
adapter.ReportRemoved("key3", "deletedValue");              // ChangeType.Removed

var changes = adapter.EndTracking();
```

## Integration with Change Tracking System

The `ChangeType` enum is used throughout the change tracking infrastructure:

### In ChangeTrackingItem
```csharp
public class ChangeTrackingItem<TValue>
{
    public ChangeType ChangeType { get; }  // Stores the type of change
    // ... other properties
}
```

### In ChangeTrackingItemCollection
```csharp
// Methods that work with ChangeType
public Dictionary<TKey, TValue> ToDictionary(ChangeType? changeType = null)
public IEnumerable<KeyValuePair<TKey, ChangeTrackingItem<TValue>>> GetItems(ChangeType changeType)
```

### In ChangeTrackingObjectAdapter
```csharp
// Methods that create changes with specific types
public bool ReportAdded(TKey key, TValue? newValue, bool forceToUpdate = false)      // Creates ChangeType.Added
public bool ReportModified(TKey key, TValue? previousValue, TValue? newValue, bool forceToUpdate = false)  // Creates ChangeType.Modified
public bool ReportRemoved(TKey key, TValue? previousValue, bool forceToUpdate = false)  // Creates ChangeType.Removed
```

## Real-World Scenarios

### Database Change Tracking

```csharp
public class EntityChangeTracker
{
    private readonly ChangeTrackingObjectAdapter<string, object> _adapter = new();
    
    public void TrackEntityChanges(Entity entity, Entity previousEntity)
    {
        _adapter.BeginTracking();
        
        // Compare properties and report appropriate change types
        if (previousEntity == null)
        {
            // New entity
            _adapter.ReportAdded("Entity", entity);
        }
        else if (!entity.Equals(previousEntity))
        {
            // Modified entity
            _adapter.ReportModified("Entity", previousEntity, entity);
        }
        
        var changes = _adapter.EndTracking();
        ProcessChanges(changes);
    }
    
    private void ProcessChanges(IEnumerable<KeyValuePair<string, ChangeTrackingItem<object>>> changes)
    {
        foreach (var change in changes)
        {
            switch (change.Value.ChangeType)
            {
                case ChangeType.Added:
                    // Insert into database
                    break;
                case ChangeType.Modified:
                    // Update database record
                    break;
                case ChangeType.Removed:
                    // Delete from database
                    break;
            }
        }
    }
}
```

### Collection Synchronization

```csharp
public class CollectionSynchronizer<T>
{
    public void SynchronizeCollections(IEnumerable<T> source, IEnumerable<T> target)
    {
        var adapter = new ChangeTrackingObjectAdapter<T, T>();
        adapter.BeginTracking();
        
        var sourceList = source.ToList();
        var targetList = target.ToList();
        
        // Find additions
        foreach (var item in sourceList.Except(targetList))
        {
            adapter.ReportAdded(item, item);
        }
        
        // Find removals
        foreach (var item in targetList.Except(sourceList))
        {
            adapter.ReportRemoved(item, item);
        }
        
        var changes = adapter.EndTracking();
        ApplyChanges(changes);
    }
    
    private void ApplyChanges(IEnumerable<KeyValuePair<T, ChangeTrackingItem<T>>> changes)
    {
        var addedItems = changes.Where(c => c.Value.ChangeType == ChangeType.Added);
        var removedItems = changes.Where(c => c.Value.ChangeType == ChangeType.Removed);
        
        // Process additions and removals
    }
}
```

## Best Practices

### Enum Usage Guidelines

✅ **Recommended practices:**
- Use appropriate change type for the actual operation being performed
- Consider the semantic meaning: Added = new, Modified = changed, Removed = deleted  
- Use switch statements with exhaustive case coverage
- Handle all enum values to future-proof code

### Filtering and Querying

✅ **Efficient patterns:**
- Use built-in collection methods like `GetAddedItems()` for common filtering
- Leverage `ToDictionary(ChangeType)` for type-specific data extraction
- Combine with LINQ for complex queries

### Error Handling

```csharp
public void ProcessChangeType(ChangeType changeType)
{
    switch (changeType)
    {
        case ChangeType.Added:
        case ChangeType.Modified:  
        case ChangeType.Removed:
            // Handle known cases
            break;
            
        default:
            throw new ArgumentOutOfRangeException(nameof(changeType), changeType, 
                "Unknown change type");
    }
}
```

## Related Components

- [`ChangeTrackingItem<TValue>`](ChangeTrackingItem.md) - Uses ChangeType to categorize changes
- [`ChangeTrackingItemCollection<TKey, TValue>`](ChangeTrackingItemCollection.md) - Provides filtering by ChangeType
- [`ChangeTrackingObjectAdapter<TKey, TValue>`](ChangeTrackingObjectAdapter.md) - Creates items with appropriate ChangeType
- [`IChangeTrackingObject<TKey, TValue>`](ChangeTrackingObject.md) - Interface contract for change tracking

## Testing

The enum behavior is covered by unit tests that verify:
- Proper value assignment in ChangeTrackingItem constructors
- Correct filtering in collection operations  
- Appropriate usage in adapter reporting methods
- Integration with the complete change tracking workflow