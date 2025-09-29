# JsonConverter

The `JsonConverter<T>` abstract class provides a robust foundation for building custom JSON converters using System.Text.Json. It extends the base `JsonConverter<T>` with enhanced safety, validation, and utility methods for handling complex serialization scenarios.

## Overview

```csharp
public abstract class JsonConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
```

`JsonConverter<T>` simplifies the creation of custom JSON converters by providing:
- Object-only deserialization with validation
- Type-safe value reading and writing utilities
- Comprehensive type support for common .NET types
- Error handling and exception management
- Recursive object and array processing

## Key Features

- **Object Validation**: Ensures JSON input starts with an object token
- **Type-Safe Reading**: Automatic type detection and conversion during deserialization
- **Comprehensive Writing**: Support for all common .NET types including collections
- **Recursive Processing**: Handles nested objects and arrays automatically
- **Error Management**: Consistent exception handling with descriptive messages
- **Extensible Design**: Abstract base allows custom implementation while providing utilities

## Core Methods

### Read Method (Sealed)
Validates input and delegates to custom implementation.

```csharp
public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
{
    if (reader.TokenType != JsonTokenType.StartObject)
        throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");

    return ReadInternal(ref reader, typeToConvert, options);
}
```

**Validation:** Ensures JSON input begins with `{` (StartObject)
**Delegation:** Calls abstract `ReadInternal` method for custom implementation

### ReadInternal Method (Abstract)
Override this method to implement custom deserialization logic.

```csharp
protected abstract T? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
```

**Purpose:** Custom deserialization implementation
**Parameters:**
- `reader`: The UTF-8 JSON reader positioned at the start object
- `typeToConvert`: The target type for conversion
- `options`: JSON serializer options

## Utility Methods

### ThrowException
Provides consistent exception throwing with JSON context.

```csharp
protected void ThrowException(string message) => throw new JsonException(message);
```

### WriteValue (Object Value)
Writes a value without a property name (for array elements).

```csharp
protected void WriteValue(Utf8JsonWriter writer, object value)
```

### WriteValue (Key-Value Pair)
Writes a property name and value pair.

```csharp
protected void WriteValue(Utf8JsonWriter writer, string? key, object objectValue)
```

**Supported Types:**
- **Enum**: Written as integer value
- **String**: Written as JSON string
- **DateTime**: Written as ISO 8601 string
- **Numeric Types**: `long`, `int`, `float`, `double`, `decimal`
- **Boolean**: Written as JSON boolean
- **Dictionary<string, object>**: Written as JSON object
- **Object Array**: Written as JSON array
- **Null**: Written as JSON null

### ReadValue
Reads and converts JSON tokens to appropriate .NET types.

```csharp
protected object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
```

**Return Types:**
- **JsonTokenType.String**: `DateTime` (if parseable) or `string`
- **JsonTokenType.True/False**: `bool`
- **JsonTokenType.Null**: `null`
- **JsonTokenType.Number**: `long` (if fits) or `decimal`
- **JsonTokenType.StartObject**: Recursively parsed object
- **JsonTokenType.StartArray**: `List<object?>`

## Usage Examples

### Basic Custom Converter

```csharp
public class PersonConverter : JsonConverter<Person>
{
    protected override Person? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? firstName = null;
        string? lastName = null;
        DateTime? birthDate = null;
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
                
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
                
            var propertyName = reader.GetString();
            reader.Read(); // Move to property value
            
            switch (propertyName?.ToLowerInvariant())
            {
                case "firstname":
                    firstName = reader.GetString();
                    break;
                case "lastname":
                    lastName = reader.GetString();
                    break;
                case "birthdate":
                    if (reader.TryGetDateTime(out var date))
                        birthDate = date;
                    break;
                default:
                    reader.Skip(); // Skip unknown properties
                    break;
            }
        }
        
        if (firstName == null || lastName == null)
            ThrowException("FirstName and LastName are required");
            
        return new Person(firstName, lastName, birthDate);
    }
    
    public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        WriteValue(writer, "firstName", value.FirstName);
        WriteValue(writer, "lastName", value.LastName);
        
        if (value.BirthDate.HasValue)
            WriteValue(writer, "birthDate", value.BirthDate.Value);
            
        writer.WriteEndObject();
    }
}

public class Person
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime? BirthDate { get; }
    
    public Person(string firstName, string lastName, DateTime? birthDate = null)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }
}

// Usage
public void DemonstrateBasicConverter()
{
    var options = new JsonSerializerOptions();
    options.Converters.Add(new PersonConverter());
    
    var person = new Person("John", "Doe", new DateTime(1990, 5, 15));
    
    // Serialize
    var json = JsonSerializer.Serialize(person, options);
    Console.WriteLine($"JSON: {json}");
    // Output: {"firstName":"John","lastName":"Doe","birthDate":"1990-05-15T00:00:00"}
    
    // Deserialize
    var deserialized = JsonSerializer.Deserialize<Person>(json, options);
    Console.WriteLine($"Name: {deserialized?.FirstName} {deserialized?.LastName}");
    Console.WriteLine($"Birth Date: {deserialized?.BirthDate:yyyy-MM-dd}");
}
```

### Complex Object Converter with Validation

```csharp
public class ProductConverter : JsonConverter<Product>
{
    protected override Product? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? name = null;
        decimal? price = null;
        string? category = null;
        var tags = new List<string>();
        var specifications = new Dictionary<string, object>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
                
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
                
            var propertyName = reader.GetString();
            reader.Read(); // Move to property value
            
            switch (propertyName?.ToLowerInvariant())
            {
                case "name":
                    name = reader.GetString();
                    if (string.IsNullOrWhiteSpace(name))
                        ThrowException("Product name cannot be empty");
                    break;
                    
                case "price":
                    if (!reader.TryGetDecimal(out var priceValue) || priceValue < 0)
                        ThrowException("Price must be a non-negative number");
                    price = priceValue;
                    break;
                    
                case "category":
                    category = reader.GetString();
                    break;
                    
                case "tags":
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            var tag = reader.GetString();
                            if (!string.IsNullOrWhiteSpace(tag))
                                tags.Add(tag);
                        }
                    }
                    break;
                    
                case "specifications":
                    var specsValue = ReadValue(ref reader, options);
                    if (specsValue is Dictionary<string, object> specsDict)
                        specifications = specsDict;
                    break;
                    
                default:
                    // Skip unknown properties
                    reader.Skip();
                    break;
            }
        }
        
        if (name == null)
            ThrowException("Product name is required");
        if (!price.HasValue)
            ThrowException("Product price is required");
            
        return new Product(name, price.Value, category, tags, specifications);
    }
    
    public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        WriteValue(writer, "name", value.Name);
        WriteValue(writer, "price", value.Price);
        
        if (!string.IsNullOrEmpty(value.Category))
            WriteValue(writer, "category", value.Category);
            
        if (value.Tags.Any())
        {
            writer.WritePropertyName("tags");
            WriteValue(writer, value.Tags.ToArray());
        }
        
        if (value.Specifications.Any())
            WriteValue(writer, "specifications", value.Specifications);
            
        writer.WriteEndObject();
    }
}

public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    public string? Category { get; }
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, object> Specifications { get; }
    
    public Product(string name, decimal price, string? category = null, 
                   IEnumerable<string>? tags = null, 
                   IDictionary<string, object>? specifications = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price >= 0 ? price : throw new ArgumentException("Price cannot be negative", nameof(price));
        Category = category;
        Tags = tags?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        Specifications = specifications?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly() 
                        ?? new Dictionary<string, object>().AsReadOnly();
    }
}

// Usage example
public void DemonstrateComplexConverter()
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new ProductConverter());
    
    var product = new Product(
        "Gaming Laptop",
        1299.99m,
        "Electronics",
        new[] { "Gaming", "High Performance", "RGB" },
        new Dictionary<string, object>
        {
            ["CPU"] = "Intel i7-11800H",
            ["RAM"] = "16GB DDR4",
            ["Storage"] = "1TB NVMe SSD",
            ["GPU"] = "RTX 3070",
            ["Display"] = new Dictionary<string, object>
            {
                ["Size"] = "15.6 inches",
                ["Resolution"] = "1920x1080",
                ["RefreshRate"] = 144
            }
        }
    );
    
    // Serialize
    var json = JsonSerializer.Serialize(product, options);
    Console.WriteLine("Serialized JSON:");
    Console.WriteLine(json);
    
    // Deserialize
    var deserialized = JsonSerializer.Deserialize<Product>(json, options);
    Console.WriteLine($"\nDeserialized: {deserialized?.Name} - ${deserialized?.Price}");
    Console.WriteLine($"Tags: {string.Join(", ", deserialized?.Tags ?? Array.Empty<string>())}");
    Console.WriteLine($"Specifications: {deserialized?.Specifications.Count} items");
}
```

### Polymorphic Converter with Type Discrimination

```csharp
public abstract class Shape
{
    public abstract string Type { get; }
    public string Color { get; set; } = "Black";
}

public class Circle : Shape
{
    public override string Type => "Circle";
    public double Radius { get; set; }
}

public class Rectangle : Shape
{
    public override string Type => "Rectangle";
    public double Width { get; set; }
    public double Height { get; set; }
}

public class ShapeConverter : JsonConverter<Shape>
{
    protected override Shape? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the entire JSON object into a dictionary first
        var jsonObject = new Dictionary<string, object?>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
                
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                var value = ReadValue(ref reader, options);
                if (propertyName != null)
                    jsonObject[propertyName] = value;
            }
        }
        
        // Determine type from discriminator
        if (!jsonObject.TryGetValue("type", out var typeValue) || typeValue is not string shapeType)
            ThrowException("Shape type discriminator is required");
        
        Shape shape = shapeType.ToLowerInvariant() switch
        {
            "circle" => new Circle(),
            "rectangle" => new Rectangle(),
            _ => throw new JsonException($"Unknown shape type: {shapeType}")
        };
        
        // Set common properties
        if (jsonObject.TryGetValue("color", out var colorValue) && colorValue is string color)
            shape.Color = color;
        
        // Set type-specific properties
        switch (shape)
        {
            case Circle circle:
                if (jsonObject.TryGetValue("radius", out var radiusValue))
                {
                    circle.Radius = Convert.ToDouble(radiusValue);
                }
                break;
                
            case Rectangle rectangle:
                if (jsonObject.TryGetValue("width", out var widthValue))
                    rectangle.Width = Convert.ToDouble(widthValue);
                if (jsonObject.TryGetValue("height", out var heightValue))
                    rectangle.Height = Convert.ToDouble(heightValue);
                break;
        }
        
        return shape;
    }
    
    public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        WriteValue(writer, "type", value.Type);
        WriteValue(writer, "color", value.Color);
        
        switch (value)
        {
            case Circle circle:
                WriteValue(writer, "radius", circle.Radius);
                break;
                
            case Rectangle rectangle:
                WriteValue(writer, "width", rectangle.Width);
                WriteValue(writer, "height", rectangle.Height);
                break;
        }
        
        writer.WriteEndObject();
    }
}

// Usage example
public void DemonstratePolymorphicConverter()
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new ShapeConverter());
    
    var shapes = new Shape[]
    {
        new Circle { Radius = 5.0, Color = "Red" },
        new Rectangle { Width = 10.0, Height = 8.0, Color = "Blue" }
    };
    
    foreach (var shape in shapes)
    {
        Console.WriteLine($"\nOriginal: {shape.Type} - {shape.Color}");
        
        // Serialize
        var json = JsonSerializer.Serialize(shape, options);
        Console.WriteLine($"JSON: {json}");
        
        // Deserialize
        var deserialized = JsonSerializer.Deserialize<Shape>(json, options);
        Console.WriteLine($"Deserialized: {deserialized?.Type} - {deserialized?.Color}");
        
        switch (deserialized)
        {
            case Circle circle:
                Console.WriteLine($"Radius: {circle.Radius}");
                break;
            case Rectangle rectangle:
                Console.WriteLine($"Dimensions: {rectangle.Width} x {rectangle.Height}");
                break;
        }
    }
}
```

### Configuration Converter with Default Values

```csharp
public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxConnections { get; set; } = 100;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableRetries { get; set; } = true;
    public Dictionary<string, string> Settings { get; set; } = new();
}

public class DatabaseConfigConverter : JsonConverter<DatabaseConfig>
{
    protected override DatabaseConfig? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var config = new DatabaseConfig();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
                
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
                
            var propertyName = reader.GetString();
            reader.Read();
            
            switch (propertyName?.ToLowerInvariant())
            {
                case "connectionstring":
                    config.ConnectionString = reader.GetString() ?? string.Empty;
                    break;
                    
                case "maxconnections":
                    if (reader.TryGetInt32(out var maxConn) && maxConn > 0)
                        config.MaxConnections = maxConn;
                    break;
                    
                case "timeout":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var timeoutStr = reader.GetString();
                        if (TimeSpan.TryParse(timeoutStr, out var timeout))
                            config.Timeout = timeout;
                    }
                    else if (reader.TryGetInt32(out var timeoutSeconds))
                    {
                        config.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    }
                    break;
                    
                case "enableretries":
                    if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
                        config.EnableRetries = reader.GetBoolean();
                    break;
                    
                case "settings":
                    var settingsValue = ReadValue(ref reader, options);
                    if (settingsValue is Dictionary<string, object> settingsDict)
                    {
                        config.Settings = settingsDict.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.ToString() ?? string.Empty
                        );
                    }
                    break;
                    
                default:
                    reader.Skip(); // Skip unknown properties gracefully
                    break;
            }
        }
        
        // Validation
        if (string.IsNullOrWhiteSpace(config.ConnectionString))
            ThrowException("ConnectionString is required");
            
        return config;
    }
    
    public override void Write(Utf8JsonWriter writer, DatabaseConfig value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        WriteValue(writer, "connectionString", value.ConnectionString);
        WriteValue(writer, "maxConnections", value.MaxConnections);
        WriteValue(writer, "timeout", value.Timeout.ToString()); // Write as string for readability
        WriteValue(writer, "enableRetries", value.EnableRetries);
        
        if (value.Settings.Any())
        {
            WriteValue(writer, "settings", value.Settings.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));
        }
        
        writer.WriteEndObject();
    }
}

// Usage example
public void DemonstrateConfigurationConverter()
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new DatabaseConfigConverter());
    
    // Create configuration
    var config = new DatabaseConfig
    {
        ConnectionString = "Server=localhost;Database=MyApp;Integrated Security=true;",
        MaxConnections = 50,
        Timeout = TimeSpan.FromMinutes(2),
        EnableRetries = false,
        Settings = new Dictionary<string, string>
        {
            ["ApplicationName"] = "MyApplication",
            ["CommandTimeout"] = "60",
            ["Pooling"] = "true"
        }
    };
    
    // Serialize
    var json = JsonSerializer.Serialize(config, options);
    Console.WriteLine("Configuration JSON:");
    Console.WriteLine(json);
    
    // Test deserialization with partial JSON (missing optional properties)
    var partialJson = """
    {
        "connectionString": "Server=remote;Database=Prod;",
        "maxConnections": 200,
        "timeout": "00:01:30"
    }
    """;
    
    var partialConfig = JsonSerializer.Deserialize<DatabaseConfig>(partialJson, options);
    Console.WriteLine($"\nPartial config - Max Connections: {partialConfig?.MaxConnections}");
    Console.WriteLine($"Timeout: {partialConfig?.Timeout}");
    Console.WriteLine($"Enable Retries (default): {partialConfig?.EnableRetries}");
}
```

### Error Handling and Validation

```csharp
public class ValidatedOrder
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ValidatedOrderConverter : JsonConverter<ValidatedOrder>
{
    protected override ValidatedOrder? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var order = new ValidatedOrder();
        var validationErrors = new List<string>();
        
        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                    
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                    
                var propertyName = reader.GetString();
                reader.Read();
                
                try
                {
                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "orderid":
                            var orderId = reader.GetString();
                            if (string.IsNullOrWhiteSpace(orderId))
                                validationErrors.Add("OrderId cannot be empty");
                            else if (orderId.Length < 3)
                                validationErrors.Add("OrderId must be at least 3 characters");
                            else
                                order.OrderId = orderId;
                            break;
                            
                        case "orderdate":
                            if (reader.TryGetDateTime(out var orderDate))
                            {
                                if (orderDate > DateTime.UtcNow.AddDays(1))
                                    validationErrors.Add("OrderDate cannot be more than 1 day in the future");
                                else
                                    order.OrderDate = orderDate;
                            }
                            else
                            {
                                validationErrors.Add("OrderDate must be a valid date");
                            }
                            break;
                            
                        case "totalamount":
                            if (reader.TryGetDecimal(out var totalAmount))
                            {
                                if (totalAmount < 0)
                                    validationErrors.Add("TotalAmount cannot be negative");
                                else
                                    order.TotalAmount = totalAmount;
                            }
                            else
                            {
                                validationErrors.Add("TotalAmount must be a valid decimal number");
                            }
                            break;
                            
                        case "items":
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                var items = new List<OrderItem>();
                                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    var item = ReadOrderItem(ref reader, validationErrors);
                                    if (item != null)
                                        items.Add(item);
                                }
                                
                                if (items.Count == 0)
                                    validationErrors.Add("Order must contain at least one item");
                                else
                                    order.Items = items;
                            }
                            break;
                            
                        default:
                            reader.Skip();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    validationErrors.Add($"Error reading property '{propertyName}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            validationErrors.Add($"General parsing error: {ex.Message}");
        }
        
        // Validate business rules
        if (order.Items.Any() && order.TotalAmount > 0)
        {
            var calculatedTotal = order.Items.Sum(item => item.Quantity * item.UnitPrice);
            if (Math.Abs(calculatedTotal - order.TotalAmount) > 0.01m)
            {
                validationErrors.Add($"TotalAmount ({order.TotalAmount}) does not match calculated total ({calculatedTotal})");
            }
        }
        
        if (validationErrors.Any())
        {
            var errorMessage = $"Validation failed: {string.Join("; ", validationErrors)}";
            ThrowException(errorMessage);
        }
        
        return order;
    }
    
    private OrderItem? ReadOrderItem(ref Utf8JsonReader reader, List<string> validationErrors)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            validationErrors.Add("Order item must be an object");
            return null;
        }
        
        var item = new OrderItem();
        
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;
                
            var propertyName = reader.GetString();
            reader.Read();
            
            switch (propertyName?.ToLowerInvariant())
            {
                case "productid":
                    var productId = reader.GetString();
                    if (string.IsNullOrWhiteSpace(productId))
                        validationErrors.Add("Item ProductId cannot be empty");
                    else
                        item.ProductId = productId;
                    break;
                    
                case "quantity":
                    if (reader.TryGetInt32(out var quantity))
                    {
                        if (quantity <= 0)
                            validationErrors.Add("Item Quantity must be positive");
                        else
                            item.Quantity = quantity;
                    }
                    else
                    {
                        validationErrors.Add("Item Quantity must be a valid integer");
                    }
                    break;
                    
                case "unitprice":
                    if (reader.TryGetDecimal(out var unitPrice))
                    {
                        if (unitPrice < 0)
                            validationErrors.Add("Item UnitPrice cannot be negative");
                        else
                            item.UnitPrice = unitPrice;
                    }
                    else
                    {
                        validationErrors.Add("Item UnitPrice must be a valid decimal");
                    }
                    break;
            }
        }
        
        return item;
    }
    
    public override void Write(Utf8JsonWriter writer, ValidatedOrder value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        WriteValue(writer, "orderId", value.OrderId);
        WriteValue(writer, "orderDate", value.OrderDate);
        WriteValue(writer, "totalAmount", value.TotalAmount);
        
        writer.WritePropertyName("items");
        writer.WriteStartArray();
        
        foreach (var item in value.Items)
        {
            writer.WriteStartObject();
            WriteValue(writer, "productId", item.ProductId);
            WriteValue(writer, "quantity", item.Quantity);
            WriteValue(writer, "unitPrice", item.UnitPrice);
            writer.WriteEndObject();
        }
        
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}

// Usage example with error handling
public void DemonstrateErrorHandling()
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new ValidatedOrderConverter());
    
    // Valid order
    var validJson = """
    {
        "orderId": "ORD-12345",
        "orderDate": "2023-09-29T10:30:00Z",
        "totalAmount": 157.50,
        "items": [
            {
                "productId": "PROD-001",
                "quantity": 2,
                "unitPrice": 25.75
            },
            {
                "productId": "PROD-002",
                "quantity": 1,
                "unitPrice": 106.00
            }
        ]
    }
    """;
    
    try
    {
        var validOrder = JsonSerializer.Deserialize<ValidatedOrder>(validJson, options);
        Console.WriteLine($"Valid order deserialized: {validOrder?.OrderId}");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    
    // Invalid order (multiple validation errors)
    var invalidJson = """
    {
        "orderId": "",
        "orderDate": "2025-12-25T00:00:00Z",
        "totalAmount": -100,
        "items": [
            {
                "productId": "PROD-001",
                "quantity": 0,
                "unitPrice": -10.50
            }
        ]
    }
    """;
    
    try
    {
        var invalidOrder = JsonSerializer.Deserialize<ValidatedOrder>(invalidJson, options);
        Console.WriteLine("This should not print - validation should fail");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Expected validation errors: {ex.Message}");
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class JsonConverterTests
{
    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
    
    private class TestObjectConverter : JsonConverter<TestObject>
    {
        protected override TestObject? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = new TestObject();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                    
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    
                    switch (propertyName)
                    {
                        case "name":
                            obj.Name = reader.GetString() ?? string.Empty;
                            break;
                        case "value":
                            obj.Value = reader.GetInt32();
                            break;
                    }
                }
            }
            
            return obj;
        }
        
        public override void Write(Utf8JsonWriter writer, TestObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteValue(writer, "name", value.Name);
            WriteValue(writer, "value", value.Value);
            writer.WriteEndObject();
        }
    }
    
    [Test]
    public void Read_ValidObject_ShouldDeserialize()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TestObjectConverter());
        
        var json = """{"name":"test","value":42}""";
        
        // Act
        var result = JsonSerializer.Deserialize<TestObject>(json, options);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("test"));
        Assert.That(result.Value, Is.EqualTo(42));
    }
    
    [Test]
    public void Read_InvalidTokenType_ShouldThrowJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TestObjectConverter());
        
        var json = """["not","an","object"]"""; // Array instead of object
        
        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<TestObject>(json, options));
        
        Assert.That(ex.Message, Contains.Substring("only objects are supported"));
    }
    
    [Test]
    public void Write_ValidObject_ShouldSerialize()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TestObjectConverter());
        
        var obj = new TestObject { Name = "test", Value = 42 };
        
        // Act
        var json = JsonSerializer.Serialize(obj, options);
        
        // Assert
        Assert.That(json, Is.EqualTo("""{"name":"test","value":42}"""));
    }
    
    [Test]
    public void WriteValue_VariousTypes_ShouldWriteCorrectly()
    {
        // This would test the WriteValue method with different types
        // Implementation would involve creating a test converter that exposes WriteValue
    }
    
    [Test]
    public void ReadValue_VariousTokens_ShouldReturnCorrectTypes()
    {
        // This would test the ReadValue method with different JSON tokens
        // Implementation would involve creating a test converter that exposes ReadValue
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class JsonConverterIntegrationTests
{
    [Test]
    public void ComplexConverter_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new ProductConverter());
        
        var original = new Product(
            "Test Product",
            99.99m,
            "Test Category",
            new[] { "tag1", "tag2" },
            new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 }
        );
        
        // Act
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Product>(json, options);
        
        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Name, Is.EqualTo(original.Name));
        Assert.That(deserialized.Price, Is.EqualTo(original.Price));
        Assert.That(deserialized.Category, Is.EqualTo(original.Category));
        Assert.That(deserialized.Tags, Is.EqualTo(original.Tags));
        Assert.That(deserialized.Specifications.Count, Is.EqualTo(original.Specifications.Count));
    }
}
```

## Best Practices

### 1. Validate Input Early
```csharp
protected override MyType? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
{
    // Validate required properties first
    if (!HasRequiredProperty(ref reader, "id"))
        ThrowException("Required property 'id' is missing");
    
    // Continue with parsing...
}
```

### 2. Handle Unknown Properties Gracefully
```csharp
default:
    reader.Skip(); // Skip unknown properties instead of throwing
    break;
```

### 3. Use Type-Safe Reading
```csharp
if (reader.TryGetInt32(out var intValue))
{
    myObject.IntProperty = intValue;
}
else
{
    ThrowException("Expected integer value");
}
```

### 4. Implement Comprehensive Error Messages
```csharp
if (string.IsNullOrWhiteSpace(name))
    ThrowException($"Property 'name' cannot be null or empty at position {reader.Position}");
```

### 5. Consider Performance for Large Objects
```csharp
// Pre-allocate collections with expected capacity
var items = new List<Item>(capacity: 100);

// Use StringBuilder for string concatenation in loops
var sb = new StringBuilder();
```

## Error Handling

### Common Error Scenarios

```csharp
public class RobustConverter : JsonConverter<MyType>
{
    protected override MyType? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Parsing logic here
        }
        catch (FormatException ex)
        {
            ThrowException($"Format error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            ThrowException($"Invalid operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            ThrowException($"Unexpected error: {ex.Message}");
        }
        
        return null;
    }
}
```

## Performance Considerations

### Optimization Strategies

```csharp
public class OptimizedConverter : JsonConverter<MyType>
{
    // Cache property names to avoid string allocations
    private static readonly Dictionary<string, Action<MyType, ref Utf8JsonReader>> PropertySetters = 
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = (obj, ref reader) => obj.Name = reader.GetString() ?? string.Empty,
            ["value"] = (obj, ref reader) => obj.Value = reader.GetInt32()
        };
    
    protected override MyType? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var obj = new MyType();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
                
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                
                if (propertyName != null && PropertySetters.TryGetValue(propertyName, out var setter))
                {
                    setter(obj, ref reader);
                }
                else
                {
                    reader.Skip();
                }
            }
        }
        
        return obj;
    }
}
```

## See Also

- [JsonHelper](../../Helpers/JsonHelper.md) - JSON serialization utilities
- [StringHelper](../../Helpers/StringHelper.md) - String manipulation for JSON processing
- [ObjectHelper](../../Helpers/ObjectHelper.md) - Object serialization and conversion
- [JsonSerializationAttribute](../../Attributes/JsonSerializationAttribute.md) - JSON serialization attributes
- [EquatableObject](../../Objects/EquatableObject.md) - Value objects for JSON serialization

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations.Json namespace - providing robust foundation for custom JSON converters with System.Text.Json.*