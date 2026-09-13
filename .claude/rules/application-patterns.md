---
paths:
  - "src/ThunderPropagator.BuildingBlocks.Application/**"
---

# Application-Layer Patterns

- **Dictionary-backed message base** — concurrent-dictionary-backed message type; typed properties wrap `GetValueOrDefault<T>()`/`GetValueOrNull<T>()`/`SetValue()`:
  ```csharp
  public Guid Id
  {
      get => GetValueOrDefault(Guid.NewGuid());
      set => SetValue(value);
  }
  ```
- **Observable configuration base** — abstract config base, change-notification interfaces, properties tracked/serialized via reflection.
- **Disposable base** — override managed/unmanaged-resource hook; use the action-based wrapper for one-off cleanup instead of a bespoke class.
- **Serialization helper** — every format exposes string/bytes/base64 in both directions; wrap each call in a telemetry activity.
