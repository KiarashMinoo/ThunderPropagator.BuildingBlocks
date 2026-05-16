# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ThunderPropagator BuildingBlocks (Project ARC) is a multi-targeted .NET library (net8.0, net9.0, net10.0) of production-ready, reusable components for cloud-native applications. It publishes two NuGet packages to GitHub Packages.

## Build Commands

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet test --filter "FullyQualifiedName~FeederMessageTest"   # single test
dotnet pack -c Release -o artifacts/pkg
```

Benchmarks run via: `dotnet run -c Release --filter "*Benchmark*"` from the unit test project.

## Architecture

Strict two-layer structure enforced by `Tests/ArchTests/ArchitectureTests.cs`:

- **Application Layer** (`src/ThunderPropagator.BuildingBlocks.Application/`): Core building blocks — zero Infrastructure dependencies. Breaking this rule will fail the arch tests.
- **Infrastructure Layer** (`src/ThunderPropagator.BuildingBlocks.Infrastructure/`): System monitoring, health checks, network. Depends on Application only.

## Key Design Patterns

**FeederMessage** — Dictionary-backed message base class (`ConcurrentDictionary` internally). Strongly-typed properties use `GetValueOrDefault<T>()` / `GetValueOrNull<T>()` / `SetValue()`. Inherit and add typed properties:
```csharp
public Guid Id
{
    get => GetValueOrDefault(Guid.NewGuid());
    set => SetValue(value);
}
```

**ServiceConfiguration** — Abstract base for config with `INotifyPropertyChanged` / `INotifyPropertyChanging`. Properties tracked and serialized via reflection with `CaseConverter` for camelCase JSON.

**DisposableObject** — Base class for all disposable types. Override `DisposeManagedResources()` or `DisposeUnmanagedResources()`. Use `AnonymousDisposable` for action-based cleanup.

**Telemetry** — Wrap all significant operations with OpenTelemetry activities:
```csharp
using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(ClassName_MethodName, ActivityKind.Internal) : null;
activity?.SetTag("key", value);
```
Naming convention: `{ClassName}_{MethodName}`.

**Platform Providers** — System monitoring pattern: define `IMetricsClient<TMetric>`, internal `IXxxProvider` with per-platform implementations, `CreatePlatformProvider()` factory using `RuntimeInformation.IsOSPlatform()`. Never use external platform-specific packages — only .NET BCL and CLI tools (e.g., nvidia-smi). Graceful degradation (null/empty + error message) when metrics unavailable.

## Code Conventions

- Internal fields: `_camelCase` with underscore prefix
- Platform name casing: `MacOs` not `MacOS`; `onAcPower` not `onACPower`
- Guard clauses via Ardalis: `Guard.Against.Null(param)` with `[CallerArgumentExpression]`
- XML docs are **required** for all public APIs (`GenerateDocumentationFile=true`; build fails without them)
- `TreatWarningsAsErrors=true` — no suppressed warnings except CS1591 and CS0067
- `sealed` classes in DEBUG builds become non-sealed for testability
- Block-scoped namespace declarations; no primary constructors; no expression-bodied methods/constructors (properties/accessors are fine)

## Serialization Helpers

All serialization helpers expose three variants (string, bytes, base64) for every format:
- JSON (`ToJson` / `FromJson`) — System.Text.Json with `[JsonSerialization]` attribute support
- NetJSON, Newtonsoft.Json, YAML (YamlDotNet), ProtoBuf (protobuf-net), MessagePack

Wrap every helper method in a telemetry activity.

## Build Configuration

- Versions centralized in `Directory.Build.props` — do not edit version manually; CI handles bumps
- Package dependencies centralized in `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`)
- Debug builds append `.Debug` to package IDs
- `EnablePreviewFeatures=true` only in test projects

## CI/CD

- `develop` branch → reusable beta workflow → increments beta version and publishes to GitHub Packages
- `release/` branch → reusable release workflow → strips beta suffix, creates GitHub Release, syncs back to develop
- Secrets required: `GH_TOKEN`, `NUGET_API_KEY`

## Adding New Features

**New metric** (Infrastructure): Create metric record → `IMetricsClient<TMetric>` interface → platform providers → register in `SystemResourceMonitorExtensions.cs` → add property to `SystemResourceMonitorMetrics.cs` → update `ISystemResourceMonitor.Collect()` → add docs in `docs/`.

**New helper** (Application): Static class with extension methods in `src/.../Helpers/` → `[CallerArgumentExpression]` for validation → XML docs → tests in `Tests/ThunderPropagator.UnitTests/`.

**New serialization format**: Implement all six variants (string/bytes/base64 × serialize/deserialize), wrap each in a telemetry activity.
