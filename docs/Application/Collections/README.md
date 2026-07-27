# Collections

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Serialization and Contracts](#serialization-and-contracts)
- [Validation and Constraints](#validation-and-constraints)
- [Performance Notes](#performance-notes)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **Collections** area groups 6 documented types, including `void`, `void`, `void`, `BindingDictionary`, `IOrderedEqualityComparer`. It provides the contracts and implementation used by this part of ThunderPropagator.BuildingBlocks.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `BindingDictionary.cs` | `void`, `void`, `void`, `BindingDictionary` | 317 | Defines void, void, void and its related behavior. |
| `GenericOrderedDictionary.cs` | `IOrderedEqualityComparer`, `GenericOrderedDictionary`, `GenericOrderedDictionaryEnumerator` | 370 | Defines IOrderedEqualityComparer, GenericOrderedDictionary, GenericOrderedDictionaryEnumerator and its related behavior. |
| `LinkedArray.cs` | `LinkedArray` | 244 | Defines LinkedArray and its related behavior. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`void`](#void) | delegate | Represents the void delegate. | — | `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot` |
| [`void`](#void) | delegate | Represents the void delegate. | — | `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot` |
| [`void`](#void) | delegate | Represents the void delegate. | — | `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot` |
| [`BindingDictionary`](#bindingdictionary) | class | Represents the BindingDictionary class. | — | `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot` |
| [`IOrderedEqualityComparer`](#iorderedequalitycomparer) | interface | Represents the IOrderedEqualityComparer interface. | `IEqualityComparer,` | — |
| [`LinkedArray`](#linkedarray) | class | Represents the LinkedArray class. | `IList<T>,` | `Empty`, `Count`, `IsReadOnly`, `GetEnumerator(…)`, `CopyTo(…)`, `ToArray(…)` |

### void

- **Kind:** delegate
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`
- **Summary:** Represents the void delegate.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve void from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### void

- **Kind:** delegate
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`
- **Summary:** Represents the void delegate.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve void from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### void

- **Kind:** delegate
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`
- **Summary:** Represents the void delegate.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve void from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### BindingDictionary

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `ConcurrentSupport`, `Keys`, `Values`, `Count`, `IsSynchronized`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`
- **Summary:** Represents the BindingDictionary class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve BindingDictionary from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### IOrderedEqualityComparer

- **Kind:** interface
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** `IEqualityComparer,`
- **Attributes:** None detected
- **Key members:** Refer to the API surface in the source package
- **Summary:** Represents the IOrderedEqualityComparer interface.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve IOrderedEqualityComparer from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

### LinkedArray

- **Kind:** class
- **Namespace:** `ThunderPropagator.BuildingBlocks.Application.Collections`
- **Inherits/implements:** `IList<T>,`
- **Attributes:** None detected
- **Key members:** `Empty`, `Count`, `IsReadOnly`, `GetEnumerator(…)`, `CopyTo(…)`, `ToArray(…)`, `ForEach(…)`, `ForEach(…)`
- **Summary:** Represents the LinkedArray class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve LinkedArray from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

## Serialization and Contracts

Serialization behavior is part of the public wire or persistence contract in this area. Preserve field names, ordering rules, content negotiation, and backward-compatibility expectations when changing these types.

## Validation and Constraints

Inputs are validated at component boundaries. Callers should provide non-null required values and handle domain or argument exceptions without retrying invalid requests unchanged.

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
  Current["Collections"]
  Current --> T0["void"]
  Current --> T1["void"]
  Current --> T2["void"]
  Current --> T3["BindingDictionary"]
  Current --> T4["IOrderedEqualityComparer"]
  Current --> T5["LinkedArray"]
```

The diagram shows the direct components documented by the **Collections** area.

## Examples

Start with `void` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Parent area](../README.md)
- [Attributes](../Attributes/README.md)
- [Certificate](../Certificate/README.md)
- [ChangeTrackingItems](../ChangeTrackingItems/README.md)
- [Ciphering](../Ciphering/README.md)
- [CorrelationId](../CorrelationId/README.md)
- [Enums](../Enums/README.md)
- [Helpers](../Helpers/README.md)
- [Identity](../Identity/README.md)
- [Objects](../Objects/README.md)
- [Serializations](../Serializations/README.md)

[↑ Back to top](#contents)
