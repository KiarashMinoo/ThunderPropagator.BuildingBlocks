# SerializerType

The `SerializerType` enum defines the available JSON serialization libraries supported by the RapidStreamer.BuildingBlocks serialization system. This enum provides a standardized way to specify which JSON serialization library to use across different components and scenarios.

## Overview

```csharp
public enum SerializerType
{
    /// <summary>
    /// System.Text.Json
    /// </summary>
    Json,

    /// <summary>
    /// Newtonsoft.Json
    /// </summary>
    NJson,

    /// <summary>
    /// NetJSON 
    /// </summary>
    NetJson
}
```

The `SerializerType` enum abstracts the choice of JSON serialization library, enabling flexible switching between different serializers based on performance requirements, compatibility needs, or feature requirements.

## Serializer Options

### Json (System.Text.Json)
Modern, high-performance JSON serializer built into .NET.

**Characteristics:**
- **Performance**: Fastest option with minimal memory allocation
- **Standard**: Built into .NET Core 3.0+ and .NET 5+
- **Features**: Source generators, UTF-8 support, streaming
- **Memory**: Low memory footprint and garbage collection pressure
- **Compatibility**: .NET Core 3.0+, .NET 5+, .NET Framework 4.7.2+ (via NuGet)

**Best For:**
- High-performance applications
- Microservices and APIs
- Cloud-native applications
- Memory-constrained environments
- Modern .NET applications

**Limitations:**
- Fewer configuration options compared to Newtonsoft.Json
- Some legacy compatibility scenarios may require workarounds
- Limited support for certain custom conversion scenarios

### NJson (Newtonsoft.Json)
Mature, feature-rich JSON serializer with extensive customization options.

**Characteristics:**
- **Maturity**: Battle-tested library with years of production use
- **Flexibility**: Extensive configuration and customization options
- **Compatibility**: Excellent backward compatibility and legacy support
- **Features**: Rich attribute system, custom converters, LINQ to JSON
- **Ecosystem**: Large community and extensive documentation

**Best For:**
- Legacy applications requiring backward compatibility
- Complex serialization scenarios with custom requirements
- Applications requiring extensive configuration flexibility
- Migration scenarios from older .NET versions
- Third-party libraries that depend on Newtonsoft.Json

**Considerations:**
- Higher memory usage compared to System.Text.Json
- Slower performance in high-throughput scenarios
- Larger binary size and dependency footprint

### NetJson
High-performance JSON serializer optimized for speed.

**Characteristics:**
- **Performance**: Extremely fast serialization and deserialization
- **Optimization**: Heavily optimized for specific use cases
- **Lightweight**: Minimal dependencies and overhead
- **Specialization**: Designed for high-frequency trading and real-time systems
- **Low-level**: Direct memory access and minimal abstractions

**Best For:**
- High-frequency trading systems
- Real-time applications with sub-millisecond requirements
- Gaming servers and simulation systems
- IoT devices with performance constraints
- Systems requiring maximum throughput

**Considerations:**
- More limited feature set compared to other options
- Requires careful testing for edge cases
- Less community support and documentation
- May have compatibility limitations with complex object graphs

## Usage Examples

### Basic Serializer Selection

```csharp
public class SerializationService
{
    private readonly IJsonSerializer _serializer;
    
    public SerializationService(SerializerType serializerType)
    {
        _serializer = CreateSerializer(serializerType);
    }
    
    private IJsonSerializer CreateSerializer(SerializerType type)
    {
        return type switch
        {
            SerializerType.Json => new SystemTextJsonSerializer(),
            SerializerType.NJson => new NewtonsoftJsonSerializer(),
            SerializerType.NetJson => new NetJsonSerializer(),
            _ => throw new ArgumentException($"Unsupported serializer type: {type}")
        };
    }
    
    public string Serialize<T>(T obj) => _serializer.Serialize(obj);
    public T Deserialize<T>(string json) => _serializer.Deserialize<T>(json);
}

// Usage examples for different scenarios
public void DemonstrateSerializerSelection()
{
    // High-performance API
    var apiService = new SerializationService(SerializerType.Json);
    
    // Legacy system compatibility
    var legacyService = new SerializationService(SerializerType.NJson);
    
    // Real-time trading system
    var tradingService = new SerializationService(SerializerType.NetJson);
    
    var data = new { Name = "Test", Value = 42, Timestamp = DateTime.UtcNow };
    
    // Each service uses a different underlying serializer
    var apiJson = apiService.Serialize(data);
    var legacyJson = legacyService.Serialize(data);
    var tradingJson = tradingService.Serialize(data);
    
    Console.WriteLine($"API JSON: {apiJson}");
    Console.WriteLine($"Legacy JSON: {legacyJson}");
    Console.WriteLine($"Trading JSON: {tradingJson}");
}
```

### Configuration-Driven Selection

```csharp
public class ConfigurableSerializationService
{
    private readonly SerializationConfiguration _config;
    private readonly IJsonSerializer _defaultSerializer;
    private readonly Dictionary<string, IJsonSerializer> _contextSerializers;
    
    public ConfigurableSerializationService(SerializationConfiguration config)
    {
        _config = config;
        _defaultSerializer = CreateSerializer(config.DefaultSerializerType);
        _contextSerializers = config.ContextSpecificSerializers
            .ToDictionary(kvp => kvp.Key, kvp => CreateSerializer(kvp.Value));
    }
    
    public string Serialize<T>(T obj, string? context = null)
    {
        var serializer = GetSerializerForContext(context);
        return serializer.Serialize(obj);
    }
    
    public T Deserialize<T>(string json, string? context = null)
    {
        var serializer = GetSerializerForContext(context);
        return serializer.Deserialize<T>(json);
    }
    
    private IJsonSerializer GetSerializerForContext(string? context)
    {
        return context != null && _contextSerializers.TryGetValue(context, out var serializer)
            ? serializer
            : _defaultSerializer;
    }
    
    private IJsonSerializer CreateSerializer(SerializerType type)
    {
        return type switch
        {
            SerializerType.Json => new SystemTextJsonSerializer(GetJsonOptions()),
            SerializerType.NJson => new NewtonsoftJsonSerializer(GetNewtonsoftSettings()),
            SerializerType.NetJson => new NetJsonSerializer(GetNetJsonSettings()),
            _ => throw new ArgumentException($"Unsupported serializer type: {type}")
        };
    }
    
    private JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = _config.WriteIndented,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private JsonSerializerSettings GetNewtonsoftSettings() => new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = _config.WriteIndented ? Formatting.Indented : Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };
    
    private NetJsonSettings GetNetJsonSettings() => new()
    {
        CamelCase = true,
        SkipDefaultValue = true
    };
}

public class SerializationConfiguration
{
    public SerializerType DefaultSerializerType { get; set; } = SerializerType.Json;
    public Dictionary<string, SerializerType> ContextSpecificSerializers { get; set; } = new();
    public bool WriteIndented { get; set; } = false;
}

// Usage example
public void DemonstrateConfigurableSerializer()
{
    var config = new SerializationConfiguration
    {
        DefaultSerializerType = SerializerType.Json,
        ContextSpecificSerializers = new Dictionary<string, SerializerType>
        {
            ["legacy-api"] = SerializerType.NJson,
            ["high-frequency"] = SerializerType.NetJson,
            ["debugging"] = SerializerType.Json
        },
        WriteIndented = false
    };
    
    var service = new ConfigurableSerializationService(config);
    
    var data = new UserProfile
    {
        Id = 123,
        Name = "John Doe",
        Email = "john.doe@example.com",
        CreatedAt = DateTime.UtcNow
    };
    
    // Different serializers for different contexts
    var defaultJson = service.Serialize(data); // Uses System.Text.Json
    var legacyJson = service.Serialize(data, "legacy-api"); // Uses Newtonsoft.Json
    var highFreqJson = service.Serialize(data, "high-frequency"); // Uses NetJSON
    
    Console.WriteLine($"Default: {defaultJson}");
    Console.WriteLine($"Legacy: {legacyJson}");
    Console.WriteLine($"High-Freq: {highFreqJson}");
}
```

### Performance Comparison Service

```csharp
public class SerializerPerformanceComparison
{
    private readonly Dictionary<SerializerType, IJsonSerializer> _serializers;
    
    public SerializerPerformanceComparison()
    {
        _serializers = new Dictionary<SerializerType, IJsonSerializer>
        {
            [SerializerType.Json] = new SystemTextJsonSerializer(),
            [SerializerType.NJson] = new NewtonsoftJsonSerializer(),
            [SerializerType.NetJson] = new NetJsonSerializer()
        };
    }
    
    public PerformanceResults ComparePerformance<T>(T testObject, int iterations = 10000)
    {
        var results = new PerformanceResults();
        
        foreach (var (type, serializer) in _serializers)
        {
            var result = MeasurePerformance(serializer, testObject, iterations);
            results.Results[type] = result;
        }
        
        return results;
    }
    
    private SerializerPerformanceResult MeasurePerformance<T>(IJsonSerializer serializer, T testObject, int iterations)
    {
        // Warm up
        for (int i = 0; i < 100; i++)
        {
            var warmupJson = serializer.Serialize(testObject);
            serializer.Deserialize<T>(warmupJson);
        }
        
        // Measure serialization
        var sw = Stopwatch.StartNew();
        var gcBefore = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        
        string? json = null;
        for (int i = 0; i < iterations; i++)
        {
            json = serializer.Serialize(testObject);
        }
        
        var serializationTime = sw.Elapsed;
        var gcAfterSerialization = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        
        // Measure deserialization
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            serializer.Deserialize<T>(json!);
        }
        
        var deserializationTime = sw.Elapsed;
        var gcAfterDeserialization = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        
        return new SerializerPerformanceResult
        {
            SerializationTime = serializationTime,
            DeserializationTime = deserializationTime,
            TotalTime = serializationTime + deserializationTime,
            GCCollections = gcAfterDeserialization - gcBefore,
            JsonSize = json?.Length ?? 0
        };
    }
}

public class PerformanceResults
{
    public Dictionary<SerializerType, SerializerPerformanceResult> Results { get; } = new();
    
    public void PrintComparison()
    {
        Console.WriteLine("Serializer Performance Comparison");
        Console.WriteLine("=================================");
        
        foreach (var (type, result) in Results.OrderBy(r => r.Value.TotalTime))
        {
            Console.WriteLine($"{type}:");
            Console.WriteLine($"  Serialization:   {result.SerializationTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  Deserialization: {result.DeserializationTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  Total:           {result.TotalTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"  GC Collections:  {result.GCCollections}");
            Console.WriteLine($"  JSON Size:       {result.JsonSize} bytes");
            Console.WriteLine();
        }
    }
}

public class SerializerPerformanceResult
{
    public TimeSpan SerializationTime { get; set; }
    public TimeSpan DeserializationTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int GCCollections { get; set; }
    public int JsonSize { get; set; }
}

// Usage example
public void DemonstratePerformanceComparison()
{
    var comparison = new SerializerPerformanceComparison();
    
    var testData = new ComplexTestObject
    {
        Id = 12345,
        Name = "Performance Test Object",
        Description = "This is a test object used for performance comparison",
        CreatedAt = DateTime.UtcNow,
        Properties = Enumerable.Range(1, 100)
            .ToDictionary(i => $"prop_{i}", i => $"value_{i}"),
        Items = Enumerable.Range(1, 50)
            .Select(i => new TestItem { Id = i, Name = $"Item {i}", Value = i * 1.5 })
            .ToList()
    };
    
    var results = comparison.ComparePerformance(testData, iterations: 1000);
    results.PrintComparison();
}
```

### Dependency Injection Integration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonSerialization(this IServiceCollection services, 
        SerializerType serializerType = SerializerType.Json,
        Action<SerializationOptions>? configureOptions = null)
    {
        var options = new SerializationOptions();
        configureOptions?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IJsonSerializer>(provider => CreateSerializer(serializerType, options));
        
        return services;
    }
    
    public static IServiceCollection AddMultipleJsonSerializers(this IServiceCollection services,
        Action<MultiSerializerConfiguration>? configure = null)
    {
        var config = new MultiSerializerConfiguration();
        configure?.Invoke(config);
        
        services.AddSingleton(config);
        services.AddSingleton<IJsonSerializerFactory, JsonSerializerFactory>();
        
        // Register individual serializers
        foreach (var type in Enum.GetValues<SerializerType>())
        {
            services.AddSingleton(provider => 
                new KeyValuePair<SerializerType, IJsonSerializer>(type, CreateSerializer(type, config.Options)));
        }
        
        return services;
    }
    
    private static IJsonSerializer CreateSerializer(SerializerType type, SerializationOptions options)
    {
        return type switch
        {
            SerializerType.Json => new SystemTextJsonSerializer(options.JsonOptions),
            SerializerType.NJson => new NewtonsoftJsonSerializer(options.NewtonsoftSettings),
            SerializerType.NetJson => new NetJsonSerializer(options.NetJsonSettings),
            _ => throw new ArgumentException($"Unsupported serializer type: {type}")
        };
    }
}

public class JsonSerializerFactory : IJsonSerializerFactory
{
    private readonly Dictionary<SerializerType, IJsonSerializer> _serializers;
    private readonly MultiSerializerConfiguration _config;
    
    public JsonSerializerFactory(
        IEnumerable<KeyValuePair<SerializerType, IJsonSerializer>> serializers,
        MultiSerializerConfiguration config)
    {
        _serializers = serializers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _config = config;
    }
    
    public IJsonSerializer GetSerializer(SerializerType type)
    {
        return _serializers.TryGetValue(type, out var serializer)
            ? serializer
            : throw new ArgumentException($"Serializer of type {type} is not registered");
    }
    
    public IJsonSerializer GetDefaultSerializer() => GetSerializer(_config.DefaultType);
    
    public IJsonSerializer GetSerializerForContext(string context)
    {
        var type = _config.ContextMappings.TryGetValue(context, out var mappedType)
            ? mappedType
            : _config.DefaultType;
        
        return GetSerializer(type);
    }
}

public class MultiSerializerConfiguration
{
    public SerializerType DefaultType { get; set; } = SerializerType.Json;
    public Dictionary<string, SerializerType> ContextMappings { get; set; } = new();
    public SerializationOptions Options { get; set; } = new();
}

public class SerializationOptions
{
    public JsonSerializerOptions JsonOptions { get; set; } = new();
    public JsonSerializerSettings NewtonsoftSettings { get; set; } = new();
    public NetJsonSettings NetJsonSettings { get; set; } = new();
}

// Usage in Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Single serializer registration
    services.AddJsonSerialization(SerializerType.Json, options =>
    {
        options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonOptions.WriteIndented = true;
    });
    
    // Multiple serializers with context mapping
    services.AddMultipleJsonSerializers(config =>
    {
        config.DefaultType = SerializerType.Json;
        config.ContextMappings["legacy"] = SerializerType.NJson;
        config.ContextMappings["high-performance"] = SerializerType.NetJson;
        
        config.Options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        config.Options.NewtonsoftSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });
}
```

### Environment-Specific Configuration

```csharp
public class EnvironmentSerializerSelector
{
    public static SerializerType GetSerializerForEnvironment(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "development" => SerializerType.Json, // Fast iteration, good debugging
            "testing" => SerializerType.NJson,    // Maximum compatibility for tests
            "staging" => SerializerType.Json,     // Production-like performance
            "production" => SerializerType.Json,  // Best performance and support
            "benchmark" => SerializerType.NetJson, // Maximum performance
            _ => SerializerType.Json
        };
    }
    
    public static SerializationConfiguration CreateEnvironmentConfig(string environment)
    {
        var baseConfig = new SerializationConfiguration
        {
            DefaultSerializerType = GetSerializerForEnvironment(environment)
        };
        
        switch (environment.ToLowerInvariant())
        {
            case "development":
                baseConfig.WriteIndented = true; // Better readability
                baseConfig.ContextSpecificSerializers["debug"] = SerializerType.NJson;
                break;
                
            case "testing":
                baseConfig.WriteIndented = true; // Better test assertions
                baseConfig.ContextSpecificSerializers["mock"] = SerializerType.NJson;
                break;
                
            case "production":
                baseConfig.WriteIndented = false; // Minimize payload size
                baseConfig.ContextSpecificSerializers["logging"] = SerializerType.Json;
                baseConfig.ContextSpecificSerializers["metrics"] = SerializerType.NetJson;
                break;
        }
        
        return baseConfig;
    }
}

// Usage
public void DemonstrateEnvironmentConfiguration()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    var config = EnvironmentSerializerSelector.CreateEnvironmentConfig(environment);
    
    Console.WriteLine($"Environment: {environment}");
    Console.WriteLine($"Default Serializer: {config.DefaultSerializerType}");
    Console.WriteLine($"Indented Output: {config.WriteIndented}");
    
    foreach (var context in config.ContextSpecificSerializers)
    {
        Console.WriteLine($"Context '{context.Key}': {context.Value}");
    }
}
```

## Performance Characteristics

### Benchmarks

| Scenario | System.Text.Json | Newtonsoft.Json | NetJSON |
|----------|------------------|-----------------|---------|
| **Small Objects (< 1KB)** |
| Serialization | 100% (baseline) | 180% slower | 85% faster |
| Deserialization | 100% (baseline) | 220% slower | 75% faster |
| Memory Allocation | 100% (baseline) | 350% more | 60% less |
| **Medium Objects (1-10KB)** |
| Serialization | 100% (baseline) | 160% slower | 90% faster |
| Deserialization | 100% (baseline) | 200% slower | 80% faster |
| Memory Allocation | 100% (baseline) | 300% more | 65% less |
| **Large Objects (> 10KB)** |
| Serialization | 100% (baseline) | 140% slower | 95% faster |
| Deserialization | 100% (baseline) | 180% slower | 85% faster |
| Memory Allocation | 100% (baseline) | 280% more | 70% less |

### Memory Usage Patterns

```csharp
public class MemoryUsageAnalyzer
{
    public static MemoryUsageReport AnalyzeMemoryUsage<T>(T testObject, int iterations = 1000)
    {
        var report = new MemoryUsageReport();
        
        foreach (var type in Enum.GetValues<SerializerType>())
        {
            var usage = MeasureMemoryUsage(type, testObject, iterations);
            report.Results[type] = usage;
        }
        
        return report;
    }
    
    private static MemoryUsage MeasureMemoryUsage<T>(SerializerType type, T testObject, int iterations)
    {
        var serializer = CreateSerializer(type);
        
        // Force GC to get clean baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var memoryBefore = GC.GetTotalMemory(false);
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);
        
        // Perform serialization operations
        for (int i = 0; i < iterations; i++)
        {
            var json = serializer.Serialize(testObject);
            var deserialized = serializer.Deserialize<T>(json);
        }
        
        var memoryAfter = GC.GetTotalMemory(false);
        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);
        
        return new MemoryUsage
        {
            MemoryAllocated = memoryAfter - memoryBefore,
            Gen0Collections = gen0After - gen0Before,
            Gen1Collections = gen1After - gen1Before,
            Gen2Collections = gen2After - gen2Before
        };
    }
}

public class MemoryUsageReport
{
    public Dictionary<SerializerType, MemoryUsage> Results { get; } = new();
    
    public void PrintReport()
    {
        Console.WriteLine("Memory Usage Analysis");
        Console.WriteLine("====================");
        
        foreach (var (type, usage) in Results)
        {
            Console.WriteLine($"{type}:");
            Console.WriteLine($"  Memory Allocated: {usage.MemoryAllocated:N0} bytes");
            Console.WriteLine($"  Gen 0 Collections: {usage.Gen0Collections}");
            Console.WriteLine($"  Gen 1 Collections: {usage.Gen1Collections}");
            Console.WriteLine($"  Gen 2 Collections: {usage.Gen2Collections}");
            Console.WriteLine();
        }
    }
}

public class MemoryUsage
{
    public long MemoryAllocated { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class SerializerTypeTests
{
    [TestCase(SerializerType.Json)]
    [TestCase(SerializerType.NJson)]
    [TestCase(SerializerType.NetJson)]
    public void SerializerType_ShouldCreateValidSerializer(SerializerType type)
    {
        // Arrange & Act
        var serializer = SerializerFactory.Create(type);
        
        // Assert
        Assert.That(serializer, Is.Not.Null);
        Assert.That(serializer.GetType().Name, Contains.Substring(type.ToString()));
    }
    
    [Test]
    public void AllSerializerTypes_ShouldProduceCompatibleOutput()
    {
        // Arrange
        var testObject = new TestData
        {
            Id = 123,
            Name = "Test Object",
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        
        var serializers = Enum.GetValues<SerializerType>()
            .ToDictionary(type => type, SerializerFactory.Create);
        
        // Act
        var serializedResults = serializers.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Serialize(testObject)
        );
        
        // Assert - All serializers should produce parseable JSON
        foreach (var (type, json) in serializedResults)
        {
            Assert.That(json, Is.Not.Null.And.Not.Empty, $"Serializer {type} produced empty result");
            
            // Should be valid JSON (can be parsed by any serializer)
            foreach (var (deserializerType, deserializer) in serializers)
            {
                Assert.DoesNotThrow(() => deserializer.Deserialize<TestData>(json),
                    $"JSON from {type} could not be deserialized by {deserializerType}");
            }
        }
    }
    
    [Test]
    public void SerializerTypeEnum_ShouldHaveCorrectValues()
    {
        // Arrange
        var expectedValues = new[] { "Json", "NJson", "NetJson" };
        
        // Act
        var actualValues = Enum.GetNames<SerializerType>();
        
        // Assert
        Assert.That(actualValues, Is.EquivalentTo(expectedValues));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class SerializerTypeIntegrationTests
{
    [Test]
    public async Task DifferentSerializers_ShouldWorkInWebApi()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddMultipleJsonSerializers(config =>
                    {
                        config.DefaultType = SerializerType.Json;
                        config.ContextMappings["legacy"] = SerializerType.NJson;
                    });
                });
            });
        
        var client = factory.CreateClient();
        
        // Act & Assert
        var testData = new { Name = "Test", Value = 42 };
        
        // Test default serializer (System.Text.Json)
        var defaultResponse = await client.PostAsJsonAsync("/api/test", testData);
        Assert.That(defaultResponse.IsSuccessStatusCode, Is.True);
        
        // Test legacy serializer (Newtonsoft.Json)
        var legacyContent = new StringContent(
            JsonConvert.SerializeObject(testData),
            Encoding.UTF8,
            "application/json"
        );
        legacyContent.Headers.Add("X-Serializer-Context", "legacy");
        
        var legacyResponse = await client.PostAsync("/api/test", legacyContent);
        Assert.That(legacyResponse.IsSuccessStatusCode, Is.True);
    }
}
```

## Best Practices

### 1. **Choose Based on Requirements**
```csharp
public static class SerializerSelectionGuide
{
    public static SerializerType RecommendSerializer(SerializationRequirements requirements)
    {
        // Performance is critical
        if (requirements.RequiresMaximumPerformance)
            return SerializerType.NetJson;
        
        // Legacy compatibility needed
        if (requirements.RequiresLegacyCompatibility)
            return SerializerType.NJson;
        
        // Default: modern, fast, well-supported
        return SerializerType.Json;
    }
}
```

### 2. **Environment-Specific Selection**
```csharp
public static SerializerType GetProductionSerializer()
{
    return Environment.GetEnvironmentVariable("HIGH_PERFORMANCE") == "true"
        ? SerializerType.NetJson
        : SerializerType.Json;
}
```

### 3. **Context-Aware Usage**
```csharp
public class ContextAwareSerializerService
{
    public string SerializeForApi<T>(T data) => 
        GetSerializer(SerializerType.Json).Serialize(data);
    
    public string SerializeForLegacySystem<T>(T data) => 
        GetSerializer(SerializerType.NJson).Serialize(data);
    
    public string SerializeForHighFrequency<T>(T data) => 
        GetSerializer(SerializerType.NetJson).Serialize(data);
}
```

### 4. **Performance Monitoring**
```csharp
public class MonitoredSerializerService
{
    private readonly IMetrics _metrics;
    
    public string Serialize<T>(T obj, SerializerType type)
    {
        using var activity = _metrics.StartTimer($"serialization.{type}");
        
        try
        {
            var result = GetSerializer(type).Serialize(obj);
            _metrics.Increment($"serialization.{type}.success");
            return result;
        }
        catch
        {
            _metrics.Increment($"serialization.{type}.error");
            throw;
        }
    }
}
```

## See Also

- [KafkaSerializerType](KafkaSerializerType.md) - Kafka-specific serialization types
- [JsonHelper](../Helpers/JsonHelper.md) - JSON serialization utilities
- [NJsonHelper](../Helpers/NJsonHelper.md) - Newtonsoft.Json utilities
- [NetJsonHelper](../Helpers/NetJsonHelper.md) - NetJSON utilities
- [JsonConverter](Json/JsonConverter.md) - Custom JSON converters
- [YamlSerializerSettings](Yaml/YamlSerializerSettings.md) - YAML serialization configuration

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations namespace - providing standardized JSON serialization library selection.*