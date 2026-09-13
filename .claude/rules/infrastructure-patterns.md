---
paths:
  - "src/ThunderPropagator.BuildingBlocks.Infrastructure/**"
---

# Infrastructure-Layer Patterns

- **Platform metric provider** — typed metrics-client interface + internal per-platform provider interface + factory selecting the provider via OS-platform runtime check. Only BCL APIs / CLI tools, never a platform-specific package. Degrade to empty/null + message when a metric can't be read.
