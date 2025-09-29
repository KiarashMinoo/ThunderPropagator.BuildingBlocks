# RsaEncryptionService

The `RsaEncryptionService` class and `RsaEncryptionHelper` static utility provide comprehensive RSA (Rivest-Shamir-Adleman) asymmetric encryption functionality in the RapidStreamer BuildingBlocks library. This system supports key generation, multiple key formats (PEM, raw binary, RSAParameters), and secure encryption/decryption operations.

## Purpose

This service provides:
- **RSA Asymmetric Encryption**: Public-key cryptography for secure data exchange
- **Multiple Key Formats**: Support for PEM, binary, and RSAParameters formats
- **Key Generation**: Secure RSA key pair generation with configurable key sizes
- **Flexible API**: Both static utility methods and instance-based service
- **Guard Clause Integration**: Built-in input validation and error handling
- **Cross-Platform Compatibility**: Works across different .NET implementations

## Key Features

- **Multiple Key Formats**: PEM strings, raw bytes, RSAParameters objects
- **Configurable Key Sizes**: Support for different RSA key lengths (512+ bits)
- **Static and Instance APIs**: Choose between utility methods or service instances
- **Automatic Key Management**: Instance-based service manages key pairs internally
- **Base64 Encoding**: Safe encoding for encrypted data transport
- **Comprehensive Validation**: Input validation using Ardalis.GuardClauses

## Classes and Components

### RsaEncryptionHelper (Static Utility)

Provides static methods for key generation and RSA parameter handling:

```csharp
public static class RsaEncryptionHelper
{
    public static (string PrivateKey, string PublicKey) GeneratePemCodes(int dwKeySize = 512);
    public static (byte[] PrivateKey, byte[] PublicKey) GenerateRsaCodes(int dwKeySize = 512);
    public static string ToXmlString(this RSAParameters key);
    public static string ToJsonString(this RSAParameters key);
}
```

### RsaEncryptionService (Instance-Based Service)

Main service class that encapsulates RSA key pairs and provides encryption operations:

```csharp
public class RsaEncryptionService
{
    public RSAParameters PrivateKey { get; }
    public RSAParameters PublicKey { get; }
    
    // Multiple constructors for different initialization methods
    public RsaEncryptionService(int dwKeySize = 512);
    public RsaEncryptionService(RSAParameters privateKey, RSAParameters publicKey, int dwKeySize = 512);
    public RsaEncryptionService(string privateKey, string publicKey, int dwKeySize = 512);
}
```

## Usage Examples

### Basic Key Generation and Encryption

```csharp
using RapidStreamer.BuildingBlocks.Application.Ciphering;

// Generate RSA key pair and create service instance
var rsaService = new RsaEncryptionService(2048); // 2048-bit keys for security

// Encrypt data using the service
string plainText = "This is sensitive information";
string encrypted = rsaService.Encrypt(plainText);
Console.WriteLine($"Encrypted: {encrypted}");

// Decrypt data using the service
string decrypted = rsaService.Decrypt(encrypted);
Console.WriteLine($"Decrypted: {decrypted}");
// Output: "This is sensitive information"
```

### Static Methods for One-off Operations

```csharp
// Generate key pairs using helper methods
var (privateKeyPem, publicKeyPem) = RsaEncryptionHelper.GeneratePemCodes(2048);
var (privateKeyBytes, publicKeyBytes) = RsaEncryptionHelper.GenerateRsaCodes(2048);

// Static encryption without service instance
string plainText = "Secret message";
string encrypted = RsaEncryptionService.Encrypt(plainText, publicKeyPem, 2048);

// Static decryption
string decrypted = RsaEncryptionService.Decrypt(encrypted, privateKeyPem, 2048);
Console.WriteLine($"Result: {decrypted}"); // "Secret message"
```

### Working with Different Key Formats

```csharp
// PEM format keys
var (privatePem, publicPem) = RsaEncryptionHelper.GeneratePemCodes(2048);
Console.WriteLine($"Public Key PEM:\n{publicPem}");

// Binary format keys
var (privateBytes, publicBytes) = RsaEncryptionHelper.GenerateRsaCodes(2048);
Console.WriteLine($"Private key size: {privateBytes.Length} bytes");

// RSAParameters format
var (privateParams, publicParams) = RsaEncryptionService.GenerateKeys(2048);

// Use different formats for encryption
string message = "Test message";

// Using PEM
string encryptedPem = RsaEncryptionService.Encrypt(message, publicPem, 2048);

// Using binary
string encryptedBytes = RsaEncryptionService.Encrypt(message, publicBytes, 2048);

// Using RSAParameters
string encryptedParams = RsaEncryptionService.Encrypt(message, publicParams, 2048);
```

### Service Instance with Pre-existing Keys

```csharp
// Load existing keys from storage/configuration
string existingPrivateKey = LoadPrivateKeyFromSecureStorage();
string existingPublicKey = LoadPublicKeyFromConfiguration();

// Create service with existing keys
var rsaService = new RsaEncryptionService(existingPrivateKey, existingPublicKey, 2048);

// Use the service normally
string encrypted = rsaService.Encrypt("Important data");
string decrypted = rsaService.Decrypt(encrypted);
```

## Real-World Applications

### Secure API Communication

```csharp
public class SecureApiClient
{
    private readonly RsaEncryptionService _rsaService;
    private readonly HttpClient _httpClient;
    
    public SecureApiClient(string serverPublicKey)
    {
        // Client generates its own key pair
        _rsaService = new RsaEncryptionService(2048);
        _httpClient = new HttpClient();
        
        // Store server's public key for encrypting requests
        ServerPublicKey = serverPublicKey;
    }
    
    public string ServerPublicKey { get; }
    public string ClientPublicKey => RsaEncryptionHelper.GeneratePemCodes(2048).PublicKey;
    
    public async Task<string> SendSecureRequest(string endpoint, object data)
    {
        // Serialize and encrypt the request data
        string jsonData = System.Text.Json.JsonSerializer.Serialize(data);
        string encryptedData = RsaEncryptionService.Encrypt(jsonData, ServerPublicKey, 2048);
        
        // Send encrypted request
        var request = new { EncryptedData = encryptedData, ClientPublicKey };
        var response = await _httpClient.PostAsJsonAsync(endpoint, request);
        
        // Decrypt response
        var responseData = await response.Content.ReadFromJsonAsync<SecureResponse>();
        return _rsaService.Decrypt(responseData.EncryptedResponse);
    }
}

public class SecureApiServer
{
    private readonly RsaEncryptionService _rsaService;
    
    public SecureApiServer()
    {
        _rsaService = new RsaEncryptionService(2048);
    }
    
    public string PublicKey => RsaEncryptionHelper.GeneratePemCodes(2048).PublicKey;
    
    public string ProcessSecureRequest(string encryptedData, string clientPublicKey)
    {
        // Decrypt client request
        string decryptedData = _rsaService.Decrypt(encryptedData);
        
        // Process the request
        var processedResult = ProcessBusinessLogic(decryptedData);
        
        // Encrypt response with client's public key
        string encryptedResponse = RsaEncryptionService.Encrypt(
            processedResult, clientPublicKey, 2048);
            
        return encryptedResponse;
    }
}
```

### Digital Document Signing

```csharp
public class DocumentSigningService
{
    private readonly RsaEncryptionService _signingService;
    
    public DocumentSigningService()
    {
        // Generate signing key pair
        _signingService = new RsaEncryptionService(4096); // Larger key for signing
    }
    
    public string PublicKey => _signingService.PublicKey.ToJsonString();
    
    public SignedDocument SignDocument(string documentContent)
    {
        // Create document hash
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] documentHash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(documentContent));
        string hashString = Convert.ToBase64String(documentHash);
        
        // Sign the hash (encrypt with private key)
        string signature = _signingService.Encrypt(hashString);
        
        return new SignedDocument
        {
            Content = documentContent,
            Signature = signature,
            SigningKey = PublicKey,
            SignedAt = DateTime.UtcNow
        };
    }
    
    public bool VerifyDocument(SignedDocument signedDoc)
    {
        try
        {
            // Recreate document hash
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] documentHash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signedDoc.Content));
            string expectedHash = Convert.ToBase64String(documentHash);
            
            // Verify signature (decrypt with public key)
            var publicKey = System.Text.Json.JsonSerializer.Deserialize<RSAParameters>(signedDoc.SigningKey);
            string decryptedHash = RsaEncryptionService.Decrypt(signedDoc.Signature, publicKey, 4096);
            
            return expectedHash == decryptedHash;
        }
        catch
        {
            return false;
        }
    }
}

public class SignedDocument
{
    public string Content { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
}
```

### Key Exchange System

```csharp
public class KeyExchangeService
{
    private readonly RsaEncryptionService _rsaService;
    private readonly Dictionary<string, string> _sessionKeys = new();
    
    public KeyExchangeService()
    {
        _rsaService = new RsaEncryptionService(2048);
    }
    
    public string PublicKey => _rsaService.PublicKey.ToJsonString();
    
    public string EstablishSecureSession(string clientId, string clientPublicKey)
    {
        // Generate a session key for symmetric encryption
        string sessionKey = PasswordGenerator.Generate(32, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;
        });
        
        // Store session key
        _sessionKeys[clientId] = sessionKey;
        
        // Encrypt session key with client's public key
        var clientKey = System.Text.Json.JsonSerializer.Deserialize<RSAParameters>(clientPublicKey);
        string encryptedSessionKey = RsaEncryptionService.Encrypt(sessionKey, clientKey, 2048);
        
        return encryptedSessionKey;
    }
    
    public string GetSessionKey(string clientId)
    {
        return _sessionKeys.TryGetValue(clientId, out string? key) ? key : string.Empty;
    }
    
    public void RevokeSession(string clientId)
    {
        _sessionKeys.Remove(clientId);
    }
}
```

### Configuration Encryption

```csharp
public class SecureConfigurationManager
{  
    private readonly string _configPath;
    private RsaEncryptionService? _rsaService;
    
    public SecureConfigurationManager(string configPath)
    {
        _configPath = configPath;
        LoadOrGenerateKeys();
    }
    
    private void LoadOrGenerateKeys()
    {
        string keyPath = Path.ChangeExtension(_configPath, ".key");
        
        if (File.Exists(keyPath))
        {
            // Load existing keys
            var keyData = System.Text.Json.JsonSerializer.Deserialize<KeyStorage>(
                File.ReadAllText(keyPath));
            _rsaService = new RsaEncryptionService(keyData.PrivateKey, keyData.PublicKey, 2048);
        }
        else
        {
            // Generate new keys
            _rsaService = new RsaEncryptionService(2048);
            
            // Save keys securely
            var keyStorage = new KeyStorage
            {
                PrivateKey = _rsaService.PrivateKey.ToJsonString(),
                PublicKey = _rsaService.PublicKey.ToJsonString()
            };
            
            File.WriteAllText(keyPath, System.Text.Json.JsonSerializer.Serialize(keyStorage));
        }
    }
    
    public void SaveSecureSetting(string key, string value)
    {
        var config = LoadConfiguration();
        config[key] = _rsaService!.Encrypt(value);
        SaveConfiguration(config);
    }
    
    public string GetSecureSetting(string key)
    {
        var config = LoadConfiguration();
        if (config.TryGetValue(key, out string? encryptedValue))
        {
            return _rsaService!.Decrypt(encryptedValue);
        }
        return string.Empty;
    }
    
    private Dictionary<string, string> LoadConfiguration()
    {
        if (!File.Exists(_configPath))
            return new Dictionary<string, string>();
            
        var json = File.ReadAllText(_configPath);
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
               ?? new Dictionary<string, string>();
    }
    
    private void SaveConfiguration(Dictionary<string, string> config)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }
}

public class KeyStorage
{
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}
```

## Advanced Usage Patterns

### Hybrid Encryption (RSA + AES)

```csharp
public class HybridEncryptionService
{
    private readonly RsaEncryptionService _rsaService;
    
    public HybridEncryptionService()
    {
        _rsaService = new RsaEncryptionService(2048);
    }
    
    public string PublicKey => _rsaService.PublicKey.ToJsonString();
    
    public HybridEncryptedData Encrypt(string data, string recipientPublicKey)
    {
        // Generate random AES key
        string aesPassword = PasswordGenerator.Generate(32);
        byte[] aesKey = EncryptionService.CreateKey(aesPassword);
        
        // Encrypt data with AES (faster for large data)
        string encryptedData = EncryptionService.Encrypt(data, aesKey);
        
        // Encrypt AES key with RSA (secure key exchange)
        var recipientKey = System.Text.Json.JsonSerializer.Deserialize<RSAParameters>(recipientPublicKey);
        string encryptedKey = RsaEncryptionService.Encrypt(aesPassword, recipientKey, 2048);
        
        return new HybridEncryptedData
        {
            EncryptedContent = encryptedData,
            EncryptedKey = encryptedKey
        };
    }
    
    public string Decrypt(HybridEncryptedData encryptedData)
    {
        // Decrypt AES key with RSA
        string aesPassword = _rsaService.Decrypt(encryptedData.EncryptedKey);
        byte[] aesKey = EncryptionService.CreateKey(aesPassword);
        
        // Decrypt data with AES
        return EncryptionService.Decrypt(encryptedData.EncryptedContent, aesKey);
    }
}

public class HybridEncryptedData
{
    public string EncryptedContent { get; set; } = string.Empty;
    public string EncryptedKey { get; set; } = string.Empty;
}
```

### Multi-Recipient Encryption

```csharp
public class MultiRecipientEncryption
{
    public MultiRecipientData EncryptForMultipleRecipients(string data, IEnumerable<string> recipientPublicKeys)
    {
        // Generate random AES key for the data
        string aesPassword = PasswordGenerator.Generate(32);
        byte[] aesKey = EncryptionService.CreateKey(aesPassword);
        
        // Encrypt data once with AES
        string encryptedData = EncryptionService.Encrypt(data, aesKey);
        
        // Encrypt AES key for each recipient
        var encryptedKeys = new Dictionary<string, string>();
        
        foreach (string publicKeyJson in recipientPublicKeys)
        {
            var publicKey = System.Text.Json.JsonSerializer.Deserialize<RSAParameters>(publicKeyJson);
            string encryptedKey = RsaEncryptionService.Encrypt(aesPassword, publicKey, 2048);
            
            // Use key hash as identifier
            string keyId = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(publicKeyJson)));
            
            encryptedKeys[keyId] = encryptedKey;
        }
        
        return new MultiRecipientData
        {
            EncryptedContent = encryptedData,
            RecipientKeys = encryptedKeys
        };
    }
    
    public string DecryptAsRecipient(MultiRecipientData data, RsaEncryptionService recipientRsa)
    {
        // Find our encrypted key
        string publicKeyJson = recipientRsa.PublicKey.ToJsonString();
        string keyId = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(publicKeyJson)));
        
        if (!data.RecipientKeys.TryGetValue(keyId, out string? encryptedKey))
        {
            throw new UnauthorizedAccessException("This recipient cannot decrypt the data");
        }
        
        // Decrypt our copy of the AES key
        string aesPassword = recipientRsa.Decrypt(encryptedKey);
        byte[] aesKey = EncryptionService.CreateKey(aesPassword);
        
        // Decrypt the data
        return EncryptionService.Decrypt(data.EncryptedContent, aesKey);
    }
}

public class MultiRecipientData
{
    public string EncryptedContent { get; set; } = string.Empty;
    public Dictionary<string, string> RecipientKeys { get; set; } = new();
}
```

## Security Considerations

### Key Size Recommendations

```csharp
public static class RsaKeyRecommendations
{
    public const int MinimumKeySize = 2048;    // Minimum for production
    public const int RecommendedKeySize = 3072; // Recommended for new systems
    public const int HighSecurityKeySize = 4096; // High security applications
    
    public static RsaEncryptionService CreateSecureService(SecurityLevel level = SecurityLevel.Recommended)
    {
        int keySize = level switch
        {
            SecurityLevel.Minimum => MinimumKeySize,
            SecurityLevel.Recommended => RecommendedKeySize,
            SecurityLevel.High => HighSecurityKeySize,
            _ => RecommendedKeySize
        };
        
        return new RsaEncryptionService(keySize);
    }
}

public enum SecurityLevel
{
    Minimum,
    Recommended,
    High
}
```

### Key Storage Best Practices

```csharp
public class SecureKeyStorage
{
    public static void SaveKeySecurely(RSAParameters privateKey, string password, string filePath)
    {
        // Serialize the private key
        string keyJson = privateKey.ToJsonString();
        
        // Encrypt with AES using password
        byte[] encryptionKey = EncryptionService.CreateKey(password, iterations: 100000);
        string encryptedKey = EncryptionService.Encrypt(keyJson, encryptionKey);
        
        // Save encrypted key
        File.WriteAllText(filePath, encryptedKey);
        
        // Set restrictive file permissions (Windows)
        if (OperatingSystem.IsWindows())
        {
            var fileInfo = new FileInfo(filePath);
            var fileSecurity = fileInfo.GetAccessControl();
            fileSecurity.SetAccessRuleProtection(true, false); // Remove inherited permissions
            fileInfo.SetAccessControl(fileSecurity);
        }
    }
    
    public static RSAParameters LoadKeySecurely(string password, string filePath)
    {
        string encryptedKey = File.ReadAllText(filePath);
        
        byte[] encryptionKey = EncryptionService.CreateKey(password, iterations: 100000);
        string keyJson = EncryptionService.Decrypt(encryptedKey, encryptionKey);
        
        return System.Text.Json.JsonSerializer.Deserialize<RSAParameters>(keyJson);
    }
}
```

## Performance Considerations

### Encryption Performance

RSA encryption is slower than symmetric encryption. Use it strategically:

```csharp
public class PerformanceAwareEncryption
{
    // Good: RSA for small data (keys, tokens, short messages)
    public string EncryptToken(string token, string publicKey)
    {
        if (token.Length > 200) // RSA has size limits
            throw new ArgumentException("Token too large for RSA encryption");
            
        return RsaEncryptionService.Encrypt(token, publicKey, 2048);
    }
    
    // Better: Hybrid encryption for large data
    public HybridEncryptedData EncryptLargeData(string data, string publicKey)
    {
        var hybridService = new HybridEncryptionService();
        return hybridService.Encrypt(data, publicKey);
    }
}
```

### Key Generation Performance

Key generation is expensive - cache when possible:

```csharp
public class KeyManager
{
    private static readonly ConcurrentDictionary<int, (RSAParameters Private, RSAParameters Public)> _keyCache = new();
    
    public static (RSAParameters Private, RSAParameters Public) GetOrGenerateKeys(int keySize)
    {
        return _keyCache.GetOrAdd(keySize, size => RsaEncryptionService.GenerateKeys(size));
    }
}
```

## Error Handling

### Comprehensive Error Handling

```csharp
public static class SafeRsaOperations
{
    public static (bool Success, string Result, string Error) TryEncrypt(
        string plainText, string publicKey, int keySize = 2048)
    {
        try
        {
            string result = RsaEncryptionService.Encrypt(plainText, publicKey, keySize);
            return (true, result, string.Empty);
        }
        catch (ArgumentException ex)
        {
            return (false, string.Empty, $"Invalid argument: {ex.Message}");
        }
        catch (CryptographicException ex)
        {
            return (false, string.Empty, $"Encryption failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Unexpected error: {ex.Message}");
        }
    }
    
    public static (bool Success, string Result, string Error) TryDecrypt(
        string cipherText, string privateKey, int keySize = 2048)
    {
        try
        {
            string result = RsaEncryptionService.Decrypt(cipherText, privateKey, keySize);
            return (true, result, string.Empty);
        }
        catch (FormatException ex)
        {
            return (false, string.Empty, $"Invalid format: {ex.Message}");
        }
        catch (CryptographicException ex)
        {
            return (false, string.Empty, $"Decryption failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Unexpected error: {ex.Message}");
        }
    }
}
```

## Implementation Details

### Build Configuration
- In **DEBUG** builds: Classes are not sealed, allowing inheritance for testing
- In **RELEASE** builds: Classes are sealed for performance optimization

### Key Format Support
- **PEM Format**: Standard ASCII armor format for keys
- **Binary Format**: Raw DER-encoded key data
- **RSAParameters**: .NET native key representation
- **JSON/XML**: Serialized RSAParameters for storage

### Validation and Guard Clauses
The service uses Ardalis.GuardClauses for input validation:
- Null/empty string checks
- Key size validation (minimum 512 bits)
- Parameter validation for all public methods

## Best Practices

### Security Guidelines

✅ **Recommended practices:**
- Use minimum 2048-bit keys (3072+ for new systems)
- Store private keys encrypted with strong passwords
- Use RSA for small data or key exchange, not bulk encryption
- Implement proper key rotation policies
- Validate all inputs and handle exceptions gracefully

### Performance Optimization

✅ **Performance tips:**
- Cache key pairs when possible
- Use hybrid encryption for large data
- Consider key size vs. performance trade-offs
- Implement async operations for key generation

### Integration Patterns

✅ **Effective patterns:**
```csharp
// Good: Service registration with DI
services.AddSingleton<RsaEncryptionService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var keySize = config.GetValue<int>("Rsa:KeySize", 2048);
    return new RsaEncryptionService(keySize);
});
```

## Related Components

- [`EncryptionService`](EncryptionService.md) - AES symmetric encryption (faster for large data)
- [`PasswordGenerator`](PasswordGenerator.md) - Generate secure passwords for key derivation
- [System.Security.Cryptography.RSA](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa) - Underlying RSA implementation

## Testing

The service behavior is thoroughly tested in `RsaEncryptionServiceTests.cs`, which verifies:
- Key generation with different sizes
- Encryption/decryption round trips
- Multiple key format handling
- Error conditions and validation
- Performance characteristics
- Integration with guard clauses