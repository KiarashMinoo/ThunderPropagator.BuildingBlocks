# Attributes

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Serialization and Contracts](#serialization-and-contracts)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **Attributes** area groups 3 documented types, including `IgnoreMemberAttribute`, `JsonSerializationAttribute`, `SensitiveDataAttribute`. It provides the contracts and implementation used by this part of ThunderPropagator.BuildingBlocks.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `IgnoreMemberAttribute.cs` | `IgnoreMemberAttribute` | 9 | Defines IgnoreMemberAttribute and its related behavior. |
| `JsonSerializationAttribute.cs` | `JsonSerializationAttribute` | 12 | Defines JsonSerializationAttribute and its related behavior. |
| `SensitiveDataAttribute.cs` | `SensitiveDataAttribute` | 15 | Defines SensitiveDataAttribute and its related behavior. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`IgnoreMemberAttribute`](#ignorememberattribute) | class | Represents the IgnoreMemberAttribute class. | `Attribute;` | — |
| [`JsonSerializationAttribute`](#jsonserializationattribute) | class | Represents the JsonSerializationAttribute class. | `Attribute` | `CamelCase` |
| [`SensitiveDataAttribute`](#sensitivedataattribute) | class | Represents the SensitiveDataAttribute class. | `Attribute;` | — |

### IgnoreMemberAttribute

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Attributes`
- **Inherits/implements:** `Attribute;`
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the IgnoreMemberAttribute class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve IgnoreMemberAttribute from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### JsonSerializationAttribute

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Attributes`
- **Inherits/implements:** `Attribute`
- **Attributes:** None detected
- **Key members:** `CamelCase`
- **Summary:** Represents the JsonSerializationAttribute class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve JsonSerializationAttribute from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### SensitiveDataAttribute

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Attributes`
- **Inherits/implements:** `Attribute;`
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the SensitiveDataAttribute class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve SensitiveDataAttribute from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

## Serialization and Contracts

Serialization behavior is part of the public wire or persistence contract in this area. Preserve field names, ordering rules, content negotiation, and backward-compatibility expectations when changing these types.

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
  Current["Attributes"]
  Current --> T0["IgnoreMemberAttribute"]
  Current --> T1["JsonSerializationAttribute"]
  Current --> T2["SensitiveDataAttribute"]
```

The diagram shows the direct components documented by the **Attributes** area.

## Examples

Start with `IgnoreMemberAttribute` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Parent area](../README.md)
- [Certificate](../Certificate/README.md)
- [ChangeTrackingItems](../ChangeTrackingItems/README.md)
- [Ciphering](../Ciphering/README.md)
- [Collections](../Collections/README.md)
- [CorrelationId](../CorrelationId/README.md)
- [Enums](../Enums/README.md)
- [Helpers](../Helpers/README.md)
- [Identity](../Identity/README.md)
- [Objects](../Objects/README.md)
- [Serializations](../Serializations/README.md)

[↑ Back to top](#contents)
