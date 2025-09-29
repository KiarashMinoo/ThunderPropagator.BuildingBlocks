# IgnoreMemberAttribute

The `IgnoreMemberAttribute` is a custom attribute in the RapidStreamer BuildingBlocks library that marks properties and fields to be excluded from certain operations like equality comparison, hash code generation, and reflection-based processing.

## Purpose

This attribute is used to:
- Exclude specific fields or properties from equality comparisons in `EquatableObject<T>`
- Prevent certain members from being included in hash code calculations
- Skip members during reflection-based operations in `ObjectHelper`

## Target Elements

The attribute can only be applied to:
- **Properties** (`AttributeTargets.Property`)
- **Fields** (`AttributeTargets.Field`)

## Usage Examples

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.Attributes;

public class Person : EquatableObject<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    [IgnoreMember]
    public DateTime LastUpdated { get; set; }  // Excluded from equality
    
    [IgnoreMember]
    private string _internalId = Guid.NewGuid().ToString();  // Excluded from equality
}
```

### In Equatable Objects

When using `EquatableObject<T>`, properties and fields marked with `[IgnoreMember]` are automatically excluded from:
- Equality comparisons (`Equals` method)
- Hash code generation (`GetHashCode` method)

```csharp
public class UserProfile : EquatableObject<UserProfile>
{
    public string Username { get; set; }
    public string Email { get; set; }
    
    [IgnoreMember]
    public DateTime LastLoginTime { get; set; }  // Changes don't affect equality
    
    [IgnoreMember]
    public int LoginCount { get; set; }  // Not part of object identity
}

// Example usage
var user1 = new UserProfile 
{ 
    Username = "john", 
    Email = "john@example.com", 
    LastLoginTime = DateTime.Now 
};

var user2 = new UserProfile 
{ 
    Username = "john", 
    Email = "john@example.com", 
    LastLoginTime = DateTime.Now.AddHours(1)  // Different login time
};

// user1.Equals(user2) returns true because LastLoginTime is ignored
```

### With ObjectHelper Methods

The `ObjectHelper` class respects the `[IgnoreMember]` attribute in its reflection operations:

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    [IgnoreMember]
    public string InternalSku { get; set; }  // Excluded from ObjectHelper operations
}

var product = new Product 
{ 
    Name = "Laptop", 
    Price = 999.99m, 
    InternalSku = "INT-12345" 
};

// GetProperties() and GetFields() will exclude members with [IgnoreMember]
var properties = product.GetProperties(); // Only includes Name and Price
```

## Implementation Details

### Attribute Definition

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public
#if !DEBUG
    sealed
#endif
class IgnoreMemberAttribute : Attribute;
```

### Build Configuration
- In **DEBUG** builds: The class is not sealed, allowing inheritance for testing purposes
- In **RELEASE** builds: The class is sealed for performance optimization

## Where It's Used

The attribute is automatically recognized and processed by:

1. **EquatableObject<T>** - Excludes marked members from equality comparisons and hash code generation
2. **ObjectHelper** - Excludes marked members from reflection-based operations like `GetFields()` and `GetProperties()`

## Best Practices

### When to Use IgnoreMember

✅ **Good candidates for [IgnoreMember]:**
- Timestamp fields (CreatedAt, LastUpdated, etc.)
- Computed or derived properties
- Internal tracking fields
- Caching or performance-related properties
- Properties that change frequently but don't affect object identity

### When NOT to Use IgnoreMember

❌ **Avoid using on:**
- Properties that define the object's identity
- Required business data
- Properties used for primary identification

### Example: Entity with Audit Fields

```csharp
public class Customer : EquatableObject<Customer>
{
    // Core identity properties (included in equality)
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    
    // Audit fields (excluded from equality)
    [IgnoreMember]
    public DateTime CreatedAt { get; set; }
    
    [IgnoreMember]
    public DateTime LastModified { get; set; }
    
    [IgnoreMember]
    public string LastModifiedBy { get; set; }
    
    // Computed property (excluded from equality)
    [IgnoreMember]
    public string DisplayName => $"{Name} ({Email})";
}
```

## Testing

The attribute behavior is covered by unit tests in `IgnoreMemberAttributeTests.cs`, which verify:
- Proper application to properties and fields
- Exclusion from reflection operations
- Attribute usage restrictions
- Integration with EquatableObject functionality

## Related Components

- [`EquatableObject<T>`](../Objects/EquatableObject.md) - Uses this attribute for equality comparisons
- [`ObjectHelper`](../Helpers/ObjectHelper.md) - Respects this attribute in reflection operations
- [JsonSerializationAttribute](JsonSerializationAttribute.md) - Related attribute for JSON serialization control