# Attributes

## Overview

The Attributes namespace provides custom attributes for controlling serialization, data processing, and object behavior in RapidStreamer BuildingBlocks applications. These attributes enable fine-grained control over how objects are processed, serialized, and handled throughout the application lifecycle.

## Components

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **[IgnoreMemberAttribute](IgnoreMemberAttribute.md)** | Exclude members from processing | Selective exclusion, reflection control |
| **[JsonSerializationAttribute](JsonSerializationAttribute.md)** | JSON serialization control | Custom serialization behavior, property mapping |

## Purpose

- **Serialization Control**: Fine-tune how objects are serialized and deserialized
- **Reflection Management**: Control which members are included in reflection-based operations
- **Data Processing**: Guide automated data processing and transformation
- **Performance Optimization**: Exclude unnecessary members from expensive operations

## Quick Start

### Basic Usage
```csharp
using RapidStreamer.BuildingBlocks.Application.Attributes;

public class UserProfile
{
    public string Name { get; set; }
    public string Email { get; set; }
    
    [IgnoreMember]
    public string InternalId { get; set; } // Excluded from serialization/processing
    
    [JsonSerialization(PropertyName = "user_password")]
    public string Password { get; set; } // Custom JSON property name
}
```

### Advanced Serialization Control
```csharp
public class ApiResponse<T>
{
    public T Data { get; set; }
    
    [JsonSerialization(IgnoreOnSerialization = true)]
    public DateTime ProcessedAt { get; set; } // Excluded from JSON output
    
    [IgnoreMember]
    public string InternalTrackingId { get; set; } // Excluded from all processing
    
    [JsonSerialization(PropertyName = "timestamp", DateFormat = "yyyy-MM-ddTHH:mm:ssZ")]
    public DateTime CreatedAt { get; set; } // Custom property name and format
}
```

### Reflection-Based Processing
```csharp
public class DataProcessor
{
    public Dictionary<string, object> ProcessObject(object source)
    {
        var result = new Dictionary<string, object>();
        var type = source.GetType();
        
        foreach (var property in type.GetProperties())
        {
            // Skip properties marked with IgnoreMemberAttribute
            if (property.GetCustomAttribute<IgnoreMemberAttribute>() != null)
                continue;
            
            var value = property.GetValue(source);
            var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
            
            string propertyName = jsonAttr?.PropertyName ?? property.Name;
            result[propertyName] = value;
        }
        
        return result;
    }
}
```

## Integration Examples

### ASP.NET Core Configuration
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Register custom attribute processors
            options.ModelBinderProviders.Insert(0, new AttributeAwareModelBinderProvider());
        })
        .AddJsonOptions(options =>
        {
            // Configure JSON serialization to respect attributes
            options.JsonSerializerOptions.Converters.Add(new AttributeAwareJsonConverter());
        });
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public ActionResult<UserResponse> CreateUser([FromBody] CreateUserRequest request)
    {
        // Attributes automatically control serialization behavior
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            // InternalId and other [IgnoreMember] properties are automatically excluded
        };
        
        return Ok(new UserResponse { User = user });
    }
}

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    
    [JsonSerialization(PropertyName = "pwd")]
    public string Password { get; set; }
    
    [IgnoreMember]
    public string ValidationToken { get; set; } // Internal use only
}
```

### Entity Framework Integration
```csharp
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities based on attributes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties())
            {
                var ignoreAttr = property.GetCustomAttribute<IgnoreMemberAttribute>();
                if (ignoreAttr != null)
                {
                    modelBuilder.Entity(entityType.ClrType).Ignore(property.Name);
                }
                
                var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
                if (jsonAttr != null && !string.IsNullOrEmpty(jsonAttr.PropertyName))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnName(jsonAttr.PropertyName);
                }
            }
        }
    }
}

[Table("Users")]
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    
    [IgnoreMember] // Excluded from database
    public string TemporaryData { get; set; }
    
    [JsonSerialization(PropertyName = "created_date")]
    public DateTime CreatedAt { get; set; } // Database column: created_date
}
```

### Message Processing
```csharp
public class MessageProcessor
{
    public void ProcessMessage<T>(T message) where T : class
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<IgnoreMemberAttribute>() == null)
            .ToArray();
        
        foreach (var property in properties)
        {
            var value = property.GetValue(message);
            var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
            
            if (jsonAttr?.IgnoreOnSerialization == true)
                continue; // Skip serialization-ignored properties
            
            ProcessProperty(property.Name, value, jsonAttr);
        }
    }
    
    private void ProcessProperty(string name, object value, JsonSerializationAttribute jsonAttr)
    {
        string processedName = jsonAttr?.PropertyName ?? name;
        
        // Custom processing logic based on attribute configuration
        if (jsonAttr?.EncryptValue == true)
        {
            value = EncryptValue(value?.ToString());
        }
        
        // Store or transmit processed value
        Console.WriteLine($"Processing {processedName}: {value}");
    }
    
    private string EncryptValue(string value)
    {
        // Encryption logic
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value ?? ""));
    }
}

public class SecureMessage
{
    public string Title { get; set; }
    
    [JsonSerialization(PropertyName = "msg_content", EncryptValue = true)]
    public string Content { get; set; }
    
    [IgnoreMember]
    public string ProcessingMetadata { get; set; }
    
    [JsonSerialization(IgnoreOnSerialization = true)]
    public DateTime ProcessedAt { get; set; }
}
```

## Advanced Usage Patterns

### Custom Attribute Processor
```csharp
public class AttributeProcessor
{
    public static bool ShouldIgnore(PropertyInfo property)
    {
        return property.GetCustomAttribute<IgnoreMemberAttribute>() != null;
    }
    
    public static string GetSerializationName(PropertyInfo property)
    {
        var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
        return jsonAttr?.PropertyName ?? property.Name;
    }
    
    public static bool ShouldSerialize(PropertyInfo property, SerializationContext context)
    {
        var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
        
        return context switch
        {
            SerializationContext.Json => jsonAttr?.IgnoreOnSerialization != true,
            SerializationContext.Xml => jsonAttr?.IgnoreOnXmlSerialization != true,
            SerializationContext.Binary => !ShouldIgnore(property),
            _ => true
        };
    }
}

public enum SerializationContext
{
    Json,
    Xml,
    Binary
}
```

### Validation Integration
```csharp
public class AttributeAwareValidator<T> : AbstractValidator<T>
{
    public AttributeAwareValidator()
    {
        var properties = typeof(T).GetProperties();
        
        foreach (var property in properties)
        {
            if (AttributeProcessor.ShouldIgnore(property))
                continue; // Skip validation for ignored properties
            
            var jsonAttr = property.GetCustomAttribute<JsonSerializationAttribute>();
            
            // Add validation rules based on attributes
            if (jsonAttr?.Required == true)
            {
                RuleFor(x => property.GetValue(x))
                    .NotNull()
                    .WithMessage($"{property.Name} is required");
            }
            
            if (jsonAttr?.MaxLength > 0)
            {
                RuleFor(x => property.GetValue(x)?.ToString())
                    .MaximumLength(jsonAttr.MaxLength)
                    .WithMessage($"{property.Name} must not exceed {jsonAttr.MaxLength} characters");
            }
        }
    }
}

// Usage
public class UserValidator : AttributeAwareValidator<User>
{
    public UserValidator()
    {
        // Additional custom validation rules
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

## Performance Considerations

### Reflection Optimization
- **Caching**: Cache attribute information to avoid repeated reflection calls
- **Compilation**: Use expression compilation for frequently accessed properties
- **Lazy Loading**: Load attribute information only when needed

### Memory Usage
- **Attribute Instances**: Attributes are instantiated once per type, not per instance
- **Metadata Caching**: Consider caching processed metadata for high-volume scenarios
- **Selective Processing**: Use attribute filtering to process only relevant properties

## Best Practices

### Attribute Design
1. **Single Responsibility**: Each attribute should have a clear, focused purpose
2. **Composability**: Attributes should work well together
3. **Performance**: Minimize reflection overhead in hot paths
4. **Documentation**: Clearly document attribute behavior and usage

### Usage Guidelines
1. **Consistent Naming**: Use consistent property naming conventions with JsonSerializationAttribute
2. **Security**: Be careful with IgnoreMemberAttribute for sensitive data
3. **Validation**: Always validate that attributes are applied correctly
4. **Testing**: Test attribute behavior thoroughly, especially in serialization scenarios

## Related Components

- **[Serializations](../Serializations/README.md)** - Serialization components and converters
- **[Objects](../Objects/README.md)** - Base object types that work with attributes
- **[Helpers](../Helpers/README.md)** - Utility helpers for reflection and serialization
- **[Application Overview](../README.md)** - Complete application building blocks documentation

## Troubleshooting

### Common Issues

#### Attributes Not Being Recognized
```csharp
// Problem: Attributes ignored in custom serializers
public class CustomSerializer
{
    public string Serialize<T>(T obj)
    {
        // Missing: Check for attributes
        return JsonSerializer.Serialize(obj);
    }
}

// Solution: Include attribute checking
public class AttributeAwareSerializer
{
    public string Serialize<T>(T obj)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new AttributeAwareConverter<T>());
        return JsonSerializer.Serialize(obj, options);
    }
}
```

#### Performance Issues with Reflection
```csharp
// Problem: Repeated reflection calls
public class SlowProcessor
{
    public void Process<T>(T obj)
    {
        foreach (var property in typeof(T).GetProperties()) // Called every time
        {
            var attr = property.GetCustomAttribute<IgnoreMemberAttribute>();
            // Process property
        }
    }
}

// Solution: Cache reflection data
public class FastProcessor
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cachedProperties 
        = new ConcurrentDictionary<Type, PropertyInfo[]>();
    
    public void Process<T>(T obj)
    {
        var properties = _cachedProperties.GetOrAdd(typeof(T), 
            type => type.GetProperties().Where(p => 
                p.GetCustomAttribute<IgnoreMemberAttribute>() == null).ToArray());
        
        foreach (var property in properties)
        {
            // Process property
        }
    }
}
```