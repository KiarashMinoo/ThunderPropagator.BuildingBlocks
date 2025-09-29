# BasicUserConfiguration

The `BasicUserConfiguration` class provides an abstract base for user authentication configuration in .NET applications. It offers essential user credential and role management functionality with built-in security considerations and serialization support.

## Overview

```csharp
public abstract class BasicUserConfiguration : EquatableObject<BasicUserConfiguration>
```

`BasicUserConfiguration` is an abstract class that extends `EquatableObject<BasicUserConfiguration>`, providing a foundation for user authentication systems with username, password, and role-based access control, along with built-in equality comparison and dual serialization support.

## Key Features

- **Core Authentication Fields**: Username, password, and role management
- **Dual Serialization Support**: Compatible with both System.Text.Json and Newtonsoft.Json
- **Abstract Design**: Provides base functionality for concrete implementations
- **Equality Comparison**: Username-based equality and hash code generation
- **Role-Based Access Control**: Support for multiple user roles
- **Security-Aware Design**: Protected setters and secure property handling
- **Extensible Foundation**: Base for complex user authentication scenarios

## Public Properties

### Username
The unique identifier for user authentication.

```csharp
[JsonProperty, JsonInclude] 
public string Username { get; protected set; } = null!;
```

**Purpose:** Primary identifier for user authentication and authorization
**Protection:** Protected setter ensures controlled access
**Validation:** Should be unique across the system

### Password
The authentication credential for the user.

```csharp
[JsonProperty, JsonInclude] 
public string Password { get; protected set; } = null!;
```

**Purpose:** Authentication credential (should be hashed in production)
**Security Note:** Consider implementing password hashing in concrete implementations
**Protection:** Protected setter for controlled access

### Roles
Optional array of roles assigned to the user for authorization.

```csharp
[JsonProperty, JsonInclude] 
public string[]? Roles { get; protected set; }
```

**Purpose:** Role-based access control and authorization
**Nullable:** Can be null if role-based authorization is not used
**Flexibility:** Supports multiple roles per user

## Equality and Hash Code

### GetHashCode Override
Provides username-based hash code generation for efficient collections and equality operations.

```csharp
public override int GetHashCode() => Username.GetHashCode();
```

**Behavior:** Uses username as the primary key for equality comparison
**Performance:** Optimized for dictionary and set operations
**Uniqueness:** Assumes username uniqueness across the system

## Usage Examples

### Basic User Configuration Implementation

```csharp
public class AppUserConfiguration : BasicUserConfiguration
{
    public AppUserConfiguration(string username, string password, params string[] roles)
    {
        Username = Guard.Against.NullOrWhiteSpace(username, nameof(username));
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));
        Roles = roles?.Length > 0 ? roles : null;
    }
    
    public static AppUserConfiguration CreateAdmin(string username, string password)
    {
        return new AppUserConfiguration(username, password, "Admin", "User");
    }
    
    public static AppUserConfiguration CreateUser(string username, string password)
    {
        return new AppUserConfiguration(username, password, "User");
    }
    
    public static AppUserConfiguration CreateGuest(string username)
    {
        return new AppUserConfiguration(username, GenerateGuestPassword(), "Guest");
    }
    
    private static string GenerateGuestPassword()
    {
        // Generate temporary password for guest accounts
        return $"guest_{Guid.NewGuid():N}";
    }
    
    public bool HasRole(string role)
    {
        return Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
    }
    
    public bool IsAdmin => HasRole("Admin");
    public bool IsUser => HasRole("User");
    public bool IsGuest => HasRole("Guest");
}
```

### Secure User Configuration with Password Hashing

```csharp
public class SecureUserConfiguration : BasicUserConfiguration
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;
    
    public SecureUserConfiguration(string username, string plainPassword, params string[] roles)
    {
        Username = Guard.Against.NullOrWhiteSpace(username, nameof(username));
        Password = HashPassword(plainPassword);
        Roles = roles?.Length > 0 ? roles : null;
    }
    
    // For deserialization (password already hashed)
    protected SecureUserConfiguration() { }
    
    public static SecureUserConfiguration FromHashedPassword(string username, string hashedPassword, params string[] roles)
    {
        return new SecureUserConfiguration
        {
            Username = username,
            Password = hashedPassword,
            Roles = roles?.Length > 0 ? roles : null
        };
    }
    
    public bool VerifyPassword(string plainPassword)
    {
        try
        {
            return VerifyHashedPassword(Password, plainPassword);
        }
        catch
        {
            return false;
        }
    }
    
    private static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);
        
        byte[] result = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, result, 0, SaltSize);
        Array.Copy(hash, 0, result, SaltSize, HashSize);
        
        return Convert.ToBase64String(result);
    }
    
    private static bool VerifyHashedPassword(string hashedPassword, string plainPassword)
    {
        byte[] combined = Convert.FromBase64String(hashedPassword);
        
        byte[] salt = new byte[SaltSize];
        byte[] existingHash = new byte[HashSize];
        
        Array.Copy(combined, 0, salt, 0, SaltSize);
        Array.Copy(combined, SaltSize, existingHash, 0, HashSize);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] newHash = pbkdf2.GetBytes(HashSize);
        
        return CryptographicOperations.FixedTimeEquals(existingHash, newHash);
    }
    
    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (!VerifyPassword(oldPassword))
            throw new UnauthorizedAccessException("Current password is incorrect");
        
        Password = HashPassword(newPassword);
    }
}
```

### Role-Based Authorization System

```csharp
public class RoleBasedUserConfiguration : BasicUserConfiguration
{
    public RoleBasedUserConfiguration(string username, string password, IEnumerable<string> roles)
    {
        Username = username;
        Password = password;
        Roles = roles?.ToArray();
    }
    
    public bool HasPermission(string permission)
    {
        return GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
    
    public IEnumerable<string> GetPermissions()
    {
        if (Roles == null) return Enumerable.Empty<string>();
        
        var permissions = new List<string>();
        
        foreach (string role in Roles)
        {
            permissions.AddRange(GetPermissionsForRole(role));
        }
        
        return permissions.Distinct();
    }
    
    private static IEnumerable<string> GetPermissionsForRole(string role)
    {
        return role.ToLower() switch
        {
            "admin" => new[] { "read", "write", "delete", "manage_users", "system_config" },
            "manager" => new[] { "read", "write", "delete", "manage_team" },
            "user" => new[] { "read", "write" },
            "guest" => new[] { "read" },
            "readonly" => new[] { "read" },
            _ => Enumerable.Empty<string>()
        };
    }
    
    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return;
        
        var currentRoles = Roles?.ToList() ?? new List<string>();
        if (!currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            currentRoles.Add(role);
            Roles = currentRoles.ToArray();
        }
    }
    
    public void RemoveRole(string role)
    {
        if (Roles == null) return;
        
        var currentRoles = Roles.ToList();
        currentRoles.RemoveAll(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        Roles = currentRoles.Count > 0 ? currentRoles.ToArray() : null;
    }
}
```

### User Configuration Management System

```csharp
public class UserConfigurationManager
{
    private readonly Dictionary<string, BasicUserConfiguration> _users = new();
    
    public void AddUser(BasicUserConfiguration user)
    {
        Guard.Against.Null(user, nameof(user));
        Guard.Against.NullOrWhiteSpace(user.Username, nameof(user.Username));
        
        if (_users.ContainsKey(user.Username))
            throw new InvalidOperationException($"User '{user.Username}' already exists");
        
        _users[user.Username] = user;
    }
    
    public BasicUserConfiguration? GetUser(string username)
    {
        return _users.TryGetValue(username, out BasicUserConfiguration? user) ? user : null;
    }
    
    public bool RemoveUser(string username)
    {
        return _users.Remove(username);
    }
    
    public IEnumerable<BasicUserConfiguration> GetUsersByRole(string role)
    {
        return _users.Values.Where(u => u.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false);
    }
    
    public async Task SaveToFileAsync(string filePath)
    {
        var userData = _users.Values.ToList();
        string json = userData.ToJson();
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        string json = await File.ReadAllTextAsync(filePath);
        var users = json.FromJson<List<AppUserConfiguration>>() ?? new List<AppUserConfiguration>();
        
        _users.Clear();
        foreach (var user in users)
        {
            _users[user.Username] = user;
        }
    }
    
    public void SaveAsYaml(string filePath)
    {
        var userData = _users.Values.ToList();
        string yaml = userData.ToYaml();
        File.WriteAllText(filePath, yaml);
    }
    
    public void LoadFromYaml(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        string yaml = File.ReadAllText(filePath);
        var users = yaml.FromYaml<List<AppUserConfiguration>>() ?? new List<AppUserConfiguration>();
        
        _users.Clear();
        foreach (var user in users)
        {
            _users[user.Username] = user;
        }
    }
}
```

### Multi-Tenant User Configuration

```csharp
public class MultiTenantUserConfiguration : BasicUserConfiguration
{
    [JsonProperty, JsonInclude]
    public string TenantId { get; protected set; } = null!;
    
    [JsonProperty, JsonInclude]
    public Dictionary<string, string[]>? TenantRoles { get; protected set; }
    
    public MultiTenantUserConfiguration(string username, string password, string tenantId, 
        params string[] globalRoles)
    {
        Username = username;
        Password = password;
        TenantId = tenantId;
        Roles = globalRoles?.Length > 0 ? globalRoles : null;
        TenantRoles = new Dictionary<string, string[]>();
    }
    
    public void AddTenantRole(string tenantId, string role)
    {
        TenantRoles ??= new Dictionary<string, string[]>();
        
        if (TenantRoles.TryGetValue(tenantId, out string[]? existingRoles))
        {
            var rolesList = existingRoles.ToList();
            if (!rolesList.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                rolesList.Add(role);
                TenantRoles[tenantId] = rolesList.ToArray();
            }
        }
        else
        {
            TenantRoles[tenantId] = new[] { role };
        }
    }
    
    public string[] GetRolesForTenant(string tenantId)
    {
        var globalRoles = Roles ?? Array.Empty<string>();
        var tenantSpecificRoles = TenantRoles?.TryGetValue(tenantId, out string[]? roles) == true 
            ? roles 
            : Array.Empty<string>();
        
        return globalRoles.Concat(tenantSpecificRoles).Distinct().ToArray();
    }
    
    public bool HasRoleInTenant(string tenantId, string role)
    {
        return GetRolesForTenant(tenantId).Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
```

### Configuration with Expiration and Metadata

```csharp
public class AdvancedUserConfiguration : BasicUserConfiguration
{
    [JsonProperty, JsonInclude]
    public DateTime? ExpiresAt { get; protected set; }
    
    [JsonProperty, JsonInclude]
    public DateTime CreatedAt { get; protected set; }
    
    [JsonProperty, JsonInclude]
    public DateTime? LastLoginAt { get; protected set; }
    
    [JsonProperty, JsonInclude]
    public bool IsActive { get; protected set; } = true;
    
    [JsonProperty, JsonInclude]
    public Dictionary<string, object>? Metadata { get; protected set; }
    
    public AdvancedUserConfiguration(string username, string password, TimeSpan? validFor = null, 
        params string[] roles)
    {
        Username = username;
        Password = password;
        Roles = roles?.Length > 0 ? roles : null;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = validFor.HasValue ? DateTime.UtcNow.Add(validFor.Value) : null;
        Metadata = new Dictionary<string, object>();
    }
    
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    
    public bool IsValid => IsActive && !IsExpired;
    
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public void ExtendExpiration(TimeSpan extension)
    {
        if (ExpiresAt.HasValue)
        {
            ExpiresAt = ExpiresAt.Value.Add(extension);
        }
        else
        {
            ExpiresAt = DateTime.UtcNow.Add(extension);
        }
    }
    
    public void SetMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }
    
    public T? GetMetadata<T>(string key)
    {
        if (Metadata?.TryGetValue(key, out object? value) == true)
        {
            if (value is T typedValue)
                return typedValue;
            
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        return default;
    }
}
```

## Integration with Authentication Systems

### ASP.NET Core Identity Integration

```csharp
public class IdentityUserConfiguration : BasicUserConfiguration
{
    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Username),
            new(ClaimTypes.NameIdentifier, Username)
        };
        
        if (Roles != null)
        {
            claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        
        var identity = new ClaimsIdentity(claims, "BasicUserConfiguration");
        return new ClaimsPrincipal(identity);
    }
    
    public static IdentityUserConfiguration FromClaimsPrincipal(ClaimsPrincipal principal, string password)
    {
        var username = principal.FindFirst(ClaimTypes.Name)?.Value 
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("No valid username claim found");
        
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        
        return new IdentityUserConfiguration(username, password, roles);
    }
    
    public IdentityUserConfiguration(string username, string password, params string[] roles)
    {
        Username = username;
        Password = password;
        Roles = roles?.Length > 0 ? roles : null;
    }
}
```

### JWT Integration

```csharp
public class JwtUserConfiguration : BasicUserConfiguration
{
    public string GenerateJwtToken(JwtConfiguration jwtConfig, TimeSpan? expiration = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtConfig.IssuerSigningKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Username),
            new(JwtRegisteredClaimNames.Sub, Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        if (Roles != null)
        {
            claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(1)),
            Issuer = jwtConfig.ValidIssuer,
            Audience = jwtConfig.ValidAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public static JwtUserConfiguration? FromJwtToken(string token, JwtConfiguration jwtConfig)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtConfig.IssuerSigningKey);
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtConfig.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = jwtConfig.ValidateIssuer,
                ValidIssuer = jwtConfig.ValidIssuer,
                ValidateAudience = jwtConfig.ValidateAudience,
                ValidAudience = jwtConfig.ValidAudience,
                ValidateLifetime = jwtConfig.ValidateLifetime,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return FromClaimsPrincipal(principal);
        }
        catch
        {
            return null;
        }
    }
    
    private static JwtUserConfiguration FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        var username = principal.FindFirst(ClaimTypes.Name)?.Value 
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new InvalidOperationException("No valid username claim found");
        
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        
        return new JwtUserConfiguration(username, "jwt-validated", roles);
    }
    
    public JwtUserConfiguration(string username, string password, params string[] roles)
    {
        Username = username;
        Password = password;
        Roles = roles?.Length > 0 ? roles : null;
    }
}
```

## Security Considerations

### Password Security

```csharp
public class PasswordSecurityValidator
{
    public static ValidationResult ValidatePassword(string password)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password cannot be empty");
            return new ValidationResult(errors, new List<string>());
        }
        
        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters long");
        
        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");
        
        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");
        
        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");
        
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Password must contain at least one special character");
        
        var warnings = new List<string>();
        if (password.Length < 12)
            warnings.Add("Consider using a password with 12 or more characters for better security");
        
        return new ValidationResult(errors, warnings);
    }
    
    public static bool IsCommonPassword(string password)
    {
        var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "password123", "admin", "qwerty",
            "letmein", "welcome", "monkey", "dragon", "password1"
        };
        
        return commonPasswords.Contains(password);
    }
}
```

### Username Validation

```csharp
public class UsernameValidator
{
    private static readonly Regex ValidUsernameRegex = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled);
    
    public static ValidationResult ValidateUsername(string username)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("Username cannot be empty");
            return new ValidationResult(errors, new List<string>());
        }
        
        if (username.Length < 3)
            errors.Add("Username must be at least 3 characters long");
        
        if (username.Length > 50)
            errors.Add("Username cannot be longer than 50 characters");
        
        if (!ValidUsernameRegex.IsMatch(username))
            errors.Add("Username can only contain letters, numbers, dots, underscores, and hyphens");
        
        if (username.StartsWith('.') || username.EndsWith('.'))
            errors.Add("Username cannot start or end with a dot");
        
        if (username.Contains(".."))
            errors.Add("Username cannot contain consecutive dots");
        
        return new ValidationResult(errors, new List<string>());
    }
}
```

## Error Handling and Validation

### Comprehensive User Validation

```csharp
public static class UserConfigurationValidator
{
    public static ValidationResult Validate(BasicUserConfiguration config)
    {
        var allErrors = new List<string>();
        var allWarnings = new List<string>();
        
        // Username validation
        var usernameResult = UsernameValidator.ValidateUsername(config.Username);
        allErrors.AddRange(usernameResult.Errors);
        allWarnings.AddRange(usernameResult.Warnings);
        
        // Password validation (if implementing custom validation)
        if (config is IPasswordValidatable passwordValidatable)
        {
            var passwordResult = passwordValidatable.ValidatePassword();
            allErrors.AddRange(passwordResult.Errors);
            allWarnings.AddRange(passwordResult.Warnings);
        }
        
        // Role validation
        if (config.Roles != null)
        {
            foreach (string role in config.Roles)
            {
                if (string.IsNullOrWhiteSpace(role))
                    allErrors.Add("Role names cannot be empty");
            }
        }
        
        return new ValidationResult(allErrors, allWarnings);
    }
}

public interface IPasswordValidatable
{
    ValidationResult ValidatePassword();
}
```

## Testing Strategies

### Unit Tests

```csharp
[Test]
public void BasicUserConfiguration_Equality_WorksCorrectly()
{
    // Arrange
    var user1 = new TestUserConfiguration("john_doe", "password123", "User");
    var user2 = new TestUserConfiguration("john_doe", "different_password", "Admin");
    var user3 = new TestUserConfiguration("jane_doe", "password123", "User");
    
    // Act & Assert
    Assert.That(user1, Is.EqualTo(user2)); // Same username
    Assert.That(user1, Is.Not.EqualTo(user3)); // Different username
    Assert.That(user1.GetHashCode(), Is.EqualTo(user2.GetHashCode()));
}

[Test]
public void BasicUserConfiguration_Serialization_PreservesAllProperties()
{
    // Arrange
    var original = new TestUserConfiguration("test_user", "test_password", "Admin", "User");
    
    // Act
    string json = original.ToJson();
    var deserialized = json.FromJson<TestUserConfiguration>();
    
    // Assert
    Assert.That(deserialized, Is.EqualTo(original));
    Assert.That(deserialized.Username, Is.EqualTo(original.Username));
    Assert.That(deserialized.Password, Is.EqualTo(original.Password));
    Assert.That(deserialized.Roles, Is.EqualTo(original.Roles));
}

[Test]
public void BasicUserConfiguration_RoleManagement_WorksCorrectly()
{
    // Arrange
    var user = new RoleBasedUserConfiguration("test_user", "password", new[] { "User", "Admin" });
    
    // Act & Assert
    Assert.That(user.HasPermission("read"), Is.True);
    Assert.That(user.HasPermission("write"), Is.True);
    Assert.That(user.HasPermission("manage_users"), Is.True);
    Assert.That(user.HasPermission("nonexistent"), Is.False);
}

public class TestUserConfiguration : BasicUserConfiguration
{
    public TestUserConfiguration(string username, string password, params string[] roles)
    {
        Username = username;
        Password = password;
        Roles = roles?.Length > 0 ? roles : null;
    }
}
```

### Integration Tests

```csharp
[Test]
public async Task UserConfigurationManager_FileOperations_WorkCorrectly()
{
    // Arrange
    var manager = new UserConfigurationManager();
    var user1 = new AppUserConfiguration("user1", "password1", "Admin");
    var user2 = new AppUserConfiguration("user2", "password2", "User");
    
    manager.AddUser(user1);
    manager.AddUser(user2);
    
    string tempFile = Path.GetTempFileName();
    
    try
    {
        // Act - Save
        await manager.SaveToFileAsync(tempFile);
        
        // Act - Load
        var newManager = new UserConfigurationManager();
        await newManager.LoadFromFileAsync(tempFile);
        
        // Assert
        var loadedUser1 = newManager.GetUser("user1");
        var loadedUser2 = newManager.GetUser("user2");
        
        Assert.That(loadedUser1, Is.Not.Null);
        Assert.That(loadedUser2, Is.Not.Null);
        Assert.That(loadedUser1.Username, Is.EqualTo("user1"));
        Assert.That(loadedUser2.Username, Is.EqualTo("user2"));
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

## Best Practices

### 1. Security-First Design
```csharp
// Preferred - Hash passwords before storage
public class SecureUserConfig : BasicUserConfiguration
{
    public SecureUserConfig(string username, string plainPassword)
    {
        Username = username;
        Password = HashPassword(plainPassword); // Hash before storing
    }
}

// Avoid - Storing plain text passwords
public class InsecureUserConfig : BasicUserConfiguration
{
    public InsecureUserConfig(string username, string plainPassword)
    {
        Username = username;
        Password = plainPassword; // Never do this in production
    }
}
```

### 2. Role-Based Access Control
```csharp
// Use specific, granular roles
var user = new AppUserConfiguration("john", "password", "BlogEditor", "CommentModerator");

// Avoid overly broad permissions
var admin = new AppUserConfiguration("admin", "password", "SuperAdmin"); // Too broad
```

### 3. Validation Before Use
```csharp
public void CreateUser(BasicUserConfiguration config)
{
    var validation = UserConfigurationValidator.Validate(config);
    validation.ThrowIfInvalid();
    
    // Safe to create user
    AddUserToSystem(config);
}
```

## Integration with BuildingBlocks Helpers

### Using with GuardClause Helper

```csharp
public class ValidatedUserConfiguration : BasicUserConfiguration
{
    public ValidatedUserConfiguration(string username, string password, params string[] roles)
    {
        Username = Guard.Against.NullOrWhiteSpace(username, nameof(username));
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));
        Guard.Against.OutOfRange(username.Length, nameof(username), 3, 50, "Username must be between 3 and 50 characters");
        Roles = roles?.Length > 0 ? roles : null;
    }
}
```

### Using with StringHelper

```csharp
public class EncodedUserConfiguration : BasicUserConfiguration
{
    public string GetUsernameBase64()
    {
        return Username.ToBase64();
    }
    
    public void SetUsernameFromBase64(string base64Username)
    {
        Username = base64Username.FromBase64();
    }
}
```

## Migration and Upgrades

When upgrading user configuration systems:

```csharp
// Old approach - Manual user management
private void CreateUserOld(string username, string password)
{
    // Manual validation and storage
}

// New approach - Using BasicUserConfiguration
private void CreateUserNew(string username, string password, params string[] roles)
{
    var config = new AppUserConfiguration(username, password, roles);
    var validation = UserConfigurationValidator.Validate(config);
    validation.ThrowIfInvalid();
    
    userManager.AddUser(config);
}
```

## See Also

- [JwtConfiguration](JwtConfiguration.md) - JWT authentication configuration
- [JwtIdentityHelper](../Helpers/JwtIdentityHelper.md) - JWT token processing utilities
- [EquatableObject](../Objects/EquatableObject.md) - Base class for value equality
- [GuardClauseHelper](../Helpers/GuardClauseHelper.md) - Input validation utilities
- [StringHelper](../Helpers/StringHelper.md) - String manipulation utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application.Identity namespace - providing user authentication configuration infrastructure for .NET applications.*