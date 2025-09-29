# ServiceConfiguration

The `ServiceConfiguration` abstract class provides a flexible, thread-safe foundation for configuration management with automatic JSON serialization, property change notifications, and type-safe value access. It combines a key-value store approach with strongly-typed property access patterns, making it ideal for service configuration scenarios.

## Overview

```csharp
public interface IServiceConfiguration : IEnumerable<KeyValuePair<string, string>>;

[JsonConverter(typeof(ServiceConfigurationJsonConverter))]
public abstract class ServiceConfiguration : IServiceConfiguration,
    INotifyPropertyChanged,
    INotifyPropertyChanging,
    IEquatable<ServiceConfiguration>
{
    protected ServiceConfiguration();
    protected ServiceConfiguration(IEnumerable<KeyValuePair<string, string>> properties);
    protected ServiceConfiguration(ServiceConfiguration serviceConfiguration);
    
    protected void Set<T>(T? value, [CallerMemberName] string? key = null);
    protected T? Get<T>([CallerMemberName] string? key = null);
    protected T Get<T>(T defaultValue, [CallerMemberName] string? key = null);
    
    public static TServiceConfiguration CreateNew<TServiceConfiguration>()
        where TServiceConfiguration : ServiceConfiguration, new();
}
```

The `ServiceConfiguration` class is designed to provide:
- **Thread-Safe Storage**: Concurrent dictionary-based property storage
- **Type Safety**: Strongly-typed property access with automatic serialization
- **Change Notifications**: INotifyPropertyChanged/INotifyPropertyChanging support
- **JSON Integration**: Automatic JSON serialization with camelCase/PascalCase conversion
- **Extensibility**: Abstract base class pattern for custom configuration types

## Key Features

### Thread-Safe Property Management
- **Concurrent Dictionary**: Thread-safe key-value storage for configuration properties
- **Atomic Operations**: Thread-safe property updates and retrievals
- **Performance Optimized**: Efficient concurrent access patterns

### Type-Safe Value Access
- **Generic Methods**: Type-safe Get/Set operations with automatic conversion
- **Default Values**: Support for default values when properties don't exist
- **Complex Types**: JSON serialization support for complex objects

### Property Change Notifications
- **INotifyPropertyChanged**: Event-driven property change notifications
- **INotifyPropertyChanging**: Pre-change notification support
- **CallerMemberName**: Automatic property name resolution

### JSON Serialization
- **Custom Converter**: Automatic camelCase/PascalCase conversion for JSON
- **Bidirectional**: Support for both serialization and deserialization
- **Type Preservation**: Maintains type information during serialization

## Constructor Details

### Default Constructor
```csharp
protected ServiceConfiguration()
```
Creates an empty configuration with initialized concurrent dictionary.

### Collection Constructor
```csharp
protected ServiceConfiguration(IEnumerable<KeyValuePair<string, string>> properties)
```
Initializes configuration with provided key-value pairs.

### Copy Constructor
```csharp
protected ServiceConfiguration(ServiceConfiguration serviceConfiguration)
```
Creates a copy of an existing configuration instance.

## Core Methods

### Set Method
```csharp
protected void Set<T>(T? value, [CallerMemberName] string? key = null)
```
Sets a property value with automatic type conversion and change notifications.

### Get Methods
```csharp
protected T? Get<T>([CallerMemberName] string? key = null)
protected T Get<T>(T defaultValue, [CallerMemberName] string? key = null)
```
Retrieves property values with automatic type conversion and default value support.

### Factory Methods
```csharp
public static TServiceConfiguration CreateNew<TServiceConfiguration>()
public static TServiceConfiguration CreateNew<TServiceConfiguration>(IEnumerable<KeyValuePair<string, string>> properties)
public static TServiceConfiguration CreateNew<TServiceConfiguration>(ServiceConfiguration serviceConfiguration)
```
Generic factory methods for creating configuration instances.

## Usage Examples

### Basic Configuration Class

```csharp
public class DatabaseConfiguration : ServiceConfiguration
{
    public string ConnectionString
    {
        get => Get<string>() ?? "";
        set => Set(value);
    }
    
    public int ConnectionTimeout
    {
        get => Get(30); // Default 30 seconds
        set => Set(value);
    }
    
    public bool EnableRetry
    {
        get => Get<bool>();
        set => Set(value);
    }
    
    public TimeSpan RetryInterval
    {
        get => Get(TimeSpan.FromSeconds(5)); // Default 5 seconds
        set => Set(value);
    }
    
    public List<string> AllowedHosts
    {
        get => Get<List<string>>() ?? new List<string>();
        set => Set(value);
    }
    
    public DatabaseType DatabaseType
    {
        get => Get<DatabaseType>();
        set => Set(value);
    }
    
    // Validation methods
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ConnectionString) &&
               ConnectionTimeout > 0 &&
               RetryInterval > TimeSpan.Zero;
    }
    
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(ConnectionString))
            result.Errors.Add("ConnectionString is required");
        
        if (ConnectionTimeout <= 0)
            result.Errors.Add("ConnectionTimeout must be greater than 0");
        
        if (RetryInterval <= TimeSpan.Zero)
            result.Errors.Add("RetryInterval must be greater than zero");
        
        return result;
    }
}

public enum DatabaseType
{
    SqlServer,
    PostgreSQL,
    MySQL,
    Oracle,
    SQLite
}

public class ValidationResult
{
    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
}

public class DatabaseConfigurationDemo
{
    public static void DemonstrateBasicUsage()
    {
        // Create new configuration
        var config = ServiceConfiguration.CreateNew<DatabaseConfiguration>();
        
        // Set properties
        config.ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;";
        config.ConnectionTimeout = 60;
        config.EnableRetry = true;
        config.RetryInterval = TimeSpan.FromSeconds(10);
        config.AllowedHosts = new List<string> { "localhost", "127.0.0.1" };
        config.DatabaseType = DatabaseType.SqlServer;
        
        // Display configuration
        Console.WriteLine($"Database Type: {config.DatabaseType}");
        Console.WriteLine($"Connection Timeout: {config.ConnectionTimeout} seconds");
        Console.WriteLine($"Retry Enabled: {config.EnableRetry}");
        Console.WriteLine($"Retry Interval: {config.RetryInterval}");
        Console.WriteLine($"Allowed Hosts: {string.Join(", ", config.AllowedHosts)}");
        
        // Validate configuration
        var validation = config.Validate();
        if (validation.IsValid)
        {
            Console.WriteLine("Configuration is valid");
        }
        else
        {
            Console.WriteLine("Configuration errors:");
            foreach (var error in validation.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
    }
}
```

### Advanced Configuration with Nested Objects

```csharp
public class ApplicationConfiguration : ServiceConfiguration
{
    // Basic properties
    public string ApplicationName
    {
        get => Get<string>() ?? "DefaultApp";
        set => Set(value);
    }
    
    public string Version
    {
        get => Get<string>() ?? "1.0.0";
        set => Set(value);
    }
    
    public LogLevel LogLevel
    {
        get => Get(LogLevel.Information);
        set => Set(value);
    }
    
    // Complex nested objects
    public ApiSettings ApiSettings
    {
        get => Get<ApiSettings>() ?? new ApiSettings();
        set => Set(value);
    }
    
    public CacheSettings CacheSettings
    {
        get => Get<CacheSettings>() ?? new CacheSettings();
        set => Set(value);
    }
    
    public SecuritySettings SecuritySettings
    {
        get => Get<SecuritySettings>() ?? new SecuritySettings();
        set => Set(value);
    }
    
    public List<EnvironmentConfig> Environments
    {
        get => Get<List<EnvironmentConfig>>() ?? new List<EnvironmentConfig>();
        set => Set(value);
    }
    
    // Computed properties
    public bool IsProduction => EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);
    
    public string EnvironmentName
    {
        get => Get<string>() ?? "Development";
        set => Set(value);
    }
    
    public EnvironmentConfig? CurrentEnvironment =>
        Environments.FirstOrDefault(e => e.Name.Equals(EnvironmentName, StringComparison.OrdinalIgnoreCase));
    
    // Configuration methods
    public void ApplyEnvironmentSettings(string environmentName)
    {
        var env = Environments.FirstOrDefault(e => e.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
        if (env != null)
        {
            EnvironmentName = env.Name;
            LogLevel = env.LogLevel;
            
            if (env.ApiSettings != null)
                ApiSettings = env.ApiSettings;
            
            if (env.CacheSettings != null)
                CacheSettings = env.CacheSettings;
        }
    }
    
    public ApplicationConfiguration CreateEnvironmentCopy(string environmentName)
    {
        var copy = ServiceConfiguration.CreateNew<ApplicationConfiguration>(this);
        copy.ApplyEnvironmentSettings(environmentName);
        return copy;
    }
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public string ApiKey { get; set; } = "";
    public List<string> AllowedOrigins { get; set; } = new();
    public bool EnableCors { get; set; }
    public RateLimitSettings RateLimit { get; set; } = new();
}

public class RateLimitSettings
{
    public int RequestsPerMinute { get; set; } = 100;
    public int BurstLimit { get; set; } = 200;
    public TimeSpan WindowSize { get; set; } = TimeSpan.FromMinutes(1);
}

public class CacheSettings
{
    public string Provider { get; set; } = "Memory";
    public string ConnectionString { get; set; } = "";
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxSizeInMB { get; set; } = 100;
    public bool EnableDistributedCache { get; set; }
}

public class SecuritySettings
{
    public string JwtSecret { get; set; } = "";
    public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromHours(24);
    public bool RequireHttps { get; set; } = true;
    public List<string> AllowedHosts { get; set; } = new();
    public EncryptionSettings Encryption { get; set; } = new();
}

public class EncryptionSettings
{
    public string Algorithm { get; set; } = "AES-256";
    public string KeySize { get; set; } = "256";
    public bool EnableEncryption { get; set; }
}

public class EnvironmentConfig
{
    public string Name { get; set; } = "";
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public ApiSettings? ApiSettings { get; set; }
    public CacheSettings? CacheSettings { get; set; }
    public SecuritySettings? SecuritySettings { get; set; }
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

public class ApplicationConfigurationDemo
{
    public static void DemonstrateAdvancedUsage()
    {
        // Create configuration with complex nested objects
        var config = ServiceConfiguration.CreateNew<ApplicationConfiguration>();
        
        // Set basic properties
        config.ApplicationName = "MyWebAPI";
        config.Version = "2.1.0";
        config.LogLevel = LogLevel.Information;
        
        // Configure API settings
        config.ApiSettings = new ApiSettings
        {
            BaseUrl = "https://api.example.com",
            TimeoutSeconds = 45,
            ApiKey = "secret-api-key",
            AllowedOrigins = new List<string> { "https://app.example.com", "https://admin.example.com" },
            EnableCors = true,
            RateLimit = new RateLimitSettings
            {
                RequestsPerMinute = 1000,
                BurstLimit = 2000,
                WindowSize = TimeSpan.FromMinutes(1)
            }
        };
        
        // Configure cache settings
        config.CacheSettings = new CacheSettings
        {
            Provider = "Redis",
            ConnectionString = "localhost:6379",
            DefaultExpiry = TimeSpan.FromHours(2),
            MaxSizeInMB = 500,
            EnableDistributedCache = true
        };
        
        // Configure security settings
        config.SecuritySettings = new SecuritySettings
        {
            JwtSecret = "super-secret-jwt-key",
            TokenExpiry = TimeSpan.FromHours(8),
            RequireHttps = true,
            AllowedHosts = new List<string> { "example.com", "*.example.com" },
            Encryption = new EncryptionSettings
            {
                Algorithm = "AES-256",
                KeySize = "256",
                EnableEncryption = true
            }
        };
        
        // Add environment configurations
        config.Environments = new List<EnvironmentConfig>
        {
            new EnvironmentConfig
            {
                Name = "Development",
                LogLevel = LogLevel.Debug,
                ApiSettings = new ApiSettings
                {
                    BaseUrl = "https://dev-api.example.com",
                    TimeoutSeconds = 30
                }
            },
            new EnvironmentConfig
            {
                Name = "Staging",
                LogLevel = LogLevel.Information,
                ApiSettings = new ApiSettings
                {
                    BaseUrl = "https://staging-api.example.com",
                    TimeoutSeconds = 45
                }
            },
            new EnvironmentConfig
            {
                Name = "Production",
                LogLevel = LogLevel.Warning,
                ApiSettings = new ApiSettings
                {
                    BaseUrl = "https://api.example.com",
                    TimeoutSeconds = 60
                }
            }
        };
        
        // Demonstrate environment switching
        Console.WriteLine($"Current Environment: {config.EnvironmentName}");
        Console.WriteLine($"API Base URL: {config.ApiSettings.BaseUrl}");
        
        // Switch to production
        config.ApplyEnvironmentSettings("Production");
        Console.WriteLine($"After switching to Production:");
        Console.WriteLine($"  Environment: {config.EnvironmentName}");
        Console.WriteLine($"  Log Level: {config.LogLevel}");
        Console.WriteLine($"  API Base URL: {config.ApiSettings.BaseUrl}");
        Console.WriteLine($"  Is Production: {config.IsProduction}");
        
        // Create environment-specific copy
        var devConfig = config.CreateEnvironmentCopy("Development");
        Console.WriteLine($"Development copy:");
        Console.WriteLine($"  Environment: {devConfig.EnvironmentName}");
        Console.WriteLine($"  Log Level: {devConfig.LogLevel}");
        Console.WriteLine($"  API Base URL: {devConfig.ApiSettings.BaseUrl}");
    }
}
```

### Property Change Notifications

```csharp
public class MonitoredConfiguration : ServiceConfiguration
{
    public string ServiceName
    {
        get => Get<string>() ?? "";
        set => Set(value);
    }
    
    public int PollingInterval
    {
        get => Get(5000); // Default 5 seconds
        set => Set(value);
    }
    
    public bool IsEnabled
    {
        get => Get<bool>();
        set => Set(value);
    }
    
    public List<string> Endpoints
    {
        get => Get<List<string>>() ?? new List<string>();
        set => Set(value);
    }
}

public class ConfigurationMonitor
{
    private readonly MonitoredConfiguration _config;
    private readonly List<string> _changeLog = new();
    
    public ConfigurationMonitor(MonitoredConfiguration config)
    {
        _config = config;
        
        // Subscribe to property change events
        _config.PropertyChanging += OnPropertyChanging;
        _config.PropertyChanged += OnPropertyChanged;
    }
    
    private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        _changeLog.Add($"[{timestamp}] Property '{e.PropertyName}' is about to change");
        
        Console.WriteLine($"🔄 Property '{e.PropertyName}' changing...");
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var newValue = GetPropertyValue(e.PropertyName);
        _changeLog.Add($"[{timestamp}] Property '{e.PropertyName}' changed to: {newValue}");
        
        Console.WriteLine($"✅ Property '{e.PropertyName}' changed to: {newValue}");
        
        // Trigger specific actions based on property changes
        HandlePropertyChange(e.PropertyName, newValue);
    }
    
    private object? GetPropertyValue(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return null;
        
        return propertyName switch
        {
            nameof(MonitoredConfiguration.ServiceName) => _config.ServiceName,
            nameof(MonitoredConfiguration.PollingInterval) => _config.PollingInterval,
            nameof(MonitoredConfiguration.IsEnabled) => _config.IsEnabled,
            nameof(MonitoredConfiguration.Endpoints) => string.Join(", ", _config.Endpoints),
            _ => null
        };
    }
    
    private void HandlePropertyChange(string? propertyName, object? newValue)
    {
        switch (propertyName)
        {
            case nameof(MonitoredConfiguration.IsEnabled):
                var isEnabled = (bool)(newValue ?? false);
                Console.WriteLine(isEnabled ? "🟢 Service monitoring enabled" : "🔴 Service monitoring disabled");
                break;
                
            case nameof(MonitoredConfiguration.PollingInterval):
                var interval = (int)(newValue ?? 0);
                if (interval < 1000)
                {
                    Console.WriteLine("⚠️  Warning: Polling interval is very short, may impact performance");
                }
                break;
                
            case nameof(MonitoredConfiguration.Endpoints):
                Console.WriteLine($"🔗 Endpoint configuration updated: {newValue}");
                break;
        }
    }
    
    public void PrintChangeLog()
    {
        Console.WriteLine("\n📋 Configuration Change Log:");
        foreach (var entry in _changeLog)
        {
            Console.WriteLine($"  {entry}");
        }
    }
    
    public void ClearChangeLog()
    {
        _changeLog.Clear();
        Console.WriteLine("🧹 Change log cleared");
    }
}

public class PropertyChangeDemo
{
    public static void DemonstratePropertyChangeNotifications()
    {
        // Create configuration and monitor
        var config = ServiceConfiguration.CreateNew<MonitoredConfiguration>();
        var monitor = new ConfigurationMonitor(config);
        
        Console.WriteLine("=== Property Change Notifications Demo ===\n");
        
        // Make various changes
        config.ServiceName = "UserService";
        Thread.Sleep(100);
        
        config.IsEnabled = true;
        Thread.Sleep(100);
        
        config.PollingInterval = 500; // This will trigger a warning
        Thread.Sleep(100);
        
        config.Endpoints = new List<string> { "https://api1.example.com", "https://api2.example.com" };
        Thread.Sleep(100);
        
        config.PollingInterval = 3000; // Change again
        Thread.Sleep(100);
        
        config.IsEnabled = false;
        Thread.Sleep(100);
        
        // Print change log
        monitor.PrintChangeLog();
    }
}
```

### JSON Serialization and Configuration Persistence

```csharp
public class PersistentConfiguration : ServiceConfiguration
{
    public string ApplicationId
    {
        get => Get<string>() ?? Guid.NewGuid().ToString();
        set => Set(value);
    }
    
    public DateTime CreatedAt
    {
        get => Get(DateTime.UtcNow);
        set => Set(value);
    }
    
    public DateTime LastModified
    {
        get => Get(DateTime.UtcNow);
        set => Set(value);
    }
    
    public Dictionary<string, object> CustomSettings
    {
        get => Get<Dictionary<string, object>>() ?? new Dictionary<string, object>();
        set => Set(value);
    }
    
    public UserPreferences UserPreferences
    {
        get => Get<UserPreferences>() ?? new UserPreferences();
        set => Set(value);
    }
    
    // Update last modified timestamp when any property changes
    protected override void Set<T>(T? value, [CallerMemberName] string? key = null)
    {
        base.Set(value, key);
        
        // Don't update LastModified when setting LastModified itself
        if (key != nameof(LastModified))
        {
            base.Set(DateTime.UtcNow, nameof(LastModified));
        }
    }
}

public class UserPreferences
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public bool EnableNotifications { get; set; } = true;
    public Dictionary<string, string> CustomPreferences { get; set; } = new();
}

public class ConfigurationPersistence
{
    private readonly string _filePath;
    
    public ConfigurationPersistence(string filePath)
    {
        _filePath = filePath;
    }
    
    public async Task<T> LoadConfigurationAsync<T>() where T : ServiceConfiguration, new()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var config = JsonConvert.DeserializeObject<T>(json);
                return config ?? ServiceConfiguration.CreateNew<T>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
        
        return ServiceConfiguration.CreateNew<T>();
    }
    
    public async Task SaveConfigurationAsync<T>(T configuration) where T : ServiceConfiguration
    {
        try
        {
            var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            await File.WriteAllTextAsync(_filePath, json);
            Console.WriteLine($"Configuration saved to: {_filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
            throw;
        }
    }
    
    public async Task<bool> BackupConfigurationAsync<T>(T configuration, string? backupSuffix = null) where T : ServiceConfiguration
    {
        try
        {
            var suffix = backupSuffix ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = $"{_filePath}.{suffix}.bak";
            
            var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            await File.WriteAllTextAsync(backupPath, json);
            
            Console.WriteLine($"Configuration backup saved to: {backupPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating backup: {ex.Message}");
            return false;
        }
    }
    
    public async Task<List<T>> LoadConfigurationHistoryAsync<T>(string backupPattern = "*.bak") where T : ServiceConfiguration, new()
    {
        var history = new List<T>();
        
        try
        {
            var directory = Path.GetDirectoryName(_filePath) ?? ".";
            var fileName = Path.GetFileNameWithoutExtension(_filePath);
            var searchPattern = $"{fileName}.{backupPattern}";
            
            var backupFiles = Directory.GetFiles(directory, searchPattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();
            
            foreach (var backupFile in backupFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(backupFile);
                    var config = JsonConvert.DeserializeObject<T>(json);
                    if (config != null)
                    {
                        history.Add(config);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading backup {backupFile}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration history: {ex.Message}");
        }
        
        return history;
    }
}

public class JsonSerializationDemo
{
    public static async Task DemonstrateJsonSerializationAsync()
    {
        var configPath = Path.Combine(Path.GetTempPath(), "app-config.json");
        var persistence = new ConfigurationPersistence(configPath);
        
        Console.WriteLine("=== JSON Serialization Demo ===\n");
        
        // Create and configure a complex configuration object
        var config = ServiceConfiguration.CreateNew<PersistentConfiguration>();
        config.ApplicationId = "demo-app-123";
        config.CreatedAt = DateTime.UtcNow.AddDays(-7); // Created 7 days ago
        
        config.CustomSettings = new Dictionary<string, object>
        {
            { "maxRetries", 5 },
            { "timeoutMs", 30000 },
            { "enableLogging", true },
            { "logLevel", "Information" },
            { "features", new List<string> { "feature1", "feature2", "feature3" } }
        };
        
        config.UserPreferences = new UserPreferences
        {
            Theme = "Dark",
            Language = "en-US",
            EnableNotifications = false,
            CustomPreferences = new Dictionary<string, string>
            {
                { "dateFormat", "yyyy-MM-dd" },
                { "timezone", "UTC" },
                { "pageSize", "25" }
            }
        };
        
        // Save configuration
        Console.WriteLine("💾 Saving configuration...");
        await persistence.SaveConfigurationAsync(config);
        
        // Create backup
        Console.WriteLine("\n📦 Creating backup...");
        await persistence.BackupConfigurationAsync(config, "demo");
        
        // Modify configuration
        config.UserPreferences.Theme = "Light";
        config.CustomSettings["maxRetries"] = 10;
        
        // Save modified version
        Console.WriteLine("\n💾 Saving modified configuration...");
        await persistence.SaveConfigurationAsync(config);
        
        // Load configuration back
        Console.WriteLine("\n📂 Loading configuration from file...");
        var loadedConfig = await persistence.LoadConfigurationAsync<PersistentConfiguration>();
        
        // Display loaded configuration
        Console.WriteLine($"Application ID: {loadedConfig.ApplicationId}");
        Console.WriteLine($"Created At: {loadedConfig.CreatedAt}");
        Console.WriteLine($"Last Modified: {loadedConfig.LastModified}");
        Console.WriteLine($"Theme: {loadedConfig.UserPreferences.Theme}");
        Console.WriteLine($"Max Retries: {loadedConfig.CustomSettings.GetValueOrDefault("maxRetries")}");
        
        // Load configuration history
        Console.WriteLine("\n📜 Loading configuration history...");
        var history = await persistence.LoadConfigurationHistoryAsync<PersistentConfiguration>();
        
        Console.WriteLine($"Found {history.Count} backup configurations:");
        foreach (var historicalConfig in history.Take(3))
        {
            Console.WriteLine($"  - Modified: {historicalConfig.LastModified}, Theme: {historicalConfig.UserPreferences.Theme}");
        }
        
        // Display JSON representation
        Console.WriteLine("\n📄 JSON representation:");
        var json = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
        Console.WriteLine(json);
        
        // Cleanup
        try
        {
            File.Delete(configPath);
            var backupFiles = Directory.GetFiles(Path.GetTempPath(), "app-config.json.*.bak");
            foreach (var backupFile in backupFiles)
            {
                File.Delete(backupFile);
            }
            Console.WriteLine("\n🧹 Cleanup completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup error: {ex.Message}");
        }
    }
}
```

### Configuration Comparison and Merging

```csharp
public static class ConfigurationExtensions
{
    /// <summary>
    /// Creates a detailed comparison between two configurations
    /// </summary>
    public static ConfigurationComparison Compare<T>(this T source, T target) where T : ServiceConfiguration
    {
        var comparison = new ConfigurationComparison();
        var sourceDict = (Dictionary<string, string>)source;
        var targetDict = (Dictionary<string, string>)target;
        
        // Find additions (in target but not in source)
        foreach (var kvp in targetDict)
        {
            if (!sourceDict.ContainsKey(kvp.Key))
            {
                comparison.Additions.Add(kvp.Key, kvp.Value);
            }
        }
        
        // Find deletions (in source but not in target)
        foreach (var kvp in sourceDict)
        {
            if (!targetDict.ContainsKey(kvp.Key))
            {
                comparison.Deletions.Add(kvp.Key, kvp.Value);
            }
        }
        
        // Find modifications (different values for same key)
        foreach (var kvp in sourceDict)
        {
            if (targetDict.TryGetValue(kvp.Key, out var targetValue) && targetValue != kvp.Value)
            {
                comparison.Modifications.Add(kvp.Key, new ValueChange(kvp.Value, targetValue));
            }
        }
        
        return comparison;
    }
    
    /// <summary>
    /// Merges configurations with conflict resolution
    /// </summary>
    public static T Merge<T>(this T source, T other, MergeStrategy strategy = MergeStrategy.PreferOther) where T : ServiceConfiguration, new()
    {
        var result = ServiceConfiguration.CreateNew<T>(source);
        var sourceDict = (Dictionary<string, string>)source;
        var otherDict = (Dictionary<string, string>)other;
        var resultDict = (Dictionary<string, string>)result;
        
        foreach (var kvp in otherDict)
        {
            if (sourceDict.ContainsKey(kvp.Key))
            {
                // Handle conflicts based on strategy
                resultDict[kvp.Key] = strategy switch
                {
                    MergeStrategy.PreferSource => sourceDict[kvp.Key],
                    MergeStrategy.PreferOther => kvp.Value,
                    MergeStrategy.PreferNewer => DetermineNewer(sourceDict[kvp.Key], kvp.Value),
                    _ => kvp.Value
                };
            }
            else
            {
                // No conflict, add new property
                resultDict[kvp.Key] = kvp.Value;
            }
        }
        
        return ServiceConfiguration.CreateNew<T>(resultDict);
    }
    
    private static string DetermineNewer(string sourceValue, string otherValue)
    {
        // Simple heuristic: prefer non-empty values, or return the "other" value
        return string.IsNullOrWhiteSpace(sourceValue) ? otherValue : sourceValue;
    }
}

public class ConfigurationComparison
{
    public Dictionary<string, string> Additions { get; set; } = new();
    public Dictionary<string, string> Deletions { get; set; } = new();
    public Dictionary<string, ValueChange> Modifications { get; set; } = new();
    
    public bool HasChanges => Additions.Count > 0 || Deletions.Count > 0 || Modifications.Count > 0;
    
    public void PrintSummary()
    {
        Console.WriteLine($"Configuration Comparison Summary:");
        Console.WriteLine($"  Additions: {Additions.Count}");
        Console.WriteLine($"  Deletions: {Deletions.Count}");
        Console.WriteLine($"  Modifications: {Modifications.Count}");
        Console.WriteLine($"  Has Changes: {HasChanges}");
    }
    
    public void PrintDetails()
    {
        if (Additions.Count > 0)
        {
            Console.WriteLine("➕ Additions:");
            foreach (var kvp in Additions)
            {
                Console.WriteLine($"  + {kvp.Key}: {kvp.Value}");
            }
        }
        
        if (Deletions.Count > 0)
        {
            Console.WriteLine("➖ Deletions:");
            foreach (var kvp in Deletions)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
            }
        }
        
        if (Modifications.Count > 0)
        {
            Console.WriteLine("🔄 Modifications:");
            foreach (var kvp in Modifications)
            {
                Console.WriteLine($"  ~ {kvp.Key}: {kvp.Value.OldValue} → {kvp.Value.NewValue}");
            }
        }
    }
}

public record ValueChange(string OldValue, string NewValue);

public enum MergeStrategy
{
    PreferSource,
    PreferOther,
    PreferNewer
}

public class ConfigurationComparisonDemo
{
    public static void DemonstrateConfigurationComparison()
    {
        Console.WriteLine("=== Configuration Comparison Demo ===\n");
        
        // Create initial configuration
        var config1 = ServiceConfiguration.CreateNew<DatabaseConfiguration>();
        config1.ConnectionString = "Server=localhost;Database=DB1;";
        config1.ConnectionTimeout = 30;
        config1.EnableRetry = true;
        config1.DatabaseType = DatabaseType.SqlServer;
        
        // Create modified configuration
        var config2 = ServiceConfiguration.CreateNew<DatabaseConfiguration>();
        config2.ConnectionString = "Server=remotehost;Database=DB1;"; // Modified
        config2.ConnectionTimeout = 60; // Modified
        config2.EnableRetry = true; // Same
        config2.RetryInterval = TimeSpan.FromSeconds(10); // Added
        config2.DatabaseType = DatabaseType.PostgreSQL; // Modified
        
        // Compare configurations
        var comparison = config1.Compare(config2);
        
        Console.WriteLine("📊 Configuration Comparison:");
        comparison.PrintSummary();
        Console.WriteLine();
        comparison.PrintDetails();
        
        // Demonstrate merging with different strategies
        Console.WriteLine("\n🔄 Merging Configurations:\n");
        
        // Prefer source
        var merged1 = config1.Merge(config2, MergeStrategy.PreferSource);
        Console.WriteLine("Merge Strategy: Prefer Source");
        Console.WriteLine($"  Connection Timeout: {merged1.ConnectionTimeout}"); // Should be 30
        Console.WriteLine($"  Database Type: {merged1.DatabaseType}"); // Should be SqlServer
        
        // Prefer other
        var merged2 = config1.Merge(config2, MergeStrategy.PreferOther);
        Console.WriteLine("\nMerge Strategy: Prefer Other");
        Console.WriteLine($"  Connection Timeout: {merged2.ConnectionTimeout}"); // Should be 60
        Console.WriteLine($"  Database Type: {merged2.DatabaseType}"); // Should be PostgreSQL
        Console.WriteLine($"  Retry Interval: {merged2.RetryInterval}"); // Should be set
        
        // Compare merged result with original
        var mergeComparison = config1.Compare(merged2);
        Console.WriteLine("\n📈 Changes after merge:");
        mergeComparison.PrintDetails();
    }
}
```

## Performance Considerations

### Thread Safety
- Uses `ConcurrentDictionary<string, string>` for thread-safe property storage
- Atomic operations for get/set operations
- Safe for multi-threaded environments without additional locking

### Memory Efficiency
- String-based storage reduces memory overhead
- Lazy deserialization of complex objects
- Efficient property access patterns

### Serialization Performance
- Custom JSON converter optimizes serialization
- Automatic camelCase/PascalCase conversion
- Minimal reflection usage for better performance

## Best Practices

### 1. **Property Design Patterns**

```csharp
public class BestPracticeConfiguration : ServiceConfiguration
{
    // Good: Use descriptive default values
    public int ConnectionTimeout
    {
        get => Get(30); // Clear default
        set => Set(value);
    }
    
    // Good: Validate values in setters
    public string ServiceUrl
    {
        get => Get<string>() ?? "";
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && !Uri.IsWellFormedUriString(value, UriKind.Absolute))
                throw new ArgumentException("Invalid URL format", nameof(value));
            Set(value);
        }
    }
    
    // Good: Use computed properties for derived values
    public bool IsSecureConnection => ServiceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    
    // Good: Provide validation methods
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ServiceUrl) &&
               ConnectionTimeout > 0 &&
               Uri.IsWellFormedUriString(ServiceUrl, UriKind.Absolute);
    }
}
```

### 2. **Error Handling**

```csharp
public class RobustConfiguration : ServiceConfiguration
{
    public T SafeGet<T>(T defaultValue, [CallerMemberName] string? key = null)
    {
        try
        {
            return Get(defaultValue, key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting property {key}: {ex.Message}");
            return defaultValue;
        }
    }
    
    public bool TrySet<T>(T value, [CallerMemberName] string? key = null)
    {
        try
        {
            Set(value, key);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting property {key}: {ex.Message}");
            return false;
        }
    }
}
```

### 3. **Configuration Validation**

```csharp
public abstract class ValidatedConfiguration : ServiceConfiguration
{
    public abstract ValidationResult Validate();
    
    protected ValidationResult CreateValidationResult()
    {
        return new ValidationResult();
    }
    
    protected void ValidateRequired<T>(T value, string propertyName, ValidationResult result)
    {
        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            result.Errors.Add($"{propertyName} is required");
        }
    }
    
    protected void ValidateRange<T>(T value, T min, T max, string propertyName, ValidationResult result)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            result.Errors.Add($"{propertyName} must be between {min} and {max}");
        }
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class ServiceConfigurationTests
{
    private class TestConfiguration : ServiceConfiguration
    {
        public string StringProperty
        {
            get => Get<string>() ?? "";
            set => Set(value);
        }
        
        public int IntProperty
        {
            get => Get<int>();
            set => Set(value);
        }
        
        public List<string> ListProperty
        {
            get => Get<List<string>>() ?? new List<string>();
            set => Set(value);
        }
    }
    
    [Test]
    public void Set_And_Get_StringProperty_Success()
    {
        // Arrange
        var config = ServiceConfiguration.CreateNew<TestConfiguration>();
        var value = "test value";
        
        // Act
        config.StringProperty = value;
        var result = config.StringProperty;
        
        // Assert
        Assert.That(result, Is.EqualTo(value));
    }
    
    [Test]
    public void Set_And_Get_ComplexProperty_Success()
    {
        // Arrange
        var config = ServiceConfiguration.CreateNew<TestConfiguration>();
        var list = new List<string> { "item1", "item2", "item3" };
        
        // Act
        config.ListProperty = list;
        var result = config.ListProperty;
        
        // Assert
        Assert.That(result, Is.EquivalentTo(list));
    }
    
    [Test]
    public void PropertyChanged_Event_Fired_When_Property_Changes()
    {
        // Arrange
        var config = ServiceConfiguration.CreateNew<TestConfiguration>();
        var eventFired = false;
        string? changedProperty = null;
        
        config.PropertyChanged += (sender, args) =>
        {
            eventFired = true;
            changedProperty = args.PropertyName;
        };
        
        // Act
        config.StringProperty = "test";
        
        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(changedProperty, Is.EqualTo(nameof(TestConfiguration.StringProperty)));
    }
    
    [Test]
    public void JsonSerialization_RoundTrip_PreservesData()
    {
        // Arrange
        var config = ServiceConfiguration.CreateNew<TestConfiguration>();
        config.StringProperty = "test";
        config.IntProperty = 42;
        config.ListProperty = new List<string> { "a", "b", "c" };
        
        // Act
        var json = JsonConvert.SerializeObject(config);
        var deserializedConfig = JsonConvert.DeserializeObject<TestConfiguration>(json);
        
        // Assert
        Assert.That(deserializedConfig, Is.Not.Null);
        Assert.That(deserializedConfig.StringProperty, Is.EqualTo(config.StringProperty));
        Assert.That(deserializedConfig.IntProperty, Is.EqualTo(config.IntProperty));
        Assert.That(deserializedConfig.ListProperty, Is.EquivalentTo(config.ListProperty));
    }
    
    [Test]
    public void Equals_SameProperties_ReturnsTrue()
    {
        // Arrange
        var config1 = ServiceConfiguration.CreateNew<TestConfiguration>();
        var config2 = ServiceConfiguration.CreateNew<TestConfiguration>();
        
        config1.StringProperty = "test";
        config1.IntProperty = 42;
        
        config2.StringProperty = "test";
        config2.IntProperty = 42;
        
        // Act & Assert
        Assert.That(config1.Equals(config2), Is.True);
    }
}
```

## See Also

- [INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged) - Property change notification interface
- [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) - Thread-safe dictionary implementation
- [JsonConverter](https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm) - Custom JSON serialization
- [CallerMemberName](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute) - Automatic property name resolution
- [Configuration Patterns](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) - .NET configuration best practices

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing a flexible, thread-safe foundation for configuration management with JSON serialization and property change notifications.*