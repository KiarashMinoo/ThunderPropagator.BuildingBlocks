# YamlTypeConverterAttribute

The `YamlTypeConverterAttribute` is a specialized attribute that enables you to specify custom YAML type converters for classes, interfaces, structs, enums, properties, and fields. This attribute provides a declarative way to associate custom serialization logic with types in the YAML serialization system.

## Overview

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
public class YamlTypeConverterAttribute : Attribute
```

The `YamlTypeConverterAttribute` integrates with the YamlDotNet serialization framework to provide custom conversion logic for complex types, enabling fine-grained control over how objects are serialized to and deserialized from YAML format.

## Key Features

- **Type-Level Control**: Apply custom converters to entire types (classes, structs, enums)
- **Member-Level Control**: Apply converters to specific properties or fields
- **Interface Support**: Works with interfaces for polymorphic scenarios
- **YamlDotNet Integration**: Seamlessly integrates with the YamlDotNet library
- **Compile-Time Safety**: Ensures converter type is specified at compile time
- **Flexible Targeting**: Supports multiple target types for versatile usage

## Properties

### ConverterType
Specifies the type of the custom YAML converter.

```csharp
public Type ConverterType { get; }
```

**Requirements:**
- Must implement `IYamlTypeConverter` or derive from `YamlTypeConverter<T>`
- Must have a parameterless constructor
- Should handle the target type appropriately

## Usage Examples

### Basic Type Converter

```csharp
// Custom converter for Person class
public class PersonYamlConverter : YamlTypeConverter<Person>
{
    protected override void WriteYamlInternal(IEmitter emitter, Person? value, Type type, ObjectSerializer serializer)
    {
        if (value == null) return;
        
        WriteMappingStart(emitter);
        WriteKeyValue(emitter, "fullName", $"{value.FirstName} {value.LastName}");
        WriteKeyValue(emitter, "age", value.Age.ToString());
        WriteKeyValue(emitter, "email", value.Email);
        WriteMappingEnd(emitter);
    }
    
    protected override Person? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var person = new Person();
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            
            switch (key)
            {
                case "fullName":
                    var fullName = ReadValueAndShift(parser);
                    var parts = fullName?.Split(' ', 2);
                    if (parts?.Length >= 2)
                    {
                        person.FirstName = parts[0];
                        person.LastName = parts[1];
                    }
                    break;
                    
                case "age":
                    person.Age = ReadNumber<int>(parser) ?? 0;
                    parser.MoveNext();
                    break;
                    
                case "email":
                    person.Email = ReadValueAndShift(parser) ?? string.Empty;
                    break;
                    
                default:
                    parser.MoveNext(); // Skip unknown properties
                    break;
            }
        }
        
        return person;
    }
}

// Apply converter to the entire class
[YamlTypeConverter(typeof(PersonYamlConverter))]
public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

// Usage example
public void DemonstrateBasicTypeConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new PersonYamlConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new PersonYamlConverter())
        .Build();
    
    var person = new Person
    {
        FirstName = "John",
        LastName = "Doe",
        Age = 30,
        Email = "john.doe@example.com"
    };
    
    // Serialize
    var yaml = serializer.Serialize(person);
    Console.WriteLine("Serialized YAML:");
    Console.WriteLine(yaml);
    // Output:
    // fullName: John Doe
    // age: 30
    // email: john.doe@example.com
    
    // Deserialize
    var deserialized = deserializer.Deserialize<Person>(yaml);
    Console.WriteLine($"\nDeserialized: {deserialized.FirstName} {deserialized.LastName}, Age: {deserialized.Age}");
}
```

### Property-Level Converter

```csharp
// Custom converter for DateTime properties
public class DateTimeYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(DateTime) || type == typeof(DateTime?);
    
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var value = (parser.Current as Scalar)?.Value;
        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;
        return type == typeof(DateTime?) ? null : DateTime.MinValue;
    }
    
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is DateTime dateTime)
        {
            emitter.Emit(new Scalar(null, null, dateTime.ToString("yyyy-MM-dd"), ScalarStyle.Plain, true, false));
        }
    }
}

public class Event
{
    public string Name { get; set; } = string.Empty;
    
    // Apply converter to specific property
    [YamlTypeConverter(typeof(DateTimeYamlConverter))]
    public DateTime StartDate { get; set; }
    
    [YamlTypeConverter(typeof(DateTimeYamlConverter))]
    public DateTime? EndDate { get; set; }
    
    public string Description { get; set; } = string.Empty;
}

// Usage example
public void DemonstratePropertyConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new DateTimeYamlConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new DateTimeYamlConverter())
        .Build();
    
    var eventItem = new Event
    {
        Name = "Conference 2023",
        StartDate = new DateTime(2023, 10, 15),
        EndDate = new DateTime(2023, 10, 17),
        Description = "Annual technology conference"
    };
    
    var yaml = serializer.Serialize(eventItem);
    Console.WriteLine("Serialized YAML:");
    Console.WriteLine(yaml);
    // Output:
    // Name: Conference 2023
    // StartDate: 2023-10-15
    // EndDate: 2023-10-17
    // Description: Annual technology conference
    
    var deserialized = deserializer.Deserialize<Event>(yaml);
    Console.WriteLine($"\nEvent: {deserialized.Name}");
    Console.WriteLine($"Start: {deserialized.StartDate:yyyy-MM-dd}");
    Console.WriteLine($"End: {deserialized.EndDate:yyyy-MM-dd}");
}
```

### Complex Object Converter

```csharp
// Converter for complex configuration objects
public class DatabaseConfigConverter : YamlTypeConverter<DatabaseConfig>
{
    protected override void WriteYamlInternal(IEmitter emitter, DatabaseConfig? value, Type type, ObjectSerializer serializer)
    {
        if (value == null) return;
        
        WriteMappingStart(emitter);
        
        WriteKeyValue(emitter, "server", value.Server);
        WriteKeyValue(emitter, "database", value.Database);
        WriteNumber(emitter, "port", value.Port);
        WriteBoolean(emitter, "useSSL", value.UseSSL);
        WriteNumber(emitter, "timeoutSeconds", value.TimeoutSeconds);
        
        if (value.ConnectionProperties.Any())
        {
            WriteKey(emitter, "properties");
            WriteMappingStart(emitter);
            
            foreach (var kvp in value.ConnectionProperties)
            {
                WriteKeyValue(emitter, kvp.Key, kvp.Value);
            }
            
            WriteMappingEnd(emitter);
        }
        
        WriteMappingEnd(emitter);
    }
    
    protected override DatabaseConfig? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var config = new DatabaseConfig();
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            
            switch (key)
            {
                case "server":
                    config.Server = ReadValueAndShift(parser) ?? string.Empty;
                    break;
                    
                case "database":
                    config.Database = ReadValueAndShift(parser) ?? string.Empty;
                    break;
                    
                case "port":
                    config.Port = ReadNumber<int>(parser) ?? 1433;
                    parser.MoveNext();
                    break;
                    
                case "useSSL":
                    config.UseSSL = ReadBoolean(parser) ?? false;
                    parser.MoveNext();
                    break;
                    
                case "timeoutSeconds":
                    config.TimeoutSeconds = ReadNumber<int>(parser) ?? 30;
                    parser.MoveNext();
                    break;
                    
                case "properties":
                    if (IsMappingStartAndShift(parser))
                    {
                        while (!IsMappingEndAndShift(parser))
                        {
                            var propKey = ReadKey(parser);
                            var propValue = ReadValueAndShift(parser);
                            if (!string.IsNullOrEmpty(propKey) && !string.IsNullOrEmpty(propValue))
                            {
                                config.ConnectionProperties[propKey] = propValue;
                            }
                        }
                    }
                    break;
                    
                default:
                    parser.MoveNext();
                    break;
            }
        }
        
        return config;
    }
}

[YamlTypeConverter(typeof(DatabaseConfigConverter))]
public class DatabaseConfig
{
    public string Server { get; set; } = "localhost";
    public string Database { get; set; } = string.Empty;
    public int Port { get; set; } = 1433;
    public bool UseSSL { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
    public Dictionary<string, string> ConnectionProperties { get; set; } = new();
}

// Usage example
public void DemonstrateComplexConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new DatabaseConfigConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new DatabaseConfigConverter())
        .Build();
    
    var config = new DatabaseConfig
    {
        Server = "prod-db-01.company.com",
        Database = "ApplicationDB",
        Port = 5432,
        UseSSL = true,
        TimeoutSeconds = 60,
        ConnectionProperties = new Dictionary<string, string>
        {
            ["ApplicationName"] = "MyApp",
            ["Pooling"] = "true",
            ["MaxPoolSize"] = "100"
        }
    };
    
    var yaml = serializer.Serialize(config);
    Console.WriteLine("Database Configuration YAML:");
    Console.WriteLine(yaml);
    
    var deserialized = deserializer.Deserialize<DatabaseConfig>(yaml);
    Console.WriteLine($"\nDeserialized Config:");
    Console.WriteLine($"Server: {deserialized.Server}:{deserialized.Port}");
    Console.WriteLine($"Database: {deserialized.Database}");
    Console.WriteLine($"SSL: {deserialized.UseSSL}");
    Console.WriteLine($"Properties: {deserialized.ConnectionProperties.Count}");
}
```

### Enum Converter with Custom Formatting

```csharp
// Custom converter for enum with specific formatting
public class StatusYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(OrderStatus) || type == typeof(OrderStatus?);
    
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var value = (parser.Current as Scalar)?.Value?.Replace("-", "");
        return Enum.TryParse<OrderStatus>(value, true, out var result) ? result : OrderStatus.Unknown;
    }
    
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is OrderStatus status)
        {
            var yamlValue = status switch
            {
                OrderStatus.InProgress => "in-progress",
                OrderStatus.ReadyToShip => "ready-to-ship",
                OrderStatus.OutForDelivery => "out-for-delivery",
                _ => status.ToString().ToLowerInvariant()
            };
            
            emitter.Emit(new Scalar(null, null, yamlValue, ScalarStyle.Plain, true, false));
        }
    }
}

public enum OrderStatus
{
    Unknown,
    Pending,
    InProgress,
    ReadyToShip,
    OutForDelivery,
    Delivered,
    Cancelled
}

public class Order
{
    public string OrderId { get; set; } = string.Empty;
    
    [YamlTypeConverter(typeof(StatusYamlConverter))]
    public OrderStatus Status { get; set; }
    
    public decimal Total { get; set; }
}

// Usage example
public void DemonstrateEnumConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new StatusYamlConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new StatusYamlConverter())
        .Build();
    
    var order = new Order
    {
        OrderId = "ORD-2023-001",
        Status = OrderStatus.OutForDelivery,
        Total = 299.99m
    };
    
    var yaml = serializer.Serialize(order);
    Console.WriteLine("Order YAML:");
    Console.WriteLine(yaml);
    // Output:
    // OrderId: ORD-2023-001
    // Status: out-for-delivery
    // Total: 299.99
    
    var deserialized = deserializer.Deserialize<Order>(yaml);
    Console.WriteLine($"\nOrder: {deserialized.OrderId}");
    Console.WriteLine($"Status: {deserialized.Status}");
    Console.WriteLine($"Total: ${deserialized.Total}");
}
```

### Collection Converter

```csharp
// Custom converter for specialized collections
public class TagCollectionConverter : YamlTypeConverter<TagCollection>
{
    protected override void WriteYamlInternal(IEmitter emitter, TagCollection? value, Type type, ObjectSerializer serializer)
    {
        if (value == null || !value.Any()) return;
        
        // Write as a flow sequence for compact representation
        emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Flow));
        
        foreach (var tag in value.OrderBy(t => t))
        {
            emitter.Emit(new Scalar(null, null, tag, ScalarStyle.Plain, true, false));
        }
        
        emitter.Emit(new SequenceEnd());
    }
    
    protected override TagCollection? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var tags = new TagCollection();
        
        if (IsSequenceStartAndShift(parser))
        {
            while (!IsSequenceEndAndShift(parser))
            {
                var tag = ReadValueAndShift(parser);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag.Trim());
                }
            }
        }
        
        return tags;
    }
}

[YamlTypeConverter(typeof(TagCollectionConverter))]
public class TagCollection : HashSet<string>
{
    public TagCollection() : base(StringComparer.OrdinalIgnoreCase) { }
    
    public TagCollection(IEnumerable<string> tags) : base(tags, StringComparer.OrdinalIgnoreCase) { }
}

public class Article
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TagCollection Tags { get; set; } = new();
}

// Usage example
public void DemonstrateCollectionConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new TagCollectionConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new TagCollectionConverter())
        .Build();
    
    var article = new Article
    {
        Title = "YAML Serialization Guide",
        Content = "Comprehensive guide to YAML serialization...",
        Tags = new TagCollection(new[] { "yaml", "serialization", "dotnet", "tutorial" })
    };
    
    var yaml = serializer.Serialize(article);
    Console.WriteLine("Article YAML:");
    Console.WriteLine(yaml);
    // Output:
    // Title: YAML Serialization Guide
    // Content: Comprehensive guide to YAML serialization...
    // Tags: [dotnet, serialization, tutorial, yaml]
    
    var deserialized = deserializer.Deserialize<Article>(yaml);
    Console.WriteLine($"\nArticle: {deserialized.Title}");
    Console.WriteLine($"Tags: {string.Join(", ", deserialized.Tags)}");
}
```

### Polymorphic Type Converter

```csharp
// Base class with type discrimination
public abstract class Shape
{
    public abstract string Type { get; }
    public string Color { get; set; } = "Black";
}

public class Circle : Shape
{
    public override string Type => "circle";
    public double Radius { get; set; }
}

public class Rectangle : Shape
{
    public override string Type => "rectangle";
    public double Width { get; set; }
    public double Height { get; set; }
}

// Polymorphic converter
public class ShapeYamlConverter : YamlTypeConverter<Shape>
{
    protected override void WriteYamlInternal(IEmitter emitter, Shape? value, Type type, ObjectSerializer serializer)
    {
        if (value == null) return;
        
        WriteMappingStart(emitter);
        WriteKeyValue(emitter, "type", value.Type);
        WriteKeyValue(emitter, "color", value.Color);
        
        switch (value)
        {
            case Circle circle:
                WriteNumber(emitter, "radius", circle.Radius);
                break;
                
            case Rectangle rectangle:
                WriteNumber(emitter, "width", rectangle.Width);
                WriteNumber(emitter, "height", rectangle.Height);
                break;
        }
        
        WriteMappingEnd(emitter);
    }
    
    protected override Shape? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var properties = new Dictionary<string, object?>();
        
        // Read all properties first
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            var value = ReadValueAndShift(parser);
            properties[key] = value;
        }
        
        // Create appropriate type based on discriminator
        if (!properties.TryGetValue("type", out var typeValue) || typeValue is not string shapeType)
            return null;
        
        Shape shape = shapeType switch
        {
            "circle" => new Circle(),
            "rectangle" => new Rectangle(),
            _ => throw new InvalidOperationException($"Unknown shape type: {shapeType}")
        };
        
        // Set common properties
        if (properties.TryGetValue("color", out var color) && color is string colorStr)
            shape.Color = colorStr;
        
        // Set type-specific properties
        switch (shape)
        {
            case Circle circle when properties.TryGetValue("radius", out var radius):
                circle.Radius = Convert.ToDouble(radius);
                break;
                
            case Rectangle rectangle:
                if (properties.TryGetValue("width", out var width))
                    rectangle.Width = Convert.ToDouble(width);
                if (properties.TryGetValue("height", out var height))
                    rectangle.Height = Convert.ToDouble(height);
                break;
        }
        
        return shape;
    }
}

[YamlTypeConverter(typeof(ShapeYamlConverter))]
public class Drawing
{
    public string Name { get; set; } = string.Empty;
    public List<Shape> Shapes { get; set; } = new();
}

// Usage example
public void DemonstratePolymorphicConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new ShapeYamlConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new ShapeYamlConverter())
        .Build();
    
    var drawing = new Drawing
    {
        Name = "Sample Drawing",
        Shapes = new List<Shape>
        {
            new Circle { Radius = 5.0, Color = "Red" },
            new Rectangle { Width = 10.0, Height = 8.0, Color = "Blue" }
        }
    };
    
    var yaml = serializer.Serialize(drawing);
    Console.WriteLine("Drawing YAML:");
    Console.WriteLine(yaml);
    
    var deserialized = deserializer.Deserialize<Drawing>(yaml);
    Console.WriteLine($"\nDrawing: {deserialized.Name}");
    foreach (var shape in deserialized.Shapes)
    {
        Console.WriteLine($"Shape: {shape.Type} - {shape.Color}");
        switch (shape)
        {
            case Circle circle:
                Console.WriteLine($"  Radius: {circle.Radius}");
                break;
            case Rectangle rectangle:
                Console.WriteLine($"  Dimensions: {rectangle.Width} x {rectangle.Height}");
                break;
        }
    }
}
```

## Integration with YamlHelper

```csharp
public static class ConfigurationLoader
{
    public static T LoadConfiguration<T>(string yamlContent) where T : new()
    {
        var deserializer = new DeserializerBuilder()
            .WithAttributeOverride<T>(GetCustomConverters<T>())
            .Build();
            
        return deserializer.Deserialize<T>(yamlContent);
    }
    
    public static string SaveConfiguration<T>(T config)
    {
        var serializer = new SerializerBuilder()
            .WithAttributeOverride<T>(GetCustomConverters<T>())
            .Build();
            
        return serializer.Serialize(config);
    }
    
    private static IEnumerable<IYamlTypeConverter> GetCustomConverters<T>()
    {
        var converters = new List<IYamlTypeConverter>();
        
        // Get type-level converter
        var typeConverter = typeof(T).GetCustomAttribute<YamlTypeConverterAttribute>();
        if (typeConverter != null)
        {
            converters.Add((IYamlTypeConverter)Activator.CreateInstance(typeConverter.ConverterType)!);
        }
        
        // Get property-level converters
        foreach (var property in typeof(T).GetProperties())
        {
            var propConverter = property.GetCustomAttribute<YamlTypeConverterAttribute>();
            if (propConverter != null)
            {
                converters.Add((IYamlTypeConverter)Activator.CreateInstance(propConverter.ConverterType)!);
            }
        }
        
        return converters.Distinct();
    }
}

// Usage with YamlHelper integration
public void DemonstrateYamlHelperIntegration()
{
    // Load configuration from file
    var yamlContent = File.ReadAllText("config.yaml");
    var config = ConfigurationLoader.LoadConfiguration<DatabaseConfig>(yamlContent);
    
    // Modify and save
    config.TimeoutSeconds = 120;
    var updatedYaml = ConfigurationLoader.SaveConfiguration(config);
    File.WriteAllText("config.yaml", updatedYaml);
    
    Console.WriteLine("Configuration updated successfully");
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class YamlTypeConverterAttributeTests
{
    [Test]
    public void TypeConverter_AppliedToClass_ShouldUseCustomSerialization()
    {
        // Arrange
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new PersonYamlConverter())
            .Build();
            
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john.doe@example.com"
        };
        
        // Act
        var yaml = serializer.Serialize(person);
        
        // Assert
        Assert.That(yaml, Contains.Substring("fullName: John Doe"));
        Assert.That(yaml, Contains.Substring("age: 30"));
        Assert.That(yaml, Contains.Substring("email: john.doe@example.com"));
    }
    
    [Test]
    public void TypeConverter_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new PersonYamlConverter())
            .Build();
            
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new PersonYamlConverter())
            .Build();
            
        var original = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25,
            Email = "jane.smith@example.com"
        };
        
        // Act
        var yaml = serializer.Serialize(original);
        var deserialized = deserializer.Deserialize<Person>(yaml);
        
        // Assert
        Assert.That(deserialized.FirstName, Is.EqualTo(original.FirstName));
        Assert.That(deserialized.LastName, Is.EqualTo(original.LastName));
        Assert.That(deserialized.Age, Is.EqualTo(original.Age));
        Assert.That(deserialized.Email, Is.EqualTo(original.Email));
    }
    
    [Test]
    public void PropertyConverter_AppliedToProperty_ShouldUseCustomFormat()
    {
        // Arrange
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new DateTimeYamlConverter())
            .Build();
            
        var eventItem = new Event
        {
            Name = "Test Event",
            StartDate = new DateTime(2023, 12, 25),
            Description = "Test Description"
        };
        
        // Act
        var yaml = serializer.Serialize(eventItem);
        
        // Assert
        Assert.That(yaml, Contains.Substring("StartDate: 2023-12-25"));
    }
}
```

## Best Practices

### 1. Implement Proper Error Handling
```csharp
public class SafeConverter : YamlTypeConverter<MyType>
{
    protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        try
        {
            // Conversion logic
        }
        catch (Exception ex)
        {
            throw new YamlException($"Failed to deserialize {type.Name}: {ex.Message}", ex);
        }
    }
}
```

### 2. Validate Input Data
```csharp
protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
{
    var obj = new MyType();
    
    while (!IsMappingEndAndShift(parser))
    {
        var key = ReadKey(parser);
        var value = ReadValueAndShift(parser);
        
        // Validate before setting
        if (key == "email" && !IsValidEmail(value))
            throw new ArgumentException($"Invalid email format: {value}");
            
        // Set property...
    }
    
    return obj;
}
```

### 3. Support Version Compatibility
```csharp
protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
{
    var obj = new MyType();
    
    while (!IsMappingEndAndShift(parser))
    {
        var key = ReadKey(parser);
        
        switch (key)
        {
            case "newProperty":
                obj.NewProperty = ReadValueAndShift(parser);
                break;
                
            case "oldProperty": // Backward compatibility
                obj.NewProperty = ConvertOldFormat(ReadValueAndShift(parser));
                break;
                
            default:
                parser.MoveNext(); // Skip unknown properties
                break;
        }
    }
    
    return obj;
}
```

### 4. Optimize Performance
```csharp
public class OptimizedConverter : YamlTypeConverter<MyType>
{
    // Cache property setters for better performance
    private static readonly Dictionary<string, Action<MyType, string?>> PropertySetters = 
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = (obj, value) => obj.Name = value ?? string.Empty,
            ["description"] = (obj, value) => obj.Description = value ?? string.Empty
        };
        
    protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var obj = new MyType();
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            var value = ReadValueAndShift(parser);
            
            if (PropertySetters.TryGetValue(key, out var setter))
                setter(obj, value);
        }
        
        return obj;
    }
}
```

## Security Considerations

### Type Safety
```csharp
public YamlTypeConverterAttribute(Type converterType)
{
    // Validate converter type implements required interface
    if (!typeof(IYamlTypeConverter).IsAssignableFrom(converterType))
        throw new ArgumentException($"Converter type must implement IYamlTypeConverter");
        
    ConverterType = converterType;
}
```

### Input Validation
```csharp
protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
{
    // Prevent injection attacks through property names
    while (!IsMappingEndAndShift(parser))
    {
        var key = ReadKey(parser);
        
        // Validate property name against whitelist
        if (!IsAllowedProperty(key))
        {
            parser.MoveNext(); // Skip disallowed property
            continue;
        }
        
        // Process allowed property...
    }
}
```

## See Also

- [YamlTypeConverter](YamlTypeConverter.md) - Base classes for custom YAML type converters
- [YamlNodeDeserializerAttribute](YamlNodeDeserializerAttribute.md) - Custom node deserializer attribute
- [YamlSerializerSettings](YamlSerializerSettings.md) - YAML serialization configuration
- [YamlHelper](../../Helpers/YamlHelper.md) - YAML serialization utilities
- [JsonSerializationAttribute](../../Attributes/JsonSerializationAttribute.md) - JSON serialization attributes

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations.Yaml namespace - providing declarative custom YAML type conversion capabilities.*