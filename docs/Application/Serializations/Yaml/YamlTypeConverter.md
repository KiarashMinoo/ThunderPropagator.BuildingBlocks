# YamlTypeConverter

The `YamlTypeConverter` class hierarchy provides a comprehensive foundation for building custom YAML type converters. This system includes base classes, interfaces, and utility methods that simplify the creation of robust, type-safe YAML serialization and deserialization logic.

## Overview

The YAML type converter system consists of several key components:

```csharp
public interface IYamlTypeConverter<T> : IYamlTypeConverter
public abstract class BaseYamlTypeConverter
public abstract class YamlTypeConverter : BaseYamlTypeConverter, IYamlTypeConverter
public abstract class YamlTypeConverter<T> : BaseYamlTypeConverter, IYamlTypeConverter<T>
```

This hierarchy provides flexible options for implementing custom YAML converters, from simple type-specific converters to complex multi-type handlers with advanced utility methods.

## Key Features

- **Type Safety**: Generic interfaces and base classes for compile-time type safety
- **Utility Methods**: Comprehensive set of helper methods for common YAML operations
- **Flexible Architecture**: Support for both typed and untyped conversion scenarios
- **Error Handling**: Built-in validation and error management
- **Performance Optimized**: Efficient parsing and emission patterns
- **YamlDotNet Integration**: Seamless integration with the YamlDotNet library

## Class Hierarchy

### IYamlTypeConverter<T>
Generic interface for type-specific converters.

```csharp
public interface IYamlTypeConverter<T> : IYamlTypeConverter
{
    T? ReadYaml(IParser parser, ObjectDeserializer rootDeserializer);
    void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer);
}
```

### BaseYamlTypeConverter
Abstract base class providing utility methods for YAML operations.

```csharp
public abstract class BaseYamlTypeConverter
{
    protected readonly char[] KeyCharactersThatRequireQuotes = [' ', '/', '\\', '~', ':', '$', '{', '}'];
    public abstract bool Accepts(Type type);
    // ... utility methods
}
```

### YamlTypeConverter (Non-Generic)
Abstract base for converters that handle multiple types.

```csharp
public abstract class YamlTypeConverter : BaseYamlTypeConverter, IYamlTypeConverter
{
    protected abstract void WriteYamlInternal(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);
    protected abstract object? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
}
```

### YamlTypeConverter<T> (Generic)
Type-safe base class for single-type converters.

```csharp
public abstract class YamlTypeConverter<T> : BaseYamlTypeConverter, IYamlTypeConverter<T>
{
    protected abstract void WriteYamlInternal(IEmitter emitter, T? value, Type type, ObjectSerializer serializer);
    protected abstract T? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
}
```

## Core Utility Methods

### Navigation and Validation

```csharp
// Parser navigation with validation
protected bool ShiftIf(Func<IParser, bool> check, IParser parser)
protected bool IsMappingStart(IParser parser)
protected bool IsMappingEnd(IParser parser)
protected bool IsSequenceStart(IParser parser)
protected bool IsSequenceEnd(IParser parser)

// Combined check and move operations
protected bool IsMappingStartAndShift(IParser parser)
protected bool IsMappingEndAndShift(IParser parser)
protected bool IsSequenceStartAndShift(IParser parser)
protected bool IsSequenceEndAndShift(IParser parser)
```

### Emission Helpers

```csharp
// Structure emission
protected void WriteMappingStart(IEmitter emitter)
protected void WriteMappingEnd(IEmitter emitter)
protected void WriteSequenceStart(IEmitter emitter)
protected void WriteSequenceEnd(IEmitter emitter)

// Value emission with smart quoting
protected void WriteKey(IEmitter emitter, string key)
protected void WriteValue(IEmitter emitter, string value)
protected void WriteKeyValue(IEmitter emitter, string key, string value)
```

### Type-Specific Helpers

```csharp
// Enum handling
protected void WriteEnum(IEmitter emitter, string key, Enum? value)
protected Enum? ReadEnum(IParser parser, Type type)
protected TEnum ReadEnum<TEnum>(IParser parser) where TEnum : struct

// Boolean handling
protected void WriteBoolean(IEmitter emitter, string key, bool? value)
protected bool? ReadBoolean(IParser parser)

// Numeric handling
protected void WriteNumber<TNumber>(IEmitter emitter, string key, TNumber? value) where TNumber : INumber<TNumber>
protected TNumber? ReadNumber<TNumber>(IParser parser) where TNumber : INumber<TNumber>
```

### Value Reading and Writing

```csharp
// Basic value operations
protected string? ReadValue(IParser parser)
protected string? ReadValueAndShift(IParser parser)
protected string ReadKey(IParser parser)

// Recursive serialization/deserialization
protected void Serialize(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
protected void Serialize<TAny>(IEmitter emitter, TAny? value, ObjectSerializer serializer)
protected object? Deserialize(IParser parser, Type type, ObjectDeserializer rootDeserializer)
protected TAny? Deserialize<TAny>(IParser parser, ObjectDeserializer rootDeserializer)
```

## Usage Examples

### Simple Type Converter

```csharp
// Converter for a custom Point struct
public class PointYamlConverter : YamlTypeConverter<Point>
{
    protected override void WriteYamlInternal(IEmitter emitter, Point? value, Type type, ObjectSerializer serializer)
    {
        if (!value.HasValue) return;
        
        var point = value.Value;
        
        WriteMappingStart(emitter);
        WriteNumber(emitter, "x", point.X);
        WriteNumber(emitter, "y", point.Y);
        WriteMappingEnd(emitter);
    }
    
    protected override Point? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        double x = 0, y = 0;
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            
            switch (key)
            {
                case "x":
                    x = ReadNumber<double>(parser) ?? 0;
                    parser.MoveNext();
                    break;
                    
                case "y":
                    y = ReadNumber<double>(parser) ?? 0;
                    parser.MoveNext();
                    break;
                    
                default:
                    parser.MoveNext(); // Skip unknown properties
                    break;
            }
        }
        
        return new Point(x, y);
    }
}

public struct Point
{
    public double X { get; }
    public double Y { get; }
    
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}

// Usage example
public void DemonstrateSimpleConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new PointYamlConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new PointYamlConverter())
        .Build();
    
    var point = new Point(3.14, 2.71);
    
    var yaml = serializer.Serialize(point);
    Console.WriteLine("Point YAML:");
    Console.WriteLine(yaml);
    // Output:
    // x: 3.14
    // y: 2.71
    
    var deserialized = deserializer.Deserialize<Point>(yaml);
    Console.WriteLine($"Deserialized Point: ({deserialized.X}, {deserialized.Y})");
}
```

### Complex Object Converter

```csharp
// Converter for a complex configuration object
public class ServerConfigConverter : YamlTypeConverter<ServerConfig>
{
    protected override void WriteYamlInternal(IEmitter emitter, ServerConfig? value, Type type, ObjectSerializer serializer)
    {
        if (value == null) return;
        
        WriteMappingStart(emitter);
        
        // Basic properties
        WriteKeyValue(emitter, "name", value.Name);
        WriteKeyValue(emitter, "host", value.Host);
        WriteNumber(emitter, "port", value.Port);
        WriteBoolean(emitter, "ssl", value.UseSSL);
        WriteEnum(emitter, "environment", value.Environment);
        
        // Timeouts as formatted strings
        WriteKey(emitter, "timeouts");
        WriteMappingStart(emitter);
        WriteKeyValue(emitter, "connection", value.ConnectionTimeout.ToString());
        WriteKeyValue(emitter, "request", value.RequestTimeout.ToString());
        WriteMappingEnd(emitter);
        
        // Endpoints as sequence
        if (value.Endpoints.Any())
        {
            WriteKey(emitter, "endpoints");
            WriteSequenceStart(emitter);
            
            foreach (var endpoint in value.Endpoints)
            {
                Serialize(emitter, endpoint, typeof(EndpointConfig), serializer);
            }
            
            WriteSequenceEnd(emitter);
        }
        
        // Security settings as nested object
        if (value.Security != null)
        {
            WriteKey(emitter, "security");
            Serialize(emitter, value.Security, typeof(SecurityConfig), serializer);
        }
        
        WriteMappingEnd(emitter);
    }
    
    protected override ServerConfig? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var config = new ServerConfig();
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            
            switch (key)
            {
                case "name":
                    config.Name = ReadValueAndShift(parser) ?? string.Empty;
                    break;
                    
                case "host":
                    config.Host = ReadValueAndShift(parser) ?? "localhost";
                    break;
                    
                case "port":
                    config.Port = ReadNumber<int>(parser) ?? 8080;
                    parser.MoveNext();
                    break;
                    
                case "ssl":
                    config.UseSSL = ReadBoolean(parser) ?? false;
                    parser.MoveNext();
                    break;
                    
                case "environment":
                    config.Environment = ReadEnum<DeploymentEnvironment>(parser);
                    parser.MoveNext();
                    break;
                    
                case "timeouts":
                    ReadTimeouts(parser, config);
                    break;
                    
                case "endpoints":
                    ReadEndpoints(parser, config, rootDeserializer);
                    break;
                    
                case "security":
                    config.Security = Deserialize<SecurityConfig>(parser, rootDeserializer);
                    parser.MoveNext();
                    break;
                    
                default:
                    parser.MoveNext();
                    break;
            }
        }
        
        return config;
    }
    
    private void ReadTimeouts(IParser parser, ServerConfig config)
    {
        if (!IsMappingStartAndShift(parser)) return;
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            var value = ReadValueAndShift(parser);
            
            if (TimeSpan.TryParse(value, out var timespan))
            {
                switch (key)
                {
                    case "connection":
                        config.ConnectionTimeout = timespan;
                        break;
                    case "request":
                        config.RequestTimeout = timespan;
                        break;
                }
            }
        }
    }
    
    private void ReadEndpoints(IParser parser, ServerConfig config, ObjectDeserializer rootDeserializer)
    {
        if (!IsSequenceStartAndShift(parser)) return;
        
        while (!IsSequenceEndAndShift(parser))
        {
            var endpoint = Deserialize<EndpointConfig>(parser, rootDeserializer);
            if (endpoint != null)
                config.Endpoints.Add(endpoint);
            parser.MoveNext();
        }
    }
}

public class ServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
    public bool UseSSL { get; set; }
    public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Development;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(120);
    public List<EndpointConfig> Endpoints { get; set; } = new();
    public SecurityConfig? Security { get; set; }
}

public class EndpointConfig
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public bool RequiresAuth { get; set; }
}

public class SecurityConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public bool ValidateCertificates { get; set; } = true;
    public List<string> AllowedOrigins { get; set; } = new();
}

public enum DeploymentEnvironment
{
    Development,
    Testing,
    Staging,
    Production
}

// Usage example
public void DemonstrateComplexConverter()
{
    var converter = new ServerConfigConverter();
    var serializer = new SerializerBuilder()
        .WithTypeConverter(converter)
        .WithTypeConverter(new EndpointConfigConverter())
        .WithTypeConverter(new SecurityConfigConverter())
        .Build();
    
    var config = new ServerConfig
    {
        Name = "API Server",
        Host = "api.example.com",
        Port = 443,
        UseSSL = true,
        Environment = DeploymentEnvironment.Production,
        ConnectionTimeout = TimeSpan.FromSeconds(10),
        RequestTimeout = TimeSpan.FromMinutes(5),
        Endpoints = new List<EndpointConfig>
        {
            new() { Path = "/api/users", Method = "GET", RequiresAuth = true },
            new() { Path = "/api/health", Method = "GET", RequiresAuth = false }
        },
        Security = new SecurityConfig
        {
            ApiKey = "secret-key-123",
            ValidateCertificates = true,
            AllowedOrigins = new List<string> { "https://app.example.com", "https://admin.example.com" }
        }
    };
    
    var yaml = serializer.Serialize(config);
    Console.WriteLine("Server Configuration YAML:");
    Console.WriteLine(yaml);
}
```

### Multi-Type Converter

```csharp
// Converter that handles multiple related types
public class GeometryConverter : YamlTypeConverter
{
    public override bool Accepts(Type type)
    {
        return type == typeof(Circle) || 
               type == typeof(Rectangle) || 
               type == typeof(Triangle) ||
               typeof(IShape).IsAssignableFrom(type);
    }
    
    protected override void WriteYamlInternal(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is not IShape shape) return;
        
        WriteMappingStart(emitter);
        WriteKeyValue(emitter, "type", shape.GetType().Name);
        WriteNumber(emitter, "area", shape.Area);
        WriteNumber(emitter, "perimeter", shape.Perimeter);
        
        // Write shape-specific properties
        switch (shape)
        {
            case Circle circle:
                WriteNumber(emitter, "radius", circle.Radius);
                break;
                
            case Rectangle rectangle:
                WriteNumber(emitter, "width", rectangle.Width);
                WriteNumber(emitter, "height", rectangle.Height);
                break;
                
            case Triangle triangle:
                WriteKey(emitter, "sides");
                WriteSequenceStart(emitter);
                WriteNumber(emitter, string.Empty, triangle.SideA);
                WriteNumber(emitter, string.Empty, triangle.SideB);
                WriteNumber(emitter, string.Empty, triangle.SideC);
                WriteSequenceEnd(emitter);
                break;
        }
        
        WriteMappingEnd(emitter);
    }
    
    protected override object? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var properties = ReadAllProperties(parser);
        
        if (!properties.TryGetValue("type", out var typeValue) || typeValue is not string shapeName)
            return null;
        
        return shapeName switch
        {
            "Circle" => CreateCircle(properties),
            "Rectangle" => CreateRectangle(properties),
            "Triangle" => CreateTriangle(properties),
            _ => null
        };
    }
    
    private Dictionary<string, object?> ReadAllProperties(IParser parser)
    {
        var properties = new Dictionary<string, object?>();
        
        while (!IsMappingEndAndShift(parser))
        {
            var key = ReadKey(parser);
            
            if (key == "sides" && IsSequenceStartAndShift(parser))
            {
                var sides = new List<double>();
                while (!IsSequenceEndAndShift(parser))
                {
                    var side = ReadNumber<double>(parser) ?? 0;
                    sides.Add(side);
                    parser.MoveNext();
                }
                properties[key] = sides.ToArray();
            }
            else
            {
                var value = ReadValueAndShift(parser);
                if (double.TryParse(value, out var numValue))
                    properties[key] = numValue;
                else
                    properties[key] = value;
            }
        }
        
        return properties;
    }
    
    private IShape? CreateCircle(Dictionary<string, object?> properties)
    {
        if (properties.TryGetValue("radius", out var radiusValue) && radiusValue is double radius)
            return new Circle(radius);
        return null;
    }
    
    private IShape? CreateRectangle(Dictionary<string, object?> properties)
    {
        var hasWidth = properties.TryGetValue("width", out var widthValue) && widthValue is double width;
        var hasHeight = properties.TryGetValue("height", out var heightValue) && heightValue is double height;
        
        if (hasWidth && hasHeight)
            return new Rectangle(width, height);
        return null;
    }
    
    private IShape? CreateTriangle(Dictionary<string, object?> properties)
    {
        if (properties.TryGetValue("sides", out var sidesValue) && sidesValue is double[] sides && sides.Length == 3)
            return new Triangle(sides[0], sides[1], sides[2]);
        return null;
    }
}

// Shape interfaces and implementations
public interface IShape
{
    double Area { get; }
    double Perimeter { get; }
}

public class Circle : IShape
{
    public double Radius { get; }
    public double Area => Math.PI * Radius * Radius;
    public double Perimeter => 2 * Math.PI * Radius;
    
    public Circle(double radius) => Radius = radius;
}

public class Rectangle : IShape
{
    public double Width { get; }
    public double Height { get; }
    public double Area => Width * Height;
    public double Perimeter => 2 * (Width + Height);
    
    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }
}

public class Triangle : IShape
{
    public double SideA { get; }
    public double SideB { get; }
    public double SideC { get; }
    
    public double Area
    {
        get
        {
            var s = Perimeter / 2;
            return Math.Sqrt(s * (s - SideA) * (s - SideB) * (s - SideC));
        }
    }
    
    public double Perimeter => SideA + SideB + SideC;
    
    public Triangle(double sideA, double sideB, double sideC)
    {
        SideA = sideA;
        SideB = sideB;
        SideC = sideC;
    }
}

// Usage example
public void DemonstrateMultiTypeConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new GeometryConverter())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new GeometryConverter())
        .Build();
    
    var shapes = new IShape[]
    {
        new Circle(5.0),
        new Rectangle(4.0, 6.0),
        new Triangle(3.0, 4.0, 5.0)
    };
    
    foreach (var shape in shapes)
    {
        var yaml = serializer.Serialize(shape);
        Console.WriteLine($"{shape.GetType().Name} YAML:");
        Console.WriteLine(yaml);
        Console.WriteLine($"Area: {shape.Area:F2}, Perimeter: {shape.Perimeter:F2}");
        
        var deserialized = deserializer.Deserialize<IShape>(yaml);
        Console.WriteLine($"Deserialized Area: {deserialized.Area:F2}");
        Console.WriteLine();
    }
}
```

### Collection Converter with Custom Logic

```csharp
// Converter for a custom priority queue
public class PriorityQueueConverter<T> : YamlTypeConverter<PriorityQueue<T, int>>
{
    protected override void WriteYamlInternal(IEmitter emitter, PriorityQueue<T, int>? value, Type type, ObjectSerializer serializer)
    {
        if (value == null || value.Count == 0) return;
        
        // Convert to sorted list for serialization
        var items = new List<(T Item, int Priority)>();
        var tempQueue = new PriorityQueue<T, int>();
        
        // Drain the queue to get all items with priorities
        while (value.TryDequeue(out var item, out var priority))
        {
            items.Add((item, priority));
            tempQueue.Enqueue(item, priority); // Keep a copy
        }
        
        // Restore original queue
        foreach (var (item, priority) in items)
        {
            value.Enqueue(item, priority);
        }
        
        // Sort by priority for consistent output
        items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
        WriteSequenceStart(emitter);
        foreach (var (item, priority) in items)
        {
            WriteMappingStart(emitter);
            WriteKey(emitter, "item");
            Serialize(emitter, item, typeof(T), serializer);
            WriteNumber(emitter, "priority", priority);
            WriteMappingEnd(emitter);
        }
        WriteSequenceEnd(emitter);
    }
    
    protected override PriorityQueue<T, int>? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var queue = new PriorityQueue<T, int>();
        
        while (!IsSequenceEndAndShift(parser))
        {
            if (IsMappingStartAndShift(parser))
            {
                T? item = default;
                int priority = 0;
                
                while (!IsMappingEndAndShift(parser))
                {
                    var key = ReadKey(parser);
                    
                    switch (key)
                    {
                        case "item":
                            item = Deserialize<T>(parser, rootDeserializer);
                            parser.MoveNext();
                            break;
                            
                        case "priority":
                            priority = ReadNumber<int>(parser) ?? 0;
                            parser.MoveNext();
                            break;
                            
                        default:
                            parser.MoveNext();
                            break;
                    }
                }
                
                if (item != null)
                    queue.Enqueue(item, priority);
            }
        }
        
        return queue;
    }
}

// Task class for demonstration
public class Task
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    
    public override string ToString() => $"{Name} (Due: {DueDate:yyyy-MM-dd})";
}

// Usage example
public void DemonstratePriorityQueueConverter()
{
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new PriorityQueueConverter<Task>())
        .Build();
        
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new PriorityQueueConverter<Task>())
        .Build();
    
    var taskQueue = new PriorityQueue<Task, int>();
    
    // Add tasks with priorities (lower number = higher priority)
    taskQueue.Enqueue(new Task 
    { 
        Name = "Fix critical bug", 
        Description = "System crashes on startup",
        DueDate = DateTime.Today.AddDays(1)
    }, 1);
    
    taskQueue.Enqueue(new Task 
    { 
        Name = "Write documentation", 
        Description = "Update API documentation",
        DueDate = DateTime.Today.AddDays(7)
    }, 3);
    
    taskQueue.Enqueue(new Task 
    { 
        Name = "Code review", 
        Description = "Review pull request #123",
        DueDate = DateTime.Today.AddDays(2)
    }, 2);
    
    var yaml = serializer.Serialize(taskQueue);
    Console.WriteLine("Priority Queue YAML:");
    Console.WriteLine(yaml);
    
    var deserializedQueue = deserializer.Deserialize<PriorityQueue<Task, int>>(yaml);
    Console.WriteLine("\nTasks in priority order:");
    while (deserializedQueue.TryDequeue(out var task, out var priority))
    {
        Console.WriteLine($"Priority {priority}: {task}");
    }
}
```

### Converter with Validation and Error Handling

```csharp
// Converter with comprehensive validation
public class ValidatedPersonConverter : YamlTypeConverter<Person>
{
    private readonly List<string> _validationErrors = new();
    
    protected override void WriteYamlInternal(IEmitter emitter, Person? value, Type type, ObjectSerializer serializer)
    {
        if (value == null) return;
        
        WriteMappingStart(emitter);
        
        WriteKeyValue(emitter, "firstName", value.FirstName);
        WriteKeyValue(emitter, "lastName", value.LastName);
        WriteKeyValue(emitter, "email", value.Email);
        WriteNumber(emitter, "age", value.Age);
        
        if (value.DateOfBirth.HasValue)
        {
            WriteKey(emitter, "dateOfBirth");
            WriteValue(emitter, value.DateOfBirth.Value.ToString("yyyy-MM-dd"));
        }
        
        if (value.Addresses.Any())
        {
            WriteKey(emitter, "addresses");
            WriteSequenceStart(emitter);
            foreach (var address in value.Addresses)
            {
                Serialize(emitter, address, typeof(Address), serializer);
            }
            WriteSequenceEnd(emitter);
        }
        
        WriteMappingEnd(emitter);
    }
    
    protected override Person? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        _validationErrors.Clear();
        
        var person = new Person();
        
        try
        {
            while (!IsMappingEndAndShift(parser))
            {
                var key = ReadKey(parser);
                
                switch (key)
                {
                    case "firstName":
                        person.FirstName = ValidateAndReadString(parser, "firstName", required: true);
                        break;
                        
                    case "lastName":
                        person.LastName = ValidateAndReadString(parser, "lastName", required: true);
                        break;
                        
                    case "email":
                        person.Email = ValidateAndReadEmail(parser);
                        break;
                        
                    case "age":
                        person.Age = ValidateAndReadAge(parser);
                        break;
                        
                    case "dateOfBirth":
                        person.DateOfBirth = ValidateAndReadDate(parser);
                        break;
                        
                    case "addresses":
                        person.Addresses = ReadAddresses(parser, rootDeserializer);
                        break;
                        
                    default:
                        parser.MoveNext(); // Skip unknown properties
                        break;
                }
            }
            
            // Cross-field validation
            ValidatePerson(person);
            
            if (_validationErrors.Any())
            {
                throw new YamlException($"Person validation failed: {string.Join("; ", _validationErrors)}");
            }
            
            return person;
        }
        catch (Exception ex) when (!(ex is YamlException))
        {
            throw new YamlException($"Failed to deserialize Person: {ex.Message}", ex);
        }
    }
    
    private string ValidateAndReadString(IParser parser, string fieldName, bool required = false, int? maxLength = null)
    {
        var value = ReadValueAndShift(parser) ?? string.Empty;
        
        if (required && string.IsNullOrWhiteSpace(value))
            _validationErrors.Add($"{fieldName} is required");
        
        if (maxLength.HasValue && value.Length > maxLength.Value)
            _validationErrors.Add($"{fieldName} cannot exceed {maxLength.Value} characters");
        
        return value;
    }
    
    private string ValidateAndReadEmail(IParser parser)
    {
        var email = ReadValueAndShift(parser) ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(email))
        {
            _validationErrors.Add("Email is required");
        }
        else if (!IsValidEmail(email))
        {
            _validationErrors.Add($"Invalid email format: {email}");
        }
        
        return email;
    }
    
    private int ValidateAndReadAge(IParser parser)
    {
        var age = ReadNumber<int>(parser) ?? 0;
        parser.MoveNext();
        
        if (age < 0 || age > 150)
            _validationErrors.Add($"Age must be between 0 and 150, got {age}");
        
        return age;
    }
    
    private DateTime? ValidateAndReadDate(IParser parser)
    {
        var dateStr = ReadValueAndShift(parser);
        
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;
        
        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", null, DateTimeStyles.None, out var date))
        {
            if (date > DateTime.Today)
                _validationErrors.Add("Date of birth cannot be in the future");
            
            return date;
        }
        
        _validationErrors.Add($"Invalid date format: {dateStr}. Expected yyyy-MM-dd");
        return null;
    }
    
    private List<Address> ReadAddresses(IParser parser, ObjectDeserializer rootDeserializer)
    {
        var addresses = new List<Address>();
        
        if (!IsSequenceStartAndShift(parser))
            return addresses;
        
        while (!IsSequenceEndAndShift(parser))
        {
            try
            {
                var address = Deserialize<Address>(parser, rootDeserializer);
                if (address != null)
                    addresses.Add(address);
            }
            catch (Exception ex)
            {
                _validationErrors.Add($"Invalid address: {ex.Message}");
            }
            
            parser.MoveNext();
        }
        
        return addresses;
    }
    
    private void ValidatePerson(Person person)
    {
        // Business rule validations
        if (person.Age > 0 && person.DateOfBirth.HasValue)
        {
            var calculatedAge = DateTime.Today.Year - person.DateOfBirth.Value.Year;
            if (person.DateOfBirth.Value.AddYears(calculatedAge) > DateTime.Today)
                calculatedAge--;
            
            if (Math.Abs(calculatedAge - person.Age) > 1)
                _validationErrors.Add("Age and date of birth are inconsistent");
        }
        
        if (person.Addresses.Count > 5)
            _validationErrors.Add("Person cannot have more than 5 addresses");
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<Address> Addresses { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

// Usage example
public void DemonstrateValidatedConverter()
{
    var deserializer = new DeserializerBuilder()
        .WithTypeConverter(new ValidatedPersonConverter())
        .WithTypeConverter(new AddressConverter())
        .Build();
    
    // Valid person
    var validYaml = """
        firstName: John
        lastName: Doe
        email: john.doe@example.com
        age: 30
        dateOfBirth: 1993-05-15
        addresses:
          - street: 123 Main St
            city: Anytown
            state: CA
            zipCode: 12345
            country: USA
        """;
    
    try
    {
        var person = deserializer.Deserialize<Person>(validYaml);
        Console.WriteLine($"Valid person: {person.FirstName} {person.LastName}");
    }
    catch (YamlException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    
    // Invalid person
    var invalidYaml = """
        firstName: 
        lastName: Doe
        email: invalid-email
        age: 200
        dateOfBirth: 2030-01-01
        """;
    
    try
    {
        var person = deserializer.Deserialize<Person>(invalidYaml);
        Console.WriteLine("This should not print");
    }
    catch (YamlException ex)
    {
        Console.WriteLine($"Expected validation errors: {ex.Message}");
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class YamlTypeConverterTests
{
    [Test]
    public void PointConverter_Serialization_ShouldProduceCorrectYaml()
    {
        // Arrange
        var converter = new PointYamlConverter();
        var serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        
        var point = new Point(3.14, 2.71);
        
        // Act
        var yaml = serializer.Serialize(point);
        
        // Assert
        Assert.That(yaml, Contains.Substring("x: 3.14"));
        Assert.That(yaml, Contains.Substring("y: 2.71"));
    }
    
    [Test]
    public void PointConverter_Deserialization_ShouldRecreateObject()
    {
        // Arrange
        var converter = new PointYamlConverter();
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        
        var yaml = "x: 10.5\ny: 20.7";
        
        // Act
        var point = deserializer.Deserialize<Point>(yaml);
        
        // Assert
        Assert.That(point.X, Is.EqualTo(10.5).Within(0.001));
        Assert.That(point.Y, Is.EqualTo(20.7).Within(0.001));
    }
    
    [Test]
    public void PointConverter_RoundTrip_ShouldPreserveValues()
    {
        // Arrange
        var converter = new PointYamlConverter();
        var serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        
        var original = new Point(Math.PI, Math.E);
        
        // Act
        var yaml = serializer.Serialize(original);
        var deserialized = deserializer.Deserialize<Point>(yaml);
        
        // Assert
        Assert.That(deserialized.X, Is.EqualTo(original.X).Within(0.0001));
        Assert.That(deserialized.Y, Is.EqualTo(original.Y).Within(0.0001));
    }
    
    [Test]
    public void ValidatedConverter_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var converter = new ValidatedPersonConverter();
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        
        var invalidYaml = "firstName: \nemail: invalid\nage: -5";
        
        // Act & Assert
        var ex = Assert.Throws<YamlException>(() => 
            deserializer.Deserialize<Person>(invalidYaml));
        
        Assert.That(ex.Message, Contains.Substring("validation failed"));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class YamlTypeConverterIntegrationTests
{
    [Test]
    public void ComplexConverter_WithNestedObjects_ShouldHandleCorrectly()
    {
        // Arrange
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new ServerConfigConverter())
            .WithTypeConverter(new EndpointConfigConverter())
            .WithTypeConverter(new SecurityConfigConverter())
            .Build();
        
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new ServerConfigConverter())
            .WithTypeConverter(new EndpointConfigConverter())
            .WithTypeConverter(new SecurityConfigConverter())
            .Build();
        
        var original = CreateComplexServerConfig();
        
        // Act
        var yaml = serializer.Serialize(original);
        var deserialized = deserializer.Deserialize<ServerConfig>(yaml);
        
        // Assert
        Assert.That(deserialized.Name, Is.EqualTo(original.Name));
        Assert.That(deserialized.Endpoints.Count, Is.EqualTo(original.Endpoints.Count));
        Assert.That(deserialized.Security?.ApiKey, Is.EqualTo(original.Security?.ApiKey));
    }
}
```

## Best Practices

### 1. Error Handling and Validation
```csharp
protected override T? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
{
    try
    {
        var result = PerformDeserialization(parser, type, rootDeserializer);
        ValidateResult(result);
        return result;
    }
    catch (Exception ex) when (!(ex is YamlException))
    {
        throw new YamlException($"Failed to deserialize {type.Name}: {ex.Message}", ex);
    }
}
```

### 2. Performance Optimization
```csharp
public class OptimizedConverter<T> : YamlTypeConverter<T>
{
    // Cache reflection information
    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new();
    
    protected override T? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // Use cached property information for better performance
        var properties = GetCachedProperties(type);
        // ... optimized deserialization
    }
}
```

### 3. Version Compatibility
```csharp
protected override MyType? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer)
{
    var version = DetectVersion(parser);
    
    return version switch
    {
        1 => ReadVersion1(parser),
        2 => ReadVersion2(parser),
        _ => ReadLatestVersion(parser)
    };
}
```

### 4. Security Considerations
```csharp
private void ValidatePropertyName(string propertyName)
{
    if (propertyName.Contains("__") || propertyName.StartsWith("_"))
        throw new SecurityException($"Property name '{propertyName}' is not allowed");
}

private void ValidateValueSize(object? value)
{
    if (value is string str && str.Length > MaxStringLength)
        throw new ArgumentException($"String value too long: {str.Length}");
    
    if (value is ICollection collection && collection.Count > MaxCollectionSize)
        throw new ArgumentException($"Collection too large: {collection.Count}");
}
```

## See Also

- [YamlTypeConverterAttribute](YamlTypeConverterAttribute.md) - Custom YAML type converter attribute
- [YamlNodeDeserializerAttribute](YamlNodeDeserializerAttribute.md) - Custom node deserializer attribute
- [YamlSerializerSettings](YamlSerializerSettings.md) - YAML serialization configuration
- [YamlHelper](../../Helpers/YamlHelper.md) - YAML serialization utilities
- [JsonConverter](../Json/JsonConverter.md) - JSON converter base class

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations.Yaml namespace - providing comprehensive base classes for custom YAML type converters.*