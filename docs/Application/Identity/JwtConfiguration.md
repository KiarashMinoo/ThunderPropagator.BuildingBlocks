# JwtConfiguration

The `JwtConfiguration` class provides an abstract base for JWT (JSON Web Token) authentication configuration in .NET applications. It offers comprehensive JWT validation settings and parameters required for secure token-based authentication systems.

## Overview

```csharp
public abstract class JwtConfiguration : EquatableObject<JwtConfiguration>
```

`JwtConfiguration` is an abstract class that extends `EquatableObject<JwtConfiguration>`, providing a foundation for JWT authentication configuration with built-in equality comparison and serialization support for both System.Text.Json and Newtonsoft.Json.

## Key Features

- **Comprehensive JWT Validation**: Complete set of JWT validation parameters
- **Dual Serialization Support**: Compatible with both System.Text.Json and Newtonsoft.Json
- **Abstract Design**: Provides base functionality for concrete implementations
- **Equality Comparison**: Inherits value-based equality from EquatableObject
- **Security Configuration**: Configurable validation rules for enhanced security
- **Issuer and Audience Validation**: Support for multi-tenant and distributed scenarios

## Public Properties

### IssuerSigningKey
The cryptographic key used for JWT signature validation.

```csharp
[JsonProperty, JsonInclude] 
public string IssuerSigningKey { get; set; } = null!;
```

**Purpose:** Contains the secret key or public key used to verify JWT signatures
**Security Note:** This should be stored securely and never exposed in logs or client-side code

### ValidAudience
The expected audience for JWT token validation.

```csharp
[JsonProperty, JsonInclude] 
public string ValidAudience { get; set; } = null!;
```

**Purpose:** Identifies the intended recipient of the JWT token
**Usage:** Typically represents the application or service that should accept the token

### ValidIssuer
The expected issuer for JWT token validation.

```csharp
[JsonProperty, JsonInclude] 
public string ValidIssuer { get; set; } = null!;
```

**Purpose:** Identifies the trusted authority that issued the JWT token
**Usage:** Used to ensure tokens come from a trusted source

### ValidateLifetime
Controls whether token expiration and not-before times are validated.

```csharp
[JsonProperty, JsonInclude] 
public bool ValidateLifetime { get; set; }
```

**Purpose:** Enables/disables validation of `exp` (expiration) and `nbf` (not before) claims
**Default Recommendation:** Should typically be `true` for security

### ValidateAudience
Controls whether the audience claim is validated.

```csharp
[JsonProperty, JsonInclude] 
public bool ValidateAudience { get; set; }
```

**Purpose:** Enables/disables validation of the `aud` (audience) claim
**Use Case:** Set to `false` for development or when audience validation is not required

### ValidateIssuer
Controls whether the issuer claim is validated.

```csharp
[JsonProperty, JsonInclude] 
public bool ValidateIssuer { get; set; }
```

**Purpose:** Enables/disables validation of the `iss` (issuer) claim
**Security:** Recommended to be `true` in production environments

### ValidateIssuerSigningKey
Controls whether the signing key is validated.

```csharp
[JsonProperty, JsonInclude] 
public bool ValidateIssuerSigningKey { get; set; }
```

**Purpose:** Enables/disables signature validation using the issuer signing key
**Critical:** Should always be `true` in production for security

## Usage Examples

### Basic JWT Configuration Implementation

```csharp
public class AppJwtConfiguration : JwtConfiguration
{
    public AppJwtConfiguration()
    {
        // Production-ready defaults
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
    
    public static AppJwtConfiguration CreateDefault(string signingKey, string issuer, string audience)
    {
        return new AppJwtConfiguration
        {
            IssuerSigningKey = signingKey,
            ValidIssuer = issuer,
            ValidAudience = audience
        };
    }
    
    public static AppJwtConfiguration CreateDevelopment()
    {
        return new AppJwtConfiguration
        {
            IssuerSigningKey = "development-secret-key-that-is-long-enough",
            ValidIssuer = "dev-issuer",
            ValidAudience = "dev-audience",
            ValidateLifetime = false, // Relaxed for development
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true // Always validate signature
        };
    }
}
```

### Configuration for Different Environments

```csharp
public class EnvironmentJwtConfigurationFactory
{
    public static JwtConfiguration CreateForEnvironment(string environment)
    {
        return environment.ToLower() switch
        {
            "development" => CreateDevelopmentConfig(),
            "staging" => CreateStagingConfig(),
            "production" => CreateProductionConfig(),
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
    }
    
    private static AppJwtConfiguration CreateDevelopmentConfig()
    {
        return new AppJwtConfiguration
        {
            IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_DEV_KEY") ?? "dev-key-123",
            ValidIssuer = "dev.myapp.com",
            ValidAudience = "dev.api.myapp.com",
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true
        };
    }
    
    private static AppJwtConfiguration CreateStagingConfig()
    {
        return new AppJwtConfiguration
        {
            IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_STAGING_KEY") 
                ?? throw new InvalidOperationException("JWT_STAGING_KEY not configured"),
            ValidIssuer = "staging.myapp.com",
            ValidAudience = "staging.api.myapp.com",
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true
        };
    }
    
    private static AppJwtConfiguration CreateProductionConfig()
    {
        return new AppJwtConfiguration
        {
            IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_PROD_KEY") 
                ?? throw new InvalidOperationException("JWT_PROD_KEY not configured"),
            ValidIssuer = "myapp.com",
            ValidAudience = "api.myapp.com",
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true
        };
    }
}
```

### ASP.NET Core Integration

```csharp
public class JwtAuthenticationSetup
{
    public static void ConfigureJwtAuthentication(IServiceCollection services, JwtConfiguration jwtConfig)
    {
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
                    ValidateLifetime = jwtConfig.ValidateLifetime,
                    ClockSkew = TimeSpan.Zero // Disable clock skew for precise timing
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"JWT Token validated for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    }
                };
            });
    }
}

// Usage in Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    var jwtConfig = configuration.GetSection("Jwt").Get<AppJwtConfiguration>();
    JwtAuthenticationSetup.ConfigureJwtAuthentication(services, jwtConfig);
}
```

### Multi-Tenant JWT Configuration

```csharp
public class MultiTenantJwtConfiguration : JwtConfiguration
{
    [JsonProperty, JsonInclude]
    public Dictionary<string, string> TenantAudiences { get; set; } = new();
    
    [JsonProperty, JsonInclude]
    public Dictionary<string, string> TenantIssuers { get; set; } = new();
    
    public string GetAudienceForTenant(string tenantId)
    {
        return TenantAudiences.TryGetValue(tenantId, out string? audience) 
            ? audience 
            : ValidAudience;
    }
    
    public string GetIssuerForTenant(string tenantId)
    {
        return TenantIssuers.TryGetValue(tenantId, out string? issuer) 
            ? issuer 
            : ValidIssuer;
    }
}

public class MultiTenantJwtValidator
{
    private readonly MultiTenantJwtConfiguration _config;
    
    public MultiTenantJwtValidator(MultiTenantJwtConfiguration config)
    {
        _config = config;
    }
    
    public TokenValidationParameters GetValidationParameters(string tenantId)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = _config.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.IssuerSigningKey)),
            ValidateIssuer = _config.ValidateIssuer,
            ValidIssuer = _config.GetIssuerForTenant(tenantId),
            ValidateAudience = _config.ValidateAudience,
            ValidAudience = _config.GetAudienceForTenant(tenantId),
            ValidateLifetime = _config.ValidateLifetime
        };
    }
}
```

### Configuration Serialization and Storage

```csharp
public class JwtConfigurationManager
{
    public async Task SaveConfigurationAsync(JwtConfiguration config, string filePath)
    {
        // Using JSON serialization (System.Text.Json)
        string json = config.ToJson();
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<T> LoadConfigurationAsync<T>(string filePath) where T : JwtConfiguration
    {
        string json = await File.ReadAllTextAsync(filePath);
        return json.FromJson<T>() ?? throw new InvalidOperationException("Failed to deserialize JWT configuration");
    }
    
    public void SaveAsYaml(JwtConfiguration config, string filePath)
    {
        // Using YAML serialization
        string yaml = config.ToYaml();
        File.WriteAllText(filePath, yaml);
    }
    
    public T LoadFromYaml<T>(string filePath) where T : JwtConfiguration
    {
        string yaml = File.ReadAllText(filePath);
        return yaml.FromYaml<T>() ?? throw new InvalidOperationException("Failed to deserialize JWT configuration from YAML");
    }
    
    public void SaveAsBase64(JwtConfiguration config, string configKey)
    {
        // For storing in environment variables or configuration systems
        string base64Config = config.ToJsonBase64();
        Environment.SetEnvironmentVariable(configKey, base64Config);
    }
    
    public T LoadFromBase64<T>(string configKey) where T : JwtConfiguration
    {
        string? base64Config = Environment.GetEnvironmentVariable(configKey);
        if (string.IsNullOrEmpty(base64Config))
            throw new InvalidOperationException($"Configuration key {configKey} not found");
        
        return base64Config.FromJsonBase64<T>() ?? throw new InvalidOperationException("Failed to deserialize JWT configuration from Base64");
    }
}
```

## Security Considerations

### Key Management Best Practices

```csharp
public class SecureJwtConfiguration : JwtConfiguration
{
    private string? _cachedKey;
    
    // Override to provide secure key retrieval
    public override string IssuerSigningKey
    {
        get => _cachedKey ??= RetrieveKeySecurely();
        set => _cachedKey = value;
    }
    
    private string RetrieveKeySecurely()
    {
        // Example: Retrieve from Azure Key Vault, AWS Secrets Manager, etc.
        // This is a simplified example
        return Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") 
            ?? throw new InvalidOperationException("JWT signing key not configured");
    }
    
    public void ValidateSecuritySettings()
    {
        if (string.IsNullOrEmpty(IssuerSigningKey))
            throw new InvalidOperationException("IssuerSigningKey must be configured");
        
        if (IssuerSigningKey.Length < 32)
            throw new InvalidOperationException("IssuerSigningKey must be at least 32 characters long");
        
        if (!ValidateIssuerSigningKey)
            throw new InvalidOperationException("ValidateIssuerSigningKey must be true for security");
        
        // Additional security validations
        if (ValidateLifetime == false)
            Console.WriteLine("Warning: Token lifetime validation is disabled");
    }
}
```

### Production Security Configuration

```csharp
public class ProductionJwtConfiguration : JwtConfiguration
{
    public ProductionJwtConfiguration()
    {
        // Secure defaults for production
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
    
    public static ProductionJwtConfiguration CreateFromEnvironment()
    {
        var config = new ProductionJwtConfiguration
        {
            IssuerSigningKey = GetRequiredEnvironmentVariable("JWT_SIGNING_KEY"),
            ValidIssuer = GetRequiredEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = GetRequiredEnvironmentVariable("JWT_AUDIENCE")
        };
        
        config.ValidateConfiguration();
        return config;
    }
    
    private static string GetRequiredEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name) 
            ?? throw new InvalidOperationException($"Required environment variable {name} is not set");
    }
    
    private void ValidateConfiguration()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(IssuerSigningKey))
            errors.Add("IssuerSigningKey is required");
        else if (IssuerSigningKey.Length < 32)
            errors.Add("IssuerSigningKey must be at least 32 characters");
        
        if (string.IsNullOrWhiteSpace(ValidIssuer))
            errors.Add("ValidIssuer is required");
        
        if (string.IsNullOrWhiteSpace(ValidAudience))
            errors.Add("ValidAudience is required");
        
        if (!ValidateIssuerSigningKey)
            errors.Add("ValidateIssuerSigningKey must be true in production");
        
        if (errors.Any())
            throw new InvalidOperationException($"JWT Configuration validation failed: {string.Join(", ", errors)}");
    }
}
```

## Integration with BuildingBlocks Helpers

### Using with ConnectionStringHelper

```csharp
public class DatabaseJwtConfiguration : JwtConfiguration
{
    [JsonProperty, JsonInclude]
    public string DatabaseConnectionString { get; set; } = null!;
    
    public string GetEnrichedConnectionString()
    {
        // Use ConnectionStringHelper to enrich with environment variables
        return DatabaseConnectionString.EnrichConnectionString();
    }
}
```

### Using with GuardClause Helper

```csharp
public class ValidatedJwtConfiguration : JwtConfiguration
{
    public void SetIssuerSigningKey(string key)
    {
        IssuerSigningKey = Guard.Against.NullOrWhiteSpace(key, nameof(key));
        Guard.Against.OutOfRange(key.Length, nameof(key), 32, 512, "Signing key must be between 32 and 512 characters");
    }
    
    public void SetValidIssuer(string issuer)
    {
        ValidIssuer = Guard.Against.NullOrWhiteSpace(issuer, nameof(issuer));
        Guard.Against.MalformedUri(issuer, nameof(issuer)); // Custom guard extension
    }
    
    public void SetValidAudience(string audience)
    {
        ValidAudience = Guard.Against.NullOrWhiteSpace(audience, nameof(audience));
    }
}
```

### Using with ExceptionHelper

```csharp
public class JwtConfigurationValidator
{
    public void ValidateConfiguration(JwtConfiguration config)
    {
        try
        {
            ValidateRequired(config);
            ValidateSecuritySettings(config);
            ValidateKeyStrength(config);
        }
        catch (Exception ex)
        {
            string detailedError = ex.GetFullDescription(); // Using ExceptionHelper
            throw new InvalidOperationException($"JWT Configuration validation failed: {detailedError}", ex);
        }
    }
    
    private void ValidateRequired(JwtConfiguration config)
    {
        if (string.IsNullOrEmpty(config.IssuerSigningKey))
            throw new ArgumentException("IssuerSigningKey is required");
        
        if (string.IsNullOrEmpty(config.ValidIssuer))
            throw new ArgumentException("ValidIssuer is required");
        
        if (string.IsNullOrEmpty(config.ValidAudience))
            throw new ArgumentException("ValidAudience is required");
    }
    
    private void ValidateSecuritySettings(JwtConfiguration config)
    {
        if (!config.ValidateIssuerSigningKey)
            throw new SecurityException("ValidateIssuerSigningKey must be enabled");
    }
    
    private void ValidateKeyStrength(JwtConfiguration config)
    {
        if (config.IssuerSigningKey.Length < 32)
            throw new SecurityException("Signing key is too short");
    }
}
```

## Error Handling and Validation

### Configuration Validation

```csharp
public static class JwtConfigurationValidator
{
    public static ValidationResult Validate(JwtConfiguration config)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(config.IssuerSigningKey))
            errors.Add("IssuerSigningKey is required");
        
        if (string.IsNullOrWhiteSpace(config.ValidIssuer))
            errors.Add("ValidIssuer is required");
        
        if (string.IsNullOrWhiteSpace(config.ValidAudience))
            errors.Add("ValidAudience is required");
        
        // Security validation
        if (config.IssuerSigningKey?.Length < 32)
            errors.Add("IssuerSigningKey should be at least 32 characters for security");
        
        if (!config.ValidateIssuerSigningKey)
            warnings.Add("Signature validation is disabled - not recommended for production");
        
        if (!config.ValidateLifetime)
            warnings.Add("Lifetime validation is disabled - tokens may be accepted indefinitely");
        
        if (!config.ValidateIssuer)
            warnings.Add("Issuer validation is disabled - tokens from any issuer will be accepted");
        
        if (!config.ValidateAudience)
            warnings.Add("Audience validation is disabled - tokens for any audience will be accepted");
        
        return new ValidationResult(errors, warnings);
    }
}

public class ValidationResult
{
    public List<string> Errors { get; }
    public List<string> Warnings { get; }
    public bool IsValid => !Errors.Any();
    
    public ValidationResult(List<string> errors, List<string> warnings)
    {
        Errors = errors;
        Warnings = warnings;
    }
    
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", Errors)}");
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[Test]
public void JwtConfiguration_DefaultValues_AreSecure()
{
    // Arrange
    var config = new TestJwtConfiguration();
    
    // Act & Assert
    Assert.That(config.ValidateLifetime, Is.True);
    Assert.That(config.ValidateAudience, Is.True);
    Assert.That(config.ValidateIssuer, Is.True);
    Assert.That(config.ValidateIssuerSigningKey, Is.True);
}

[Test]
public void JwtConfiguration_Serialization_PreservesAllProperties()
{
    // Arrange
    var original = new TestJwtConfiguration
    {
        IssuerSigningKey = "test-signing-key-that-is-long-enough",
        ValidIssuer = "test-issuer",
        ValidAudience = "test-audience",
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true
    };
    
    // Act
    string json = original.ToJson();
    var deserialized = json.FromJson<TestJwtConfiguration>();
    
    // Assert
    Assert.That(deserialized, Is.EqualTo(original));
    Assert.That(deserialized.IssuerSigningKey, Is.EqualTo(original.IssuerSigningKey));
    Assert.That(deserialized.ValidateLifetime, Is.EqualTo(original.ValidateLifetime));
}

[Test]
public void JwtConfiguration_Equality_WorksCorrectly()
{
    // Arrange
    var config1 = new TestJwtConfiguration { IssuerSigningKey = "key1", ValidIssuer = "issuer1" };
    var config2 = new TestJwtConfiguration { IssuerSigningKey = "key1", ValidIssuer = "issuer1" };
    var config3 = new TestJwtConfiguration { IssuerSigningKey = "key2", ValidIssuer = "issuer1" };
    
    // Act & Assert
    Assert.That(config1, Is.EqualTo(config2));
    Assert.That(config1, Is.Not.EqualTo(config3));
    Assert.That(config1.GetHashCode(), Is.EqualTo(config2.GetHashCode()));
}

public class TestJwtConfiguration : JwtConfiguration
{
    public TestJwtConfiguration()
    {
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
}
```

### Integration Tests

```csharp
[Test]
public async Task JwtConfiguration_FileOperations_WorkCorrectly()
{
    // Arrange
    var config = new TestJwtConfiguration
    {
        IssuerSigningKey = "integration-test-key-that-is-sufficiently-long",
        ValidIssuer = "integration-test-issuer",
        ValidAudience = "integration-test-audience"
    };
    
    string tempFile = Path.GetTempFileName();
    
    try
    {
        // Act - Save
        var manager = new JwtConfigurationManager();
        await manager.SaveConfigurationAsync(config, tempFile);
        
        // Act - Load
        var loaded = await manager.LoadConfigurationAsync<TestJwtConfiguration>(tempFile);
        
        // Assert
        Assert.That(loaded, Is.EqualTo(config));
        Assert.That(loaded.IssuerSigningKey, Is.EqualTo(config.IssuerSigningKey));
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

## Best Practices

### 1. Security-First Configuration
```csharp
// Preferred - Secure defaults
public class SecureJwtConfiguration : JwtConfiguration
{
    public SecureJwtConfiguration()
    {
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
}

// Avoid - Insecure defaults
public class InsecureJwtConfiguration : JwtConfiguration
{
    public InsecureJwtConfiguration()
    {
        ValidateLifetime = false; // Dangerous
        ValidateIssuerSigningKey = false; // Never do this
    }
}
```

### 2. Environment-Specific Configuration
```csharp
// Use factory pattern for environment-specific configurations
public static JwtConfiguration Create(string environment)
{
    return environment switch
    {
        "Production" => CreateProductionConfig(),
        "Development" => CreateDevelopmentConfig(),
        _ => throw new ArgumentException($"Unknown environment: {environment}")
    };
}
```

### 3. Validation Before Use
```csharp
public void UseConfiguration(JwtConfiguration config)
{
    var validation = JwtConfigurationValidator.Validate(config);
    validation.ThrowIfInvalid();
    
    // Safe to use config
    ConfigureJwtAuthentication(config);
}
```

## Migration and Upgrades

When upgrading JWT configuration systems:

```csharp
// Old approach - Manual configuration
private void ConfigureJwtOld()
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("manual-key"));
    // Manual configuration...
}

// New approach - Using JwtConfiguration
private void ConfigureJwtNew(JwtConfiguration config)
{
    var parameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = config.ValidateIssuerSigningKey,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.IssuerSigningKey)),
        // Other parameters from config...
    };
}
```

## See Also

- [BasicUserConfiguration](BasicUserConfiguration.md) - User authentication configuration
- [JwtIdentityHelper](../Helpers/JwtIdentityHelper.md) - JWT token processing utilities
- [EquatableObject](../Objects/EquatableObject.md) - Base class for value equality
- [ConnectionStringHelper](../Helpers/ConnectionStringHelper.md) - Configuration string processing
- [GuardClauseHelper](../Helpers/GuardClauseHelper.md) - Input validation utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application.Identity namespace - providing JWT authentication configuration infrastructure for .NET applications.*