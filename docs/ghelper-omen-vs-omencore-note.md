# ghelper-omen vs omencore HP Design Note

## 1. HP WMI structure

- `ghelper-omen`: closest to VictusX because it keeps HP/Omen WMI code inside the G-Helper-shaped `app/Omen/` layer. `HpWmiBios.cs` and `IHpWmiBios.cs` are the clearest references for HP `root\wmi`, `hpqBIntM`, `hpqBDataIn`, and command naming.
- `omencore`: has similar HP WMI concepts under `src/OmenCoreApp/Hardware/`, but with a larger app/service architecture around it.

## 2. Capability detection approach

- `ghelper-omen`: practical and close to the existing app shape, with `CapabilityDetectionService.cs`, `DeviceCapabilities.cs`, and `ModelCapabilityDatabase.cs`.
- `omencore`: more formal and layered, with capability models, hardware bring-up, system-info checks, and service boundaries.

## 3. Safety/error handling style

- `ghelper-omen`: direct, pragmatic, and easier to port incrementally into VictusX, but some paths are close to write-capable hardware behavior.
- `omencore`: stronger safety posture, richer diagnostics, fallback states, and more explicit runtime coordination.

## 4. UI/hardware separation

- `ghelper-omen`: partial separation; useful because it fits the imported G-Helper/VictusX structure.
- `omencore`: cleaner separation through hardware/services/UI projects and service abstractions, but too large to map directly onto VictusX right now.

## 5. What VictusX should learn from ghelper-omen

- Use its HP WMI class/method naming and command catalog as the primary near-term command reference.
- Keep the first HP steps small and compatible with the existing G-Helper-style codebase.

## 6. What VictusX should learn from omencore

- Prefer explicit capability states, diagnostics, conflict checks, and service boundaries before any hardware writes.
- Treat uncertain hardware behavior as unsupported until proven safely.

## 7. Guide repo for the next HP WMI read-only invocation step

Use `ghelper-omen` as the primary guide for the next HP WMI read-only invocation step, especially `app/Omen/HpWmiBios.cs`. Use `omencore` only as a safety and architecture cross-check.
