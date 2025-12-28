# ThunderPropagator BuildingBlocks (Project ARC)

ThunderPropagator BuildingBlocks (Project ARC) is a comprehensive .NET library providing production-ready, reusable components for building high-performance, cloud-native applications. Targets .NET 8.0, 9.0, and 10.0 with multi-platform support (AnyCPU, x86, x64, ARM64).

---

## 📚 Documentation

**Main Documentation Hub:** [docs/README.md](docs/README.md)

### Documentation Catalog

#### Application Layer (Core Building Blocks)

| Area | Types | Files | Diagrams | Description |
|------|-------|-------|----------|-------------|
| **[BuildingBlocks.Application](docs/BuildingBlocks.Application/README.md)** | `15` | `12` | `✓` | Core abstractions (FeederMessage, ServiceConfiguration, Telemetry) |
| ├─ [Attributes](docs/BuildingBlocks.Application/Attributes/README.md) | `2` | `2` | `✓` | JSON serialization control and member ignore attributes |
| ├─ [Certificate](docs/BuildingBlocks.Application/Certificate/README.md) | `1` | `1` | `✗` | X.509 certificate handling and management |
| ├─ [ChangeTrackingItems](docs/BuildingBlocks.Application/ChangeTrackingItems/README.md) | `5` | `5` | `✗` | Property change tracking with observable patterns |
| ├─ [Ciphering](docs/BuildingBlocks.Application/Ciphering/README.md) | `3` | `3` | `✓` | AES/RSA encryption and password generation |
| ├─ [Collections](docs/BuildingBlocks.Application/Collections/README.md) | `3` | `3` | `✓` | LinkedArray, BindingDictionary, GenericOrderedDictionary |
| ├─ [CorrelationId](docs/BuildingBlocks.Application/CorrelationId/README.md) | `3` | `3` | `✗` | Correlation ID management for distributed tracing |
| ├─ [Enums](docs/BuildingBlocks.Application/Enums/README.md) | `4` | `4` | `✗` | Common enumerations (AuthenticationType, CastType, DataType) |
| ├─ [Helpers](docs/BuildingBlocks.Application/Helpers/README.md) | `18` | `18` | `✓` | Serialization, collection, string, date/time utilities |
| ├─ [Identity](docs/BuildingBlocks.Application/Identity/README.md) | `2` | `2` | `✗` | JWT identity helper utilities |
| ├─ [Objects](docs/BuildingBlocks.Application/Objects/README.md) | `5` | `5` | `✓` | Base classes (DisposableObject, EquatableObject, NotifiableObject) |
| └─ [Serializations](docs/BuildingBlocks.Application/Serializations/README.md) | `4` | `4` | `✗` | Serialization abstractions and Kafka serializer types |

#### Infrastructure Layer (System-Level Components)

| Area | Types | Files | Diagrams | Description |
|------|-------|-------|----------|-------------|
| **[BuildingBlocks.Infrastructure](docs/BuildingBlocks.Infrastructure/README.md)** | `0` | `1` | `✓` | Infrastructure layer entry point |
| ├─ [HealthChecks](docs/BuildingBlocks.Infrastructure/HealthChecks/README.md) | `0` | `0` | `✗` | ASP.NET Core health check integrations |
| ├─ [System](docs/BuildingBlocks.Infrastructure/System/README.md) | `0` | `0` | `✗` | System-level utilities and abstractions |
| │  └─ [Network](docs/BuildingBlocks.Infrastructure/System/Network/README.md) | `2` | `2` | `✗` | Network performance monitoring |
| └─ **[SystemResourceMonitor](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/README.md)** | `4` | `4` | `✓` | Cross-platform resource monitoring |
|    ├─ [Metrics](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/README.md) | `2` | `2` | `✗` | Metrics client abstractions |
|    ├─ [Battery](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/Battery/README.md) | `3` | `2` | `✗` | Battery status, charge, health (Windows/macOS/Linux) |
|    ├─ [Cpu](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/Cpu/README.md) | `4` | `4` | `✗` | CPU usage and temperature monitoring |
|    ├─ [Disk](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/Disk/README.md) | `6` | `4` | `✗` | Disk health (SMART) and I/O performance |
|    ├─ [Gpu](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/Gpu/README.md) | `3` | `2` | `✗` | GPU utilization, memory, temperature |
|    ├─ [Memory](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/Memory/README.md) | `2` | `2` | `✗` | System and process memory usage |
|    └─ [SystemDrives](docs/BuildingBlocks.Infrastructure/SystemResourceMonitor/Metrics/SystemDrives/README.md) | `2` | `2` | `✗` | System drive enumeration and space |

**Total**: `100+` types, `92` files, `25+` diagrams created

**Last generated**: December 28, 2025

---

## 📦 NuGet Installation (GitHub Packages)

ThunderPropagator packages are hosted on **GitHub Packages**: `https://nuget.pkg.github.com/KiarashMinoo/index.json`

### Configure NuGet Source

**Option 1: nuget.config (recommended)**

```xml
<!-- nuget.config -->
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/KiarashMinoo/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="github">
      <package pattern="ThunderPropagator.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

**Option 2: CLI**

```bash
dotnet nuget add source https://nuget.pkg.github.com/KiarashMinoo/index.json \
  --name github \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

### Install Packages

```bash
# Core application building blocks
dotnet add package ThunderPropagator.BuildingBlocks.Application

# Infrastructure components
dotnet add package ThunderPropagator.BuildingBlocks.Infrastructure
```

---

## 🎯 Quick Start

### FeederMessage Pattern

```csharp
using ThunderPropagator.BuildingBlocks.Application;

public class OrderMessage : FeederMessage
{
    public Guid OrderId
    {
        get => GetValueOrDefault(Guid.NewGuid());
        set => SetValue(value);
    }
    
    public decimal Amount
    {
        get => GetValueOrDefault(0m);
        set => SetValue(value);
    }
}

var order = new OrderMessage
{
    OrderId = Guid.NewGuid(),
    Amount = 99.99m,
    CorrelationId = "req-12345"
};
```

### System Resource Monitoring

```csharp
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;

services.AddSystemResourceMonitor(options =>
{
    options.EnableCpuMetrics = true;
    options.EnableMemoryMetrics = true;
    options.EnableDiskHealth = true;
    options.DefaultSamplingWindowMs = 500;
});

// Inject and use
public class MonitoringService
{
    private readonly ISystemResourceMonitor _monitor;
    
    public MonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }
    
    public async Task<SystemResourceMonitorMetrics> GetMetricsAsync()
    {
        return await _monitor.GetMetricsAsync();
    }
}
```

### Serialization Helpers

```csharp
using ThunderPropagator.BuildingBlocks.Application.Helpers;

// JSON
var json = myObject.ToJson();
var obj = json.FromJson<MyType>();

// YAML
var yaml = myObject.ToYaml();

// ProtoBuf
var bytes = myObject.ToProtoBufBytes();

// MessagePack
var base64 = myObject.ToMessagePackBase64();
```

---

## 🛠 Build & Test

```bash
# Restore dependencies
dotnet restore

# Build (Release)
dotnet build -c Release

# Run tests
dotnet test -c Release

# Package
dotnet pack -c Release -o artifacts/pkg
```

**Multi-Platform Builds**:
- Platforms: AnyCPU, x86, x64, ARM64
- Frameworks: net8.0, net9.0, net10.0

---

## 📄 Available Packages

| Package | Version | Description | Documentation |
|---------|---------|-------------|---------------|
| **ThunderPropagator.BuildingBlocks.Application** | `1.0.1-beta.*` | Core application building blocks (FeederMessage, ServiceConfiguration, Helpers, Serialization) | [docs/BuildingBlocks.Application](docs/BuildingBlocks.Application/README.md) |
| **ThunderPropagator.BuildingBlocks.Infrastructure** | `1.0.1-beta.*` | Infrastructure components (SystemResourceMonitor, HealthChecks, Network) | [docs/BuildingBlocks.Infrastructure](docs/BuildingBlocks.Infrastructure/README.md) |

---

## 📖 Key Features

### Application Layer
- **FeederMessage**: Dictionary-based message abstraction with correlation ID
- **ServiceConfiguration**: Strongly-typed configuration with change notifications
- **Telemetry**: OpenTelemetry integration (Activities, Counters, Histograms)
- **Helpers**: Comprehensive serialization (JSON, YAML, ProtoBuf, MessagePack, NetJSON, Newtonsoft.Json)
- **Collections**: LinkedArray, BindingDictionary, GenericOrderedDictionary
- **Ciphering**: AES/RSA encryption, password generation
- **Objects**: DisposableObject, EquatableObject, NotifiableObject base classes

### Infrastructure Layer
- **SystemResourceMonitor**: Cross-platform monitoring (CPU, Memory, Disk, GPU, Battery)
- **Platform Providers**: Windows/Linux/macOS with graceful degradation
- **SMART Disk Health**: Disk health monitoring via platform-specific tools
- **GPU Metrics**: nvidia-smi/rocm-smi integration
- **Battery Status**: Power management metrics
- **No External Packages**: Uses .NET BCL and CLI tools only

---

## 🏗 Architecture

The solution follows a strict two-layer architecture:

1. **Application Layer** (`ThunderPropagator.BuildingBlocks.Application`)
   - Core building blocks with **NO infrastructure dependencies**
   - Helpers, serialization, collections, base classes
   - Verified by `Tests/ArchTests/ArchitectureTests.cs`

2. **Infrastructure Layer** (`ThunderPropagator.BuildingBlocks.Infrastructure`)
   - System-level components (monitoring, health checks)
   - **Depends on Application layer**
   - Platform-specific providers with graceful degradation

**Critical Rule**: Application layer MUST NEVER depend on Infrastructure layer.

---

## 🧪 Testing

### Run All Tests

```bash
dotnet test -c Release
```

### Architecture Tests

Architecture constraints are enforced by NetArchTest.Rules in `Tests/ArchTests/`:
- Application layer has no Infrastructure dependencies
- Naming conventions
- Layer boundaries

### Unit Tests

Comprehensive unit tests with xUnit and NSubstitute in `Tests/ThunderPropagator.UnitTests/`:
- FeederMessage, ServiceConfiguration tests
- Helper method tests
- Collection tests
- SystemResourceMonitor tests

### Benchmarks

BenchmarkDotNet benchmarks for performance-critical code:
- `CollectionHelperBenchmark.cs`
- `SizeBenchmark.cs`

---

## 🚀 CI/CD Workflows

- **develop** branch → `develop-beta-ci.yml` → Increments beta version
- **release/** branch → `develop-release-ci.yml` → Creates GitHub release, strips beta suffix
- GitHub Packages feed: `https://nuget.pkg.github.com/KiarashMinoo/index.json`

---

## 📝 Contributing

1. Follow the existing code conventions (see `.github/copilot-instructions.md`)
2. Add XML documentation for all public APIs
3. Include unit tests for new features
4. Update relevant documentation in `/docs`
5. Ensure architecture tests pass

---

## 📜 License

See [LICENSE](LICENSE) for details.
