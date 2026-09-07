# HP Performance Read-Only Command Investigation

## Scope

This is a documentation-only investigation of performance and power status. It does not authorize WMI invocation, performance-mode control, GPU control, power-limit changes, fan writes, EC access, or `0x37`.

Reference commits reviewed: ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4`, omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b`, OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81`, and OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`.

## 1. Commands Found

| Command | Reference role | Direction/risk |
| --- | --- | --- |
| `0x28` SystemDesignData | Capability/defaults block: thermal-policy version, factory adapter rating, default PL4, and feature declarations | Read-only; already proven and decoded in VictusX. |
| `0x21` GpuGetPower | GPU power-policy configuration status | Read-shaped candidate; not a generic performance-mode getter. |
| `0x23` temperature selector | CPU/GPU or board sensor temperature | Read-shaped telemetry; not a performance-mode status command. |
| `0x1A` SetFanMode | Thermal/performance policy selection | Write/control. |
| `0x22` SetGpuPower | Custom TGP, PPAB, D-state, and related GPU power settings | Write/control. |
| `0x29` power-limit setters | Concurrent TDP and CPU PL1/PL2/PL4 changes | Write/control with direct thermal/power risk. |
| `0x37` multiplexed control family | References use it for two-byte limits, LoadLine, and IccMax; some V2 code also names it a fan-level read | Ambiguous and blocked. |

No reviewed reference exposes a direct, reliable WMI getter for the *currently active* performance/fan mode. Their `GetSupportedPerformanceModes` helpers derive possible modes from `SystemDesignData`; they do not read the active mode.

## 2. Read-Only / Status Evidence

### SystemDesignData (`0x28`)

The references use default command `0x20008`, a four-zero-byte input, and 128-byte output. VictusX already has a successful elevated manual result and safe decoder. It supplies capability/default facts, not current performance mode.

### GpuGetPower (`0x21`)

ghelper-omen, omencore, and OmenXHub expose a getter paired with GPU power writes:

- Default command `0x20008`
- Command type `0x21`
- Input: four zero bytes
- Output: four bytes through `hpqBIOSInt4`

References interpret the first three bytes as custom-TGP enabled, PPAB level/enabled, and D-state. OmenXHub labels byte four as a GPU slowdown-temperature field; other references leave it outside their public result. Those meanings are reference interpretations, so a future VictusX decoder would need to preserve unknown bytes and avoid inferring wattage, performance tier, or applied power limits.

### Temperature (`0x23`)

ghelper-omen and omencore use four-byte selector inputs such as `01-00-00-00` for CPU and `02-00-00-00` for GPU, with four-byte output. OmenSuperHub and OmenXHub also treat `0x23` as a sensor query. This is ordinary telemetry, not evidence of active performance mode or a substitute for `0x21`.

## 3. Write / Control Commands

- `0x1A` changes the thermal or fan policy. Its payload shape and mode meanings differ by thermal-policy generation.
- `0x22` changes custom TGP, PPAB, D-state, and related GPU policy fields.
- `0x29` changes concurrent TDP and CPU power limits. References describe direct watt/power-limit writes.
- `0x37` is used for two-byte power-limit, LoadLine, and IccMax writes. Its reuse in V2 fan-related code makes command-family assumptions unsafe.

None may be enabled, retried, restored, or surfaced in UI.

## 4. Power-Limit And Thermal-Risk Commands

`0x22`, `0x29`, and write-shaped `0x37` operations can affect GPU boost behavior, CPU limits, current limits, thermals, battery draw, and firmware policy. A successful return code does not prove a safe applied state. `0x1A` is also thermal-risk because it changes the state that governs fan and performance behavior.

The read-shaped `0x37` LoadLine example in hub references does not reduce this risk: the command type is multiplexed, its selector semantics are unvalidated on this V1 Victus, and this project explicitly keeps `0x37` blocked.

## 5. Device-Specific Risks

This HP Victus is `16-s0035nt` / `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1, and has only proven the read-only path. Its decoded SystemDesignData reports a 280 W factory adapter rating and a 30 W default CPU-with-GPU limit, but factory defaults and capability bytes do not authorize power changes.

No reviewed reference supplies an exact-device `0x21` result, active-performance-mode readback, or proof that reference GPU-power flags correspond to a stable operating mode on this BIOS. `0x21` output could be unsupported, incomplete, or have board-specific semantics. It must never be treated as a wattage meter or as permission to use `0x22`.

## 6. Candidate Decision

`GpuGetPower` / `0x21` is the only performance-adjacent command that is sufficiently read-shaped to investigate as a **future raw-first read-only probe candidate**. It is not approved for invocation or report wiring yet.

Before preparation, VictusX needs a focused plan that preserves all four output bytes, reports only reference-backed flags, summarizes unknown fields, uses the existing explicit HP read-only gates, and has no paired write path. There is no safe active-performance-mode command ready for preparation.

## 7. Still Forbidden

Performance-mode writes, GPU power writes, CPU/PL power-limit writes, `0x37`, `SetFanMode`, `SetFanLevel`, `SetFanMax`, EC access, BIOS writes, hardware writes, retries, polling control, and any control UI remain forbidden.

## 8. Recommended Next Safe Task

Create a documentation-only `GpuGetPower` / `0x21` raw-read probe plan. It should validate the command's reference evidence, exact safe gates, four-byte raw-only decoder expectations, report fields, and manual verification criteria before any code preparation or WMI invocation is considered.

## Files Reviewed

- ghelper-omen `app/Omen/HpWmiBios.cs`
- omencore `src/OmenCoreApp/Hardware/HpWmiBios.cs`
- omencore `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`
- OmenSuperHub `OmenHardware.cs`
- OmenXHub `OmenHardware.cs`
