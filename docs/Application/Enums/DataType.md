# DataType

The `DataType` enum provides a comprehensive classification system for data types used throughout the RapidStreamer BuildingBlocks framework. It standardizes data type identification for serialization, validation, formatting, and type conversion operations across different platforms and languages.

## Overview

The `DataType` enum serves as a universal data type classification that bridges differences between various programming languages, databases, and serialization formats. It includes rich metadata through XML documentation comments that provide examples and cross-platform mappings.

## Enum Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.Enums
{
    public enum DataType
    {
        String = 1,

        /// <summary>
        /// C# = Long
        /// JS = Number
        /// Ex: 10000000000000000
        /// </summary>
        Number,

        /// <summary>
        /// Ex: 100000000000.01
        /// </summary>
        Decimal,

        /// <summary>
        /// Ex: 12%
        /// </summary>
        Percent,

        /// <summary>
        /// Ex: 100,000,000
        /// </summary>
        Currency,

        /// <summary>
        /// Ex: 2024-01-01T12:00:00.000Z
        /// </summary>
        DateTime,

        /// <summary>
        /// Ex: 2024-01-01
        /// </summary>
        Date,

        /// <summary>
        /// Ex: 12:00:00.000
        /// </summary>
        Time,

        /// <summary>
        /// True/False
        /// </summary>
        Boolean,
        Enum,
        Json
    }
}
```

## Values

### String
- **Value**: `1`
- **Description**: Text data of variable length
- **Use Case**: Names, descriptions, messages, identifiers
- **Examples**: `"Hello World"`, `"Customer Name"`, `"SKU-12345"`
- **Platform Mapping**: C# `string`, JS `string`, SQL `VARCHAR/TEXT`

### Number
- **Value**: `2`
- **Description**: Large integer values, equivalent to `long` in C# and `Number` in JavaScript
- **Use Case**: IDs, counters, large numeric values, timestamps
- **Examples**: `10000000000000000`, `1234567890`, `-9223372036854775808`
- **Platform Mapping**: C# `long`, JS `Number`, SQL `BIGINT`
- **Range**: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807

### Decimal
- **Value**: `3`
- **Description**: High-precision decimal values for exact calculations
- **Use Case**: Monetary calculations, scientific measurements, precise calculations
- **Examples**: `100000000000.01`, `3.14159265359`, `0.000000001`
- **Platform Mapping**: C# `decimal`, JS `BigDecimal` (with library), SQL `DECIMAL/NUMERIC`
- **Precision**: High precision for financial calculations

### Percent
- **Value**: `4`
- **Description**: Percentage values with specific formatting
- **Use Case**: Rates, ratios, completion percentages, tax rates
- **Examples**: `12%`, `99.95%`, `0.5%`, `150%`
- **Platform Mapping**: Stored as decimal, formatted with % symbol
- **Range**: Typically 0-100% but can exceed for ratios

### Currency
- **Value**: `5`
- **Description**: Monetary values with currency-specific formatting
- **Use Case**: Prices, costs, financial amounts, account balances
- **Examples**: `100,000,000`, `$1,234.56`, `€999.99`, `¥500,000`
- **Platform Mapping**: C# `decimal` with currency formatting, locale-aware display
- **Formatting**: Includes thousand separators and currency symbols

### DateTime
- **Value**: `6`
- **Description**: Combined date and time with timezone information
- **Use Case**: Timestamps, event times, created/modified dates
- **Examples**: `2024-01-01T12:00:00.000Z`, `2023-12-25T18:30:45.123+05:00`
- **Platform Mapping**: C# `DateTime`/`DateTimeOffset`, JS `Date`, SQL `DATETIME/TIMESTAMP`
- **Format**: ISO 8601 standard with optional timezone

### Date
- **Value**: `7`
- **Description**: Date-only values without time component
- **Use Case**: Birth dates, due dates, event dates, schedules
- **Examples**: `2024-01-01`, `2023-12-25`, `1990-05-15`
- **Platform Mapping**: C# `DateOnly`, JS `Date` (time set to midnight), SQL `DATE`
- **Format**: ISO 8601 date format (YYYY-MM-DD)

### Time
- **Value**: `8`
- **Description**: Time-only values without date component
- **Use Case**: Business hours, durations, scheduled times
- **Examples**: `12:00:00.000`, `09:30:45`, `23:59:59.999`
- **Platform Mapping**: C# `TimeOnly`/`TimeSpan`, JS time handling libraries, SQL `TIME`
- **Format**: HH:mm:ss.fff format

### Boolean
- **Value**: `9`
- **Description**: True/false values for binary states
- **Use Case**: Flags, switches, yes/no questions, active/inactive states
- **Examples**: `true`, `false`
- **Platform Mapping**: C# `bool`, JS `boolean`, SQL `BIT/BOOLEAN`
- **Values**: Only `true` or `false`

### Enum
- **Value**: `10`
- **Description**: Enumerated values from a predefined set
- **Use Case**: Status codes, categories, types, classifications
- **Examples**: `Active`, `Pending`, `Cancelled`, `High`, `Medium`, `Low`
- **Platform Mapping**: C# `enum`, JS string constants/enums, SQL `ENUM` or lookup tables
- **Validation**: Must be one of the predefined values

### Json
- **Value**: `11`
- **Description**: Complex structured data in JSON format
- **Use Case**: Configuration objects, nested data, API payloads, metadata
- **Examples**: `{"name": "John", "age": 30}`, `[1, 2, 3]`, `{"config": {"enabled": true}}`
- **Platform Mapping**: C# `object`/`JObject`, JS `Object`, SQL `JSON/TEXT`
- **Validation**: Must be valid JSON format

## Usage Examples

### Type Detection and Conversion

```csharp
using RapidStreamer.BuildingBlocks.Application.Enums;
using System.Text.Json;

public class DataTypeConverter
{
    public object? ConvertValue(string input, DataType dataType)
    {
        try
        {
            return dataType switch
            {
                DataType.String => input,
                
                DataType.Number => long.Parse(input),
                
                DataType.Decimal => decimal.Parse(input),
                
                DataType.Percent => ParsePercent(input),
                
                DataType.Currency => ParseCurrency(input),
                
                DataType.DateTime => DateTime.Parse(input),
                
                DataType.Date => DateOnly.Parse(input),
                
                DataType.Time => TimeOnly.Parse(input),
                
                DataType.Boolean => ParseBoolean(input),
                
                DataType.Enum => input, // Return as string, specific enum parsing handled elsewhere
                
                DataType.Json => JsonSerializer.Deserialize<object>(input),
                
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, 
                    "Unknown data type")
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to convert '{input}' to {dataType}: {ex.Message}", ex);
        }
    }
    
    private decimal ParsePercent(string input)
    {
        // Handle both "12%" and "12" formats
        var value = input.TrimEnd('%');
        return decimal.Parse(value);
    }
    
    private decimal ParseCurrency(string input)
    {
        // Remove currency symbols and thousand separators
        var cleaned = input.Replace("$", "")
                          .Replace("€", "")
                          .Replace("¥", "")
                          .Replace(",", "");
        return decimal.Parse(cleaned);
    }
    
    private bool ParseBoolean(string input)
    {
        return input.ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "on" or "enabled" => true,
            "false" or "0" or "no" or "off" or "disabled" => false,
            _ => throw new FormatException($"Cannot parse '{input}' as boolean")
        };
    }
}
```

### Data Validation System

```csharp
public class DataTypeValidator
{
    public ValidationResult ValidateValue(object? value, DataType expectedType)
    {
        if (value == null)
        {
            return ValidationResult.Success("Null value accepted");
        }
        
        var stringValue = value.ToString()!;
        
        return expectedType switch
        {
            DataType.String => ValidateString(stringValue),
            DataType.Number => ValidateNumber(stringValue),
            DataType.Decimal => ValidateDecimal(stringValue),
            DataType.Percent => ValidatePercent(stringValue),
            DataType.Currency => ValidateCurrency(stringValue),
            DataType.DateTime => ValidateDateTime(stringValue),
            DataType.Date => ValidateDate(stringValue),
            DataType.Time => ValidateTime(stringValue),
            DataType.Boolean => ValidateBoolean(stringValue),
            DataType.Enum => ValidateEnum(stringValue, value),
            DataType.Json => ValidateJson(stringValue),
            _ => ValidationResult.Failure($"Unknown data type: {expectedType}")
        };
    }
    
    private ValidationResult ValidateString(string value)
    {
        // Basic string validation
        if (value.Length > 10000)
        {
            return ValidationResult.Failure("String too long (max 10,000 characters)");
        }
        
        return ValidationResult.Success("Valid string");
    }
    
    private ValidationResult ValidateNumber(string value)
    {
        if (long.TryParse(value, out var number))
        {
            return ValidationResult.Success($"Valid number: {number}");
        }
        
        return ValidationResult.Failure($"Invalid number format: {value}");
    }
    
    private ValidationResult ValidateDecimal(string value)
    {
        if (decimal.TryParse(value, out var decimalValue))
        {
            return ValidationResult.Success($"Valid decimal: {decimalValue}");
        }
        
        return ValidationResult.Failure($"Invalid decimal format: {value}");
    }
    
    private ValidationResult ValidatePercent(string value)
    {
        // Allow both "12%" and "12" formats
        var cleaned = value.TrimEnd('%');
        
        if (decimal.TryParse(cleaned, out var percentValue))
        {
            if (percentValue < 0 || percentValue > 100)
            {
                return ValidationResult.Warning($"Percentage outside normal range: {percentValue}%");
            }
            
            return ValidationResult.Success($"Valid percentage: {percentValue}%");
        }
        
        return ValidationResult.Failure($"Invalid percentage format: {value}");
    }
    
    private ValidationResult ValidateCurrency(string value)
    {
        // Remove common currency symbols for validation
        var cleaned = value.Replace("$", "")
                          .Replace("€", "")
                          .Replace("¥", "")
                          .Replace(",", "");
        
        if (decimal.TryParse(cleaned, out var currencyValue))
        {
            if (currencyValue < 0)
            {
                return ValidationResult.Warning($"Negative currency value: {currencyValue}");
            }
            
            return ValidationResult.Success($"Valid currency: {currencyValue}");
        }
        
        return ValidationResult.Failure($"Invalid currency format: {value}");
    }
    
    private ValidationResult ValidateDateTime(string value)
    {
        if (DateTime.TryParse(value, out var dateTime))
        {
            return ValidationResult.Success($"Valid date/time: {dateTime:yyyy-MM-dd HH:mm:ss}");
        }
        
        return ValidationResult.Failure($"Invalid date/time format: {value}");
    }
    
    private ValidationResult ValidateDate(string value)
    {
        if (DateOnly.TryParse(value, out var date))
        {
            return ValidationResult.Success($"Valid date: {date:yyyy-MM-dd}");
        }
        
        return ValidationResult.Failure($"Invalid date format: {value}");
    }
    
    private ValidationResult ValidateTime(string value)
    {
        if (TimeOnly.TryParse(value, out var time))
        {
            return ValidationResult.Success($"Valid time: {time:HH:mm:ss}");
        }
        
        return ValidationResult.Failure($"Invalid time format: {value}");
    }
    
    private ValidationResult ValidateBoolean(string value)
    {
        var normalized = value.ToLowerInvariant();
        var validBooleans = new[] { "true", "false", "1", "0", "yes", "no", "on", "off" };
        
        if (validBooleans.Contains(normalized))
        {
            return ValidationResult.Success($"Valid boolean: {normalized}");
        }
        
        return ValidationResult.Failure($"Invalid boolean format: {value}");
    }
    
    private ValidationResult ValidateEnum(string value, object originalValue)
    {
        // Basic enum validation - could be extended with specific enum types
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("Enum value cannot be empty");
        }
        
        return ValidationResult.Success($"Valid enum value: {value}");
    }
    
    private ValidationResult ValidateJson(string value)
    {
        try
        {
            JsonDocument.Parse(value);
            return ValidationResult.Success("Valid JSON");
        }
        catch (JsonException ex)
        {
            return ValidationResult.Failure($"Invalid JSON: {ex.Message}");
        }
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public bool IsWarning { get; private set; }
    public string Message { get; private set; }
    
    private ValidationResult(bool isValid, bool isWarning, string message)
    {
        IsValid = isValid;
        IsWarning = isWarning;
        Message = message;
    }
    
    public static ValidationResult Success(string message) => 
        new(true, false, message);
    
    public static ValidationResult Warning(string message) => 
        new(true, true, message);
    
    public static ValidationResult Failure(string message) => 
        new(false, false, message);
}
```

### Formatting System

```csharp
public class DataTypeFormatter
{
    private readonly CultureInfo _culture;
    
    public DataTypeFormatter(CultureInfo? culture = null)
    {
        _culture = culture ?? CultureInfo.CurrentCulture;
    }
    
    public string FormatValue(object? value, DataType dataType, string? format = null)
    {
        if (value == null)
        {
            return string.Empty;
        }
        
        return dataType switch
        {
            DataType.String => FormatString(value),
            DataType.Number => FormatNumber(value, format),
            DataType.Decimal => FormatDecimal(value, format),
            DataType.Percent => FormatPercent(value, format),
            DataType.Currency => FormatCurrency(value, format),
            DataType.DateTime => FormatDateTime(value, format),
            DataType.Date => FormatDate(value, format),
            DataType.Time => FormatTime(value, format),
            DataType.Boolean => FormatBoolean(value, format),
            DataType.Enum => FormatEnum(value, format),
            DataType.Json => FormatJson(value, format),
            _ => value.ToString() ?? string.Empty
        };
    }
    
    private string FormatString(object value)
    {
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatNumber(object value, string? format)
    {
        if (value is long number)
        {
            return number.ToString(format ?? "N0", _culture);
        }
        
        if (long.TryParse(value.ToString(), out var parsed))
        {
            return parsed.ToString(format ?? "N0", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatDecimal(object value, string? format)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString(format ?? "F2", _culture);
        }
        
        if (decimal.TryParse(value.ToString(), out var parsed))
        {
            return parsed.ToString(format ?? "F2", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatPercent(object value, string? format)
    {
        if (decimal.TryParse(value.ToString()?.TrimEnd('%'), out var percent))
        {
            // Convert to decimal representation (e.g., 12% becomes 0.12)
            var decimalPercent = percent / 100m;
            return decimalPercent.ToString(format ?? "P2", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatCurrency(object value, string? format)
    {
        if (decimal.TryParse(value.ToString()?.Replace(",", ""), out var currency))
        {
            return currency.ToString(format ?? "C", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatDateTime(object value, string? format)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(format ?? "yyyy-MM-dd HH:mm:ss", _culture);
        }
        
        if (DateTime.TryParse(value.ToString(), out var parsed))
        {
            return parsed.ToString(format ?? "yyyy-MM-dd HH:mm:ss", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatDate(object value, string? format)
    {
        if (value is DateOnly date)
        {
            return date.ToString(format ?? "yyyy-MM-dd", _culture);
        }
        
        if (DateOnly.TryParse(value.ToString(), out var parsed))
        {
            return parsed.ToString(format ?? "yyyy-MM-dd", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatTime(object value, string? format)
    {
        if (value is TimeOnly time)
        {
            return time.ToString(format ?? "HH:mm:ss", _culture);
        }
        
        if (TimeOnly.TryParse(value.ToString(), out var parsed))
        {
            return parsed.ToString(format ?? "HH:mm:ss", _culture);
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatBoolean(object value, string? format)
    {
        if (value is bool boolValue)
        {
            return format?.ToLowerInvariant() switch
            {
                "yesno" => boolValue ? "Yes" : "No",
                "onoff" => boolValue ? "On" : "Off",
                "numeric" => boolValue ? "1" : "0",
                _ => boolValue ? "True" : "False"
            };
        }
        
        return value.ToString() ?? string.Empty;
    }
    
    private string FormatEnum(object value, string? format)
    {
        var enumString = value.ToString() ?? string.Empty;
        
        return format?.ToLowerInvariant() switch
        {
            "lower" => enumString.ToLowerInvariant(),
            "upper" => enumString.ToUpperInvariant(),
            "title" => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(enumString.ToLowerInvariant()),
            _ => enumString
        };
    }
    
    private string FormatJson(object value, string? format)
    {
        var jsonString = value.ToString() ?? string.Empty;
        
        if (format?.ToLowerInvariant() == "pretty")
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(jsonString);
                return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
            catch
            {
                return jsonString;
            }
        }
        
        return jsonString;
    }
}
```

### Configuration and Schema Definition

```csharp
public class DataTypeSchema
{
    public class FieldDefinition
    {
        public string Name { get; set; } = string.Empty;
        public DataType DataType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsArray { get; set; }
        public object? DefaultValue { get; set; }
        public Dictionary<string, object> Constraints { get; set; } = new();
        public string? Format { get; set; }
        public string? Description { get; set; }
    }
    
    public string SchemaName { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = new();
    
    public ValidationResult ValidateData(Dictionary<string, object?> data)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        foreach (var field in Fields)
        {
            var hasValue = data.TryGetValue(field.Name, out var value);
            
            if (field.IsRequired && (!hasValue || value == null))
            {
                errors.Add($"Required field '{field.Name}' is missing");
                continue;
            }
            
            if (hasValue && value != null)
            {
                var validator = new DataTypeValidator();
                var validationResult = validator.ValidateValue(value, field.DataType);
                
                if (!validationResult.IsValid)
                {
                    errors.Add($"Field '{field.Name}': {validationResult.Message}");
                }
                else if (validationResult.IsWarning)
                {
                    warnings.Add($"Field '{field.Name}': {validationResult.Message}");
                }
            }
        }
        
        if (errors.Any())
        {
            return ValidationResult.Failure($"Validation failed: {string.Join(", ", errors)}");
        }
        
        if (warnings.Any())
        {
            return ValidationResult.Warning($"Validation warnings: {string.Join(", ", warnings)}");
        }
        
        return ValidationResult.Success("All fields valid");
    }
}

// Usage example
var userSchema = new DataTypeSchema
{
    SchemaName = "User",
    Fields = new List<DataTypeSchema.FieldDefinition>
    {
        new()
        {
            Name = "Id",
            DataType = DataType.Number,
            IsRequired = true,
            Description = "Unique user identifier"
        },
        new()
        {
            Name = "Name",
            DataType = DataType.String,
            IsRequired = true,
            Constraints = { ["maxLength"] = 100 }
        },
        new()
        {
            Name = "Email",
            DataType = DataType.String,
            IsRequired = true,
            Format = "email"
        },
        new()
        {
            Name = "BirthDate",
            DataType = DataType.Date,
            IsRequired = false
        },
        new()
        {
            Name = "IsActive",
            DataType = DataType.Boolean,
            IsRequired = false,
            DefaultValue = true
        },
        new()
        {
            Name = "Metadata",
            DataType = DataType.Json,
            IsRequired = false
        }
    }
};
```

### YAML Type Converter Integration

```csharp
// Integration with the existing YAML system found in the codebase
public class DataTypeAwareYamlConverter : BaseYamlTypeConverter
{
    public object? ReadTypedValue(IParser parser, DataType dataType)
    {
        return dataType switch
        {
            DataType.String => ReadValueAndShift(parser),
            DataType.Number => ReadNumberAndShift<long>(parser),
            DataType.Decimal => ReadNumberAndShift<decimal>(parser),
            DataType.Boolean => ReadBooleanAndShift(parser),
            DataType.Enum => ReadValueAndShift(parser), // Return as string
            DataType.DateTime => ReadDateTimeAndShift(parser),
            DataType.Date => ReadDateAndShift(parser),
            DataType.Time => ReadTimeAndShift(parser),
            DataType.Json => ReadJsonAndShift(parser),
            DataType.Percent => ReadPercentAndShift(parser),
            DataType.Currency => ReadCurrencyAndShift(parser),
            _ => ReadValueAndShift(parser)
        };
    }
    
    public void WriteTypedValue(IEmitter emitter, string key, object? value, DataType dataType)
    {
        switch (dataType)
        {
            case DataType.String:
                WriteKeyValue(emitter, key, value?.ToString() ?? "");
                break;
                
            case DataType.Number:
                WriteNumber(emitter, key, Convert.ToInt64(value));
                break;
                
            case DataType.Decimal:
                WriteNumber(emitter, key, Convert.ToDecimal(value));
                break;
                
            case DataType.Boolean:
                WriteBoolean(emitter, key, Convert.ToBoolean(value));
                break;
                
            case DataType.Enum:
                if (value is Enum enumValue)
                    WriteEnum(emitter, key, enumValue);
                else
                    WriteKeyValue(emitter, key, value?.ToString() ?? "");
                break;
                
            default:
                WriteKeyValue(emitter, key, value?.ToString() ?? "");
                break;
        }
    }
    
    private DateTime? ReadDateTimeAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        return DateTime.TryParse(value, out var result) ? result : null;
    }
    
    private DateOnly? ReadDateAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        return DateOnly.TryParse(value, out var result) ? result : null;
    }
    
    private TimeOnly? ReadTimeAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        return TimeOnly.TryParse(value, out var result) ? result : null;
    }
    
    private object? ReadJsonAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        try
        {
            return string.IsNullOrEmpty(value) ? null : JsonSerializer.Deserialize<object>(value);
        }
        catch
        {
            return value; // Return as string if not valid JSON
        }
    }
    
    private decimal? ReadPercentAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        var cleaned = value?.TrimEnd('%');
        return decimal.TryParse(cleaned, out var result) ? result : null;
    }
    
    private decimal? ReadCurrencyAndShift(IParser parser)
    {
        var value = ReadValueAndShift(parser);
        var cleaned = value?.Replace("$", "").Replace("€", "").Replace("¥", "").Replace(",", "");
        return decimal.TryParse(cleaned, out var result) ? result : null;
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[TestClass]
public class DataTypeTests
{
    [TestMethod]
    public void DataType_HasExpectedValues()
    {
        Assert.AreEqual(1, (int)DataType.String);
        Assert.AreEqual(2, (int)DataType.Number);
        Assert.AreEqual(3, (int)DataType.Decimal);
        Assert.AreEqual(4, (int)DataType.Percent);
        Assert.AreEqual(5, (int)DataType.Currency);
        Assert.AreEqual(6, (int)DataType.DateTime);
        Assert.AreEqual(7, (int)DataType.Date);
        Assert.AreEqual(8, (int)DataType.Time);
        Assert.AreEqual(9, (int)DataType.Boolean);
        Assert.AreEqual(10, (int)DataType.Enum);
        Assert.AreEqual(11, (int)DataType.Json);
    }
    
    [TestMethod]
    public void DataTypeConverter_HandlesAllTypes()
    {
        var converter = new DataTypeConverter();
        
        // Test each data type
        Assert.AreEqual("test", converter.ConvertValue("test", DataType.String));
        Assert.AreEqual(123L, converter.ConvertValue("123", DataType.Number));
        Assert.AreEqual(123.45m, converter.ConvertValue("123.45", DataType.Decimal));
        Assert.IsTrue(converter.ConvertValue("true", DataType.Boolean) is bool);
    }
    
    [TestMethod]
    public void DataTypeValidator_ValidatesCorrectly()
    {
        var validator = new DataTypeValidator();
        
        // Valid cases
        Assert.IsTrue(validator.ValidateValue("test", DataType.String).IsValid);
        Assert.IsTrue(validator.ValidateValue("123", DataType.Number).IsValid);
        Assert.IsTrue(validator.ValidateValue("true", DataType.Boolean).IsValid);
        
        // Invalid cases
        Assert.IsFalse(validator.ValidateValue("not-a-number", DataType.Number).IsValid);
        Assert.IsFalse(validator.ValidateValue("invalid-json", DataType.Json).IsValid);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class DataTypeIntegrationTests
{
    [TestMethod]
    public void SchemaValidation_WorksEndToEnd()
    {
        var schema = new DataTypeSchema
        {
            Fields = new List<DataTypeSchema.FieldDefinition>
            {
                new() { Name = "id", DataType = DataType.Number, IsRequired = true },
                new() { Name = "name", DataType = DataType.String, IsRequired = true },
                new() { Name = "active", DataType = DataType.Boolean, IsRequired = false }
            }
        };
        
        var validData = new Dictionary<string, object?>
        {
            ["id"] = 123L,
            ["name"] = "Test User",
            ["active"] = true
        };
        
        var result = schema.ValidateData(validData);
        Assert.IsTrue(result.IsValid);
    }
    
    [TestMethod]
    public void FormatterProducesExpectedOutput()
    {
        var formatter = new DataTypeFormatter(CultureInfo.InvariantCulture);
        
        Assert.AreEqual("123", formatter.FormatValue(123L, DataType.Number));
        Assert.AreEqual("123.45", formatter.FormatValue(123.45m, DataType.Decimal, "F2"));
        Assert.AreEqual("True", formatter.FormatValue(true, DataType.Boolean));
    }
}
```

## Performance Considerations

### Type Detection Optimization

```csharp
public class OptimizedDataTypeDetector
{
    private static readonly Dictionary<Type, DataType> TypeMappings = new()
    {
        [typeof(string)] = DataType.String,
        [typeof(long)] = DataType.Number,
        [typeof(decimal)] = DataType.Decimal,
        [typeof(bool)] = DataType.Boolean,
        [typeof(DateTime)] = DataType.DateTime,
        [typeof(DateOnly)] = DataType.Date,
        [typeof(TimeOnly)] = DataType.Time
    };
    
    public DataType DetectDataType(object value)
    {
        if (value == null)
            return DataType.String; // Default for null values
        
        var type = value.GetType();
        
        // Fast lookup for common types
        if (TypeMappings.TryGetValue(type, out var dataType))
            return dataType;
        
        // Handle special cases
        if (type.IsEnum)
            return DataType.Enum;
        
        // Check if it's a JSON string
        if (value is string stringValue && IsJsonString(stringValue))
            return DataType.Json;
        
        // Default to string for unknown types
        return DataType.String;
    }
    
    private bool IsJsonString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        value = value.Trim();
        return (value.StartsWith("{") && value.EndsWith("}")) ||
               (value.StartsWith("[") && value.EndsWith("]"));
    }
}
```

## Best Practices

1. **Type Safety**: Always validate data types before conversion operations
2. **Cultural Awareness**: Use appropriate culture settings for formatting and parsing
3. **Error Handling**: Implement robust error handling for type conversion failures
4. **Performance**: Cache type mappings and use optimized detection for high-volume scenarios
5. **Extensibility**: Design systems to easily add new data types
6. **Documentation**: Clearly document expected formats and examples for each data type
7. **Validation**: Always validate data against expected types in schemas and APIs

## Related Components

- **YAML Type Converters**: Integration with the existing YAML serialization system
- **Validation Systems**: For ensuring data integrity and format compliance
- **Serialization**: For converting between different data representations
- **Configuration Management**: For schema definitions and type mappings

## See Also

- [Enums System Overview](README.md)
- [YAML Type Converters](../Serializations/YamlTypeConverter.md)
- [Data Validation Patterns](../Patterns/DataValidation.md)