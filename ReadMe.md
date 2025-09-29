# RapidStreamer BuildingBlocks

**RapidStreamer BuildingBlocks** is a comprehensive collection of production-ready components desiIf you don't already have a `nuget.config` file in your project or solution directory, create one. If you do, update it to include the GitHub Packages repository.

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

**Authentication Note**: For authentication with GitHub Packages, configure credentials using environment variables (recommended) or user-level NuGet configuration to avoid exposing tokens in your repository. Set the `GITHUB_TOKEN` environment variable with a Personal Access Token that has `read:packages` scope.te application development. This foundational library provides **robust, reusable building blocks** for creating scalable, high-performance applications with **effortless integration**, **blazing-fast performance**, and **cloud-native architecture**.

The library supports **.NET 9** and **.NET 8**, with cross-platform compatibility across **ARM64**, **x64**, **x86**, and **AnyCPU** architectures. Available as **NuGet packages** from **GitHub Packages**: **`https://nuget.pkg.github.com/KiarashMinoo/index.json`**.

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

📚 **[Complete Documentation Catalog](docs/README.md)** - Comprehensive guide to all building blocks and components

### Main Documentation Sections
- **[Application Building Blocks](docs/Application/README.md)** - Core application-level components (60+ components)
- **[Infrastructure Building Blocks](docs/Infrastructure/README.md)** - Infrastructure and monitoring components (13+ components)

Each section includes:
- Component overviews and feature lists
- Detailed API documentation
- Real-world usage examples
- Integration guidelines and best practices
- Performance considerations and troubleshooting

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
- **Serialization & Attributes**: JSON control, reflection management, metadata attributes
- **Security & Encryption**: RSA encryption, password generation, certificate management
- **Collections**: High-performance dictionaries, ordered collections, linked arrays
- **Identity & Correlation**: Request correlation, user identity management, session tracking
- **Change Tracking**: Object state monitoring, property change detection, audit trails
- **Helpers & Utilities**: Reflection helpers, validation utilities, data transformation
- **Enums & Extensions**: Strongly-typed enumerations with rich functionality
- **Objects & Models**: Base classes, interfaces, and common data structures

### Infrastructure Layer (13+ Components)  
- **System Monitoring**: CPU, memory, disk, and network resource tracking
- **Health Checks**: Application health monitoring and dependency validation
- **Performance Metrics**: Real-time performance counters and analytics
- **System Integration**: OS-level integrations and platform-specific optimizations

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

### 3. Getting Started Resources

- **[📖 Complete Documentation](docs/README.md)** - Full component catalog and guides
- **[🚀 Application Components](docs/Application/README.md)** - Core building blocks documentation  
- **[🔧 Infrastructure Components](docs/Infrastructure/README.md)** - Monitoring and system components
- **[💡 Examples Repository](https://github.com/RapidStreamer/BuildingBlocks.Examples)** - Sample projects and use cases

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Support & Resources

- **📚 [Documentation Hub](docs/README.md)** - Complete documentation catalog
- **🐛 [Issues](https://github.com/KiarashMinoo/RapidStreamer.BuildingBlocks/issues)** - Bug reports and feature requests  
- **💬 [Discussions](https://github.com/KiarashMinoo/RapidStreamer.BuildingBlocks/discussions)** - Community support and Q&A
- **📧 [Contact](mailto:support@rapidstreamer.com)** - Direct support for enterprise customers
- **🌐 [Website](https://rapidstreamer.com)** - Official RapidStreamer website

---

© 2024 RapidStreamer. All rights reserved.