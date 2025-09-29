# GuardClauseHelper

The `GuardClauseHelper` is a comprehensive validation utility that extends the popular Ardalis.GuardClauses library with additional validation methods for numeric types, strings, and pattern matching. It provides a fluent API for parameter validation with automatic parameter name capture and customizable error messages.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `GuardClauseHelper` enhances input validation by providing:

- **Numeric Range Validation**: Type-safe validation for any `INumber<T>` implementation
- **String Length Validation**: Minimum and maximum length constraints
- **Pattern Matching**: Regex-based validation for string formats
- **Fluent API**: Both extension methods on `IGuardClause` and direct static methods
- **Automatic Parameter Names**: Uses `CallerArgumentExpression` for precise error reporting

## Key Features

### 🔢 Numeric Validation
- `GreaterThan<T>` / `GreaterThanOrEqual<T>`: Ensures values exceed specified thresholds
- `LessThan<T>` / `LessThanOrEqual<T>`: Ensures values stay within upper bounds
- Generic constraints ensure type safety with `INumber<T>`

### 📏 String Validation
- `MinLength` / `MaxLength`: String length boundary validation
- Null-safe operations with detailed error messages

### 🔍 Pattern Validation
- `MeetRegex`: Validates strings against compiled regular expressions
- Performance-optimized regex matching

### 🎯 Developer Experience
- Automatic parameter name capture via `CallerArgumentExpression`
- Customizable error messages
- Fluent method chaining for complex validations

## Core Methods

### Numeric Range Validation

#### GreaterThan
```csharp
public static T GreaterThan<T>(
    this IGuardClause guardClause,
    T input,
    T indicator,
    [CallerArgumentExpression("input")] string? parameterName = null,
    string? message = null)
    where T : INumber<T>

// Direct extension method
public static T GreaterThan<T>(this T input, T indicator, ...)
    where T : INumber<T>
```

#### LessThan
```csharp
public static T LessThan<T>(
    this IGuardClause guardClause,
    T input,
    T indicator,
    [CallerArgumentExpression("input")] string? parameterName = null,
    string? message = null)
    where T : INumber<T>

// Direct extension method
public static T LessThan<T>(this T input, T indicator, ...)
    where T : INumber<T>
```

### String Length Validation

#### MinLength / MaxLength
```csharp
public static string MinLength(
    this IGuardClause guardClause,
    string input,
    int size,
    [CallerArgumentExpression("input")] string? parameterName = null,
    string? message = null)

public static string MaxLength(
    this IGuardClause guardClause,
    string input,
    int size,
    [CallerArgumentExpression("input")] string? parameterName = null,
    string? message = null)
```

### Pattern Validation

#### MeetRegex
```csharp
public static string MeetRegex(
    this IGuardClause guardClause,
    string input,
    Regex regex,
    [CallerArgumentExpression("input")] string? parameterName = null,
    string? message = null)
```

## Usage Examples

### Basic Numeric Validation
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

public void ProcessScore(int score)
{
    // Using Guard.Against syntax
    var validScore = Guard.Against.GreaterThan(score, 0)
                                  .LessThan(100);
    
    // Using direct extension methods
    var alternativeValidation = score.GreaterThan(0).LessThan(100);
    
    // Process valid score...
}

public void SetTemperature(double celsius)
{
    var temperature = Guard.Against.GreaterThanOrEqual(celsius, -273.15, 
        message: "Temperature cannot be below absolute zero");
    
    // Temperature is guaranteed to be valid
    UpdateTemperatureReading(temperature);
}
```

### String Validation
```csharp
public void CreateUserAccount(string username, string password)
{
    // Validate username length
    var validUsername = Guard.Against.MinLength(username, 3)
                                     .MaxLength(20);
    
    // Direct extension method approach
    var validPassword = password.MinLength(8).MaxLength(128);
    
    // Create account with validated inputs
    var account = new UserAccount(validUsername, validPassword);
}

public void ValidateEmail(string email)
{
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    var validEmail = Guard.Against.MeetRegex(email, emailRegex, 
        message: "Invalid email format");
    
    // Email format is guaranteed to be valid
    SendVerificationEmail(validEmail);
}
```

### Advanced Validation Chains
```csharp
public class OrderProcessor
{
    public void ProcessOrder(decimal amount, int quantity, string customerCode)
    {
        // Chain multiple validations
        var validAmount = amount.GreaterThan(0m)
                               .LessThanOrEqual(10000m);
        
        var validQuantity = quantity.GreaterThan(0)
                                   .LessThanOrEqual(1000);
        
        var customerCodeRegex = new Regex(@"^CUST\d{6}$");
        var validCustomerCode = customerCode.MinLength(9)
                                           .MaxLength(9)
                                           .MeetRegex(customerCodeRegex);
        
        // All parameters are now validated
        var order = new Order(validAmount, validQuantity, validCustomerCode);
        ProcessValidatedOrder(order);
    }
}
```

### Custom Error Messages
```csharp
public void ValidateAge(int age)
{
    var validAge = Guard.Against.LessThan(age, 0, 
        message: "Age cannot be negative") 
        .GreaterThan(150, 
        message: "Age seems unrealistic - please verify");
    
    // Use validated age
    CalculateInsurancePremium(validAge);
}

public void ValidateProductCode(string productCode)
{
    var productRegex = new Regex(@"^[A-Z]{3}\d{4}$");
    var validCode = productCode.MeetRegex(productRegex, 
        message: "Product code must follow format: ABC1234");
    
    // Product code format is guaranteed
    LookupProduct(validCode);
}
```

## Integration Patterns

### With ASP.NET Core Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            // Validate all inputs upfront
            var validName = request.Name.MinLength(1).MaxLength(100);
            var validPrice = request.Price.GreaterThan(0m);
            var validStock = request.Stock.GreaterThanOrEqual(0);
            
            var product = new Product(validName, validPrice, validStock);
            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

### With Domain Models
```csharp
public class BankAccount
{
    public decimal Balance { get; private set; }
    
    public void Deposit(decimal amount)
    {
        var validAmount = amount.GreaterThan(0m, 
            message: "Deposit amount must be positive");
        
        Balance += validAmount;
    }
    
    public void Withdraw(decimal amount)
    {
        var validAmount = amount.GreaterThan(0m)
                               .LessThanOrEqual(Balance, 
                                   message: "Insufficient funds");
        
        Balance -= validAmount;
    }
}
```

### With Configuration Validation
```csharp
public class DatabaseConfiguration
{
    public string ConnectionString { get; }
    public int Timeout { get; }
    public int MaxRetries { get; }
    
    public DatabaseConfiguration(string connectionString, int timeout, int maxRetries)
    {
        ConnectionString = connectionString.MinLength(10, 
            message: "Connection string too short");
        
        Timeout = timeout.GreaterThan(0)
                         .LessThanOrEqual(300, 
                             message: "Timeout must be between 1-300 seconds");
        
        MaxRetries = maxRetries.GreaterThanOrEqual(0)
                              .LessThanOrEqual(10, 
                                  message: "Max retries must be between 0-10");
    }
}
```

## Performance Considerations

### Optimal Usage Patterns

1. **Regex Compilation**: Pre-compile regex patterns for repeated validations
```csharp
public static class ValidationPatterns
{
    public static readonly Regex EmailPattern = 
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    public static readonly Regex PhonePattern = 
        new(@"^\+?[\d\s\-\(\)]+$", RegexOptions.Compiled);
}

// Use pre-compiled patterns
var validEmail = email.MeetRegex(ValidationPatterns.EmailPattern);
```

2. **Early Validation**: Validate at system boundaries
```csharp
// Validate inputs early in the request pipeline
public class ValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("User-Id", out var userIdValue))
        {
            var userId = int.Parse(userIdValue).GreaterThan(0);
            context.Items["ValidatedUserId"] = userId;
        }
        
        await next(context);
    }
}
```

3. **Avoid Repeated Validations**: Cache validation results when possible
```csharp
public class CachedValidator
{
    private readonly ConcurrentDictionary<string, bool> _validationCache = new();
    
    public string ValidateAndCache(string input, Regex pattern)
    {
        var isValid = _validationCache.GetOrAdd(input, 
            key => pattern.IsMatch(key));
        
        return isValid ? input : 
            throw new ArgumentException($"Input {input} is invalid");
    }
}
```

## Error Handling Strategy

### Exception Types
All validation methods throw `ArgumentException` with detailed messages when validation fails.

### Error Message Format
```csharp
// Default format for range validation
"Required input {parameterName} cannot be less than {indicator}."

// Default format for length validation  
"Required input {parameterName} length cannot be less than {size}."

// Default format for regex validation
"Required input {parameterName} does not meet regex {regex}."
```

### Custom Error Handling
```csharp
public class ValidationService
{
    public ValidationResult ValidateProduct(ProductDto product)
    {
        var errors = new List<string>();
        
        try { product.Name.MinLength(1).MaxLength(100); }
        catch (ArgumentException ex) { errors.Add(ex.Message); }
        
        try { product.Price.GreaterThan(0m); }
        catch (ArgumentException ex) { errors.Add(ex.Message); }
        
        return new ValidationResult 
        { 
            IsValid = !errors.Any(), 
            Errors = errors 
        };
    }
}
```

## Testing Strategies

### Unit Testing Validation Logic
```csharp
[Test]
public void GreaterThan_WithValidInput_ReturnsInput()
{
    // Arrange
    var input = 10;
    var threshold = 5;
    
    // Act
    var result = input.GreaterThan(threshold);
    
    // Assert
    Assert.AreEqual(input, result);
}

[Test]
public void GreaterThan_WithInvalidInput_ThrowsArgumentException()
{
    // Arrange
    var input = 3;
    var threshold = 5;
    
    // Act & Assert
    var ex = Assert.Throws<ArgumentException>(() => input.GreaterThan(threshold));
    Assert.That(ex.Message, Contains.Substring("cannot be less than 5"));
}
```

### Integration Testing
```csharp
[Test]
public void CreateProduct_WithInvalidInputs_ReturnsValidationErrors()
{
    // Arrange
    var request = new CreateProductRequest
    {
        Name = "", // Too short
        Price = -10m, // Negative
        Stock = -5 // Negative
    };
    
    // Act
    var response = controller.CreateProduct(request);
    
    // Assert
    Assert.IsInstanceOf<BadRequestObjectResult>(response);
}
```

## Best Practices

### 1. Validate Early and Consistently
```csharp
// ✅ Good: Validate at method entry
public void ProcessPayment(decimal amount)
{
    var validAmount = amount.GreaterThan(0m);
    // Continue with validated amount
}

// ❌ Avoid: Validating deep in business logic
public void ProcessPayment(decimal amount)
{
    // Complex logic...
    if (amount <= 0) throw new ArgumentException(); // Too late
}
```

### 2. Use Meaningful Error Messages
```csharp
// ✅ Good: Specific, actionable messages
var validAge = age.GreaterThanOrEqual(18, 
    message: "User must be 18 or older to create account");

// ❌ Avoid: Generic messages
var validAge = age.GreaterThanOrEqual(18, message: "Invalid age");
```

### 3. Chain Related Validations
```csharp
// ✅ Good: Logical validation chains
var validScore = score.GreaterThanOrEqual(0).LessThanOrEqual(100);

// ✅ Good: Separate unrelated validations
var validName = name.MinLength(1).MaxLength(50);
var validAge = age.GreaterThanOrEqual(18);
```

### 4. Handle Edge Cases
```csharp
public void ValidateTemperature(double? temperature)
{
    if (temperature.HasValue)
    {
        var validTemp = temperature.Value
            .GreaterThan(double.MinValue)
            .LessThan(double.MaxValue);
    }
}
```

## Related Components

- **[ExceptionHelper](ExceptionHelper.md)**: Use together for comprehensive error handling
- **[StringHelper](StringHelper.md)**: Complement string validations with string utilities
- **Ardalis.GuardClauses**: Base library providing standard guard clauses
- **System.Numerics.INumber<T>**: Enables generic numeric validation

## Migration Guide

### From Basic Validation
```csharp
// Before: Manual validation
if (age < 0 || age > 150)
    throw new ArgumentException("Invalid age", nameof(age));

// After: Using GuardClauseHelper
var validAge = age.GreaterThanOrEqual(0).LessThanOrEqual(150);
```

### From Ardalis.GuardClauses Only
```csharp
// Before: Limited to basic guard clauses
Guard.Against.NegativeOrZero(amount, nameof(amount));

// After: Enhanced with range validation
var validAmount = amount.GreaterThan(0m).LessThanOrEqual(maxAmount);
```

The GuardClauseHelper provides a robust, type-safe foundation for input validation throughout the RapidStreamer BuildingBlocks system, ensuring data integrity and providing clear error feedback for invalid inputs.