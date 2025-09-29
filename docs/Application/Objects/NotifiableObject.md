# NotifiableObject

The `NotifiableObject` abstract class provides a foundation for implementing change notification patterns in .NET applications. It defines the `NotifiableChangeType` enumeration for tracking different types of changes (Added, Modified, Removed) and serves as a base for objects that need to notify observers about state changes.

## Overview

```csharp
public abstract class NotifiableObject
{
    public enum NotifiableChangeType
    {
        Added = 0,
        Modified,
        Removed
    }
}
```

`NotifiableObject` provides the infrastructure for change tracking and notification patterns, commonly used in MVVM architectures, data binding scenarios, and reactive programming patterns where objects need to notify dependents about state changes.

## Key Features

- **Change Type Classification**: Standardized enumeration for change types
- **Base Infrastructure**: Foundation for implementing `INotifyPropertyChanged` patterns
- **MVVM Support**: Essential building block for data binding and UI frameworks
- **Observer Pattern**: Enables reactive programming and change observation
- **Extensible Design**: Abstract base allows custom notification implementations

## NotifiableChangeType Enumeration

### Change Types

#### Added (0)
Indicates that a new item or property has been added.

```csharp
NotifiableChangeType.Added
```

**Use Cases:**
- New items added to collections
- New properties dynamically added to objects
- New relationships established
- New data entries created

#### Modified (1)
Indicates that an existing item or property has been changed.

```csharp
NotifiableChangeType.Modified
```

**Use Cases:**
- Property values updated
- Collection items modified
- Data records updated
- State transitions

#### Removed (2)
Indicates that an item or property has been removed or deleted.

```csharp
NotifiableChangeType.Removed
```

**Use Cases:**
- Items removed from collections
- Properties cleared or nullified
- Data records deleted
- Relationships terminated

## Implementation Examples

### Basic Property Change Notification

```csharp
public class ObservableEntity : NotifiableObject, INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _value;
    private bool _isActive;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
    
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        OnNotifiableChanged(NotifiableChangeType.Modified, propertyName, null);
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        var oldValue = field;
        field = value;
        OnPropertyChanged(propertyName);
        OnPropertyChangedWithValues(propertyName, oldValue, value);
        return true;
    }
    
    protected virtual void OnNotifiableChanged(NotifiableChangeType changeType, string? propertyName, object? additionalData)
    {
        // Override in derived classes for custom change handling
        Console.WriteLine($"Property '{propertyName}' {changeType}");
    }
    
    protected virtual void OnPropertyChangedWithValues(string? propertyName, object? oldValue, object? newValue)
    {
        // Override for value-aware change handling
        Console.WriteLine($"Property '{propertyName}' changed from '{oldValue}' to '{newValue}'");
    }
}

// Usage example
public void DemonstrateBasicNotification()
{
    var entity = new ObservableEntity();
    
    entity.PropertyChanged += (sender, e) => 
        Console.WriteLine($"PropertyChanged event: {e.PropertyName}");
    
    // Property changes trigger notifications
    entity.Name = "Test Entity";     // PropertyChanged: Name, Modified notification
    entity.Value = 42;               // PropertyChanged: Value, Modified notification
    entity.IsActive = true;          // PropertyChanged: IsActive, Modified notification
    
    // No notification for same value
    entity.Name = "Test Entity";     // No notification (same value)
}
```

### Advanced Change Tracking

```csharp
public class ChangeTrackingEntity : NotifiableObject, INotifyPropertyChanged
{
    private readonly Dictionary<string, object?> _originalValues = new();
    private readonly Dictionary<string, object?> _currentValues = new();
    private readonly List<PropertyChangeInfo> _changeHistory = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ChangeNotificationEventArgs>? ChangeNotification;
    
    public IReadOnlyList<PropertyChangeInfo> ChangeHistory => _changeHistory.AsReadOnly();
    public bool HasChanges => _changeHistory.Any(c => c.ChangeType != NotifiableChangeType.Added || _originalValues.ContainsKey(c.PropertyName));
    
    protected void InitializeProperty<T>(string propertyName, T value)
    {
        _originalValues[propertyName] = value;
        _currentValues[propertyName] = value;
    }
    
    protected T GetProperty<T>(string propertyName)
    {
        return _currentValues.TryGetValue(propertyName, out var value) && value is T typedValue 
            ? typedValue 
            : default!;
    }
    
    protected bool SetProperty<T>(T value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) return false;
        
        var currentValue = GetProperty<T>(propertyName);
        if (EqualityComparer<T>.Default.Equals(currentValue, value))
            return false;
        
        var oldValue = currentValue;
        _currentValues[propertyName] = value;
        
        // Determine change type
        var changeType = DetermineChangeType(propertyName, oldValue, value);
        
        // Record change
        var changeInfo = new PropertyChangeInfo(propertyName, changeType, oldValue, value, DateTime.UtcNow);
        _changeHistory.Add(changeInfo);
        
        // Fire events
        OnPropertyChanged(propertyName);
        OnChangeNotification(changeInfo);
        
        return true;
    }
    
    private NotifiableChangeType DetermineChangeType(string propertyName, object? oldValue, object? newValue)
    {
        var hasOriginal = _originalValues.ContainsKey(propertyName);
        
        return (hasOriginal, oldValue, newValue) switch
        {
            (false, _, _) => NotifiableChangeType.Added,
            (true, _, null) => NotifiableChangeType.Removed,
            (true, _, _) => NotifiableChangeType.Modified
        };
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected virtual void OnChangeNotification(PropertyChangeInfo changeInfo)
    {
        var args = new ChangeNotificationEventArgs(changeInfo);
        ChangeNotification?.Invoke(this, args);
    }
    
    public void AcceptChanges()
    {
        // Update original values to current values
        _originalValues.Clear();
        foreach (var kvp in _currentValues)
        {
            _originalValues[kvp.Key] = kvp.Value;
        }
        
        _changeHistory.Clear();
        OnPropertyChanged(nameof(HasChanges));
    }
    
    public void RejectChanges()
    {
        // Revert to original values
        _currentValues.Clear();
        foreach (var kvp in _originalValues)
        {
            _currentValues[kvp.Key] = kvp.Value;
        }
        
        _changeHistory.Clear();
        OnPropertyChanged(nameof(HasChanges));
        
        // Notify all properties changed
        foreach (var propertyName in _originalValues.Keys)
        {
            OnPropertyChanged(propertyName);
        }
    }
    
    public T? GetOriginalValue<T>(string propertyName)
    {
        return _originalValues.TryGetValue(propertyName, out var value) && value is T typedValue 
            ? typedValue 
            : default;
    }
    
    public IEnumerable<string> GetChangedProperties()
    {
        return _changeHistory.Select(c => c.PropertyName).Distinct();
    }
}

public class PropertyChangeInfo
{
    public string PropertyName { get; }
    public NotifiableObject.NotifiableChangeType ChangeType { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public DateTime Timestamp { get; }
    
    public PropertyChangeInfo(string propertyName, NotifiableObject.NotifiableChangeType changeType, 
                             object? oldValue, object? newValue, DateTime timestamp)
    {
        PropertyName = propertyName;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        Timestamp = timestamp;
    }
    
    public override string ToString() => $"{PropertyName}: {ChangeType} ({OldValue} -> {NewValue}) at {Timestamp:HH:mm:ss}";
}

public class ChangeNotificationEventArgs : EventArgs
{
    public PropertyChangeInfo ChangeInfo { get; }
    
    public ChangeNotificationEventArgs(PropertyChangeInfo changeInfo)
    {
        ChangeInfo = changeInfo;
    }
}

// Concrete implementation
public class Product : ChangeTrackingEntity
{
    public Product(string name, decimal price, string category)
    {
        InitializeProperty(nameof(Name), name);
        InitializeProperty(nameof(Price), price);
        InitializeProperty(nameof(Category), category);
        InitializeProperty(nameof(CreatedAt), DateTime.UtcNow);
    }
    
    public string Name
    {
        get => GetProperty<string>(nameof(Name));
        set => SetProperty(value);
    }
    
    public decimal Price
    {
        get => GetProperty<decimal>(nameof(Price));
        set => SetProperty(value);
    }
    
    public string Category
    {
        get => GetProperty<string>(nameof(Category));
        set => SetProperty(value);
    }
    
    public DateTime CreatedAt
    {
        get => GetProperty<DateTime>(nameof(CreatedAt));
        private set => SetProperty(value);
    }
    
    public void UpdatePrice(decimal newPrice, string reason)
    {
        Console.WriteLine($"Updating price: {reason}");
        Price = newPrice;
    }
}

// Usage example
public void DemonstrateAdvancedChangeTracking()
{
    var product = new Product("Laptop", 999.99m, "Electronics");
    
    product.ChangeNotification += (sender, e) =>
        Console.WriteLine($"Change detected: {e.ChangeInfo}");
    
    Console.WriteLine($"Initial state - Has changes: {product.HasChanges}");
    
    // Make changes
    product.Name = "Gaming Laptop";
    product.UpdatePrice(1299.99m, "Price increase for gaming features");
    product.Category = "Gaming";
    
    Console.WriteLine($"\nAfter changes - Has changes: {product.HasChanges}");
    Console.WriteLine($"Changed properties: {string.Join(", ", product.GetChangedProperties())}");
    
    Console.WriteLine("\nChange history:");
    foreach (var change in product.ChangeHistory)
    {
        Console.WriteLine($"  {change}");
    }
    
    // Accept changes
    Console.WriteLine("\nAccepting changes...");
    product.AcceptChanges();
    Console.WriteLine($"After accept - Has changes: {product.HasChanges}");
    
    // Make more changes
    product.Price = 1199.99m;
    Console.WriteLine($"\nAfter new change - Has changes: {product.HasChanges}");
    
    // Reject changes
    Console.WriteLine("Rejecting changes...");
    product.RejectChanges();
    Console.WriteLine($"After reject - Price: {product.Price}, Has changes: {product.HasChanges}");
}
```

### Collection Change Notification

```csharp
public class NotifiableCollection<T> : NotifiableObject, INotifyCollectionChanged, IList<T>
{
    private readonly List<T> _items = new();
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event EventHandler<CollectionChangeNotificationEventArgs>? CollectionChangeNotification;
    
    public int Count => _items.Count;
    public bool IsReadOnly => false;
    
    public T this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            _items[index] = value;
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, oldItem, index);
            OnCollectionChangeNotification(NotifiableChangeType.Modified, value, oldItem, index);
        }
    }
    
    public void Add(T item)
    {
        _items.Add(item);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, default, _items.Count - 1);
        OnCollectionChangeNotification(NotifiableChangeType.Added, item, default, _items.Count - 1);
    }
    
    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        if (index < 0) return false;
        
        _items.RemoveAt(index);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, default, item, index);
        OnCollectionChangeNotification(NotifiableChangeType.Removed, default, item, index);
        return true;
    }
    
    public void Insert(int index, T item)
    {
        _items.Insert(index, item);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, default, index);
        OnCollectionChangeNotification(NotifiableChangeType.Added, item, default, index);
    }
    
    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, default, item, index);
        OnCollectionChangeNotification(NotifiableChangeType.Removed, default, item, index);
    }
    
    public void Clear()
    {
        var oldItems = _items.ToList();
        _items.Clear();
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, default, default, -1);
        
        foreach (var item in oldItems)
        {
            OnCollectionChangeNotification(NotifiableChangeType.Removed, default, item, -1);
        }
    }
    
    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, T? newItem, T? oldItem, int index)
    {
        var args = action switch
        {
            NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(action, newItem, index),
            NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(action, oldItem, index),
            NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index),
            NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(action),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
        
        CollectionChanged?.Invoke(this, args);
    }
    
    protected virtual void OnCollectionChangeNotification(NotifiableChangeType changeType, T? newItem, T? oldItem, int index)
    {
        var args = new CollectionChangeNotificationEventArgs(changeType, newItem, oldItem, index);
        CollectionChangeNotification?.Invoke(this, args);
    }
    
    // IList<T> implementation
    public int IndexOf(T item) => _items.IndexOf(item);
    public bool Contains(T item) => _items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class CollectionChangeNotificationEventArgs : EventArgs
{
    public NotifiableObject.NotifiableChangeType ChangeType { get; }
    public object? NewItem { get; }
    public object? OldItem { get; }
    public int Index { get; }
    public DateTime Timestamp { get; }
    
    public CollectionChangeNotificationEventArgs(NotifiableObject.NotifiableChangeType changeType, 
                                                object? newItem, object? oldItem, int index)
    {
        ChangeType = changeType;
        NewItem = newItem;
        OldItem = oldItem;
        Index = index;
        Timestamp = DateTime.UtcNow;
    }
    
    public override string ToString() => $"{ChangeType}: {OldItem} -> {NewItem} at index {Index}";
}

// Usage example
public void DemonstrateCollectionNotification()
{
    var collection = new NotifiableCollection<string>();
    
    collection.CollectionChanged += (sender, e) =>
        Console.WriteLine($"CollectionChanged: {e.Action}");
    
    collection.CollectionChangeNotification += (sender, e) =>
        Console.WriteLine($"ChangeNotification: {e}");
    
    // Add items
    collection.Add("First");      // Added notification
    collection.Add("Second");     // Added notification
    collection.Add("Third");      // Added notification
    
    Console.WriteLine($"Collection count: {collection.Count}");
    
    // Modify item
    collection[1] = "Modified Second";  // Modified notification
    
    // Remove item
    collection.Remove("First");   // Removed notification
    
    // Insert item
    collection.Insert(0, "New First");  // Added notification
    
    Console.WriteLine($"Final items: {string.Join(", ", collection)}");
}
```

### MVVM Integration

```csharp
public abstract class ViewModelBase : NotifiableObject, INotifyPropertyChanged
{
    private bool _isBusy;
    private string _busyMessage = string.Empty;
    private readonly Dictionary<string, List<string>> _validationErrors = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    
    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }
    
    public bool HasErrors => _validationErrors.Count > 0;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        ValidateProperty(value, propertyName);
        return true;
    }
    
    protected virtual void ValidateProperty(object? value, string? propertyName)
    {
        if (propertyName == null) return;
        
        var errors = new List<string>();
        
        // Override in derived classes for validation logic
        ValidatePropertyCore(value, propertyName, errors);
        
        if (errors.Count > 0)
            _validationErrors[propertyName] = errors;
        else
            _validationErrors.Remove(propertyName);
            
        OnPropertyChanged(nameof(HasErrors));
    }
    
    protected virtual void ValidatePropertyCore(object? value, string propertyName, List<string> errors)
    {
        // Override in derived classes
    }
    
    public IEnumerable<string> GetErrors(string propertyName)
    {
        return _validationErrors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }
    
    public void ClearErrors()
    {
        _validationErrors.Clear();
        OnPropertyChanged(nameof(HasErrors));
    }
    
    protected async Task ExecuteAsync(Func<Task> operation, string busyMessage = "Processing...")
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            BusyMessage = busyMessage;
            
            await operation();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }
    
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string busyMessage = "Processing...")
    {
        if (IsBusy) return default!;
        
        try
        {
            IsBusy = true;
            BusyMessage = busyMessage;
            
            return await operation();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }
}

public class ProductViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private decimal _price;
    private string _category = string.Empty;
    private bool _isAvailable = true;
    private readonly NotifiableCollection<string> _tags = new();
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }
    
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }
    
    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }
    
    public NotifiableCollection<string> Tags => _tags;
    
    public ICommand SaveCommand { get; }
    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }
    
    public ProductViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        AddTagCommand = new RelayCommand<string>(AddTag, CanAddTag);
        RemoveTagCommand = new RelayCommand<string>(RemoveTag);
        
        // Subscribe to tag collection changes
        _tags.CollectionChangeNotification += OnTagsChanged;
    }
    
    private void OnTagsChanged(object? sender, CollectionChangeNotificationEventArgs e)
    {
        OnPropertyChanged(nameof(Tags));
        ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }
    
    protected override void ValidatePropertyCore(object? value, string propertyName, List<string> errors)
    {
        switch (propertyName)
        {
            case nameof(Name):
                if (string.IsNullOrWhiteSpace(value as string))
                    errors.Add("Name is required");
                break;
                
            case nameof(Price):
                if (value is decimal price && price < 0)
                    errors.Add("Price cannot be negative");
                break;
                
            case nameof(Category):
                if (string.IsNullOrWhiteSpace(value as string))
                    errors.Add("Category is required");
                break;
        }
    }
    
    private bool CanSave() => !HasErrors && !IsBusy && !string.IsNullOrWhiteSpace(Name);
    
    private async Task SaveAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Simulate save operation
            await Task.Delay(2000);
            
            Console.WriteLine($"Saved product: {Name} - ${Price} in {Category}");
            Console.WriteLine($"Tags: {string.Join(", ", Tags)}");
            
        }, "Saving product...");
    }
    
    private bool CanAddTag(string? tag) => !string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag!);
    
    private void AddTag(string? tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            Tags.Add(tag);
    }
    
    private void RemoveTag(string? tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            Tags.Remove(tag);
    }
}

// Command implementations (simplified)
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    public void Execute(object? parameter) => _execute((T?)parameter);
    
    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;
    
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);
    
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        
        try
        {
            _isExecuting = true;
            NotifyCanExecuteChanged();
            
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            NotifyCanExecuteChanged();
        }
    }
    
    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

// Usage example
public void DemonstrateMVVMIntegration()
{
    var viewModel = new ProductViewModel();
    
    viewModel.PropertyChanged += (sender, e) =>
        Console.WriteLine($"Property changed: {e.PropertyName}");
    
    // Set properties (triggers validation and notifications)
    viewModel.Name = "Sample Product";
    viewModel.Price = 29.99m;
    viewModel.Category = "Electronics";
    
    // Work with tags
    viewModel.AddTagCommand.Execute("Popular");
    viewModel.AddTagCommand.Execute("New");
    viewModel.AddTagCommand.Execute("Electronics");
    
    Console.WriteLine($"Can save: {viewModel.SaveCommand.CanExecute(null)}");
    Console.WriteLine($"Has errors: {viewModel.HasErrors}");
    
    // Trigger save
    if (viewModel.SaveCommand.CanExecute(null))
    {
        viewModel.SaveCommand.Execute(null);
    }
    
    // Test validation
    viewModel.Price = -10m; // Should trigger validation error
    Console.WriteLine($"Has errors after invalid price: {viewModel.HasErrors}");
    
    var priceErrors = viewModel.GetErrors(nameof(viewModel.Price));
    Console.WriteLine($"Price errors: {string.Join(", ", priceErrors)}");
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class NotifiableObjectTests
{
    private class TestNotifiableObject : NotifiableObject, INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _value;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    [Test]
    public void NotifiableChangeType_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.That((int)NotifiableObject.NotifiableChangeType.Added, Is.EqualTo(0));
        Assert.That((int)NotifiableObject.NotifiableChangeType.Modified, Is.EqualTo(1));
        Assert.That((int)NotifiableObject.NotifiableChangeType.Removed, Is.EqualTo(2));
    }
    
    [Test]
    public void PropertyChanged_ShouldFire_WhenPropertyChanges()
    {
        // Arrange
        var obj = new TestNotifiableObject();
        var changedProperties = new List<string>();
        
        obj.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName!);
        
        // Act
        obj.Name = "Test";
        obj.Value = 42;
        
        // Assert
        Assert.That(changedProperties, Contains.Item(nameof(obj.Name)));
        Assert.That(changedProperties, Contains.Item(nameof(obj.Value)));
    }
    
    [Test]
    public void PropertyChanged_ShouldNotFire_WhenSameValue()
    {
        // Arrange
        var obj = new TestNotifiableObject();
        obj.Name = "Initial";
        
        var changeCount = 0;
        obj.PropertyChanged += (sender, e) => changeCount++;
        
        // Act
        obj.Name = "Initial"; // Same value
        
        // Assert
        Assert.That(changeCount, Is.EqualTo(0));
    }
}

[TestFixture]
public class NotifiableCollectionTests
{
    [Test]
    public void Add_ShouldTriggerNotifications()
    {
        // Arrange
        var collection = new NotifiableCollection<string>();
        var collectionChanges = new List<NotifyCollectionChangedAction>();
        var notificationChanges = new List<NotifiableObject.NotifiableChangeType>();
        
        collection.CollectionChanged += (s, e) => collectionChanges.Add(e.Action);
        collection.CollectionChangeNotification += (s, e) => notificationChanges.Add(e.ChangeType);
        
        // Act
        collection.Add("Item1");
        collection.Add("Item2");
        
        // Assert
        Assert.That(collectionChanges, Is.EqualTo(new[] { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add }));
        Assert.That(notificationChanges, Is.EqualTo(new[] { NotifiableObject.NotifiableChangeType.Added, NotifiableObject.NotifiableChangeType.Added }));
    }
    
    [Test]
    public void Remove_ShouldTriggerNotifications()
    {
        // Arrange
        var collection = new NotifiableCollection<string> { "Item1", "Item2" };
        var collectionChanges = new List<NotifyCollectionChangedAction>();
        var notificationChanges = new List<NotifiableObject.NotifiableChangeType>();
        
        collection.CollectionChanged += (s, e) => collectionChanges.Add(e.Action);
        collection.CollectionChangeNotification += (s, e) => notificationChanges.Add(e.ChangeType);
        
        // Act
        collection.Remove("Item1");
        
        // Assert
        Assert.That(collectionChanges, Contains.Item(NotifyCollectionChangedAction.Remove));
        Assert.That(notificationChanges, Contains.Item(NotifiableObject.NotifiableChangeType.Removed));
    }
}
```

## Best Practices

### 1. Use CallerMemberName for Property Names
```csharp
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

### 2. Implement Efficient Property Setters
```csharp
protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
{
    if (EqualityComparer<T>.Default.Equals(field, value))
        return false; // No change, no notification
        
    field = value;
    OnPropertyChanged(propertyName);
    return true; // Indicates change occurred
}
```

### 3. Handle Validation in Property Setters
```csharp
public string Email
{
    get => _email;
    set
    {
        if (SetProperty(ref _email, value))
        {
            ValidateEmail(value);
        }
    }
}
```

### 4. Use Weak Event Patterns for Long-Lived Objects
```csharp
public class WeakEventHandler
{
    public static void Subscribe<T>(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> handler)
        where T : class
    {
        // Use WeakEventManager or similar pattern to avoid memory leaks
        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
            .AddHandler(source, nameof(INotifyPropertyChanged.PropertyChanged), handler);
    }
}
```

## Error Handling

### Notification Error Handling

```csharp
public class RobustNotifiableObject : NotifiableObject, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        try
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        catch (Exception ex)
        {
            // Log error but don't let it crash the application
            Console.WriteLine($"Error in property change notification for {propertyName}: {ex.Message}");
        }
    }
}
```

## Performance Considerations

### Optimization Strategies

```csharp
public class OptimizedNotifiableObject : NotifiableObject, INotifyPropertyChanged
{
    private readonly PropertyChangedEventArgs[] _propertyChangedArgsCache = new PropertyChangedEventArgs[10];
    private int _cacheIndex;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (PropertyChanged == null || propertyName == null) return;
        
        // Reuse PropertyChangedEventArgs instances to reduce allocations
        var args = GetCachedPropertyChangedEventArgs(propertyName);
        PropertyChanged.Invoke(this, args);
    }
    
    private PropertyChangedEventArgs GetCachedPropertyChangedEventArgs(string propertyName)
    {
        // Simple cache implementation
        var index = _cacheIndex++ % _propertyChangedArgsCache.Length;
        return _propertyChangedArgsCache[index] ??= new PropertyChangedEventArgs(propertyName);
    }
}
```

## See Also

- [EquatableObject](EquatableObject.md) - Value-based equality foundation
- [DisposableObject](DisposableObject.md) - Resource management with notifications
- [ImmutableObject](ImmutableObject.md) - Immutable objects with notification support
- [CompressedObject](CompressedObject.md) - Compressed data containers
- [ChangeTrackingItem](../ChangeTrackingItems/ChangeTrackingItem.md) - Advanced change tracking

---

*Part of the RapidStreamer.BuildingBlocks.Application.Objects namespace - providing change notification infrastructure for MVVM and reactive programming patterns in .NET applications.*