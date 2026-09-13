# CLAUDE.md

<!-- Humans: keep this file lean — layer-specific implementation patterns live in .claude/rules/, not here. -->

## Commands

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet test --filter "FullyQualifiedName~<Name>"
dotnet pack -c Release -o artifacts/pkg
dotnet run -c Release --filter "*Benchmark*"   # benchmarks, from the unit-test project
```

## Architecture

Two layers, one-way dependency (architecture-test enforced):

- **Application** — the reusable building blocks. No dependency on Infrastructure.
- **Infrastructure** — system/platform monitoring, health checks. Depends on Application only.

Per-layer implementation patterns: `.claude/rules/application-patterns.md`, `.claude/rules/infrastructure-patterns.md`.

## Conventions

- **Telemetry** — wrap every non-trivial operation in an activity, guarded by a listener check, named `{ClassName}_{MethodName}`:
  ```csharp
  using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(ClassName_MethodName, ActivityKind.Internal) : null;
  activity?.SetTag("key", value);
  ```

- Private fields `_camelCase`.
- Platform names: mixed inner-case, not all-caps acronyms.
- Guard-clause library for argument validation, caller-expression attribute for messages.
- XML docs required on all public API — build fails without them.
- Warnings are errors, two narrow explicit suppressions.
- Sealed in Release, non-sealed in Debug.
- Block-scoped namespaces; no primary constructors; no expression-bodied methods/constructors (accessors OK).

## Adding a Feature

- **New metric**: record → typed client interface → per-platform providers → register in monitor's DI extension → add to metrics aggregate → wire into collector → document.
- **New helper**: static extension-method class, caller-expression guard validation, XML docs, unit tests.
- **New serialization format**: all six variants (string/bytes/base64 × serialize/deserialize), each wrapped in telemetry.

## Build & Versioning

Version/TFMs centralized; CI bumps per branch — never hand-edit during feature work. Package versions centrally managed. Debug builds get a distinguishing package-id suffix. Preview language features: test projects only.

CI: beta channel bumps + publishes a prerelease on every push; release channel finalizes the version and publishes a stable release.
