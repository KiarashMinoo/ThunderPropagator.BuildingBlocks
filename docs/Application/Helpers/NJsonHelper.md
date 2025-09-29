# NJsonHelper

The `NJsonHelper` is a comprehensive JSON serialization utility built on Newtonsoft.Json that provides advanced serialization features with custom attribute support and telemetry integration. It offers the most flexible JSON serialization capabilities while maintaining compatibility with the RapidStreamer BuildingBlocks attribute system.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `NJsonHelper` enhances Newtonsoft.Json operations by providing:

- **Advanced Serialization Features**: Full Newtonsoft.Json feature set with custom converters and settings
- **Custom Attribute Support**: Integration with `JsonSerializationAttribute` for camelCase control  
- **Exception Serialization**: Specialized handling for `Exception` objects through `ExceptionInfo`
- **Multiple Format Support**: JSON string, byte array, and Base64 encoding
- **Backward Compatibility**: Maintains compatibility with existing Newtonsoft.Json code
- **Telemetry Integration**: Built-in activity tracking for performance monitoring

## Key Features

### 🔧 Advanced JSON Features
- Complex object graph serialization with reference loop handling
- Custom converters and contract resolvers
- Flexible date/time formatting and culture handling
- Support for complex inheritance hierarchies
- Advanced LINQ to JSON capabilities

### 🎛️ Comprehensive Configuration
- `JsonSerializationAttribute` support for per-type camelCase control
- Configurable `JsonSerializerSettings` through lambda expressions
- Default camelCase naming with reference loop ignoring
- Type-specific setting resolution with caching

### 🔄 Multiple Format Support
- JSON string serialization/deserialization
- Byte array encoding for efficient storage
- Base64 encoding for text-safe transmission
- Exception-specific serialization through `ExceptionInfo`

### 📊 Observability
- Built-in telemetry tracking for all operations
- Performance monitoring and debugging capabilities
- Activity correlation for distributed tracing

## Core Methods

### JSON String Operations

#### ToNJson
```csharp
public static string ToNJson<T>(this T instance, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
```

#### FromNJson
```csharp
public static T? FromNJson<T>(this string json, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)

public static object? FromNJson(this string json, Type type, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
```

### Byte Array Operations

#### ToNJsonBytes / FromNJsonBytes
```csharp
public static byte[] ToNJsonBytes<T>(this T instance, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
    where T : notnull

public static T? FromNJsonBytes<T>(this byte[] bytes, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
```

### Base64 Operations

#### ToNJsonBase64 / FromNJsonBase64
```csharp
public static string ToNJsonBase64<T>(this T instance, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
    where T : notnull

public static T? FromNJsonBase64<T>(this string str, 
    Func<JsonSerializerSettings, JsonSerializerSettings>? settings = null)
```

## Configuration System

### Default Newtonsoft.Json Settings
```csharp
private static JsonSerializerSettings BuildDefaultNSerializerSettings()
    => new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
```

### JsonSerializationAttribute Integration
The helper automatically detects and applies `JsonSerializationAttribute` settings:

```csharp
[JsonSerialization(CamelCase = false)]
public class DatabaseModel
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
}

// Serialization will use PascalCase despite default camelCase setting
var json = dbModel.ToNJson();
```

### Advanced Settings Configuration
```csharp
var json = myObject.ToNJson(settings => 
{
    settings.ContractResolver = new DefaultContractResolver();
    settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    settings.NullValueHandling = NullValueHandling.Ignore;
    settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
    settings.Converters.Add(new StringEnumConverter());
    return settings;
});
```

## Usage Examples

### Basic Advanced Serialization
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;
using Newtonsoft.Json;

public class ComplexObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<NestedObject> Children { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

var complexObj = new ComplexObject
{
    Id = 1,
    Name = "Complex Item",
    CreatedAt = DateTime.UtcNow,
    Children = new List<NestedObject> { new() { Value = "Nested" } },
    Metadata = new Dictionary<string, object> 
    { 
        ["type"] = "sample",
        ["version"] = 1.2
    }
};

// Serialize complex object with full feature support
var json = complexObj.ToNJson();

// Deserialize maintaining all relationships
var restored = json.FromNJson<ComplexObject>();
```

### Advanced Date/Time Handling
```csharp
var dataWithDates = new
{
    Created = DateTime.UtcNow,
    Modified = DateTimeOffset.Now,
    Expires = new DateTime(2025, 12, 31)
};

// Custom date formatting
var json = dataWithDates.ToNJson(settings =>
{
    settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
    return settings;
});

Console.WriteLine(json);
// Output: {"created":"2024-01-15 10:30:45","modified":"2024-01-15 10:30:45",...}
```

### Custom Converters
```csharp
public class CustomDecimalConverter : JsonConverter<decimal>
{
    public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
    {
        writer.WriteValue(Math.Round(value, 2));
    }
    
    public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, 
        bool hasExistingValue, JsonSerializer serializer)
    {
        return Convert.ToDecimal(reader.Value);
    }
}

var financial = new { Amount = 123.456789m, Tax = 12.34567m };

var json = financial.ToNJson(settings =>
{
    settings.Converters.Add(new CustomDecimalConverter());
    return settings;
});

// Decimal values will be rounded to 2 decimal places
```

### Exception Serialization with Full Details
```csharp
try
{
    throw new InvalidOperationException("Operation failed", 
        new ArgumentException("Invalid argument", "parameter"));
}
catch (Exception ex)
{
    // Newtonsoft.Json excels at complex exception serialization
    var exceptionJson = ex.ToNJson(settings =>
    {
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.MaxDepth = 10;
        return settings;
    });
    
    // Rich exception information preserved
    var exceptionInfo = exceptionJson.FromNJson<ExceptionInfo>();
}
```

### Polymorphic Serialization
```csharp
[JsonConverter(typeof(TypeNameHandlingConverter))]
public abstract class Shape
{
    public abstract double Area { get; }
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public override double Area => Width * Height;
}

var shapes = new Shape[]
{
    new Circle { Radius = 5 },
    new Rectangle { Width = 4, Height = 6 }
};

// Serialize with type information preserved
var json = shapes.ToNJson(settings =>
{
    settings.TypeNameHandling = TypeNameHandling.Auto;
    return settings;
});

// Deserialize maintaining concrete types
var restoredShapes = json.FromNJson<Shape[]>();
// restoredShapes[0] is Circle, restoredShapes[1] is Rectangle
```

## Advanced Scenarios

### Dynamic Object Handling
```csharp
public class DynamicDataProcessor
{
    public string ProcessDynamicData(dynamic data)
    {
        // Newtonsoft.Json excels with dynamic objects
        var json = ((object)data).ToNJson(settings =>
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        });
        
        return json;
    }
    
    public dynamic ParseDynamicData(string json)
    {
        // Parse to JObject for dynamic access
        var jobject = JObject.Parse(json);
        return jobject;
    }
}
```

### LINQ to JSON Integration
```csharp
public class JsonQueryProcessor
{
    public IEnumerable<T> QueryJson<T>(string json, string path)
    {
        var jtoken = JToken.Parse(json);
        var selectedTokens = jtoken.SelectTokens(path);
        
        return selectedTokens.Select(token => token.ToObject<T>())
                            .Where(item => item != null)
                            .Cast<T>();
    }
    
    public string TransformJson(string json, Func<JToken, JToken> transformer)
    {
        var jtoken = JToken.Parse(json);
        var transformed = transformer(jtoken);
        return transformed.ToString();
    }
}
```

### Custom Contract Resolver
```csharp
public class IgnoreReadOnlyContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        
        // Ignore read-only properties
        if (property.DeclaringType != null && property.PropertyType != null)
        {
            var propInfo = property.DeclaringType.GetProperty(property.PropertyName!);
            if (propInfo?.CanWrite == false)
            {
                property.ShouldSerialize = _ => false;
            }
        }
        
        return property;
    }
}

// Usage
var json = data.ToNJson(settings =>
{
    settings.ContractResolver = new IgnoreReadOnlyContractResolver();
    return settings;
});
```

### Conditional Serialization
```csharp
public class ConditionalSerializationExample
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        
        public bool ShouldSerializePassword()
        {
            return false; // Never serialize password
        }
        
        public bool ShouldSerializeIsAdmin()
        {
            return IsAdmin; // Only serialize if true
        }
    }
    
    public string SerializeUser(User user, bool includeSecrets = false)
    {
        return user.ToNJson(settings =>
        {
            if (!includeSecrets)
            {
                settings.ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = false
                };
            }
            return settings;
        });
    }
}
```

### Migration and Versioning
```csharp
public class VersionedDataProcessor
{
    public string SerializeWithVersion<T>(T data, string version = "1.0")
    {
        var wrapper = new
        {
            Version = version,
            Timestamp = DateTime.UtcNow,
            Data = data
        };
        
        return wrapper.ToNJson(settings =>
        {
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            return settings;
        });
    }
    
    public T? DeserializeVersioned<T>(string json)
    {
        var wrapper = json.FromNJson<dynamic>();
        
        if (wrapper?.Version != null)
        {
            // Handle version-specific deserialization
            return HandleVersionedData<T>(wrapper);
        }
        
        // Fallback to direct deserialization
        return json.FromNJson<T>();
    }
    
    private T? HandleVersionedData<T>(dynamic wrapper)
    {
        var version = (string)wrapper.Version;
        var dataJson = wrapper.Data.ToString();
        
        return version switch
        {
            "1.0" => dataJson.FromNJson<T>(),
            "2.0" => MigrateFromV2<T>(dataJson),
            _ => throw new NotSupportedException($"Version {version} not supported")
        };
    }
    
    private T? MigrateFromV2<T>(string json)
    {
        // Handle migration logic for version 2.0
        var jObject = JObject.Parse(json);
        
        // Apply transformations for backward compatibility
        if (jObject["oldProperty"] != null)
        {
            jObject["newProperty"] = jObject["oldProperty"];
            jObject.Remove("oldProperty");
        }
        
        return jObject.ToObject<T>();
    }
}
```

## Performance Characteristics

### Feature vs Performance Trade-offs
Newtonsoft.Json provides the most features but with performance considerations:

| Aspect | Newtonsoft.Json | System.Text.Json | NetJSON |
|--------|-----------------|------------------|---------|
| Features | Excellent | Good | Basic |
| Performance | Good | Very Good | Excellent |
| Memory Usage | Higher | Lower | Lowest |
| Compatibility | Excellent | Good | Limited |

### Optimization Strategies
```csharp
public class OptimizedNJsonUsage
{
    // Pre-configured settings for better performance
    private static readonly JsonSerializerSettings _optimizedSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat
    };
    
    public string FastSerialize<T>(T data)
    {
        // Use pre-configured settings for better performance
        return JsonConvert.SerializeObject(data, _optimizedSettings);
    }
    
    public T? FastDeserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _optimizedSettings);
    }
}
```

### Memory Management
```csharp
public class MemoryEfficientNJson
{
    public async Task ProcessLargeJsonStream(Stream jsonStream)
    {
        using var streamReader = new StreamReader(jsonStream);
        using var jsonReader = new JsonTextReader(streamReader);
        
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        
        // Stream processing to avoid loading entire JSON into memory
        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                var obj = serializer.Deserialize<YourObject>(jsonReader);
                await ProcessObjectAsync(obj);
            }
        }
    }
}
```

## Error Handling and Debugging

### Comprehensive Error Information
```csharp
public class DiagnosticNJsonHelper
{
    public static string SerializeWithDiagnostics<T>(T data)
    {
        try
        {
            return data.ToNJson(settings =>
            {
                settings.TraceWriter = new MemoryTraceWriter();
                settings.SerializationBinder = new DiagnosticSerializationBinder();
                return settings;
            });
        }
        catch (JsonSerializationException ex)
        {
            // Rich serialization error information
            throw new InvalidOperationException(
                $"Failed to serialize {typeof(T).Name}: {ex.Message}", ex);
        }
    }
}

public class DiagnosticSerializationBinder : ISerializationBinder
{
    public Type BindToType(string? assemblyName, string typeName)
    {
        // Log type binding for debugging
        Console.WriteLine($"Binding to type: {typeName} from {assemblyName}");
        return Type.GetType($"{typeName}, {assemblyName}") ?? typeof(object);
    }
    
    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        assemblyName = serializedType.Assembly.FullName;
        typeName = serializedType.FullName;
    }
}
```

### Validation and Schema Support
```csharp
public class ValidatedNJsonHelper
{
    public static bool ValidateAgainstSchema(string json, JSchema schema)
    {
        try
        {
            var jObject = JObject.Parse(json);
            return jObject.IsValid(schema);
        }
        catch
        {
            return false;
        }
    }
    
    public static T? SafeDeserializeWithValidation<T>(string json, JSchema? schema = null)
        where T : class
    {
        if (string.IsNullOrEmpty(json)) return null;
        
        try
        {
            if (schema != null)
            {
                var jObject = JObject.Parse(json);
                if (!jObject.IsValid(schema))
                {
                    throw new JsonSerializationException("JSON does not match required schema");
                }
            }
            
            return json.FromNJson<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NJson deserialization failed: {ex.Message}");
            return null;
        }
    }
}
```

## Testing Strategies

### Feature Testing
```csharp
[Test]
public void NJson_ComplexObjectGraph_SerializesCorrectly()
{
    // Arrange
    var parent = new Parent { Name = "Parent" };
    var child = new Child { Name = "Child", Parent = parent };
    parent.Children = new List<Child> { child };
    
    // Act
    var json = parent.ToNJson(settings =>
    {
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        return settings;
    });
    
    var restored = json.FromNJson<Parent>();
    
    // Assert
    Assert.IsNotNull(restored);
    Assert.AreEqual("Parent", restored.Name);
    Assert.AreEqual(1, restored.Children.Count);
    Assert.AreEqual("Child", restored.Children[0].Name);
}
```

### Compatibility Testing
```csharp
[Test]
public void NJson_BackwardCompatibility_HandlesOldVersions()
{
    // Simulate old version JSON
    var oldVersionJson = @"{
        ""id"": 1,
        ""name"": ""Test"",
        ""oldProperty"": ""value""
    }";
    
    // Should handle missing properties gracefully
    var result = oldVersionJson.FromNJson<NewVersionClass>();
    
    Assert.IsNotNull(result);
    Assert.AreEqual(1, result.Id);
    Assert.AreEqual("Test", result.Name);
}
```

### Performance Comparison
```csharp
[Test]
public void NJson_VsOtherSerializers_FeatureComparison()
{
    var complexData = GenerateComplexTestData();
    
    // Test Newtonsoft.Json features
    var nJsonResult = complexData.ToNJson(settings =>
    {
        settings.TypeNameHandling = TypeNameHandling.Auto;
        settings.Converters.Add(new StringEnumConverter());
        return settings;
    });
    
    // Verify rich feature support
    Assert.Contains("$type", nJsonResult); // Type information preserved
    Assert.DoesNotContain("0", nJsonResult); // Enums as strings, not numbers
}
```

## Best Practices

### 1. Use for Feature-Rich Scenarios
```csharp
// ✅ Good: Use NJson for complex serialization needs
public string SerializeComplexConfiguration(Configuration config)
{
    return config.ToNJson(settings =>
    {
        settings.TypeNameHandling = TypeNameHandling.Auto;
        settings.Converters.Add(new StringEnumConverter());
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        return settings;
    });
}

// ✅ Good: Use for backward compatibility requirements
public T? DeserializeWithMigration<T>(string json, string expectedVersion)
{
    return HandleVersionMigration<T>(json, expectedVersion);
}
```

### 2. Configure Settings for Your Needs
```csharp
// ✅ Good: Pre-configure common settings
private static readonly JsonSerializerSettings ApiSettings = new()
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    NullValueHandling = NullValueHandling.Ignore,
    DateFormatHandling = DateFormatHandling.IsoDateFormat,
    Converters = { new StringEnumConverter() }
};

public string SerializeForApi<T>(T data)
{
    return JsonConvert.SerializeObject(data, ApiSettings);
}
```

### 3. Handle Complex Scenarios Gracefully
```csharp
// ✅ Good: Use for dynamic data handling
public dynamic ProcessDynamicData(string json)
{
    try
    {
        return JObject.Parse(json);
    }
    catch (JsonReaderException ex)
    {
        logger.LogError("Invalid JSON format: {Error}", ex.Message);
        return new JObject();
    }
}
```

### 4. Optimize When Possible
```csharp
// ✅ Good: Use streaming for large data
public async Task ProcessLargeJsonFile(string filePath)
{
    using var fileStream = File.OpenRead(filePath);
    using var streamReader = new StreamReader(fileStream);
    using var jsonReader = new JsonTextReader(streamReader);
    
    // Process JSON incrementally
    while (jsonReader.Read())
    {
        // Process tokens without loading entire file
    }
}
```

## Related Components

- **[JsonHelper](JsonHelper.md)**: System.Text.Json alternative for modern applications
- **[NetJsonHelper](NetJsonHelper.md)**: High-performance alternative for speed-critical scenarios
- **[MessagePackHelper](MessagePackHelper.md)**: Binary serialization alternative
- **[JsonSerializationAttribute](../Attributes/JsonSerializationAttribute.md)**: Controls camelCase behavior
- **[ExceptionInfo](../ExceptionInfo.md)**: Structured exception representation
- **[Telemetry](../Telemetry.md)**: Activity tracking and performance monitoring

## Migration Guide

### From Direct Newtonsoft.Json
```csharp
// Before: Direct Newtonsoft.Json usage
var json = JsonConvert.SerializeObject(data, Formatting.Indented);
var restored = JsonConvert.DeserializeObject<MyClass>(json);

// After: Using NJsonHelper
var json = data.ToNJson(settings => 
{
    settings.Formatting = Formatting.Indented;
    return settings;
});
var restored = json.FromNJson<MyClass>();
```

### To Higher Performance Alternatives
```csharp
// For simple scenarios, consider migration to System.Text.Json
// Before: NJson for simple data
var json = simpleData.ToNJson();

// After: JsonHelper for better performance
var json = simpleData.ToJson();

// Keep NJson for complex scenarios that require its advanced features
var complexJson = complexData.ToNJson(settings =>
{
    settings.TypeNameHandling = TypeNameHandling.Auto;
    return settings;
});
```

The NJsonHelper provides the most comprehensive JSON serialization solution for the RapidStreamer BuildingBlocks system, ideal for complex data structures, legacy compatibility, and scenarios requiring advanced JSON features.