# VictusControl Agent Rules

## Global Rules

- Work only in this repository. Preserve user changes and keep diffs scoped.
- Search with `rg` before reading files; do not load whole directories by default.
- Read `AI_WORKING_STATE.md`, then the nearest path `AGENTS.md` and one relevant context pack.
- References under `D:\Projects\Workspace\references\` are read-only. Use `docs/reference-index.md` and `REFERENCE_POLICY.md` before reference research.
- Do not stage, commit, push, publish, sign, or create release artifacts unless explicitly requested.
- Use `apply_patch` for manual edits. Run focused verification, then solution build/tests when warranted.

## HP Safety

- Never infer support across HP models, BIOS versions, or thermal-policy generations.
- Normal/user-facing fan control is NO-GO. Do not add fan sliders, toggles, curves, pulse buttons, background writes, EC/PawnIO fallbacks, or generic fan-control APIs.
- `DeviceValidatedInputLength` stays null until separately proven on the exact device.
- Do not execute fan experiments or invoke `hpqBIOSInt*` without an explicit user request naming that operation.
- Developer SetFanMax pulse/hold paths remain CLI-only, explicitly gated, four-byte, no-retry, and restore-protected.
- SetFanLevel research remains non-executable; dry-run/preflight evidence does not authorize writes.

## Context Map

| Task | Read next |
| --- | --- |
| HP fan research | `app/Hardware/Hp/AGENTS.md`, `docs/context-packs/fan-research.md` |
| Telemetry | `app/Hardware/Hp/AGENTS.md`, `docs/context-packs/telemetry.md` |
| UI | `app/UI/AGENTS.md`, `docs/context-packs/ui.md` |
| Tests | `tests/AGENTS.md`, then the matching domain pack |
| Packaging | `docs/context-packs/packaging.md` |
| Identity/branding | `docs/context-packs/identity.md` |

Use `tools/context/measure.ps1` before expanding a large selection and `tools/context/pack.ps1` to materialize only a named pack. Historical evidence stays in `docs/`; do not use `SESSION_STATE.md` as default context.
