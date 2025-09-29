# JsonSerializationAttribute

The `JsonSerializationAttribute` is a custom attribute in the RapidStreamer BuildingBlocks library that controls JSON serialization behavior for classes, specifically managing the property naming policy (camelCase vs PascalCase) when serializing to and deserializing from JSON.

## Purpose

This attribute is used to:
- Override the default JSON serialization naming policy for specific classes
- Control whether property names should use camelCase formatting during JSON serialization
- Provide fine-grained control over JSON serialization behavior at the class level

## Target Elements

The attribute can only be applied to:
- **Classes** (`AttributeTargets.Class`)

## Properties

### CamelCase
- **Type**: `bool`
- **Default**: `true`
- **Description**: Determines whether property names should be serialized using camelCase formatting

## Usage Examples

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.Attributes;

// Class with default camelCase serialization (default behavior)
[JsonSerialization]
public class UserProfile
{
    public string FirstName { get; set; }  // Serialized as "firstName"
    public string LastName { get; set; }   // Serialized as "lastName"
    public DateTime CreatedAt { get; set; } // Serialized as "createdAt"
}

// Class with camelCase explicitly disabled
[JsonSerialization(CamelCase = false)]
public class LegacyApiResponse
{
    public string FirstName { get; set; }  // Serialized as "FirstName"
    public string LastName { get; set; }   // Serialized as "LastName"
    public DateTime CreatedAt { get; set; } // Serialized as "CreatedAt"
}
```

### Real-World Examples

#### API Response Models

```csharp
// Modern API with camelCase (default)
[JsonSerialization]
public class ModernUserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public DateTime LastLoginDate { get; set; }
}

// Legacy API compatibility with PascalCase
[JsonSerialization(CamelCase = false)]
public class LegacyUserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public DateTime LastLoginDate { get; set; }
}
```

#### JSON Output Comparison

```csharp
var user = new ModernUserResponse 
{
    UserId = 123,
    UserName = "john_doe",
    EmailAddress = "john@example.com",
    LastLoginDate = DateTime.Now
};

// Modern API output (camelCase):
// {
//   "userId": 123,
//   "userName": "john_doe", 
//   "emailAddress": "john@example.com",
//   "lastLoginDate": "2025-09-29T10:30:00Z"
// }

var legacyUser = new LegacyUserResponse 
{
    UserId = 123,
    UserName = "john_doe",
    EmailAddress = "john@example.com",
    LastLoginDate = DateTime.Now
};

// Legacy API output (PascalCase):
// {
//   "UserId": 123,
//   "UserName": "john_doe",
//   "EmailAddress": "john@example.com", 
//   "LastLoginDate": "2025-09-29T10:30:00Z"
// }
```

## Implementation Details

### Attribute Definition

```csharp
[AttributeUsage(AttributeTargets.Class)]
public
#if !DEBUG
    sealed
#endif
class JsonSerializationAttribute : Attribute
{
    public bool CamelCase { get; set; } = true;
}
```

### Build Configuration
- In **DEBUG** builds: The class is not sealed, allowing inheritance for testing purposes
- In **RELEASE** builds: The class is sealed for performance optimization

## Integration with JsonHelper

The attribute is automatically recognized and processed by the `JsonHelper` class:

### How It Works

1. **Attribute Caching**: The `JsonHelper` uses a `ConcurrentDictionary` to cache attribute lookups for performance
2. **Naming Policy Override**: When `CamelCase = false`, the default camelCase naming policy is disabled
3. **Automatic Detection**: The helper automatically detects the attribute on the target type during serialization/deserialization

### JsonHelper Integration Code

```csharp
private static JsonSerializerOptions JsonSerializerOptions(Type type, JsonSerializerOptions? serializerOptions = null)
{
    var jsonSerializationAttribute = JsonSerializationAttributes.GetOrAdd(type, key =>
    {
        var jsonSerializationAttributes = key.GetCustomAttributes(typeof(JsonSerializationAttribute), true);
        if (jsonSerializationAttributes.Length == 0)
            return null;

        return jsonSerializationAttributes.First() as JsonSerializationAttribute;
    });

    serializerOptions ??= BuildDefaultSerializerOptions();

    if (serializerOptions is { IsReadOnly: false, PropertyNamingPolicy: not null } && 
        jsonSerializationAttribute?.CamelCase == false)
        serializerOptions.PropertyNamingPolicy = null;

    return serializerOptions;
}
```

## Usage with JsonHelper Methods

All `JsonHelper` extension methods automatically respect the `JsonSerializationAttribute`:

```csharp
// Serialization methods
string json = myObject.ToJson();
byte[] jsonBytes = myObject.ToJsonBytes();
string base64Json = myObject.ToJsonBase64();

// Deserialization methods
var obj = jsonString.FromJson<MyClass>();
var objFromBytes = byteArray.FromJsonBytes<MyClass>();
var objFromBase64 = base64String.FromJsonBase64<MyClass>();
```

## Best Practices

### When to Use JsonSerializationAttribute

✅ **Good use cases:**
- **Legacy API compatibility**: Maintain existing API contracts that expect PascalCase
- **Third-party integrations**: Match the naming conventions of external systems
- **Database/ORM compatibility**: When property names need to match database column names exactly
- **Specific client requirements**: When clients expect a particular naming convention

### When NOT to Use JsonSerializationAttribute

❌ **Avoid using when:**
- Following modern web API conventions (camelCase is standard)
- Building new APIs where you control both client and server
- The default camelCase behavior meets your requirements

### Example: Legacy Integration

```csharp
// For integrating with a legacy system that expects PascalCase
[JsonSerialization(CamelCase = false)]
public class LegacyPaymentRequest
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string MerchantId { get; set; }
    public DateTime TransactionDate { get; set; }
}

// For modern APIs, use default behavior (no attribute needed)
public class ModernPaymentRequest
{
    public decimal Amount { get; set; }        // Serialized as "amount"
    public string CurrencyCode { get; set; }   // Serialized as "currencyCode"
    public string MerchantId { get; set; }     // Serialized as "merchantId"
    public DateTime TransactionDate { get; set; } // Serialized as "transactionDate"
}
```

## Performance Considerations

- **Attribute Caching**: The `JsonHelper` caches attribute lookups using `ConcurrentDictionary` for optimal performance
- **Reflection Overhead**: Attribute detection uses reflection, but this is minimized through caching
- **Thread Safety**: The implementation is thread-safe for concurrent operations

## Testing

The attribute behavior is thoroughly tested in `JsonSerializationAttributeTests.cs`, which verifies:
- Proper application to classes only
- Default `CamelCase` property behavior (defaults to `true`)
- Correct property value assignment
- Attribute usage restrictions
- Integration with serialization systems

## Related Components

- [`JsonHelper`](../Helpers/JsonHelper.md) - Primary consumer of this attribute
- [`IgnoreMemberAttribute`](IgnoreMemberAttribute.md) - Related attribute for excluding members from operations
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json) - Underlying JSON serialization framework

## Migration Guide

### From Manual JsonSerializerOptions to JsonSerializationAttribute

**Before:**
```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = null // No camelCase
};
var json = JsonSerializer.Serialize(obj, options);
```

**After:**
```csharp
[JsonSerialization(CamelCase = false)]
public class MyClass { /* ... */ }

// Now automatic with JsonHelper
var json = myObject.ToJson();
```