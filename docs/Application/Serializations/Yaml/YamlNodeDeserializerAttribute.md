# YamlNodeDeserializerAttribute

The `YamlNodeDeserializerAttribute` is a specialized attribute that enables you to specify custom YAML node deserializers for classes, interfaces, structs, enums, properties, and fields. This attribute provides fine-grained control over the deserialization process by allowing custom handling of YAML nodes during the parsing phase.

## Overview

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
public class YamlNodeDeserializerAttribute : Attribute
```

The `YamlNodeDeserializerAttribute` integrates with the YamlDotNet deserialization pipeline to provide custom node processing logic, enabling advanced scenarios like conditional deserialization, node transformation, and complex object construction patterns.

## Key Features

- **Node-Level Control**: Direct manipulation of YAML nodes during deserialization
- **Pipeline Integration**: Seamlessly integrates with YamlDotNet's deserialization pipeline
- **Flexible Targeting**: Supports multiple target types for versatile usage
- **Advanced Scenarios**: Enables complex deserialization patterns and transformations
- **Type Safety**: Compile-time specification of deserializer types
- **Performance Optimization**: Allows for optimized deserialization strategies

## Properties

### NodeDeserializer
Specifies the type of the custom YAML node deserializer.

```csharp
public Type NodeDeserializer { get; }
```

**Requirements:**
- Must implement `INodeDeserializer` interface
- Must have a parameterless constructor
- Should handle the target node types appropriately

## Usage Examples

### Basic Node Deserializer

```csharp
// Custom node deserializer for configuration objects
public class ConfigurationNodeDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        // Only handle Configuration types
        if (!typeof(IConfiguration).IsAssignableFrom(expectedType))
            return false;
        
        if (reader.Current is not MappingStart mappingStart)
            return false;
        
        // Move past the mapping start
        reader.MoveNext();
        
        var configData = new Dictionary<string, object?>();
        
        while (reader.Current is not MappingEnd)
        {
            // Read key
            if (reader.Current is Scalar keyScalar)
            {
                var key = keyScalar.Value;
                reader.MoveNext();
                
                // Read value
                var nestedValue = nestedObjectDeserializer(reader, typeof(object));
                configData[key] = nestedValue;
                
                reader.MoveNext();
            }
            else
            {
                reader.MoveNext();
            }
        }
        
        // Move past the mapping end
        reader.MoveNext();
        
        // Create configuration instance
        value = CreateConfiguration(expectedType, configData);
        return true;
    }
    
    private object CreateConfiguration(Type expectedType, Dictionary<string, object?> data)
    {
        var instance = Activator.CreateInstance(expectedType);
        
        if (instance is IConfiguration config)
        {
            foreach (var kvp in data)
            {
                config.SetValue(kvp.Key, kvp.Value);
            }
        }
        
        return instance!;
    }
}

// Configuration interface
public interface IConfiguration
{
    void SetValue(string key, object? value);
    T? GetValue<T>(string key);
}

// Apply node deserializer to configuration class
[YamlNodeDeserializer(typeof(ConfigurationNodeDeserializer))]
public class AppConfiguration : IConfiguration
{
    private readonly Dictionary<string, object?> _values = new();
    
    public void SetValue(string key, object? value) => _values[key] = value;
    public T? GetValue<T>(string key) => _values.TryGetValue(key, out var value) ? (T?)value : default;
    
    // Typed properties for common configuration
    public string ApplicationName => GetValue<string>("applicationName") ?? "DefaultApp";
    public int Port => GetValue<int>("port");
    public bool EnableLogging => GetValue<bool>("enableLogging");
}

// Usage example
public void DemonstrateBasicNodeDeserializer()
{
    var deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(new ConfigurationNodeDeserializer())
        .Build();
    
    var yaml = """
        applicationName: MyWebApp
        port: 8080
        enableLogging: true
        database:
          connectionString: Server=localhost;Database=MyDB
          timeout: 30
        """;
    
    var config = deserializer.Deserialize<AppConfiguration>(yaml);
    
    Console.WriteLine($"Application: {config.ApplicationName}");
    Console.WriteLine($"Port: {config.Port}");
    Console.WriteLine($"Logging: {config.EnableLogging}");
    Console.WriteLine($"DB Connection: {config.GetValue<Dictionary<object, object>>("database")}");
}
```

### Conditional Deserialization

```csharp
// Node deserializer that handles conditional object creation based on properties
public class ConditionalObjectDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        // Only handle types marked with our attribute
        if (expectedType.GetCustomAttribute<ConditionalDeserializationAttribute>() == null)
            return false;
        
        if (reader.Current is not MappingStart)
            return false;
        
        // First pass: read all properties to determine object type
        var properties = ReadAllProperties(reader, nestedObjectDeserializer);
        
        // Determine the concrete type based on discriminator
        var concreteType = DetermineConcreteType(expectedType, properties);
        if (concreteType == null)
            return false;
        
        // Create and populate the object
        value = CreateAndPopulateObject(concreteType, properties);
        return true;
    }
    
    private Dictionary<string, object?> ReadAllProperties(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        var properties = new Dictionary<string, object?>();
        
        reader.MoveNext(); // Move past mapping start
        
        while (reader.Current is not MappingEnd)
        {
            if (reader.Current is Scalar keyScalar)
            {
                var key = keyScalar.Value;
                reader.MoveNext();
                
                var value = nestedObjectDeserializer(reader, typeof(object));
                properties[key] = value;
                
                reader.MoveNext();
            }
            else
            {
                reader.MoveNext();
            }
        }
        
        reader.MoveNext(); // Move past mapping end
        return properties;
    }
    
    private Type? DetermineConcreteType(Type baseType, Dictionary<string, object?> properties)
    {
        if (!properties.TryGetValue("type", out var typeValue) || typeValue is not string typeName)
            return null;
        
        // Look for types that inherit from the base type
        var assembly = baseType.Assembly;
        return assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(baseType) && 
                                t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
    }
    
    private object? CreateAndPopulateObject(Type type, Dictionary<string, object?> properties)
    {
        var instance = Activator.CreateInstance(type);
        
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (properties.TryGetValue(property.Name, out var value))
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch
                {
                    // Skip properties that can't be converted
                }
            }
        }
        
        return instance;
    }
}

// Marker attribute for conditional deserialization
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ConditionalDeserializationAttribute : Attribute { }

// Base class for conditional deserialization
[ConditionalDeserialization]
[YamlNodeDeserializer(typeof(ConditionalObjectDeserializer))]
public abstract class Vehicle
{
    public abstract string Type { get; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
}

public class Car : Vehicle
{
    public override string Type => "Car";
    public int Doors { get; set; }
    public string FuelType { get; set; } = string.Empty;
}

public class Motorcycle : Vehicle
{
    public override string Type => "Motorcycle";
    public int EngineSize { get; set; }
    public bool HasSidecar { get; set; }
}

// Usage example
public void DemonstrateConditionalDeserialization()
{
    var deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(new ConditionalObjectDeserializer())
        .Build();
    
    var yaml = """
        - type: Car
          brand: Toyota
          model: Camry
          year: 2023
          doors: 4
          fuelType: Hybrid
        - type: Motorcycle
          brand: Harley-Davidson
          model: Street 750
          year: 2023
          engineSize: 750
          hasSidecar: false
        """;
    
    var vehicles = deserializer.Deserialize<List<Vehicle>>(yaml);
    
    foreach (var vehicle in vehicles)
    {
        Console.WriteLine($"{vehicle.Type}: {vehicle.Brand} {vehicle.Model} ({vehicle.Year})");
        
        switch (vehicle)
        {
            case Car car:
                Console.WriteLine($"  Doors: {car.Doors}, Fuel: {car.FuelType}");
                break;
            case Motorcycle motorcycle:
                Console.WriteLine($"  Engine: {motorcycle.EngineSize}cc, Sidecar: {motorcycle.HasSidecar}");
                break;
        }
    }
}
```

### Advanced Property Mapping

```csharp
// Node deserializer for advanced property mapping and transformation
public class PropertyMappingDeserializer : INodeDeserializer
{
    private readonly Dictionary<Type, Dictionary<string, string>> _propertyMappings;
    
    public PropertyMappingDeserializer()
    {
        _propertyMappings = new Dictionary<Type, Dictionary<string, string>>();
        InitializeMappings();
    }
    
    private void InitializeMappings()
    {
        // Define property mappings for different types
        _propertyMappings[typeof(Person)] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["full_name"] = "FullName",
            ["email_address"] = "Email",
            ["phone_number"] = "Phone",
            ["date_of_birth"] = "DateOfBirth"
        };
        
        _propertyMappings[typeof(Address)] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["street_address"] = "Street",
            ["postal_code"] = "ZipCode",
            ["country_code"] = "CountryCode"
        };
    }
    
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        // Only handle types with property mappings
        if (!_propertyMappings.ContainsKey(expectedType))
            return false;
        
        if (reader.Current is not MappingStart)
            return false;
        
        var mappings = _propertyMappings[expectedType];
        var properties = new Dictionary<string, object?>();
        
        reader.MoveNext(); // Move past mapping start
        
        while (reader.Current is not MappingEnd)
        {
            if (reader.Current is Scalar keyScalar)
            {
                var yamlKey = keyScalar.Value;
                reader.MoveNext();
                
                // Map YAML key to property name
                var propertyName = mappings.TryGetValue(yamlKey, out var mapped) ? mapped : yamlKey;
                
                var propertyValue = nestedObjectDeserializer(reader, typeof(object));
                properties[propertyName] = propertyValue;
                
                reader.MoveNext();
            }
            else
            {
                reader.MoveNext();
            }
        }
        
        reader.MoveNext(); // Move past mapping end
        
        // Create and populate object
        value = CreateObjectFromProperties(expectedType, properties);
        return true;
    }
    
    private object? CreateObjectFromProperties(Type type, Dictionary<string, object?> properties)
    {
        var instance = Activator.CreateInstance(type);
        
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (properties.TryGetValue(property.Name, out var value))
            {
                try
                {
                    var convertedValue = ConvertValue(value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set property {property.Name}: {ex.Message}");
                }
            }
        }
        
        return instance;
    }
    
    private object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;
        
        if (targetType.IsAssignableFrom(value.GetType()))
            return value;
        
        // Handle special conversions
        if (targetType == typeof(DateTime) && value is string dateStr)
        {
            return DateTime.TryParse(dateStr, out var date) ? date : DateTime.MinValue;
        }
        
        if (targetType == typeof(DateTime?) && value is string nullableDateStr)
        {
            return DateTime.TryParse(nullableDateStr, out var date) ? date : null;
        }
        
        return Convert.ChangeType(value, targetType);
    }
}

[YamlNodeDeserializer(typeof(PropertyMappingDeserializer))]
public class Person
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Address? Address { get; set; }
}

[YamlNodeDeserializer(typeof(PropertyMappingDeserializer))]
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

// Usage example
public void DemonstratePropertyMapping()
{
    var deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(new PropertyMappingDeserializer())
        .Build();
    
    var yaml = """
        full_name: John Doe
        email_address: john.doe@example.com
        phone_number: +1-555-123-4567
        date_of_birth: 1990-05-15
        address:
          street_address: 123 Main St
          city: Anytown
          state: CA
          postal_code: 12345
          country_code: US
        """;
    
    var person = deserializer.Deserialize<Person>(yaml);
    
    Console.WriteLine($"Name: {person.FullName}");
    Console.WriteLine($"Email: {person.Email}");
    Console.WriteLine($"Phone: {person.Phone}");
    Console.WriteLine($"DOB: {person.DateOfBirth:yyyy-MM-dd}");
    
    if (person.Address != null)
    {
        Console.WriteLine($"Address: {person.Address.Street}, {person.Address.City}, {person.Address.State} {person.Address.ZipCode}");
        Console.WriteLine($"Country: {person.Address.CountryCode}");
    }
}
```

### Collection Node Deserializer

```csharp
// Specialized node deserializer for custom collection handling
public class CustomCollectionDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        // Handle custom collection types
        if (!IsCustomCollection(expectedType))
            return false;
        
        if (reader.Current is SequenceStart)
        {
            value = DeserializeSequence(reader, expectedType, nestedObjectDeserializer);
            return true;
        }
        
        if (reader.Current is MappingStart)
        {
            value = DeserializeMapping(reader, expectedType, nestedObjectDeserializer);
            return true;
        }
        
        return false;
    }
    
    private bool IsCustomCollection(Type type)
    {
        return type.IsGenericType && 
               (type.GetGenericTypeDefinition() == typeof(CustomList<>) ||
                type.GetGenericTypeDefinition() == typeof(CustomDictionary<,>));
    }
    
    private object? DeserializeSequence(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        reader.MoveNext(); // Move past sequence start
        
        var elementType = expectedType.GetGenericArguments()[0];
        var listType = typeof(CustomList<>).MakeGenericType(elementType);
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");
        
        while (reader.Current is not SequenceEnd)
        {
            var item = nestedObjectDeserializer(reader, elementType);
            addMethod?.Invoke(list, new[] { item });
            reader.MoveNext();
        }
        
        reader.MoveNext(); // Move past sequence end
        return list;
    }
    
    private object? DeserializeMapping(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        reader.MoveNext(); // Move past mapping start
        
        var keyType = expectedType.GetGenericArguments()[0];
        var valueType = expectedType.GetGenericArguments()[1];
        var dictType = typeof(CustomDictionary<,>).MakeGenericType(keyType, valueType);
        var dict = Activator.CreateInstance(dictType);
        var addMethod = dictType.GetMethod("Add", new[] { keyType, valueType });
        
        while (reader.Current is not MappingEnd)
        {
            var key = nestedObjectDeserializer(reader, keyType);
            reader.MoveNext();
            var value = nestedObjectDeserializer(reader, valueType);
            
            if (key != null)
                addMethod?.Invoke(dict, new[] { key, value });
            
            reader.MoveNext();
        }
        
        reader.MoveNext(); // Move past mapping end
        return dict;
    }
}

// Custom collection classes
[YamlNodeDeserializer(typeof(CustomCollectionDeserializer))]
public class CustomList<T> : List<T>
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    
    public void AddRange(IEnumerable<T> items, string? source = null)
    {
        base.AddRange(items);
        if (!string.IsNullOrEmpty(source))
            Description += $" (Added from {source})";
    }
}

[YamlNodeDeserializer(typeof(CustomCollectionDeserializer))]
public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    public DateTime LastModified { get; private set; } = DateTime.UtcNow;
    public int Version { get; private set; } = 1;
    
    public new void Add(TKey key, TValue value)
    {
        base.Add(key, value);
        LastModified = DateTime.UtcNow;
        Version++;
    }
    
    public new TValue this[TKey key]
    {
        get => base[key];
        set
        {
            base[key] = value;
            LastModified = DateTime.UtcNow;
            Version++;
        }
    }
}

// Usage example
public void DemonstrateCustomCollections()
{
    var deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(new CustomCollectionDeserializer())
        .Build();
    
    var listYaml = """
        - Apple
        - Banana
        - Cherry
        - Date
        """;
    
    var dictYaml = """
        name: John Doe
        age: 30
        city: New York
        country: USA
        """;
    
    var customList = deserializer.Deserialize<CustomList<string>>(listYaml);
    var customDict = deserializer.Deserialize<CustomDictionary<string, object>>(dictYaml);
    
    Console.WriteLine($"Custom List ({customList.Count} items, created at {customList.CreatedAt:HH:mm:ss}):");
    foreach (var item in customList)
    {
        Console.WriteLine($"  - {item}");
    }
    
    Console.WriteLine($"\nCustom Dictionary (Version {customDict.Version}, modified at {customDict.LastModified:HH:mm:ss}):");
    foreach (var kvp in customDict)
    {
        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
    }
}
```

### Validation Node Deserializer

```csharp
// Node deserializer with built-in validation
public class ValidatingNodeDeserializer : INodeDeserializer
{
    private readonly Dictionary<Type, List<IValidator>> _validators;
    
    public ValidatingNodeDeserializer()
    {
        _validators = new Dictionary<Type, List<IValidator>>();
        RegisterValidators();
    }
    
    private void RegisterValidators()
    {
        _validators[typeof(User)] = new List<IValidator>
        {
            new RequiredFieldValidator("Email"),
            new EmailFormatValidator(),
            new AgeRangeValidator(13, 120)
        };
    }
    
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        // Only handle types with registered validators
        if (!_validators.ContainsKey(expectedType))
            return false;
        
        if (reader.Current is not MappingStart)
            return false;
        
        // Deserialize normally first
        var tempValue = nestedObjectDeserializer(reader, expectedType);
        
        // Validate the deserialized object
        var validators = _validators[expectedType];
        var validationErrors = new List<string>();
        
        foreach (var validator in validators)
        {
            if (!validator.IsValid(tempValue, out var errorMessage))
            {
                validationErrors.Add(errorMessage);
            }
        }
        
        if (validationErrors.Any())
        {
            throw new YamlException($"Validation failed for {expectedType.Name}: {string.Join(", ", validationErrors)}");
        }
        
        value = tempValue;
        return true;
    }
}

// Validation interfaces and implementations
public interface IValidator
{
    bool IsValid(object? obj, out string errorMessage);
}

public class RequiredFieldValidator : IValidator
{
    private readonly string _fieldName;
    
    public RequiredFieldValidator(string fieldName)
    {
        _fieldName = fieldName;
    }
    
    public bool IsValid(object? obj, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (obj == null)
        {
            errorMessage = $"Object is null";
            return false;
        }
        
        var property = obj.GetType().GetProperty(_fieldName);
        if (property == null)
        {
            errorMessage = $"Property {_fieldName} not found";
            return false;
        }
        
        var value = property.GetValue(obj);
        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            errorMessage = $"Required field {_fieldName} is missing or empty";
            return false;
        }
        
        return true;
    }
}

public class EmailFormatValidator : IValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    
    public bool IsValid(object? obj, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (obj == null) return true; // Let required validator handle null
        
        var emailProperty = obj.GetType().GetProperty("Email");
        if (emailProperty?.GetValue(obj) is string email && !string.IsNullOrEmpty(email))
        {
            if (!EmailRegex.IsMatch(email))
            {
                errorMessage = $"Invalid email format: {email}";
                return false;
            }
        }
        
        return true;
    }
}

public class AgeRangeValidator : IValidator
{
    private readonly int _minAge;
    private readonly int _maxAge;
    
    public AgeRangeValidator(int minAge, int maxAge)
    {
        _minAge = minAge;
        _maxAge = maxAge;
    }
    
    public bool IsValid(object? obj, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (obj == null) return true;
        
        var ageProperty = obj.GetType().GetProperty("Age");
        if (ageProperty?.GetValue(obj) is int age)
        {
            if (age < _minAge || age > _maxAge)
            {
                errorMessage = $"Age {age} is outside valid range ({_minAge}-{_maxAge})";
                return false;
            }
        }
        
        return true;
    }
}

[YamlNodeDeserializer(typeof(ValidatingNodeDeserializer))]
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Phone { get; set; }
}

// Usage example
public void DemonstrateValidatingDeserializer()
{
    var deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(new ValidatingNodeDeserializer())
        .Build();
    
    // Valid user
    var validYaml = """
        name: John Doe
        email: john.doe@example.com
        age: 30
        phone: +1-555-123-4567
        """;
    
    try
    {
        var validUser = deserializer.Deserialize<User>(validYaml);
        Console.WriteLine($"Valid user created: {validUser.Name} ({validUser.Email})");
    }
    catch (YamlException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    
    // Invalid user (missing email, invalid age)
    var invalidYaml = """
        name: Jane Smith
        age: 150
        phone: +1-555-987-6543
        """;
    
    try
    {
        var invalidUser = deserializer.Deserialize<User>(invalidYaml);
        Console.WriteLine("This should not print - validation should fail");
    }
    catch (YamlException ex)
    {
        Console.WriteLine($"Expected validation error: {ex.Message}");
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class YamlNodeDeserializerAttributeTests
{
    [Test]
    public void NodeDeserializer_WithCustomLogic_ShouldDeserializeCorrectly()
    {
        // Arrange
        var deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new ConfigurationNodeDeserializer())
            .Build();
        
        var yaml = """
            applicationName: TestApp
            port: 9000
            enableLogging: false
            """;
        
        // Act
        var config = deserializer.Deserialize<AppConfiguration>(yaml);
        
        // Assert
        Assert.That(config.ApplicationName, Is.EqualTo("TestApp"));
        Assert.That(config.Port, Is.EqualTo(9000));
        Assert.That(config.EnableLogging, Is.False);
    }
    
    [Test]
    public void ConditionalDeserializer_WithTypeDiscriminator_ShouldCreateCorrectType()
    {
        // Arrange
        var deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new ConditionalObjectDeserializer())
            .Build();
        
        var yaml = """
            type: Car
            brand: Honda
            model: Civic
            year: 2023
            doors: 4
            """;
        
        // Act
        var vehicle = deserializer.Deserialize<Vehicle>(yaml);
        
        // Assert
        Assert.That(vehicle, Is.InstanceOf<Car>());
        Assert.That(vehicle.Brand, Is.EqualTo("Honda"));
        Assert.That(((Car)vehicle).Doors, Is.EqualTo(4));
    }
    
    [Test]
    public void ValidatingDeserializer_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new ValidatingNodeDeserializer())
            .Build();
        
        var invalidYaml = """
            name: Test User
            email: invalid-email
            age: 200
            """;
        
        // Act & Assert
        Assert.Throws<YamlException>(() => deserializer.Deserialize<User>(invalidYaml));
    }
}
```

## Best Practices

### 1. Handle Edge Cases
```csharp
public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
{
    value = null;
    
    // Validate input parameters
    if (reader == null || expectedType == null)
        return false;
    
    // Check if we can handle this type
    if (!CanHandle(expectedType))
        return false;
    
    try
    {
        // Deserialization logic with proper error handling
        value = PerformDeserialization(reader, expectedType, nestedObjectDeserializer);
        return true;
    }
    catch (Exception ex)
    {
        // Log the error and let the pipeline continue
        Console.WriteLine($"Deserialization failed: {ex.Message}");
        return false;
    }
}
```

### 2. Implement Proper Type Checking
```csharp
private bool CanHandle(Type expectedType)
{
    // Check for specific interfaces or base classes
    if (typeof(IMyInterface).IsAssignableFrom(expectedType))
        return true;
    
    // Check for specific attributes
    if (expectedType.GetCustomAttribute<MyCustomAttribute>() != null)
        return true;
    
    // Check for generic types
    if (expectedType.IsGenericType && 
        expectedType.GetGenericTypeDefinition() == typeof(MyGenericType<>))
        return true;
    
    return false;
}
```

### 3. Ensure Parser State Management
```csharp
public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
{
    value = null;
    
    // Remember the current position
    var startPosition = reader.Current;
    
    try
    {
        // Perform deserialization
        value = PerformDeserialization(reader, expectedType, nestedObjectDeserializer);
        return true;
    }
    catch
    {
        // Reset parser position if deserialization fails
        // Note: This is conceptual - actual implementation depends on parser capabilities
        return false;
    }
}
```

### 4. Optimize Performance
```csharp
public class OptimizedNodeDeserializer : INodeDeserializer
{
    // Cache reflection information
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new();
    
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        // Use cached reflection information
        var properties = PropertyCache.GetOrAdd(expectedType, type => 
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        
        var constructor = ConstructorCache.GetOrAdd(expectedType, type =>
            type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"No parameterless constructor found for {type}"));
        
        // Perform optimized deserialization
        value = OptimizedDeserialization(reader, expectedType, properties, constructor, nestedObjectDeserializer);
        return true;
    }
}
```

## Security Considerations

### Input Validation
```csharp
public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
{
    value = null;
    
    // Validate that we're allowed to deserialize this type
    if (!IsAllowedType(expectedType))
    {
        throw new UnauthorizedAccessException($"Deserialization of type {expectedType} is not allowed");
    }
    
    // Continue with safe deserialization...
}

private bool IsAllowedType(Type type)
{
    // Implement type whitelist/blacklist logic
    var allowedNamespaces = new[] { "MyApp.Models", "MyApp.Configuration" };
    return allowedNamespaces.Any(ns => type.Namespace?.StartsWith(ns) == true);
}
```

### Size Limits
```csharp
private const int MaxCollectionSize = 10000;
private const int MaxStringLength = 1000000;

private bool ValidateSize(object? obj)
{
    switch (obj)
    {
        case ICollection collection when collection.Count > MaxCollectionSize:
            throw new InvalidOperationException($"Collection size {collection.Count} exceeds maximum {MaxCollectionSize}");
        
        case string str when str.Length > MaxStringLength:
            throw new InvalidOperationException($"String length {str.Length} exceeds maximum {MaxStringLength}");
    }
    
    return true;
}
```

## See Also

- [YamlTypeConverterAttribute](YamlTypeConverterAttribute.md) - Custom YAML type converter attribute
- [YamlTypeConverter](YamlTypeConverter.md) - Base classes for custom YAML type converters
- [YamlSerializerSettings](YamlSerializerSettings.md) - YAML serialization configuration
- [YamlHelper](../../Helpers/YamlHelper.md) - YAML serialization utilities
- [JsonConverter](../Json/JsonConverter.md) - JSON converter base class

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations.Yaml namespace - providing advanced YAML node deserialization capabilities.*