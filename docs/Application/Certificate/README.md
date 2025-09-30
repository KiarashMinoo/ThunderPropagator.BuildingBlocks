# Certificate

## Overview

The Certificate namespace provides comprehensive certificate management functionality for RapidStreamer BuildingBlocks applications. This component handles X.509 certificate operations, validation, storage, and integration with cryptographic operations across different platforms and environments.

## Components

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **CertificateModel** | Certificate data model | X.509 certificate representation, validation, metadata |

## Purpose

- **Certificate Management**: Store and manage X.509 certificates and related metadata
- **Validation**: Validate certificate chains, expiration, and trust relationships
- **Integration**: Seamless integration with authentication and encryption services
- **Cross-Platform**: Support for Windows, Linux, and macOS certificate stores

## Quick Start

### Basic Certificate Usage
```csharp
using RapidStreamer.BuildingBlocks.Application.Certificate;

// Load certificate from various sources
var certModel = new CertificateModel
{
    Certificate = LoadCertificateFromFile("certificate.pfx", "password"),
    FriendlyName = "My Application Certificate",
    Purpose = CertificatePurpose.Authentication | CertificatePurpose.Encryption
};

// Validate certificate
if (certModel.IsValid)
{
    Console.WriteLine($"Certificate '{certModel.FriendlyName}' is valid until {certModel.ExpirationDate}");
}
```

### Certificate Loading and Validation
```csharp
public class CertificateService
{
    public CertificateModel LoadFromStore(string thumbprint, StoreLocation location = StoreLocation.CurrentUser)
    {
        using var store = new X509Store(StoreName.My, location);
        store.Open(OpenFlags.ReadOnly);
        
        var certificate = store.Certificates
            .Find(X509FindType.FindByThumbprint, thumbprint, false)
            .OfType<X509Certificate2>()
            .FirstOrDefault();
        
        if (certificate == null)
            throw new CertificateNotFoundException($"Certificate with thumbprint {thumbprint} not found");
        
        return new CertificateModel
        {
            Certificate = certificate,
            FriendlyName = certificate.FriendlyName ?? certificate.Subject,
            Thumbprint = certificate.Thumbprint,
            IssuedBy = certificate.Issuer,
            IssuedTo = certificate.Subject,
            ExpirationDate = certificate.NotAfter,
            Purpose = DetermineCertificatePurpose(certificate)
        };
    }
    
    public CertificateModel LoadFromFile(string filePath, string password = null)
    {
        try
        {
            var certificate = string.IsNullOrEmpty(password) 
                ? new X509Certificate2(filePath)
                : new X509Certificate2(filePath, password, X509KeyStorageFlags.Exportable);
            
            return new CertificateModel
            {
                Certificate = certificate,
                FriendlyName = Path.GetFileNameWithoutExtension(filePath),
                Thumbprint = certificate.Thumbprint,
                IssuedBy = certificate.Issuer,
                IssuedTo = certificate.Subject,
                ExpirationDate = certificate.NotAfter,
                FilePath = filePath,
                Purpose = DetermineCertificatePurpose(certificate)
            };
        }
        catch (Exception ex)
        {
            throw new CertificateLoadException($"Failed to load certificate from {filePath}", ex);
        }
    }
    
    private CertificatePurpose DetermineCertificatePurpose(X509Certificate2 certificate)
    {
        var purpose = CertificatePurpose.None;
        
        // Check key usage extensions
        foreach (var extension in certificate.Extensions)
        {
            if (extension is X509KeyUsageExtension keyUsage)
            {
                if (keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature))
                    purpose |= CertificatePurpose.Authentication;
                
                if (keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.KeyEncipherment) ||
                    keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.DataEncipherment))
                    purpose |= CertificatePurpose.Encryption;
                
                if (keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.NonRepudiation))
                    purpose |= CertificatePurpose.Signing;
            }
        }
        
        return purpose == CertificatePurpose.None ? CertificatePurpose.General : purpose;
    }
}

[Flags]
public enum CertificatePurpose
{
    None = 0,
    Authentication = 1,
    Encryption = 2,
    Signing = 4,
    General = 8
}

public class CertificateNotFoundException : Exception
{
    public CertificateNotFoundException(string message) : base(message) { }
    public CertificateNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class CertificateLoadException : Exception
{
    public CertificateLoadException(string message) : base(message) { }
    public CertificateLoadException(string message, Exception innerException) : base(message, innerException) { }
}
```

## Integration Examples

### ASP.NET Core Authentication
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Certificate-based authentication
        services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options =>
            {
                options.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        var certModel = new CertificateModel
                        {
                            Certificate = context.ClientCertificate,
                            FriendlyName = context.ClientCertificate.Subject
                        };
                        
                        if (!certModel.IsValid)
                        {
                            context.Fail("Certificate validation failed");
                            return Task.CompletedTask;
                        }
                        
                        // Create claims based on certificate
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, certModel.Thumbprint),
                            new Claim(ClaimTypes.Name, certModel.IssuedTo),
                            new Claim("certificate_thumbprint", certModel.Thumbprint)
                        };
                        
                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
                        
                        return Task.CompletedTask;
                    }
                };
            });
        
        services.AddSingleton<CertificateService>();
        services.AddScoped<ICertificateValidator, CertificateValidator>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecureController : ControllerBase
{
    private readonly ICertificateValidator _certificateValidator;
    
    public SecureController(ICertificateValidator certificateValidator)
    {
        _certificateValidator = certificateValidator;
    }
    
    [HttpGet("certificate-info")]
    public ActionResult<CertificateInfo> GetCertificateInfo()
    {
        var thumbprint = User.FindFirst("certificate_thumbprint")?.Value;
        if (string.IsNullOrEmpty(thumbprint))
            return BadRequest("No certificate information found");
        
        var certModel = _certificateValidator.GetCertificateByThumbprint(thumbprint);
        
        return Ok(new CertificateInfo
        {
            Subject = certModel.IssuedTo,
            Issuer = certModel.IssuedBy,
            ExpirationDate = certModel.ExpirationDate,
            IsValid = certModel.IsValid,
            Purpose = certModel.Purpose.ToString()
        });
    }
}

public class CertificateInfo
{
    public string Subject { get; set; }
    public string Issuer { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsValid { get; set; }
    public string Purpose { get; set; }
}
```

### HTTPS Client Configuration
```csharp
public class SecureHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly CertificateService _certificateService;
    
    public SecureHttpClientService(CertificateService certificateService)
    {
        _certificateService = certificateService;
        _httpClient = CreateSecureHttpClient();
    }
    
    private HttpClient CreateSecureHttpClient()
    {
        var handler = new HttpClientHandler();
        
        // Load client certificate for mutual TLS
        var clientCert = _certificateService.LoadFromStore("client_certificate_thumbprint");
        if (clientCert.IsValid && clientCert.HasPrivateKey)
        {
            handler.ClientCertificates.Add(clientCert.Certificate);
        }
        
        // Custom server certificate validation
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (errors == SslPolicyErrors.None)
                return true;
            
            // Custom validation logic
            var serverCertModel = new CertificateModel
            {
                Certificate = new X509Certificate2(cert.GetRawCertData()),
                FriendlyName = "Server Certificate"
            };
            
            return ValidateServerCertificate(serverCertModel, errors);
        };
        
        return new HttpClient(handler);
    }
    
    private bool ValidateServerCertificate(CertificateModel certificate, SslPolicyErrors errors)
    {
        // Custom server certificate validation logic
        if (!certificate.IsValid)
        {
            Console.WriteLine($"Server certificate is invalid: {certificate.ValidationErrors}");
            return false;
        }
        
        // Check if certificate is from trusted CA
        if (errors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
        {
            // Implement custom chain validation
            return ValidateCertificateChain(certificate);
        }
        
        return true;
    }
    
    private bool ValidateCertificateChain(CertificateModel certificate)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
        
        bool isValid = chain.Build(certificate.Certificate);
        
        if (!isValid)
        {
            foreach (var status in chain.ChainStatus)
            {
                Console.WriteLine($"Chain validation error: {status.Status} - {status.StatusInformation}");
            }
        }
        
        return isValid;
    }
    
    public async Task<string> GetSecureDataAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new SecureHttpException($"Secure request failed: {ex.Message}", ex);
        }
    }
}

public class SecureHttpException : Exception
{
    public SecureHttpException(string message) : base(message) { }
    public SecureHttpException(string message, Exception innerException) : base(message, innerException) { }
}
```

### Certificate Monitoring Service
```csharp
public class CertificateMonitoringService : BackgroundService
{
    private readonly CertificateService _certificateService;
    private readonly ILogger<CertificateMonitoringService> _logger;
    private readonly INotificationService _notificationService;
    private readonly CertificateMonitoringOptions _options;
    
    public CertificateMonitoringService(
        CertificateService certificateService,
        ILogger<CertificateMonitoringService> logger,
        INotificationService notificationService,
        IOptions<CertificateMonitoringOptions> options)
    {
        _certificateService = certificateService;
        _logger = logger;
        _notificationService = notificationService;
        _options = options.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckCertificatesAsync();
                await Task.Delay(_options.CheckInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during certificate monitoring");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry after 5 minutes
            }
        }
    }
    
    private async Task CheckCertificatesAsync()
    {
        var certificates = await GetMonitoredCertificatesAsync();
        
        foreach (var cert in certificates)
        {
            await CheckCertificateAsync(cert);
        }
    }
    
    private async Task<List<CertificateModel>> GetMonitoredCertificatesAsync()
    {
        var certificates = new List<CertificateModel>();
        
        // Check certificates from store
        foreach (var thumbprint in _options.MonitoredCertificates)
        {
            try
            {
                var cert = _certificateService.LoadFromStore(thumbprint);
                certificates.Add(cert);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load certificate {Thumbprint}", thumbprint);
            }
        }
        
        // Check certificates from files
        foreach (var filePath in _options.MonitoredCertificateFiles)
        {
            try
            {
                var cert = _certificateService.LoadFromFile(filePath.Path, filePath.Password);
                certificates.Add(cert);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load certificate from {FilePath}", filePath.Path);
            }
        }
        
        return certificates;
    }
    
    private async Task CheckCertificateAsync(CertificateModel certificate)
    {
        var alerts = new List<CertificateAlert>();
        
        // Check expiration
        var daysUntilExpiration = (certificate.ExpirationDate - DateTime.UtcNow).TotalDays;
        
        if (daysUntilExpiration <= 0)
        {
            alerts.Add(new CertificateAlert
            {
                Type = CertificateAlertType.Expired,
                Severity = AlertSeverity.Critical,
                Message = $"Certificate '{certificate.FriendlyName}' has expired",
                Certificate = certificate
            });
        }
        else if (daysUntilExpiration <= _options.CriticalExpirationDays)
        {
            alerts.Add(new CertificateAlert
            {
                Type = CertificateAlertType.ExpiringCritical,
                Severity = AlertSeverity.Critical,
                Message = $"Certificate '{certificate.FriendlyName}' expires in {daysUntilExpiration:F0} days",
                Certificate = certificate
            });
        }
        else if (daysUntilExpiration <= _options.WarningExpirationDays)
        {
            alerts.Add(new CertificateAlert
            {
                Type = CertificateAlertType.ExpiringWarning,
                Severity = AlertSeverity.Warning,
                Message = $"Certificate '{certificate.FriendlyName}' expires in {daysUntilExpiration:F0} days",
                Certificate = certificate
            });
        }
        
        // Check validity
        if (!certificate.IsValid)
        {
            alerts.Add(new CertificateAlert
            {
                Type = CertificateAlertType.Invalid,
                Severity = AlertSeverity.Critical,
                Message = $"Certificate '{certificate.FriendlyName}' is invalid: {certificate.ValidationErrors}",
                Certificate = certificate
            });
        }
        
        // Send alerts
        foreach (var alert in alerts)
        {
            await _notificationService.SendAlertAsync(alert);
            _logger.LogWarning("Certificate alert: {Message}", alert.Message);
        }
    }
}

public class CertificateMonitoringOptions
{
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromHours(6);
    public int WarningExpirationDays { get; set; } = 30;
    public int CriticalExpirationDays { get; set; } = 7;
    public List<string> MonitoredCertificates { get; set; } = new();
    public List<CertificateFileInfo> MonitoredCertificateFiles { get; set; } = new();
}

public class CertificateFileInfo
{
    public string Path { get; set; }
    public string Password { get; set; }
}

public class CertificateAlert
{
    public CertificateAlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; }
    public CertificateModel Certificate { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum CertificateAlertType
{
    Expired,
    ExpiringCritical,
    ExpiringWarning,
    Invalid,
    ChainValidationFailed
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public interface INotificationService
{
    Task SendAlertAsync(CertificateAlert alert);
}
```

## Advanced Usage Patterns

### Certificate Repository Pattern
```csharp
public interface ICertificateRepository
{
    Task<CertificateModel> GetByThumbprintAsync(string thumbprint);
    Task<CertificateModel> GetByFriendlyNameAsync(string friendlyName);
    Task<IEnumerable<CertificateModel>> GetByPurposeAsync(CertificatePurpose purpose);
    Task<IEnumerable<CertificateModel>> GetExpiringCertificatesAsync(DateTime before);
    Task AddOrUpdateAsync(CertificateModel certificate);
    Task RemoveAsync(string thumbprint);
}

public class CertificateRepository : ICertificateRepository
{
    private readonly IMemoryCache _cache;
    private readonly CertificateService _certificateService;
    private readonly ILogger<CertificateRepository> _logger;
    
    public CertificateRepository(
        IMemoryCache cache,
        CertificateService certificateService,
        ILogger<CertificateRepository> logger)
    {
        _cache = cache;
        _certificateService = certificateService;
        _logger = logger;
    }
    
    public async Task<CertificateModel> GetByThumbprintAsync(string thumbprint)
    {
        var cacheKey = $"cert_thumbprint_{thumbprint}";
        
        if (_cache.TryGetValue(cacheKey, out CertificateModel cached))
        {
            return cached;
        }
        
        try
        {
            var certificate = _certificateService.LoadFromStore(thumbprint);
            _cache.Set(cacheKey, certificate, TimeSpan.FromHours(1));
            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate by thumbprint {Thumbprint}", thumbprint);
            throw;
        }
    }
    
    public async Task<IEnumerable<CertificateModel>> GetExpiringCertificatesAsync(DateTime before)
    {
        var certificates = new List<CertificateModel>();
        
        // Scan certificate stores
        var stores = new[] { StoreName.My, StoreName.Root, StoreName.CertificateAuthority };
        var locations = new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
        
        foreach (var location in locations)
        {
            foreach (var storeName in stores)
            {
                try
                {
                    using var store = new X509Store(storeName, location);
                    store.Open(OpenFlags.ReadOnly);
                    
                    foreach (var cert in store.Certificates)
                    {
                        if (cert.NotAfter <= before)
                        {
                            var model = new CertificateModel
                            {
                                Certificate = cert,
                                FriendlyName = cert.FriendlyName ?? cert.Subject,
                                Thumbprint = cert.Thumbprint,
                                ExpirationDate = cert.NotAfter
                            };
                            certificates.Add(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scan certificate store {Store} in {Location}", storeName, location);
                }
            }
        }
        
        return certificates;
    }
}
```

## Performance Considerations

### Certificate Caching
- **Memory Caching**: Cache frequently accessed certificates to avoid repeated store access
- **Validation Caching**: Cache validation results for a reasonable time period
- **Store Optimization**: Minimize certificate store access operations

### Security Best Practices
- **Private Key Protection**: Ensure private keys are properly protected and not exposed
- **Certificate Validation**: Always validate certificates before use
- **Secure Storage**: Use secure storage mechanisms for sensitive certificate data
- **Access Control**: Implement proper access controls for certificate operations

## Related Components

- **[Ciphering](../Ciphering/README.md)** - Encryption and cryptographic services
- **[Identity](../Identity/README.md)** - Authentication and identity management
- **[Helpers](../Helpers/README.md)** - Utility helpers for cryptographic operations
- **[Application Overview](../README.md)** - Complete application building blocks documentation

## Troubleshooting

### Common Issues

#### Certificate Store Access Denied
```csharp
// Problem: Insufficient permissions to access certificate store
try
{
    var cert = certificateService.LoadFromStore(thumbprint, StoreLocation.LocalMachine);
}
catch (CryptographicException ex) when (ex.Message.Contains("Access is denied"))
{
    // Solution: Try current user store or run with elevated permissions
    var cert = certificateService.LoadFromStore(thumbprint, StoreLocation.CurrentUser);
}
```

#### Certificate Chain Validation Failures
```csharp
public bool ValidateCertificateChain(X509Certificate2 certificate, bool allowSelfSigned = false)
{
    using var chain = new X509Chain();
    
    // Configure chain validation options
    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
    
    if (allowSelfSigned)
    {
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
    }
    
    bool isValid = chain.Build(certificate);
    
    if (!isValid)
    {
        foreach (var status in chain.ChainStatus)
        {
            _logger.LogWarning("Certificate chain validation failed: {Status} - {StatusInformation}", 
                status.Status, status.StatusInformation);
        }
    }
    
    return isValid;
}
```