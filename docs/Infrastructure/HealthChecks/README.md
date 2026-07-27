# HealthChecks

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **HealthChecks** area groups 1 documented type, including `ActiveMQHealthCheckExtensions`. It provides the contracts and implementation used by this part of ThunderPropagator.BuildingBlocks.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `ActiveMQHealthCheck.cs` | `ActiveMQHealthCheck` | 53 | Defines ActiveMQHealthCheck and its related behavior. |
| `ActiveMQHealthCheckExtensions.cs` | `ActiveMQHealthCheckExtensions` | 21 | Defines ActiveMQHealthCheckExtensions and its related behavior. |
| `ActiveMQHealthCheckOptions.cs` | `ActiveMQHealthCheckOptions` | 15 | Defines ActiveMQHealthCheckOptions and its related behavior. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`ActiveMQHealthCheckExtensions`](#activemqhealthcheckextensions) | class | Represents the ActiveMQHealthCheckExtensions class. | — | `AddActiveMQHealthCheck(…)` |

### ActiveMQHealthCheckExtensions

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Infrastructure.HealthChecks`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `AddActiveMQHealthCheck(…)`
- **Summary:** Represents the ActiveMQHealthCheckExtensions class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve ActiveMQHealthCheckExtensions from the configured service container or construct it with its declared dependencies.
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
  Current["HealthChecks"]
  Current --> T0["ActiveMQHealthCheckExtensions"]
```

The diagram shows the direct components documented by the **HealthChecks** area.

## Examples

Start with `ActiveMQHealthCheckExtensions` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Parent area](../README.md)
- [System](../System/README.md)
- [SystemResourceMonitor](../SystemResourceMonitor/README.md)

[↑ Back to top](#contents)
