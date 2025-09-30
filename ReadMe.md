# RapidStreamer BuildingBlocks

**RapidStreamer BuildingBlocks** is a comprehensive collection of production-ready components designed for enterprise applicati#### Create or Update `nuget.config`
If you don't already have a `nuget.config` file in your project or solution directory, create one. If you do, update it to include the GitHub Packages repository.

Here's an example of what the `nuget.config` file should look like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Add the official NuGet.org source -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <!-- Add the GitHub Packages repository -->
    <add key="github" value="https://nuget.pkg.github.com/KiarashMinoo/index.json" />
  </packageSources>
  
  <!-- Package source mapping for enhanced security -->
  <packageSourceMapping>
    <packageSource key="github">
      <package pattern="RapidStreamer.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Place the `nuget.config` file in the root of your solution or project directory. This ensures that all projects in the solution can access the GitHub Packages repository.

**Authentication Note**: For authentication with GitHub Packages, configure credentials using environment variables (recommended) or user-level NuGet configuration to avoid exposing tokens in your repository. Set the `GITHUB_TOKEN` environment variable with a Personal Access Token that has `read:packages` scope.his foundational library provides **robust, reusable building blocks** for creating scalable, high-performance applications with **effortless integration**, **blazing-fast performance**, and **cloud-native architecture**.

The library supports **.NET 9** and **.NET 8**, with cross-platform compatibility across **ARM64**, **x64**, **x86**, and **AnyCPU** architectures. Available as **NuGet packages** from **GitHub Packages**: **`https://nuget.pkg.github.com/KiarashMinoo/index.json`**.

> 📚 **[Complete Documentation Catalog](docs/README.md)** - Comprehensive documentation with performance benchmarks, integration patterns, and detailed API references for all 70+ components

---

## Table of Contents

- [Overview](#overview)
- [Documentation](#documentation)
- [Key Features](#key-features)
- [Building Blocks](#building-blocks)
- [Supported Platforms](#supported-platforms)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [License](#license)

---

## Overview

RapidStreamer BuildingBlocks revolutionizes application development by providing:

- **📦 Comprehensive Components**: 70+ production-ready building blocks for common application needs
- **⚡ High Performance**: Optimized for low-latency, high-throughput scenarios
- **🌐 Cross-Platform**: Seamless operation across Windows, Linux, and macOS
- **🔧 Easy Integration**: Simple APIs with extensive documentation and examples
- **🏗️ Modular Design**: Use only what you need, scale as you grow
- **🔒 Production-Ready**: Battle-tested components with security and performance built-in

Whether you're building web applications, microservices, data processing pipelines, or distributed systems, RapidStreamer BuildingBlocks provides the foundation you need to deliver robust solutions quickly.

---

## Documentation

📚 **[Complete Documentation Catalog](docs/README.md)** - Comprehensive documentation hub with architecture guides, performance benchmarks, and integration patterns

### 🔧 Application Building Blocks
**[Application Components Documentation](docs/Application/README.md)** - 60+ core application-level components

| Component Category | Documentation | Description |
|-------------------|---------------|-------------|
| **[🏷️ Attributes](docs/Application/Attributes/README.md)** | Custom attributes & metadata | JSON serialization control, reflection management |
| **[🔐 Certificate Management](docs/Application/Certificate/README.md)** | X.509 certificate handling | Security operations, authentication, HTTPS clients |
| **[📊 Change Tracking](docs/Application/ChangeTrackingItems/README.md)** | Comprehensive change tracking | Thread-safe collections, immutable records, audit trails |
| **[🔒 Cryptography & Security](docs/Application/Ciphering/README.md)** | Complete cryptographic toolkit | AES/RSA encryption, password generation, hybrid patterns |
| **[📦 Collections](docs/Application/Collections/README.md)** | High-performance collections | Observable dictionaries, ordered collections, memory-efficient arrays |
| **[🔗 Correlation Support](docs/Application/CorrelationId/README.md)** | Request correlation | Distributed systems tracking, unique identifier generation |
| **[📋 Enumerations](docs/Application/Enums/README.md)** | Strongly-typed enums | Authentication types, data types, recovery storage options |
| **[🛠️ Helper Utilities](docs/Application/Helpers/README.md)** | Comprehensive utilities | [Serialization](docs/Application/Helpers/README.md#serialization-helpers), [configuration](docs/Application/Helpers/README.md#configuration-helpers), validation, data manipulation |
| **[👤 Identity Management](docs/Application/Identity/README.md)** | Authentication & authorization | [JWT configuration](docs/Application/Identity/README.md#jwtconfiguration), user management |
| **[🎯 Object Models](docs/Application/Objects/README.md)** | Foundational object patterns | Compressed objects, disposable bases, equatable objects |
| **[📄 Serialization](docs/Application/Serializations/README.md)** | Serialization frameworks | [JSON](docs/Application/Serializations/README.md#json-serialization), [YAML](docs/Application/Serializations/README.md#yaml-serialization), Kafka, custom types |

### 🏗️ Infrastructure Building Blocks  
**[Infrastructure Components Documentation](docs/Infrastructure/README.md)** - 13+ infrastructure and monitoring components

| Component Category | Documentation | Description |
|-------------------|---------------|-------------|
| **[🏥 Health Checks](docs/Infrastructure/HealthChecks/README.md)** | Health monitoring | ActiveMQ broker monitoring, connection validation, ASP.NET Core integration |
| **[🖥️ System Resource Monitor](docs/Infrastructure/SystemResourceMonitor/README.md)** | Resource tracking | [CPU](docs/Infrastructure/SystemResourceMonitor/README.md#cpu-performance-monitoring), [memory](docs/Infrastructure/SystemResourceMonitor/README.md#memory-performance-monitoring), [disk](docs/Infrastructure/SystemResourceMonitor/README.md#disk-performance-monitoring), network performance |
| **[🌐 System Components](docs/Infrastructure/System/README.md)** | System-level monitoring | [Network performance](docs/Infrastructure/System/README.md#network-performance-monitoring-components) with ETW, process tracking |

### Documentation Features
- **Performance Benchmarks**: BenchmarkDotNet-style performance data with microsecond precision
- **Integration Patterns**: Real-world usage examples and architectural guidance  
- **Cross-Component Navigation**: Bookmark links for precise section navigation
- **API References**: Comprehensive method documentation with examples
- **Best Practices**: Security guidelines, performance optimization tips

---

## Key Features

- **🔧 Comprehensive Component Library**: 70+ building blocks covering serialization, encryption, monitoring, collections, identity management, and more
- **⚡ High Performance**: Optimized algorithms and data structures for maximum throughput and minimal latency
- **🌐 Cross-Platform Compatibility**: Native support for ARM64, x64, x86, and AnyCPU across Windows, Linux, and macOS
- **📦 .NET Modern**: Full compatibility with .NET 9 and .NET 8 with latest language features
- **🏗️ Modular Architecture**: Granular components allowing selective usage and reduced dependencies
- **🔒 Security First**: Built-in encryption, certificate management, and secure coding practices
- **📊 Production Monitoring**: Comprehensive telemetry, health checks, and system resource monitoring
- **🚀 Developer Experience**: Extensive documentation, examples, and IntelliSense support
- **☁️ Cloud-Native Ready**: Designed for containerized and distributed environments
- **🔄 Change Tracking**: Built-in support for object change detection and state management

---

## Building Blocks

### Application Layer (60+ Components)
Detailed in **[Application Building Blocks Documentation](docs/Application/README.md)**

- **[Attributes & Serialization](docs/Application/Attributes/README.md)**: JSON control, reflection management, metadata attributes
- **[Security & Encryption](docs/Application/Ciphering/README.md)**: [RSA encryption](docs/Application/Ciphering/README.md#rsaencryptionservice), [password generation](docs/Application/Ciphering/README.md#passwordgenerator), [certificate management](docs/Application/Certificate/README.md)
- **[Collections](docs/Application/Collections/README.md)**: [High-performance dictionaries](docs/Application/Collections/README.md#bindingdictionary-tkey-tvalue), [ordered collections](docs/Application/Collections/README.md#genericordereddictionary-tkey-tvalue), [linked arrays](docs/Application/Collections/README.md#linkedarray-t)
- **[Identity & Correlation](docs/Application/Identity/README.md)**: Request correlation, [user identity management](docs/Application/Identity/README.md#basicuserconfiguration), session tracking
- **[Change Tracking](docs/Application/ChangeTrackingItems/README.md)**: Object state monitoring, property change detection, audit trails
- **[Helper Utilities](docs/Application/Helpers/README.md)**: [Reflection helpers](docs/Application/Helpers/README.md#utility-helpers), [validation utilities](docs/Application/Helpers/README.md#collection-helpers), data transformation
- **[Enumerations](docs/Application/Enums/README.md)**: Strongly-typed enumerations with rich functionality
- **[Object Models](docs/Application/Objects/README.md)**: Base classes, interfaces, and common data structures

### Infrastructure Layer (13+ Components)
Detailed in **[Infrastructure Building Blocks Documentation](docs/Infrastructure/README.md)**

- **[System Monitoring](docs/Infrastructure/SystemResourceMonitor/README.md)**: [CPU](docs/Infrastructure/SystemResourceMonitor/README.md#cpu-performance-monitoring), [memory](docs/Infrastructure/SystemResourceMonitor/README.md#memory-performance-monitoring), [disk](docs/Infrastructure/SystemResourceMonitor/README.md#disk-performance-monitoring), and [network resource tracking](docs/Infrastructure/System/README.md#network-performance-monitoring-components)
- **[Health Checks](docs/Infrastructure/HealthChecks/README.md)**: Application health monitoring and dependency validation
- **[Performance Metrics](docs/Infrastructure/README.md#performance-benchmarks)**: Real-time performance counters and analytics
- **[System Integration](docs/Infrastructure/System/README.md)**: OS-level integrations and platform-specific optimizations

---

## Supported Platforms

The building blocks support deployment across multiple platforms and architectures:

### Target Frameworks
- **.NET 9** (Latest LTS)
- **.NET 8** (Current LTS)

### Supported Architectures
- **ARM64** - Apple Silicon, ARM-based servers
- **x64** - 64-bit Intel/AMD processors  
- **x86** - 32-bit Intel/AMD processors
- **AnyCPU** - Platform-agnostic deployment

### Operating Systems
- **Windows** (Windows 10/11, Windows Server 2019/2022)
- **Linux** (Ubuntu, CentOS, Alpine, Red Hat)
- **macOS** (Intel and Apple Silicon)

Both **Debug** and **Release** configurations are available for all platforms with optimized builds for production deployment.

---

## Installation

### Step 1: Add the GitHub Packages Repository
To install the libraries as NuGet packages, you need to add the GitHub Packages repository to your NuGet configuration.

#### Using Visual Studio:
1. Open Visual Studio.
2. Go to **Tools** > **NuGet Package Manager** > **Package Manager Settings**.
3. Under **Package Sources**, click the **+** button to add a new source.
4. Enter the following details:
   - **Name**: `GitHub-KiarashMinoo`
   - **Source**: `https://nuget.pkg.github.com/KiarashMinoo/index.json`
5. Click **Update** and then **OK**.

#### Using the Command Line:
Add the NuGet source using the following command:
```bash
dotnet nuget add source --name github --source https://nuget.pkg.github.com/KiarashMinoo/index.json --username KiarashMinoo --password YOUR_GITHUB_TOKEN --store-password-in-clear-text
```

#### Create or Update `nuget.config`
If you don’t already have a `nuget.config` file in your project or solution directory, create one. If you do, update it to include the custom repository.

Here’s an example of what the `nuget.config` file should look like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Add the official NuGet.org source -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <!-- Add the custom RapidStreamer NuGet repository -->
    <add key="RapidStreamer" value="https://nuget.rapidstreamer.com/v3/index.json" />
  </packageSources>
</configuration>
```

Place the `nuget.config` file in the root of your solution or project directory. This ensures that all projects in the solution can access the custom NuGet repository.

### Step 2: Install the Building Blocks Packages

Once the repository is configured, install the packages using your preferred method:

#### Via .NET CLI:
```bash
# Install the core building blocks library
dotnet add package RapidStreamer.BuildingBlocks.Application

# Install the infrastructure components
dotnet add package RapidStreamer.BuildingBlocks.Infrastructure
```

#### Via Package Manager Console (Visual Studio):
```powershell
# Install the core building blocks library
Install-Package RapidStreamer.BuildingBlocks.Application

# Install the infrastructure components  
Install-Package RapidStreamer.BuildingBlocks.Infrastructure
```

#### Via PackageReference (in .csproj):
```xml
<PackageReference Include="RapidStreamer.BuildingBlocks.Application" Version="*" />
<PackageReference Include="RapidStreamer.BuildingBlocks.Infrastructure" Version="*" />
```

### Step 3: Verify Installation

To verify that the package sources are correctly configured, use the following command:
```bash
dotnet nuget list source
```

You should see the GitHub packages source in the output:
```text
Registered Sources:
  1.  nuget.org [Enabled]
      https://api.nuget.org/v3/index.json
  2.  github [Enabled]
      https://nuget.pkg.github.com/KiarashMinoo/index.json
```

---

## Quick Start

### 1. Basic Usage Example

```csharp
using RapidStreamer.BuildingBlocks.Application;
using RapidStreamer.BuildingBlocks.Infrastructure;

// Example: Using correlation ID for request tracking
var correlationId = CorrelationIdProvider.GetOrCreate();
Console.WriteLine($"Request ID: {correlationId}");

// Example: Using concurrent string builder for high-performance string operations
var builder = new ConcurrentStringBuilder();
await builder.AppendLineAsync("Processing request...");
var result = builder.ToString();

// Example: System resource monitoring
var monitor = new SystemResourceMonitor();
var metrics = await monitor.GetSystemMetricsAsync();
Console.WriteLine($"CPU Usage: {metrics.CpuUsage:P}");
```

> 📖 **Learn More**: [Application Components Guide](docs/Application/README.md#quick-start) | [Infrastructure Setup Guide](docs/Infrastructure/README.md#getting-started)

### 2. Configuration in Startup

```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add building blocks services
    services.AddRapidStreamerBuildingBlocks(options =>
    {
        options.EnableCorrelationId = true;
        options.EnableSystemMonitoring = true;
        options.EnableChangeTracking = true;
    });
    
    // Add health checks with building blocks
    services.AddHealthChecks()
        .AddRapidStreamerHealthChecks();
}
```

> 📖 **Learn More**: [Integration Guidelines](docs/Application/README.md#integration-guidelines) | [Health Check Setup](docs/Infrastructure/HealthChecks/README.md#health-check-setup)

### 3. Getting Started Resources

| Resource | Description | Link |
|----------|-------------|------|
| **📖 Complete Documentation** | Full component catalog and guides | **[Documentation Hub](docs/README.md)** |
| **🚀 Application Components** | Core building blocks documentation | **[Application Guide](docs/Application/README.md)** |
| **🔧 Infrastructure Components** | Monitoring and system components | **[Infrastructure Guide](docs/Infrastructure/README.md)** |
| **⚡ Performance Benchmarks** | Component performance analysis | **[Performance Data](docs/README.md#performance-characteristics)** |
| **🏗️ Architecture Patterns** | Integration patterns and best practices | **[Use Cases & Scenarios](docs/README.md#use-cases--scenarios)** |
| **💡 Examples Repository** | Sample projects and use cases | [BuildingBlocks.Examples](https://github.com/RapidStreamer/BuildingBlocks.Examples) |

### Quick Navigation by Use Case

| Use Case | Recommended Components | Documentation Links |
|----------|----------------------|-------------------|
| **Web Applications** | Identity, Correlation, Health Checks | [Web App Guide](docs/README.md#web-application-development) |
| **Microservices** | Monitoring, Health Checks, Serialization | [Microservices Guide](docs/README.md#microservices-architecture) |
| **Data Processing** | Collections, Serialization, Change Tracking | [Data Pipeline Guide](docs/README.md#data-processing-pipelines) |
| **Security & Auth** | Certificate Management, Encryption, Identity | [Security Components](docs/Application/README.md#security-integration) |

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Support & Resources

| Resource | Description | Link |
|----------|-------------|------|
| **📚 Documentation Hub** | Complete documentation catalog with performance benchmarks | **[docs/README.md](docs/README.md)** |
| **🔧 Application Guide** | 60+ application components with examples | **[docs/Application/README.md](docs/Application/README.md)** |
| **🏗️ Infrastructure Guide** | 13+ infrastructure and monitoring components | **[docs/Infrastructure/README.md](docs/Infrastructure/README.md)** |
| **⚡ Performance Data** | BenchmarkDotNet performance analysis | **[Performance Benchmarks](docs/README.md#performance-characteristics)** |
| **🏗️ Architecture Patterns** | Integration patterns and use cases | **[Use Cases & Scenarios](docs/README.md#use-cases--scenarios)** |
| **🐛 Issues** | Bug reports and feature requests | [GitHub Issues](https://github.com/KiarashMinoo/RapidStreamer.BuildingBlocks/issues) |
| **💬 Discussions** | Community support and Q&A | [GitHub Discussions](https://github.com/KiarashMinoo/RapidStreamer.BuildingBlocks/discussions) |
| **📧 Contact** | Direct support for enterprise customers | [support@rapidstreamer.com](mailto:support@rapidstreamer.com) |
| **🌐 Website** | Official RapidStreamer website | [rapidstreamer.com](https://rapidstreamer.com) |

---

© 2024 RapidStreamer. All rights reserved.