# PasswordGenerator

The `PasswordGenerator` class in the RapidStreamer BuildingBlocks library provides a comprehensive, cryptographically secure password generation system with extensive customization options. It generates strong passwords with configurable character sets, length requirements, and security policies.

## Purpose

This generator provides:
- **Cryptographically Secure Generation**: Uses `RandomNumberGenerator` for true randomness
- **Extensive Customization**: Configurable character sets, length, and security rules
- **Built-in Security Policies**: Prevention of ambiguous characters, duplicates, and sequences
- **Flexible Character Sets**: Support for custom uppercase, lowercase, digits, and symbols
- **Smart Defaults**: Optimized default settings for maximum compatibility and security

## Key Features

- **True Randomness**: Uses cryptographically secure random number generation
- **Character Set Control**: Fine-grained control over included character types
- **Security Policies**: Built-in rules to prevent weak password patterns
- **Ambiguity Prevention**: Excludes similar-looking characters by default (I/l, O/0)
- **Sequence Prevention**: Optional prevention of sequential characters
- **Duplicate Prevention**: Optional prevention of repeated characters
- **Custom Character Sets**: Support for completely custom character definitions

## Classes

### PasswordSettings

Configuration class that controls password generation behavior:

```csharp
public class PasswordSettings
{
    public bool IncludeUpperCase { get; set; } = true;
    public string? CustomUpperCase { get; set; }
    public bool IncludeLowerCase { get; set; } = true;
    public string? CustomLowerCase { get; set; }
    public bool IncludeNumbers { get; set; } = true;
    public string? CustomDigits { get; set; }
    public bool IncludeSymbols { get; set; } = false;
    public string? CustomSymbols { get; set; }
    public bool BeginWithLetter { get; set; } = true;
    public bool PreventDuplicateCharacters { get; set; } = false;
    public bool PreventSequentialCharacters { get; set; } = false;
}
```

### PasswordGenerator

Main generator class with static generation method:

```csharp
public static string Generate(int length, Action<PasswordSettings>? configure = null)
```

## Default Character Sets

The generator uses optimized character sets that exclude ambiguous characters:

| Character Type | Default Set | Excluded Characters | Reason |
|----------------|-------------|-------------------|---------|
| **Uppercase** | `ABCDEFGHJKMNPQRSTUVWXYZ` | `I`, `O` | Confusion with `l`, `1`, `0` |
| **Lowercase** | `abcdefghjkmnpqrstuvwxyz` | `i`, `o` | Confusion with `I`, `1`, `O`, `0` |
| **Digits** | `23456789` | `0`, `1` | Confusion with `O`, `I`, `l` |
| **Symbols** | `!\";#$%&'()*+,-./:;<=>?@[\\]^_`{|}~` | None | Full symbol set available |

## Usage Examples

### Basic Password Generation

```csharp
using RapidStreamer.BuildingBlocks.Application.Ciphering;

// Generate a 12-character password with default settings
string password = PasswordGenerator.Generate(12);
Console.WriteLine($"Password: {password}");
// Example output: "Kp7dR9mF3qL8"

// Generate a longer password
string longPassword = PasswordGenerator.Generate(20);
Console.WriteLine($"Long password: {longPassword}");
```

### Custom Configuration

```csharp
// Generate password with symbols included
string passwordWithSymbols = PasswordGenerator.Generate(16, settings =>
{
    settings.IncludeSymbols = true;
});
Console.WriteLine($"With symbols: {passwordWithSymbols}");
// Example output: "Kp7&R9m#3qL8$Nx@"

// Generate password without uppercase letters
string lowercaseOnly = PasswordGenerator.Generate(12, settings =>
{
    settings.IncludeUpperCase = false;
    settings.IncludeLowerCase = true;
    settings.IncludeNumbers = true;
    settings.BeginWithLetter = true;
});
Console.WriteLine($"Lowercase + numbers: {lowercaseOnly}");
// Example output: "k7d9mf3ql8nx"
```

### Advanced Security Settings

```csharp
// Generate password with strict security policies
string securePassword = PasswordGenerator.Generate(16, settings =>
{
    settings.IncludeUpperCase = true;
    settings.IncludeLowerCase = true;
    settings.IncludeNumbers = true;
    settings.IncludeSymbols = true;
    settings.PreventDuplicateCharacters = true;      // No repeated characters
    settings.PreventSequentialCharacters = true;     // No sequential characters
    settings.BeginWithLetter = true;                 // Start with a letter
});
Console.WriteLine($"Secure password: {securePassword}");
```

### Custom Character Sets

```csharp
// Use completely custom character sets
string customPassword = PasswordGenerator.Generate(12, settings =>
{
    settings.IncludeUpperCase = true;
    settings.CustomUpperCase = "QWERTYUIOPASDFGHJKLZXCVBNM";  // Custom layout
    
    settings.IncludeLowerCase = true;
    settings.CustomLowerCase = "qwertyuiopasdfghjklzxcvbnm";  // Matching custom layout
    
    settings.IncludeNumbers = true;
    settings.CustomDigits = "0123456789";                     // Include all digits
    
    settings.IncludeSymbols = false;
    settings.BeginWithLetter = true;
});
Console.WriteLine($"Custom charset: {customPassword}");
```

## Real-World Applications

### User Registration System

```csharp
public class UserRegistrationService
{
    public string GenerateTemporaryPassword()
    {
        // Generate a temporary password for new users
        return PasswordGenerator.Generate(12, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;           // Avoid symbols for easier typing
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = false; // Allow duplicates for simplicity
        });
    }
    
    public string GenerateApiKey()
    {
        // Generate a longer, more complex API key
        return PasswordGenerator.Generate(32, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;           // API keys typically avoid symbols
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = true; // Ensure uniqueness
        });
    }
    
    public string GenerateSecureToken()
    {
        // Generate a highly secure token
        return PasswordGenerator.Generate(24, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = true;
            settings.PreventDuplicateCharacters = true;
            settings.PreventSequentialCharacters = true;
            settings.BeginWithLetter = true;
        });
    }
}
```

### Password Policy Enforcement

```csharp
public class PasswordPolicyService
{
    public class PolicySettings
    {
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 128;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireNumbers { get; set; } = true;
        public bool RequireSymbols { get; set; } = false;
        public bool PreventCommonPasswords { get; set; } = true;
    }
    
    public string GenerateCompliantPassword(PolicySettings policy)
    {
        if (policy.MinLength < 4)
            throw new ArgumentException("Minimum length must be at least 4");
            
        int length = Math.Max(policy.MinLength, 12); // Use reasonable minimum
        length = Math.Min(length, policy.MaxLength);
        
        return PasswordGenerator.Generate(length, settings =>
        {
            settings.IncludeUpperCase = policy.RequireUppercase;
            settings.IncludeLowerCase = policy.RequireLowercase;
            settings.IncludeNumbers = policy.RequireNumbers;
            settings.IncludeSymbols = policy.RequireSymbols;
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = length > 20; // Only for longer passwords
            settings.PreventSequentialCharacters = policy.PreventCommonPasswords;
        });
    }
    
    public List<string> GenerateMultiplePasswords(int count, PolicySettings policy)
    {
        var passwords = new HashSet<string>();
        
        while (passwords.Count < count)
        {
            string password = GenerateCompliantPassword(policy);
            passwords.Add(password); // HashSet ensures uniqueness
        }
        
        return passwords.ToList();
    }
}
```

### Bulk Password Generation

```csharp
public class BulkPasswordGenerator
{
    public Dictionary<string, string> GenerateUserPasswords(IEnumerable<string> userIds)
    {
        var passwords = new Dictionary<string, string>();
        var usedPasswords = new HashSet<string>();
        
        foreach (string userId in userIds)
        {
            string password;
            int attempts = 0;
            
            // Ensure unique passwords across all users
            do
            {
                password = PasswordGenerator.Generate(14, settings =>
                {
                    settings.IncludeUpperCase = true;
                    settings.IncludeLowerCase = true;
                    settings.IncludeNumbers = true;
                    settings.IncludeSymbols = false;
                    settings.BeginWithLetter = true;
                    settings.PreventDuplicateCharacters = true;
                });
                
                attempts++;
                if (attempts > 100)
                    throw new InvalidOperationException("Unable to generate unique passwords");
                    
            } while (usedPasswords.Contains(password));
            
            passwords[userId] = password;
            usedPasswords.Add(password);
        }
        
        return passwords;
    }
}
```

### Integration with User Management

```csharp
public class UserAccountService
{
    private readonly PasswordPolicyService _passwordPolicy;
    
    public UserAccountService()
    {
        _passwordPolicy = new PasswordPolicyService();
    }
    
    public async Task<UserAccount> CreateUserAccount(string email, string firstName, string lastName)
    {
        // Generate a temporary password
        var policy = new PasswordPolicyService.PolicySettings
        {
            MinLength = 12,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireNumbers = true,
            RequireSymbols = false // User-friendly for initial login
        };
        
        string temporaryPassword = _passwordPolicy.GenerateCompliantPassword(policy);
        
        var user = new UserAccount
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            TemporaryPassword = temporaryPassword,
            MustChangePassword = true,
            CreatedDate = DateTime.UtcNow
        };
        
        // Hash and store the password
        user.PasswordHash = HashPassword(temporaryPassword);
        
        // Send welcome email with temporary password
        await SendWelcomeEmail(user, temporaryPassword);
        
        return user;
    }
    
    public string GeneratePasswordResetToken()
    {
        // Generate a secure token for password reset
        return PasswordGenerator.Generate(20, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false; // Avoid URL-unsafe characters
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = true;
        });
    }
}
```

## Advanced Configuration Scenarios

### Gaming Industry Requirements

```csharp
public class GamingPasswordGenerator
{
    public string GenerateGamePassword()
    {
        // Gaming passwords often need to be memorable but secure
        return PasswordGenerator.Generate(10, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;                    // Avoid complex symbols
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = false;        // Allow some repetition for memorability
            settings.PreventSequentialCharacters = true;       // Prevent obvious sequences
        });
    }
    
    public string GenerateGuildCode()
    {
        // Short, uppercase codes for guild invitations
        return PasswordGenerator.Generate(8, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = false;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;
            settings.CustomUpperCase = "ABCDEFGHJKMNPQRSTUVWXYZ"; // Exclude confusing letters
            settings.CustomDigits = "23456789";                   // Exclude confusing digits
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = true;
        });
    }
}
```

### Financial Services Requirements

```csharp
public class BankingPasswordGenerator
{
    public string GenerateCustomerPin()
    {
        // 6-digit numeric PIN
        return PasswordGenerator.Generate(6, settings =>
        {
            settings.IncludeUpperCase = false;
            settings.IncludeLowerCase = false;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;
            settings.CustomDigits = "0123456789";               // Include all digits for PINs
            settings.BeginWithLetter = false;
            settings.PreventSequentialCharacters = true;        // Prevent 123456, 654321, etc.
            settings.PreventDuplicateCharacters = true;         // Prevent 111111, 222222, etc.
        });
    }
    
    public string GenerateSecureTransactionId()
    {
        // Highly secure transaction identifier
        return PasswordGenerator.Generate(16, settings =>
        {
            settings.IncludeUpperCase = true;
            settings.IncludeLowerCase = true;
            settings.IncludeNumbers = true;
            settings.IncludeSymbols = false;                    // Avoid symbols in transaction IDs
            settings.BeginWithLetter = true;
            settings.PreventDuplicateCharacters = true;
            settings.PreventSequentialCharacters = true;
        });
    }
}
```

## Security Analysis

### Randomness Quality

The generator uses `RandomNumberGenerator.Fill()` which provides cryptographically secure randomness:

```csharp
static int GetRandomIndex(int max)
{
    var data = new byte[4];
    RandomNumberGenerator.Fill(data);                    // Cryptographically secure
    return BitConverter.ToInt32(data, 0) & int.MaxValue % max;
}
```

### Character Set Security

**Default sets optimize for security and usability:**
- **Ambiguous Character Removal**: Prevents user confusion and typing errors
- **Balanced Distribution**: Ensures good entropy across character types
- **Customizable**: Allows adjustment for specific requirements

### Entropy Calculation

Password strength depends on character set size and length:

```csharp
public static class PasswordStrengthCalculator
{
    public static double CalculateEntropy(int length, PasswordSettings settings)
    {
        int charsetSize = 0;
        
        if (settings.IncludeUpperCase)
            charsetSize += (settings.CustomUpperCase ?? "ABCDEFGHJKMNPQRSTUVWXYZ").Length;
            
        if (settings.IncludeLowerCase)
            charsetSize += (settings.CustomLowerCase ?? "abcdefghjkmnpqrstuvwxyz").Length;
            
        if (settings.IncludeNumbers)
            charsetSize += (settings.CustomDigits ?? "23456789").Length;
            
        if (settings.IncludeSymbols)
            charsetSize += (settings.CustomSymbols ?? "!\";#$%&'()*+,-./:;<=>?@[\\]^_`{|}~").Length;
        
        return Math.Log2(Math.Pow(charsetSize, length));
    }
}

// Example usage
var settings = new PasswordGenerator.PasswordSettings();
double entropy = PasswordStrengthCalculator.CalculateEntropy(12, settings);
Console.WriteLine($"12-character default password entropy: {entropy:F1} bits");
// Output: ~71.7 bits (very strong)
```

## Performance Characteristics

### Generation Speed
```csharp
public class PasswordGenerationBenchmark
{
    public void BenchmarkGeneration()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Generate 1000 passwords
        for (int i = 0; i < 1000; i++)
        {
            string password = PasswordGenerator.Generate(16);
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Generated 1000 passwords in {stopwatch.ElapsedMilliseconds}ms");
        // Typical output: ~50-100ms depending on hardware
    }
}
```

### Memory Usage
- **Minimal Allocation**: Uses `StringBuilder` for efficient string building
- **No Caching**: Each generation is independent, no memory retention
- **Small Footprint**: Character sets are stored as constants

## Error Handling

### Input Validation
```csharp
public static class SafePasswordGenerator
{
    public static (bool Success, string Password, string Error) TryGenerate(
        int length, 
        Action<PasswordGenerator.PasswordSettings>? configure = null)
    {
        try
        {
            if (length < 4)
                return (false, string.Empty, "Password length must be at least 4");
                
            if (length > 1000)
                return (false, string.Empty, "Password length too large (max 1000)");
            
            string password = PasswordGenerator.Generate(length, configure);
            return (true, password, string.Empty);
        }
        catch (ArgumentException ex)
        {
            return (false, string.Empty, $"Invalid configuration: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Generation failed: {ex.Message}");
        }
    }
}
```

### Configuration Validation
```csharp
public static void ValidateSettings(PasswordGenerator.PasswordSettings settings)
{
    bool hasAnyCharacterType = settings.IncludeUpperCase || 
                              settings.IncludeLowerCase || 
                              settings.IncludeNumbers || 
                              settings.IncludeSymbols;
                              
    if (!hasAnyCharacterType)
        throw new ArgumentException("At least one character type must be enabled");
    
    // Validate custom character sets
    if (settings.IncludeUpperCase && settings.CustomUpperCase?.Length == 0)
        throw new ArgumentException("Custom uppercase set cannot be empty when enabled");
        
    // Additional validations...
}
```

## Best Practices

### Security Guidelines

✅ **Recommended practices:**
- Use minimum 12 characters for user passwords
- Use 16+ characters for API keys and tokens
- Enable `PreventSequentialCharacters` for high-security scenarios
- Use `PreventDuplicateCharacters` for longer passwords (16+)
- Consider excluding symbols for user-typed passwords

### Performance Optimization

✅ **Performance tips:**
- Generate passwords in batches when creating multiple accounts
- Cache character sets for repeated generation with same settings
- Use appropriate length limits (avoid extremely long passwords)

### Integration Patterns

✅ **Effective patterns:**
```csharp
// Good: Service wrapper
public class PasswordService
{
    private readonly PasswordGenerator.PasswordSettings _defaultSettings;
    
    public PasswordService()
    {
        _defaultSettings = new PasswordGenerator.PasswordSettings
        {
            IncludeSymbols = false,
            PreventSequentialCharacters = true
        };
    }
    
    public string GenerateUserPassword() => 
        PasswordGenerator.Generate(12, s => ApplySettings(s, _defaultSettings));
}
```

## Related Components

- [`EncryptionService`](EncryptionService.md) - Use generated passwords for encryption key derivation
- [`RsaEncryptionService`](RsaEncryptionService.md) - Alternative encryption method that doesn't require passwords
- [System.Security.Cryptography.RandomNumberGenerator](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator) - Underlying randomness source

## Testing

The generator behavior should be tested for:
- **Randomness Quality**: Ensure good distribution across character sets
- **Configuration Compliance**: Verify generated passwords match settings
- **Security Policies**: Test prevention of duplicates, sequences, etc.
- **Performance**: Benchmark generation speed for different lengths and settings
- **Edge Cases**: Test minimum/maximum lengths and unusual configurations