# YamlSerializerSettings

The `YamlSerializerSettings` class provides comprehensive configuration options for YAML serialization and deserialization operations. It centralizes all customization settings in a single, easy-to-use configuration object that integrates seamlessly with the YamlDotNet library.

## Overview

```csharp
public class YamlSerializerSettings
```

`YamlSerializerSettings` serves as a configuration hub for YAML operations, allowing you to customize serialization behavior, formatting, naming conventions, type handling, and extensibility through custom converters and deserializers.

## Key Features

- **Flexible Formatting**: Control YAML output style and structure
- **JSON Compatibility**: Optional JSON-compatible YAML output
- **Naming Conventions**: Customizable property and enum naming strategies
- **Type Resolution**: Advanced type handling and resolution
- **Extensibility**: Support for custom type converters and node deserializers
- **Privacy Control**: Configuration for private members and constructors

## Properties

### Style
Controls the scalar style for YAML output.

```csharp
public ScalarStyle? Style { get; set; }
```

**Options:**
- `ScalarStyle.Plain` - Unquoted values where possible
- `ScalarStyle.SingleQuoted` - Single-quoted strings
- `ScalarStyle.DoubleQuoted` - Double-quoted strings
- `ScalarStyle.Literal` - Literal block style (preserves newlines)
- `ScalarStyle.Folded` - Folded block style (converts newlines to spaces)

### JsonCompatible
Enables JSON-compatible YAML output.

```csharp
public bool JsonCompatible { get; set; } = false;
```

**When enabled:**
- Uses double quotes for all strings
- Uses flow style for collections
- Ensures compatibility with JSON parsers

### IgnoreFields
Controls whether fields are ignored during serialization.

```csharp
public bool IgnoreFields { get; set; }
```

**Usage:**
- `true` - Only properties are serialized
- `false` - Both fields and properties are serialized

### IncludeNonPublicProperties
Enables serialization of non-public properties.

```csharp
public bool IncludeNonPublicProperties { get; set; }
```

**When enabled:**
- Private and protected properties are included
- Internal properties are included
- Useful for complete object state serialization

### EnablePrivateConstructors
Allows deserialization using private constructors.

```csharp
public bool EnablePrivateConstructors { get; set; }
```

**Benefits:**
- Supports immutable objects with private constructors
- Enables deserialization of types with complex initialization

### NamingConvention
Specifies the naming convention for properties.

```csharp
public INamingConvention? NamingConvention { get; set; }
```

**Common conventions:**
- `CamelCaseNamingConvention` - camelCase
- `PascalCaseNamingConvention` - PascalCase
- `UnderscoredNamingConvention` - snake_case
- `HyphenatedNamingConvention` - kebab-case

### EnumNamingConvention
Specifies the naming convention for enum values.

```csharp
public INamingConvention? EnumNamingConvention { get; set; }
```

**Allows separate naming strategy for enums**

### TypeResolver
Custom type resolution logic.

```csharp
public ITypeResolver? TypeResolver { get; set; }
```

**Use cases:**
- Dynamic type loading
- Type mapping and aliases
- Assembly resolution strategies

### TypeConverters
Collection of custom type converters.

```csharp
public IEnumerable<IYamlTypeConverter>? TypeConverters { get; set; }
```

**Enables custom serialization for specific types**

### NodeDeserializers
Collection of custom node deserializers.

```csharp
public IEnumerable<INodeDeserializer>? NodeDeserializers { get; set; }
```

**Provides fine-grained deserialization control**

## Usage Examples

### Basic Configuration

```csharp
public class BasicYamlService
{
    private readonly YamlSerializerSettings _settings;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    
    public BasicYamlService()
    {
        _settings = new YamlSerializerSettings
        {
            Style = ScalarStyle.Plain,
            JsonCompatible = false,
            IgnoreFields = true,
            NamingConvention = new CamelCaseNamingConvention()
        };
        
        _serializer = CreateSerializer(_settings);
        _deserializer = CreateDeserializer(_settings);
    }
    
    private ISerializer CreateSerializer(YamlSerializerSettings settings)
    {
        var builder = new SerializerBuilder();
        
        if (settings.Style.HasValue)
            builder = builder.WithDefaultScalarStyle(settings.Style.Value);
        
        if (settings.JsonCompatible)
            builder = builder.JsonCompatible();
        
        if (settings.IgnoreFields)
            builder = builder.IgnoreFields();
        
        if (settings.IncludeNonPublicProperties)
            builder = builder.IncludeNonPublicProperties();
        
        if (settings.NamingConvention != null)
            builder = builder.WithNamingConvention(settings.NamingConvention);
        
        if (settings.EnumNamingConvention != null)
            builder = builder.WithEnumNamingConvention(settings.EnumNamingConvention);
        
        if (settings.TypeConverters != null)
        {
            foreach (var converter in settings.TypeConverters)
                builder = builder.WithTypeConverter(converter);
        }
        
        return builder.Build();
    }
    
    private IDeserializer CreateDeserializer(YamlSerializerSettings settings)
    {
        var builder = new DeserializerBuilder();
        
        if (settings.IgnoreFields)
            builder = builder.IgnoreFields();
        
        if (settings.IncludeNonPublicProperties)
            builder = builder.IncludeNonPublicProperties();
        
        if (settings.EnablePrivateConstructors)
            builder = builder.WithObjectFactory(new PrivateConstructorObjectFactory());
        
        if (settings.NamingConvention != null)
            builder = builder.WithNamingConvention(settings.NamingConvention);
        
        if (settings.EnumNamingConvention != null)
            builder = builder.WithEnumNamingConvention(settings.EnumNamingConvention);
        
        if (settings.TypeResolver != null)
            builder = builder.WithTypeResolver(settings.TypeResolver);
        
        if (settings.TypeConverters != null)
        {
            foreach (var converter in settings.TypeConverters)
                builder = builder.WithTypeConverter(converter);
        }
        
        if (settings.NodeDeserializers != null)
        {
            foreach (var deserializer in settings.NodeDeserializers)
                builder = builder.WithNodeDeserializer(deserializer);
        }
        
        return builder.Build();
    }
    
    public string Serialize<T>(T obj) => _serializer.Serialize(obj);
    public T Deserialize<T>(string yaml) => _deserializer.Deserialize<T>(yaml);
}

// Usage example
public void DemonstrateBasicConfiguration()
{
    var service = new BasicYamlService();
    
    var person = new Person
    {
        FirstName = "John",
        LastName = "Doe",
        Age = 30,
        Email = "john.doe@example.com"
    };
    
    var yaml = service.Serialize(person);
    Console.WriteLine("Serialized YAML:");
    Console.WriteLine(yaml);
    // Output (camelCase naming):
    // firstName: John
    // lastName: Doe
    // age: 30
    // email: john.doe@example.com
    
    var deserialized = service.Deserialize<Person>(yaml);
    Console.WriteLine($"\nDeserialized: {deserialized.FirstName} {deserialized.LastName}");
}
```

### JSON-Compatible Configuration

```csharp
public class JsonCompatibleYamlService
{
    private readonly YamlSerializerSettings _settings;
    private readonly ISerializer _serializer;
    
    public JsonCompatibleYamlService()
    {
        _settings = new YamlSerializerSettings
        {
            JsonCompatible = true,
            Style = ScalarStyle.DoubleQuoted,
            NamingConvention = new CamelCaseNamingConvention()
        };
        
        _serializer = CreateSerializer(_settings);
    }
    
    private ISerializer CreateSerializer(YamlSerializerSettings settings)
    {
        var builder = new SerializerBuilder()
            .JsonCompatible()
            .WithDefaultScalarStyle(ScalarStyle.DoubleQuoted)
            .WithNamingConvention(new CamelCaseNamingConvention());
        
        return builder.Build();
    }
    
    public string SerializeForJsonCompatibility<T>(T obj)
    {
        return _serializer.Serialize(obj);
    }
}

// Usage example
public void DemonstrateJsonCompatibility()
{
    var service = new JsonCompatibleYamlService();
    
    var config = new AppConfig
    {
        ApplicationName = "My App",
        Version = "1.0.0",
        Features = new List<string> { "feature1", "feature2" },
        Settings = new Dictionary<string, object>
        {
            ["debug"] = true,
            ["timeout"] = 30,
            ["url"] = "https://api.example.com"
        }
    };
    
    var yaml = service.SerializeForJsonCompatibility(config);
    Console.WriteLine("JSON-Compatible YAML:");
    Console.WriteLine(yaml);
    // Output:
    // {
    //   "applicationName": "My App",
    //   "version": "1.0.0",
    //   "features": ["feature1", "feature2"],
    //   "settings": {
    //     "debug": true,
    //     "timeout": 30,
    //     "url": "https://api.example.com"
    //   }
    // }
}
```

### Advanced Configuration with Custom Components

```csharp
// Custom type converter for TimeSpan
public class TimeSpanYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(TimeSpan) || type == typeof(TimeSpan?);
    
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var value = (parser.Current as Scalar)?.Value;
        return TimeSpan.TryParse(value, out var result) ? result : 
               type == typeof(TimeSpan?) ? null : TimeSpan.Zero;
    }
    
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is TimeSpan timeSpan)
        {
            emitter.Emit(new Scalar(null, null, timeSpan.ToString(), ScalarStyle.Plain, true, false));
        }
    }
}

// Custom type resolver for plugin loading
public class PluginTypeResolver : ITypeResolver
{
    private readonly Dictionary<string, Type> _typeMap;
    
    public PluginTypeResolver()
    {
        _typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["Logger"] = typeof(FileLogger),
            ["Cache"] = typeof(MemoryCache),
            ["Database"] = typeof(SqlDatabase)
        };
    }
    
    public Type Resolve(Type staticType, string? dynamicType)
    {
        if (dynamicType != null && _typeMap.TryGetValue(dynamicType, out var mappedType))
            return mappedType;
        
        return staticType;
    }
}

// Custom node deserializer for configuration sections
public class ConfigSectionDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        
        if (!typeof(IConfigSection).IsAssignableFrom(expectedType))
            return false;
        
        if (reader.Current is not MappingStart)
            return false;
        
        var section = (IConfigSection)Activator.CreateInstance(expectedType)!;
        reader.MoveNext();
        
        while (reader.Current is not MappingEnd)
        {
            if (reader.Current is Scalar keyScalar)
            {
                var key = keyScalar.Value;
                reader.MoveNext();
                
                var nestedValue = nestedObjectDeserializer(reader, typeof(object));
                section.SetValue(key, nestedValue);
                
                reader.MoveNext();
            }
            else
            {
                reader.MoveNext();
            }
        }
        
        reader.MoveNext();
        value = section;
        return true;
    }
}

public class AdvancedYamlService
{
    private readonly YamlSerializerSettings _settings;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    
    public AdvancedYamlService()
    {
        _settings = new YamlSerializerSettings
        {
            Style = ScalarStyle.Plain,
            IgnoreFields = true,
            IncludeNonPublicProperties = false,
            EnablePrivateConstructors = true,
            NamingConvention = new UnderscoredNamingConvention(),
            EnumNamingConvention = new HyphenatedNamingConvention(),
            TypeResolver = new PluginTypeResolver(),
            TypeConverters = new List<IYamlTypeConverter>
            {
                new TimeSpanYamlConverter(),
                new DateTimeOffsetConverter(),
                new GuidConverter()
            },
            NodeDeserializers = new List<INodeDeserializer>
            {
                new ConfigSectionDeserializer(),
                new ValidationNodeDeserializer()
            }
        };
        
        _serializer = CreateAdvancedSerializer(_settings);
        _deserializer = CreateAdvancedDeserializer(_settings);
    }
    
    private ISerializer CreateAdvancedSerializer(YamlSerializerSettings settings)
    {
        var builder = new SerializerBuilder()
            .WithDefaultScalarStyle(settings.Style ?? ScalarStyle.Plain)
            .IgnoreFields()
            .WithNamingConvention(settings.NamingConvention!)
            .WithEnumNamingConvention(settings.EnumNamingConvention!);
        
        foreach (var converter in settings.TypeConverters!)
            builder = builder.WithTypeConverter(converter);
        
        return builder.Build();
    }
    
    private IDeserializer CreateAdvancedDeserializer(YamlSerializerSettings settings)
    {
        var builder = new DeserializerBuilder()
            .IgnoreFields()
            .WithObjectFactory(new PrivateConstructorObjectFactory())
            .WithNamingConvention(settings.NamingConvention!)
            .WithEnumNamingConvention(settings.EnumNamingConvention!)
            .WithTypeResolver(settings.TypeResolver!);
        
        foreach (var converter in settings.TypeConverters!)
            builder = builder.WithTypeConverter(converter);
        
        foreach (var deserializer in settings.NodeDeserializers!)
            builder = builder.WithNodeDeserializer(deserializer);
        
        return builder.Build();
    }
    
    public string Serialize<T>(T obj) => _serializer.Serialize(obj);
    public T Deserialize<T>(string yaml) => _deserializer.Deserialize<T>(yaml);
}

// Supporting classes
public interface IConfigSection
{
    void SetValue(string key, object? value);
    T? GetValue<T>(string key);
}

public class DatabaseSection : IConfigSection
{
    private readonly Dictionary<string, object?> _values = new();
    
    public void SetValue(string key, object? value) => _values[key] = value;
    public T? GetValue<T>(string key) => _values.TryGetValue(key, out var value) ? (T?)value : default;
    
    public string ConnectionString => GetValue<string>("connection_string") ?? string.Empty;
    public TimeSpan Timeout => GetValue<TimeSpan>("timeout");
    public int MaxConnections => GetValue<int>("max_connections");
}

// Usage example
public void DemonstrateAdvancedConfiguration()
{
    var service = new AdvancedYamlService();
    
    var config = new SystemConfig
    {
        ApplicationName = "Advanced App",
        StartupTimeout = TimeSpan.FromMinutes(5),
        Environment = DeploymentEnvironment.Production,
        Database = new DatabaseSection()
    };
    
    // Set database configuration
    config.Database.SetValue("connection_string", "Server=prod;Database=MyApp");
    config.Database.SetValue("timeout", TimeSpan.FromSeconds(30));
    config.Database.SetValue("max_connections", 100);
    
    var yaml = service.Serialize(config);
    Console.WriteLine("Advanced YAML Configuration:");
    Console.WriteLine(yaml);
    
    var deserialized = service.Deserialize<SystemConfig>(yaml);
    Console.WriteLine($"\nApplication: {deserialized.ApplicationName}");
    Console.WriteLine($"Startup Timeout: {deserialized.StartupTimeout}");
    Console.WriteLine($"Environment: {deserialized.Environment}");
    Console.WriteLine($"DB Timeout: {deserialized.Database.Timeout}");
}
```

### Configuration Presets

```csharp
public static class YamlConfigurationPresets
{
    public static YamlSerializerSettings WebApiConfiguration => new()
    {
        JsonCompatible = true,
        Style = ScalarStyle.DoubleQuoted,
        IgnoreFields = true,
        NamingConvention = new CamelCaseNamingConvention(),
        TypeConverters = new[]
        {
            new DateTimeOffsetConverter(),
            new TimeSpanConverter(),
            new UriConverter()
        }
    };
    
    public static YamlSerializerSettings ConfigurationFileSettings => new()
    {
        Style = ScalarStyle.Plain,
        IgnoreFields = true,
        IncludeNonPublicProperties = false,
        NamingConvention = new UnderscoredNamingConvention(),
        EnumNamingConvention = new HyphenatedNamingConvention()
    };
    
    public static YamlSerializerSettings DataTransferSettings => new()
    {
        JsonCompatible = false,
        Style = ScalarStyle.Plain,
        IgnoreFields = false,
        IncludeNonPublicProperties = true,
        EnablePrivateConstructors = true,
        NamingConvention = new PascalCaseNamingConvention()
    };
    
    public static YamlSerializerSettings LoggingConfiguration => new()
    {
        Style = ScalarStyle.Literal,
        IgnoreFields = true,
        NamingConvention = new CamelCaseNamingConvention(),
        TypeConverters = new[]
        {
            new ExceptionConverter(),
            new LogLevelConverter(),
            new DateTimeConverter()
        }
    };
}

// Preset usage service
public class PresetYamlService
{
    public static ISerializer CreateWebApiSerializer()
    {
        var settings = YamlConfigurationPresets.WebApiConfiguration;
        return CreateSerializer(settings);
    }
    
    public static IDeserializer CreateConfigurationDeserializer()
    {
        var settings = YamlConfigurationPresets.ConfigurationFileSettings;
        return CreateDeserializer(settings);
    }
    
    public static (ISerializer, IDeserializer) CreateDataTransferPair()
    {
        var settings = YamlConfigurationPresets.DataTransferSettings;
        return (CreateSerializer(settings), CreateDeserializer(settings));
    }
    
    private static ISerializer CreateSerializer(YamlSerializerSettings settings)
    {
        var builder = new SerializerBuilder();
        
        if (settings.JsonCompatible)
            builder = builder.JsonCompatible();
        
        if (settings.Style.HasValue)
            builder = builder.WithDefaultScalarStyle(settings.Style.Value);
        
        if (settings.IgnoreFields)
            builder = builder.IgnoreFields();
        
        if (settings.IncludeNonPublicProperties)
            builder = builder.IncludeNonPublicProperties();
        
        if (settings.NamingConvention != null)
            builder = builder.WithNamingConvention(settings.NamingConvention);
        
        if (settings.EnumNamingConvention != null)
            builder = builder.WithEnumNamingConvention(settings.EnumNamingConvention);
        
        if (settings.TypeConverters != null)
        {
            foreach (var converter in settings.TypeConverters)
                builder = builder.WithTypeConverter(converter);
        }
        
        return builder.Build();
    }
    
    private static IDeserializer CreateDeserializer(YamlSerializerSettings settings)
    {
        var builder = new DeserializerBuilder();
        
        if (settings.IgnoreFields)
            builder = builder.IgnoreFields();
        
        if (settings.IncludeNonPublicProperties)
            builder = builder.IncludeNonPublicProperties();
        
        if (settings.EnablePrivateConstructors)
            builder = builder.WithObjectFactory(new PrivateConstructorObjectFactory());
        
        if (settings.NamingConvention != null)
            builder = builder.WithNamingConvention(settings.NamingConvention);
        
        if (settings.EnumNamingConvention != null)
            builder = builder.WithEnumNamingConvention(settings.EnumNamingConvention);
        
        if (settings.TypeResolver != null)
            builder = builder.WithTypeResolver(settings.TypeResolver);
        
        if (settings.TypeConverters != null)
        {
            foreach (var converter in settings.TypeConverters)
                builder = builder.WithTypeConverter(converter);
        }
        
        if (settings.NodeDeserializers != null)
        {
            foreach (var deserializer in settings.NodeDeserializers)
                builder = builder.WithNodeDeserializer(deserializer);
        }
        
        return builder.Build();
    }
}

// Usage example
public void DemonstratePresets()
{
    // Web API serialization
    var webApiSerializer = PresetYamlService.CreateWebApiSerializer();
    var apiResponse = new { status = "success", data = new { id = 123, name = "Test" } };
    var webApiYaml = webApiSerializer.Serialize(apiResponse);
    Console.WriteLine("Web API YAML:");
    Console.WriteLine(webApiYaml);
    
    // Configuration file handling
    var configDeserializer = PresetYamlService.CreateConfigurationDeserializer();
    var configYaml = """
        application_name: MyApp
        database_timeout: 00:00:30
        log_level: information
        """;
    var config = configDeserializer.Deserialize<Dictionary<string, object>>(configYaml);
    Console.WriteLine("\nConfiguration:");
    foreach (var kvp in config)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
    
    // Data transfer
    var (dataSerializer, dataDeserializer) = PresetYamlService.CreateDataTransferPair();
    var dataObject = new ComplexDataObject
    {
        PublicProperty = "Public Value",
        InternalData = "Internal Value"
    };
    
    var dataYaml = dataSerializer.Serialize(dataObject);
    var deserializedData = dataDeserializer.Deserialize<ComplexDataObject>(dataYaml);
    Console.WriteLine($"\nData Transfer - Public: {deserializedData.PublicProperty}");
}
```

### Environment-Specific Configuration

```csharp
public class EnvironmentYamlConfigurationService
{
    public enum Environment
    {
        Development,
        Testing,
        Staging,
        Production
    }
    
    public static YamlSerializerSettings GetSettingsForEnvironment(Environment environment)
    {
        return environment switch
        {
            Environment.Development => new YamlSerializerSettings
            {
                Style = ScalarStyle.Plain,
                IgnoreFields = false, // Include everything for debugging
                IncludeNonPublicProperties = true,
                NamingConvention = new CamelCaseNamingConvention(),
                TypeConverters = new[]
                {
                    new VerboseExceptionConverter(),
                    new DetailedDateTimeConverter()
                }
            },
            
            Environment.Testing => new YamlSerializerSettings
            {
                JsonCompatible = true, // For test assertions
                Style = ScalarStyle.DoubleQuoted,
                IgnoreFields = true,
                NamingConvention = new CamelCaseNamingConvention(),
                TypeConverters = new[]
                {
                    new TestFriendlyConverter(),
                    new MockObjectConverter()
                }
            },
            
            Environment.Staging => new YamlSerializerSettings
            {
                Style = ScalarStyle.Plain,
                IgnoreFields = true,
                IncludeNonPublicProperties = false,
                NamingConvention = new UnderscoredNamingConvention(),
                TypeConverters = new[]
                {
                    new ProductionReadyConverter(),
                    new SecuritySafeConverter()
                }
            },
            
            Environment.Production => new YamlSerializerSettings
            {
                Style = ScalarStyle.Plain,
                IgnoreFields = true,
                IncludeNonPublicProperties = false,
                NamingConvention = new UnderscoredNamingConvention(),
                TypeConverters = new[]
                {
                    new OptimizedConverter(),
                    new SecurityConverter(),
                    new AuditConverter()
                },
                NodeDeserializers = new[]
                {
                    new ValidationDeserializer(),
                    new SecurityDeserializer()
                }
            },
            
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
    }
    
    public static (ISerializer, IDeserializer) CreateForEnvironment(Environment environment)
    {
        var settings = GetSettingsForEnvironment(environment);
        return (CreateSerializer(settings), CreateDeserializer(settings));
    }
    
    // Implementation methods similar to previous examples...
}

// Usage example
public void DemonstrateEnvironmentConfiguration()
{
    var env = EnvironmentYamlConfigurationService.Environment.Production;
    var (serializer, deserializer) = EnvironmentYamlConfigurationService.CreateForEnvironment(env);
    
    var sensitiveConfig = new SecurityConfig
    {
        ApiEndpoint = "https://api.production.com",
        ConnectionString = "Server=prod;Database=MyApp;",
        EncryptionKey = "***ENCRYPTED***"
    };
    
    var yaml = serializer.Serialize(sensitiveConfig);
    Console.WriteLine($"Production YAML ({env}):");
    Console.WriteLine(yaml);
    
    var deserializedConfig = deserializer.Deserialize<SecurityConfig>(yaml);
    Console.WriteLine($"Deserialized endpoint: {deserializedConfig.ApiEndpoint}");
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class YamlSerializerSettingsTests
{
    [Test]
    public void Settings_WithBasicConfiguration_ShouldCreateValidSerializer()
    {
        // Arrange
        var settings = new YamlSerializerSettings
        {
            Style = ScalarStyle.Plain,
            IgnoreFields = true,
            NamingConvention = new CamelCaseNamingConvention()
        };
        
        // Act
        var serializer = CreateSerializer(settings);
        var testObject = new { Name = "Test", Value = 42 };
        var yaml = serializer.Serialize(testObject);
        
        // Assert
        Assert.That(yaml, Contains.Substring("name: Test"));
        Assert.That(yaml, Contains.Substring("value: 42"));
    }
    
    [Test]
    public void Settings_WithJsonCompatibility_ShouldProduceJsonCompatibleOutput()
    {
        // Arrange
        var settings = new YamlSerializerSettings
        {
            JsonCompatible = true,
            Style = ScalarStyle.DoubleQuoted
        };
        
        // Act
        var serializer = CreateSerializer(settings);
        var testObject = new { message = "Hello, World!" };
        var yaml = serializer.Serialize(testObject);
        
        // Assert
        Assert.That(yaml, Contains.Substring("\"message\": \"Hello, World!\""));
    }
    
    [Test]
    public void Settings_WithCustomTypeConverter_ShouldUseConverter()
    {
        // Arrange
        var settings = new YamlSerializerSettings
        {
            TypeConverters = new[] { new TimeSpanYamlConverter() }
        };
        
        // Act
        var serializer = CreateSerializer(settings);
        var testObject = new { timeout = TimeSpan.FromMinutes(5) };
        var yaml = serializer.Serialize(testObject);
        
        // Assert
        Assert.That(yaml, Contains.Substring("timeout: 00:05:00"));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class YamlSerializerSettingsIntegrationTests
{
    [Test]
    public void Settings_RoundTripSerialization_ShouldPreserveData()
    {
        // Arrange
        var settings = new YamlSerializerSettings
        {
            IncludeNonPublicProperties = true,
            EnablePrivateConstructors = true,
            NamingConvention = new UnderscoredNamingConvention()
        };
        
        var original = new ComplexObject("test", 42, DateTime.Now);
        
        // Act
        var serializer = CreateSerializer(settings);
        var deserializer = CreateDeserializer(settings);
        
        var yaml = serializer.Serialize(original);
        var deserialized = deserializer.Deserialize<ComplexObject>(yaml);
        
        // Assert
        Assert.That(deserialized.Name, Is.EqualTo(original.Name));
        Assert.That(deserialized.Value, Is.EqualTo(original.Value));
        Assert.That(deserialized.CreatedAt.Date, Is.EqualTo(original.CreatedAt.Date));
    }
}
```

## Best Practices

### 1. Environment-Specific Settings
```csharp
public static class SettingsFactory
{
    public static YamlSerializerSettings CreateForEnvironment(string environment)
    {
        return environment?.ToLowerInvariant() switch
        {
            "development" => CreateDevelopmentSettings(),
            "production" => CreateProductionSettings(),
            _ => CreateDefaultSettings()
        };
    }
}
```

### 2. Performance Optimization
```csharp
public class CachedYamlService
{
    private static readonly ConcurrentDictionary<string, (ISerializer, IDeserializer)> Cache = new();
    
    public static (ISerializer, IDeserializer) GetOrCreate(string settingsKey, YamlSerializerSettings settings)
    {
        return Cache.GetOrAdd(settingsKey, _ => (CreateSerializer(settings), CreateDeserializer(settings)));
    }
}
```

### 3. Validation
```csharp
public static class SettingsValidator
{
    public static void Validate(YamlSerializerSettings settings)
    {
        if (settings.JsonCompatible && settings.Style == ScalarStyle.Literal)
            throw new InvalidOperationException("JSON compatibility is not compatible with literal scalar style");
        
        if (settings.TypeConverters?.Any(c => c == null) == true)
            throw new ArgumentException("Type converters collection contains null values");
    }
}
```

### 4. Security Considerations
```csharp
public static YamlSerializerSettings CreateSecureSettings()
{
    return new YamlSerializerSettings
    {
        IgnoreFields = true, // Prevent field injection
        IncludeNonPublicProperties = false, // Limit exposure
        TypeResolver = new RestrictedTypeResolver(), // Control type instantiation
        NodeDeserializers = new[] { new SecurityValidatingDeserializer() }
    };
}
```

## See Also

- [YamlTypeConverter](YamlTypeConverter.md) - Base classes for custom YAML type converters
- [YamlTypeConverterAttribute](YamlTypeConverterAttribute.md) - Custom YAML type converter attribute
- [YamlNodeDeserializerAttribute](YamlNodeDeserializerAttribute.md) - Custom node deserializer attribute
- [YamlHelper](../../Helpers/YamlHelper.md) - YAML serialization utilities
- [JsonSerializationAttribute](../../Attributes/JsonSerializationAttribute.md) - JSON serialization attributes

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations.Yaml namespace - providing comprehensive YAML serialization configuration.*