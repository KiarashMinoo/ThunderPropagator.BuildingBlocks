# CLAUDE.md

Guidance for working in this repository.

## Commands

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet test --filter "FullyQualifiedName~<Name>"
dotnet pack -c Release -o artifacts/pkg
```

Benchmarks: `dotnet run -c Release --filter "*Benchmark*"` from the unit-test project.

## Architecture

Two layers, one-way dependency, enforced by an architecture-test project:

- **Application** — the reusable building blocks themselves; zero dependency on Infrastructure.
- **Infrastructure** — system/platform monitoring and health checks; depends on Application only.

## Key patterns

- **Dictionary-backed message base** — a concurrent-dictionary-backed message type; typed properties wrap `GetValueOrDefault<T>()`/`GetValueOrNull<T>()`/`SetValue()`:
  ```csharp
  public Guid Id
  {
      get => GetValueOrDefault(Guid.NewGuid());
      set => SetValue(value);
  }
  ```
- **Observable configuration base** — abstract config base with change-notification interfaces; properties tracked/serialized via reflection.
- **Disposable base** — override a managed- or unmanaged-resource hook; use an action-based wrapper for one-off cleanup instead of a bespoke class.
- **Telemetry** — wrap every non-trivial operation in an activity, guarded by a listener check:
  ```csharp
  using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(ClassName_MethodName, ActivityKind.Internal) : null;
  activity?.SetTag("key", value);
  ```
  Name activities `{ClassName}_{MethodName}`.
- **Platform metric provider** — a typed metrics-client interface, an internal per-platform provider interface, and a factory selecting the concrete provider via an OS-platform runtime check. Never depend on a platform-specific package — only BCL APIs and command-line tools. Degrade gracefully (empty/null + message) when a metric can't be read.
- **Serialization helper** — every format exposes three encodings (string/bytes/base64) in both directions; wrap each call in a telemetry activity.

## Conventions

- Private fields: `_camelCase`.
- Platform names use mixed inner-case, not all-caps acronyms.
- Guard-clause library for argument validation, using the caller-expression attribute for messages.
- XML docs required on all public API — the build fails without them.
- Warnings are errors, with two narrow, explicit suppressions.
- Sealed in Release, non-sealed in Debug, for testability.
- Block-scoped namespaces; no primary constructors; no expression-bodied methods/constructors (accessors are fine).

## Adding a feature

- **New metric**: metric record → typed client interface → per-platform providers → register in the monitor's DI extension → add the property to the metrics aggregate → wire into the collector → document it.
- **New helper**: static extension-method class, argument validation via the caller-expression guard pattern, XML docs, unit tests.
- **New serialization format**: implement all six variants (string/bytes/base64 × serialize/deserialize), each wrapped in telemetry.

## Build & versioning

Version and target frameworks are centralized; CI bumps the version automatically per branch — never hand-edit it during feature work. Package versions are centrally managed. Debug builds get a distinguishing package-id suffix. Preview language features are enabled only in test projects.

CI publishes on two branch patterns: a beta channel that bumps and publishes a prerelease on every push, and a release channel that finalizes the version and publishes a stable release.
