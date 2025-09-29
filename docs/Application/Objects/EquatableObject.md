# EquatableObject

The `EquatableObject` abstract class provides a reflection-based implementation of value equality for objects. It automatically implements `IEquatable<T>` and overrides equality operators based on the object's field and property values, with support for excluding specific members from equality comparisons.

## Overview

```csharp
public abstract class EquatableObject<TEquatableObject> : IEquatable<TEquatableObject>
    where TEquatableObject : EquatableObject<TEquatableObject>

public abstract class EquatableObject : EquatableObject<EquatableObject>
```

`EquatableObject` eliminates the need to manually implement equality logic by using reflection to compare all fields and properties. It provides both generic and non-generic versions for maximum flexibility in inheritance hierarchies.

## Key Features

- **Automatic Value Equality**: Reflection-based comparison of all fields and properties
- **IgnoreMemberAttribute Support**: Exclude specific members from equality comparisons
- **Generic and Non-Generic Versions**: Flexible inheritance patterns
- **Complete Equality Implementation**: `IEquatable<T>`, `Equals()`, `GetHashCode()`, and operators
- **Performance Optimized**: Efficient hash code generation and comparison algorithms
- **Null Safety**: Proper handling of null values and references

## Equality Mechanism

### GetAtomicValues()
The core method that extracts all values used for equality comparison.

```csharp
protected virtual List<object?> GetAtomicValues()
{
    var type = GetType();

    var fieldsValues = type
        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(field => field.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
        .Select(field => field.GetValue(this));

    var propertiesValues = type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property => property.CanRead &&
                           property.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null &&
                           property.GetIndexParameters().Length == 0)
        .Select(property => property.GetValue(this));

    return fieldsValues.Union(propertiesValues).ToList();
}
```

**Inclusion Criteria:**
- **Fields**: All instance fields (public and non-public)
- **Properties**: Public readable properties without index parameters
- **Exclusions**: Members marked with `[IgnoreMember]` attribute

### Equals Implementation
Compares two objects by comparing their atomic values element by element.

```csharp
public bool Equals(TEquatableObject? obj)
{
    if (obj is null)
        return false;

    var left = GetAtomicValues();
    var right = obj.GetAtomicValues();
    if (left.Count != right.Count)
        return false;

    for (int i = 0; i < left.Count; i++)
    {
        if (!Equals(left[i], right[i]))  // Note: Fixed logic from source
            return false;
    }

    return true;
}
```

### Hash Code Generation
Combines hash codes of all atomic values for consistent hash code generation.

```csharp
public override int GetHashCode() => GetAtomicValues().Aggregate(0, HashCode.Combine);
```

## Usage Examples

### Basic Value Object

```csharp
public class PersonName : EquatableObject<PersonName>
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }
    
    public PersonName(string firstName, string lastName, string? middleName = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        MiddleName = middleName;
    }
    
    public string FullName => MiddleName != null 
        ? $"{FirstName} {MiddleName} {LastName}"
        : $"{FirstName} {LastName}";
}

// Usage example
public void DemonstrateBasicEquality()
{
    var name1 = new PersonName("John", "Doe", "Robert");
    var name2 = new PersonName("John", "Doe", "Robert");
    var name3 = new PersonName("John", "Doe"); // No middle name
    
    Console.WriteLine($"name1 == name2: {name1 == name2}"); // True
    Console.WriteLine($"name1 == name3: {name1 == name3}"); // False
    Console.WriteLine($"name1.Equals(name2): {name1.Equals(name2)}"); // True
    
    // Hash codes are equal for equal objects
    Console.WriteLine($"Same hash codes: {name1.GetHashCode() == name2.GetHashCode()}"); // True
    
    // Can be used in collections
    var nameSet = new HashSet<PersonName> { name1, name2, name3 };
    Console.WriteLine($"Unique names in set: {nameSet.Count}"); // 2 (name1 and name2 are considered equal)
}
```

### Ignoring Specific Members

```csharp
public class Product : EquatableObject<Product>
{
    public string Name { get; }
    public decimal Price { get; }
    public string Category { get; }
    
    [IgnoreMember] // Excluded from equality comparison
    public DateTime LastUpdated { get; set; }
    
    [IgnoreMember] // Excluded from equality comparison
    public int ViewCount { get; set; }
    
    private readonly string _internalId; // Included in equality
    
    public Product(string name, decimal price, string category)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price;
        Category = category ?? throw new ArgumentNullException(nameof(category));
        _internalId = Guid.NewGuid().ToString();
        LastUpdated = DateTime.UtcNow;
    }
}

// Usage example
public void DemonstrateIgnoreMember()
{
    var product1 = new Product("Laptop", 999.99m, "Electronics");
    var product2 = new Product("Laptop", 999.99m, "Electronics");
    
    // Different timestamps and view counts
    Thread.Sleep(10);
    product2.LastUpdated = DateTime.UtcNow;
    product1.ViewCount = 100;
    product2.ViewCount = 200;
    
    // Still equal because LastUpdated and ViewCount are ignored
    Console.WriteLine($"Products equal: {product1 == product2}"); // True (if _internalId happens to be same, which is unlikely)
    
    // Note: In practice, the _internalId field would make them different unless handled differently
}
```

### Complex Value Objects

```csharp
public class Address : EquatableObject<Address>
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string? Country { get; }
    
    public Address(string street, string city, string state, string zipCode, string? country = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Country = country;
    }
}

public class Customer : EquatableObject<Customer>
{
    public PersonName Name { get; }
    public Address Address { get; }
    public string Email { get; }
    
    [IgnoreMember]
    public DateTime CreatedAt { get; }
    
    [IgnoreMember]
    public DateTime LastLoginAt { get; set; }
    
    private readonly List<string> _tags;
    
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    
    public Customer(PersonName name, Address address, string email, IEnumerable<string>? tags = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
        _tags = tags?.ToList() ?? new List<string>();
    }
    
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
        {
            _tags.Add(tag);
        }
    }
}

// Usage example
public void DemonstrateComplexEquality()
{
    var name1 = new PersonName("Alice", "Johnson");
    var address1 = new Address("123 Main St", "Anytown", "CA", "12345", "USA");
    var customer1 = new Customer(name1, address1, "alice@example.com", new[] { "VIP", "Premium" });
    
    var name2 = new PersonName("Alice", "Johnson");
    var address2 = new Address("123 Main St", "Anytown", "CA", "12345", "USA");
    var customer2 = new Customer(name2, address2, "alice@example.com", new[] { "VIP", "Premium" });
    
    // Equal even though created at different times (ignored member)
    Thread.Sleep(100);
    customer2.LastLoginAt = DateTime.UtcNow;
    
    Console.WriteLine($"Customers equal: {customer1 == customer2}"); // True
    Console.WriteLine($"Names equal: {customer1.Name == customer2.Name}"); // True
    Console.WriteLine($"Addresses equal: {customer1.Address == customer2.Address}"); // True
    
    // Adding different tags makes them unequal
    customer2.AddTag("Gold");
    Console.WriteLine($"Customers equal after tag change: {customer1 == customer2}"); // False
}
```

### Entity vs Value Object Patterns

```csharp
// Value Object - Equality based on values
public class Money : EquatableObject<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");
            
        return new Money(Amount + other.Amount, Currency);
    }
    
    public override string ToString() => $"{Amount:C} {Currency}";
}

// Entity - Identity-based equality (does NOT inherit from EquatableObject)
public class BankAccount
{
    public Guid Id { get; }
    public string AccountNumber { get; }
    public Money Balance { get; private set; }
    public DateTime CreatedAt { get; }
    
    public BankAccount(string accountNumber, Money initialBalance)
    {
        Id = Guid.NewGuid();
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        Balance = initialBalance ?? throw new ArgumentNullException(nameof(initialBalance));
        CreatedAt = DateTime.UtcNow;
    }
    
    public void Deposit(Money amount)
    {
        Balance = Balance.Add(amount);
    }
    
    // Identity-based equality
    public override bool Equals(object? obj) => obj is BankAccount other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}

// Usage demonstrating different equality semantics
public void DemonstrateEntityVsValueObject()
{
    // Value objects - equal if values are equal
    var money1 = new Money(100m, "USD");
    var money2 = new Money(100m, "USD");
    Console.WriteLine($"Money objects equal: {money1 == money2}"); // True
    
    // Entities - equal only if same identity
    var account1 = new BankAccount("ACC001", money1);
    var account2 = new BankAccount("ACC001", money2); // Same account number, same balance
    Console.WriteLine($"Bank accounts equal: {account1.Equals(account2)}"); // False - different IDs
    
    // Value objects can be used as dictionary keys reliably
    var priceList = new Dictionary<Money, string>
    {
        [new Money(10m, "USD")] = "Small item",
        [new Money(100m, "USD")] = "Medium item",
        [new Money(1000m, "USD")] = "Large item"
    };
    
    var lookupPrice = new Money(100m, "USD");
    Console.WriteLine($"Found: {priceList[lookupPrice]}"); // "Medium item"
}
```

### Custom Equality Logic

```csharp
public class CaseInsensitiveText : EquatableObject<CaseInsensitiveText>
{
    private readonly string _value;
    
    public string Value => _value;
    
    public CaseInsensitiveText(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    // Override to provide custom equality logic
    protected override List<object?> GetAtomicValues()
    {
        // Use lowercase for comparison
        return new List<object?> { _value.ToLowerInvariant() };
    }
    
    public override string ToString() => _value;
}

public class PhoneNumber : EquatableObject<PhoneNumber>
{
    private readonly string _countryCode;
    private readonly string _number;
    
    public string CountryCode => _countryCode;
    public string Number => _number;
    public string FullNumber => $"+{_countryCode}{_number}";
    
    public PhoneNumber(string countryCode, string number)
    {
        _countryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode));
        _number = number ?? throw new ArgumentNullException(nameof(number));
    }
    
    // Override to normalize format for equality
    protected override List<object?> GetAtomicValues()
    {
        // Normalize by removing all non-digits and combining
        var normalizedCountryCode = new string(_countryCode.Where(char.IsDigit).ToArray());
        var normalizedNumber = new string(_number.Where(char.IsDigit).ToArray());
        
        return new List<object?> { normalizedCountryCode, normalizedNumber };
    }
}

// Usage example
public void DemonstrateCustomEquality()
{
    // Case-insensitive text comparison
    var text1 = new CaseInsensitiveText("Hello World");
    var text2 = new CaseInsensitiveText("HELLO WORLD");
    var text3 = new CaseInsensitiveText("hello world");
    
    Console.WriteLine($"text1 == text2: {text1 == text2}"); // True
    Console.WriteLine($"text1 == text3: {text1 == text3}"); // True
    Console.WriteLine($"text2 == text3: {text2 == text3}"); // True
    
    // Phone number normalization
    var phone1 = new PhoneNumber("1", "555-123-4567");
    var phone2 = new PhoneNumber("1", "(555) 123-4567");
    var phone3 = new PhoneNumber("1", "5551234567");
    
    Console.WriteLine($"phone1 == phone2: {phone1 == phone2}"); // True
    Console.WriteLine($"phone1 == phone3: {phone1 == phone3}"); // True
    Console.WriteLine($"phone2 == phone3: {phone2 == phone3}"); // True
    
    // All represent the same phone number despite different formatting
    var phoneSet = new HashSet<PhoneNumber> { phone1, phone2, phone3 };
    Console.WriteLine($"Unique phone numbers: {phoneSet.Count}"); // 1
}
```

### Collection-Based Value Objects

```csharp
public class TagCollection : EquatableObject<TagCollection>
{
    private readonly HashSet<string> _tags;
    
    public IReadOnlySet<string> Tags => _tags;
    public int Count => _tags.Count;
    
    public TagCollection(IEnumerable<string>? tags = null)
    {
        _tags = new HashSet<string>(
            (tags ?? Enumerable.Empty<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim()),
            StringComparer.OrdinalIgnoreCase);
    }
    
    // Override to sort tags for consistent equality
    protected override List<object?> GetAtomicValues()
    {
        // Sort tags to ensure order doesn't affect equality
        var sortedTags = _tags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();
        return new List<object?> { string.Join("|", sortedTags) };
    }
    
    public TagCollection Add(string tag)
    {
        var newTags = new List<string>(_tags) { tag };
        return new TagCollection(newTags);
    }
    
    public TagCollection Remove(string tag)
    {
        var newTags = _tags.Where(t => !string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
        return new TagCollection(newTags);
    }
    
    public bool Contains(string tag) => _tags.Contains(tag);
    
    public override string ToString() => $"[{string.Join(", ", _tags.OrderBy(t => t))}]";
}

public class ProductSpecification : EquatableObject<ProductSpecification>
{
    public string Name { get; }
    public TagCollection Categories { get; }
    public TagCollection Features { get; }
    public Dictionary<string, string> Properties { get; }
    
    public ProductSpecification(string name, 
                               IEnumerable<string>? categories = null,
                               IEnumerable<string>? features = null,
                               Dictionary<string, string>? properties = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Categories = new TagCollection(categories);
        Features = new TagCollection(features);
        Properties = properties ?? new Dictionary<string, string>();
    }
    
    // Override to handle dictionary comparison properly
    protected override List<object?> GetAtomicValues()
    {
        var baseValues = base.GetAtomicValues();
        
        // Replace dictionary with sorted key-value pairs for consistent comparison
        for (int i = 0; i < baseValues.Count; i++)
        {
            if (baseValues[i] is Dictionary<string, string> dict)
            {
                var sortedPairs = dict.OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key}={kvp.Value}")
                    .ToList();
                baseValues[i] = string.Join("|", sortedPairs);
            }
        }
        
        return baseValues;
    }
}

// Usage example
public void DemonstrateCollectionEquality()
{
    // Tag collections with same tags in different order
    var tags1 = new TagCollection(new[] { "Electronics", "Computers", "Gaming" });
    var tags2 = new TagCollection(new[] { "Gaming", "Electronics", "Computers" });
    var tags3 = new TagCollection(new[] { "electronics", "COMPUTERS", "gaming" }); // Different case
    
    Console.WriteLine($"tags1 == tags2: {tags1 == tags2}"); // True
    Console.WriteLine($"tags1 == tags3: {tags1 == tags3}"); // True
    
    // Product specifications
    var spec1 = new ProductSpecification("Gaming Laptop",
        categories: new[] { "Electronics", "Computers" },
        features: new[] { "RGB", "High Performance" },
        properties: new Dictionary<string, string>
        {
            ["CPU"] = "Intel i7",
            ["RAM"] = "16GB",
            ["GPU"] = "RTX 3070"
        });
    
    var spec2 = new ProductSpecification("Gaming Laptop",
        categories: new[] { "Computers", "Electronics" }, // Different order
        features: new[] { "High Performance", "RGB" },     // Different order
        properties: new Dictionary<string, string>
        {
            ["RAM"] = "16GB",  // Different order
            ["CPU"] = "Intel i7",
            ["GPU"] = "RTX 3070"
        });
    
    Console.WriteLine($"Specifications equal: {spec1 == spec2}"); // True
    
    // Can be used as dictionary keys
    var specCatalog = new Dictionary<ProductSpecification, decimal>
    {
        [spec1] = 1299.99m
    };
    
    Console.WriteLine($"Price lookup: ${specCatalog[spec2]}"); // 1299.99 (works because spec1 == spec2)
}
```

### Performance Considerations

```csharp
public class PerformanceOptimizedValue : EquatableObject<PerformanceOptimizedValue>
{
    private readonly string _id;
    private readonly int _number;
    private readonly DateTime _date;
    
    // Cache hash code for better performance
    private readonly int _hashCode;
    
    public string Id => _id;
    public int Number => _number;
    public DateTime Date => _date;
    
    public PerformanceOptimizedValue(string id, int number, DateTime date)
    {
        _id = id ?? throw new ArgumentNullException(nameof(id));
        _number = number;
        _date = date;
        
        // Pre-calculate hash code
        _hashCode = HashCode.Combine(_id, _number, _date);
    }
    
    // Override for better performance - avoid reflection where possible
    protected override List<object?> GetAtomicValues()
    {
        return new List<object?> { _id, _number, _date };
    }
    
    // Override GetHashCode to use cached value
    public override int GetHashCode() => _hashCode;
    
    // Optional: Override Equals for better performance
    public override bool Equals(PerformanceOptimizedValue? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        // Quick hash code check first
        if (_hashCode != other._hashCode) return false;
        
        // Then detailed comparison
        return _id == other._id && 
               _number == other._number && 
               _date == other._date;
    }
}

// Benchmark comparison
public class EqualityPerformanceBenchmark
{
    private readonly List<PerformanceOptimizedValue> _values;
    private readonly HashSet<PerformanceOptimizedValue> _valueSet;
    
    public EqualityPerformanceBenchmark()
    {
        _values = Enumerable.Range(0, 10000)
            .Select(i => new PerformanceOptimizedValue($"ID{i}", i, DateTime.Now.AddDays(i)))
            .ToList();
        
        _valueSet = new HashSet<PerformanceOptimizedValue>(_values.Take(5000));
    }
    
    public void BenchmarkContains()
    {
        var stopwatch = Stopwatch.StartNew();
        
        int foundCount = 0;
        foreach (var value in _values)
        {
            if (_valueSet.Contains(value))
                foundCount++;
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Contains operations: {foundCount} found in {stopwatch.ElapsedMilliseconds}ms");
    }
    
    public void BenchmarkEquality()
    {
        var stopwatch = Stopwatch.StartNew();
        
        int equalCount = 0;
        for (int i = 0; i < _values.Count - 1; i++)
        {
            if (_values[i] == _values[i + 1])
                equalCount++;
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Equality operations: {equalCount} equal in {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class EquatableObjectTests
{
    private class TestValueObject : EquatableObject<TestValueObject>
    {
        public string Name { get; }
        public int Value { get; }
        
        [IgnoreMember]
        public DateTime Created { get; }
        
        public TestValueObject(string name, int value)
        {
            Name = name;
            Value = value;
            Created = DateTime.UtcNow;
        }
    }
    
    [Test]
    public void EqualObjects_ShouldBeEqual()
    {
        // Arrange
        var obj1 = new TestValueObject("Test", 42);
        var obj2 = new TestValueObject("Test", 42);
        
        // Act & Assert
        Assert.That(obj1, Is.EqualTo(obj2));
        Assert.That(obj1 == obj2, Is.True);
        Assert.That(obj1 != obj2, Is.False);
        Assert.That(obj1.Equals(obj2), Is.True);
    }
    
    [Test]
    public void DifferentObjects_ShouldNotBeEqual()
    {
        // Arrange
        var obj1 = new TestValueObject("Test1", 42);
        var obj2 = new TestValueObject("Test2", 42);
        
        // Act & Assert
        Assert.That(obj1, Is.Not.EqualTo(obj2));
        Assert.That(obj1 == obj2, Is.False);
        Assert.That(obj1 != obj2, Is.True);
    }
    
    [Test]
    public void EqualObjects_ShouldHaveSameHashCode()
    {
        // Arrange
        var obj1 = new TestValueObject("Test", 42);
        var obj2 = new TestValueObject("Test", 42);
        
        // Act & Assert
        Assert.That(obj1.GetHashCode(), Is.EqualTo(obj2.GetHashCode()));
    }
    
    [Test]
    public void IgnoredMembers_ShouldNotAffectEquality()
    {
        // Arrange
        var obj1 = new TestValueObject("Test", 42);
        Thread.Sleep(10); // Ensure different Created times
        var obj2 = new TestValueObject("Test", 42);
        
        // Act & Assert
        Assert.That(obj1.Created, Is.Not.EqualTo(obj2.Created));
        Assert.That(obj1, Is.EqualTo(obj2)); // Still equal because Created is ignored
    }
    
    [Test]
    public void NullComparison_ShouldWork()
    {
        // Arrange
        var obj = new TestValueObject("Test", 42);
        
        // Act & Assert
        Assert.That(obj.Equals(null), Is.False);
        Assert.That(obj == null, Is.False);
        Assert.That(obj != null, Is.True);
    }
    
    [Test]
    public void HashSet_ShouldWorkCorrectly()
    {
        // Arrange
        var obj1 = new TestValueObject("Test", 42);
        var obj2 = new TestValueObject("Test", 42);
        var obj3 = new TestValueObject("Different", 42);
        
        var hashSet = new HashSet<TestValueObject> { obj1, obj2, obj3 };
        
        // Act & Assert
        Assert.That(hashSet.Count, Is.EqualTo(2)); // obj1 and obj2 are considered equal
        Assert.That(hashSet.Contains(new TestValueObject("Test", 42)), Is.True);
        Assert.That(hashSet.Contains(new TestValueObject("Different", 42)), Is.True);
        Assert.That(hashSet.Contains(new TestValueObject("NotFound", 42)), Is.False);
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class EquatableObjectIntegrationTests
{
    [Test]
    public void ComplexValueObject_ShouldWorkInCollections()
    {
        // Arrange
        var addresses = new List<Address>
        {
            new("123 Main St", "Anytown", "CA", "12345"),
            new("456 Oak Ave", "Otherville", "NY", "67890"),
            new("123 Main St", "Anytown", "CA", "12345") // Duplicate
        };
        
        // Act
        var uniqueAddresses = addresses.Distinct().ToList();
        var addressSet = addresses.ToHashSet();
        
        // Assert
        Assert.That(uniqueAddresses.Count, Is.EqualTo(2));
        Assert.That(addressSet.Count, Is.EqualTo(2));
    }
    
    [Test]
    public void ValueObject_ShouldWorkAsDictionaryKey()
    {
        // Arrange
        var priceList = new Dictionary<Money, string>();
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD"); // Equal to money1
        
        // Act
        priceList[money1] = "First entry";
        priceList[money2] = "Second entry"; // Should overwrite first
        
        // Assert
        Assert.That(priceList.Count, Is.EqualTo(1));
        Assert.That(priceList[money1], Is.EqualTo("Second entry"));
        Assert.That(priceList[money2], Is.EqualTo("Second entry"));
    }
}
```

## Best Practices

### 1. Use for Value Objects, Not Entities
```csharp
// Good - Value Object
public class Email : EquatableObject<Email>
{
    public string Address { get; }
    
    public Email(string address)
    {
        if (!IsValidEmail(address))
            throw new ArgumentException("Invalid email format", nameof(address));
        Address = address.ToLowerInvariant();
    }
    
    private static bool IsValidEmail(string email) => /* validation logic */;
}

// Bad - Entity should use identity-based equality
public class User // DON'T inherit from EquatableObject for entities
{
    public Guid Id { get; }
    public Email Email { get; set; }
    // Use identity-based equality instead
}
```

### 2. Consider Performance for Frequently Compared Objects
```csharp
public class HighFrequencyValue : EquatableObject<HighFrequencyValue>
{
    private readonly int _cachedHashCode;
    
    public HighFrequencyValue(string value)
    {
        Value = value;
        _cachedHashCode = value?.GetHashCode() ?? 0;
    }
    
    public string Value { get; }
    
    public override int GetHashCode() => _cachedHashCode;
    
    // Override for better performance
    protected override List<object?> GetAtomicValues() => new() { Value };
}
```

### 3. Handle Collections Carefully
```csharp
public class OrderedList : EquatableObject<OrderedList>
{
    private readonly List<string> _items;
    
    public IReadOnlyList<string> Items => _items.AsReadOnly();
    
    public OrderedList(IEnumerable<string> items)
    {
        _items = items.ToList();
    }
    
    // Order matters for equality
    protected override List<object?> GetAtomicValues()
    {
        return new List<object?> { string.Join("|", _items) };
    }
}

public class UnorderedSet : EquatableObject<UnorderedSet>
{
    private readonly HashSet<string> _items;
    
    public IReadOnlySet<string> Items => _items;
    
    public UnorderedSet(IEnumerable<string> items)
    {
        _items = items.ToHashSet();
    }
    
    // Order doesn't matter for equality
    protected override List<object?> GetAtomicValues()
    {
        var sorted = _items.OrderBy(x => x).ToList();
        return new List<object?> { string.Join("|", sorted) };
    }
}
```

### 4. Validate Invariants in Constructor
```csharp
public class PositiveInteger : EquatableObject<PositiveInteger>
{
    public int Value { get; }
    
    public PositiveInteger(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be positive", nameof(value));
        Value = value;
    }
}
```

## Error Handling

### Common Issues and Solutions

```csharp
public class RobustValueObject : EquatableObject<RobustValueObject>
{
    public string? NullableProperty { get; }
    public List<string> CollectionProperty { get; }
    
    public RobustValueObject(string? nullableProperty, IEnumerable<string>? collection)
    {
        NullableProperty = nullableProperty;
        CollectionProperty = collection?.ToList() ?? new List<string>();
    }
    
    protected override List<object?> GetAtomicValues()
    {
        // Handle null values and collections properly
        var values = new List<object?>
        {
            NullableProperty, // null is fine
            CollectionProperty.Count, // Use count instead of reference
            string.Join("|", CollectionProperty.OrderBy(x => x)) // Ordered string representation
        };
        
        return values;
    }
}
```

## See Also

- [ImmutableObject](ImmutableObject.md) - Immutable object patterns with equality
- [DisposableObject](DisposableObject.md) - Resource management with inherited equality
- [NotifiableObject](NotifiableObject.md) - Change notification with equality support
- [IgnoreMemberAttribute](../Attributes/IgnoreMemberAttribute.md) - Exclude members from equality
- [ObjectHelper](../Helpers/ObjectHelper.md) - Object manipulation utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application.Objects namespace - providing comprehensive value-based equality infrastructure for .NET applications.*