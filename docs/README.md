# RapidStreamer BuildingBlocks Documentation

Welcome to the comprehensive documentation for RapidStreamer BuildingBlocks - a robust collection of reusable components designed to accelerate application development across the .NET ecosystem.

## 📚 Documentation Overview

This documentation provides detailed guidance for integrating and using RapidStreamer BuildingBlocks in your applications. Each component includes comprehensive API references, usage examples, performance characteristics, and architectural guidance.

### 🏗️ Architecture

RapidStreamer BuildingBlocks is organized into two main layers:

- **[Application BuildingBlocks](BuildingBlocks.Application/README.md)** - Core application-level components
- **[Infrastructure BuildingBlocks](BuildingBlocks.Infrastructure/README.md)** - Infrastructure and operational components

### 🔧 Key Features

- **High Performance**: Optimized implementations with telemetry and monitoring
- **Type Safety**: Strongly-typed APIs with comprehensive validation
- **Extensibility**: Attribute-based configuration and custom serialization
- **Observability**: Built-in telemetry, metrics, and health checks
- **Cross-Platform**: .NET 8.0+ support with consistent behavior

## 🚀 Quick Start

### Installation

Add RapidStreamer packages from GitHub Packages:

```bash
# Add GitHub Packages source
dotnet nuget add source https://nuget.pkg.github.com/KiarashMinoo/index.json \
  --name "RapidStreamer" \
  --username "KiarashMinoo" \
  --password "YOUR_GITHUB_TOKEN"

# Install packages
dotnet add package RapidStreamer.BuildingBlocks.Application
dotnet add package RapidStreamer.BuildingBlocks.Infrastructure
```

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application;
using RapidStreamer.BuildingBlocks.Application.Helpers;

// Create a message with correlation tracking
var message = new FeederMessage
{
    ["userId"] = "12345",
    ["action"] = "purchase",
    CorrelationId = Guid.NewGuid().ToString()
};

// Serialize with telemetry
string json = message.ToJson();

// Deserialize safely
var restored = json.FromJson<FeederMessage>();
```

## 📖 Documentation Sections

### Application Components

| Component | Description | Key Classes |
|-----------|-------------|-------------|
| **[Attributes](BuildingBlocks.Application/Attributes/README.md)** | Serialization and processing control | `JsonSerializationAttribute`, `IgnoreMemberAttribute` |
| **[Helpers](BuildingBlocks.Application/Helpers/README.md)** | Utility classes and extensions | `JsonHelper`, `StringHelper`, `CollectionHelper` |
| **[Collections](BuildingBlocks.Application/Collections/README.md)** | Specialized collection types | `ObservableList`, `LinkedArray` |
| **[Ciphering](BuildingBlocks.Application/Ciphering/README.md)** | Cryptography and security | AES/RSA encryption, password hashing |
| **[Serializations](BuildingBlocks.Application/Serializations/README.md)** | Serialization abstractions | Format-agnostic serialization interfaces |

### Infrastructure Components

| Component | Description | Key Classes |
|-----------|-------------|-------------|
| **[HealthChecks](BuildingBlocks.Infrastructure/HealthChecks/README.md)** | System health monitoring | Health check implementations |
| **[SystemResourceMonitor](BuildingBlocks.Infrastructure/SystemResourceMonitor/README.md)** | Performance tracking | CPU, memory, disk monitoring |

## 🔍 API Reference

Each component provides comprehensive API documentation including:

- **Class Diagrams**: Visual representation of type relationships
- **Sequence Diagrams**: Interaction flows and data pipelines
- **Usage Recipes**: Practical code examples
- **Performance Notes**: Optimization guidance and benchmarks
- **Configuration Options**: Attribute-based customization

## 📊 Performance & Monitoring

All components include built-in telemetry and performance monitoring:

```csharp
// Automatic activity tracking
using var activity = Telemetry.StartActivity("ProcessOrder", ActivityKind.Internal);

// Performance metrics
var counter = Telemetry.CreateCounter<int>("orders_processed");
counter.Add(1);

// Health checks
var healthCheck = new SystemHealthCheck();
var result = await healthCheck.CheckHealthAsync();
```

## 🤝 Contributing

This documentation is automatically generated from source code analysis. To contribute:

1. Add XML documentation comments to public APIs
2. Use appropriate attributes for serialization control
3. Include usage examples in method documentation
4. Run documentation generation to validate changes

## 📄 License

RapidStreamer BuildingBlocks is licensed under the MIT License. See the repository LICENSE file for details.

---

**Last generated:** December 2025  
**Framework Support:** .NET 8.0, 9.0, 10.0  
**Package Source:** GitHub Packages

[View on GitHub](https://github.com/KiarashMinoo/RapidStreamer.BuildingBlocks)</content>
<parameter name="filePath">C:\Users\Kiarash\RiderProjects\RapidStreamer.BuildingBlocks\docs\README.md
