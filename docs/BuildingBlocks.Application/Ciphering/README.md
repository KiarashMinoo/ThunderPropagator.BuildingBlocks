# Ciphering

## Contents
- [Overview](#overview)
- [Files](#files)
- [Types & Members](#types--members)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

Cryptography and security utilities including AES/RSA encryption services and secure password generation with configurable complexity.

## Files

| File | Primary Type(s) | LOC | Responsibility |
|------|-----------------|-----|----------------|
| [EncryptionService.cs](../../../src/ThunderPropagator.BuildingBlocks.Application/Ciphering/EncryptionService.cs) | `EncryptionService` | 80 | AES encryption/decryption with key derivation |
| [RsaEncryptionService.cs](../../../src/ThunderPropagator.BuildingBlocks.Application/Ciphering/RsaEncryptionService.cs) | `RsaEncryptionService` | 120 | RSA asymmetric encryption |
| [PasswordGenerator.cs](../../../src/ThunderPropagator.BuildingBlocks.Application/Ciphering/PasswordGenerator.cs) | `PasswordGenerator` | 90 | Secure password generation |

## Types & Members

### Types Summary

| Type | Kind | Summary | Inherits/Implements | Key Members |
|------|------|---------|---------------------|-------------|
| `EncryptionService` | Static Class | AES encryption with PBKDF2 key derivation | - | `Encrypt()`, `Decrypt()`, `CreateKey()` |
| `RsaEncryptionService` | Static Class | RSA asymmetric encryption | - | `Encrypt()`, `Decrypt()`, `GenerateKeys()` |
| `PasswordGenerator` | Static Class | Secure password generation | - | `Generate()`, complexity options |

### EncryptionService

**Kind**: Static Class  
**Namespace**: `ThunderPropagator.BuildingBlocks.Application.Ciphering`

AES encryption service with PBKDF2 key derivation supporting SHA3-256.

**Key Methods**:
- `string Encrypt(string plainText, byte[] encryptionKeyBytes)` — Encrypts to Base64
- `string Decrypt(string cipherText, byte[] encryptionKeyBytes)` — Decrypts from Base64
- `byte[] CreateKey(string password, int keyBytes = 32, int iterations = 300, HashAlgorithmName? algorithmName = null)` — Derives encryption key from password

**Key Derivation**:
- Uses `Rfc2898DeriveBytes.Pbkdf2` on .NET 10+
- Falls back to `Rfc2898DeriveBytes` on earlier versions
- Default: SHA3-256, 32 bytes (256-bit), 300 iterations
- Fixed salt: `[10, 20, 30, 40, 50, 60, 70, 80]`

**Usage Recipe**:

```csharp
using ThunderPropagator.BuildingBlocks.Application.Ciphering;

var password = "mySecurePassword123!";
var key = EncryptionService.CreateKey(password);

// Encrypt
var plainText = "Sensitive data here";
var encrypted = EncryptionService.Encrypt(plainText, key);
Console.WriteLine($"Encrypted: {encrypted}");

// Decrypt
var decrypted = EncryptionService.Decrypt(encrypted, key);
Console.WriteLine($"Decrypted: {decrypted}"); // "Sensitive data here"

// Custom key derivation
var strongKey = EncryptionService.CreateKey(
    password,
    keyBytes: 32,
    iterations: 10000,
    algorithmName: HashAlgorithmName.SHA512);
```

[↑ Back to top](#contents)

## Diagrams

### AES Encryption Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant ES as EncryptionService
    participant AES as Aes.Create
    participant PBKDF2 as Rfc2898DeriveBytes
    
    C->>ES: CreateKey(password)
    ES->>PBKDF2: Pbkdf2(password, salt, 300, SHA3-256, 32)
    PBKDF2-->>ES: key bytes
    ES-->>C: key bytes
    
    C->>ES: Encrypt(plainText, key)
    ES->>AES: Create()
    ES->>AES: aes.Key = key
    ES->>AES: aes.IV = [16 zeros]
    ES->>AES: CreateEncryptor()
    ES->>ES: CryptoStream write
    ES->>ES: Convert to Base64
    ES-->>C: cipherText (Base64)
```

### Password Generation

```mermaid
flowchart TD
    A[PasswordGenerator.Generate] --> B{Options}
    B --> C[Length]
    B --> D[Include Uppercase]
    B --> E[Include Lowercase]
    B --> F[Include Digits]
    B --> G[Include Symbols]
    
    C --> H[Random Selection]
    D --> H
    E --> H
    F --> H
    G --> H
    
    H --> I[Cryptographic RNG]
    I --> J[Validate Requirements]
    J --> K{All Requirements Met?}
    K -->|No| H
    K -->|Yes| L[Return Password]
```

[↑ Back to top](#contents)

## Examples

### Encrypting Configuration Data

```csharp
using ThunderPropagator.BuildingBlocks.Application.Ciphering;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

public class SecureConfig
{
    private static readonly byte[] Key = EncryptionService.CreateKey(
        Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "default-key",
        iterations: 10000);
    
    public string ConnectionString { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    
    public string Encrypt()
    {
        var json = this.ToJson();
        return EncryptionService.Encrypt(json, Key);
    }
    
    public static SecureConfig Decrypt(string encrypted)
    {
        var json = EncryptionService.Decrypt(encrypted, Key);
        return json.FromJson<SecureConfig>() ?? new SecureConfig();
    }
}

// Usage
var config = new SecureConfig
{
    ConnectionString = "Server=prod-db;Database=myapp",
    ApiKey = "secret-api-key-12345"
};

// Encrypt and store
var encrypted = config.Encrypt();
File.WriteAllText("config.encrypted", encrypted);

// Later: decrypt and load
var loaded = SecureConfig.Decrypt(File.ReadAllText("config.encrypted"));
Console.WriteLine($"Loaded connection: {loaded.ConnectionString}");
```

### Generating Secure Passwords

```csharp
using ThunderPropagator.BuildingBlocks.Application.Ciphering;

// Simple password
var password = PasswordGenerator.Generate(length: 16);
Console.WriteLine($"Password: {password}");

// Complex password with all character types
var complexPassword = PasswordGenerator.Generate(
    length: 20,
    includeUppercase: true,
    includeLowercase: true,
    includeDigits: true,
    includeSymbols: true);
Console.WriteLine($"Complex password: {complexPassword}");

// PIN-style (digits only)
var pin = PasswordGenerator.Generate(
    length: 6,
    includeUppercase: false,
    includeLowercase: false,
    includeDigits: true,
    includeSymbols: false);
Console.WriteLine($"PIN: {pin}");
```

### RSA Key Exchange

```csharp
using ThunderPropagator.BuildingBlocks.Application.Ciphering;

// Generate RSA key pair
var (publicKey, privateKey) = RsaEncryptionService.GenerateKeys(keySize: 2048);

// Alice encrypts with Bob's public key
var message = "Secret message from Alice";
var encrypted = RsaEncryptionService.Encrypt(message, publicKey);

// Send encrypted message...

// Bob decrypts with his private key
var decrypted = RsaEncryptionService.Decrypt(encrypted, privateKey);
Console.WriteLine($"Decrypted: {decrypted}"); // "Secret message from Alice"
```

## See Also

- [Application Layer](../README.md)
- [Helpers](../Helpers/README.md)
- [Objects](../Objects/README.md)
- [Documentation Home](../../README.md)

[↑ Back to top](#contents)
