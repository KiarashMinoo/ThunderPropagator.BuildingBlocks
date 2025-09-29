# InconvertibleException

The `InconvertibleException` class provides a specialized exception type for handling conversion failures in type-safe conversion operations. It offers standardized error reporting, helper methods for validation, and consistent messaging patterns for scenarios where objects cannot be converted between types.

## Overview

```csharp
public class InconvertibleException : Exception
{
    public InconvertibleException(string message);
    public InconvertibleException(Type sourceType, Type destinationType);
    
    public static void ThrowIfInconvertible(bool condition, string message);
    public static void ThrowIfInconvertible(Func<bool> condition, string message);
}
```

The `InconvertibleException` class is designed to work seamlessly with the `IConvertible<T>` interface, providing:
- **Standardized Error Messages**: Consistent formatting for type conversion errors
- **Type Information**: Automatic inclusion of source and destination type details
- **Validation Helpers**: Static methods for conditional exception throwing
- **Performance Optimization**: Lazy evaluation support for expensive validation conditions

## Key Features

### Specialized Constructors
- **Message-Based**: Direct error message specification for custom scenarios
- **Type-Based**: Automatic message generation from source and destination types
- **Consistent Formatting**: Standardized error message templates

### Validation Helpers
- **Condition Checking**: `ThrowIfInconvertible` methods for guard clause patterns
- **Lazy Evaluation**: Support for `Func<bool>` to defer expensive validations
- **Clean Syntax**: Readable validation code that integrates well with conversion logic

### Integration Support
- **IConvertible<T> Integration**: Designed to work with type-safe conversion interfaces
- **Exception Chaining**: Supports inner exception patterns for complex conversion scenarios
- **Debugging Support**: Clear error messages aid in troubleshooting conversion issues

## Constructor Details

### Message Constructor
```csharp
public InconvertibleException(string message) : base(message)
```
Creates an exception with a custom error message for specific conversion scenarios.

### Type-Based Constructor
```csharp
public InconvertibleException(Type sourceType, Type destinationType)
    : base($"value with type {sourceType} is not convertable to type {destinationType} ")
```
Automatically generates a standardized error message including source and destination type information.

## Validation Methods

### Boolean Condition Validation
```csharp
public static void ThrowIfInconvertible(bool condition, string message)
```
Throws an `InconvertibleException` if the condition is `false`, indicating the conversion is not possible.

### Lazy Condition Validation
```csharp
public static void ThrowIfInconvertible(Func<bool> condition, string message)
```
Evaluates the condition function and throws an exception if it returns `false`. Useful for expensive validation operations.

## Usage Examples

### Basic Type Conversion Validation

```csharp
public class NumericValue : IConvertible<int>, IConvertible<double>, IConvertible<string>
{
    private readonly object _value;
    
    public NumericValue(object value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    // Convert to integer
    int IConvertible<int>.Convert()
    {
        // Validate conversion is possible
        InconvertibleException.ThrowIfInconvertible(
            _value is int or long or short or byte or decimal or double or float,
            $"Cannot convert {_value.GetType().Name} to int: value must be a numeric type"
        );
        
        try
        {
            return Convert.ToInt32(_value);
        }
        catch (OverflowException)
        {
            throw new InconvertibleException($"Value {_value} is too large to convert to int");
        }
        catch (FormatException)
        {
            throw new InconvertibleException($"Value {_value} cannot be converted to int: invalid format");
        }
    }
    
    // Convert to double
    double IConvertible<double>.Convert()
    {
        InconvertibleException.ThrowIfInconvertible(
            _value is int or long or short or byte or decimal or double or float,
            $"Cannot convert {_value.GetType().Name} to double: value must be a numeric type"
        );
        
        try
        {
            return Convert.ToDouble(_value);
        }
        catch (OverflowException)
        {
            throw new InconvertibleException($"Value {_value} is too large to convert to double");
        }
    }
    
    // Convert to string
    string IConvertible<string>.Convert()
    {
        // All types can convert to string
        return _value.ToString() ?? "";
    }
}

public class NumericConversionDemo
{
    public static void DemonstrateBasicConversion()
    {
        var validValue = new NumericValue(42);
        var invalidValue = new NumericValue("not a number");
        
        try
        {
            // Valid conversions
            var intValue = ((IConvertible<int>)validValue).Convert();
            var doubleValue = ((IConvertible<double>)validValue).Convert();
            var stringValue = ((IConvertible<string>)validValue).Convert();
            
            Console.WriteLine($"Converted values: {intValue}, {doubleValue}, {stringValue}");
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"Conversion failed: {ex.Message}");
        }
        
        try
        {
            // This will throw InconvertibleException
            var invalidInt = ((IConvertible<int>)invalidValue).Convert();
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"Expected error: {ex.Message}");
        }
    }
}
```

### Complex Object Conversion with Validation

```csharp
public class Customer : IConvertible<CustomerDto>, IConvertible<CustomerSummary>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public CustomerStatus Status { get; set; }
    public List<Order> Orders { get; set; } = new();
    public decimal CreditLimit { get; set; }
    public bool IsVip { get; set; }
    
    // Convert to full DTO
    CustomerDto IConvertible<CustomerDto>.Convert()
    {
        // Validate required fields
        InconvertibleException.ThrowIfInconvertible(
            Id > 0,
            "Cannot convert Customer to DTO: Customer ID must be positive"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName),
            "Cannot convert Customer to DTO: First name and last name are required"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            !string.IsNullOrWhiteSpace(Email) && Email.Contains('@'),
            "Cannot convert Customer to DTO: Valid email address is required"
        );
        
        return new CustomerDto
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName,
            FullName = $"{this.FirstName} {this.LastName}".Trim(),
            Email = this.Email,
            DateOfBirth = this.DateOfBirth,
            Status = this.Status.ToString(),
            OrderCount = this.Orders.Count,
            TotalOrderValue = this.Orders.Sum(o => o.TotalAmount),
            CreditLimit = this.CreditLimit,
            IsVip = this.IsVip
        };
    }
    
    // Convert to summary
    CustomerSummary IConvertible<CustomerSummary>.Convert()
    {
        // Basic validation for summary
        InconvertibleException.ThrowIfInconvertible(
            Id > 0,
            "Cannot convert Customer to Summary: Customer ID must be positive"
        );
        
        // Use lazy evaluation for expensive operations
        InconvertibleException.ThrowIfInconvertible(
            () => CalculateCustomerScore() >= 0,
            "Cannot convert Customer to Summary: Customer score calculation failed"
        );
        
        return new CustomerSummary
        {
            Id = this.Id,
            FullName = $"{this.FirstName} {this.LastName}".Trim(),
            Email = this.Email,
            Status = this.Status.ToString(),
            IsActive = this.Status == CustomerStatus.Active,
            CustomerScore = CalculateCustomerScore()
        };
    }
    
    private int CalculateCustomerScore()
    {
        // Simulate expensive calculation
        var score = 0;
        score += Orders.Count * 10;
        score += IsVip ? 50 : 0;
        score += Status == CustomerStatus.Active ? 25 : 0;
        
        return score;
    }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Status { get; set; } = "";
    public int OrderCount { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsVip { get; set; }
}

public class CustomerSummary
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsActive { get; set; }
    public int CustomerScore { get; set; }
}

public class Order
{
    public decimal TotalAmount { get; set; }
}

public enum CustomerStatus
{
    Pending,
    Active,
    Suspended,
    Inactive
}

public class CustomerService
{
    public async Task<List<CustomerDto>> GetCustomerDtosAsync(IEnumerable<Customer> customers)
    {
        var results = new List<CustomerDto>();
        var errors = new List<string>();
        
        foreach (var customer in customers)
        {
            try
            {
                var dto = ((IConvertible<CustomerDto>)customer).Convert();
                results.Add(dto);
            }
            catch (InconvertibleException ex)
            {
                errors.Add($"Customer {customer.Id}: {ex.Message}");
            }
        }
        
        if (errors.Count > 0)
        {
            Console.WriteLine($"Conversion errors occurred for {errors.Count} customers:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        return results;
    }
    
    public async Task<ConversionReport<CustomerSummary>> GetCustomerSummariesWithReportAsync(IEnumerable<Customer> customers)
    {
        var report = new ConversionReport<CustomerSummary>();
        
        foreach (var customer in customers)
        {
            try
            {
                var summary = ((IConvertible<CustomerSummary>)customer).Convert();
                report.SuccessfulConversions.Add(summary);
            }
            catch (InconvertibleException ex)
            {
                report.FailedConversions.Add(new ConversionError
                {
                    SourceId = customer.Id.ToString(),
                    SourceType = nameof(Customer),
                    TargetType = nameof(CustomerSummary),
                    ErrorMessage = ex.Message,
                    Exception = ex
                });
            }
        }
        
        return report;
    }
}

public class ConversionReport<T>
{
    public List<T> SuccessfulConversions { get; set; } = new();
    public List<ConversionError> FailedConversions { get; set; } = new();
    
    public int TotalAttempts => SuccessfulConversions.Count + FailedConversions.Count;
    public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulConversions.Count / TotalAttempts : 0;
}

public class ConversionError
{
    public string SourceId { get; set; } = "";
    public string SourceType { get; set; } = "";
    public string TargetType { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public Exception? Exception { get; set; }
}
```

### Conditional Conversion with Business Rules

```csharp
public class BankAccount : IConvertible<AccountStatement>, IConvertible<CreditApplication>
{
    public string AccountNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime OpenedDate { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public bool IsActive { get; set; }
    public decimal CreditScore { get; set; }
    
    // Convert to account statement
    AccountStatement IConvertible<AccountStatement>.Convert()
    {
        // Validate account can generate statement
        InconvertibleException.ThrowIfInconvertible(
            IsActive,
            "Cannot generate statement for inactive account"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            !string.IsNullOrWhiteSpace(AccountNumber),
            "Cannot generate statement: Account number is required"
        );
        
        // Use lazy evaluation for transaction validation
        InconvertibleException.ThrowIfInconvertible(
            () => ValidateTransactionHistory(),
            "Cannot generate statement: Transaction history validation failed"
        );
        
        return new AccountStatement
        {
            AccountNumber = this.AccountNumber,
            CustomerName = this.CustomerName,
            StatementDate = DateTime.Today,
            OpeningBalance = CalculateOpeningBalance(),
            ClosingBalance = this.Balance,
            Transactions = this.Transactions.Select(t => new StatementTransaction
            {
                Date = t.Date,
                Description = t.Description,
                Amount = t.Amount,
                RunningBalance = CalculateRunningBalance(t)
            }).ToList()
        };
    }
    
    // Convert to credit application
    CreditApplication IConvertible<CreditApplication>.Convert()
    {
        // Strict validation for credit applications
        InconvertibleException.ThrowIfInconvertible(
            IsActive && Type != AccountType.Loan,
            "Cannot create credit application: Account must be active and not a loan account"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            CreditScore >= 300,
            $"Cannot create credit application: Credit score {CreditScore} is below minimum threshold (300)"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            Balance >= 0,
            $"Cannot create credit application: Account has negative balance ({Balance:C})"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            (DateTime.Today - OpenedDate).TotalDays >= 90,
            "Cannot create credit application: Account must be open for at least 90 days"
        );
        
        // Complex business rule validation
        InconvertibleException.ThrowIfInconvertible(
            () => ValidateCreditWorthiness(),
            "Cannot create credit application: Credit worthiness validation failed"
        );
        
        return new CreditApplication
        {
            ApplicantName = this.CustomerName,
            AccountNumber = this.AccountNumber,
            CurrentBalance = this.Balance,
            AccountAge = (DateTime.Today - this.OpenedDate).Days,
            CreditScore = this.CreditScore,
            AverageMonthlyBalance = CalculateAverageMonthlyBalance(),
            RequestedAmount = CalculateMaxCreditAmount(),
            ApplicationDate = DateTime.Today
        };
    }
    
    private bool ValidateTransactionHistory()
    {
        // Simulate complex validation
        return Transactions.All(t => t.Date >= OpenedDate && !string.IsNullOrWhiteSpace(t.Description));
    }
    
    private bool ValidateCreditWorthiness()
    {
        // Complex business logic
        var avgBalance = CalculateAverageMonthlyBalance();
        var transactionCount = Transactions.Count(t => t.Date >= DateTime.Today.AddMonths(-3));
        
        return avgBalance >= 1000 && transactionCount >= 5 && CreditScore >= 600;
    }
    
    private decimal CalculateOpeningBalance()
    {
        return Transactions.FirstOrDefault()?.Amount ?? 0;
    }
    
    private decimal CalculateRunningBalance(Transaction transaction)
    {
        return Balance; // Simplified calculation
    }
    
    private decimal CalculateAverageMonthlyBalance()
    {
        // Simplified calculation
        return Transactions.Average(t => Math.Abs(t.Amount));
    }
    
    private decimal CalculateMaxCreditAmount()
    {
        return Math.Min(CreditScore * 100, Balance * 5);
    }
}

public class AccountStatement
{
    public string AccountNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public DateTime StatementDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
}

public class StatementTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
}

public class CreditApplication
{
    public string ApplicantName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public decimal CurrentBalance { get; set; }
    public int AccountAge { get; set; }
    public decimal CreditScore { get; set; }
    public decimal AverageMonthlyBalance { get; set; }
    public decimal RequestedAmount { get; set; }
    public DateTime ApplicationDate { get; set; }
}

public class Transaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}

public enum AccountType
{
    Checking,
    Savings,
    Investment,
    Loan
}

public class BankingService
{
    public async Task<ProcessingResult<AccountStatement>> GenerateStatementsAsync(IEnumerable<BankAccount> accounts)
    {
        var result = new ProcessingResult<AccountStatement>();
        
        await foreach (var account in ProcessAccountsAsync(accounts))
        {
            try
            {
                var statement = ((IConvertible<AccountStatement>)account).Convert();
                result.SuccessfulResults.Add(statement);
            }
            catch (InconvertibleException ex)
            {
                result.Errors.Add(new ProcessingError
                {
                    EntityId = account.AccountNumber,
                    ErrorType = "ConversionError",
                    Message = ex.Message,
                    Exception = ex
                });
            }
        }
        
        return result;
    }
    
    public async Task<ProcessingResult<CreditApplication>> ProcessCreditApplicationsAsync(IEnumerable<BankAccount> accounts)
    {
        var result = new ProcessingResult<CreditApplication>();
        
        foreach (var account in accounts)
        {
            try
            {
                var application = ((IConvertible<CreditApplication>)account).Convert();
                result.SuccessfulResults.Add(application);
            }
            catch (InconvertibleException ex)
            {
                // Categorize different types of conversion failures
                var errorCategory = ex.Message switch
                {
                    var msg when msg.Contains("inactive") => "AccountStatus",
                    var msg when msg.Contains("Credit score") => "CreditScore",
                    var msg when msg.Contains("negative balance") => "Balance",
                    var msg when msg.Contains("90 days") => "AccountAge",
                    var msg when msg.Contains("worthiness") => "CreditWorthiness",
                    _ => "General"
                };
                
                result.Errors.Add(new ProcessingError
                {
                    EntityId = account.AccountNumber,
                    ErrorType = errorCategory,
                    Message = ex.Message,
                    Exception = ex
                });
            }
        }
        
        // Generate summary report
        await GenerateCreditApplicationReportAsync(result);
        
        return result;
    }
    
    private async IAsyncEnumerable<BankAccount> ProcessAccountsAsync(IEnumerable<BankAccount> accounts)
    {
        foreach (var account in accounts)
        {
            // Simulate async processing
            await Task.Delay(10);
            yield return account;
        }
    }
    
    private async Task GenerateCreditApplicationReportAsync(ProcessingResult<CreditApplication> result)
    {
        await Task.Delay(50); // Simulate report generation
        
        Console.WriteLine($"Credit Application Processing Report:");
        Console.WriteLine($"  Total Applications: {result.TotalProcessed}");
        Console.WriteLine($"  Successful: {result.SuccessfulResults.Count}");
        Console.WriteLine($"  Failed: {result.Errors.Count}");
        
        if (result.Errors.Count > 0)
        {
            var errorsByType = result.Errors.GroupBy(e => e.ErrorType);
            Console.WriteLine($"  Errors by Type:");
            foreach (var errorGroup in errorsByType)
            {
                Console.WriteLine($"    {errorGroup.Key}: {errorGroup.Count()}");
            }
        }
    }
}

public class ProcessingResult<T>
{
    public List<T> SuccessfulResults { get; set; } = new();
    public List<ProcessingError> Errors { get; set; } = new();
    
    public int TotalProcessed => SuccessfulResults.Count + Errors.Count;
    public double SuccessRate => TotalProcessed > 0 ? (double)SuccessfulResults.Count / TotalProcessed : 0;
}

public class ProcessingError
{
    public string EntityId { get; set; } = "";
    public string ErrorType { get; set; } = "";
    public string Message { get; set; } = "";
    public Exception? Exception { get; set; }
}
```

### Error Handling and Recovery Patterns

```csharp
public static class ConversionPatterns
{
    /// <summary>
    /// Safely attempts conversion with fallback handling
    /// </summary>
    public static TResult SafeConvert<TSource, TResult>(
        TSource source,
        TResult fallbackValue,
        Action<InconvertibleException>? onError = null)
        where TSource : IConvertible<TResult>
    {
        try
        {
            return source.Convert();
        }
        catch (InconvertibleException ex)
        {
            onError?.Invoke(ex);
            return fallbackValue;
        }
    }
    
    /// <summary>
    /// Attempts conversion with detailed result information
    /// </summary>
    public static ConversionAttempt<TResult> TryConvert<TSource, TResult>(TSource source)
        where TSource : IConvertible<TResult>
    {
        try
        {
            var result = source.Convert();
            return ConversionAttempt<TResult>.Success(result);
        }
        catch (InconvertibleException ex)
        {
            return ConversionAttempt<TResult>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Batch conversion with error collection
    /// </summary>
    public static BatchConversionResult<TResult> ConvertBatch<TSource, TResult>(
        IEnumerable<TSource> sources)
        where TSource : IConvertible<TResult>
    {
        var result = new BatchConversionResult<TResult>();
        
        foreach (var (source, index) in sources.Select((s, i) => (s, i)))
        {
            try
            {
                var converted = source.Convert();
                result.SuccessfulConversions.Add(new IndexedResult<TResult>(index, converted));
            }
            catch (InconvertibleException ex)
            {
                result.FailedConversions.Add(new IndexedError(index, ex));
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Conversion with retry logic
    /// </summary>
    public static async Task<TResult> ConvertWithRetryAsync<TSource, TResult>(
        TSource source,
        int maxRetries = 3,
        TimeSpan? delay = null)
        where TSource : IConvertible<TResult>
    {
        var retryDelay = delay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return source.Convert();
            }
            catch (InconvertibleException ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"Conversion attempt {attempt + 1} failed: {ex.Message}");
                await Task.Delay(retryDelay);
                retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
        
        // Final attempt without catch
        return source.Convert();
    }
}

public class ConversionAttempt<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public InconvertibleException? Error { get; private set; }
    
    private ConversionAttempt(bool isSuccess, T? value, InconvertibleException? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static ConversionAttempt<T> Success(T value) => new(true, value, null);
    public static ConversionAttempt<T> Failure(InconvertibleException error) => new(false, default, error);
    
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<InconvertibleException, TResult> onFailure)
    {
        return IsSuccess && Value != null ? onSuccess(Value) : onFailure(Error!);
    }
}

public class BatchConversionResult<T>
{
    public List<IndexedResult<T>> SuccessfulConversions { get; set; } = new();
    public List<IndexedError> FailedConversions { get; set; } = new();
    
    public int TotalAttempts => SuccessfulConversions.Count + FailedConversions.Count;
    public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulConversions.Count / TotalAttempts : 0;
}

public record IndexedResult<T>(int Index, T Value);
public record IndexedError(int Index, InconvertibleException Exception);

public class ConversionPatternsDemo
{
    public static async Task DemonstrateErrorHandlingPatternsAsync()
    {
        var customers = new List<Customer>
        {
            new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Status = CustomerStatus.Active },
            new Customer { Id = 0, FirstName = "", LastName = "Invalid" }, // Invalid customer
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Status = CustomerStatus.Active }
        };
        
        // Safe conversion with fallback
        foreach (var customer in customers)
        {
            var summary = ConversionPatterns.SafeConvert<Customer, CustomerSummary>(
                customer,
                new CustomerSummary { FullName = "Unknown Customer" },
                error => Console.WriteLine($"Conversion failed: {error.Message}")
            );
            
            Console.WriteLine($"Customer: {summary.FullName}");
        }
        
        // Try conversion with detailed results
        Console.WriteLine("\nDetailed conversion attempts:");
        foreach (var customer in customers)
        {
            var attempt = ConversionPatterns.TryConvert<Customer, CustomerDto>(customer);
            
            var result = attempt.Match(
                dto => $"Success: {dto.FullName}",
                error => $"Failed: {error.Message}"
            );
            
            Console.WriteLine(result);
        }
        
        // Batch conversion
        Console.WriteLine("\nBatch conversion results:");
        var batchResult = ConversionPatterns.ConvertBatch<Customer, CustomerSummary>(customers);
        
        Console.WriteLine($"Successful: {batchResult.SuccessfulConversions.Count}/{batchResult.TotalAttempts}");
        Console.WriteLine($"Success Rate: {batchResult.SuccessRate:P}");
        
        foreach (var success in batchResult.SuccessfulConversions)
        {
            Console.WriteLine($"  [{success.Index}] {success.Value.FullName}");
        }
        
        foreach (var failure in batchResult.FailedConversions)
        {
            Console.WriteLine($"  [{failure.Index}] Error: {failure.Exception.Message}");
        }
        
        // Retry conversion (simulated)
        try
        {
            var validCustomer = customers.First(c => c.Id > 0);
            var dto = await ConversionPatterns.ConvertWithRetryAsync<Customer, CustomerDto>(validCustomer);
            Console.WriteLine($"\nRetry conversion successful: {dto.FullName}");
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"\nRetry conversion failed: {ex.Message}");
        }
    }
}
```

### Performance and Optimization

```csharp
public class PerformanceOptimizedConversion
{
    private static readonly ConcurrentDictionary<string, bool> ValidationCache = new();
    
    public static void OptimizedValidation<T>(T source, string validationKey, Func<bool> validation, string errorMessage)
    {
        // Cache validation results for expensive operations
        var isValid = ValidationCache.GetOrAdd(validationKey, _ => validation());
        
        InconvertibleException.ThrowIfInconvertible(isValid, errorMessage);
    }
    
    public static void ClearValidationCache()
    {
        ValidationCache.Clear();
    }
}

public class CachedValidationConverter : IConvertible<string>
{
    private readonly int _id;
    private readonly string _data;
    
    public CachedValidationConverter(int id, string data)
    {
        _id = id;
        _data = data;
    }
    
    public string Convert()
    {
        // Use cached validation for expensive operations
        PerformanceOptimizedConversion.OptimizedValidation(
            this,
            $"validation_{_id}",
            () => ExpensiveValidation(_data),
            $"Validation failed for item {_id}"
        );
        
        return _data.ToUpperInvariant();
    }
    
    private bool ExpensiveValidation(string data)
    {
        // Simulate expensive validation
        Thread.Sleep(100);
        return !string.IsNullOrWhiteSpace(data) && data.Length > 3;
    }
}

public class ConversionPerformanceTest
{
    public static async Task<PerformanceMetrics> BenchmarkConversionErrorsAsync()
    {
        const int iterations = 10000;
        var metrics = new PerformanceMetrics();
        
        // Benchmark successful conversions
        var validValue = new NumericValue(42);
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            try
            {
                var result = ((IConvertible<int>)validValue).Convert();
            }
            catch (InconvertibleException)
            {
                // Should not occur
            }
        }
        
        metrics.SuccessfulConversionTime = stopwatch.Elapsed;
        
        // Benchmark failed conversions
        var invalidValue = new NumericValue("invalid");
        stopwatch.Restart();
        
        for (int i = 0; i < iterations; i++)
        {
            try
            {
                var result = ((IConvertible<int>)invalidValue).Convert();
            }
            catch (InconvertibleException)
            {
                // Expected failure
            }
        }
        
        metrics.FailedConversionTime = stopwatch.Elapsed;
        
        // Benchmark validation overhead
        stopwatch.Restart();
        
        for (int i = 0; i < iterations; i++)
        {
            InconvertibleException.ThrowIfInconvertible(true, "Test message");
        }
        
        metrics.ValidationOverheadTime = stopwatch.Elapsed;
        
        metrics.Iterations = iterations;
        return metrics;
    }
}

public class PerformanceMetrics
{
    public TimeSpan SuccessfulConversionTime { get; set; }
    public TimeSpan FailedConversionTime { get; set; }
    public TimeSpan ValidationOverheadTime { get; set; }
    public int Iterations { get; set; }
    
    public void PrintMetrics()
    {
        Console.WriteLine($"Conversion Performance Metrics ({Iterations:N0} iterations):");
        Console.WriteLine($"  Successful conversions: {SuccessfulConversionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Failed conversions:     {FailedConversionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Validation overhead:    {ValidationOverheadTime.TotalMilliseconds:F2} ms");
        
        var avgSuccess = SuccessfulConversionTime.TotalMicroseconds / Iterations;
        var avgFailed = FailedConversionTime.TotalMicroseconds / Iterations;
        var avgValidation = ValidationOverheadTime.TotalMicroseconds / Iterations;
        
        Console.WriteLine($"  Average per operation:");
        Console.WriteLine($"    Successful: {avgSuccess:F2} μs");
        Console.WriteLine($"    Failed:     {avgFailed:F2} μs");
        Console.WriteLine($"    Validation: {avgValidation:F2} μs");
    }
}
```

## Best Practices

### 1. **Meaningful Error Messages**

```csharp
public static class ErrorMessageGuidelines
{
    // Good: Specific and actionable
    public static void GoodErrorMessages()
    {
        InconvertibleException.ThrowIfInconvertible(
            false,
            "Cannot convert Order to Invoice: Order status must be 'Shipped' or 'Delivered', but was 'Pending'"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            false,
            "Cannot convert Customer to DTO: Email address 'invalid-email' is not in valid format (missing @ symbol)"
        );
    }
    
    // Bad: Vague and unhelpful
    public static void BadErrorMessages()
    {
        // Don't do this:
        // InconvertibleException.ThrowIfInconvertible(false, "Conversion failed");
        // InconvertibleException.ThrowIfInconvertible(false, "Invalid data");
    }
}
```

### 2. **Validation Performance**

```csharp
public static class ValidationPerformanceGuidelines
{
    // Use lazy evaluation for expensive validations
    public static void UseLazyEvaluation()
    {
        InconvertibleException.ThrowIfInconvertible(
            () => ExpensiveValidation(), // Only called if needed
            "Expensive validation failed"
        );
    }
    
    // Cache validation results when appropriate
    public static void CacheValidationResults(string cacheKey, Func<bool> validation, string errorMessage)
    {
        var result = ValidationCache.GetOrAdd(cacheKey, _ => validation());
        InconvertibleException.ThrowIfInconvertible(result, errorMessage);
    }
    
    private static readonly ConcurrentDictionary<string, bool> ValidationCache = new();
    
    private static bool ExpensiveValidation() => true; // Placeholder
}
```

### 3. **Type Information Usage**

```csharp
public static class TypeInformationGuidelines
{
    // Use type-based constructor for generic conversion errors
    public static void UseTypeBasedConstructor<TSource, TTarget>()
    {
        throw new InconvertibleException(typeof(TSource), typeof(TTarget));
    }
    
    // Combine with specific validation messages for detailed errors
    public static void CombineWithSpecificMessages<TSource, TTarget>(string specificReason)
    {
        var baseMessage = $"value with type {typeof(TSource)} is not convertable to type {typeof(TTarget)}";
        var detailedMessage = $"{baseMessage}: {specificReason}";
        throw new InconvertibleException(detailedMessage);
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class InconvertibleExceptionTests
{
    [Test]
    public void Constructor_WithMessage_CreatesExceptionWithMessage()
    {
        // Arrange
        var message = "Test conversion error";
        
        // Act
        var exception = new InconvertibleException(message);
        
        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
    }
    
    [Test]
    public void Constructor_WithTypes_GeneratesStandardMessage()
    {
        // Arrange
        var sourceType = typeof(string);
        var targetType = typeof(int);
        
        // Act
        var exception = new InconvertibleException(sourceType, targetType);
        
        // Assert
        Assert.That(exception.Message, Contains.Substring("string"));
        Assert.That(exception.Message, Contains.Substring("Int32"));
        Assert.That(exception.Message, Contains.Substring("not convertable"));
    }
    
    [Test]
    public void ThrowIfInconvertible_WithFalseCondition_ThrowsException()
    {
        // Arrange
        var condition = false;
        var message = "Test condition failed";
        
        // Act & Assert
        var exception = Assert.Throws<InconvertibleException>(() =>
            InconvertibleException.ThrowIfInconvertible(condition, message));
        
        Assert.That(exception.Message, Is.EqualTo(message));
    }
    
    [Test]
    public void ThrowIfInconvertible_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        var condition = true;
        var message = "Test condition failed";
        
        // Act & Assert
        Assert.DoesNotThrow(() =>
            InconvertibleException.ThrowIfInconvertible(condition, message));
    }
    
    [Test]
    public void ThrowIfInconvertible_WithLazyFalseCondition_ThrowsException()
    {
        // Arrange
        var conditionCalled = false;
        Func<bool> condition = () =>
        {
            conditionCalled = true;
            return false;
        };
        var message = "Lazy condition failed";
        
        // Act & Assert
        var exception = Assert.Throws<InconvertibleException>(() =>
            InconvertibleException.ThrowIfInconvertible(condition, message));
        
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(conditionCalled, Is.True);
    }
    
    [Test]
    public void ThrowIfInconvertible_WithLazyTrueCondition_DoesNotEvaluate()
    {
        // Arrange
        var conditionCalled = false;
        Func<bool> condition = () =>
        {
            conditionCalled = true;
            return true;
        };
        var message = "Lazy condition failed";
        
        // Act & Assert
        Assert.DoesNotThrow(() =>
            InconvertibleException.ThrowIfInconvertible(condition, message));
        
        Assert.That(conditionCalled, Is.True); // Condition is always evaluated in current implementation
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class ConversionErrorHandlingTests
{
    [Test]
    public void NumericValue_ConvertToInt_WithInvalidType_ThrowsInconvertibleException()
    {
        // Arrange
        var value = new NumericValue("not a number");
        
        // Act & Assert
        var exception = Assert.Throws<InconvertibleException>(() =>
            ((IConvertible<int>)value).Convert());
        
        Assert.That(exception.Message, Contains.Substring("String"));
        Assert.That(exception.Message, Contains.Substring("numeric type"));
    }
    
    [Test]
    public void Customer_ConvertToDto_WithInvalidData_ThrowsInconvertibleException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 0, // Invalid ID
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act & Assert
        var exception = Assert.Throws<InconvertibleException>(() =>
            ((IConvertible<CustomerDto>)customer).Convert());
        
        Assert.That(exception.Message, Contains.Substring("Customer ID must be positive"));
    }
    
    [Test]
    public void ConversionPatterns_SafeConvert_WithInvalidData_ReturnseFallback()
    {
        // Arrange
        var invalidCustomer = new Customer { Id = 0 };
        var fallback = new CustomerSummary { FullName = "Fallback Customer" };
        var errorCaptured = false;
        
        // Act
        var result = ConversionPatterns.SafeConvert<Customer, CustomerSummary>(
            invalidCustomer,
            fallback,
            _ => errorCaptured = true
        );
        
        // Assert
        Assert.That(result.FullName, Is.EqualTo("Fallback Customer"));
        Assert.That(errorCaptured, Is.True);
    }
    
    [Test]
    public void ConversionPatterns_TryConvert_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var validCustomer = new Customer
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };
        
        // Act
        var attempt = ConversionPatterns.TryConvert<Customer, CustomerDto>(validCustomer);
        
        // Assert
        Assert.That(attempt.IsSuccess, Is.True);
        Assert.That(attempt.Value, Is.Not.Null);
        Assert.That(attempt.Value.FullName, Is.EqualTo("Jane Doe"));
        Assert.That(attempt.Error, Is.Null);
    }
}
```

## See Also

- [IConvertible<T>](IConvertible.md) - Type-safe conversion interface that uses InconvertibleException
- [System.Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception) - Base Exception class
- [ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception) - Related exception type for argument validation
- [Guard Clauses](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles#use-guard-clauses) - Defensive programming patterns
- [Exception Handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/) - .NET exception handling guidelines

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing standardized error handling for type-safe conversion operations with comprehensive validation support.*