# SetFanMax Model/BIOS Evidence Matrix

## Current Victus Facts

| Fact | Value | Evidence value |
| --- | --- | --- |
| Model | HP Victus Gaming Laptop 16-s0035nt | Exact device identity |
| SKU | `7Z5Z2EA#AB8` | Exact device identity |
| BIOS | `F.31` | Exact device identity |
| Thermal policy | `1` | Real read-only `SystemDesignData` decode |
| Fan count | `2` | Real read-only `FanGetCount` decode |
| Max-fan readback | `FanMaxGet` works; disabled baseline | Real read-only decode |
| Fan-level readback | `FanGetLevel` works; raw-only | Real read-only decode |

None of these read-only facts includes a SetFanMax write or chooses an input length.

## Reference Matrix

| Reference | SetFanMax length | Method / command | Surrounding flow | Restore/readback | Model/BIOS relevance |
| --- | --- | --- | --- | --- | --- |
| ghelper-omen `1694844` | `4` | `hpqBIOSInt0`, command `0x20008`, type `0x27`, output `0`; state plus three zero bytes | Direct WMI with mode/level commands and some heartbeat/reapply logic | `FanMaxGet` exists; the direct max-fan method checks the returned WMI result | Its code mentions a Victus 16-s0xxx BIOS `F.31` thermal-zone case, but does not bind that case to a SetFanMax payload validation. Indirect only. |
| omencore `b39b449` | `4` | `hpqBIOSInt0`, command `0x20008`, type `0x27`, output `0`; state plus three zero bytes | Stateful WMI fan controller, capability database, logs, readback/maintenance behavior | Has max-fan readback and controller restore paths using disable/default mode | Closest cohort: its 8BD4 Victus 16-s0xxx profile describes V1 WMI fan control, two exposed levels, and notes `7Z5Z2EA` reports. That profile is explicitly conservative and not user-verified; it records BIOS `F.30`, not this device's `F.31`, and does not validate this SetFanMax payload. |
| OmenSuperHub `a6ab698` | `1` | `hpqBIOSInt0`, command `0x20008`, type `0x27`, output `0`; state only | Direct WMI helper plus mode/level writes | One-byte on/off helpers exist; no complete guarded transaction is evident in the helper | Detects broad Victus/Pavilion families, but no discovered evidence names this 16-s0035nt, SKU, or BIOS. Generic only. |
| OmenXHub `ca84cb0` | `1` | `hpqBIOSInt0`, command `0x20008`, type `0x27`, output `0`; state only | Direct WMI helper, fan services, and optional EC fallback for failed level writes | Has max-off calls in transition paths; comments warn repeated transitions can overwhelm EC behavior | Identifies a `0x2D` Victus_S read path and broad Victus support, but no discovered SetFanMax evidence for this SKU/BIOS. Its EC fallback is outside VictusX's boundary. |

## Evidence For One Byte

- Two references use a one-byte `0x27` input with the same state byte and zero output method.
- Both treat disable as the matching one-byte `0x00` state.

This is cross-project generic evidence only. Neither source supplies a model-and-BIOS-specific SetFanMax validation for this Victus.

## Evidence For Four Bytes

- Two references use `{ state, 0, 0, 0 }` through the same apparent WMI transport.
- omencore has the closest discovered Victus 16-s0xxx / V1 / two-level cohort and cites `7Z5Z2EA` reports elsewhere in that profile.

This is still insufficient: the cohort profile is conservative, its cited BIOS is `F.30`, its verification flag is false, and its four-byte SetFanMax implementation is not documented as a test result for this exact SKU or `F.31`.

## Evidence Against Choosing Now

- The exact model/SKU/BIOS has no discovered recorded SetFanMax input-length result.
- `ThermalPolicyVersion=1`, two fans, and successful readbacks establish no write ABI.
- The closest profile includes conservative safeguards after reports of problematic V1/manual fan behavior.
- A firmware revision difference (`F.30` reference cohort versus `F.31` device) is enough to reject extrapolation.
- Reference transport agreement cannot prove a trailing-zero requirement or its absence.

## Evidence Required To Make A Choice

Only independently reviewable, device-specific evidence may set the value:

1. This exact model/SKU and BIOS, or a documented firmware-equivalence proof.
2. One successful, bounded, human-reviewed validation of either length `1` or `4`, with no alternate-length fallback.
3. Matching enable and restore/disable behavior using that same length.
4. `FanMaxGet` baseline, post-enable, and post-restore evidence showing disabled state is recovered.
5. Recorded AC power, thermal observation, elevation, recovery path, and human approval.

## Current Decision

`DeviceValidatedInputLength` remains **unset**. SetFanMax remains **NO-GO**. This matrix intentionally does not score, rank, or select either payload length.

## Recommended Next Safe Task

Keep all write code absent. Seek a reviewable device-specific capture or field report that names the model, SKU, BIOS, command type `0x27`, exact input length, and matched restore outcome; then update the missing-proof tracker only.

Related: [payload-shape decision plan](set-fan-max-payload-shape-decision-plan.md), [reference fan write flow comparison](reference-fan-write-flow-comparison.md), [omencore Victus 16-s0xxx deep dive](omencore-victus-16s-setfanmax-evidence-deep-dive.md), and [OmenSuperHub/OmenXHub one-byte deep dive](omensuperhub-omenxhub-setfanmax-one-byte-evidence-deep-dive.md). Neither deep dive validates a payload length; `DeviceValidatedInputLength` remains unset.
