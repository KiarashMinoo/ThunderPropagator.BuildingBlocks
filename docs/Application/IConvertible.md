# IConvertible&lt;T&gt;

The `IConvertible<T>` interface provides a type-safe alternative to the standard .NET `IConvertible` interface, enabling objects to define custom conversion logic to specific target types. This interface ensures compile-time type safety and eliminates the need for runtime type checking and casting during conversion operations.

## Overview

```csharp
public interface IConvertible<out T>
{
    T Convert();
}
```

The generic `IConvertible<T>` interface addresses the limitations of the built-in `System.IConvertible` interface by providing:
- **Type Safety**: Returns `T` instead of requiring runtime type specification
- **Covariance**: Uses the `out` keyword to support covariant return types
- **Compile-Time Checking**: Ensures conversion target types are validated at compile time
- **Custom Logic**: Allows implementation-specific conversion algorithms
- **Performance**: Eliminates boxing and runtime type checking overhead

## Key Features

### Type Safety
- **Strong Typing**: Returns the specific type `T` without requiring casts
- **Compile-Time Validation**: Catches conversion errors during compilation
- **No Runtime Type Checks**: Eliminates `typeof()` comparisons and `is` checks
- **Generic Constraints**: Can be used with generic type constraints

### Covariance Support
- **Covariant Interface**: Supports inheritance hierarchies with `out T`
- **Polymorphic Conversions**: Enables conversions to base types
- **Interface Composition**: Works seamlessly with other covariant interfaces
- **Flexible Assignments**: Allows assignment to more general interface types

### Custom Conversion Logic
- **Implementation-Specific**: Each type defines its own conversion semantics
- **Context-Aware**: Can incorporate business rules and validation
- **Error Handling**: Integrates with `InconvertibleException` for consistent error reporting
- **Performance Optimized**: Avoids reflection and dynamic dispatch

## Related Types

### InconvertibleException
```csharp
public class InconvertibleException : Exception
{
    public InconvertibleException(string message);
    public InconvertibleException(Type sourceType, Type destinationType);
    
    public static void ThrowIfInconvertible(bool condition, string message);
    public static void ThrowIfInconvertible(Func<bool> condition, string message);
}
```

The `InconvertibleException` provides standardized error handling for conversion failures with helpful type information and validation utilities.

## Usage Examples

### Basic Type Conversion

```csharp
public class Temperature : IConvertible<double>, IConvertible<string>
{
    public double Celsius { get; }
    public TemperatureScale Scale { get; }
    
    public Temperature(double value, TemperatureScale scale = TemperatureScale.Celsius)
    {
        Celsius = scale switch
        {
            TemperatureScale.Celsius => value,
            TemperatureScale.Fahrenheit => (value - 32) * 5 / 9,
            TemperatureScale.Kelvin => value - 273.15,
            _ => throw new ArgumentException($"Unsupported temperature scale: {scale}")
        };
        Scale = scale;
    }
    
    // Convert to double (Celsius value)
    double IConvertible<double>.Convert()
    {
        return Celsius;
    }
    
    // Convert to string representation
    string IConvertible<string>.Convert()
    {
        return Scale switch
        {
            TemperatureScale.Celsius => $"{Celsius:F1}°C",
            TemperatureScale.Fahrenheit => $"{ToFahrenheit():F1}°F",
            TemperatureScale.Kelvin => $"{ToKelvin():F1}K",
            _ => $"{Celsius:F1}°C"
        };
    }
    
    public double ToFahrenheit() => Celsius * 9 / 5 + 32;
    public double ToKelvin() => Celsius + 273.15;
    
    public Temperature ToScale(TemperatureScale targetScale)
    {
        return new Temperature(Celsius, TemperatureScale.Celsius).ConvertToScale(targetScale);
    }
    
    private Temperature ConvertToScale(TemperatureScale targetScale)
    {
        var value = targetScale switch
        {
            TemperatureScale.Celsius => Celsius,
            TemperatureScale.Fahrenheit => ToFahrenheit(),
            TemperatureScale.Kelvin => ToKelvin(),
            _ => throw new InconvertibleException($"Cannot convert to scale {targetScale}")
        };
        
        return new Temperature(value, targetScale);
    }
}

public enum TemperatureScale
{
    Celsius,
    Fahrenheit,
    Kelvin
}

public class TemperatureConverter
{
    public static void DemonstrateBasicConversion()
    {
        var temp = new Temperature(25.0, TemperatureScale.Celsius);
        
        // Type-safe conversions
        var celsiusValue = ((IConvertible<double>)temp).Convert();
        var stringRepresentation = ((IConvertible<string>)temp).Convert();
        
        Console.WriteLine($"Celsius: {celsiusValue}");
        Console.WriteLine($"Display: {stringRepresentation}");
        
        // Convert to different scales
        var fahrenheitTemp = temp.ToScale(TemperatureScale.Fahrenheit);
        var kelvinTemp = temp.ToScale(TemperatureScale.Kelvin);
        
        Console.WriteLine($"Fahrenheit: {((IConvertible<string>)fahrenheitTemp).Convert()}");
        Console.WriteLine($"Kelvin: {((IConvertible<string>)kelvinTemp).Convert()}");
    }
}
```

### Data Transfer Object Conversion

```csharp
public class UserEntity : IConvertible<UserDto>, IConvertible<UserSummaryDto>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public UserStatus Status { get; set; }
    public List<string> Roles { get; set; } = new();
    public UserProfile Profile { get; set; } = new();
    
    // Convert to full DTO
    UserDto IConvertible<UserDto>.Convert()
    {
        return new UserDto
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName,
            FullName = $"{this.FirstName} {this.LastName}".Trim(),
            Email = this.Email,
            CreatedAt = this.CreatedAt,
            LastLoginAt = this.LastLoginAt,
            Status = this.Status.ToString(),
            Roles = new List<string>(this.Roles),
            Profile = this.Profile.ConvertToDto()
        };
    }
    
    // Convert to summary DTO
    UserSummaryDto IConvertible<UserSummaryDto>.Convert()
    {
        return new UserSummaryDto
        {
            Id = this.Id,
            FullName = $"{this.FirstName} {this.LastName}".Trim(),
            Email = this.Email,
            Status = this.Status.ToString(),
            IsActive = this.Status == UserStatus.Active,
            LastActivity = this.LastLoginAt ?? this.CreatedAt
        };
    }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string Status { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public UserProfileDto Profile { get; set; } = new();
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime LastActivity { get; set; }
}

public class UserProfile : IConvertible<UserProfileDto>
{
    public string PhoneNumber { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Country { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public Dictionary<string, string> CustomFields { get; set; } = new();
    
    UserProfileDto IConvertible<UserProfileDto>.Convert()
    {
        return ConvertToDto();
    }
    
    internal UserProfileDto ConvertToDto()
    {
        return new UserProfileDto
        {
            PhoneNumber = this.PhoneNumber,
            DateOfBirth = this.DateOfBirth,
            Country = this.Country,
            TimeZone = this.TimeZone,
            CustomFields = new Dictionary<string, string>(this.CustomFields)
        };
    }
}

public class UserProfileDto
{
    public string PhoneNumber { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Country { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public enum UserStatus
{
    Pending,
    Active,
    Suspended,
    Deactivated
}

public class UserService
{
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var users = await GetUserEntitiesAsync();
        return users.Select(u => ((IConvertible<UserDto>)u).Convert()).ToList();
    }
    
    public async Task<List<UserSummaryDto>> GetUserSummariesAsync()
    {
        var users = await GetUserEntitiesAsync();
        return users.Select(u => ((IConvertible<UserSummaryDto>)u).Convert()).ToList();
    }
    
    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await GetUserEntityByIdAsync(id);
        return user != null ? ((IConvertible<UserDto>)user).Convert() : null;
    }
    
    private async Task<List<UserEntity>> GetUserEntitiesAsync()
    {
        // Simulate database access
        await Task.Delay(100);
        return new List<UserEntity>
        {
            new UserEntity
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddHours(-2),
                Status = UserStatus.Active,
                Roles = new List<string> { "User", "Editor" },
                Profile = new UserProfile
                {
                    PhoneNumber = "+1-555-123-4567",
                    Country = "United States",
                    TimeZone = "America/New_York"
                }
            },
            new UserEntity
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                LastLoginAt = DateTime.UtcNow.AddDays(-1),
                Status = UserStatus.Active,
                Roles = new List<string> { "User", "Admin" },
                Profile = new UserProfile
                {
                    PhoneNumber = "+1-555-987-6543",
                    Country = "Canada",
                    TimeZone = "America/Toronto"
                }
            }
        };
    }
    
    private async Task<UserEntity?> GetUserEntityByIdAsync(int id)
    {
        var users = await GetUserEntitiesAsync();
        return users.FirstOrDefault(u => u.Id == id);
    }
}
```

### Complex Object Conversion with Validation

```csharp
public class Order : IConvertible<OrderSummary>, IConvertible<OrderInvoice>
{
    public string OrderId { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public PaymentInfo Payment { get; set; } = new();
    
    public decimal TotalAmount => Items.Sum(i => i.Quantity * i.Price);
    public decimal TaxAmount => TotalAmount * 0.08m; // 8% tax
    public decimal FinalAmount => TotalAmount + TaxAmount;
    
    // Convert to order summary
    OrderSummary IConvertible<OrderSummary>.Convert()
    {
        InconvertibleException.ThrowIfInconvertible(
            !string.IsNullOrEmpty(OrderId),
            "Cannot convert order to summary: OrderId is required"
        );
        
        return new OrderSummary
        {
            OrderId = this.OrderId,
            CustomerId = this.CustomerId,
            OrderDate = this.OrderDate,
            Status = this.Status.ToString(),
            ItemCount = this.Items.Count,
            TotalAmount = this.TotalAmount,
            FinalAmount = this.FinalAmount
        };
    }
    
    // Convert to invoice
    OrderInvoice IConvertible<OrderInvoice>.Convert()
    {
        InconvertibleException.ThrowIfInconvertible(
            this.Status is OrderStatus.Shipped or OrderStatus.Delivered,
            $"Cannot convert order to invoice: Order status must be Shipped or Delivered, but was {Status}"
        );
        
        InconvertibleException.ThrowIfInconvertible(
            this.Payment.IsCompleted,
            "Cannot convert order to invoice: Payment must be completed"
        );
        
        return new OrderInvoice
        {
            InvoiceId = $"INV-{this.OrderId}",
            OrderId = this.OrderId,
            CustomerId = this.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            Items = this.Items.Select(item => new InvoiceItem
            {
                Description = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                LineTotal = item.Quantity * item.Price
            }).ToList(),
            SubTotal = this.TotalAmount,
            TaxAmount = this.TaxAmount,
            TotalAmount = this.FinalAmount,
            BillingAddress = this.BillingAddress,
            PaymentMethod = this.Payment.Method,
            PaymentReference = this.Payment.TransactionId
        };
    }
}

public class OrderItem
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string Country { get; set; } = "";
}

public class PaymentInfo
{
    public string Method { get; set; } = "";
    public string TransactionId { get; set; } = "";
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderSummary
{
    public string OrderId { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "";
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class OrderInvoice
{
    public string InvoiceId { get; set; } = "";
    public string OrderId { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public DateTime InvoiceDate { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Address BillingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = "";
    public string PaymentReference { get; set; } = "";
}

public class InvoiceItem
{
    public string Description { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderProcessor
{
    public List<OrderSummary> GetOrderSummaries(IEnumerable<Order> orders)
    {
        var summaries = new List<OrderSummary>();
        
        foreach (var order in orders)
        {
            try
            {
                var summary = ((IConvertible<OrderSummary>)order).Convert();
                summaries.Add(summary);
            }
            catch (InconvertibleException ex)
            {
                Console.WriteLine($"Failed to convert order {order.OrderId} to summary: {ex.Message}");
            }
        }
        
        return summaries;
    }
    
    public List<OrderInvoice> GenerateInvoices(IEnumerable<Order> orders)
    {
        var invoices = new List<OrderInvoice>();
        
        foreach (var order in orders)
        {
            try
            {
                var invoice = ((IConvertible<OrderInvoice>)order).Convert();
                invoices.Add(invoice);
            }
            catch (InconvertibleException ex)
            {
                Console.WriteLine($"Failed to generate invoice for order {order.OrderId}: {ex.Message}");
            }
        }
        
        return invoices;
    }
    
    public void ProcessOrderBatch(List<Order> orders)
    {
        Console.WriteLine($"Processing {orders.Count} orders...");
        
        // Generate summaries (should work for all orders)
        var summaries = GetOrderSummaries(orders);
        Console.WriteLine($"Generated {summaries.Count} order summaries");
        
        // Generate invoices (only for completed orders)
        var invoices = GenerateInvoices(orders);
        Console.WriteLine($"Generated {invoices.Count} invoices");
        
        // Display results
        foreach (var summary in summaries)
        {
            Console.WriteLine($"Order {summary.OrderId}: {summary.ItemCount} items, ${summary.FinalAmount:F2}");
        }
        
        foreach (var invoice in invoices)
        {
            Console.WriteLine($"Invoice {invoice.InvoiceId}: ${invoice.TotalAmount:F2}");
        }
    }
}
```

### Generic Conversion Utilities

```csharp
public static class ConversionExtensions
{
    /// <summary>
    /// Safely converts an object if it implements IConvertible&lt;T&gt;
    /// </summary>
    public static T? ConvertTo<T>(this object? source) where T : class
    {
        return source is IConvertible<T> convertible ? convertible.Convert() : null;
    }
    
    /// <summary>
    /// Converts an object with a fallback value if conversion fails
    /// </summary>
    public static T ConvertToOrDefault<T>(this object source, T defaultValue) where T : class
    {
        try
        {
            return source is IConvertible<T> convertible ? convertible.Convert() : defaultValue;
        }
        catch (InconvertibleException)
        {
            return defaultValue;
        }
    }
    
    /// <summary>
    /// Attempts to convert an object, returning success status and result
    /// </summary>
    public static bool TryConvertTo<T>(this object? source, out T? result) where T : class
    {
        try
        {
            if (source is IConvertible<T> convertible)
            {
                result = convertible.Convert();
                return true;
            }
        }
        catch (InconvertibleException)
        {
            // Conversion failed
        }
        
        result = null;
        return false;
    }
    
    /// <summary>
    /// Converts a collection of objects to target type
    /// </summary>
    public static List<T> ConvertAll<T>(this IEnumerable<object> source) where T : class
    {
        var results = new List<T>();
        
        foreach (var item in source)
        {
            if (item.TryConvertTo<T>(out var converted) && converted != null)
            {
                results.Add(converted);
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Converts objects with detailed error reporting
    /// </summary>
    public static ConversionResult<T> SafeConvertTo<T>(this object? source) where T : class
    {
        if (source == null)
        {
            return ConversionResult<T>.Failure("Source object is null");
        }
        
        if (source is not IConvertible<T> convertible)
        {
            return ConversionResult<T>.Failure($"Source type {source.GetType().Name} does not implement IConvertible<{typeof(T).Name}>");
        }
        
        try
        {
            var result = convertible.Convert();
            return ConversionResult<T>.Success(result);
        }
        catch (InconvertibleException ex)
        {
            return ConversionResult<T>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return ConversionResult<T>.Failure($"Unexpected error during conversion: {ex.Message}");
        }
    }
}

public class ConversionResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private ConversionResult(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }
    
    public static ConversionResult<T> Success(T value) => new(true, value, null);
    public static ConversionResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess && Value != null ? onSuccess(Value) : onFailure(ErrorMessage ?? "Unknown error");
    }
}

public class ConversionUtilityDemo
{
    public static void DemonstrateConversionUtilities()
    {
        var user = new UserEntity
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@example.com",
            Status = UserStatus.Active
        };
        
        var temperature = new Temperature(30.0, TemperatureScale.Celsius);
        
        var objects = new List<object> { user, temperature, "not convertible" };
        
        // Safe single conversions
        var userDto = user.ConvertTo<UserDto>();
        var userSummary = user.ConvertTo<UserSummaryDto>();
        var tempString = temperature.ConvertTo<string>();
        
        Console.WriteLine($"UserDto: {userDto?.FullName}");
        Console.WriteLine($"UserSummary: {userSummary?.FullName}");
        Console.WriteLine($"Temperature: {tempString}");
        
        // Try conversions with error handling
        foreach (var obj in objects)
        {
            var userResult = obj.SafeConvertTo<UserDto>();
            userResult.Match(
                dto => Console.WriteLine($"Successfully converted to UserDto: {dto.FullName}"),
                error => Console.WriteLine($"Conversion to UserDto failed: {error}")
            );
            
            var stringResult = obj.SafeConvertTo<string>();
            stringResult.Match(
                str => Console.WriteLine($"Successfully converted to string: {str}"),
                error => Console.WriteLine($"Conversion to string failed: {error}")
            );
        }
        
        // Batch conversions
        var userDtos = objects.ConvertAll<UserDto>();
        Console.WriteLine($"Converted {userDtos.Count} objects to UserDto");
    }
}
```

### Polymorphic Conversion with Covariance

```csharp
public abstract class Document : IConvertible<Document>
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; } = "";
    
    // Abstract conversion - implemented by derived classes
    public abstract Document Convert();
}

public class TextDocument : Document, IConvertible<TextDocument>, IConvertible<string>
{
    public string Content { get; set; } = "";
    public Encoding TextEncoding { get; set; } = Encoding.UTF8;
    
    // Convert to same type (copy)
    TextDocument IConvertible<TextDocument>.Convert()
    {
        return new TextDocument
        {
            Id = this.Id,
            Title = this.Title,
            CreatedAt = this.CreatedAt,
            Author = this.Author,
            Content = this.Content,
            TextEncoding = this.TextEncoding
        };
    }
    
    // Convert to base type
    public override Document Convert()
    {
        return ((IConvertible<TextDocument>)this).Convert();
    }
    
    // Convert to string representation
    string IConvertible<string>.Convert()
    {
        return $"Title: {Title}\nAuthor: {Author}\nCreated: {CreatedAt:yyyy-MM-dd}\n\n{Content}";
    }
}

public class PdfDocument : Document, IConvertible<PdfDocument>, IConvertible<byte[]>
{
    public byte[] PdfData { get; set; } = Array.Empty<byte>();
    public string Version { get; set; } = "1.4";
    public bool IsEncrypted { get; set; }
    
    // Convert to same type (copy)
    PdfDocument IConvertible<PdfDocument>.Convert()
    {
        return new PdfDocument
        {
            Id = this.Id,
            Title = this.Title,
            CreatedAt = this.CreatedAt,
            Author = this.Author,
            PdfData = (byte[])this.PdfData.Clone(),
            Version = this.Version,
            IsEncrypted = this.IsEncrypted
        };
    }
    
    // Convert to base type
    public override Document Convert()
    {
        return ((IConvertible<PdfDocument>)this).Convert();
    }
    
    // Convert to byte array
    byte[] IConvertible<byte[]>.Convert()
    {
        InconvertibleException.ThrowIfInconvertible(
            !IsEncrypted,
            "Cannot convert encrypted PDF to byte array without decryption"
        );
        
        return (byte[])PdfData.Clone();
    }
}

public class ImageDocument : Document, IConvertible<ImageDocument>, IConvertible<Stream>
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "PNG";
    public int Width { get; set; }
    public int Height { get; set; }
    
    // Convert to same type (copy)
    ImageDocument IConvertible<ImageDocument>.Convert()
    {
        return new ImageDocument
        {
            Id = this.Id,
            Title = this.Title,
            CreatedAt = this.CreatedAt,
            Author = this.Author,
            ImageData = (byte[])this.ImageData.Clone(),
            Format = this.Format,
            Width = this.Width,
            Height = this.Height
        };
    }
    
    // Convert to base type
    public override Document Convert()
    {
        return ((IConvertible<ImageDocument>)this).Convert();
    }
    
    // Convert to stream
    Stream IConvertible<Stream>.Convert()
    {
        return new MemoryStream((byte[])ImageData.Clone());
    }
}

public class DocumentProcessor
{
    public List<Document> ProcessDocuments(IEnumerable<Document> documents)
    {
        var processedDocuments = new List<Document>();
        
        foreach (var doc in documents)
        {
            // Each document converts to its own type via polymorphism
            var processedDoc = doc.Convert();
            
            // Type-specific processing
            switch (processedDoc)
            {
                case TextDocument textDoc:
                    ProcessTextDocument(textDoc);
                    break;
                    
                case PdfDocument pdfDoc:
                    ProcessPdfDocument(pdfDoc);
                    break;
                    
                case ImageDocument imageDoc:
                    ProcessImageDocument(imageDoc);
                    break;
            }
            
            processedDocuments.Add(processedDoc);
        }
        
        return processedDocuments;
    }
    
    private void ProcessTextDocument(TextDocument doc)
    {
        // Text-specific processing
        if (string.IsNullOrEmpty(doc.Content))
        {
            doc.Content = "[Empty Document]";
        }
        
        // Convert to string for processing
        var textContent = ((IConvertible<string>)doc).Convert();
        Console.WriteLine($"Processed text document: {textContent.Length} characters");
    }
    
    private void ProcessPdfDocument(PdfDocument doc)
    {
        // PDF-specific processing
        if (doc.PdfData.Length == 0)
        {
            Console.WriteLine($"Warning: PDF document {doc.Id} has no data");
            return;
        }
        
        try
        {
            // Convert to byte array for processing
            var pdfBytes = ((IConvertible<byte[]>)doc).Convert();
            Console.WriteLine($"Processed PDF document: {pdfBytes.Length} bytes");
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"Cannot process encrypted PDF {doc.Id}: {ex.Message}");
        }
    }
    
    private void ProcessImageDocument(ImageDocument doc)
    {
        // Image-specific processing
        if (doc.ImageData.Length == 0)
        {
            Console.WriteLine($"Warning: Image document {doc.Id} has no data");
            return;
        }
        
        // Convert to stream for processing
        using var imageStream = ((IConvertible<Stream>)doc).Convert();
        Console.WriteLine($"Processed image document: {doc.Width}x{doc.Height} {doc.Format}");
    }
    
    public void DemonstratePolymorphicConversion()
    {
        var documents = new List<Document>
        {
            new TextDocument
            {
                Id = "DOC-001",
                Title = "Sample Text",
                Author = "John Doe",
                Content = "This is a sample text document.",
                CreatedAt = DateTime.UtcNow
            },
            new PdfDocument
            {
                Id = "DOC-002",
                Title = "Sample PDF",
                Author = "Jane Smith",
                PdfData = new byte[1024], // Simulated PDF data
                Version = "1.7",
                CreatedAt = DateTime.UtcNow
            },
            new ImageDocument
            {
                Id = "DOC-003",
                Title = "Sample Image",
                Author = "Bob Johnson",
                ImageData = new byte[2048], // Simulated image data
                Format = "JPEG",
                Width = 800,
                Height = 600,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        // Process all documents polymorphically
        var processedDocs = ProcessDocuments(documents);
        
        Console.WriteLine($"Processed {processedDocs.Count} documents");
        
        // Demonstrate covariance
        IConvertible<Document> documentConverter = new TextDocument { Title = "Covariant Test" };
        Document convertedDoc = documentConverter.Convert(); // Returns Document, not TextDocument
        
        Console.WriteLine($"Covariant conversion result: {convertedDoc.Title} (Type: {convertedDoc.GetType().Name})");
    }
}
```

### Conversion Pipeline and Chaining

```csharp
public class ConversionPipeline<TSource>
{
    private readonly TSource _source;
    
    public ConversionPipeline(TSource source)
    {
        _source = source;
    }
    
    public ConversionPipeline<TTarget> Convert<TTarget>() where TTarget : class
    {
        if (_source is IConvertible<TTarget> convertible)
        {
            var converted = convertible.Convert();
            return new ConversionPipeline<TTarget>(converted);
        }
        
        throw new InconvertibleException($"Cannot convert from {typeof(TSource).Name} to {typeof(TTarget).Name}");
    }
    
    public ConversionPipeline<TTarget> TryConvert<TTarget>(TTarget fallback) where TTarget : class
    {
        try
        {
            return Convert<TTarget>();
        }
        catch (InconvertibleException)
        {
            return new ConversionPipeline<TTarget>(fallback);
        }
    }
    
    public TResult Execute<TResult>(Func<TSource, TResult> operation)
    {
        return operation(_source);
    }
    
    public ConversionPipeline<TSource> Validate(Func<TSource, bool> predicate, string errorMessage)
    {
        InconvertibleException.ThrowIfInconvertible(predicate(_source), errorMessage);
        return this;
    }
    
    public ConversionPipeline<TSource> Process(Action<TSource> processor)
    {
        processor(_source);
        return this;
    }
    
    public TSource Result => _source;
}

public static class ConversionPipelineExtensions
{
    public static ConversionPipeline<T> ToPipeline<T>(this T source)
    {
        return new ConversionPipeline<T>(source);
    }
}

public class PipelineDemo
{
    public static void DemonstrateConversionPipeline()
    {
        var user = new UserEntity
        {
            Id = 1,
            FirstName = "Charlie",
            LastName = "Brown",
            Email = "charlie@example.com",
            Status = UserStatus.Active
        };
        
        try
        {
            // Conversion pipeline with validation and processing
            var result = user
                .ToPipeline()
                .Validate(u => !string.IsNullOrEmpty(u.Email), "Email is required")
                .Process(u => Console.WriteLine($"Processing user: {u.FirstName} {u.LastName}"))
                .Convert<UserDto>()
                .Validate(dto => dto.Id > 0, "User ID must be positive")
                .Process(dto => Console.WriteLine($"Converted to DTO: {dto.FullName}"))
                .Convert<UserSummaryDto>()
                .Process(summary => Console.WriteLine($"Final summary: {summary.FullName} ({summary.Status})"))
                .Result;
            
            Console.WriteLine($"Pipeline result: {result.FullName}");
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"Pipeline failed: {ex.Message}");
        }
        
        // Pipeline with fallback handling
        var temperatureResult = new Temperature(100, TemperatureScale.Celsius)
            .ToPipeline()
            .Convert<string>()
            .TryConvert<UserDto>(new UserDto { FullName = "Fallback User" })
            .Execute(dto => dto.FullName);
        
        Console.WriteLine($"Fallback result: {temperatureResult}");
    }
}
```

## Performance Considerations

### Optimized Conversion Implementations

```csharp
public class PerformanceOptimizedConverter : IConvertible<string>, IConvertible<byte[]>
{
    private readonly byte[] _data;
    private string? _cachedString;
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;
    
    public PerformanceOptimizedConverter(byte[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }
    
    // Cached string conversion
    string IConvertible<string>.Convert()
    {
        return _cachedString ??= DefaultEncoding.GetString(_data);
    }
    
    // Direct byte array conversion (no allocation)
    byte[] IConvertible<byte[]>.Convert()
    {
        return _data; // Return reference for performance (caller should not modify)
    }
}

public class LargeObjectConverter : IConvertible<Stream>, IConvertible<IAsyncEnumerable<byte>>
{
    private readonly byte[] _largeData;
    
    public LargeObjectConverter(byte[] largeData)
    {
        _largeData = largeData;
    }
    
    // Stream conversion for large data
    Stream IConvertible<Stream>.Convert()
    {
        return new MemoryStream(_largeData, false); // Read-only stream
    }
    
    // Async enumerable for chunked processing
    IAsyncEnumerable<byte> IConvertible<IAsyncEnumerable<byte>>.Convert()
    {
        return ConvertToAsyncEnumerable();
    }
    
    private async IAsyncEnumerable<byte> ConvertToAsyncEnumerable()
    {
        const int chunkSize = 1024;
        
        for (int i = 0; i < _largeData.Length; i += chunkSize)
        {
            var remainingBytes = Math.Min(chunkSize, _largeData.Length - i);
            
            for (int j = 0; j < remainingBytes; j++)
            {
                yield return _largeData[i + j];
            }
            
            // Yield control for other operations
            await Task.Yield();
        }
    }
}

public class ConversionPerformanceTest
{
    public static async Task<ConversionBenchmark> BenchmarkConversionsAsync()
    {
        const int iterations = 100_000;
        var benchmark = new ConversionBenchmark();
        
        // Test simple conversions
        var temp = new Temperature(25.0);
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var value = ((IConvertible<double>)temp).Convert();
        }
        
        benchmark.SimpleConversionTime = stopwatch.Elapsed;
        
        // Test complex conversions
        var user = new UserEntity { FirstName = "Test", LastName = "User", Email = "test@example.com" };
        stopwatch.Restart();
        
        for (int i = 0; i < iterations; i++)
        {
            var dto = ((IConvertible<UserDto>)user).Convert();
        }
        
        benchmark.ComplexConversionTime = stopwatch.Elapsed;
        
        // Test cached conversions
        var data = new byte[1024];
        var converter = new PerformanceOptimizedConverter(data);
        stopwatch.Restart();
        
        for (int i = 0; i < iterations; i++)
        {
            var str = ((IConvertible<string>)converter).Convert();
        }
        
        benchmark.CachedConversionTime = stopwatch.Elapsed;
        
        benchmark.Iterations = iterations;
        return benchmark;
    }
}

public class ConversionBenchmark
{
    public TimeSpan SimpleConversionTime { get; set; }
    public TimeSpan ComplexConversionTime { get; set; }
    public TimeSpan CachedConversionTime { get; set; }
    public int Iterations { get; set; }
    
    public void PrintResults()
    {
        Console.WriteLine($"Conversion Performance Benchmark ({Iterations:N0} iterations):");
        Console.WriteLine($"  Simple Conversion:   {SimpleConversionTime.TotalMilliseconds:F2} ms ({SimpleConversionTime.Ticks / Iterations:F0} ticks/op)");
        Console.WriteLine($"  Complex Conversion:  {ComplexConversionTime.TotalMilliseconds:F2} ms ({ComplexConversionTime.Ticks / Iterations:F0} ticks/op)");
        Console.WriteLine($"  Cached Conversion:   {CachedConversionTime.TotalMilliseconds:F2} ms ({CachedConversionTime.Ticks / Iterations:F0} ticks/op)");
    }
}
```

## Best Practices

### 1. **Error Handling and Validation**

```csharp
public static class ConversionBestPractices
{
    /// <summary>
    /// Always validate preconditions before conversion
    /// </summary>
    public static void ValidateBeforeConversion()
    {
        // Example of proper validation
        var order = new Order { Status = OrderStatus.Pending };
        
        try
        {
            // This should throw because order is not ready for invoice
            var invoice = ((IConvertible<OrderInvoice>)order).Convert();
        }
        catch (InconvertibleException ex)
        {
            Console.WriteLine($"Expected error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Use InconvertibleException.ThrowIfInconvertible for consistency
    /// </summary>
    public static void UseStandardErrorHandling()
    {
        // Good: Use helper method
        InconvertibleException.ThrowIfInconvertible(
            condition: false,
            message: "Conversion is not possible"
        );
        
        // Also good: Use lambda for expensive conditions
        InconvertibleException.ThrowIfInconvertible(
            condition: () => ExpensiveValidation(),
            message: "Expensive validation failed"
        );
    }
    
    private static bool ExpensiveValidation() => true; // Placeholder
    
    /// <summary>
    /// Provide meaningful error messages
    /// </summary>
    public static void ProvideMeaningfulErrors()
    {
        // Good: Specific error message
        InconvertibleException.ThrowIfInconvertible(
            false,
            "Cannot convert Order to Invoice: Payment must be completed and order must be shipped"
        );
        
        // Bad: Generic error message
        // throw new InconvertibleException("Conversion failed");
    }
}
```

### 2. **Performance Guidelines**

```csharp
public static class PerformanceGuidelines
{
    /// <summary>
    /// Cache expensive conversion results when appropriate
    /// </summary>
    public class CachingConverter : IConvertible<string>
    {
        private readonly Lazy<string> _cachedResult;
        
        public CachingConverter(Func<string> expensiveConversion)
        {
            _cachedResult = new Lazy<string>(expensiveConversion);
        }
        
        public string Convert() => _cachedResult.Value;
    }
    
    /// <summary>
    /// Avoid creating unnecessary objects during conversion
    /// </summary>
    public class EfficientConverter : IConvertible<ReadOnlySpan<char>>
    {
        private readonly string _data;
        
        public EfficientConverter(string data) => _data = data;
        
        // Return span to avoid string allocation
        public ReadOnlySpan<char> Convert() => _data.AsSpan();
    }
    
    /// <summary>
    /// Use object pooling for frequently converted objects
    /// </summary>
    public class PooledConverter : IConvertible<StringBuilder>
    {
        private static readonly ObjectPool<StringBuilder> Pool = 
            new StringBuilderPool();
        
        private readonly string _content;
        
        public PooledConverter(string content) => _content = content;
        
        public StringBuilder Convert()
        {
            var sb = Pool.Get();
            sb.Clear();
            sb.Append(_content);
            // Note: Caller is responsible for returning to pool
            return sb;
        }
    }
    
    private class StringBuilderPool : ObjectPool<StringBuilder>
    {
        public override StringBuilder Get() => new StringBuilder();
        public override void Return(StringBuilder obj) => obj.Clear();
    }
    
    private abstract class ObjectPool<T>
    {
        public abstract T Get();
        public abstract void Return(T obj);
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class IConvertibleTests
{
    [Test]
    public void Convert_Temperature_ToDouble_ReturnsCorrectValue()
    {
        // Arrange
        var temperature = new Temperature(25.0, TemperatureScale.Celsius);
        
        // Act
        var celsiusValue = ((IConvertible<double>)temperature).Convert();
        
        // Assert
        Assert.That(celsiusValue, Is.EqualTo(25.0));
    }
    
    [Test]
    public void Convert_Temperature_ToString_ReturnsFormattedString()
    {
        // Arrange
        var temperature = new Temperature(32.0, TemperatureScale.Fahrenheit);
        
        // Act
        var stringValue = ((IConvertible<string>)temperature).Convert();
        
        // Assert
        Assert.That(stringValue, Is.EqualTo("32.0°F"));
    }
    
    [Test]
    public void Convert_UserEntity_ToUserDto_MapsAllProperties()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CreatedAt = new DateTime(2023, 1, 1),
            Status = UserStatus.Active
        };
        
        // Act
        var dto = ((IConvertible<UserDto>)user).Convert();
        
        // Assert
        Assert.That(dto.Id, Is.EqualTo(1));
        Assert.That(dto.FirstName, Is.EqualTo("John"));
        Assert.That(dto.LastName, Is.EqualTo("Doe"));
        Assert.That(dto.FullName, Is.EqualTo("John Doe"));
        Assert.That(dto.Email, Is.EqualTo("john@example.com"));
        Assert.That(dto.Status, Is.EqualTo("Active"));
    }
    
    [Test]
    public void Convert_InvalidOrder_ToInvoice_ThrowsInconvertibleException()
    {
        // Arrange
        var order = new Order
        {
            OrderId = "ORDER-001",
            Status = OrderStatus.Pending // Invalid status for invoice
        };
        
        // Act & Assert
        Assert.Throws<InconvertibleException>(() =>
        {
            ((IConvertible<OrderInvoice>)order).Convert();
        });
    }
    
    [Test]
    public void ConversionExtensions_TryConvertTo_WithValidConversion_ReturnsTrue()
    {
        // Arrange
        var user = new UserEntity { FirstName = "Test", LastName = "User" };
        
        // Act
        var success = user.TryConvertTo<UserDto>(out var result);
        
        // Assert
        Assert.That(success, Is.True);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FullName, Is.EqualTo("Test User"));
    }
    
    [Test]
    public void ConversionExtensions_TryConvertTo_WithInvalidConversion_ReturnsFalse()
    {
        // Arrange
        var temperature = new Temperature(25.0);
        
        // Act
        var success = temperature.TryConvertTo<UserDto>(out var result);
        
        // Assert
        Assert.That(success, Is.False);
        Assert.That(result, Is.Null);
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class ConversionIntegrationTests
{
    [Test]
    public void ConversionPipeline_ComplexFlow_ProcessesCorrectly()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Status = UserStatus.Active
        };
        
        // Act
        var result = user
            .ToPipeline()
            .Convert<UserDto>()
            .Convert<UserSummaryDto>()
            .Result;
        
        // Assert
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.FullName, Is.EqualTo("Alice Smith"));
        Assert.That(result.IsActive, Is.True);
    }
    
    [Test]
    public void BatchConversion_MixedObjects_ConvertsOnlyCompatible()
    {
        // Arrange
        var objects = new List<object>
        {
            new UserEntity { FirstName = "User1", LastName = "Test" },
            new Temperature(25.0),
            "string object",
            new UserEntity { FirstName = "User2", LastName = "Test" }
        };
        
        // Act
        var userDtos = objects.ConvertAll<UserDto>();
        var strings = objects.ConvertAll<string>();
        
        // Assert
        Assert.That(userDtos.Count, Is.EqualTo(2));
        Assert.That(strings.Count, Is.EqualTo(1)); // Only Temperature converts to string
    }
}
```

## See Also

- [System.IConvertible](https://learn.microsoft.com/en-us/dotnet/api/system.iconvertible) - Built-in .NET conversion interface
- [InconvertibleException](InconvertibleException.md) - Custom exception for conversion failures
- [ICloneable<T>](ICloneable.md) - Type-safe cloning interface
- [FeederMessage](FeederMessage.md) - Dictionary-based message that supports conversion patterns
- [Type Conversion](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/casting-and-type-conversions) - .NET type conversion overview

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing type-safe object conversion capabilities with covariant support and comprehensive error handling.*