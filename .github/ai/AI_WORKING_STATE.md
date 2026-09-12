# AI Working State

## Core

- VictusXHub: .NET 10 Windows utility. Exact scope: HP Victus 16-s0035nt, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, Thermal Policy V1. Never generalize HP support across models, BIOS, or policy generations.
- Fan Control is **NO-GO**; `DeviceValidatedInputLength` remains `null`; `FanGetLevel` is raw-only, never RPM or percent. Root `AGENTS.md` is the primary safety layer.
- Normal HP startup may issue only read-only SystemDesignData and FanGetCount. Opening Diagnostic adds no HP WMI request. Non-HP behavior is unchanged.

## Current Capability State

- Proven read-only: HP identity, CPU load, battery/AC, refresh rate, NVIDIA GPU temperature when available, CPU `Core (Tctl/Tdie)` with usable PawnIO access, SystemDesignData/Thermal Policy, and Fan Count.
- CPU temperature fails closed to Unavailable without elevated PawnIO access; do not add elevation or low-level fallbacks.
- Unavailable: Fan RPM, Performance Mode, current GPU mode, keyboard-lighting state/levels, and numeric charge limit. Capability is not current-state proof.

## Routing

- HP telemetry/fan: `docs/context-packs/telemetry.md`, `docs/context-packs/fan-research.md`, and `docs/hp-temperature-fan-rpm-telemetry.md`.
- Packaging/release: `docs/context-packs/packaging.md` and `docs/preview-release-current-blockers.md`; publishing is blocked pending clean-machine validation.
- UI/identity: `docs/context-packs/ui.md` or `docs/context-packs/identity.md`. Historical evidence is demand-loaded.
