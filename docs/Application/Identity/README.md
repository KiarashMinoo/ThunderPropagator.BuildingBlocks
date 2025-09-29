# Identity System Overview

The `RapidStreamer.BuildingBlocks.Application.Identity` namespace provides comprehensive authentication and authorization infrastructure for .NET applications. It offers abstract base classes for user authentication configuration and JWT-based security systems with built-in serialization support and integration capabilities.

## Architecture Overview

The Identity system is built around two core abstract configurations that provide the foundation for authentication and authorization:

```
Identity Namespace
├── BasicUserConfiguration    - User authentication with role-based access control
├── JwtConfiguration          - JWT token validation and security parameters
└── Integration Points        - Seamless integration with authentication systems
```

## Core Components

### [BasicUserConfiguration](BasicUserConfiguration.md)
Abstract base class for user authentication configuration.

**Key Features:**
- Username/password authentication foundation
- Role-based access control (RBAC) support
- Dual serialization (System.Text.Json & Newtonsoft.Json)
- Username-based equality comparison
- Extensible design for custom authentication scenarios

**Use Cases:**
- User credential management
- Role-based authorization systems
- Multi-tenant user configurations
- Integration with Identity providers

### [JwtConfiguration](JwtConfiguration.md)
Abstract base class for JWT authentication configuration.

**Key Features:**
- Comprehensive JWT validation parameters
- Issuer and audience validation
- Signing key management
- Configurable security settings
- Production-ready security defaults

**Use Cases:**
- Token-based authentication
- API security configuration
- Microservices authentication
- Single sign-on (SSO) systems

## Design Principles

### 1. Abstract Foundation Pattern
Both configuration classes are abstract, providing base functionality while allowing concrete implementations:

```csharp
// Abstract base provides common functionality
public abstract class BasicUserConfiguration : EquatableObject<BasicUserConfiguration>
{
    [JsonProperty, JsonInclude] public string Username { get; protected set; } = null!;
    // ... other properties
}

// Concrete implementation adds specific behavior
public class AppUserConfiguration : BasicUserConfiguration
{
    public AppUserConfiguration(string username, string password, params string[] roles)
    {
        Username = username;
        Password = password;
        Roles = roles;
    }
    
    public bool HasRole(string role) => Roles?.Contains(role) ?? false;
}
```

### 2. Security-First Design
Security considerations are built into the core design:

```csharp
// Protected setters prevent unauthorized modification
[JsonProperty, JsonInclude] public string Password { get; protected set; } = null!;

// Default secure configurations
public ProductionJwtConfiguration()
{
    ValidateLifetime = true;          // Always validate token expiration
    ValidateAudience = true;          // Validate intended recipient
    ValidateIssuer = true;            // Validate trusted source
    ValidateIssuerSigningKey = true;  // Always validate signature
}
```

### 3. Dual Serialization Support
Both System.Text.Json and Newtonsoft.Json serialization:

```csharp
[JsonProperty, JsonInclude] public string Username { get; protected set; } = null!;
//     ↑             ↑
// Newtonsoft    System.Text.Json
```

### 4. Value-Based Equality
Inheriting from `EquatableObject<T>` provides proper equality semantics:

```csharp
// Username-based equality for user configurations
public override int GetHashCode() => Username.GetHashCode();

// Two users with same username are considered equal
var user1 = new AppUserConfiguration("john", "password1", "User");
var user2 = new AppUserConfiguration("john", "password2", "Admin");
Assert.That(user1, Is.EqualTo(user2)); // True - same username
```

## Common Usage Patterns

### 1. Environment-Specific Configuration

```csharp
public class ConfigurationFactory
{
    public static (BasicUserConfiguration User, JwtConfiguration Jwt) CreateForEnvironment(string env)
    {
        return env.ToLower() switch
        {
            "development" => (CreateDevUser(), CreateDevJwt()),
            "staging" => (CreateStagingUser(), CreateStagingJwt()),
            "production" => (CreateProdUser(), CreateProdJwt()),
            _ => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
    
    private static AppUserConfiguration CreateDevUser()
    {
        return new AppUserConfiguration("dev_admin", "dev_password", "Admin", "Developer");
    }
    
    private static DevJwtConfiguration CreateDevJwt()
    {
        return new DevJwtConfiguration
        {
            IssuerSigningKey = "development-signing-key-that-is-long-enough",
            ValidIssuer = "dev.myapp.com",
            ValidAudience = "dev.api.myapp.com",
            ValidateLifetime = false // Relaxed for development
        };
    }
}
```

### 2. Multi-Tenant Authentication

```csharp
public class MultiTenantAuthSystem
{
    private readonly Dictionary<string, BasicUserConfiguration> _tenantUsers = new();
    private readonly Dictionary<string, JwtConfiguration> _tenantJwtConfigs = new();
    
    public void ConfigureTenant(string tenantId, BasicUserConfiguration userConfig, JwtConfiguration jwtConfig)
    {
        _tenantUsers[tenantId] = userConfig;
        _tenantJwtConfigs[tenantId] = jwtConfig;
    }
    
    public bool AuthenticateUser(string tenantId, string username, string password)
    {
        if (!_tenantUsers.TryGetValue(tenantId, out var userConfig))
            return false;
        
        return userConfig.Username == username && VerifyPassword(userConfig, password);
    }
    
    public string? GenerateToken(string tenantId, BasicUserConfiguration user)
    {
        if (!_tenantJwtConfigs.TryGetValue(tenantId, out var jwtConfig))
            return null;
        
        return GenerateJwtToken(user, jwtConfig);
    }
}
```

### 3. Role-Based Authorization Pipeline

```csharp
public class AuthorizationPipeline
{
    public async Task<bool> AuthorizeAsync(BasicUserConfiguration user, string resource, string action)
    {
        // Get required roles for the resource/action
        var requiredRoles = await GetRequiredRolesAsync(resource, action);
        
        // Check if user has any of the required roles
        return user.Roles?.Any(role => requiredRoles.Contains(role)) ?? false;
    }
    
    public ClaimsPrincipal CreatePrincipal(BasicUserConfiguration user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Username)
        };
        
        if (user.Roles != null)
        {
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "BasicAuth"));
    }
}
```

## Integration Scenarios

### ASP.NET Core Integration

```csharp
public class IdentityIntegrationExtensions
{
    public static IServiceCollection AddBuildingBlocksIdentity(
        this IServiceCollection services,
        BasicUserConfiguration userConfig,
        JwtConfiguration jwtConfig)
    {
        // Register configurations
        services.AddSingleton(userConfig);
        services.AddSingleton(jwtConfig);
        
        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtConfig.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.IssuerSigningKey)),
                    ValidateIssuer = jwtConfig.ValidateIssuer,
                    ValidIssuer = jwtConfig.ValidIssuer,
                    ValidateAudience = jwtConfig.ValidateAudience,
                    ValidAudience = jwtConfig.ValidAudience,
                    ValidateLifetime = jwtConfig.ValidateLifetime
                };
            });
        
        // Configure authorization
        services.AddAuthorization(options =>
        {
            if (userConfig.Roles != null)
            {
                foreach (string role in userConfig.Roles)
                {
                    options.AddPolicy($"Require{role}Role", 
                        policy => policy.RequireRole(role));
                }
            }
        });
        
        return services;
    }
}
```

### Configuration Management

```csharp
public class IdentityConfigurationManager
{
    public async Task<(BasicUserConfiguration User, JwtConfiguration Jwt)> LoadConfigurationAsync(string configPath)
    {
        var configData = await File.ReadAllTextAsync(configPath);
        
        // Support multiple formats
        if (configPath.EndsWith(".json"))
        {
            return LoadFromJson(configData);
        }
        else if (configPath.EndsWith(".yml") || configPath.EndsWith(".yaml"))
        {
            return LoadFromYaml(configData);
        }
        
        throw new NotSupportedException($"Configuration format not supported: {configPath}");
    }
    
    private (BasicUserConfiguration User, JwtConfiguration Jwt) LoadFromJson(string json)
    {
        var config = json.FromJson<IdentityConfiguration>();
        return (config.User, config.Jwt);
    }
    
    private (BasicUserConfiguration User, JwtConfiguration Jwt) LoadFromYaml(string yaml)
    {
        var config = yaml.FromYaml<IdentityConfiguration>();
        return (config.User, config.Jwt);
    }
    
    public async Task SaveConfigurationAsync(
        string configPath,
        BasicUserConfiguration userConfig,
        JwtConfiguration jwtConfig)
    {
        var config = new IdentityConfiguration { User = userConfig, Jwt = jwtConfig };
        
        string content = configPath.EndsWith(".yml") || configPath.EndsWith(".yaml")
            ? config.ToYaml()
            : config.ToJson();
        
        await File.WriteAllTextAsync(configPath, content);
    }
}

public class IdentityConfiguration
{
    public BasicUserConfiguration User { get; set; } = null!;
    public JwtConfiguration Jwt { get; set; } = null!;
}
```

## Security Best Practices

### 1. Password Security

```csharp
public class SecureUserConfiguration : BasicUserConfiguration
{
    public SecureUserConfiguration(string username, string plainPassword, params string[] roles)
    {
        Username = username;
        Password = HashPassword(plainPassword); // Always hash passwords
        Roles = roles;
    }
    
    public bool VerifyPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, Password);
    }
    
    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12); // Use strong work factor
    }
}
```

### 2. JWT Security

```csharp
public class SecureJwtConfiguration : JwtConfiguration
{
    public SecureJwtConfiguration()
    {
        // Secure defaults
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
    
    public void ValidateSecurityRequirements()
    {
        if (IssuerSigningKey?.Length < 32)
            throw new SecurityException("Signing key must be at least 32 characters");
        
        if (!ValidateIssuerSigningKey)
            throw new SecurityException("Signature validation must be enabled");
        
        if (string.IsNullOrEmpty(ValidIssuer))
            throw new SecurityException("Valid issuer must be specified");
        
        if (string.IsNullOrEmpty(ValidAudience))
            throw new SecurityException("Valid audience must be specified");
    }
}
```

### 3. Environment Variable Security

```csharp
public class EnvironmentSecureConfiguration
{
    public static SecureUserConfiguration CreateUserFromEnvironment()
    {
        var username = Environment.GetEnvironmentVariable("AUTH_USERNAME")
            ?? throw new InvalidOperationException("AUTH_USERNAME not configured");
        
        var password = Environment.GetEnvironmentVariable("AUTH_PASSWORD")
            ?? throw new InvalidOperationException("AUTH_PASSWORD not configured");
        
        var rolesStr = Environment.GetEnvironmentVariable("AUTH_ROLES") ?? "User";
        var roles = rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        return new SecureUserConfiguration(username, password, roles);
    }
    
    public static SecureJwtConfiguration CreateJwtFromEnvironment()
    {
        return new SecureJwtConfiguration
        {
            IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
                ?? throw new InvalidOperationException("JWT_SIGNING_KEY not configured"),
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new InvalidOperationException("JWT_ISSUER not configured"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? throw new InvalidOperationException("JWT_AUDIENCE not configured")
        };
    }
}
```

## Testing Infrastructure

### Unit Testing Utilities

```csharp
public static class IdentityTestHelpers
{
    public static AppUserConfiguration CreateTestUser(
        string username = "testuser",
        string password = "testpassword",
        params string[] roles)
    {
        return new AppUserConfiguration(username, password, roles);
    }
    
    public static TestJwtConfiguration CreateTestJwt(
        string signingKey = "test-signing-key-that-is-long-enough",
        string issuer = "test-issuer",
        string audience = "test-audience")
    {
        return new TestJwtConfiguration
        {
            IssuerSigningKey = signingKey,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateLifetime = false, // Relaxed for testing
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true
        };
    }
    
    public static void AssertUserConfigurationValid(BasicUserConfiguration config)
    {
        Assert.That(config.Username, Is.Not.Null.And.Not.Empty);
        Assert.That(config.Password, Is.Not.Null.And.Not.Empty);
        
        if (config.Roles != null)
        {
            Assert.That(config.Roles, Has.All.Not.Null.And.Not.Empty);
        }
    }
    
    public static void AssertJwtConfigurationValid(JwtConfiguration config)
    {
        Assert.That(config.IssuerSigningKey, Is.Not.Null.And.Not.Empty);
        Assert.That(config.ValidIssuer, Is.Not.Null.And.Not.Empty);
        Assert.That(config.ValidAudience, Is.Not.Null.And.Not.Empty);
    }
}
```

### Integration Testing

```csharp
[Test]
public async Task IdentitySystem_EndToEndAuthentication_WorksCorrectly()
{
    // Arrange
    var userConfig = IdentityTestHelpers.CreateTestUser("integrationuser", "password123", "Admin");
    var jwtConfig = IdentityTestHelpers.CreateTestJwt();
    
    var authSystem = new AuthenticationSystem(userConfig, jwtConfig);
    
    // Act - Authenticate
    var isAuthenticated = authSystem.Authenticate("integrationuser", "password123");
    Assert.That(isAuthenticated, Is.True);
    
    // Act - Generate token
    var token = authSystem.GenerateToken("integrationuser");
    Assert.That(token, Is.Not.Null.And.Not.Empty);
    
    // Act - Validate token
    var principal = authSystem.ValidateToken(token);
    Assert.That(principal.Identity.Name, Is.EqualTo("integrationuser"));
    Assert.That(principal.IsInRole("Admin"), Is.True);
}
```

## Performance Considerations

### 1. Configuration Caching

```csharp
public class CachedIdentityProvider
{
    private readonly ConcurrentDictionary<string, BasicUserConfiguration> _userCache = new();
    private readonly ConcurrentDictionary<string, JwtConfiguration> _jwtCache = new();
    
    public BasicUserConfiguration GetUserConfiguration(string tenantId)
    {
        return _userCache.GetOrAdd(tenantId, LoadUserConfiguration);
    }
    
    public JwtConfiguration GetJwtConfiguration(string tenantId)
    {
        return _jwtCache.GetOrAdd(tenantId, LoadJwtConfiguration);
    }
    
    private BasicUserConfiguration LoadUserConfiguration(string tenantId)
    {
        // Load from database, file, or external service
        return LoadUserConfigurationFromSource(tenantId);
    }
}
```

### 2. Memory Optimization

```csharp
public class MemoryEfficientIdentityManager
{
    private readonly WeakReference<Dictionary<string, BasicUserConfiguration>> _configCache;
    
    public async Task<long> CalculateMemoryUsage()
    {
        if (_configCache.TryGetTarget(out var cache))
        {
            return await Size.Calculate(cache);
        }
        
        return 0;
    }
    
    public void OptimizeMemoryUsage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
```

## Migration and Upgrade Strategies

### Configuration Migration

```csharp
public class IdentityConfigurationMigrator
{
    public async Task MigrateFromLegacyFormat(string legacyConfigPath, string newConfigPath)
    {
        // Read legacy configuration
        var legacyConfig = await File.ReadAllTextAsync(legacyConfigPath);
        var legacy = JsonSerializer.Deserialize<LegacyConfiguration>(legacyConfig);
        
        // Convert to new format
        var userConfig = new AppUserConfiguration(
            legacy.Username,
            legacy.Password,
            legacy.Roles?.Split(',') ?? Array.Empty<string>());
        
        var jwtConfig = new AppJwtConfiguration
        {
            IssuerSigningKey = legacy.JwtSecret,
            ValidIssuer = legacy.Issuer,
            ValidAudience = legacy.Audience,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true
        };
        
        // Save in new format
        var newConfig = new IdentityConfiguration { User = userConfig, Jwt = jwtConfig };
        await File.WriteAllTextAsync(newConfigPath, newConfig.ToJson());
    }
}
```

## Integration with BuildingBlocks Ecosystem

### Helper Integration Examples

```csharp
// Using with GuardClause Helper
public class ValidatedUserConfiguration : BasicUserConfiguration
{
    public void SetUsername(string username)
    {
        Username = Guard.Against.NullOrWhiteSpace(username, nameof(username));
        Guard.Against.OutOfRange(username.Length, nameof(username), 3, 50);
    }
}

// Using with StringHelper
public class EncodedConfiguration
{
    public string SerializeUserToBase64(BasicUserConfiguration user)
    {
        return user.ToJsonBase64();
    }
    
    public BasicUserConfiguration DeserializeUserFromBase64(string base64)
    {
        return base64.FromJsonBase64<AppUserConfiguration>();
    }
}

// Using with ConnectionStringHelper
public class DatabaseIntegratedAuth : BasicUserConfiguration
{
    public string ConnectionString { get; set; } = "Server={SERVER};Database={DATABASE};";
    
    public string GetEnrichedConnectionString()
    {
        return ConnectionString.EnrichConnectionString();
    }
}
```

## Error Handling and Diagnostics

### Comprehensive Error Handling

```csharp
public class IdentityErrorHandler
{
    public static void HandleConfigurationError(Exception ex, string context)
    {
        var fullDescription = ex.GetFullDescription(); // Using ExceptionHelper
        
        Logger.LogError($"Identity configuration error in {context}: {fullDescription}");
        
        // Additional diagnostics
        if (ex is SecurityException)
        {
            Logger.LogCritical("Security-related identity configuration error detected");
        }
        
        throw new InvalidOperationException($"Identity system configuration failed: {context}", ex);
    }
}
```

## See Also

- **Core Components:**
  - [BasicUserConfiguration](BasicUserConfiguration.md) - User authentication configuration
  - [JwtConfiguration](JwtConfiguration.md) - JWT authentication configuration

- **Related BuildingBlocks:**
  - [EquatableObject](../Objects/EquatableObject.md) - Base class for value equality
  - [GuardClauseHelper](../Helpers/GuardClauseHelper.md) - Input validation utilities
  - [JwtIdentityHelper](../Helpers/JwtIdentityHelper.md) - JWT token processing
  - [StringHelper](../Helpers/StringHelper.md) - String manipulation utilities
  - [ConnectionStringHelper](../Helpers/ConnectionStringHelper.md) - Configuration processing

- **Serialization Support:**
  - [JsonHelper](../Helpers/JsonHelper.md) - System.Text.Json serialization
  - [NJsonHelper](../Helpers/NJsonHelper.md) - Newtonsoft.Json serialization
  - [YamlHelper](../Helpers/YamlHelper.md) - YAML configuration support

---

*The Identity namespace provides the authentication and authorization foundation for secure .NET applications, with built-in serialization support and seamless integration with the broader RapidStreamer BuildingBlocks ecosystem.*