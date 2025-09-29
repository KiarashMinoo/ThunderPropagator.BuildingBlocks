# AuthenticationType

The `AuthenticationType` enum defines the supported authentication mechanisms in the RapidStreamer BuildingBlocks framework. It provides a standardized way to specify how services, APIs, and components should authenticate requests and establish secure connections.

## Overview

The `AuthenticationType` enum is used throughout the RapidStreamer framework to configure authentication behaviors for various components including HTTP clients, API endpoints, message brokers, and external service integrations.

## Enum Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.Enums
{
    public enum AuthenticationType
    {
        None,
        Basic,
        OAuth2
    }
}
```

## Values

### None
- **Value**: `0`
- **Description**: No authentication required or performed
- **Use Case**: Public endpoints, internal services within trusted networks, development/testing scenarios
- **Security Level**: No security - use only for non-sensitive operations

### Basic
- **Value**: `1` 
- **Description**: HTTP Basic Authentication using username and password
- **Use Case**: Simple authentication scenarios, legacy system integration, internal APIs
- **Security Level**: Medium security - credentials are base64 encoded but not encrypted
- **Requirements**: Always use with HTTPS in production

### OAuth2
- **Value**: `2`
- **Description**: OAuth 2.0 authentication protocol
- **Use Case**: Modern web applications, third-party integrations, microservices
- **Security Level**: High security - token-based authentication with expiration
- **Requirements**: Requires OAuth 2.0 provider and proper token management

## Usage Examples

### Configuration Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.Enums;

public class ApiClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public AuthenticationType AuthType { get; set; } = AuthenticationType.None;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AccessToken { get; set; }
    public TimeSpan? TokenExpiry { get; set; }
}

// Configure different authentication types
var configurations = new[]
{
    new ApiClientConfiguration 
    { 
        BaseUrl = "https://public-api.example.com",
        AuthType = AuthenticationType.None 
    },
    
    new ApiClientConfiguration 
    { 
        BaseUrl = "https://internal-api.example.com",
        AuthType = AuthenticationType.Basic,
        Username = "admin",
        Password = "secure-password"
    },
    
    new ApiClientConfiguration 
    { 
        BaseUrl = "https://oauth-api.example.com",
        AuthType = AuthenticationType.OAuth2,
        AccessToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        TokenExpiry = DateTime.UtcNow.AddHours(1)
    }
};
```

### HTTP Client Factory

```csharp
public class AuthenticatedHttpClientFactory
{
    public HttpClient CreateClient(ApiClientConfiguration config)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(config.BaseUrl);
        
        ConfigureAuthentication(client, config);
        
        return client;
    }
    
    private void ConfigureAuthentication(HttpClient client, ApiClientConfiguration config)
    {
        switch (config.AuthType)
        {
            case AuthenticationType.None:
                // No authentication headers needed
                break;
                
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}")
                    );
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Basic", credentials);
                }
                break;
                
            case AuthenticationType.OAuth2:
                if (!string.IsNullOrEmpty(config.AccessToken))
                {
                    // Handle both "Bearer token" and "token" formats
                    var token = config.AccessToken.StartsWith("Bearer ") 
                        ? config.AccessToken.Substring(7)
                        : config.AccessToken;
                        
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                }
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(config.AuthType), 
                    config.AuthType, "Unsupported authentication type");
        }
    }
}
```

### Service Configuration

```csharp
public class ExternalServiceSettings
{
    public string ServiceName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public AuthenticationType Authentication { get; set; } = AuthenticationType.None;
    public Dictionary<string, string> AuthParameters { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class ExternalServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ExternalServiceSettings _settings;
    
    public ExternalServiceClient(HttpClient httpClient, ExternalServiceSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
        
        ConfigureClient();
    }
    
    private void ConfigureClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.Endpoint);
        _httpClient.Timeout = _settings.Timeout;
        
        switch (_settings.Authentication)
        {
            case AuthenticationType.None:
                // Public API - no authentication
                break;
                
            case AuthenticationType.Basic:
                var username = _settings.AuthParameters.GetValueOrDefault("username");
                var password = _settings.AuthParameters.GetValueOrDefault("password");
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    SetBasicAuthentication(username, password);
                }
                break;
                
            case AuthenticationType.OAuth2:
                var clientId = _settings.AuthParameters.GetValueOrDefault("client_id");
                var clientSecret = _settings.AuthParameters.GetValueOrDefault("client_secret");
                var scope = _settings.AuthParameters.GetValueOrDefault("scope");
                
                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    ConfigureOAuth2Authentication(clientId, clientSecret, scope);
                }
                break;
        }
    }
    
    private void SetBasicAuthentication(string username, string password)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{username}:{password}")
        );
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
    }
    
    private async void ConfigureOAuth2Authentication(string clientId, string clientSecret, string? scope)
    {
        var tokenEndpoint = _settings.AuthParameters.GetValueOrDefault("token_endpoint");
        
        if (!string.IsNullOrEmpty(tokenEndpoint))
        {
            var token = await GetOAuth2Token(tokenEndpoint, clientId, clientSecret, scope);
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
    
    private async Task<string?> GetOAuth2Token(string tokenEndpoint, string clientId, 
        string clientSecret, string? scope)
    {
        using var tokenClient = new HttpClient();
        
        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret)
        };
        
        if (!string.IsNullOrEmpty(scope))
        {
            requestBody.Add(new("scope", scope));
        }
        
        var response = await tokenClient.PostAsync(tokenEndpoint, 
            new FormUrlEncodedContent(requestBody));
            
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(tokenResponse);
            
            return tokenData?.GetValueOrDefault("access_token")?.ToString();
        }
        
        return null;
    }
}
```

### Dependency Injection Configuration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticatedServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register different service configurations
        services.Configure<ExternalServiceSettings>("PaymentService", config =>
        {
            config.ServiceName = "Payment API";
            config.Endpoint = "https://payment-api.company.com";
            config.Authentication = AuthenticationType.OAuth2;
            config.AuthParameters = new Dictionary<string, string>
            {
                ["client_id"] = configuration["PaymentApi:ClientId"],
                ["client_secret"] = configuration["PaymentApi:ClientSecret"],
                ["token_endpoint"] = "https://auth.company.com/oauth/token",
                ["scope"] = "payment:read payment:write"
            };
        });
        
        services.Configure<ExternalServiceSettings>("InternalService", config =>
        {
            config.ServiceName = "Internal API";
            config.Endpoint = "https://internal-api.company.com";
            config.Authentication = AuthenticationType.Basic;
            config.AuthParameters = new Dictionary<string, string>
            {
                ["username"] = configuration["InternalApi:Username"],
                ["password"] = configuration["InternalApi:Password"]
            };
        });
        
        services.Configure<ExternalServiceSettings>("PublicService", config =>
        {
            config.ServiceName = "Public API";
            config.Endpoint = "https://public-api.example.com";
            config.Authentication = AuthenticationType.None;
        });
        
        // Register HTTP clients with authentication
        services.AddHttpClient<ExternalServiceClient>("PaymentService")
            .ConfigureHttpClient((provider, client) =>
            {
                var settings = provider.GetRequiredService<IOptionsSnapshot<ExternalServiceSettings>>()
                    .Get("PaymentService");
                // Client configuration will be handled by ExternalServiceClient constructor
            });
            
        return services;
    }
}
```

### Authentication Middleware

```csharp
public class AuthenticationTypeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthenticationOptions _options;
    
    public AuthenticationTypeMiddleware(RequestDelegate next, 
        IOptions<AuthenticationOptions> options)
    {
        _next = next;
        _options = options.Value;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var authAttribute = endpoint?.Metadata.GetMetadata<RequireAuthenticationAttribute>();
        
        if (authAttribute != null)
        {
            var isAuthenticated = await ValidateAuthentication(context, authAttribute.AuthType);
            
            if (!isAuthenticated)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication required");
                return;
            }
        }
        
        await _next(context);
    }
    
    private async Task<bool> ValidateAuthentication(HttpContext context, 
        AuthenticationType authType)
    {
        switch (authType)
        {
            case AuthenticationType.None:
                return true;
                
            case AuthenticationType.Basic:
                return ValidateBasicAuthentication(context);
                
            case AuthenticationType.OAuth2:
                return await ValidateOAuth2Authentication(context);
                
            default:
                return false;
        }
    }
    
    private bool ValidateBasicAuthentication(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            return false;
        }
        
        try
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);
            
            if (parts.Length != 2)
            {
                return false;
            }
            
            var username = parts[0];
            var password = parts[1];
            
            // Validate against configured credentials
            return _options.BasicAuth.Username == username && 
                   _options.BasicAuth.Password == password;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> ValidateOAuth2Authentication(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return false;
        }
        
        var token = authHeader.Substring("Bearer ".Length);
        
        // Validate token with OAuth 2.0 provider
        return await ValidateTokenWithProvider(token);
    }
    
    private async Task<bool> ValidateTokenWithProvider(string token)
    {
        // Implementation depends on your OAuth 2.0 provider
        // This is a simplified example
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
                
            var response = await client.GetAsync(_options.OAuth2.IntrospectionEndpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                
                return tokenInfo?.GetValueOrDefault("active")?.ToString() == "true";
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthenticationAttribute : Attribute
{
    public AuthenticationType AuthType { get; }
    
    public RequireAuthenticationAttribute(AuthenticationType authType)
    {
        AuthType = authType;
    }
}

// Usage on controllers/actions
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [HttpGet("public")]
    [RequireAuthentication(AuthenticationType.None)]
    public IActionResult GetPublicData()
    {
        return Ok("This is public data");
    }
    
    [HttpGet("internal")]
    [RequireAuthentication(AuthenticationType.Basic)]
    public IActionResult GetInternalData()
    {
        return Ok("This is internal data");
    }
    
    [HttpGet("secure")]
    [RequireAuthentication(AuthenticationType.OAuth2)]
    public IActionResult GetSecureData()
    {
        return Ok("This is secure data");
    }
}
```

## Integration Patterns

### Repository Pattern with Authentication

```csharp
public interface IAuthenticatedRepository<T>
{
    Task<T> GetAsync(string id, AuthenticationType authType = AuthenticationType.None);
    Task<T> CreateAsync(T entity, AuthenticationType authType = AuthenticationType.Basic);
    Task<T> UpdateAsync(T entity, AuthenticationType authType = AuthenticationType.OAuth2);
    Task DeleteAsync(string id, AuthenticationType authType = AuthenticationType.OAuth2);
}

public class ExternalDataRepository : IAuthenticatedRepository<DataModel>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationConfiguration _authConfig;
    
    public ExternalDataRepository(IHttpClientFactory httpClientFactory, 
        AuthenticationConfiguration authConfig)
    {
        _httpClientFactory = httpClientFactory;
        _authConfig = authConfig;
    }
    
    public async Task<DataModel> GetAsync(string id, 
        AuthenticationType authType = AuthenticationType.None)
    {
        using var client = CreateAuthenticatedClient(authType);
        var response = await client.GetAsync($"api/data/{id}");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DataModel>(content)!;
    }
    
    private HttpClient CreateAuthenticatedClient(AuthenticationType authType)
    {
        var client = _httpClientFactory.CreateClient();
        
        switch (authType)
        {
            case AuthenticationType.Basic:
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_authConfig.Username}:{_authConfig.Password}")
                );
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Basic", credentials);
                break;
                
            case AuthenticationType.OAuth2:
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _authConfig.AccessToken);
                break;
        }
        
        return client;
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[TestClass]
public class AuthenticationTypeTests
{
    [TestMethod]
    public void AuthenticationType_HasExpectedValues()
    {
        // Verify enum values
        Assert.AreEqual(0, (int)AuthenticationType.None);
        Assert.AreEqual(1, (int)AuthenticationType.Basic);
        Assert.AreEqual(2, (int)AuthenticationType.OAuth2);
    }
    
    [TestMethod]
    public void AuthenticationType_CanBeConvertedToString()
    {
        Assert.AreEqual("None", AuthenticationType.None.ToString());
        Assert.AreEqual("Basic", AuthenticationType.Basic.ToString());
        Assert.AreEqual("OAuth2", AuthenticationType.OAuth2.ToString());
    }
    
    [TestMethod]
    public void AuthenticationType_CanBeParsedFromString()
    {
        Assert.AreEqual(AuthenticationType.None, 
            Enum.Parse<AuthenticationType>("None"));
        Assert.AreEqual(AuthenticationType.Basic, 
            Enum.Parse<AuthenticationType>("Basic"));
        Assert.AreEqual(AuthenticationType.OAuth2, 
            Enum.Parse<AuthenticationType>("OAuth2"));
    }
    
    [TestMethod]
    public void AuthenticationConfiguration_UsesCorrectType()
    {
        var config = new ApiClientConfiguration
        {
            AuthType = AuthenticationType.OAuth2
        };
        
        Assert.AreEqual(AuthenticationType.OAuth2, config.AuthType);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class AuthenticatedServiceTests
{
    private TestServer _server;
    private HttpClient _client;
    
    [TestInitialize]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<TestStartup>();
            
        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }
    
    [TestMethod]
    public async Task PublicEndpoint_AllowsNoAuthentication()
    {
        var response = await _client.GetAsync("/api/public");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task BasicAuthEndpoint_RequiresCredentials()
    {
        // Test without credentials
        var response = await _client.GetAsync("/api/basic");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        
        // Test with valid credentials
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("admin:password")
        );
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
            
        response = await _client.GetAsync("/api/basic");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task OAuth2Endpoint_RequiresValidToken()
    {
        // Test without token
        var response = await _client.GetAsync("/api/oauth");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        
        // Test with valid token
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "valid-test-token");
            
        response = await _client.GetAsync("/api/oauth");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Security Considerations

### Best Practices

1. **Always use HTTPS**: Never send Basic authentication over unencrypted connections
2. **Token Management**: Implement proper token refresh mechanisms for OAuth2
3. **Credential Storage**: Store credentials securely using configuration providers
4. **Validation**: Always validate authentication parameters before use
5. **Logging**: Log authentication attempts (success/failure) for security monitoring

### Security Guidelines

```csharp
public class SecureAuthenticationService
{
    public bool ValidateAuthenticationType(AuthenticationType authType, 
        bool isProductionEnvironment)
    {
        switch (authType)
        {
            case AuthenticationType.None:
                // Only allow in development or for truly public endpoints
                if (isProductionEnvironment)
                {
                    // Log security warning
                    LogSecurityWarning("No authentication configured for production endpoint");
                }
                return !isProductionEnvironment;
                
            case AuthenticationType.Basic:
                // Ensure HTTPS is required
                return true; // Should be validated at the transport level
                
            case AuthenticationType.OAuth2:
                // Preferred for production
                return true;
                
            default:
                return false;
        }
    }
    
    private void LogSecurityWarning(string message)
    {
        // Implementation depends on your logging framework
        Console.WriteLine($"SECURITY WARNING: {message}");
    }
}
```

## Performance Considerations

### Caching Authentication

```csharp
public class CachedAuthenticationService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);
    
    public CachedAuthenticationService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public async Task<string?> GetAuthTokenAsync(AuthenticationType authType, 
        string cacheKey, Func<Task<string?>> tokenProvider)
    {
        if (authType != AuthenticationType.OAuth2)
        {
            // Only cache OAuth2 tokens
            return await tokenProvider();
        }
        
        if (_cache.TryGetValue(cacheKey, out string? cachedToken))
        {
            return cachedToken;
        }
        
        var token = await tokenProvider();
        
        if (!string.IsNullOrEmpty(token))
        {
            _cache.Set(cacheKey, token, _cacheExpiry);
        }
        
        return token;
    }
}
```

## Best Practices

1. **Enum Usage**: Use switch statements with exhaustive case coverage
2. **Configuration**: Store authentication settings in secure configuration
3. **Validation**: Always validate authentication type before processing
4. **Fallback**: Implement graceful fallback for unsupported authentication types
5. **Monitoring**: Log authentication events for security and debugging
6. **Testing**: Test all authentication types in your integration tests

## Related Components

- **Configuration Management**: For storing authentication settings securely
- **HTTP Client Factories**: For creating authenticated HTTP clients
- **Middleware Components**: For authentication pipeline integration
- **Security Services**: For token validation and credential management

## See Also

- [Enums System Overview](README.md)
- [Security Configuration Patterns](../Patterns/Security.md)
- [HTTP Client Authentication](../Patterns/HttpClientAuth.md)