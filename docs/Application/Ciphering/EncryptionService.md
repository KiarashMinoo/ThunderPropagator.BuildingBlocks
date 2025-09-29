# EncryptionService

The `EncryptionService` is a static utility class in the RapidStreamer BuildingBlocks library that provides AES (Advanced Encryption Standard) encryption and decryption functionality with PBKDF2 key derivation. This service offers a simple, secure, and standardized approach to symmetric encryption.

## Purpose

This service provides:
- **AES Encryption/Decryption**: Secure symmetric encryption using industry-standard AES algorithm
- **Key Derivation**: PBKDF2-based key generation from passwords with configurable parameters
- **Base64 Encoding**: Automatic encoding of encrypted data for safe transport and storage
- **Zero IV Management**: Uses a zero-filled initialization vector for consistent results
- **Easy Integration**: Simple static methods for encryption operations

## Key Features

- **AES Algorithm**: Uses the Advanced Encryption Standard for symmetric encryption
- **PBKDF2 Key Derivation**: Secure password-based key derivation with salt and iterations
- **Configurable Parameters**: Customizable key size, iterations, and hash algorithms
- **Automatic Padding**: Handles PKCS7 padding automatically
- **Base64 Output**: Encrypted data is returned as Base64 strings for easy handling

## Methods

### Encryption

#### Encrypt()
```csharp
public static string Encrypt(string plainText, byte[] encryptionKeyBytes)
```
**Purpose**: Encrypts plaintext using AES algorithm with the provided key.

**Parameters**:
- `plainText`: The text to encrypt
- `encryptionKeyBytes`: The encryption key as a byte array

**Returns**: Base64-encoded encrypted string (with trailing "==" removed)

**Behavior**:
- Uses a zero-filled 16-byte initialization vector (IV)
- Creates AES encryptor with the provided key
- Encrypts the plaintext using AES in CBC mode
- Returns Base64-encoded result with padding removed

### Decryption

#### Decrypt()
```csharp
public static string Decrypt(string cipherText, byte[] encryptionKeyBytes)
```
**Purpose**: Decrypts AES-encrypted ciphertext using the provided key.

**Parameters**:
- `cipherText`: Base64-encoded encrypted string
- `encryptionKeyBytes`: The decryption key as a byte array

**Returns**: Decrypted plaintext string

**Behavior**:
- Automatically handles Base64 padding restoration
- Uses the same zero-filled IV as encryption
- Creates AES decryptor with the provided key
- Returns the original plaintext

### Key Generation

#### CreateKey()
```csharp
public static byte[] CreateKey(string password, int keyBytes = 32, int iterations = 300, HashAlgorithmName? algorithmName = null)
```
**Purpose**: Derives encryption keys from passwords using PBKDF2.

**Parameters**:
- `password`: The password to derive the key from
- `keyBytes`: Size of the key in bytes (default: 32 for AES-256)
- `iterations`: Number of PBKDF2 iterations (default: 300)
- `algorithmName`: Hash algorithm to use (default: SHA3-256)

**Returns**: Derived key as a byte array

**Security Features**:
- Uses a fixed salt: `[10, 20, 30, 40, 50, 60, 70, 80]`
- Configurable iteration count for security tuning
- Supports modern hash algorithms like SHA3-256

## Usage Examples

### Basic Encryption and Decryption

```csharp
using RapidStreamer.BuildingBlocks.Application.Ciphering;

// Create encryption key from password
string password = "MySecurePassword123";
byte[] key = EncryptionService.CreateKey(password);

// Encrypt sensitive data
string plainText = "This is sensitive information";
string encrypted = EncryptionService.Encrypt(plainText, key);
Console.WriteLine($"Encrypted: {encrypted}");

// Decrypt the data
string decrypted = EncryptionService.Decrypt(encrypted, key);
Console.WriteLine($"Decrypted: {decrypted}");
// Output: "This is sensitive information"
```

### Custom Key Parameters

```csharp
// Create a stronger key with custom parameters
string password = "StrongPassword!@#";
byte[] strongKey = EncryptionService.CreateKey(
    password: password,
    keyBytes: 32,           // AES-256 key size
    iterations: 10000,      // Higher iteration count for better security
    algorithmName: HashAlgorithmName.SHA512
);

// Use the custom key for encryption
string sensitive = "Financial data: $1,000,000";
string encrypted = EncryptionService.Encrypt(sensitive, strongKey);
string decrypted = EncryptionService.Decrypt(encrypted, strongKey);
```

### Configuration Storage Example

```csharp
public class SecureConfigurationManager
{
    private readonly byte[] _encryptionKey;
    
    public SecureConfigurationManager(string masterPassword)
    {
        _encryptionKey = EncryptionService.CreateKey(
            masterPassword, 
            keyBytes: 32,
            iterations: 5000
        );
    }
    
    public void SaveSetting(string key, string value)
    {
        string encrypted = EncryptionService.Encrypt(value, _encryptionKey);
        // Save encrypted value to storage
        SaveToStorage(key, encrypted);
    }
    
    public string GetSetting(string key)
    {
        string encrypted = LoadFromStorage(key);
        if (string.IsNullOrEmpty(encrypted))
            return string.Empty;
            
        return EncryptionService.Decrypt(encrypted, _encryptionKey);
    }
    
    private void SaveToStorage(string key, string encryptedValue)
    {
        // Implementation for saving to file/database/registry
        File.WriteAllText($"config_{key}.dat", encryptedValue);
    }
    
    private string LoadFromStorage(string key)
    {
        string filePath = $"config_{key}.dat";
        return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
    }
}

// Usage
var configManager = new SecureConfigurationManager("MasterPassword123");
configManager.SaveSetting("DatabaseConnectionString", "Server=localhost;Database=MyApp;...");
string connectionString = configManager.GetSetting("DatabaseConnectionString");
```

### Bulk Data Encryption

```csharp
public class BulkDataEncryptor
{
    private readonly byte[] _key;
    
    public BulkDataEncryptor(string password)
    {
        _key = EncryptionService.CreateKey(password, iterations: 1000);
    }
    
    public Dictionary<string, string> EncryptData(Dictionary<string, string> data)
    {
        var encrypted = new Dictionary<string, string>();
        
        foreach (var item in data)
        {
            encrypted[item.Key] = EncryptionService.Encrypt(item.Value, _key);
        }
        
        return encrypted;
    }
    
    public Dictionary<string, string> DecryptData(Dictionary<string, string> encryptedData)
    {
        var decrypted = new Dictionary<string, string>();
        
        foreach (var item in encryptedData)
        {
            try
            {
                decrypted[item.Key] = EncryptionService.Decrypt(item.Value, _key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decrypt {item.Key}: {ex.Message}");
                // Handle decryption failure
            }
        }
        
        return decrypted;
    }
}
```

## Security Considerations

### Key Management

✅ **Best Practices:**
- Store passwords securely (environment variables, secure vaults)
- Use sufficiently long and complex passwords for key derivation
- Consider using different passwords for different data types
- Implement proper key rotation strategies

```csharp
// Good: Secure password storage
string password = Environment.GetEnvironmentVariable("ENCRYPTION_PASSWORD") 
                 ?? throw new InvalidOperationException("Encryption password not configured");

// Good: Strong password
string strongPassword = "MyApp!2024#SecureEncryption$Key%789";

// Avoid: Hard-coded passwords
string weakPassword = "password123"; // Don't do this!
```

### Algorithm Parameters

✅ **Security recommendations:**
- Use at least 32 bytes (256 bits) for key size
- Use minimum 10,000 iterations for PBKDF2 (more for sensitive data)
- Use modern hash algorithms like SHA-256 or SHA-512

```csharp
// Recommended for sensitive data
byte[] secureKey = EncryptionService.CreateKey(
    password,
    keyBytes: 32,           // AES-256
    iterations: 100000,     // High iteration count
    algorithmName: HashAlgorithmName.SHA512
);

// Minimum acceptable for regular data
byte[] standardKey = EncryptionService.CreateKey(
    password,
    keyBytes: 32,           // AES-256
    iterations: 10000,      // Reasonable iteration count
    algorithmName: HashAlgorithmName.SHA256
);
```

## Real-World Applications

### Database Field Encryption

```csharp
public class UserRepository
{
    private readonly byte[] _encryptionKey;
    
    public UserRepository(string encryptionPassword)
    {
        _encryptionKey = EncryptionService.CreateKey(encryptionPassword, iterations: 50000);
    }
    
    public void SaveUser(User user)
    {
        // Encrypt sensitive fields before saving
        user.EncryptedSSN = EncryptionService.Encrypt(user.SSN, _encryptionKey);
        user.EncryptedCreditCard = EncryptionService.Encrypt(user.CreditCard, _encryptionKey);
        
        // Clear plaintext from memory
        user.SSN = string.Empty;
        user.CreditCard = string.Empty;
        
        // Save to database with encrypted fields
        SaveToDatabase(user);
    }
    
    public User GetUser(int userId)
    {
        var user = LoadFromDatabase(userId);
        
        // Decrypt sensitive fields
        if (!string.IsNullOrEmpty(user.EncryptedSSN))
        {
            user.SSN = EncryptionService.Decrypt(user.EncryptedSSN, _encryptionKey);
        }
        
        if (!string.IsNullOrEmpty(user.EncryptedCreditCard))
        {
            user.CreditCard = EncryptionService.Decrypt(user.EncryptedCreditCard, _encryptionKey);
        }
        
        return user;
    }
}
```

### API Token Encryption

```csharp
public class ApiTokenManager
{
    private readonly byte[] _tokenKey;
    
    public ApiTokenManager()
    {
        string keyPassword = Environment.GetEnvironmentVariable("API_TOKEN_KEY") 
                           ?? "DefaultTokenKey2024!";
        _tokenKey = EncryptionService.CreateKey(keyPassword);
    }
    
    public string CreateSecureToken(string userId, DateTime expires)
    {
        var tokenData = new
        {
            UserId = userId,
            Expires = expires,
            Created = DateTime.UtcNow,
            Random = Guid.NewGuid().ToString()
        };
        
        string json = System.Text.Json.JsonSerializer.Serialize(tokenData);
        return EncryptionService.Encrypt(json, _tokenKey);
    }
    
    public (string UserId, DateTime Expires, bool IsValid) ValidateToken(string encryptedToken)
    {
        try
        {
            string json = EncryptionService.Decrypt(encryptedToken, _tokenKey);
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
            
            var expires = DateTime.Parse(tokenData.GetProperty("Expires").GetString());
            var userId = tokenData.GetProperty("UserId").GetString();
            var isValid = expires > DateTime.UtcNow;
            
            return (userId, expires, isValid);
        }
        catch
        {
            return (string.Empty, DateTime.MinValue, false);
        }
    }
}
```

### File Encryption Utility

```csharp
public class FileEncryptor
{
    public static void EncryptFile(string inputPath, string outputPath, string password)
    {
        byte[] key = EncryptionService.CreateKey(password, iterations: 25000);
        
        string content = File.ReadAllText(inputPath);
        string encrypted = EncryptionService.Encrypt(content, key);
        
        File.WriteAllText(outputPath, encrypted);
    }
    
    public static void DecryptFile(string inputPath, string outputPath, string password)
    {
        byte[] key = EncryptionService.CreateKey(password, iterations: 25000);
        
        string encrypted = File.ReadAllText(inputPath);
        string decrypted = EncryptionService.Decrypt(encrypted, key);
        
        File.WriteAllText(outputPath, decrypted);
    }
}

// Usage
FileEncryptor.EncryptFile("sensitive_data.txt", "sensitive_data.enc", "MyFilePassword123");
FileEncryptor.DecryptFile("sensitive_data.enc", "recovered_data.txt", "MyFilePassword123");
```

## Performance Considerations

### Key Derivation Performance
```csharp
// Expensive operation - cache the key when possible
byte[] key = EncryptionService.CreateKey(password, iterations: 100000);

// Good: Cache keys for repeated operations
private static readonly ConcurrentDictionary<string, byte[]> _keyCache = new();

public byte[] GetOrCreateKey(string password, int iterations = 10000)
{
    string cacheKey = $"{password}:{iterations}";
    return _keyCache.GetOrAdd(cacheKey, _ => 
        EncryptionService.CreateKey(password, iterations: iterations));
}
```

### Batch Operations
```csharp
public class BatchEncryptor
{
    private readonly byte[] _key;
    
    public BatchEncryptor(string password)
    {
        // Create key once for batch operations
        _key = EncryptionService.CreateKey(password);
    }
    
    public List<string> EncryptBatch(IEnumerable<string> items)
    {
        return items.Select(item => EncryptionService.Encrypt(item, _key)).ToList();
    }
}
```

## Error Handling

### Common Exceptions
```csharp
public static class SafeEncryptionService
{
    public static (bool Success, string Result, string Error) TryEncrypt(string plainText, byte[] key)
    {
        try
        {
            var result = EncryptionService.Encrypt(plainText, key);
            return (true, result, string.Empty);
        }
        catch (ArgumentNullException ex)
        {
            return (false, string.Empty, $"Null argument: {ex.ParamName}");
        }
        catch (CryptographicException ex)
        {
            return (false, string.Empty, $"Cryptographic error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Unexpected error: {ex.Message}");
        }
    }
    
    public static (bool Success, string Result, string Error) TryDecrypt(string cipherText, byte[] key)
    {
        try
        {
            var result = EncryptionService.Decrypt(cipherText, key);
            return (true, result, string.Empty);
        }
        catch (FormatException ex)
        {
            return (false, string.Empty, $"Invalid Base64 format: {ex.Message}");
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

### AES Configuration
- **Algorithm**: AES (Advanced Encryption Standard)
- **Mode**: CBC (Cipher Block Chaining)
- **Padding**: PKCS7
- **IV**: Zero-filled 16-byte array (consistent for all operations)
- **Key Size**: Configurable (default 256-bit)

### PBKDF2 Configuration
- **Default Salt**: `[10, 20, 30, 40, 50, 60, 70, 80]`
- **Default Iterations**: 300 (increase for production)
- **Default Hash**: SHA3-256
- **Key Length**: 32 bytes (256-bit) by default

### Base64 Handling
- Output strings have trailing "==" removed for cleaner appearance
- Input strings automatically restore padding during decryption

## Best Practices

### Security Guidelines

✅ **Recommended practices:**
- Use strong passwords for key derivation (minimum 12 characters)
- Increase iteration count for sensitive data (10,000+)
- Store encryption keys securely (never hard-code)
- Use environment variables or secure key management systems
- Implement proper error handling without exposing sensitive information

### Performance Optimization

✅ **Performance tips:**
- Cache derived keys for repeated operations
- Use appropriate iteration counts (balance security vs. performance)
- Consider async operations for large data encryption
- Implement batch processing for multiple items

### Integration Patterns

✅ **Effective patterns:**
```csharp
// Good: Service-oriented approach
public class EncryptionManager
{
    private readonly byte[] _key;
    
    public EncryptionManager(IConfiguration config)
    {
        string password = config["Encryption:Password"] 
                         ?? throw new InvalidOperationException("Encryption password not configured");
        _key = EncryptionService.CreateKey(password);
    }
    
    public string Protect(string data) => EncryptionService.Encrypt(data, _key);
    public string Unprotect(string encryptedData) => EncryptionService.Decrypt(encryptedData, _key);
}
```

## Related Components

- [`PasswordGenerator`](PasswordGenerator.md) - Generate secure passwords for encryption keys
- [`RsaEncryptionService`](RsaEncryptionService.md) - Asymmetric encryption alternative
- [System.Security.Cryptography](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography) - Underlying cryptographic framework

## Testing

The service behavior is thoroughly tested in `EncryptionServiceTests.cs`, which verifies:
- Successful encryption and decryption round trips
- Key derivation with different parameters
- Base64 encoding/decoding handling
- Error conditions and exception handling
- Performance characteristics with different iteration counts