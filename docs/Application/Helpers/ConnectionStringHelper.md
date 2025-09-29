# ConnectionStringHelper

The `ConnectionStringHelper` class is a static utility class in the RapidStreamer BuildingBlocks that provides functionality for enriching connection strings with environment variables. It enables secure configuration management by replacing environment variable placeholders in connection strings with actual environment variable values at runtime.

## Purpose

This helper serves as:
- A connection string enrichment utility for secure configuration management
- An environment variable resolver for database and service connections
- A configuration security tool that keeps sensitive values out of source code
- A runtime configuration provider that supports dynamic environment-based setup
- A bridge between configuration templates and actual runtime values

## Key Features

- **Environment Variable Resolution**: Automatically resolves `$VAR$` placeholders with environment variable values
- **Secure Configuration**: Keeps sensitive connection information out of source code and configuration files
- **Runtime Flexibility**: Allows different configurations for different environments (dev, staging, production)
- **Error Handling**: Throws exceptions for missing or empty environment variables
- **Template Support**: Supports connection string templates with multiple environment variable placeholders

## Method

### EnrichConnectionString
Processes a connection string template and replaces environment variable placeholders with actual values.

```csharp
public static string EnrichConnectionString(string connectionString)
```

**Key Features:**
- Identifies environment variable placeholders using `$VARIABLE$` syntax
- Resolves each placeholder by looking up the corresponding environment variable
- Throws `ArgumentException` if any required environment variable is missing or empty
- Returns the fully enriched connection string ready for use

**Process Flow:**
1. Parse connection string to find environment variable keys using `GetEnvironmentKeys()`
2. For each key, extract the variable name (removing the `$` delimiters)
3. Look up the environment variable value using `Environment.GetEnvironmentVariable()`
4. Validate that the environment variable exists and is not empty
5. Replace the placeholder with the actual value in the connection string

## Usage Examples

### Database Connection String Enrichment

```csharp
// Set environment variables (typically done in deployment configuration)
Environment.SetEnvironmentVariable("DB_SERVER", "production-sql-server.domain.com");
Environment.SetEnvironmentVariable("DB_NAME", "ProductionDatabase");
Environment.SetEnvironmentVariable("DB_USER", "app_user");
Environment.SetEnvironmentVariable("DB_PASSWORD", "SecurePassword123!");

// Connection string template with placeholders
string connectionTemplate = "Server=$DB_SERVER$;Database=$DB_NAME$;User Id=$DB_USER$;Password=$DB_PASSWORD$;TrustServerCertificate=true;";

// Enrich the connection string
string actualConnectionString = ConnectionStringHelper.EnrichConnectionString(connectionTemplate);

// Result: "Server=production-sql-server.domain.com;Database=ProductionDatabase;User Id=app_user;Password=SecurePassword123!;TrustServerCertificate=true;"

Console.WriteLine("Enriched connection string ready for use");
```

### Redis Connection Configuration

```csharp
// Environment variables for Redis configuration
Environment.SetEnvironmentVariable("REDIS_HOST", "redis-cluster.internal");
Environment.SetEnvironmentVariable("REDIS_PORT", "6379");
Environment.SetEnvironmentVariable("REDIS_PASSWORD", "RedisSecretKey");

// Redis connection template
string redisTemplate = "$REDIS_HOST$:$REDIS_PORT$,password=$REDIS_PASSWORD$";

// Enrich for actual connection
string redisConnection = ConnectionStringHelper.EnrichConnectionString(redisTemplate);

// Result: "redis-cluster.internal:6379,password=RedisSecretKey"

// Use with Redis client
var redis = ConnectionMultiplexer.Connect(redisConnection);
```

### Multi-Environment Configuration

```csharp
public class DatabaseConfiguration
{
    private readonly string _environment;
    
    public DatabaseConfiguration(string environment)
    {
        _environment = environment;
    }
    
    public string GetConnectionString()
    {
        // Environment-specific templates
        string template = _environment.ToLower() switch
        {
            "development" => "Server=localhost;Database=DevDB;Integrated Security=true;",
            "staging" => "Server=$STAGE_DB_SERVER$;Database=$STAGE_DB_NAME$;User Id=$STAGE_DB_USER$;Password=$STAGE_DB_PASSWORD$;",
            "production" => "Server=$PROD_DB_SERVER$;Database=$PROD_DB_NAME$;User Id=$PROD_DB_USER$;Password=$PROD_DB_PASSWORD$;Encrypt=true;",
            _ => throw new InvalidOperationException($"Unknown environment: {_environment}")
        };
        
        // Enrich only if environment variables are present
        return template.Contains('$') 
            ? ConnectionStringHelper.EnrichConnectionString(template)
            : template;
    }
}

// Usage
var dbConfig = new DatabaseConfiguration("production");
string connectionString = dbConfig.GetConnectionString();
```

### Microservices Configuration

```csharp
public class ServiceConfiguration
{
    public string DatabaseConnection { get; set; } = string.Empty;
    public string RedisConnection { get; set; } = string.Empty;
    public string MessageBusConnection { get; set; } = string.Empty;
    
    public static ServiceConfiguration CreateFromEnvironment()
    {
        return new ServiceConfiguration
        {
            DatabaseConnection = ConnectionStringHelper.EnrichConnectionString(
                "Server=$DB_HOST$;Database=$DB_NAME$;User Id=$DB_USER$;Password=$DB_PASSWORD$;"),
                
            RedisConnection = ConnectionStringHelper.EnrichConnectionString(
                "$REDIS_HOST$:$REDIS_PORT$,password=$REDIS_PASSWORD$"),
                
            MessageBusConnection = ConnectionStringHelper.EnrichConnectionString(
                "amqp://$RABBITMQ_USER$:$RABBITMQ_PASSWORD$@$RABBITMQ_HOST$:$RABBITMQ_PORT$/")
        };
    }
}

// Set environment variables through deployment configuration
// DB_HOST=prod-db.internal
// DB_NAME=ServiceDatabase
// DB_USER=service_account
// DB_PASSWORD=SecureDbPassword
// REDIS_HOST=redis.internal
// REDIS_PORT=6379
// REDIS_PASSWORD=RedisPassword
// RABBITMQ_HOST=messagebus.internal
// RABBITMQ_PORT=5672
// RABBITMQ_USER=service_user
// RABBITMQ_PASSWORD=BusPassword

var config = ServiceConfiguration.CreateFromEnvironment();
```

## Real-World Applications

### ASP.NET Core Configuration Integration

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Get connection string template from appsettings.json
        string connectionTemplate = Configuration.GetConnectionString("DefaultConnection");
        
        // Enrich with environment variables
        string enrichedConnection = ConnectionStringHelper.EnrichConnectionString(connectionTemplate);
        
        // Register with DI container
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(enrichedConnection));
    }
}

// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=$DB_SERVER$;Database=$DB_NAME$;User Id=$DB_USER$;Password=$DB_PASSWORD$;TrustServerCertificate=true;"
  }
}
```

### Docker Container Configuration

```csharp
public class ContainerizedApplication
{
    public async Task StartAsync()
    {
        try
        {
            // Connection strings that will be resolved from Docker environment variables
            var connectionStrings = new Dictionary<string, string>
            {
                ["Database"] = ConnectionStringHelper.EnrichConnectionString(
                    "Server=$DATABASE_HOST$;Database=$DATABASE_NAME$;User Id=$DATABASE_USER$;Password=$DATABASE_PASSWORD$;"),
                    
                ["Cache"] = ConnectionStringHelper.EnrichConnectionString(
                    "$CACHE_HOST$:$CACHE_PORT$,password=$CACHE_PASSWORD$"),
                    
                ["Storage"] = ConnectionStringHelper.EnrichConnectionString(
                    "DefaultEndpointsProtocol=https;AccountName=$STORAGE_ACCOUNT$;AccountKey=$STORAGE_KEY$;EndpointSuffix=core.windows.net")
            };
            
            // Initialize services with enriched connection strings
            await InitializeServicesAsync(connectionStrings);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Configuration error: {ex.Message}");
            Console.WriteLine("Please ensure all required environment variables are set.");
            throw;
        }
    }
}

// Docker run command or docker-compose.yml would set these:
// -e DATABASE_HOST=db-server
// -e DATABASE_NAME=appdb
// -e DATABASE_USER=appuser
// -e DATABASE_PASSWORD=secretpassword
// -e CACHE_HOST=redis-server
// -e CACHE_PORT=6379
// -e CACHE_PASSWORD=redispassword
```

### Kubernetes Configuration Management

```csharp
public class KubernetesConfigurationManager
{
    public ServiceConnections LoadConnections()
    {
        try
        {
            // These will be set by Kubernetes secrets and configmaps
            return new ServiceConnections
            {
                PrimaryDatabase = ConnectionStringHelper.EnrichConnectionString(
                    "Host=$POSTGRES_HOST$;Database=$POSTGRES_DB$;Username=$POSTGRES_USER$;Password=$POSTGRES_PASSWORD$;"),
                    
                ReadReplica = ConnectionStringHelper.EnrichConnectionString(
                    "Host=$POSTGRES_READ_HOST$;Database=$POSTGRES_DB$;Username=$POSTGRES_READ_USER$;Password=$POSTGRES_READ_PASSWORD$;"),
                    
                EventStore = ConnectionStringHelper.EnrichConnectionString(
                    "esdb://$EVENTSTORE_USER$:$EVENTSTORE_PASSWORD$@$EVENTSTORE_HOST$:$EVENTSTORE_PORT$?tls=false"),
                    
                MessageQueue = ConnectionStringHelper.EnrichConnectionString(
                    "$KAFKA_BROKERS$")
            };
        }
        catch (ArgumentException ex)
        {
            throw new ConfigurationException($"Missing required environment variable: {ex.Message}", ex);
        }
    }
}

// Kubernetes deployment would include:
// env:
//   - name: POSTGRES_HOST
//     valueFrom:
//       secretKeyRef:
//         name: database-secret
//         key: host
//   - name: POSTGRES_PASSWORD
//     valueFrom:
//       secretKeyRef:
//         name: database-secret
//         key: password
```

## Integration with EnvironmentHelper

The `ConnectionStringHelper` relies on the `EnvironmentHelper` for parsing environment variable placeholders:

```csharp
// EnvironmentHelper identifies $VAR$ patterns
var environmentKeys = connectionString.GetEnvironmentKeys();

// ConnectionStringHelper resolves and replaces them
foreach (var key in environmentKeys)
{
    var variableName = key.Replace("$", "");
    var value = Environment.GetEnvironmentVariable(variableName);
    // Replace placeholder with actual value
}
```

## Security Considerations

### Best Practices

```csharp
public class SecureConnectionManager
{
    public string GetSecureConnection(string template)
    {
        try
        {
            // Validate template format
            if (!template.Contains('$'))
            {
                throw new ArgumentException("Connection template must contain environment variable placeholders");
            }
            
            // Enrich connection string
            string enriched = ConnectionStringHelper.EnrichConnectionString(template);
            
            // Log success without exposing sensitive data
            Console.WriteLine("Connection string successfully enriched");
            
            return enriched;
        }
        catch (ArgumentException ex)
        {
            // Log error without exposing template content
            Console.WriteLine($"Failed to enrich connection string: Missing environment variable");
            throw new ConfigurationException("Required configuration is missing", ex);
        }
    }
    
    public void ValidateEnvironmentConfiguration(string[] requiredVariables)
    {
        var missing = requiredVariables
            .Where(var => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(var)))
            .ToList();
            
        if (missing.Any())
        {
            throw new ConfigurationException($"Missing required environment variables: {string.Join(", ", missing)}");
        }
    }
}
```

## Error Handling

### Robust Error Management

```csharp
public class ConnectionStringManager
{
    public string SafeEnrichConnectionString(string template, Dictionary<string, string>? fallbackValues = null)
    {
        try
        {
            return ConnectionStringHelper.EnrichConnectionString(template);
        }
        catch (ArgumentException ex) when (fallbackValues != null)
        {
            // Try using fallback values
            Console.WriteLine($"Environment variable missing, using fallback: {ex.Message}");
            return EnrichWithFallback(template, fallbackValues);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Failed to enrich connection string - missing environment variables");
            throw new InvalidOperationException("Application configuration is incomplete. Please check environment variables.");
        }
    }
    
    private string EnrichWithFallback(string template, Dictionary<string, string> fallbackValues)
    {
        string result = template;
        
        foreach (var (key, value) in fallbackValues)
        {
            result = result.Replace($"${key}$", value);
        }
        
        return result;
    }
}
```

## Testing Strategies

```csharp
[Test]
public void EnrichConnectionString_WithValidEnvironmentVariables_ReturnsEnrichedString()
{
    // Arrange
    Environment.SetEnvironmentVariable("TEST_SERVER", "testserver");
    Environment.SetEnvironmentVariable("TEST_DB", "testdb");
    string template = "Server=$TEST_SERVER$;Database=$TEST_DB$;";
    
    // Act
    string result = ConnectionStringHelper.EnrichConnectionString(template);
    
    // Assert
    Assert.Equal("Server=testserver;Database=testdb;", result);
    
    // Cleanup
    Environment.SetEnvironmentVariable("TEST_SERVER", null);
    Environment.SetEnvironmentVariable("TEST_DB", null);
}

[Test]
public void EnrichConnectionString_WithMissingEnvironmentVariable_ThrowsArgumentException()
{
    // Arrange
    string template = "Server=$MISSING_SERVER$;Database=test;";
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => 
        ConnectionStringHelper.EnrichConnectionString(template));
}
```

## Performance Considerations

- **Single Pass Processing**: Efficiently processes connection strings in a single iteration
- **Memory Efficient**: Uses string replacement without creating unnecessary intermediate objects
- **Validation Early**: Fails fast if required environment variables are missing
- **Minimal Overhead**: Simple string operations with minimal computational overhead

## Thread Safety

- **Static Method**: Thread-safe as it's a stateless static method
- **Environment Variables**: Reading environment variables is thread-safe
- **Immutable Operations**: String operations create new instances without modifying original input

## Best Practices

1. **Environment Variable Naming**: Use consistent, descriptive names for environment variables
2. **Template Validation**: Validate connection string templates during application startup
3. **Error Handling**: Implement proper exception handling for missing variables
4. **Security**: Never log enriched connection strings as they contain sensitive information
5. **Documentation**: Document required environment variables for each deployment environment

## Related Components

- **[EnvironmentHelper](EnvironmentHelper.md)**: Provides `GetEnvironmentKeys()` method for parsing placeholders
- **[ServiceConfiguration](../ServiceConfiguration.md)**: Used in conjunction with application configuration management
- **[Identity System](../Identity/README.md)**: Part of secure configuration management practices

The `ConnectionStringHelper` provides a secure and flexible way to manage connection strings across different environments, ensuring sensitive configuration data remains secure while maintaining deployment flexibility.