# CertificateModel

The `CertificateModel` class is a utility wrapper in the RapidStreamer BuildingBlocks library that provides a convenient and flexible way to load X.509 certificates from various sources with automatic certificate generation and cross-framework compatibility.

## Purpose

This class is designed to:
- Simplify X.509 certificate loading from files or raw data
- Provide automatic certificate regeneration when properties change
- Support both password-protected and unprotected certificates
- Offer cross-framework compatibility between .NET versions
- Handle different certificate storage flags for security requirements

## Key Features

- **Multiple Input Sources**: Load certificates from file paths or raw byte arrays
- **Automatic Regeneration**: Certificates are automatically regenerated when any property changes
- **Password Support**: Handle password-protected certificates (PKCS#12)
- **Storage Flags**: Configure X509KeyStorageFlags for specific security requirements
- **Framework Compatibility**: Uses modern .NET 9+ APIs when available, falls back to legacy APIs
- **Thread-Safe Properties**: Property setters trigger immediate certificate regeneration

## Properties

### Path
- **Type**: `string?`
- **Description**: File path to the certificate file
- **Behavior**: Setting this property triggers automatic certificate loading

### RawData
- **Type**: `byte[]?`
- **Description**: Raw certificate data as a byte array
- **Behavior**: Setting this property triggers automatic certificate loading

### Passphrase
- **Type**: `string?`
- **Description**: Password for password-protected certificates (PKCS#12 format)
- **Behavior**: Setting this property triggers certificate regeneration

### KeyStorageFlags
- **Type**: `X509KeyStorageFlags?`
- **Description**: Defines how and where the private key should be stored
- **Behavior**: Setting this property triggers certificate regeneration

### Certificate
- **Type**: `X509Certificate2?`
- **Access**: Read-only
- **Description**: The loaded X.509 certificate instance

## Usage Examples

### Loading Certificate from File

```csharp
using RapidStreamer.BuildingBlocks.Application.Certificate;

// Load a simple certificate file
var certModel = new CertificateModel
{
    Path = @"C:\certificates\mycert.crt"
};

// Access the loaded certificate
X509Certificate2? certificate = certModel.Certificate;
```

### Loading Password-Protected Certificate

```csharp
// Load a PKCS#12 certificate with password
var certModel = new CertificateModel
{
    Path = @"C:\certificates\mycert.p12",
    Passphrase = "mySecurePassword"
};

// Certificate is automatically loaded with the provided password
var certificate = certModel.Certificate;
```

### Loading Certificate from Raw Data

```csharp
// Load certificate from byte array (e.g., from database or API)
byte[] certificateData = GetCertificateDataFromDatabase();

var certModel = new CertificateModel
{
    RawData = certificateData,
    Passphrase = "password123" // If the certificate is password-protected
};

var certificate = certModel.Certificate;
```

### Using Key Storage Flags

```csharp
// Load certificate with specific storage requirements
var certModel = new CertificateModel
{
    Path = @"C:\certificates\server.pfx",
    Passphrase = "serverPassword",
    KeyStorageFlags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
};

// Certificate will be stored in machine key store and persisted
var certificate = certModel.Certificate;
```

### Dynamic Certificate Updates

```csharp
var certModel = new CertificateModel();

// Initially load from file
certModel.Path = @"C:\certs\cert1.crt";
Console.WriteLine($"Certificate 1: {certModel.Certificate?.Subject}");

// Change to different certificate - automatically regenerates
certModel.Path = @"C:\certs\cert2.crt";
Console.WriteLine($"Certificate 2: {certModel.Certificate?.Subject}");

// Switch to raw data source
certModel.Path = null; // Clear file path
certModel.RawData = File.ReadAllBytes(@"C:\certs\cert3.der");
Console.WriteLine($"Certificate 3: {certModel.Certificate?.Subject}");
```

## Framework Compatibility

The `CertificateModel` class adapts to different .NET framework versions:

### .NET 9.0 and Later
Uses the modern `X509CertificateLoader` API for improved performance and security:
```csharp
// From file
X509CertificateLoader.LoadPkcs12FromFile(path, password, keyStorageFlags);
X509CertificateLoader.LoadCertificateFromFile(path);

// From raw data
X509CertificateLoader.LoadPkcs12(rawData, password, keyStorageFlags);
X509CertificateLoader.LoadCertificate(rawData);
```

### Earlier .NET Versions
Falls back to the traditional `X509Certificate2` constructor:
```csharp
// From file
new X509Certificate2(path, password, keyStorageFlags);
new X509Certificate2(path);

// From raw data
new X509Certificate2(rawData, password, keyStorageFlags);
new X509Certificate2(rawData);
```

## Implementation Details

### Automatic Regeneration Logic

The class uses property setters to trigger automatic certificate loading:

```csharp
public string? Path
{
    get => _path;
    set
    {
        _path = value;
        GenerateCertificate(); // Automatic regeneration
    }
}
```

### Certificate Loading Priority

1. **File Path**: If `Path` is provided and valid, load from file
2. **Raw Data**: If `Path` is null/empty but `RawData` is available, load from byte array
3. **No Source**: If neither source is available, set `Certificate` to null

### Loading Logic Flow

```csharp
private void GenerateCertificate()
{
    // Priority 1: Load from file path
    if (!string.IsNullOrWhiteSpace(Path))
    {
        // Load certificate from file with appropriate method
        // Consider passphrase and key storage flags
        return;
    }

    // Priority 2: Load from raw data
    if (RawData is { Length: > 0 })
    {
        // Load certificate from byte array
        // Consider passphrase and key storage flags
        return;
    }

    // No valid source available
    Certificate = null;
}
```

## Common Use Cases

### HTTPS Server Configuration

```csharp
// Load server certificate for HTTPS
var serverCert = new CertificateModel
{
    Path = @"C:\certificates\server.pfx",
    Passphrase = Environment.GetEnvironmentVariable("SERVER_CERT_PASSWORD"),
    KeyStorageFlags = X509KeyStorageFlags.MachineKeySet
};

// Use in web server configuration
services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        https.ServerCertificate = serverCert.Certificate;
    });
});
```

### Client Certificate Authentication

```csharp
// Load client certificate for mutual TLS
var clientCert = new CertificateModel
{
    Path = @"C:\certificates\client.p12",
    Passphrase = "clientPassword",
    KeyStorageFlags = X509KeyStorageFlags.UserKeySet
};

// Use in HTTP client
var handler = new HttpClientHandler();
if (clientCert.Certificate != null)
{
    handler.ClientCertificates.Add(clientCert.Certificate);
}
var httpClient = new HttpClient(handler);
```

### Certificate Validation

```csharp
var certModel = new CertificateModel { Path = certificatePath };
var certificate = certModel.Certificate;

if (certificate != null)
{
    // Check certificate validity
    Console.WriteLine($"Subject: {certificate.Subject}");
    Console.WriteLine($"Issuer: {certificate.Issuer}");
    Console.WriteLine($"Valid From: {certificate.NotBefore}");
    Console.WriteLine($"Valid To: {certificate.NotAfter}");
    Console.WriteLine($"Has Private Key: {certificate.HasPrivateKey}");
    
    // Validate certificate chain
    using var chain = new X509Chain();
    bool isValid = chain.Build(certificate);
    Console.WriteLine($"Certificate Chain Valid: {isValid}");
}
```

## Best Practices

### Security Considerations

✅ **Recommended practices:**
- Store certificate passwords in secure configuration (environment variables, Azure Key Vault)
- Use appropriate `X509KeyStorageFlags` for your security requirements
- Validate certificates before use in production
- Handle certificate expiration and renewal
- Use machine key store for server applications, user key store for client applications

### Performance Optimization

✅ **Performance tips:**
- Reuse `CertificateModel` instances when possible
- Be aware that property changes trigger certificate regeneration
- Consider caching certificates for high-frequency operations
- Dispose of certificates properly when no longer needed

### Error Handling

```csharp
try
{
    var certModel = new CertificateModel
    {
        Path = certificatePath,
        Passphrase = password
    };

    if (certModel.Certificate == null)
    {
        throw new InvalidOperationException("Failed to load certificate");
    }

    // Use certificate
    var certificate = certModel.Certificate;
}
catch (CryptographicException ex)
{
    // Handle certificate loading errors (invalid format, wrong password, etc.)
    Console.WriteLine($"Certificate error: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    // Handle missing certificate file
    Console.WriteLine($"Certificate file not found: {ex.Message}");
}
```

### Configuration Examples

#### appsettings.json Configuration

```json
{
  "Certificates": {
    "Server": {
      "Path": "certificates/server.pfx",
      "Password": "ServerCertPassword"
    },
    "Client": {
      "Path": "certificates/client.p12",
      "Password": "ClientCertPassword"
    }
  }
}
```

#### Configuration Class

```csharp
public class CertificateConfiguration
{
    public string? Path { get; set; }
    public string? Password { get; set; }
    public X509KeyStorageFlags KeyStorageFlags { get; set; } = X509KeyStorageFlags.DefaultKeySet;
}

// Usage
var config = configuration.GetSection("Certificates:Server").Get<CertificateConfiguration>();
var certModel = new CertificateModel
{
    Path = config?.Path,
    Passphrase = config?.Password,
    KeyStorageFlags = config?.KeyStorageFlags
};
```

## Common X509KeyStorageFlags

| Flag | Description | Use Case |
|------|-------------|----------|
| `DefaultKeySet` | Default behavior | General purpose |
| `UserKeySet` | Store in user profile | Client applications |
| `MachineKeySet` | Store in machine store | Server applications, services |
| `PersistKeySet` | Persist keys after use | Long-running applications |
| `Exportable` | Allow private key export | Key backup scenarios |
| `EphemeralKeySet` | Don't persist keys | Temporary operations |

## Troubleshooting

### Common Issues

1. **Certificate is null after setting properties**
   - Check file path validity
   - Verify certificate format (PEM, DER, PKCS#12)
   - Ensure correct password for encrypted certificates

2. **CryptographicException when loading**
   - Incorrect password
   - Corrupted certificate file
   - Unsupported certificate format

3. **Access denied errors**
   - Insufficient permissions to certificate file
   - Key store access issues
   - Use appropriate `X509KeyStorageFlags`

## Related Components

- [System.Security.Cryptography.X509Certificates](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates) - Underlying certificate framework
- [X509Certificate2](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2) - Core certificate class
- [X509CertificateLoader](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificateloader) - Modern .NET 9+ loader API