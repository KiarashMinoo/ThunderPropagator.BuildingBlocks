# RFC-48 — FeederMessage Envelope/Payload Split

**Status**: Proposed  
**Issue**: [#48](https://github.com/KiarashMinoo/ThunderPropagator.BuildingBlocks/issues/48)  
**Parent**: [#15](https://github.com/KiarashMinoo/ThunderPropagator.BuildingBlocks/issues/15)

## Contents

- [Motivation](#motivation)
- [Current State](#current-state)
- [Proposed Design](#proposed-design)
  - [FeederMessageEnvelope](#feedermessageenvelope)
  - [FeederMessagePayload](#feedermessagepayload)
  - [Updated FeederMessage](#updated-feedermessage)
- [Wire Format](#wire-format)
- [Migration Path](#migration-path)
- [Alternatives Considered](#alternatives-considered)
- [Acceptance Criteria](#acceptance-criteria)

---

## Motivation

`FeederMessage` is simultaneously a domain object and a serialization DTO. Its internal `Dictionary<string, object?>` is the single backing store for both infrastructure-level **protocol fields** (`CorrelationId`, `HashKey`, `CastType`, `IsDeleted`) and application-level **user fields** (any property defined in a concrete subclass). This coupling causes three concrete problems:

1. **Renaming a property is a breaking wire change.** Property names become dictionary keys via `[CallerMemberName]`. Renaming `OrderId` to `Id` changes the emitted key on the wire, silently breaking any receiver that was reading `OrderId`.

2. **Type information is lost on deserialization.** The value type is `object?`. When a `FeederMessage` is round-tripped through any JSON serializer, numeric values resurface as `long` (not `int`), decimals may resurface as `double`, and custom value types require the serializer to know the CLR type ahead of time. There is no per-key type manifest in the current format.

3. **No boundary between protocol and user fields.** `CorrelationId` and `OrderId` live in the same flat dictionary with no structural distinction. This makes it impossible for infrastructure code (routers, brokers, correlation trackers) to read protocol fields without inspecting keys by name, and impossible for schema validation to restrict which keys users may add.

---

## Current State

```
FeederMessage
  _dictionary : Dictionary<string, object?>
    "CorrelationId" → "req-abc-123"   ← protocol field
    "HashKey"       → 42              ← protocol field
    "CastType"      → CastType.Multicast
    "IsDeleted"     → false
    "OrderId"       → Guid(...)       ← user field
    "Amount"        → 99.99m          ← user field
```

All keys in the same flat store. Protocol fields are indistinguishable from user fields at the storage level.

---

## Proposed Design

Introduce three types in `ThunderPropagator.BuildingBlocks.Application`:

```
FeederMessageEnvelope   (protocol fields — strongly typed POCO)
FeederMessagePayload    (user-defined fields — dictionary-backed)
FeederMessage           (inherits Envelope, holds Payload internally)
```

### FeederMessageEnvelope

A **concrete, non-abstract** POCO whose properties map 1:1 to protocol fields. It has no dictionary backing — each field is a real CLR property. This guarantees:

- Stable wire keys that do not change when `FeederMessage` subclasses are modified.
- Direct deserialization by any standard JSON serializer with no type ambiguity.
- Accessible by infrastructure code without depending on concrete message types.

```csharp
/// <summary>
/// The immutable, infrastructure-managed header section of a <see cref="FeederMessage"/>.
/// Contains routing and correlation fields; never contains user-defined payload.
/// </summary>
public class FeederMessageEnvelope
{
    /// <summary>Gets or sets the correlation identifier for distributed tracing.</summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the internal routing hash key.</summary>
    public int? HashKey { get; set; }

    /// <summary>Gets or sets whether the message targets one or all subscribers.</summary>
    public CastType CastType { get; set; } = CastType.Multicast;

    /// <summary>Gets or sets the soft-delete flag.</summary>
    public bool IsDeleted { get; set; }
}
```

### FeederMessagePayload

A **thin dictionary wrapper** that exposes the same `SetValue` / `GetValue` / `GetValueOrNull` / `GetValueOrDefault` helpers that `FeederMessage` currently provides. Concrete message subclasses will inherit from `FeederMessage`, which exposes these helpers; `FeederMessagePayload` acts as the isolated backing store for the user-defined section.

```csharp
/// <summary>
/// The user-defined field store for a <see cref="FeederMessage"/>.
/// Keys are property names (supplied by <see cref="CallerMemberNameAttribute"/>);
/// values are application-level objects.
/// </summary>
public sealed class FeederMessagePayload : IDictionary<string, object?>, IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _store = [];

    // All IDictionary / IReadOnlyDictionary members delegate to _store.
    // SetValue / GetValue / GetValueOrNull / GetValueOrDefault are internal helpers
    // called by FeederMessage's protected accessors — not part of the public payload API.

    internal void SetValue(object? value, string key) => _store[key] = value;
    internal T? GetValueOrNull<T>(string key) => _store.TryGetValue(key, out var v) && v is T t ? t : default;
    internal T GetValueOrDefault<T>(T @default, string key) => GetValueOrNull<T>(key) ?? @default;
    internal T GetValue<T>(string key) => (T)_store[key]!;

    internal void Clear() => _store.Clear();
    internal int Count => _store.Count;

    // ... IDictionary / IReadOnlyDictionary delegation omitted for brevity
}
```

### Updated FeederMessage

`FeederMessage` inherits `FeederMessageEnvelope` (gains protocol properties directly as CLR properties, no `[CallerMemberName]` involved) and holds a `FeederMessagePayload` instance for user fields. The protected accessor helpers (`SetValue`, `GetValue`, etc.) delegate to the payload.

```csharp
[JsonSerialization(CamelCase = false)]
public abstract class FeederMessage : DisposableObject,
    ICorrelationIdSupport,
    ICloneable,
    ICloneable<IDictionary<string, object?>>
{
    [IgnoreMember] private readonly FeederMessagePayload _payload = new();

    // ── Protocol properties (from envelope base class, stored as CLR fields) ──

    public string CorrelationId { get; set; } = string.Empty;

    public int? HashKey { get; set; }

    public CastType CastType { get; set; } = CastType.Multicast;

    public bool IsDeleted { get; set; }

    // ── User-field accessors (delegate to payload) ──

    protected void SetValue(object? value, [CallerMemberName] string? key = null)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _payload.SetValue(value, Guard.Against.NullOrWhiteSpace(key));
    }

    protected T GetValue<T>([CallerMemberName] string? key = null)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return _payload.GetValue<T>(Guard.Against.NullOrWhiteSpace(key));
    }

    protected T? GetValueOrNull<T>([CallerMemberName] string? key = null)
        => _payload.GetValueOrNull<T>(Guard.Against.NullOrWhiteSpace(key));

    protected T GetValueOrDefault<T>(T @default, [CallerMemberName] string? key = null)
        => _payload.GetValueOrDefault(@default, Guard.Against.NullOrWhiteSpace(key));

    // ── Payload access for serializers and infrastructure code ──

    /// <summary>The user-defined field section of this message.</summary>
    public FeederMessagePayload Payload => _payload;

    // ── ICloneable ──

    object ICloneable.Clone() => MemberwiseClone();
    IDictionary<string, object?> ICloneable<IDictionary<string, object?>>.Clone()
        => new Dictionary<string, object?>(_payload);

    // ── Reset (object pool support) ──

    public virtual bool Reset()
    {
        if (IsDisposed) return false;
        _payload.Clear();
        CorrelationId = string.Empty;
        HashKey = null;
        CastType = CastType.Multicast;
        IsDeleted = false;
        return true;
    }

    protected override void DisposeManagedResources() => _payload.Clear();
}
```

> **Note on IDictionary removal**: `FeederMessage` no longer implements `IDictionary<string, object?>` directly. Infrastructure code that needs the full dictionary should access `.Payload` explicitly, narrowing the coupling. If backward compatibility requires keeping `IDictionary<string, object?>` on `FeederMessage`, `Payload` can be exposed via that interface through explicit implementation.

---

## Wire Format

### Before (flat dictionary, no separation)

```json
{
  "CorrelationId": "req-abc-123",
  "HashKey": 42,
  "CastType": 0,
  "IsDeleted": false,
  "OrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 99.99
}
```

### After (envelope + payload sections)

```json
{
  "envelope": {
    "correlationId": "req-abc-123",
    "hashKey": 42,
    "castType": 0,
    "isDeleted": false
  },
  "payload": {
    "OrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "Amount": 99.99
  }
}
```

**Key changes:**

| Aspect | Before | After |
|--------|--------|-------|
| Protocol key casing | PascalCase (C# property name) | camelCase (standard JSON convention) |
| User-field key casing | PascalCase | PascalCase (unchanged) |
| Type disambiguation | None — all `object?` | None in payload; envelope fields are strongly typed |
| Schema validation | Not possible | Envelope section has a fixed schema |

> **Breaking change**: The wire format for any message that is currently serialized and deserialized across a network boundary will change. See [Migration Path](#migration-path).

---

## Migration Path

### Phase 1 — Non-breaking internal restructuring (v1.1)

Restructure the implementation without changing the wire format:

1. Move protocol fields (`CorrelationId`, `HashKey`, `CastType`, `IsDeleted`) out of the `Dictionary` and into real CLR properties on a new base class `FeederMessageEnvelope`. These properties are no longer dictionary-backed.
2. Introduce `FeederMessagePayload` as the backing store for user-defined fields.
3. Keep `FeederMessage` implementing `IDictionary<string, object?>` by delegating to `Payload` for dictionary operations, so all existing call sites continue to compile.
4. The **serialized JSON output remains identical** to before because the `[JsonSerialization]` attribute serializer still writes all dictionary entries into the root object, and the new CLR properties on the envelope base class are included alongside them.

This phase has **no wire format change** and is fully backward compatible.

### Phase 2 — Wire format migration (v2.0, major version bump)

Introduce the envelope/payload JSON sections described in [Wire Format](#wire-format):

1. Update the `[JsonSerialization]` serializer to write `"envelope": { ... }` and `"payload": { ... }` sections.
2. Update deserializers to read both sections.
3. Add a **compatibility reader** that accepts the flat (Phase 1) format as a fallback, enabling a rolling upgrade window where old senders talk to new receivers.
4. Provide a `FeederMessageMigrationHelper.MigrateV1ToV2(string json)` utility for consumers storing serialized messages (e.g., outboxes, audit logs).
5. Remove the compatibility reader in v2.1 once the upgrade window closes.

### Existing consumer impact

| Consumer type | Phase 1 impact | Phase 2 impact |
|---------------|---------------|---------------|
| Subclass adding typed properties | None | None — user-field keys unchanged |
| Code reading `CorrelationId` via property | None | None |
| Code reading `CorrelationId` via `message["CorrelationId"]` | Breaks in Phase 1 (key no longer in dictionary) | N/A |
| Code that serializes to a persistent store | None | Requires migration utility before upgrading receivers |
| Code that deserializes from a network peer | None | Requires coordinated upgrade (old format fallback covers the window) |

> **Action required for Phase 1**: Any code that reads protocol fields via the dictionary indexer (`message["CorrelationId"]`, `message["CastType"]`, etc.) must be updated to use the typed property instead. A Roslyn analyzer or grep for `\["CorrelationId"\]`, `\["HashKey"\]`, `\["CastType"\]`, `\["IsDeleted"\]` will identify all call sites.

---

## Alternatives Considered

### A — Keep the flat dictionary but add a key-prefix convention

Prefix all protocol fields with `_` (e.g., `_correlationId`) to distinguish them from user fields without changing the type hierarchy.

- **Pro**: Zero structural change; no migration needed.
- **Con**: Convention-based, not type-enforced. Infrastructure code still depends on string constants. Does not solve the type information loss problem.

### B — Introduce a `[ProtocolField]` attribute

Mark protocol properties with an attribute; serializers emit them into a separate section while user fields remain flat.

- **Pro**: Minimal change to `FeederMessage` itself.
- **Con**: Requires attribute inspection at serialization time (slow path or cache). The type hierarchy remains unchanged, so deserialization still needs to know the concrete type to reconstruct user fields.

### C — Full separation: `FeederMessage` carries only an `Envelope` reference, no dictionary

Remove the dictionary entirely. User-defined subclasses declare their fields as normal CLR properties backed by regular auto-properties, not the shared dictionary.

- **Pro**: Maximum type safety; serialization is trivial.
- **Con**: Eliminates the dynamic property access pattern that ThunderPropagator relies on for runtime field injection. Requires a large, coordinated breaking change across all message types in ThunderPropagator.

**Chosen approach**: The Envelope/Payload split (described in [Proposed Design](#proposed-design)) preserves the dynamic field pattern for user fields while providing type safety for protocol fields, and allows a phased migration with a stable compatibility window.

---

## Acceptance Criteria

- [ ] `FeederMessageEnvelope` concrete class defined with strongly-typed protocol properties (`CorrelationId`, `HashKey`, `CastType`, `IsDeleted`).
- [ ] `FeederMessagePayload` class defined, implementing `IDictionary<string, object?>` and `IReadOnlyDictionary<string, object?>` over a `Dictionary<string, object?>` backing store.
- [ ] `FeederMessage` inherits `FeederMessageEnvelope` and holds `FeederMessagePayload` internally; `SetValue` / `GetValue` / `GetValueOrNull` / `GetValueOrDefault` delegate to the payload.
- [ ] Phase 1 wire format is identical to the current format (no existing test breaks after Phase 1).
- [ ] `FeederMessage.Reset()` clears both the payload dictionary and resets all envelope fields to their defaults.
- [ ] All existing `FeederMessageTest` and `DelegativeFeederTests` pass after Phase 1 changes.
- [ ] Roslyn / documentation note identifies the `message["CorrelationId"]` indexer pattern as a Phase 1 break and provides a code migration hint.
- [ ] Phase 2 serialization format (envelope + payload sections) documented with a compatibility reader design and versioning strategy.
- [ ] `FeederMessageMigrationHelper` design (or stub) outlined for Phase 2.
