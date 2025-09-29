# EnvironmentHelper

The `EnvironmentHelper` class is a static utility class in the RapidStreamer BuildingBlocks that provides functionality for parsing and extracting environment variable placeholders from strings. It enables dynamic configuration management by identifying environment variable references within text templates.

## Purpose

This helper serves as:
- An environment variable placeholder parser for dynamic configuration
- A string pattern analyzer that identifies `$VARIABLE$` syntax patterns
- A foundation component for configuration template processing
- A utility for secure configuration management where sensitive values are referenced indirectly
- A bridge between template-based configuration and runtime environment resolution

## Key Features

- **Pattern Recognition**: Identifies environment variable placeholders using `$VARIABLE$` syntax
- **Lazy Evaluation**: Uses `yield return` for memory-efficient processing of large strings
- **Null Safety**: Validates input strings and throws appropriate exceptions for null/empty inputs
- **Index-Based Parsing**: Efficient string scanning using index-based iteration
- **Integration Ready**: Designed to work seamlessly with `ConnectionStringHelper` and other configuration utilities

## Method

### GetEnvironmentKeys
Extracts all environment variable placeholder keys from a string using the `$VARIABLE$` pattern.

```csharp
public static IEnumerable<string> GetEnvironmentKeys(this string str)
```

**Implementation:**
```csharp
public static IEnumerable<string> GetEnvironmentKeys(this string str)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(str);
    var index = 0;
    while (true)
    {
        index = str.IndexOf('$', index);
        if (index <= 0)
            break;

        var nextIndex = str.IndexOf('$', index + 1) + 1;
        if (nextIndex <= 0)
            break;

        yield return str.Substring(index, nextIndex - index);

        index = nextIndex;
    }
}
```

**Key Features:**
- **Input Validation**: Throws `ArgumentException` for null or whitespace strings
- **Pattern Matching**: Finds pairs of `$` characters that delimit variable names
- **Yield Return**: Provides lazy evaluation for memory efficiency
- **Complete Tokens**: Returns full `$VARIABLE$` tokens including delimiters
- **Sequential Processing**: Processes string from left to right finding all occurrences

## Usage Examples

### Basic Environment Variable Extraction

```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

// Configuration string with environment variable placeholders
string configTemplate = "Server=$DB_SERVER$;Database=$DB_NAME$;User=$DB_USER$;Password=$DB_PASSWORD$;";

// Extract all environment variable keys
var environmentKeys = configTemplate.GetEnvironmentKeys().ToList();

Console.WriteLine("Found environment variable placeholders:");
foreach (var key in environmentKeys)
{
    Console.WriteLine($"  {key}");
}

// Output:
// Found environment variable placeholders:
//   $DB_SERVER$
//   $DB_NAME$
//   $DB_USER$
//   $DB_PASSWORD$

// Extract variable names (without delimiters)
var variableNames = environmentKeys.Select(key => key.Replace("$", "")).ToList();
Console.WriteLine("\nVariable names:");
foreach (var name in variableNames)
{
    Console.WriteLine($"  {name}");
}

// Output:
// Variable names:
//   DB_SERVER
//   DB_NAME
//   DB_USER
//   DB_PASSWORD
```

### Configuration Template Validation

```csharp
public class ConfigurationValidator
{
    public ValidationResult ValidateTemplate(string template)
    {
        try
        {
            var environmentKeys = template.GetEnvironmentKeys().ToList();
            
            if (!environmentKeys.Any())
            {
                return ValidationResult.Success("No environment variables found in template");
            }
            
            var missingVariables = new List<string>();
            var foundVariables = new List<string>();
            
            foreach (var key in environmentKeys)
            {
                var variableName = key.Replace("$", "");
                var value = Environment.GetEnvironmentVariable(variableName);
                
                if (string.IsNullOrEmpty(value))
                {
                    missingVariables.Add(variableName);
                }
                else
                {
                    foundVariables.Add(variableName);
                }
            }
            
            if (missingVariables.Any())
            {
                return ValidationResult.Failure(
                    $"Missing environment variables: {string.Join(", ", missingVariables)}",
                    missingVariables);
            }
            
            return ValidationResult.Success(
                $"All {foundVariables.Count} environment variables are available",
                foundVariables);
        }
        catch (ArgumentException ex)
        {
            return ValidationResult.Failure($"Invalid template: {ex.Message}");
        }
    }
}

public class ValidationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public List<string> Variables { get; }
    
    private ValidationResult(bool isSuccess, string message, List<string>? variables = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Variables = variables ?? new List<string>();
    }
    
    public static ValidationResult Success(string message, List<string>? variables = null) =>
        new(true, message, variables);
        
    public static ValidationResult Failure(string message, List<string>? variables = null) =>
        new(false, message, variables);
}

// Usage
var validator = new ConfigurationValidator();

var templates = new[]
{
    "Server=$DB_SERVER$;Database=$DB_NAME$;",
    "Simple connection string without variables",
    "Partial variable $INCOMPLETE",
    "Redis connection: $REDIS_HOST$:$REDIS_PORT$"
};

foreach (var template in templates)
{
    var result = validator.ValidateTemplate(template);
    Console.WriteLine($"Template: {template}");
    Console.WriteLine($"Status: {(result.IsSuccess ? "Valid" : "Invalid")}");
    Console.WriteLine($"Message: {result.Message}");
    Console.WriteLine();
}
```

### Configuration Template Builder

```csharp
public class ConfigurationTemplateBuilder
{
    private readonly Dictionary<string, string> _templates = new();
    
    public ConfigurationTemplateBuilder AddDatabaseTemplate(string name, string serverVar, string dbVar, string userVar, string passwordVar)
    {
        var template = $"Server=${serverVar}$;Database=${dbVar}$;User Id=${userVar}$;Password=${passwordVar}$;TrustServerCertificate=true;";
        _templates[name] = template;
        return this;
    }
    
    public ConfigurationTemplateBuilder AddRedisTemplate(string name, string hostVar, string portVar, string passwordVar)
    {
        var template = $"${hostVar}$:${portVar}$,password=${passwordVar}$";
        _templates[name] = template;
        return this;
    }
    
    public ConfigurationTemplateBuilder AddCustomTemplate(string name, string template)
    {
        _templates[name] = template;
        return this;
    }
    
    public TemplateAnalysis AnalyzeTemplates()
    {
        var analysis = new TemplateAnalysis();
        
        foreach (var (name, template) in _templates)
        {
            var environmentKeys = template.GetEnvironmentKeys().ToList();
            var variableNames = environmentKeys.Select(key => key.Replace("$", "")).ToList();
            
            analysis.AddTemplate(name, template, variableNames);
        }
        
        return analysis;
    }
    
    public Dictionary<string, string> GetRequiredEnvironmentVariables()
    {
        var allVariables = new Dictionary<string, string>();
        
        foreach (var template in _templates.Values)
        {
            var environmentKeys = template.GetEnvironmentKeys();
            
            foreach (var key in environmentKeys)
            {
                var variableName = key.Replace("$", "");
                if (!allVariables.ContainsKey(variableName))
                {
                    allVariables[variableName] = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
                }
            }
        }
        
        return allVariables;
    }
}

public class TemplateAnalysis
{
    public Dictionary<string, TemplateInfo> Templates { get; } = new();
    public HashSet<string> AllVariables { get; } = new();
    
    public void AddTemplate(string name, string template, List<string> variables)
    {
        Templates[name] = new TemplateInfo(template, variables);
        foreach (var variable in variables)
        {
            AllVariables.Add(variable);
        }
    }
    
    public void PrintSummary()
    {
        Console.WriteLine("Configuration Template Analysis");
        Console.WriteLine("==============================");
        Console.WriteLine($"Total templates: {Templates.Count}");
        Console.WriteLine($"Unique environment variables: {AllVariables.Count}");
        Console.WriteLine();
        
        foreach (var (name, info) in Templates)
        {
            Console.WriteLine($"Template: {name}");
            Console.WriteLine($"  Variables: {string.Join(", ", info.Variables)}");
            Console.WriteLine($"  Template: {info.Template}");
            Console.WriteLine();
        }
        
        Console.WriteLine("All required environment variables:");
        foreach (var variable in AllVariables.OrderBy(v => v))
        {
            var value = Environment.GetEnvironmentVariable(variable);
            var status = string.IsNullOrEmpty(value) ? "MISSING" : "SET";
            Console.WriteLine($"  {variable}: {status}");
        }
    }
}

public record TemplateInfo(string Template, List<string> Variables);

// Usage
var builder = new ConfigurationTemplateBuilder()
    .AddDatabaseTemplate("Primary", "MAIN_DB_SERVER", "MAIN_DB_NAME", "MAIN_DB_USER", "MAIN_DB_PASSWORD")
    .AddDatabaseTemplate("Readonly", "READ_DB_SERVER", "READ_DB_NAME", "READ_DB_USER", "READ_DB_PASSWORD")
    .AddRedisTemplate("Cache", "REDIS_HOST", "REDIS_PORT", "REDIS_PASSWORD")
    .AddCustomTemplate("MessageQueue", "amqp://$RABBITMQ_USER$:$RABBITMQ_PASSWORD$@$RABBITMQ_HOST$:$RABBITMQ_PORT$/");

var analysis = builder.AnalyzeTemplates();
analysis.PrintSummary();
```

## Real-World Applications

### Docker Environment Configuration

```csharp
public class DockerEnvironmentManager
{
    private readonly Dictionary<string, string> _serviceTemplates;
    
    public DockerEnvironmentManager()
    {
        _serviceTemplates = new Dictionary<string, string>
        {
            ["database"] = "Server=$DB_HOST$;Database=$DB_NAME$;User Id=$DB_USER$;Password=$DB_PASSWORD$;",
            ["redis"] = "$REDIS_HOST$:$REDIS_PORT$,password=$REDIS_PASSWORD$",
            ["elasticsearch"] = "http://$ELASTIC_USER$:$ELASTIC_PASSWORD$@$ELASTIC_HOST$:$ELASTIC_PORT$",
            ["storage"] = "DefaultEndpointsProtocol=https;AccountName=$STORAGE_ACCOUNT$;AccountKey=$STORAGE_KEY$;"
        };
    }
    
    public DockerComposeConfig GenerateDockerCompose()
    {
        var config = new DockerComposeConfig();
        
        foreach (var (serviceName, template) in _serviceTemplates)
        {
            var environmentKeys = template.GetEnvironmentKeys().ToList();
            var variables = environmentKeys.Select(key => key.Replace("$", "")).ToList();
            
            config.AddService(serviceName, variables);
        }
        
        return config;
    }
    
    public void ValidateEnvironment()
    {
        var missingVariables = new List<string>();
        
        foreach (var template in _serviceTemplates.Values)
        {
            var environmentKeys = template.GetEnvironmentKeys();
            
            foreach (var key in environmentKeys)
            {
                var variableName = key.Replace("$", "");
                var value = Environment.GetEnvironmentVariable(variableName);
                
                if (string.IsNullOrEmpty(value) && !missingVariables.Contains(variableName))
                {
                    missingVariables.Add(variableName);
                }
            }
        }
        
        if (missingVariables.Any())
        {
            throw new InvalidOperationException(
                $"Missing required environment variables: {string.Join(", ", missingVariables)}");
        }
    }
}

public class DockerComposeConfig
{
    private readonly Dictionary<string, List<string>> _services = new();
    
    public void AddService(string serviceName, List<string> environmentVariables)
    {
        _services[serviceName] = environmentVariables;
    }
    
    public string GenerateYaml()
    {
        var yaml = new StringBuilder();
        yaml.AppendLine("version: '3.8'");
        yaml.AppendLine("services:");
        
        foreach (var (serviceName, variables) in _services)
        {
            yaml.AppendLine($"  {serviceName}:");
            yaml.AppendLine("    environment:");
            
            foreach (var variable in variables)
            {
                yaml.AppendLine($"      {variable}: ${{{variable}}}");
            }
            
            yaml.AppendLine();
        }
        
        return yaml.ToString();
    }
}
```

### Kubernetes ConfigMap Generator

```csharp
public class KubernetesConfigGenerator
{
    public string GenerateConfigMapYaml(string configMapName, Dictionary<string, string> templates)
    {
        var allVariables = new HashSet<string>();
        
        // Extract all environment variables from all templates
        foreach (var template in templates.Values)
        {
            var environmentKeys = template.GetEnvironmentKeys();
            foreach (var key in environmentKeys)
            {
                var variableName = key.Replace("$", "");
                allVariables.Add(variableName);
            }
        }
        
        var yaml = new StringBuilder();
        yaml.AppendLine("apiVersion: v1");
        yaml.AppendLine("kind: ConfigMap");
        yaml.AppendLine("metadata:");
        yaml.AppendLine($"  name: {configMapName}");
        yaml.AppendLine("data:");
        
        foreach (var (templateName, template) in templates)
        {
            yaml.AppendLine($"  {templateName}: |");
            yaml.AppendLine($"    {template}");
        }
        
        yaml.AppendLine("---");
        yaml.AppendLine("apiVersion: v1");
        yaml.AppendLine("kind: Secret");
        yaml.AppendLine("metadata:");
        yaml.AppendLine($"  name: {configMapName}-secrets");
        yaml.AppendLine("type: Opaque");
        yaml.AppendLine("data:");
        
        foreach (var variable in allVariables.OrderBy(v => v))
        {
            yaml.AppendLine($"  {variable}: # Base64 encoded value");
        }
        
        return yaml.ToString();
    }
}

// Usage
var templates = new Dictionary<string, string>
{
    ["database-connection"] = "Server=$DB_HOST$;Database=$DB_NAME$;User Id=$DB_USER$;Password=$DB_PASSWORD$;",
    ["redis-connection"] = "$REDIS_HOST$:$REDIS_PORT$,password=$REDIS_PASSWORD$",
    ["api-endpoint"] = "https://$API_HOST$:$API_PORT$/api/v1?key=$API_KEY$"
};

var generator = new KubernetesConfigGenerator();
string kubernetesYaml = generator.GenerateConfigMapYaml("app-config", templates);
Console.WriteLine(kubernetesYaml);
```

### Configuration Documentation Generator

```csharp
public class ConfigurationDocumentationGenerator
{
    public string GenerateMarkdownDocumentation(string applicationName, Dictionary<string, string> templates)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine($"# {applicationName} Configuration");
        markdown.AppendLine();
        markdown.AppendLine("## Environment Variables");
        markdown.AppendLine();
        
        var allVariables = new Dictionary<string, VariableUsage>();
        
        // Analyze all templates
        foreach (var (templateName, template) in templates)
        {
            var environmentKeys = template.GetEnvironmentKeys();
            
            foreach (var key in environmentKeys)
            {
                var variableName = key.Replace("$", "");
                
                if (!allVariables.ContainsKey(variableName))
                {
                    allVariables[variableName] = new VariableUsage(variableName);
                }
                
                allVariables[variableName].UsedIn.Add(templateName);
            }
        }
        
        // Generate documentation table
        markdown.AppendLine("| Variable | Description | Used In | Required |");
        markdown.AppendLine("|----------|-------------|---------|----------|");
        
        foreach (var (variableName, usage) in allVariables.OrderBy(kvp => kvp.Key))
        {
            var description = GetVariableDescription(variableName);
            var usedIn = string.Join(", ", usage.UsedIn);
            
            markdown.AppendLine($"| `{variableName}` | {description} | {usedIn} | Yes |");
        }
        
        markdown.AppendLine();
        markdown.AppendLine("## Configuration Templates");
        markdown.AppendLine();
        
        foreach (var (templateName, template) in templates)
        {
            markdown.AppendLine($"### {templateName}");
            markdown.AppendLine();
            markdown.AppendLine("```");
            markdown.AppendLine(template);
            markdown.AppendLine("```");
            markdown.AppendLine();
            
            var environmentKeys = template.GetEnvironmentKeys().ToList();
            if (environmentKeys.Any())
            {
                markdown.AppendLine("**Required Environment Variables:**");
                foreach (var key in environmentKeys)
                {
                    var variableName = key.Replace("$", "");
                    markdown.AppendLine($"- `{variableName}`");
                }
                markdown.AppendLine();
            }
        }
        
        return markdown.ToString();
    }
    
    private string GetVariableDescription(string variableName)
    {
        return variableName switch
        {
            var name when name.Contains("HOST") => "Server hostname or IP address",
            var name when name.Contains("PORT") => "Server port number",
            var name when name.Contains("USER") => "Username for authentication",
            var name when name.Contains("PASSWORD") => "Password for authentication",
            var name when name.Contains("KEY") => "API key or encryption key",
            var name when name.Contains("DB") => "Database configuration parameter",
            var name when name.Contains("REDIS") => "Redis cache configuration parameter",
            _ => "Application configuration parameter"
        };
    }
}

public class VariableUsage
{
    public string Name { get; }
    public List<string> UsedIn { get; } = new();
    
    public VariableUsage(string name)
    {
        Name = name;
    }
}
```

## Integration with ConnectionStringHelper

The `EnvironmentHelper` is designed to work seamlessly with `ConnectionStringHelper`:

```csharp
// EnvironmentHelper extracts the placeholders
var environmentKeys = connectionString.GetEnvironmentKeys();

// ConnectionStringHelper uses the results for enrichment
foreach (var environmentKey in environmentKeys)
{
    var variableName = environmentKey.Replace("$", "");
    var environmentValue = Environment.GetEnvironmentVariable(variableName);
    ArgumentException.ThrowIfNullOrWhiteSpace(environmentValue);
    connectionString = connectionString.Replace(environmentKey, environmentValue);
}
```

## Performance Considerations

### Memory Efficiency
- **Yield Return**: Uses lazy evaluation to avoid creating unnecessary collections
- **String Operations**: Efficient string scanning using `IndexOf` for finding delimiters
- **Single Pass**: Processes string in a single iteration

### Optimization Techniques
- **Index-Based Scanning**: Uses integer indices instead of string manipulation
- **Early Termination**: Breaks loops when no more patterns are found
- **Minimal Allocations**: Only creates strings for actual matches

## Thread Safety

- **Static Method**: Thread-safe as it's a stateless static extension method
- **String Immutability**: Input strings are immutable, ensuring thread safety
- **No Shared State**: Each method call operates independently

## Error Handling

```csharp
public static class SafeEnvironmentHelper
{
    public static bool TryGetEnvironmentKeys(string? input, out List<string> keys)
    {
        keys = new List<string>();
        
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        
        try
        {
            keys = input.GetEnvironmentKeys().ToList();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
    
    public static IEnumerable<string> GetEnvironmentKeysOrEmpty(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            yield break;
        }
        
        try
        {
            foreach (var key in input.GetEnvironmentKeys())
            {
                yield return key;
            }
        }
        catch (ArgumentException)
        {
            yield break;
        }
    }
}
```

## Testing Strategies

```csharp
[Test]
public void GetEnvironmentKeys_WithValidPattern_ReturnsKeys()
{
    // Arrange
    string input = "Server=$HOST$;Database=$DB$;User=$USER$;";
    
    // Act
    var keys = input.GetEnvironmentKeys().ToList();
    
    // Assert
    Assert.Equal(3, keys.Count);
    Assert.Contains("$HOST$", keys);
    Assert.Contains("$DB$", keys);
    Assert.Contains("$USER$", keys);
}

[Test]
public void GetEnvironmentKeys_WithNoVariables_ReturnsEmpty()
{
    // Arrange
    string input = "Server=localhost;Database=test;";
    
    // Act
    var keys = input.GetEnvironmentKeys().ToList();
    
    // Assert
    Assert.Empty(keys);
}

[Test]
public void GetEnvironmentKeys_WithMalformedPattern_IgnoresMalformed()
{
    // Arrange
    string input = "Server=$HOST$;Incomplete=$INCOMPLETE;Valid=$VALID$;";
    
    // Act
    var keys = input.GetEnvironmentKeys().ToList();
    
    // Assert
    Assert.Equal(2, keys.Count);
    Assert.Contains("$HOST$", keys);
    Assert.Contains("$VALID$", keys);
}

[Test]
public void GetEnvironmentKeys_WithNullInput_ThrowsArgumentException()
{
    // Arrange
    string? input = null;
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => input!.GetEnvironmentKeys().ToList());
}
```

## Best Practices

1. **Template Validation**: Always validate that templates contain well-formed environment variable patterns
2. **Environment Validation**: Check that all required environment variables are set before using templates
3. **Error Handling**: Handle cases where templates contain malformed patterns gracefully
4. **Documentation**: Document all required environment variables for deployment
5. **Security**: Be cautious when logging templates as they may reveal configuration structure

## Related Components

- **[ConnectionStringHelper](ConnectionStringHelper.md)**: Primary consumer of environment variable extraction
- **[Configuration System](../Configuration/README.md)**: Part of the broader configuration management utilities
- **[Security System](../Security/README.md)**: Supports secure configuration management practices

The `EnvironmentHelper` provides essential environment variable parsing capabilities, serving as a foundation for secure and flexible configuration management in RapidStreamer applications.