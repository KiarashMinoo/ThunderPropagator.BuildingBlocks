# SystemDrives

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **SystemDrives** area groups 2 documented types, including `SystemDriveMetrics`, `ISystemDriveMetricsClient`. It provides the contracts and implementation used by this part of ThunderPropagator.BuildingBlocks.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `SystemDriveMetrics.cs` | `SystemDriveMetrics` | 23 | Defines SystemDriveMetrics and its related behavior. |
| `SystemDriveMetricsClient.cs` | `ISystemDriveMetricsClient`, `SystemDriveMetricsClient` | 18 | Defines ISystemDriveMetricsClient, SystemDriveMetricsClient and its related behavior. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`SystemDriveMetrics`](#systemdrivemetrics) | record | Represents the SystemDriveMetrics record. | — | `Used` |
| [`ISystemDriveMetricsClient`](#isystemdrivemetricsclient) | interface | Represents the ISystemDriveMetricsClient interface. | `IMetricsClient<SystemDriveMetrics[]>;` | `GetMetricsAsync(…)` |

### SystemDriveMetrics

- **Kind:** record
- **Namespace:** `ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `Used`
- **Summary:** Represents the SystemDriveMetrics record.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve SystemDriveMetrics from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### ISystemDriveMetricsClient

- **Kind:** interface
- **Namespace:** `ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives`
- **Inherits/implements:** `IMetricsClient<SystemDriveMetrics[]>;`
- **Attributes:** None detected
- **Key members:** `GetMetricsAsync(…)`
- **Summary:** Represents the ISystemDriveMetricsClient interface.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve ISystemDriveMetricsClient from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

## Package Dependencies

| Package | Version | Description | Links |
|---|---|---|---|
| `Apache.NMS.ActiveMQ` | `2.2.0` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Apache.NMS.ActiveMQ) |
| `Ardalis.GuardClauses` | `5.0.0` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Ardalis.GuardClauses) |
| `CaseConverter` | `2.0.1` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/CaseConverter) |
| `JetBrains.Annotations` | `2026.2.0` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/JetBrains.Annotations) |
| `Microsoft.Diagnostics.Tracing.TraceEvent` | `3.2.5` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent) |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.*` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions) |
| `Microsoft.Extensions.Diagnostics.HealthChecks` | `10.*` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.HealthChecks) |
| `Newtonsoft.Json` | `13.0.4` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Newtonsoft.Json) |
| `SharpZipLib` | `1.4.2` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/SharpZipLib) |
| `System.IdentityModel.Tokens.Jwt` | `8.19.2` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt) |

## Diagrams

### Component overview

```mermaid
graph TD
  Current["SystemDrives"]
  Current --> T0["SystemDriveMetrics"]
  Current --> T1["ISystemDriveMetricsClient"]
```

The diagram shows the direct components documented by the **SystemDrives** area.

## Examples

Start with `SystemDriveMetrics` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Parent area](../README.md)
- [Battery](../Battery/README.md)
- [Cpu](../Cpu/README.md)
- [Disk](../Disk/README.md)
- [Gpu](../Gpu/README.md)
- [Memory](../Memory/README.md)

[↑ Back to top](#contents)
