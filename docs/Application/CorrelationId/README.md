# CorrelationId

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Performance Notes](#performance-notes)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **CorrelationId** area groups 3 documented types, including `CorrelationIdProvider`, `CorrelationIdSupportHelper`, `ICorrelationIdSupport`. It provides the contracts and implementation used by this part of ThunderPropagator.BuildingBlocks.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `CorrelationIdProvider.cs` | `CorrelationIdProvider` | 50 | Defines CorrelationIdProvider and its related behavior. |
| `CorrelationIdSupportHelper.cs` | `CorrelationIdSupportHelper` | 31 | Defines CorrelationIdSupportHelper and its related behavior. |
| `ICorrelationIdSupport.cs` | `ICorrelationIdSupport` | 7 | Defines ICorrelationIdSupport and its related behavior. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`CorrelationIdProvider`](#correlationidprovider) | class | Represents the CorrelationIdProvider class. | — | — |
| [`CorrelationIdSupportHelper`](#correlationidsupporthelper) | class | Represents the CorrelationIdSupportHelper class. | — | — |
| [`ICorrelationIdSupport`](#icorrelationidsupport) | interface | Represents the ICorrelationIdSupport interface. | — | — |

### CorrelationIdProvider

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.CorrelationId`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the CorrelationIdProvider class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve CorrelationIdProvider from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### CorrelationIdSupportHelper

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.CorrelationId`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the CorrelationIdSupportHelper class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve CorrelationIdSupportHelper from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### ICorrelationIdSupport

- **Kind:** interface
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.CorrelationId`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the ICorrelationIdSupport interface.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve ICorrelationIdSupport from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

## Performance Notes

This area contains performance-sensitive constructs such as pooled buffers, spans, asynchronous value types, or concurrent collections. Avoid unnecessary allocations and blocking calls on streaming or message-processing paths.

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
  Current["CorrelationId"]
  Current --> T0["CorrelationIdProvider"]
  Current --> T1["CorrelationIdSupportHelper"]
  Current --> T2["ICorrelationIdSupport"]
```

The diagram shows the direct components documented by the **CorrelationId** area.

## Examples

Start with `CorrelationIdProvider` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Parent area](../README.md)
- [Attributes](../Attributes/README.md)
- [Certificate](../Certificate/README.md)
- [ChangeTrackingItems](../ChangeTrackingItems/README.md)
- [Ciphering](../Ciphering/README.md)
- [Collections](../Collections/README.md)
- [Enums](../Enums/README.md)
- [Helpers](../Helpers/README.md)
- [Identity](../Identity/README.md)
- [Objects](../Objects/README.md)
- [Serializations](../Serializations/README.md)

[↑ Back to top](#contents)
