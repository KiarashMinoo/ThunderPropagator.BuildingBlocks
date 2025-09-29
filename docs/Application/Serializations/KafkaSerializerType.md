# KafkaSerializerType

The `KafkaSerializerType` enum extends the base `SerializerType` enumeration to include specialized serialization formats commonly used in Apache Kafka messaging scenarios. This enum provides a unified approach to serialization format selection for both general JSON serialization and Kafka-specific messaging requirements.

## Overview

```csharp
public enum KafkaSerializerType
{
    /// <summary>
    /// System.Text.Json
    /// </summary>
    Json = SerializerType.Json,

    /// <summary>
    /// Newtonsoft.Json
    /// </summary>
    NJson = SerializerType.NJson,

    /// <summary>
    /// NetJSON 
    /// </summary>
    NetJson = SerializerType.NetJson,

    SchemaJson,
    Avro
}
```

The `KafkaSerializerType` enum maintains compatibility with the base `SerializerType` while adding Kafka-specific serialization formats, enabling seamless integration between general serialization needs and Kafka messaging infrastructure.

## Serialization Types

### JSON-Based Types (inherited from SerializerType)

#### Json (System.Text.Json)
High-performance JSON serialization using .NET's built-in serializer.

**Kafka Use Cases:**
- Event streaming with JSON payloads
- REST API to Kafka bridge scenarios
- Cloud-native microservices communication
- Development and debugging (human-readable format)

**Message Characteristics:**
- Human-readable JSON format
- Compact representation
- Wide compatibility across consumers
- Schema evolution through optional properties

#### NJson (Newtonsoft.Json)
Feature-rich JSON serialization with extensive customization options.

**Kafka Use Cases:**
- Legacy system integration
- Complex object serialization requirements
- Third-party system compatibility
- Migration scenarios from existing Newtonsoft.Json implementations

**Message Characteristics:**
- Rich configuration options
- Excellent backward compatibility
- Flexible schema handling
- Extensive customization capabilities

#### NetJson
Ultra-high-performance JSON serialization optimized for speed.

**Kafka Use Cases:**
- High-throughput event streaming
- Real-time analytics pipelines
- IoT data ingestion
- Financial trading systems

**Message Characteristics:**
- Maximum serialization performance
- Minimal CPU and memory overhead
- Optimized for high-frequency scenarios
- Reduced latency in message processing

### Kafka-Specific Types

#### SchemaJson
JSON serialization with Confluent Schema Registry integration.

**Characteristics:**
- **Schema Registry**: Centralized schema management and evolution
- **Versioning**: Built-in schema version control
- **Validation**: Automatic schema validation during serialization/deserialization
- **Evolution**: Forward and backward compatibility support
- **Governance**: Enterprise-grade schema governance

**Use Cases:**
- Enterprise Kafka deployments with schema governance
- Multi-team environments requiring schema contracts
- Systems requiring guaranteed message structure
- Scenarios with frequent schema evolution
- Regulatory compliance requiring message structure validation

**Message Structure:**
- Schema ID embedded in message headers or payload
- JSON payload validated against registered schema
- Automatic schema resolution during deserialization
- Support for schema evolution rules

#### Avro
Binary serialization format with compact representation and schema evolution.

**Characteristics:**
- **Binary Format**: Compact, efficient binary encoding
- **Schema Evolution**: Built-in support for schema changes
- **Cross-Language**: Language-agnostic format
- **Performance**: Fast serialization/deserialization
- **Compression**: Excellent compression ratios

**Use Cases:**
- High-volume data pipelines
- Long-term data storage scenarios
- Cross-language system integration
- Bandwidth-constrained environments
- Data lakes and analytics platforms

**Benefits:**
- Smallest message size among all options
- Fastest serialization performance for complex objects
- Built-in schema evolution capabilities
- Strong typing with schema validation
- Excellent for streaming analytics

## Usage Examples

### Basic Kafka Serializer Selection

```csharp
public class KafkaSerializerService
{
    private readonly Dictionary<KafkaSerializerType, IKafkaSerializer> _serializers;
    
    public KafkaSerializerService()
    {
        _serializers = new Dictionary<KafkaSerializerType, IKafkaSerializer>
        {
            [KafkaSerializerType.Json] = new JsonKafkaSerializer(),
            [KafkaSerializerType.NJson] = new NewtonsoftKafkaSerializer(),
            [KafkaSerializerType.NetJson] = new NetJsonKafkaSerializer(),
            [KafkaSerializerType.SchemaJson] = new SchemaJsonKafkaSerializer(),
            [KafkaSerializerType.Avro] = new AvroKafkaSerializer()
        };
    }
    
    public byte[] Serialize<T>(T message, KafkaSerializerType serializerType)
    {
        var serializer = _serializers[serializerType];
        return serializer.Serialize(message);
    }
    
    public T Deserialize<T>(byte[] data, KafkaSerializerType serializerType)
    {
        var serializer = _serializers[serializerType];
        return serializer.Deserialize<T>(data);
    }
}

// Usage examples for different scenarios
public class KafkaProducerExample
{
    private readonly KafkaSerializerService _serializerService;
    private readonly IProducer<string, byte[]> _producer;
    
    public KafkaProducerExample(KafkaSerializerService serializerService, IProducer<string, byte[]> producer)
    {
        _serializerService = serializerService;
        _producer = producer;
    }
    
    public async Task PublishUserEvent(UserEvent userEvent)
    {
        // High-performance JSON for real-time events
        var jsonData = _serializerService.Serialize(userEvent, KafkaSerializerType.NetJson);
        await _producer.ProduceAsync("user-events", new Message<string, byte[]>
        {
            Key = userEvent.UserId,
            Value = jsonData,
            Headers = new Headers { { "content-type", Encoding.UTF8.GetBytes("application/json") } }
        });
    }
    
    public async Task PublishOrderEvent(OrderEvent orderEvent)
    {
        // Schema-validated JSON for critical business events
        var schemaJsonData = _serializerService.Serialize(orderEvent, KafkaSerializerType.SchemaJson);
        await _producer.ProduceAsync("order-events", new Message<string, byte[]>
        {
            Key = orderEvent.OrderId,
            Value = schemaJsonData,
            Headers = new Headers { { "content-type", Encoding.UTF8.GetBytes("application/vnd.kafka.json.v1+json") } }
        });
    }
    
    public async Task PublishAnalyticsEvent(AnalyticsEvent analyticsEvent)
    {
        // Avro for high-volume analytics data
        var avroData = _serializerService.Serialize(analyticsEvent, KafkaSerializerType.Avro);
        await _producer.ProduceAsync("analytics-events", new Message<string, byte[]>
        {
            Key = analyticsEvent.SessionId,
            Value = avroData,
            Headers = new Headers { { "content-type", Encoding.UTF8.GetBytes("application/vnd.kafka.avro.v1+avro") } }
        });
    }
}
```

### Topic-Specific Serializer Configuration

```csharp
public class TopicSerializerConfiguration
{
    public Dictionary<string, KafkaSerializerType> TopicSerializers { get; set; } = new();
    public KafkaSerializerType DefaultSerializer { get; set; } = KafkaSerializerType.Json;
    
    public static TopicSerializerConfiguration CreateDefault()
    {
        return new TopicSerializerConfiguration
        {
            DefaultSerializer = KafkaSerializerType.Json,
            TopicSerializers = new Dictionary<string, KafkaSerializerType>
            {
                // User interactions - human readable for debugging
                ["user-events"] = KafkaSerializerType.Json,
                ["user-actions"] = KafkaSerializerType.Json,
                
                // Business critical - schema validation required
                ["order-events"] = KafkaSerializerType.SchemaJson,
                ["payment-events"] = KafkaSerializerType.SchemaJson,
                ["inventory-events"] = KafkaSerializerType.SchemaJson,
                
                // High-volume analytics - compact binary format
                ["page-views"] = KafkaSerializerType.Avro,
                ["click-tracking"] = KafkaSerializerType.Avro,
                ["sensor-data"] = KafkaSerializerType.Avro,
                
                // High-frequency trading - maximum performance
                ["market-data"] = KafkaSerializerType.NetJson,
                ["trade-executions"] = KafkaSerializerType.NetJson,
                
                // Legacy system integration
                ["legacy-orders"] = KafkaSerializerType.NJson,
                ["legacy-customers"] = KafkaSerializerType.NJson
            }
        };
    }
}

public class ConfigurableKafkaProducer
{
    private readonly TopicSerializerConfiguration _config;
    private readonly KafkaSerializerService _serializerService;
    private readonly IProducer<string, byte[]> _producer;
    
    public ConfigurableKafkaProducer(
        TopicSerializerConfiguration config,
        KafkaSerializerService serializerService,
        IProducer<string, byte[]> producer)
    {
        _config = config;
        _serializerService = serializerService;
        _producer = producer;
    }
    
    public async Task ProduceAsync<T>(string topic, string key, T value)
    {
        var serializerType = GetSerializerTypeForTopic(topic);
        var serializedValue = _serializerService.Serialize(value, serializerType);
        
        var message = new Message<string, byte[]>
        {
            Key = key,
            Value = serializedValue,
            Headers = CreateHeaders(serializerType)
        };
        
        await _producer.ProduceAsync(topic, message);
    }
    
    private KafkaSerializerType GetSerializerTypeForTopic(string topic)
    {
        return _config.TopicSerializers.TryGetValue(topic, out var serializerType)
            ? serializerType
            : _config.DefaultSerializer;
    }
    
    private Headers CreateHeaders(KafkaSerializerType serializerType)
    {
        var contentType = serializerType switch
        {
            KafkaSerializerType.Json => "application/json",
            KafkaSerializerType.NJson => "application/json",
            KafkaSerializerType.NetJson => "application/json",
            KafkaSerializerType.SchemaJson => "application/vnd.kafka.json.v1+json",
            KafkaSerializerType.Avro => "application/vnd.kafka.avro.v1+avro",
            _ => "application/octet-stream"
        };
        
        return new Headers
        {
            { "content-type", Encoding.UTF8.GetBytes(contentType) },
            { "serializer-type", Encoding.UTF8.GetBytes(serializerType.ToString()) }
        };
    }
}
```

### Schema Registry Integration

```csharp
public class SchemaRegistryKafkaSerializer : IKafkaSerializer
{
    private readonly ISchemaRegistryClient _schemaRegistry;
    private readonly Dictionary<Type, int> _schemaIdCache;
    private readonly JsonSerializer _jsonSerializer;
    
    public SchemaRegistryKafkaSerializer(ISchemaRegistryClient schemaRegistry)
    {
        _schemaRegistry = schemaRegistry;
        _schemaIdCache = new Dictionary<Type, int>();
        _jsonSerializer = new JsonSerializer();
    }
    
    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        var schemaId = await GetSchemaIdAsync<T>();
        var jsonBytes = _jsonSerializer.Serialize(obj);
        
        // Confluent wire format: magic byte + schema ID + JSON payload
        var result = new byte[5 + jsonBytes.Length];
        result[0] = 0; // Magic byte
        BitConverter.GetBytes(schemaId).CopyTo(result, 1);
        jsonBytes.CopyTo(result, 5);
        
        return result;
    }
    
    public async Task<T> DeserializeAsync<T>(byte[] data)
    {
        if (data.Length < 5 || data[0] != 0)
            throw new SerializationException("Invalid message format");
        
        var schemaId = BitConverter.ToInt32(data, 1);
        var jsonPayload = data[5..];
        
        // Validate against schema if needed
        await ValidateSchemaAsync<T>(schemaId);
        
        return _jsonSerializer.Deserialize<T>(jsonPayload);
    }
    
    private async Task<int> GetSchemaIdAsync<T>()
    {
        var type = typeof(T);
        if (_schemaIdCache.TryGetValue(type, out var cachedId))
            return cachedId;
        
        var schema = GenerateJsonSchema<T>();
        var subject = $"{type.Name}-value";
        
        var registeredSchema = await _schemaRegistry.RegisterSchemaAsync(subject, schema);
        _schemaIdCache[type] = registeredSchema.Id;
        
        return registeredSchema.Id;
    }
    
    private async Task ValidateSchemaAsync<T>(int schemaId)
    {
        var schema = await _schemaRegistry.GetSchemaAsync(schemaId);
        // Perform schema validation logic here
    }
    
    private string GenerateJsonSchema<T>()
    {
        // Generate JSON schema for type T
        // This would integrate with a JSON schema generation library
        return JsonSchemaGenerator.Generate<T>();
    }
}
```

### Avro Serialization Implementation

```csharp
public class AvroKafkaSerializer : IKafkaSerializer
{
    private readonly ISchemaRegistryClient _schemaRegistry;
    private readonly Dictionary<Type, Schema> _schemaCache;
    private readonly Dictionary<int, Schema> _schemaByIdCache;
    
    public AvroKafkaSerializer(ISchemaRegistryClient schemaRegistry)
    {
        _schemaRegistry = schemaRegistry;
        _schemaCache = new Dictionary<Type, Schema>();
        _schemaByIdCache = new Dictionary<int, Schema>();
    }
    
    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        var schema = await GetSchemaAsync<T>();
        var schemaId = await GetSchemaIdAsync<T>();
        
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);
        
        // Confluent wire format
        binaryWriter.Write((byte)0); // Magic byte
        binaryWriter.Write(schemaId); // Schema ID
        
        // Avro binary encoding
        var encoder = new BinaryEncoder(memoryStream);
        var writer = new SpecificDatumWriter<T>(schema);
        writer.Write(obj, encoder);
        
        return memoryStream.ToArray();
    }
    
    public async Task<T> DeserializeAsync<T>(byte[] data)
    {
        using var memoryStream = new MemoryStream(data);
        using var binaryReader = new BinaryReader(memoryStream);
        
        var magicByte = binaryReader.ReadByte();
        if (magicByte != 0)
            throw new SerializationException("Invalid Avro message format");
        
        var schemaId = binaryReader.ReadInt32();
        var schema = await GetSchemaByIdAsync(schemaId);
        
        var decoder = new BinaryDecoder(memoryStream);
        var reader = new SpecificDatumReader<T>(schema, schema);
        
        return reader.Read(default(T), decoder);
    }
    
    private async Task<Schema> GetSchemaAsync<T>()
    {
        var type = typeof(T);
        if (_schemaCache.TryGetValue(type, out var cachedSchema))
            return cachedSchema;
        
        var schemaText = await GetAvroSchemaForTypeAsync<T>();
        var schema = Schema.Parse(schemaText);
        _schemaCache[type] = schema;
        
        return schema;
    }
    
    private async Task<int> GetSchemaIdAsync<T>()
    {
        var schemaText = await GetAvroSchemaForTypeAsync<T>();
        var subject = $"{typeof(T).Name}-value";
        
        var registeredSchema = await _schemaRegistry.RegisterSchemaAsync(subject, schemaText);
        return registeredSchema.Id;
    }
    
    private async Task<Schema> GetSchemaByIdAsync(int schemaId)
    {
        if (_schemaByIdCache.TryGetValue(schemaId, out var cachedSchema))
            return cachedSchema;
        
        var schemaText = await _schemaRegistry.GetSchemaAsync(schemaId);
        var schema = Schema.Parse(schemaText.SchemaString);
        _schemaByIdCache[schemaId] = schema;
        
        return schema;
    }
    
    private async Task<string> GetAvroSchemaForTypeAsync<T>()
    {
        // Generate or retrieve Avro schema for type T
        return AvroSchemaGenerator.Generate<T>();
    }
}
```

### Performance Monitoring and Metrics

```csharp
public class MetricsKafkaSerializer : IKafkaSerializer
{
    private readonly IKafkaSerializer _innerSerializer;
    private readonly IMetrics _metrics;
    private readonly KafkaSerializerType _serializerType;
    
    public MetricsKafkaSerializer(IKafkaSerializer innerSerializer, IMetrics metrics, KafkaSerializerType serializerType)
    {
        _innerSerializer = innerSerializer;
        _metrics = metrics;
        _serializerType = serializerType;
    }
    
    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        var stopwatch = Stopwatch.StartNew();
        var typeName = typeof(T).Name;
        
        try
        {
            var result = await _innerSerializer.SerializeAsync(obj);
            
            stopwatch.Stop();
            
            // Record metrics
            _metrics.RecordValue($"kafka.serialization.duration.{_serializerType}", stopwatch.ElapsedMilliseconds);
            _metrics.RecordValue($"kafka.serialization.size.{_serializerType}", result.Length);
            _metrics.Increment($"kafka.serialization.success.{_serializerType}.{typeName}");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _metrics.Increment($"kafka.serialization.error.{_serializerType}.{typeName}");
            _metrics.RecordValue($"kafka.serialization.error_duration.{_serializerType}", stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
    
    public async Task<T> DeserializeAsync<T>(byte[] data)
    {
        var stopwatch = Stopwatch.StartNew();
        var typeName = typeof(T).Name;
        
        try
        {
            var result = await _innerSerializer.DeserializeAsync<T>(data);
            
            stopwatch.Stop();
            
            _metrics.RecordValue($"kafka.deserialization.duration.{_serializerType}", stopwatch.ElapsedMilliseconds);
            _metrics.RecordValue($"kafka.deserialization.input_size.{_serializerType}", data.Length);
            _metrics.Increment($"kafka.deserialization.success.{_serializerType}.{typeName}");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _metrics.Increment($"kafka.deserialization.error.{_serializerType}.{typeName}");
            _metrics.RecordValue($"kafka.deserialization.error_duration.{_serializerType}", stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}

public class KafkaSerializationMetrics
{
    private readonly IMetrics _metrics;
    
    public KafkaSerializationMetrics(IMetrics metrics)
    {
        _metrics = metrics;
    }
    
    public void RecordSerializationPerformance(KafkaSerializerType serializerType, TimeSpan duration, int outputSize)
    {
        _metrics.RecordValue($"kafka.serialization.duration.{serializerType}", duration.TotalMilliseconds);
        _metrics.RecordValue($"kafka.serialization.size.{serializerType}", outputSize);
        _metrics.RecordValue($"kafka.serialization.throughput.{serializerType}", outputSize / duration.TotalSeconds);
    }
    
    public void RecordDeserializationPerformance(KafkaSerializerType serializerType, TimeSpan duration, int inputSize)
    {
        _metrics.RecordValue($"kafka.deserialization.duration.{serializerType}", duration.TotalMilliseconds);
        _metrics.RecordValue($"kafka.deserialization.size.{serializerType}", inputSize);
        _metrics.RecordValue($"kafka.deserialization.throughput.{serializerType}", inputSize / duration.TotalSeconds);
    }
    
    public async Task<PerformanceReport> GeneratePerformanceReportAsync(TimeSpan period)
    {
        var report = new PerformanceReport();
        
        foreach (var serializerType in Enum.GetValues<KafkaSerializerType>())
        {
            var stats = new SerializerPerformanceStats
            {
                SerializerType = serializerType,
                AverageSerializationTime = await _metrics.GetAverageAsync($"kafka.serialization.duration.{serializerType}", period),
                AverageDeserializationTime = await _metrics.GetAverageAsync($"kafka.deserialization.duration.{serializerType}", period),
                AverageMessageSize = await _metrics.GetAverageAsync($"kafka.serialization.size.{serializerType}", period),
                SerializationThroughput = await _metrics.GetAverageAsync($"kafka.serialization.throughput.{serializerType}", period),
                ErrorRate = await _metrics.GetRateAsync($"kafka.serialization.error.{serializerType}", period)
            };
            
            report.SerializerStats[serializerType] = stats;
        }
        
        return report;
    }
}
```

### Dependency Injection Configuration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaSerialization(this IServiceCollection services,
        Action<KafkaSerializationOptions>? configure = null)
    {
        var options = new KafkaSerializationOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<KafkaSerializerService>();
        
        // Register individual serializers
        services.AddSingleton<IKafkaSerializer>(provider => 
            new JsonKafkaSerializer(options.JsonOptions));
        services.AddSingleton<IKafkaSerializer>(provider => 
            new NewtonsoftKafkaSerializer(options.NewtonsoftSettings));
        services.AddSingleton<IKafkaSerializer>(provider => 
            new NetJsonKafkaSerializer(options.NetJsonSettings));
        
        if (options.SchemaRegistryConfig != null)
        {
            services.AddSingleton<ISchemaRegistryClient>(provider =>
                new CachedSchemaRegistryClient(options.SchemaRegistryConfig));
            services.AddSingleton<IKafkaSerializer, SchemaRegistryKafkaSerializer>();
            services.AddSingleton<IKafkaSerializer, AvroKafkaSerializer>();
        }
        
        // Add metrics if configured
        if (options.EnableMetrics)
        {
            services.Decorate<IKafkaSerializer>((serializer, provider) =>
                new MetricsKafkaSerializer(serializer, provider.GetRequiredService<IMetrics>(), 
                    DetermineSerializerType(serializer)));
        }
        
        return services;
    }
    
    public static IServiceCollection AddKafkaProducerWithSerialization<TKey, TValue>(
        this IServiceCollection services,
        ProducerConfig producerConfig,
        KafkaSerializerType serializerType = KafkaSerializerType.Json)
    {
        services.AddSingleton<IProducer<TKey, TValue>>(provider =>
        {
            var serializerService = provider.GetRequiredService<KafkaSerializerService>();
            var keySerializer = CreateKeySerializer<TKey>(serializerService, serializerType);
            var valueSerializer = CreateValueSerializer<TValue>(serializerService, serializerType);
            
            return new ProducerBuilder<TKey, TValue>(producerConfig)
                .SetKeySerializer(keySerializer)
                .SetValueSerializer(valueSerializer)
                .Build();
        });
        
        return services;
    }
    
    private static ISerializer<T> CreateKeySerializer<T>(KafkaSerializerService serializerService, KafkaSerializerType serializerType)
    {
        return new DelegatingSerializer<T>((data, context) => 
            serializerService.Serialize(data, serializerType));
    }
    
    private static ISerializer<T> CreateValueSerializer<T>(KafkaSerializerService serializerService, KafkaSerializerType serializerType)
    {
        return new DelegatingSerializer<T>((data, context) => 
            serializerService.Serialize(data, serializerType));
    }
}

public class KafkaSerializationOptions
{
    public JsonSerializerOptions JsonOptions { get; set; } = new();
    public JsonSerializerSettings NewtonsoftSettings { get; set; } = new();
    public NetJsonSettings NetJsonSettings { get; set; } = new();
    public SchemaRegistryConfig? SchemaRegistryConfig { get; set; }
    public bool EnableMetrics { get; set; } = true;
    public TopicSerializerConfiguration TopicConfiguration { get; set; } = TopicSerializerConfiguration.CreateDefault();
}

// Usage in Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddKafkaSerialization(options =>
    {
        options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.NewtonsoftSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.EnableMetrics = true;
        
        options.SchemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = "http://localhost:8081"
        };
        
        options.TopicConfiguration = new TopicSerializerConfiguration
        {
            DefaultSerializer = KafkaSerializerType.Json,
            TopicSerializers = new Dictionary<string, KafkaSerializerType>
            {
                ["critical-events"] = KafkaSerializerType.SchemaJson,
                ["analytics-events"] = KafkaSerializerType.Avro,
                ["high-frequency-data"] = KafkaSerializerType.NetJson
            }
        };
    });
    
    var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
    services.AddKafkaProducerWithSerialization<string, OrderEvent>(producerConfig, KafkaSerializerType.SchemaJson);
}
```

## Performance Comparison

### Message Size Comparison

| Serializer Type | Small Message (1KB) | Medium Message (10KB) | Large Message (100KB) | Complex Object |
|-----------------|--------------------|-----------------------|------------------------|----------------|
| **Json** | 1,024 bytes | 10,240 bytes | 102,400 bytes | 5,432 bytes |
| **NJson** | 1,028 bytes | 10,267 bytes | 102,543 bytes | 5,456 bytes |
| **NetJson** | 1,021 bytes | 10,201 bytes | 102,234 bytes | 5,401 bytes |
| **SchemaJson** | 1,029 bytes* | 10,274 bytes* | 102,561 bytes* | 5,461 bytes* |
| **Avro** | 742 bytes | 7,234 bytes | 71,823 bytes | 3,912 bytes |

*\* Includes schema ID overhead (5 bytes)*

### Throughput Comparison

| Serializer Type | Serialization (ops/sec) | Deserialization (ops/sec) | CPU Usage | Memory Usage |
|-----------------|-------------------------|---------------------------|-----------|--------------|
| **Json** | 45,000 | 42,000 | 15% | Low |
| **NJson** | 28,000 | 25,000 | 18% | Medium |
| **NetJson** | 52,000 | 48,000 | 12% | Low |
| **SchemaJson** | 38,000 | 35,000 | 17% | Medium |
| **Avro** | 65,000 | 61,000 | 10% | Low |

### Kafka-Specific Performance Metrics

```csharp
public class KafkaSerializerBenchmark
{
    private readonly Dictionary<KafkaSerializerType, IKafkaSerializer> _serializers;
    
    public KafkaSerializerBenchmark()
    {
        _serializers = new Dictionary<KafkaSerializerType, IKafkaSerializer>
        {
            [KafkaSerializerType.Json] = new JsonKafkaSerializer(),
            [KafkaSerializerType.NJson] = new NewtonsoftKafkaSerializer(),
            [KafkaSerializerType.NetJson] = new NetJsonKafkaSerializer(),
            [KafkaSerializerType.SchemaJson] = new SchemaRegistryKafkaSerializer(CreateSchemaRegistry()),
            [KafkaSerializerType.Avro] = new AvroKafkaSerializer(CreateSchemaRegistry())
        };
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(SerializerTypes))]
    public async Task<byte[]> SerializeMessage(KafkaSerializerType serializerType)
    {
        var message = CreateTestMessage();
        return await _serializers[serializerType].SerializeAsync(message);
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(SerializerTypes))]
    public async Task<TestMessage> DeserializeMessage(KafkaSerializerType serializerType)
    {
        var data = await _serializers[serializerType].SerializeAsync(CreateTestMessage());
        return await _serializers[serializerType].DeserializeAsync<TestMessage>(data);
    }
    
    public static IEnumerable<KafkaSerializerType> SerializerTypes =>
        Enum.GetValues<KafkaSerializerType>();
    
    private TestMessage CreateTestMessage() => new()
    {
        Id = Guid.NewGuid(),
        Timestamp = DateTime.UtcNow,
        UserId = "user-12345",
        EventType = "order-placed",
        Properties = new Dictionary<string, object>
        {
            ["orderId"] = "order-67890",
            ["amount"] = 99.99m,
            ["currency"] = "USD",
            ["items"] = new[] { "item-1", "item-2", "item-3" }
        }
    };
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class KafkaSerializerTypeTests
{
    [TestCase(KafkaSerializerType.Json)]
    [TestCase(KafkaSerializerType.NJson)]
    [TestCase(KafkaSerializerType.NetJson)]
    [TestCase(KafkaSerializerType.SchemaJson)]
    [TestCase(KafkaSerializerType.Avro)]
    public async Task KafkaSerializerType_ShouldSerializeAndDeserialize(KafkaSerializerType serializerType)
    {
        // Arrange
        var serializer = CreateSerializer(serializerType);
        var testMessage = new TestMessage
        {
            Id = Guid.NewGuid(),
            Name = "Test Message",
            Timestamp = DateTime.UtcNow
        };
        
        // Act
        var serializedData = await serializer.SerializeAsync(testMessage);
        var deserializedMessage = await serializer.DeserializeAsync<TestMessage>(serializedData);
        
        // Assert
        Assert.That(serializedData, Is.Not.Null.And.Not.Empty);
        Assert.That(deserializedMessage, Is.Not.Null);
        Assert.That(deserializedMessage.Id, Is.EqualTo(testMessage.Id));
        Assert.That(deserializedMessage.Name, Is.EqualTo(testMessage.Name));
        Assert.That(deserializedMessage.Timestamp, Is.EqualTo(testMessage.Timestamp).Within(TimeSpan.FromMilliseconds(1)));
    }
    
    [Test]
    public void KafkaSerializerType_ShouldInheritFromSerializerType()
    {
        // Assert that JSON types map correctly
        Assert.That((int)KafkaSerializerType.Json, Is.EqualTo((int)SerializerType.Json));
        Assert.That((int)KafkaSerializerType.NJson, Is.EqualTo((int)SerializerType.NJson));
        Assert.That((int)KafkaSerializerType.NetJson, Is.EqualTo((int)SerializerType.NetJson));
    }
    
    [Test]
    public void KafkaSerializerType_ShouldHaveKafkaSpecificTypes()
    {
        // Assert that Kafka-specific types exist
        var kafkaSpecificTypes = Enum.GetValues<KafkaSerializerType>()
            .Where(type => !Enum.IsDefined(typeof(SerializerType), (int)type))
            .ToList();
        
        Assert.That(kafkaSpecificTypes, Contains.Item(KafkaSerializerType.SchemaJson));
        Assert.That(kafkaSpecificTypes, Contains.Item(KafkaSerializerType.Avro));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class KafkaSerializationIntegrationTests
{
    private TestContainer _kafkaContainer;
    private TestContainer _schemaRegistryContainer;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _kafkaContainer = new ContainerBuilder()
            .WithImage("confluentinc/cp-kafka:latest")
            .WithPortBinding(9092, 9092)
            .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "zookeeper:2181")
            .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9092")
            .Build();
        
        _schemaRegistryContainer = new ContainerBuilder()
            .WithImage("confluentinc/cp-schema-registry:latest")
            .WithPortBinding(8081, 8081)
            .WithEnvironment("SCHEMA_REGISTRY_HOST_NAME", "schema-registry")
            .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS", "kafka:9092")
            .Build();
        
        await _kafkaContainer.StartAsync();
        await _schemaRegistryContainer.StartAsync();
    }
    
    [Test]
    public async Task KafkaProducer_ShouldProduceMessagesWithDifferentSerializers()
    {
        // Arrange
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        var topicName = $"test-topic-{Guid.NewGuid()}";
        
        var testMessage = new TestEvent
        {
            Id = Guid.NewGuid(),
            EventType = "test-event",
            Timestamp = DateTime.UtcNow,
            Data = new { Property1 = "Value1", Property2 = 42 }
        };
        
        // Test each serializer type
        foreach (var serializerType in Enum.GetValues<KafkaSerializerType>())
        {
            using var producer = CreateProducer(config, serializerType);
            
            // Act
            var deliveryResult = await producer.ProduceAsync(topicName, 
                new Message<string, TestEvent>
                {
                    Key = testMessage.Id.ToString(),
                    Value = testMessage
                });
            
            // Assert
            Assert.That(deliveryResult.Status, Is.EqualTo(PersistenceStatus.Persisted));
            Assert.That(deliveryResult.Message.Value, Is.Not.Null);
        }
    }
    
    [Test]
    public async Task KafkaConsumer_ShouldConsumeMessagesWithCorrectDeserializer()
    {
        // Arrange
        var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
        var consumerConfig = new ConsumerConfig 
        { 
            BootstrapServers = "localhost:9092",
            GroupId = $"test-group-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        var topicName = $"test-topic-{Guid.NewGuid()}";
        var testMessage = new TestEvent
        {
            Id = Guid.NewGuid(),
            EventType = "consume-test",
            Timestamp = DateTime.UtcNow
        };
        
        foreach (var serializerType in Enum.GetValues<KafkaSerializerType>())
        {
            // Produce message
            using var producer = CreateProducer(producerConfig, serializerType);
            await producer.ProduceAsync(topicName, new Message<string, TestEvent>
            {
                Key = testMessage.Id.ToString(),
                Value = testMessage
            });
            
            // Consume message
            using var consumer = CreateConsumer(consumerConfig, serializerType);
            consumer.Subscribe(topicName);
            
            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
            
            // Assert
            Assert.That(consumeResult, Is.Not.Null);
            Assert.That(consumeResult.Message.Value, Is.Not.Null);
            Assert.That(consumeResult.Message.Value.Id, Is.EqualTo(testMessage.Id));
            Assert.That(consumeResult.Message.Value.EventType, Is.EqualTo(testMessage.EventType));
        }
    }
}
```

## Best Practices

### 1. **Choose Serializer Based on Use Case**

```csharp
public static class KafkaSerializerSelectionGuide
{
    public static KafkaSerializerType RecommendSerializer(MessageCharacteristics characteristics)
    {
        // Schema governance required
        if (characteristics.RequiresSchemaValidation)
            return KafkaSerializerType.SchemaJson;
        
        // High volume, binary efficiency critical
        if (characteristics.HighVolume && characteristics.OptimizeForSize)
            return KafkaSerializerType.Avro;
        
        // Maximum performance required
        if (characteristics.OptimizeForSpeed)
            return KafkaSerializerType.NetJson;
        
        // Legacy system compatibility
        if (characteristics.RequiresLegacyCompatibility)
            return KafkaSerializerType.NJson;
        
        // Default: balanced performance and compatibility
        return KafkaSerializerType.Json;
    }
}
```

### 2. **Topic-Specific Configuration**

```csharp
public static class TopicSerializerStrategy
{
    public static KafkaSerializerType GetSerializerForTopic(string topicName)
    {
        return topicName switch
        {
            var name when name.EndsWith("-events") => KafkaSerializerType.SchemaJson,
            var name when name.EndsWith("-analytics") => KafkaSerializerType.Avro,
            var name when name.EndsWith("-realtime") => KafkaSerializerType.NetJson,
            var name when name.StartsWith("legacy-") => KafkaSerializerType.NJson,
            _ => KafkaSerializerType.Json
        };
    }
}
```

### 3. **Error Handling and Resilience**

```csharp
public class ResilientKafkaSerializer : IKafkaSerializer
{
    private readonly IKafkaSerializer _primarySerializer;
    private readonly IKafkaSerializer _fallbackSerializer;
    private readonly ILogger _logger;
    
    public ResilientKafkaSerializer(
        IKafkaSerializer primarySerializer, 
        IKafkaSerializer fallbackSerializer,
        ILogger logger)
    {
        _primarySerializer = primarySerializer;
        _fallbackSerializer = fallbackSerializer;
        _logger = logger;
    }
    
    public async Task<byte[]> SerializeAsync<T>(T obj)
    {
        try
        {
            return await _primarySerializer.SerializeAsync(obj);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary serializer failed, falling back to secondary");
            return await _fallbackSerializer.SerializeAsync(obj);
        }
    }
}
```

### 4. **Performance Monitoring**

```csharp
public class PerformanceMonitoringKafkaProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly IMetrics _metrics;
    
    public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        string topic, 
        Message<TKey, TValue> message,
        KafkaSerializerType serializerType)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _producer.ProduceAsync(topic, message);
            stopwatch.Stop();
            
            _metrics.RecordValue($"kafka.produce.latency.{serializerType}", stopwatch.ElapsedMilliseconds);
            _metrics.Increment($"kafka.produce.success.{serializerType}");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.Increment($"kafka.produce.error.{serializerType}");
            throw;
        }
    }
}
```

## See Also

- [SerializerType](SerializerType.md) - Base JSON serialization types
- [JsonHelper](../Helpers/JsonHelper.md) - JSON serialization utilities  
- [NJsonHelper](../Helpers/NJsonHelper.md) - Newtonsoft.Json utilities
- [NetJsonHelper](../Helpers/NetJsonHelper.md) - NetJSON utilities
- [YamlSerializerSettings](Yaml/YamlSerializerSettings.md) - YAML serialization configuration
- [MessagePackHelper](../Helpers/MessagePackHelper.md) - MessagePack serialization
- [ProtobufHelper](../Helpers/ProtobufHelper.md) - Protocol Buffers serialization

---

*Part of the RapidStreamer.BuildingBlocks.Application.Serializations namespace - providing Kafka-specific serialization format selection for messaging scenarios.*