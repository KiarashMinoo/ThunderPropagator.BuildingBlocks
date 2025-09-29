# YamlHelper

The `YamlHelper` class provides YAML serialization utilities for .NET applications using YamlDotNet. It offers comprehensive YAML serialization and deserialization capabilities with extensive configuration options, custom attribute support, and multiple output formats.

## Overview

```csharp
public static class YamlHelper
```

`YamlHelper` is a static utility class that provides extension methods for YAML serialization and deserialization, leveraging the YamlDotNet library with enhanced configuration options and custom attribute support.

## Key Features

- **Comprehensive YAML Serialization**: Full support for YAML serialization and deserialization
- **Configurable Settings**: Extensive configuration through `YamlSerializerSettings`
- **Custom Attributes**: Support for custom type converters and node deserializers via attributes
- **Naming Conventions**: Flexible naming convention support (CamelCase, PascalCase, etc.)
- **Multiple Output Formats**: String, byte array, and Base64 representations
- **Type Converters**: Custom type converter support for complex objects
- **Node Deserializers**: Advanced deserialization customization
- **Telemetry Integration**: Built-in activity tracking for performance monitoring

## Public API

### Static Properties

#### DefaultSerializerSettings
Global default settings for YAML serialization operations.

```csharp
public static YamlSerializerSettings DefaultSerializerSettings { get; set; }
```

### Extension Methods

#### ToYaml<T>(this T instance, YamlSerializerSettings? serializerSettings)
Serializes an object to YAML string format.

```csharp
public static string ToYaml<T>(this T instance, YamlSerializerSettings? serializerSettings = null)
```

**Parameters:**
- `instance`: The object to serialize
- `serializerSettings`: Optional serialization settings (uses defaults if null)

**Returns:** string containing YAML representation

#### FromYaml<T>(this string yaml, YamlSerializerSettings? serializerSettings)
Deserializes a YAML string to a strongly-typed object.

```csharp
public static T FromYaml<T>(this string yaml, YamlSerializerSettings? serializerSettings = null)
```

**Parameters:**
- `yaml`: The YAML string to deserialize
- `serializerSettings`: Optional deserialization settings

**Returns:** Deserialized object of type T

#### FromYaml(this string yaml, Type type, YamlSerializerSettings? serializerSettings)
Deserializes a YAML string to an object of the specified type.

```csharp
public static object? FromYaml(this string yaml, Type type, YamlSerializerSettings? serializerSettings = null)
```

**Parameters:**
- `yaml`: The YAML string to deserialize
- `type`: The target type for deserialization
- `serializerSettings`: Optional deserialization settings

**Returns:** Deserialized object of the specified type

#### ToYamlBytes<T>(this T instance, YamlSerializerSettings? serializerSettings)
Serializes an object to UTF-8 encoded byte array in YAML format.

```csharp
public static byte[] ToYamlBytes<T>(this T instance, YamlSerializerSettings? serializerSettings = null) where T : notnull
```

#### FromYamlBytes<T>(this byte[] bytes, YamlSerializerSettings? serializerSettings)
Deserializes UTF-8 encoded YAML bytes to a strongly-typed object.

```csharp
public static T? FromYamlBytes<T>(this byte[] bytes, YamlSerializerSettings? serializerSettings = null)
```

#### ToYamlBase64<T>(this T instance, YamlSerializerSettings? serializerSettings)
Serializes an object to Base64-encoded YAML string.

```csharp
public static string ToYamlBase64<T>(this T instance, YamlSerializerSettings? serializerSettings = null) where T : notnull
```

#### FromYamlBase64<T>(this string str, YamlSerializerSettings? serializerSettings)
Deserializes a Base64-encoded YAML string to a strongly-typed object.

```csharp
public static T? FromYamlBase64<T>(this string str, YamlSerializerSettings? serializerSettings = null)
```

## YamlSerializerSettings Configuration

The `YamlSerializerSettings` class provides comprehensive configuration options:

```csharp
public class YamlSerializerSettings
{
    public bool JsonCompatible { get; set; }
    public bool IgnoreFields { get; set; }
    public bool IncludeNonPublicProperties { get; set; }
    public bool EnablePrivateConstructors { get; set; }
    public INamingConvention? NamingConvention { get; set; }
    public INamingConvention? EnumNamingConvention { get; set; }
    public ITypeResolver? TypeResolver { get; set; }
    public List<IYamlTypeConverter>? TypeConverters { get; set; }
    public List<INodeDeserializer>? NodeDeserializers { get; set; }
    public ScalarStyle? Style { get; set; }
}
```

## Usage Examples

### Basic YAML Serialization

```csharp
public class BasicYamlOperations
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<string> Hobbies { get; set; }
        public Address Address { get; set; }
    }
    
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
    
    public void DemonstrateBasicSerialization()
    {
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            Hobbies = new List<string> { "Reading", "Gaming", "Cooking" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                Country = "USA"
            }
        };
        
        // Serialize to YAML
        string yaml = person.ToYaml();
        Console.WriteLine("YAML Output:");
        Console.WriteLine(yaml);
        
        // Deserialize back
        Person deserializedPerson = yaml.FromYaml<Person>();
        Console.WriteLine($"Deserialized: {deserializedPerson.Name}, Age: {deserializedPerson.Age}");
    }
}
```

### Configuration and Settings

```csharp
public class YamlConfigurationExamples
{
    public void ConfigureNamingConventions()
    {
        var data = new { FirstName = "John", LastName = "Doe", EmailAddress = "john@example.com" };
        
        // Default CamelCase naming
        string camelCase = data.ToYaml();
        Console.WriteLine("CamelCase (default):");
        Console.WriteLine(camelCase);
        
        // PascalCase naming
        var pascalSettings = new YamlSerializerSettings
        {
            NamingConvention = PascalCaseNamingConvention.Instance
        };
        string pascalCase = data.ToYaml(pascalSettings);
        Console.WriteLine("PascalCase:");
        Console.WriteLine(pascalCase);
        
        // Underscore naming
        var underscoreSettings = new YamlSerializerSettings
        {
            NamingConvention = UnderscoredNamingConvention.Instance
        };
        string underscored = data.ToYaml(underscoreSettings);
        Console.WriteLine("Underscored:");
        Console.WriteLine(underscored);
    }
    
    public void ConfigureJsonCompatibility()
    {
        var complexObject = new
        {
            Numbers = new[] { 1, 2, 3 },
            Flags = new[] { true, false, true },
            NullValue = (string?)null
        };
        
        // Standard YAML
        string standardYaml = complexObject.ToYaml();
        Console.WriteLine("Standard YAML:");
        Console.WriteLine(standardYaml);
        
        // JSON-compatible YAML
        var jsonSettings = new YamlSerializerSettings
        {
            JsonCompatible = true
        };
        string jsonCompatible = complexObject.ToYaml(jsonSettings);
        Console.WriteLine("JSON-compatible YAML:");
        Console.WriteLine(jsonCompatible);
    }
    
    public void ConfigurePrivateMembers()
    {
        var settings = new YamlSerializerSettings
        {
            IgnoreFields = false,
            IncludeNonPublicProperties = true,
            EnablePrivateConstructors = true
        };
        
        var data = new PersonWithPrivateMembers("John", 30);
        string yaml = data.ToYaml(settings);
        Console.WriteLine("With private members:");
        Console.WriteLine(yaml);
        
        PersonWithPrivateMembers restored = yaml.FromYaml<PersonWithPrivateMembers>(settings);
        Console.WriteLine($"Restored: {restored}");
    }
}

public class PersonWithPrivateMembers
{
    private string _name;
    private int _age;
    
    private PersonWithPrivateMembers() { } // Private constructor
    
    public PersonWithPrivateMembers(string name, int age)
    {
        _name = name;
        _age = age;
    }
    
    private string Name => _name;
    private int Age => _age;
    
    public override string ToString() => $"{_name} ({_age})";
}
```

### Custom Type Converters and Attributes

```csharp
[YamlTypeConverter(typeof(PersonTypeConverter))]
public class PersonWithConverter
{
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
}

public class PersonTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(PersonWithConverter);
    
    public object ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();
        var parts = scalar.Value.Split('|');
        return new PersonWithConverter
        {
            FullName = parts[0],
            BirthDate = DateTime.Parse(parts[1])
        };
    }
    
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is PersonWithConverter person)
        {
            emitter.Emit(new Scalar($"{person.FullName}|{person.BirthDate:yyyy-MM-dd}"));
        }
    }
}

public class CustomConverterExamples
{
    public void DemonstrateCustomConverter()
    {
        var person = new PersonWithConverter
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1990, 5, 15)
        };
        
        // Serialize using custom converter
        string yaml = person.ToYaml();
        Console.WriteLine("Custom format:");
        Console.WriteLine(yaml);
        
        // Deserialize using custom converter
        PersonWithConverter restored = yaml.FromYaml<PersonWithConverter>();
        Console.WriteLine($"Restored: {restored.FullName}, Born: {restored.BirthDate:yyyy-MM-dd}");
    }
}
```

### Multiple Output Formats

```csharp
public class MultipleFormatExamples
{
    public void DemonstrateMultipleFormats()
    {
        var config = new
        {
            AppName = "MyApplication",
            Version = "1.0.0",
            Settings = new
            {
                LogLevel = "Info",
                ConnectionString = "Server=localhost;Database=MyDB",
                Features = new[] { "Caching", "Monitoring", "Security" }
            }
        };
        
        // String format
        string yamlString = config.ToYaml();
        Console.WriteLine("YAML String:");
        Console.WriteLine(yamlString);
        Console.WriteLine($"Length: {yamlString.Length} characters");
        
        // Byte array format
        byte[] yamlBytes = config.ToYamlBytes();
        Console.WriteLine($"Byte array: {yamlBytes.Length} bytes");
        
        // Base64 format
        string yamlBase64 = config.ToYamlBase64();
        Console.WriteLine($"Base64: {yamlBase64.Length} characters");
        Console.WriteLine($"Base64 content: {yamlBase64}");
        
        // Verify round-trip for all formats
        var fromString = yamlString.FromYaml<object>();
        var fromBytes = yamlBytes.FromYamlBytes<object>();
        var fromBase64 = yamlBase64.FromYamlBase64<object>();
        
        Console.WriteLine("All formats deserialized successfully");
    }
}
```

### Advanced Configuration Scenarios

```csharp
public class AdvancedYamlConfiguration
{
    public void ConfigureComplexScenarios()
    {
        var advancedSettings = new YamlSerializerSettings
        {
            JsonCompatible = true,
            IgnoreFields = false,
            IncludeNonPublicProperties = true,
            EnablePrivateConstructors = true,
            NamingConvention = CamelCaseNamingConvention.Instance,
            EnumNamingConvention = UnderscoredNamingConvention.Instance,
            Style = ScalarStyle.DoubleQuoted
        };
        
        var complexData = new ComplexConfiguration
        {
            ApplicationName = "Advanced App",
            LogLevel = LogLevel.Warning,
            DatabaseSettings = new DatabaseConfig(),
            FeatureFlags = new Dictionary<string, bool>
            {
                ["EnableCaching"] = true,
                ["EnableMetrics"] = false
            }
        };
        
        string yaml = complexData.ToYaml(advancedSettings);
        Console.WriteLine("Advanced configuration YAML:");
        Console.WriteLine(yaml);
        
        ComplexConfiguration restored = yaml.FromYaml<ComplexConfiguration>(advancedSettings);
        Console.WriteLine($"Restored app: {restored.ApplicationName}");
    }
}

public class ComplexConfiguration
{
    public string ApplicationName { get; set; }
    public LogLevel LogLevel { get; set; }
    public DatabaseConfig DatabaseSettings { get; set; }
    public Dictionary<string, bool> FeatureFlags { get; set; }
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = "DefaultConnection";
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetry { get; set; } = true;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
```

### Configuration File Processing

```csharp
public class ConfigurationFileProcessor
{
    public async Task ProcessConfigurationFiles()
    {
        // Sample application configuration
        var appConfig = new
        {
            Application = new
            {
                Name = "MyWebApp",
                Version = "2.1.0",
                Environment = "Production"
            },
            Database = new
            {
                ConnectionString = "Server=prod-db;Database=MyApp;Trusted_Connection=true",
                ConnectionPoolSize = 100,
                CommandTimeout = 60
            },
            Logging = new
            {
                Level = "Information",
                Providers = new[] { "Console", "File", "Database" },
                FileSettings = new
                {
                    Path = "/var/log/myapp",
                    MaxSizeInMB = 100,
                    RetentionDays = 30
                }
            },
            Features = new Dictionary<string, object>
            {
                ["EnableSwagger"] = true,
                ["EnableCaching"] = true,
                ["CacheExpirationMinutes"] = 60,
                ["EnableMetrics"] = false
            }
        };
        
        // Save configuration to file
        string configYaml = appConfig.ToYaml();
        await File.WriteAllTextAsync("appsettings.yml", configYaml);
        Console.WriteLine("Configuration saved to appsettings.yml");
        
        // Load and validate configuration
        string loadedConfig = await File.ReadAllTextAsync("appsettings.yml");
        var config = loadedConfig.FromYaml<object>();
        Console.WriteLine("Configuration loaded and validated successfully");
        
        // Process different environment configurations
        await ProcessEnvironmentConfigs(appConfig);
    }
    
    private async Task ProcessEnvironmentConfigs(object baseConfig)
    {
        var environments = new[] { "Development", "Staging", "Production" };
        
        foreach (string env in environments)
        {
            // Modify config for environment
            var envSpecificConfig = ModifyConfigForEnvironment(baseConfig, env);
            
            // Save environment-specific configuration
            string envYaml = envSpecificConfig.ToYaml();
            await File.WriteAllTextAsync($"appsettings.{env.ToLower()}.yml", envYaml);
            
            Console.WriteLine($"Created configuration for {env} environment");
        }
    }
    
    private object ModifyConfigForEnvironment(object baseConfig, string environment)
    {
        // This would contain environment-specific logic
        // For demo purposes, return the base config
        return baseConfig;
    }
}
```

## Performance Characteristics

### Serialization Performance
```csharp
public class YamlPerformanceAnalysis
{
    public void BenchmarkYamlOperations()
    {
        var testData = new
        {
            Users = Enumerable.Range(1, 1000).Select(i => new
            {
                Id = i,
                Name = $"User {i}",
                Email = $"user{i}@example.com",
                CreatedAt = DateTime.Now.AddDays(-i),
                IsActive = i % 2 == 0
            }).ToList()
        };
        
        const int iterations = 100;
        
        // Benchmark serialization
        var sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            string _ = testData.ToYaml();
        }
        sw1.Stop();
        
        // Benchmark with different settings
        var jsonCompatibleSettings = new YamlSerializerSettings { JsonCompatible = true };
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            string _ = testData.ToYaml(jsonCompatibleSettings);
        }
        sw2.Stop();
        
        Console.WriteLine($"Standard YAML: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"JSON-compatible YAML: {sw2.ElapsedMilliseconds}ms");
        
        // Compare with other formats
        CompareWithOtherFormats(testData);
    }
    
    private void CompareWithOtherFormats(object data)
    {
        var sw = Stopwatch.StartNew();
        
        // YAML
        sw.Restart();
        string yaml = data.ToYaml();
        sw.Stop();
        long yamlTime = sw.ElapsedTicks;
        
        // JSON
        sw.Restart();
        string json = data.ToJson();
        sw.Stop();
        long jsonTime = sw.ElapsedTicks;
        
        // MessagePack
        sw.Restart();
        byte[] msgPack = data.ToMessagePackBytes();
        sw.Stop();
        long msgPackTime = sw.ElapsedTicks;
        
        Console.WriteLine($"YAML: {yamlTime} ticks, {yaml.Length} chars");
        Console.WriteLine($"JSON: {jsonTime} ticks, {json.Length} chars");
        Console.WriteLine($"MessagePack: {msgPackTime} ticks, {msgPack.Length} bytes");
    }
}
```

## Error Handling and Edge Cases

### Serialization Error Handling
```csharp
public class YamlErrorHandling
{
    public string SafeYamlSerialization<T>(T obj) where T : notnull
    {
        try
        {
            return obj.ToYaml();
        }
        catch (YamlException ex)
        {
            Console.WriteLine($"YAML serialization error: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during YAML serialization: {ex.Message}");
            return string.Empty;
        }
    }
    
    public T? SafeYamlDeserialization<T>(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            return default;
        
        try
        {
            return yaml.FromYaml<T>();
        }
        catch (YamlException ex)
        {
            Console.WriteLine($"YAML parsing error: {ex.Message}");
            return default;
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine($"Type conversion error: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during YAML deserialization: {ex.Message}");
            return default;
        }
    }
    
    public void HandleComplexErrorScenarios()
    {
        // Test circular references
        var parent = new CircularNode { Name = "Parent" };
        var child = new CircularNode { Name = "Child", Parent = parent };
        parent.Children = new List<CircularNode> { child };
        
        string result = SafeYamlSerialization(parent);
        Console.WriteLine($"Circular reference handled: {!string.IsNullOrEmpty(result)}");
        
        // Test invalid YAML
        string invalidYaml = "invalid: yaml: content: [unclosed";
        var parsed = SafeYamlDeserialization<object>(invalidYaml);
        Console.WriteLine($"Invalid YAML handled: {parsed == null}");
    }
}

public class CircularNode
{
    public string Name { get; set; }
    public CircularNode Parent { get; set; }
    public List<CircularNode> Children { get; set; } = new();
}
```

### Type Safety and Validation
```csharp
public class YamlValidation
{
    public T ValidateAndDeserialize<T>(string yaml, Func<T, bool> validator) where T : new()
    {
        if (string.IsNullOrWhiteSpace(yaml))
            throw new ArgumentException("YAML content cannot be empty");
        
        try
        {
            T result = yaml.FromYaml<T>();
            
            if (result == null)
                throw new InvalidOperationException("Deserialization resulted in null object");
            
            if (!validator(result))
                throw new ValidationException("Object validation failed");
            
            return result;
        }
        catch (YamlException ex)
        {
            throw new InvalidDataException($"Invalid YAML format: {ex.Message}", ex);
        }
    }
    
    public void ValidateConfigurationFile()
    {
        string configYaml = @"
application:
  name: MyApp
  version: 1.0.0
database:
  connectionString: Server=localhost;Database=MyDB
  timeout: 30
";
        
        var config = ValidateAndDeserialize<ApplicationConfig>(configYaml, cfg =>
            !string.IsNullOrEmpty(cfg.Application?.Name) &&
            !string.IsNullOrEmpty(cfg.Database?.ConnectionString) &&
            cfg.Database.Timeout > 0);
        
        Console.WriteLine($"Valid configuration loaded: {config.Application.Name}");
    }
}

public class ApplicationConfig
{
    public ApplicationInfo Application { get; set; }
    public DatabaseInfo Database { get; set; }
}

public class ApplicationInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
}

public class DatabaseInfo
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

## Integration with Configuration Systems

### ASP.NET Core Configuration
```csharp
public class YamlConfigurationIntegration
{
    public void IntegrateWithAspNetCore()
    {
        // Example of using YAML with ASP.NET Core configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        // Convert existing configuration to YAML
        var configDict = configuration.AsEnumerable().ToDictionary(kv => kv.Key, kv => kv.Value);
        string yamlConfig = configDict.ToYaml();
        
        Console.WriteLine("Configuration as YAML:");
        Console.WriteLine(yamlConfig);
        
        // Load YAML configuration
        var yamlSource = new YamlConfigurationSource();
        // Would implement IConfigurationSource for full integration
    }
    
    public void ProcessConfigurationHierarchy()
    {
        // Hierarchical configuration example
        var baseConfig = new
        {
            Logging = new { Level = "Information" },
            Database = new { Timeout = 30 }
        };
        
        var environmentOverrides = new
        {
            Logging = new { Level = "Debug" },
            Features = new { EnableSwagger = true }
        };
        
        // Serialize each level
        string baseYaml = baseConfig.ToYaml();
        string overrideYaml = environmentOverrides.ToYaml();
        
        Console.WriteLine("Base configuration:");
        Console.WriteLine(baseYaml);
        Console.WriteLine("Environment overrides:");
        Console.WriteLine(overrideYaml);
        
        // In a real scenario, you'd merge these configurations
    }
}
```

## Testing Strategies

### Unit Tests
```csharp
[Test]
public void ToYaml_WithValidObject_ReturnsValidYaml()
{
    // Arrange
    var testObject = new { Name = "Test", Value = 42 };
    
    // Act
    string yaml = testObject.ToYaml();
    
    // Assert
    Assert.That(yaml, Is.Not.Null.And.Not.Empty);
    Assert.That(yaml, Contains.Substring("name: Test"));
    Assert.That(yaml, Contains.Substring("value: 42"));
}

[Test]
public void FromYaml_WithValidYaml_ReturnsCorrectObject()
{
    // Arrange
    string yaml = "name: Test\nvalue: 42";
    
    // Act
    var result = yaml.FromYaml<dynamic>();
    
    // Assert
    Assert.That(result, Is.Not.Null);
    // Note: Dynamic object testing would require more specific assertions
}

[Test]
public void YamlRoundTrip_PreservesData()
{
    // Arrange
    var original = new TestData
    {
        StringValue = "Test",
        IntValue = 42,
        DateValue = new DateTime(2023, 1, 1),
        ListValue = new List<string> { "a", "b", "c" }
    };
    
    // Act
    string yaml = original.ToYaml();
    TestData roundTrip = yaml.FromYaml<TestData>();
    
    // Assert
    Assert.That(roundTrip.StringValue, Is.EqualTo(original.StringValue));
    Assert.That(roundTrip.IntValue, Is.EqualTo(original.IntValue));
    Assert.That(roundTrip.DateValue, Is.EqualTo(original.DateValue));
    Assert.That(roundTrip.ListValue, Is.EqualTo(original.ListValue));
}

public class TestData
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public DateTime DateValue { get; set; }
    public List<string> ListValue { get; set; }
}
```

### Integration Tests
```csharp
[Test]
public async Task YamlConfigurationFile_LoadAndSave_WorksCorrectly()
{
    // Arrange
    var config = new
    {
        AppName = "TestApp",
        Version = "1.0.0",
        Settings = new { LogLevel = "Debug", Port = 8080 }
    };
    
    string tempFile = Path.GetTempFileName();
    
    try
    {
        // Act - Save
        string yaml = config.ToYaml();
        await File.WriteAllTextAsync(tempFile, yaml);
        
        // Act - Load
        string loadedYaml = await File.ReadAllTextAsync(tempFile);
        var loadedConfig = loadedYaml.FromYaml<object>();
        
        // Assert
        Assert.That(loadedConfig, Is.Not.Null);
        Assert.That(File.Exists(tempFile), Is.True);
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

## Best Practices

### 1. Configuration Management
```csharp
// Preferred - Use static settings for consistency
YamlHelper.DefaultSerializerSettings = new YamlSerializerSettings
{
    NamingConvention = CamelCaseNamingConvention.Instance,
    JsonCompatible = true
};

// Then use without specifying settings each time
string yaml = data.ToYaml();
```

### 2. Error Handling
```csharp
public T SafeYamlOperation<T>(Func<T> operation)
{
    try
    {
        return operation();
    }
    catch (YamlException ex)
    {
        Logger.LogError($"YAML error: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        Logger.LogError($"Unexpected error in YAML operation: {ex.Message}");
        throw;
    }
}
```

### 3. Type Safety
```csharp
// Preferred - Use strongly typed objects
public class AppConfig
{
    public string AppName { get; set; }
    public DatabaseConfig Database { get; set; }
}

var config = yamlString.FromYaml<AppConfig>();

// Avoid - Using dynamic or object types
var config = yamlString.FromYaml<object>(); // Less type safety
```

## Integration with Other Helpers

### Comparison with Other Serialization Helpers
```csharp
public class SerializationComparison
{
    public void CompareSerializationFormats<T>(T data) where T : notnull
    {
        // YAML
        string yaml = data.ToYaml();
        
        // JSON formats
        string systemJson = data.ToJson();
        string newtonsoftJson = data.ToNJson();
        
        // Binary formats
        byte[] messagePackData = data.ToMessagePackBytes();
        byte[] protobufData = data.ToProtobufBytes();
        
        Console.WriteLine($"YAML: {yaml.Length} chars");
        Console.WriteLine($"System.Text.Json: {systemJson.Length} chars");
        Console.WriteLine($"Newtonsoft.Json: {newtonsoftJson.Length} chars");
        Console.WriteLine($"MessagePack: {messagePackData.Length} bytes");
        Console.WriteLine($"Protobuf: {protobufData.Length} bytes");
        
        // Human readability comparison
        Console.WriteLine("\nHuman Readability (YAML):");
        Console.WriteLine(yaml);
    }
}
```

### String Helper Integration
```csharp
public class YamlStringIntegration
{
    public void ProcessYamlWithStringHelpers(object data)
    {
        // Serialize to YAML
        string yaml = data.ToYaml();
        
        // Use StringHelper for encoding operations
        byte[] yamlBytes = yaml.ToByteArray();
        string yamlBase64 = yaml.ToBase64();
        
        // Direct YAML byte operations
        byte[] directYamlBytes = data.ToYamlBytes();
        string directYamlBase64 = data.ToYamlBase64();
        
        // Verify consistency
        bool bytesMatch = yamlBytes.SequenceEqual(directYamlBytes);
        Console.WriteLine($"Bytes consistency: {bytesMatch}");
    }
}
```

## Migration and Upgrades

When migrating from other configuration formats:

```csharp
// Old approach - JSON configuration
private void ProcessJsonConfig()
{
    string json = File.ReadAllText("config.json");
    var config = JsonSerializer.Deserialize<AppConfig>(json);
}

// New approach - YAML configuration  
private void ProcessYamlConfig()
{
    string yaml = File.ReadAllText("config.yml");
    var config = yaml.FromYaml<AppConfig>();
}

// Migration utility
public void ConvertJsonToYaml(string jsonFile, string yamlFile)
{
    string json = File.ReadAllText(jsonFile);
    var data = json.FromJson<object>();
    string yaml = data.ToYaml();
    File.WriteAllText(yamlFile, yaml);
}
```

## See Also

- [JsonHelper](JsonHelper.md) - JSON serialization utilities
- [NJsonHelper](NJsonHelper.md) - Newtonsoft.Json serialization
- [MessagePackHelper](MessagePackHelper.md) - Binary serialization
- [StringHelper](StringHelper.md) - String manipulation utilities
- [ObjectHelper](ObjectHelper.md) - Object manipulation and compression

---

*Part of the RapidStreamer.BuildingBlocks.Application.Helpers namespace - providing comprehensive YAML serialization utilities for .NET applications.*