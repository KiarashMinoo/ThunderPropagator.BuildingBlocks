# JwtIdentityHelper

The `JwtIdentityHelper` is a specialized utility for JWT (JSON Web Token) validation and claims principal extraction. It provides secure token validation with configurable parameters and seamless integration with ASP.NET Core identity systems.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `JwtIdentityHelper` enhances JWT operations by providing:

- **Token Validation**: Comprehensive JWT token validation using Microsoft.IdentityModel.Tokens
- **Claims Extraction**: Safe extraction of `ClaimsPrincipal` from validated tokens
- **Configuration-Driven**: Uses `JwtConfiguration` for flexible validation parameters
- **Security-First**: Built-in exception handling for secure token processing
- **Integration Ready**: Seamless integration with ASP.NET Core authentication

## Key Features

### 🔐 Secure Token Validation
- Comprehensive validation using `TokenValidationParameters`
- Symmetric key signature validation
- Audience and issuer verification
- Configurable lifetime validation

### 👤 Claims Principal Extraction
- Safe extraction of user claims from validated tokens
- Integration with .NET Identity framework
- Support for custom claim types and values

### 🎛️ Flexible Configuration
- Configuration-driven validation through `JwtConfiguration`
- Customizable validation parameters per environment
- Support for different signing keys and audiences

### 🛡️ Exception Safety
- Safe token processing with built-in exception handling
- Graceful failure handling for invalid tokens
- No sensitive information leakage in error scenarios

## Core Methods

### GetPrincipalFromToken
```csharp
public static ClaimsPrincipal? GetPrincipalFromToken(string token, JwtConfiguration jwtConfiguration)
```

Validates a JWT token and extracts the claims principal if valid.

**Parameters:**
- `token`: The JWT token string to validate
- `jwtConfiguration`: Configuration object containing validation parameters

**Returns:**
- `ClaimsPrincipal?`: The claims principal if validation succeeds, `null` if validation fails

### IsTokenValid
```csharp
public static bool IsTokenValid(string token, JwtConfiguration jwtConfiguration, out ClaimsPrincipal? claimsPrincipal)
```

Validates a JWT token and provides both validation result and claims principal.

**Parameters:**
- `token`: The JWT token string to validate
- `jwtConfiguration`: Configuration object containing validation parameters
- `claimsPrincipal`: Output parameter containing the claims principal if validation succeeds

**Returns:**
- `bool`: `true` if token is valid, `false` otherwise

## JwtConfiguration Structure

The helper works with `JwtConfiguration` objects that define validation parameters:

```csharp
public abstract class JwtConfiguration : EquatableObject<JwtConfiguration>
{
    public string IssuerSigningKey { get; set; } = null!;
    public string ValidAudience { get; set; } = null!;
    public string ValidIssuer { get; set; } = null!;
    public bool ValidateLifetime { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateIssuerSigningKey { get; set; }
}
```

## Usage Examples

### Basic Token Validation
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.BuildingBlocks.Application.Identity;

public class AuthenticationService
{
    private readonly JwtConfiguration _jwtConfig;
    
    public AuthenticationService(JwtConfiguration jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }
    
    public ClaimsPrincipal? ValidateUserToken(string authToken)
    {
        // Extract claims principal from token
        var principal = JwtIdentityHelper.GetPrincipalFromToken(authToken, _jwtConfig);
        
        if (principal != null)
        {
            // Token is valid, extract user information
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            Console.WriteLine($"Valid token for user: {username} (ID: {userId})");
            Console.WriteLine($"Roles: {string.Join(", ", roles)}");
        }
        
        return principal;
    }
}
```

### Token Validation with Result Check
```csharp
public class TokenValidator
{
    private readonly JwtConfiguration _jwtConfig;
    
    public TokenValidator(JwtConfiguration jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }
    
    public (bool IsValid, string UserId, string[] Roles) ValidateToken(string token)
    {
        // Validate token and get both result and principal
        bool isValid = JwtIdentityHelper.IsTokenValid(token, _jwtConfig, out ClaimsPrincipal? principal);
        
        if (isValid && principal != null)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            return (true, userId, roles);
        }
        
        return (false, "", Array.Empty<string>());
    }
}
```

### Custom JWT Configuration
```csharp
public class CustomJwtConfiguration : JwtConfiguration
{
    public CustomJwtConfiguration()
    {
        IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") 
                          ?? "your-256-bit-secret-key-here";
        ValidAudience = "your-api-audience";
        ValidIssuer = "your-api-issuer";
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
}

// Usage
var jwtConfig = new CustomJwtConfiguration();
var principal = JwtIdentityHelper.GetPrincipalFromToken(token, jwtConfig);
```

### Configuration from Settings
```csharp
public class JwtSettings : JwtConfiguration
{
    public JwtSettings(IConfiguration configuration)
    {
        IssuerSigningKey = configuration["Jwt:SigningKey"];
        ValidAudience = configuration["Jwt:Audience"];
        ValidIssuer = configuration["Jwt:Issuer"];
        ValidateLifetime = configuration.GetValue<bool>("Jwt:ValidateLifetime");
        ValidateAudience = configuration.GetValue<bool>("Jwt:ValidateAudience");
        ValidateIssuer = configuration.GetValue<bool>("Jwt:ValidateIssuer");
        ValidateIssuerSigningKey = configuration.GetValue<bool>("Jwt:ValidateIssuerSigningKey");
    }
}
```

## Advanced Scenarios

### API Middleware Integration
```csharp
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtConfiguration _jwtConfig;
    
    public JwtAuthenticationMiddleware(RequestDelegate next, JwtConfiguration jwtConfig)
    {
        _next = next;
        _jwtConfig = jwtConfig;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Validate token using JwtIdentityHelper
            if (JwtIdentityHelper.IsTokenValid(token, _jwtConfig, out ClaimsPrincipal? principal))
            {
                // Set the user context
                context.User = principal!;
                
                // Log successful authentication
                var userId = principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"Authenticated user: {userId}");
            }
            else
            {
                // Token validation failed
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token");
                return;
            }
        }
        
        await _next(context);
    }
}
```

### Custom Claims Extraction
```csharp
public class ClaimsExtractor
{
    private readonly JwtConfiguration _jwtConfig;
    
    public ClaimsExtractor(JwtConfiguration jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }
    
    public UserProfile? ExtractUserProfile(string token)
    {
        var principal = JwtIdentityHelper.GetPrincipalFromToken(token, _jwtConfig);
        
        if (principal == null)
            return null;
        
        return new UserProfile
        {
            Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Username = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
            
            // Custom claims
            Department = principal.FindFirst("department")?.Value ?? "",
            LastLogin = DateTime.TryParse(
                principal.FindFirst("last_login")?.Value, 
                out DateTime lastLogin) ? lastLogin : DateTime.MinValue,
            
            // Permission claims
            Permissions = principal.FindAll("permission").Select(c => c.Value).ToArray()
        };
    }
}

public class UserProfile
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string Department { get; set; } = "";
    public DateTime LastLogin { get; set; }
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
```

### Role-Based Authorization
```csharp
public class AuthorizationService
{
    private readonly JwtConfiguration _jwtConfig;
    
    public AuthorizationService(JwtConfiguration jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }
    
    public bool HasPermission(string token, string requiredRole)
    {
        if (!JwtIdentityHelper.IsTokenValid(token, _jwtConfig, out ClaimsPrincipal? principal))
            return false;
        
        return principal!.IsInRole(requiredRole);
    }
    
    public bool HasAnyPermission(string token, params string[] requiredRoles)
    {
        if (!JwtIdentityHelper.IsTokenValid(token, _jwtConfig, out ClaimsPrincipal? principal))
            return false;
        
        return requiredRoles.Any(role => principal!.IsInRole(role));
    }
    
    public bool HasAllPermissions(string token, params string[] requiredRoles)
    {
        if (!JwtIdentityHelper.IsTokenValid(token, _jwtConfig, out ClaimsPrincipal? principal))
            return false;
        
        return requiredRoles.All(role => principal!.IsInRole(role));
    }
}
```

### Multi-Tenant Token Validation
```csharp
public class MultiTenantJwtValidator
{
    private readonly Dictionary<string, JwtConfiguration> _tenantConfigurations;
    
    public MultiTenantJwtValidator(Dictionary<string, JwtConfiguration> tenantConfigurations)
    {
        _tenantConfigurations = tenantConfigurations;
    }
    
    public (bool IsValid, ClaimsPrincipal? Principal, string TenantId) ValidateToken(string token)
    {
        // Try to decode token without validation to extract tenant information
        var handler = new JwtSecurityTokenHandler();
        
        if (handler.ReadJwtToken(token) is JwtSecurityToken jwt)
        {
            var tenantId = jwt.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            
            if (tenantId != null && _tenantConfigurations.TryGetValue(tenantId, out JwtConfiguration? config))
            {
                // Validate using tenant-specific configuration
                bool isValid = JwtIdentityHelper.IsTokenValid(token, config, out ClaimsPrincipal? principal);
                return (isValid, principal, tenantId);
            }
        }
        
        return (false, null, "");
    }
}
```

## Configuration Patterns

### appsettings.json Configuration
```json
{
  "Jwt": {
    "SigningKey": "your-256-bit-secret-key-here-that-is-long-enough",
    "Audience": "your-api-audience",
    "Issuer": "your-api-issuer",
    "ValidateLifetime": true,
    "ValidateAudience": true,
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true
  }
}
```

### Environment-Based Configuration
```csharp
public class EnvironmentJwtConfiguration : JwtConfiguration
{
    public EnvironmentJwtConfiguration()
    {
        IssuerSigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") 
                          ?? throw new InvalidOperationException("JWT_SIGNING_KEY environment variable is required");
        
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                       ?? "default-audience";
        
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                     ?? "default-issuer";
        
        // Default to secure settings
        ValidateLifetime = bool.Parse(Environment.GetEnvironmentVariable("JWT_VALIDATE_LIFETIME") ?? "true");
        ValidateAudience = bool.Parse(Environment.GetEnvironmentVariable("JWT_VALIDATE_AUDIENCE") ?? "true");
        ValidateIssuer = bool.Parse(Environment.GetEnvironmentVariable("JWT_VALIDATE_ISSUER") ?? "true");
        ValidateIssuerSigningKey = bool.Parse(Environment.GetEnvironmentVariable("JWT_VALIDATE_KEY") ?? "true");
    }
}
```

### Development vs Production Configuration
```csharp
public class DevelopmentJwtConfiguration : JwtConfiguration
{
    public DevelopmentJwtConfiguration()
    {
        IssuerSigningKey = "development-key-not-for-production-use";
        ValidAudience = "development-audience";
        ValidIssuer = "development-issuer";
        
        // Relaxed validation for development
        ValidateLifetime = false;
        ValidateAudience = false;
        ValidateIssuer = false;
        ValidateIssuerSigningKey = true;
    }
}

public class ProductionJwtConfiguration : JwtConfiguration
{
    public ProductionJwtConfiguration(IConfiguration configuration)
    {
        IssuerSigningKey = configuration["Jwt:SigningKey"] 
                          ?? throw new InvalidOperationException("JWT signing key is required in production");
        
        ValidAudience = configuration["Jwt:Audience"];
        ValidIssuer = configuration["Jwt:Issuer"];
        
        // Strict validation for production
        ValidateLifetime = true;
        ValidateAudience = true;
        ValidateIssuer = true;
        ValidateIssuerSigningKey = true;
    }
}
```

## Error Handling and Security

### Token Validation Process
The helper performs comprehensive validation:

1. **Signature Validation**: Verifies token signature using the provided signing key
2. **Audience Validation**: Ensures token is intended for the correct audience
3. **Issuer Validation**: Verifies the token was issued by a trusted issuer
4. **Lifetime Validation**: Checks token expiration and not-before claims
5. **Key Validation**: Validates the signing key parameters

### Security Considerations
```csharp
public class SecureTokenValidator
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly ILogger<SecureTokenValidator> _logger;
    
    public SecureTokenValidator(JwtConfiguration jwtConfig, ILogger<SecureTokenValidator> logger)
    {
        _jwtConfig = jwtConfig;
        _logger = logger;
    }
    
    public ClaimsPrincipal? ValidateTokenSecurely(string token, string clientIpAddress)
    {
        // Log validation attempt (without token content)
        _logger.LogInformation("Token validation attempted from IP: {ClientIp}", clientIpAddress);
        
        var principal = JwtIdentityHelper.GetPrincipalFromToken(token, _jwtConfig);
        
        if (principal != null)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Successful token validation for user: {UserId}", userId);
        }
        else
        {
            // Log failed validation without exposing token details
            _logger.LogWarning("Token validation failed from IP: {ClientIp}", clientIpAddress);
        }
        
        return principal;
    }
}
```

### Exception Safety
The helper includes built-in exception handling:

```csharp
try
{
    claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
}
catch
{
    // All exceptions are caught and ignored
    // Returns null to indicate validation failure
    // No sensitive information is exposed
}
```

## Testing Strategies

### Unit Testing
```csharp
[Test]
public void GetPrincipalFromToken_WithValidToken_ReturnsPrincipal()
{
    // Arrange
    var jwtConfig = new TestJwtConfiguration();
    var validToken = GenerateValidTestToken();
    
    // Act
    var principal = JwtIdentityHelper.GetPrincipalFromToken(validToken, jwtConfig);
    
    // Assert
    Assert.IsNotNull(principal);
    Assert.AreEqual("test-user", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
}

[Test]
public void IsTokenValid_WithInvalidToken_ReturnsFalse()
{
    // Arrange
    var jwtConfig = new TestJwtConfiguration();
    var invalidToken = "invalid.jwt.token";
    
    // Act
    var isValid = JwtIdentityHelper.IsTokenValid(invalidToken, jwtConfig, out var principal);
    
    // Assert
    Assert.IsFalse(isValid);
    Assert.IsNull(principal);
}
```

### Integration Testing
```csharp
[Test]
public async Task AuthenticationMiddleware_WithValidToken_SetsUserContext()
{
    // Arrange
    var client = CreateTestClient();
    var token = await GenerateValidTokenAsync();
    
    client.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    // Act
    var response = await client.GetAsync("/api/protected");
    
    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

### Security Testing
```csharp
[Test]
public void ValidateToken_WithExpiredToken_ReturnsFalse()
{
    var jwtConfig = new TestJwtConfiguration { ValidateLifetime = true };
    var expiredToken = GenerateExpiredToken();
    
    var isValid = JwtIdentityHelper.IsTokenValid(expiredToken, jwtConfig, out _);
    
    Assert.IsFalse(isValid);
}

[Test]
public void ValidateToken_WithWrongAudience_ReturnsFalse()
{
    var jwtConfig = new TestJwtConfiguration 
    { 
        ValidAudience = "correct-audience",
        ValidateAudience = true 
    };
    var tokenWithWrongAudience = GenerateTokenWithAudience("wrong-audience");
    
    var isValid = JwtIdentityHelper.IsTokenValid(tokenWithWrongAudience, jwtConfig, out _);
    
    Assert.IsFalse(isValid);
}
```

## Best Practices

### 1. Use Strong Signing Keys
```csharp
// ✅ Good: Use sufficiently long, random signing keys
var signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

// ❌ Avoid: Weak or predictable signing keys
var weakKey = "password123";
```

### 2. Enable Appropriate Validations
```csharp
// ✅ Good: Enable all relevant validations for production
var productionConfig = new JwtConfiguration
{
    ValidateLifetime = true,
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateIssuerSigningKey = true
};

// ❌ Avoid: Disabling validations in production
var insecureConfig = new JwtConfiguration
{
    ValidateLifetime = false,  // Dangerous in production
    ValidateAudience = false   // Allows token reuse across services
};
```

### 3. Handle Validation Failures Gracefully
```csharp
// ✅ Good: Check validation result before using principal
if (JwtIdentityHelper.IsTokenValid(token, config, out var principal))
{
    // Safe to use principal
    var userId = principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

// ❌ Avoid: Assuming validation always succeeds
var principal = JwtIdentityHelper.GetPrincipalFromToken(token, config);
var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Potential null reference
```

### 4. Log Security Events
```csharp
// ✅ Good: Log authentication events for monitoring
if (JwtIdentityHelper.IsTokenValid(token, config, out var principal))
{
    logger.LogInformation("Successful authentication for user {UserId}", 
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value);
}
else
{
    logger.LogWarning("Authentication failed from IP {ClientIp}", clientIpAddress);
}
```

## Related Components

- **[JwtConfiguration](../Identity/JwtConfiguration.md)**: Configuration class for JWT validation parameters
- **[GuardClauseHelper](GuardClauseHelper.md)**: Validation utilities for input parameters
- **[JsonHelper](JsonHelper.md)**: JSON serialization for configuration and logging
- **Microsoft.IdentityModel.Tokens**: Underlying JWT validation library
- **System.IdentityModel.Tokens.Jwt**: JWT token handling functionality

## Migration Guide

### From Manual JWT Validation
```csharp
// Before: Manual JWT validation
var tokenHandler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters
{
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
    ValidateIssuerSigningKey = true,
    ValidateIssuer = false,
    ValidateAudience = false,
    ClockSkew = TimeSpan.Zero
};

ClaimsPrincipal principal;
try
{
    principal = tokenHandler.ValidateToken(token, validationParameters, out _);
}
catch
{
    principal = null;
}

// After: Using JwtIdentityHelper
var jwtConfig = new MyJwtConfiguration();
var principal = JwtIdentityHelper.GetPrincipalFromToken(token, jwtConfig);
```

### From ASP.NET Core JWT Bearer
```csharp
// Before: Built-in JWT bearer authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience
        };
    });

// After: Using JwtIdentityHelper with custom middleware
services.AddScoped<JwtConfiguration>(provider => new MyJwtConfiguration());
app.UseMiddleware<JwtAuthenticationMiddleware>();
```

The JwtIdentityHelper provides a secure, configurable foundation for JWT token validation throughout the RapidStreamer BuildingBlocks system, with comprehensive validation options and seamless integration capabilities.