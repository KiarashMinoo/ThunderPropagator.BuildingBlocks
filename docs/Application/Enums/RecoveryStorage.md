# RecoveryStorage

The `RecoveryStorage` enum defines available storage backends for data recovery and persistence operations in the RapidStreamer BuildingBlocks framework. It uses the `[Flags]` attribute to support multiple storage options simultaneously, enabling flexible and redundant storage configurations.

## Overview

The `RecoveryStorage` enum is designed for systems that need fault tolerance and data recovery capabilities. It allows applications to configure multiple storage backends for redundancy, implement storage hierarchies, and provide fallback mechanisms for critical data persistence.

## Enum Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.Enums
{
    [Flags]
    public enum RecoveryStorage
    {
        None = 0,
        Redis = 1,
        MongoDb = 2,
        Postgresql = 3,
    }
}
```

> **Note**: The current implementation has overlapping bit values (MongoDb = 2, Postgresql = 3). For proper flags behavior, values should be powers of 2. This documentation will show both current usage and recommended proper flag values.

## Values

### None
- **Value**: `0`
- **Description**: No recovery storage configured
- **Use Case**: Disable recovery storage, testing scenarios, stateless operations
- **Behavior**: No data persistence or recovery capabilities

### Redis
- **Value**: `1` (Binary: `0001`)
- **Description**: Redis in-memory data store for fast recovery
- **Use Case**: Session storage, caching, real-time data, temporary recovery data
- **Characteristics**: High performance, volatile storage, distributed caching
- **Best For**: Frequently accessed recovery data, session state, cache invalidation

### MongoDb
- **Value**: `2` (Binary: `0010`)
- **Description**: MongoDB document database for flexible recovery storage
- **Use Case**: Complex document storage, schema-less recovery data, JSON documents
- **Characteristics**: Document-oriented, flexible schema, horizontal scaling
- **Best For**: Complex nested data, audit trails, configuration snapshots

### Postgresql
- **Value**: `3` (Binary: `0011`) - *Current Implementation Issue*
- **Recommended Value**: `4` (Binary: `0100`)
- **Description**: PostgreSQL relational database for structured recovery storage
- **Use Case**: Transactional data, structured recovery information, ACID compliance
- **Characteristics**: ACID transactions, structured schema, SQL queries
- **Best For**: Critical transactional data, structured audit logs, relational recovery data

## Usage Examples

### Basic Storage Configuration

```csharp
using RapidStreamer.BuildingBlocks.Application.Enums;

public class RecoveryConfiguration
{
    public RecoveryStorage StorageOptions { get; set; } = RecoveryStorage.None;
    public string? RedisConnectionString { get; set; }
    public string? MongoDbConnectionString { get; set; }
    public string? PostgresqlConnectionString { get; set; }
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);
}

// Single storage backend
var redisConfig = new RecoveryConfiguration
{
    StorageOptions = RecoveryStorage.Redis,
    RedisConnectionString = "localhost:6379",
    RetentionPeriod = TimeSpan.FromHours(24)
};

// Multiple storage backends (with proper flag values)
var redundantConfig = new RecoveryConfiguration
{
    StorageOptions = RecoveryStorage.Redis | RecoveryStorage.MongoDb,
    RedisConnectionString = "localhost:6379",
    MongoDbConnectionString = "mongodb://localhost:27017/recovery",
    RetentionPeriod = TimeSpan.FromDays(7)
};

// All storage options enabled
var comprehensiveConfig = new RecoveryConfiguration
{
    StorageOptions = RecoveryStorage.Redis | RecoveryStorage.MongoDb | RecoveryStorage.Postgresql,
    RedisConnectionString = "localhost:6379",
    MongoDbConnectionString = "mongodb://localhost:27017/recovery",
    PostgresqlConnectionString = "Host=localhost;Database=recovery;Username=postgres;Password=password"
};
```

### Storage Manager Implementation

```csharp
public class RecoveryStorageManager
{
    private readonly RecoveryConfiguration _config;
    private readonly IRedisStorage? _redisStorage;
    private readonly IMongoDbStorage? _mongoStorage;
    private readonly IPostgresqlStorage? _postgresStorage;
    private readonly ILogger<RecoveryStorageManager> _logger;
    
    public RecoveryStorageManager(
        RecoveryConfiguration config,
        IRedisStorage? redisStorage = null,
        IMongoDbStorage? mongoStorage = null,
        IPostgresqlStorage? postgresStorage = null,
        ILogger<RecoveryStorageManager> logger = null!)
    {
        _config = config;
        _redisStorage = redisStorage;
        _mongoStorage = mongoStorage;
        _postgresStorage = postgresStorage;
        _logger = logger;
    }
    
    public async Task<bool> StoreRecoveryDataAsync<T>(string key, T data, 
        RecoveryStorage? storageOverride = null)
    {
        var targetStorage = storageOverride ?? _config.StorageOptions;
        
        if (targetStorage == RecoveryStorage.None)
        {
            _logger?.LogWarning("No recovery storage configured for key: {Key}", key);
            return false;
        }
        
        var results = new List<bool>();
        
        // Store in Redis if enabled
        if (targetStorage.HasFlag(RecoveryStorage.Redis) && _redisStorage != null)
        {
            try
            {
                var redisResult = await _redisStorage.SetAsync(key, data, _config.RetentionPeriod);
                results.Add(redisResult);
                
                _logger?.LogDebug("Stored recovery data in Redis for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to store recovery data in Redis for key: {Key}", key);
                results.Add(false);
            }
        }
        
        // Store in MongoDB if enabled
        if (targetStorage.HasFlag(RecoveryStorage.MongoDb) && _mongoStorage != null)
        {
            try
            {
                var mongoResult = await _mongoStorage.InsertAsync(key, data);
                results.Add(mongoResult);
                
                _logger?.LogDebug("Stored recovery data in MongoDB for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to store recovery data in MongoDB for key: {Key}", key);
                results.Add(false);
            }
        }
        
        // Store in PostgreSQL if enabled
        if (targetStorage.HasFlag(RecoveryStorage.Postgresql) && _postgresStorage != null)
        {
            try
            {
                var postgresResult = await _postgresStorage.SaveAsync(key, data);
                results.Add(postgresResult);
                
                _logger?.LogDebug("Stored recovery data in PostgreSQL for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to store recovery data in PostgreSQL for key: {Key}", key);
                results.Add(false);
            }
        }
        
        // Return true if at least one storage succeeded
        var success = results.Any(r => r);
        
        if (!success)
        {
            _logger?.LogError("Failed to store recovery data in any configured storage for key: {Key}", key);
        }
        
        return success;
    }
    
    public async Task<T?> RetrieveRecoveryDataAsync<T>(string key, 
        RecoveryStorage? storagePreference = null)
    {
        var searchOrder = GetStorageSearchOrder(storagePreference);
        
        foreach (var storage in searchOrder)
        {
            try
            {
                var result = await TryRetrieveFromStorage<T>(key, storage);
                
                if (result != null)
                {
                    _logger?.LogDebug("Retrieved recovery data from {Storage} for key: {Key}", 
                        storage, key);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to retrieve from {Storage} for key: {Key}", 
                    storage, key);
            }
        }
        
        _logger?.LogWarning("Recovery data not found in any storage for key: {Key}", key);
        return default;
    }
    
    private IEnumerable<RecoveryStorage> GetStorageSearchOrder(RecoveryStorage? preference)
    {
        var available = _config.StorageOptions;
        
        if (preference.HasValue)
        {
            // If preference specified, try it first
            if (available.HasFlag(preference.Value))
            {
                if (preference.Value.HasFlag(RecoveryStorage.Redis))
                    yield return RecoveryStorage.Redis;
                if (preference.Value.HasFlag(RecoveryStorage.MongoDb))
                    yield return RecoveryStorage.MongoDb;
                if (preference.Value.HasFlag(RecoveryStorage.Postgresql))
                    yield return RecoveryStorage.Postgresql;
            }
        }
        else
        {
            // Default search order: fastest to slowest
            if (available.HasFlag(RecoveryStorage.Redis))
                yield return RecoveryStorage.Redis;
            if (available.HasFlag(RecoveryStorage.MongoDb))
                yield return RecoveryStorage.MongoDb;
            if (available.HasFlag(RecoveryStorage.Postgresql))
                yield return RecoveryStorage.Postgresql;
        }
    }
    
    private async Task<T?> TryRetrieveFromStorage<T>(string key, RecoveryStorage storage)
    {
        return storage switch
        {
            RecoveryStorage.Redis when _redisStorage != null => 
                await _redisStorage.GetAsync<T>(key),
                
            RecoveryStorage.MongoDb when _mongoStorage != null => 
                await _mongoStorage.FindAsync<T>(key),
                
            RecoveryStorage.Postgresql when _postgresStorage != null => 
                await _postgresStorage.LoadAsync<T>(key),
                
            _ => default
        };
    }
    
    public async Task<bool> DeleteRecoveryDataAsync(string key)
    {
        var results = new List<bool>();
        var targetStorage = _config.StorageOptions;
        
        if (targetStorage.HasFlag(RecoveryStorage.Redis) && _redisStorage != null)
        {
            results.Add(await _redisStorage.DeleteAsync(key));
        }
        
        if (targetStorage.HasFlag(RecoveryStorage.MongoDb) && _mongoStorage != null)
        {
            results.Add(await _mongoStorage.DeleteAsync(key));
        }
        
        if (targetStorage.HasFlag(RecoveryStorage.Postgresql) && _postgresStorage != null)
        {
            results.Add(await _postgresStorage.DeleteAsync(key));
        }
        
        return results.All(r => r);
    }
}
```

### Storage Backend Interfaces

```csharp
public interface IRedisStorage
{
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}

public interface IMongoDbStorage
{
    Task<bool> InsertAsync<T>(string key, T document);
    Task<T?> FindAsync<T>(string key);
    Task<bool> UpdateAsync<T>(string key, T document);
    Task<bool> DeleteAsync(string key);
}

public interface IPostgresqlStorage
{
    Task<bool> SaveAsync<T>(string key, T data);
    Task<T?> LoadAsync<T>(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}
```

### Hierarchical Recovery Strategy

```csharp
public class HierarchicalRecoveryService
{
    private readonly RecoveryStorageManager _storageManager;
    private readonly ILogger<HierarchicalRecoveryService> _logger;
    
    public HierarchicalRecoveryService(RecoveryStorageManager storageManager,
        ILogger<HierarchicalRecoveryService> logger)
    {
        _storageManager = storageManager;
        _logger = logger;
    }
    
    public async Task<bool> StoreWithHierarchy<T>(string key, T data, 
        RecoveryPriority priority = RecoveryPriority.Normal)
    {
        var storageStrategy = GetStorageStrategy(priority);
        
        return await _storageManager.StoreRecoveryDataAsync(key, data, storageStrategy);
    }
    
    public async Task<T?> RetrieveWithFallback<T>(string key, 
        RecoveryPriority priority = RecoveryPriority.Normal)
    {
        var searchOrder = GetRetrievalOrder(priority);
        
        foreach (var storage in searchOrder)
        {
            try
            {
                var result = await _storageManager.RetrieveRecoveryDataAsync<T>(key, storage);
                
                if (result != null)
                {
                    // If found in slower storage, promote to faster storage
                    if (storage != RecoveryStorage.Redis)
                    {
                        _ = Task.Run(async () => await PromoteToFasterStorage(key, result));
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallback failed for storage {Storage}, key: {Key}", 
                    storage, key);
            }
        }
        
        return default;
    }
    
    private RecoveryStorage GetStorageStrategy(RecoveryPriority priority)
    {
        return priority switch
        {
            RecoveryPriority.Critical => RecoveryStorage.Redis | RecoveryStorage.MongoDb | RecoveryStorage.Postgresql,
            RecoveryPriority.High => RecoveryStorage.Redis | RecoveryStorage.Postgresql,
            RecoveryPriority.Normal => RecoveryStorage.Redis | RecoveryStorage.MongoDb,
            RecoveryPriority.Low => RecoveryStorage.MongoDb,
            _ => RecoveryStorage.Redis
        };
    }
    
    private IEnumerable<RecoveryStorage> GetRetrievalOrder(RecoveryPriority priority)
    {
        // Always try fastest first, but vary depth based on priority
        yield return RecoveryStorage.Redis;
        
        if (priority >= RecoveryPriority.Normal)
        {
            yield return RecoveryStorage.MongoDb;
        }
        
        if (priority >= RecoveryPriority.High)
        {
            yield return RecoveryStorage.Postgresql;
        }
    }
    
    private async Task PromoteToFasterStorage<T>(string key, T data)
    {
        try
        {
            await _storageManager.StoreRecoveryDataAsync(key, data, RecoveryStorage.Redis);
            _logger.LogDebug("Promoted data to Redis for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to promote data to Redis for key: {Key}", key);
        }
    }
}

public enum RecoveryPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}
```

### Configuration and Dependency Injection

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRecoveryStorage(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<RecoveryConfiguration>(
            configuration.GetSection("RecoveryStorage"));
        
        // Register storage implementations
        services.AddScoped<IRedisStorage, RedisStorageImplementation>();
        services.AddScoped<IMongoDbStorage, MongoDbStorageImplementation>();
        services.AddScoped<IPostgresqlStorage, PostgresqlStorageImplementation>();
        
        // Register recovery services
        services.AddScoped<RecoveryStorageManager>();
        services.AddScoped<HierarchicalRecoveryService>();
        
        // Register health checks
        services.AddHealthChecks()
            .AddCheck<RecoveryStorageHealthCheck>("recovery-storage");
        
        return services;
    }
}

// appsettings.json configuration
{
  "RecoveryStorage": {
    "StorageOptions": "Redis, MongoDb",
    "RedisConnectionString": "localhost:6379",
    "MongoDbConnectionString": "mongodb://localhost:27017/recovery",
    "PostgresqlConnectionString": "Host=localhost;Database=recovery;Username=postgres;Password=password",
    "RetentionPeriod": "7.00:00:00"
  }
}
```

### Health Monitoring

```csharp
public class RecoveryStorageHealthCheck : IHealthCheck
{
    private readonly RecoveryConfiguration _config;
    private readonly IRedisStorage? _redisStorage;
    private readonly IMongoDbStorage? _mongoStorage;
    private readonly IPostgresqlStorage? _postgresStorage;
    
    public RecoveryStorageHealthCheck(
        IOptionsMonitor<RecoveryConfiguration> config,
        IRedisStorage? redisStorage = null,
        IMongoDbStorage? mongoStorage = null,
        IPostgresqlStorage? postgresStorage = null)
    {
        _config = config.CurrentValue;
        _redisStorage = redisStorage;
        _mongoStorage = mongoStorage;
        _postgresStorage = postgresStorage;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, object>();
        var healthyCount = 0;
        var totalConfigured = 0;
        
        // Check Redis
        if (_config.StorageOptions.HasFlag(RecoveryStorage.Redis))
        {
            totalConfigured++;
            var redisHealthy = await CheckRedisHealth();
            results["Redis"] = redisHealthy ? "Healthy" : "Unhealthy";
            if (redisHealthy) healthyCount++;
        }
        
        // Check MongoDB
        if (_config.StorageOptions.HasFlag(RecoveryStorage.MongoDb))
        {
            totalConfigured++;
            var mongoHealthy = await CheckMongoDbHealth();
            results["MongoDb"] = mongoHealthy ? "Healthy" : "Unhealthy";
            if (mongoHealthy) healthyCount++;
        }
        
        // Check PostgreSQL
        if (_config.StorageOptions.HasFlag(RecoveryStorage.Postgresql))
        {
            totalConfigured++;
            var postgresHealthy = await CheckPostgresqlHealth();
            results["Postgresql"] = postgresHealthy ? "Healthy" : "Unhealthy";
            if (postgresHealthy) healthyCount++;
        }
        
        results["HealthyStorages"] = healthyCount;
        results["TotalConfigured"] = totalConfigured;
        
        var status = healthyCount == 0 ? HealthStatus.Unhealthy :
                    healthyCount < totalConfigured ? HealthStatus.Degraded :
                    HealthStatus.Healthy;
        
        var description = $"{healthyCount}/{totalConfigured} recovery storage backends healthy";
        
        return new HealthCheckResult(status, description, data: results);
    }
    
    private async Task<bool> CheckRedisHealth()
    {
        if (_redisStorage == null) return false;
        
        try
        {
            var testKey = $"health-check-{Guid.NewGuid()}";
            await _redisStorage.SetAsync(testKey, "health-check", TimeSpan.FromSeconds(10));
            var result = await _redisStorage.GetAsync<string>(testKey);
            await _redisStorage.DeleteAsync(testKey);
            
            return result == "health-check";
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> CheckMongoDbHealth()
    {
        if (_mongoStorage == null) return false;
        
        try
        {
            var testKey = $"health-check-{Guid.NewGuid()}";
            await _mongoStorage.InsertAsync(testKey, new { HealthCheck = true });
            var result = await _mongoStorage.FindAsync<object>(testKey);
            await _mongoStorage.DeleteAsync(testKey);
            
            return result != null;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> CheckPostgresqlHealth()
    {
        if (_postgresStorage == null) return false;
        
        try
        {
            var testKey = $"health-check-{Guid.NewGuid()}";
            await _postgresStorage.SaveAsync(testKey, new { HealthCheck = true });
            var result = await _postgresStorage.LoadAsync<object>(testKey);
            await _postgresStorage.DeleteAsync(testKey);
            
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}
```

## Proper Flags Implementation

```csharp
// Recommended proper flags implementation
[Flags]
public enum RecoveryStorageProper
{
    None = 0,
    Redis = 1,      // 2^0 = 1
    MongoDb = 2,    // 2^1 = 2  
    Postgresql = 4  // 2^2 = 4
}

// Extension methods for working with flags
public static class RecoveryStorageExtensions
{
    public static bool HasStorage(this RecoveryStorage storage, RecoveryStorage target)
    {
        return (storage & target) == target;
    }
    
    public static RecoveryStorage AddStorage(this RecoveryStorage storage, RecoveryStorage target)
    {
        return storage | target;
    }
    
    public static RecoveryStorage RemoveStorage(this RecoveryStorage storage, RecoveryStorage target)
    {
        return storage & ~target;
    }
    
    public static IEnumerable<RecoveryStorage> GetIndividualStorages(this RecoveryStorage storage)
    {
        if (storage.HasFlag(RecoveryStorage.Redis))
            yield return RecoveryStorage.Redis;
        if (storage.HasFlag(RecoveryStorage.MongoDb))
            yield return RecoveryStorage.MongoDb;
        if (storage.HasFlag(RecoveryStorage.Postgresql))
            yield return RecoveryStorage.Postgresql;
    }
    
    public static string ToDisplayString(this RecoveryStorage storage)
    {
        if (storage == RecoveryStorage.None)
            return "None";
        
        var individual = storage.GetIndividualStorages().Select(s => s.ToString());
        return string.Join(", ", individual);
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[TestClass]
public class RecoveryStorageTests
{
    [TestMethod]
    public void RecoveryStorage_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)RecoveryStorage.None);
        Assert.AreEqual(1, (int)RecoveryStorage.Redis);
        Assert.AreEqual(2, (int)RecoveryStorage.MongoDb);
        Assert.AreEqual(3, (int)RecoveryStorage.Postgresql);
    }
    
    [TestMethod]
    public void RecoveryStorage_SupportsFlagsOperations()
    {
        var combined = RecoveryStorage.Redis | RecoveryStorage.MongoDb;
        
        Assert.IsTrue(combined.HasFlag(RecoveryStorage.Redis));
        Assert.IsTrue(combined.HasFlag(RecoveryStorage.MongoDb));
        Assert.IsFalse(combined.HasFlag(RecoveryStorage.Postgresql));
    }
    
    [TestMethod]
    public void RecoveryStorageExtensions_WorkCorrectly()
    {
        var storage = RecoveryStorage.None;
        
        storage = storage.AddStorage(RecoveryStorage.Redis);
        Assert.IsTrue(storage.HasStorage(RecoveryStorage.Redis));
        
        storage = storage.AddStorage(RecoveryStorage.MongoDb);
        Assert.IsTrue(storage.HasStorage(RecoveryStorage.Redis | RecoveryStorage.MongoDb));
        
        storage = storage.RemoveStorage(RecoveryStorage.Redis);
        Assert.IsFalse(storage.HasStorage(RecoveryStorage.Redis));
        Assert.IsTrue(storage.HasStorage(RecoveryStorage.MongoDb));
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class RecoveryStorageIntegrationTests
{
    private RecoveryStorageManager _storageManager;
    private Mock<IRedisStorage> _redisMock;
    private Mock<IMongoDbStorage> _mongoMock;
    
    [TestInitialize]
    public void Setup()
    {
        _redisMock = new Mock<IRedisStorage>();
        _mongoMock = new Mock<IMongoDbStorage>();
        
        var config = new RecoveryConfiguration
        {
            StorageOptions = RecoveryStorage.Redis | RecoveryStorage.MongoDb,
            RetentionPeriod = TimeSpan.FromHours(1)
        };
        
        _storageManager = new RecoveryStorageManager(
            config,
            _redisMock.Object,
            _mongoMock.Object,
            null,
            Mock.Of<ILogger<RecoveryStorageManager>>());
    }
    
    [TestMethod]
    public async Task StoreRecoveryData_CallsAllConfiguredStorages()
    {
        // Arrange
        var testData = new { Id = 1, Name = "Test" };
        var key = "test-key";
        
        _redisMock.Setup(r => r.SetAsync(key, testData, It.IsAny<TimeSpan>()))
                  .ReturnsAsync(true);
        _mongoMock.Setup(m => m.InsertAsync(key, testData))
                  .ReturnsAsync(true);
        
        // Act
        var result = await _storageManager.StoreRecoveryDataAsync(key, testData);
        
        // Assert
        Assert.IsTrue(result);
        _redisMock.Verify(r => r.SetAsync(key, testData, It.IsAny<TimeSpan>()), Times.Once);
        _mongoMock.Verify(m => m.InsertAsync(key, testData), Times.Once);
    }
    
    [TestMethod]
    public async Task RetrieveRecoveryData_ReturnsFromFirstAvailableStorage()
    {
        // Arrange
        var testData = new { Id = 1, Name = "Test" };
        var key = "test-key";
        
        _redisMock.Setup(r => r.GetAsync<object>(key))
                  .ReturnsAsync(testData);
        
        // Act
        var result = await _storageManager.RetrieveRecoveryDataAsync<object>(key);
        
        // Assert
        Assert.AreEqual(testData, result);
        _redisMock.Verify(r => r.GetAsync<object>(key), Times.Once);
        _mongoMock.Verify(m => m.FindAsync<object>(key), Times.Never);
    }
}
```

## Performance Considerations

### Storage Performance Characteristics

| Storage | Read Latency | Write Latency | Throughput | Durability | Scalability |
|---------|--------------|---------------|------------|------------|-------------|
| Redis | Lowest | Lowest | Highest | Medium | Excellent |
| MongoDB | Medium | Medium | Medium | High | Excellent |
| PostgreSQL | Medium | Highest | Medium | Highest | Good |

### Optimization Strategies

```csharp
public class OptimizedRecoveryService
{
    public async Task<bool> OptimizedStore<T>(string key, T data, 
        RecoveryStorage storage)
    {
        var tasks = new List<Task<bool>>();
        
        // Parallel storage for redundancy
        if (storage.HasFlag(RecoveryStorage.Redis))
        {
            tasks.Add(StoreInRedis(key, data));
        }
        
        if (storage.HasFlag(RecoveryStorage.MongoDb))
        {
            tasks.Add(StoreInMongo(key, data));
        }
        
        if (storage.HasFlag(RecoveryStorage.Postgresql))
        {
            tasks.Add(StoreInPostgres(key, data));
        }
        
        var results = await Task.WhenAll(tasks);
        
        // Return true if at least one succeeded
        return results.Any(r => r);
    }
    
    private async Task<bool> StoreInRedis<T>(string key, T data)
    {
        // Optimized Redis storage
        await Task.Delay(1); // Simulate fast storage
        return true;
    }
    
    private async Task<bool> StoreInMongo<T>(string key, T data)
    {
        // Optimized MongoDB storage
        await Task.Delay(5); // Simulate medium storage
        return true;
    }
    
    private async Task<bool> StoreInPostgres<T>(string key, T data)
    {
        // Optimized PostgreSQL storage
        await Task.Delay(10); // Simulate slower storage
        return true;
    }
}
```

## Best Practices

1. **Flags Design**: Use proper power-of-2 values for flags enum
2. **Redundancy**: Use multiple storage backends for critical data
3. **Performance Tiering**: Fast storage for frequent access, durable storage for long-term retention
4. **Health Monitoring**: Implement health checks for all configured storage backends
5. **Graceful Degradation**: Continue operating even if some storage backends fail
6. **Configuration**: Make storage options configurable through appsettings
7. **Error Handling**: Implement robust error handling and retry mechanisms

## Related Components

- **Storage Implementations**: Concrete implementations of Redis, MongoDB, and PostgreSQL storage
- **Health Monitoring**: Health check systems for storage availability
- **Configuration Management**: For managing storage connection strings and options
- **Retry Policies**: For handling transient failures in storage operations

## See Also

- [Enums System Overview](README.md)
- [Storage Implementation Patterns](../Patterns/StorageImplementation.md)
- [Health Monitoring Systems](../Monitoring/HealthChecks.md)