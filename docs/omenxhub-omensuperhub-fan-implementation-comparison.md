# OmenXHub and OmenSuperHub Fan Implementation Comparison

## Scope And Revisions

This is a source-only comparison. No reference code was copied, no hardware path was invoked, and no product behavior was changed.

- OmenXHub revision: `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`
- OmenSuperHub revision: `a6ab6988c446ee5421466097fdf60c0d521e5c81`
- VictusX target: HP Victus Gaming Laptop 16-s0035nt, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`

## Executive Summary

The follow-up [SetFanLevel ABI/units audit](set-fan-level-abi-units-evidence-audit.md) expands the comparison to all six local references. It distinguishes the Omen wrappers' two/three-byte and +128 cleaning encodings from omencore's four-byte and ghelper-omen's conflicting 128-/four-byte requests. CPU/GPU naming and mixed percent/RPM scaling are not exact-device proof; `80-80` remains uncertain and SetFanLevel first-write **NO-GO**. OmenSuperHub remains the narrower SetFanMax behavior reference, not a validated SetFanLevel source.

At the SetFanMax wire level, OmenXHub and OmenSuperHub are effectively tied. Both send command `0x20008`, command type `0x27`, through `root\wmi` class `hpqBIntM`, method `hpqBIOSInt0`, with one-byte `01` enable and `00` disable payloads.

OmenSuperHub is the closer reference for the narrow behavior to study because its user-facing Max Fan transition directly calls the BIOS-only SetFanMax on/off pair and no application-level direct-EC fan fallback was found. OmenXHub still contains the same pair, but its current main `max` configuration uses SetFanMax off followed by SetFanLevel at 100 percent, and its broader fan-level path can fall back to PawnIO direct EC writes.

Neither repository is safe to port as an end-to-end control flow. Both use one-byte SetFanMax payloads, select the first `hpqBIntM` instance, expose persistent controls, and lack VictusX's exact-device gates, one-attempt rule, fixed four-byte pulse metadata, bounded wait, `finally` restore, append-only evidence log, and manual/readback outcome classification. VictusX's existing developer pulse is already the safer implementation.

## Repo-By-Repo Inventory

### OmenXHub

Reviewed symbols and paths:

- `D:\Projects\Workspace\references\OmenXHub\OmenHardware.cs`
  - `SendOmenBiosWmi`: shared HP BIOS WMI transport with cached scope/object, serialized invocation, and return-code checking.
  - `SetMaxFanSpeedOn` / `SetMaxFanSpeedOff`: one-byte SetFanMax wrappers.
  - `GetFanLevel`: command type `0x2D`, four-byte zero input, 128-byte output; first bytes are treated as levels.
  - `GetFanSpeedRpm`: command type `0x11`, indexed four-byte input, four-byte output; not validated for the VictusX target.
  - `SetFanLevel`: command type `0x2E`, two- or three-byte payload, with a direct-EC fallback.
  - `SetFanMode`: command type `0x1A`, two-byte payload.
  - several command type `0x37` power-control operations.
- `D:\Projects\Workspace\references\OmenXHub\Services\EcFanService.cs`
  - PawnIO `LpcAcpiEc` direct EC reads and writes.
  - writes manual-control and fan target registers after WMI SetFanLevel failure.
  - up to three EC operation retries and repeated EC-ready polling.
- `D:\Projects\Workspace\references\OmenXHub\Services\TrayService.cs`
  - one-second SetFanMax-off plus SetFanLevel background writes for software fan control.
  - 30-second mode/config reassertion.
- `D:\Projects\Workspace\references\OmenXHub\Pages\FanPage.xaml.cs`
  - selectable curves and fixed-speed behavior.
- `D:\Projects\Workspace\references\OmenXHub\Services\HardwareApiService.cs`
  - remote SetFanMax on/off route; its void wrapper does not propagate WMI failure to the HTTP success response.

The SetFanMax wrappers themselves stay on HP BIOS WMI and do not call the EC fallback. The surrounding fan-control system is not similarly narrow.

### OmenSuperHub

Reviewed symbols and paths:

- `D:\Projects\Workspace\references\OmenSuperHub\OmenHardware.cs`
  - `SendOmenBiosWmi`: per-call HP BIOS WMI transport with return-code checking and disposal.
  - `SetMaxFanSpeedOn` / `SetMaxFanSpeedOff`: one-byte SetFanMax wrappers.
  - `GetFanLevel`: command type `0x2D`, four-byte zero input, 128-byte output; first bytes are treated as levels.
  - `SetFanLevel`: command type `0x2E`, two- or three-byte payload.
  - `SetFanMode`: command type `0x1A`, two-byte payload.
  - command type `0x37` power-control operations.
- `D:\Projects\Workspace\references\OmenSuperHub\Program.Menu.cs`
  - Max Fan invokes SetFanMax on and stops the curve timer.
  - Auto invokes SetFanMax off and starts the one-second fan timer.
  - a fan-speed slider calls SetFanLevel.
- `D:\Projects\Workspace\references\OmenSuperHub\Program.Config.cs`
  - persisted `max`, `auto`, and fixed-RPM states are reapplied at startup/config restore.
- `D:\Projects\Workspace\references\OmenSuperHub\Program.cs`
  - automatic mode evaluates a curve and may call SetFanLevel every second.
- `D:\Projects\Workspace\references\OmenSuperHub\FanCurveProfile.cs` and `FanCurveForm.cs`
  - editable fan-curve model and UI.

No application-level direct-EC fan write or fan-write fallback was found outside its vendored LibreHardwareMonitor tree. Its normal fan system is nevertheless persistent, timer-driven, and broader than VictusX permits.

## Third-Party Driver Search

| Repository | PawnIO / PwnIO | WinRing0 | LibreHardwareMonitor | Fan-control relevance |
| --- | --- | --- | --- | --- |
| OmenXHub | PawnIO is directly referenced and used. No `PwnIO` spelling was found. | No use was found; `EcFanService` explicitly states that its PawnIO path does not need WinRing0. | Project reference to `LibreHMLib_PawnIo`; also used for telemetry. | High risk: `EcFanService` uses PawnIO `LpcAcpiEc` for direct fan EC fallback after SetFanLevel WMI failure. |
| OmenSuperHub | PawnIO status and documentation are present; its README says PawnIO is required for CPU data. No `PwnIO` spelling was found. | No match was found. | Vendored `LibreHardwareMonitorLib` project reference used for CPU/GPU telemetry. | No PawnIO or direct-EC fan action was found in the SetFanMax/SetFanLevel application call chain. Do not infer that the vendored monitor is risk-free or needed by VictusX. |

Both repositories use HP BIOS WMI for their SetFanMax wrappers. Only OmenXHub was found to add a separate application-level direct-EC fan write fallback, and that fallback belongs to SetFanLevel rather than SetFanMax.

## Decision Matrix

| Area | OmenXHub finding | OmenSuperHub finding | VictusX current evidence | Risk | Decision |
| --- | --- | --- | --- | --- | --- |
| WMI namespace/class | `root\wmi`, `hpqBIntM` | `root\wmi`, `hpqBIntM` | `\\.\root\wmi`, `hpqBIntM` | Low as reference identity only | Wire-level match. |
| WMI method | output size 0 maps to `hpqBIOSInt0` | output size 0 maps to `hpqBIOSInt0` | fixed `hpqBIOSInt0` | Low as reference identity only | Exact method match. |
| WMI instance selection | Caches the first `hpqBIntM` object | Uses the first `hpqBIntM` object per call | Requires instance containing `ACPI\PNP0C14\0_0` | Medium | Keep VictusX's exact instance filter; do not port first-object selection. |
| Command | default `0x20008` | default `0x20008` | fixed `0x20008` | Low | Exact match. |
| Command type | `0x27` | `0x27` | fixed `0x27` | Low | Exact match. |
| SetFanMax enable/disable | one byte: `01` / `00` | one byte: `01` / `00` | developer pulse uses four bytes: `01-00-00-00` / `00-00-00-00`; one-byte comparison also produced response | High ABI uncertainty | Reference pair supports semantics, not a VictusX payload-length decision. |
| WMI result handling | Checks `rwReturnCode`; serialized cached transport; MaxFan wrapper discards the result | Checks `rwReturnCode`; MaxFan wrapper discards the result | Records attempted/succeeded/error | Medium | Study serialization and result propagation only; never port void/unconditional-success behavior. |
| FanMax readback | No explicit FanMaxGet/max-latch readback found | No explicit FanMaxGet/max-latch readback found | FanMaxGet stayed false during observed response | High | References do not close the readback gap. |
| FanGetLevel | Treats initial `0x2D` bytes as levels; also has unvalidated `0x11` RPM path | Treats initial `0x2D` bytes as levels | `0x2D` remains raw-only, not RPM/percent/control state | High | Do not port decoding or RPM assumptions without separate exact-device proof. |
| Restore/disable | Explicit off wrapper and many off transitions; no bounded `finally` restore for SetFanMax on | Auto/fixed-RPM transitions call off; persisted max remains on until another transition; no bounded `finally` restore | matching four-byte restore runs in `finally` after an enable attempt | High | Keep VictusX restore model; reference off calls are not durable restore proof. |
| Retry/payload fallback | No SetFanMax retry or alternate payload fallback. WMI cache resets for a later call after exception. EC fan-level fallback retries EC operations. | No SetFanMax retry or alternate payload fallback found | exactly one pulse attempt, no retry, no fallback | High if broader logic is copied | Preserve VictusX behavior; import neither EC fallback nor background reassertion. |
| SetFanMode | `0x1A`, two-byte writes; also automated/reasserted | `0x1A`, two-byte writes | unvalidated and blocked | High | Do not port. |
| SetFanLevel | `0x2E`, two/three bytes, curves, fixed levels, EC fallback | `0x2E`, two/three bytes, slider and curves | unvalidated and blocked | Critical | Do not port. |
| Command type `0x37` | Used for power-related writes | Used for power-related writes | ambiguous and blocked | Critical | Do not port or probe. |
| Direct EC writes | PawnIO `LpcAcpiEc` writes fan registers after WMI SetFanLevel failure | No application-level direct-EC fan write found | no EC path | Critical | OmenXHub EC path is explicitly out of scope. |
| Fan curves/background writes | one-second writes plus 30-second reassertion and fan UI/API | one-second curve writes, slider, persisted control | no curves, scheduler, UI, or background writes | Critical | Do not port. |
| Exact-device safety gates | No VictusX identity/elevation/AC/baseline/manual approval sequence | No VictusX identity/elevation/AC/baseline/manual approval sequence | exact SKU/BIOS/V1, admin, AC, baseline, explicit approvals | Critical | Keep VictusX gates unchanged. |
| Logging/classification | General logs, no append-only experiment evidence contract | General logs, no append-only experiment evidence contract | append-only records distinguish command, physical response, restore, and inconclusive readback | High | Keep VictusX evidence model. |

## Match Against Exact-Device Evidence

The common reference packet confirms that `0x20008` / `0x27` / `hpqBIntM` / `hpqBIOSInt0` is a real SetFanMax family used by both applications. It also confirms matching enable/disable values at the first byte. It does not prove that one byte is the supported input ABI for BIOS `F.31`, that one- and four-byte payloads are equivalent, or that either reference's persistent control behavior is safe on the target.

VictusX evidence is stronger for its bounded operation than either reference:

- repeated four-byte command success and physical fan response on the exact device
- observed matching restore
- explicit finding that FanMaxGet remains inconclusive
- raw-only handling for FanGetLevel
- no retries or alternate-shape fallback
- `DeviceValidatedInputLength=null`

The references conflict with VictusX's preferred developer payload only in length. The target accepted a one-byte comparison too, so the conflict is not evidence of failure; it remains unresolved ABI ambiguity.

## Safest Candidate To Study Further

OmenSuperHub is the safer primary reference for the narrow SetFanMax behavior because its Max Fan menu directly selects the BIOS-only on wrapper and its Auto transition selects the matching off wrapper, without an application-level EC fallback in that path. This is a source-study preference, not approval to copy or run it.

OmenXHub is the better secondary reference for two transport ideas: serialize access to the BIOS WMI channel and propagate `rwReturnCode` accurately. Its current MaxFan HTTP route must not be copied because it reports success after a void wrapper even when the WMI helper can return failure. Its EC fallback, timer writes, mode reassertion, and main `max` implementation are incompatible with VictusX safety requirements.

## What May Be Adapted Later

Only behavior-level ideas may be reconsidered after a separate implementation decision:

- preserve a narrowly named SetFanMax enable/restore pair
- serialize the single HP BIOS transport if concurrency evidence requires it
- surface transport return codes instead of hiding failures
- keep disable/restore metadata paired with enable metadata

VictusX already implements these ideas more safely for its developer-only four-byte pulse and bounded [Max Fan Hold command](set-fan-max-developer-hold-command.md). The hold reuses the same fixed metadata, exact-device gates, one-attempt behavior, `finally` restore, and append-only evidence model for a `10`-to-`180`-second pre-restore wait. Its [first result](set-fan-max-developer-hold-first-result.md) observed a response of about two minutes after a ten-second wait, so the wait must not be treated as direct physical-duration control. It does not port reference code or persistent reference control behavior.

## What Must Not Be Ported Yet

- one-byte payload selection or `DeviceValidatedInputLength` assignment
- persistent Max Fan mode or normal fan UI
- SetFanMode, SetFanLevel, command type `0x37`, or power writes
- fan sliders, curves, timers, startup restore of active control, APIs, or background reassertion
- OmenXHub PawnIO/direct-EC fallback or any EC register mapping
- first-`hpqBIntM` instance selection
- inferred FanGetLevel percentages/RPM or unvalidated `0x11` RPM decoding
- retries, payload fallback, or success claims without observed transport results

## Recommended Next Implementation Step

The [SetFanLevel dry-run scaffold](set-fan-level-dry-run-research-scaffold.md) now permits pure candidate serialization only: one two-byte raw-value hypothesis, explicitly not executable or validated. It does not port reference code, use a transport, select a supported ABI, or alter the catalog's forbidden SetFanLevel status. Review units, fan mapping, and restore/abort proof in documentation before considering any separate execution proposal. The bounded developer hold remains on the existing `FourByteMaxFanPulse` metadata and unchanged gates; any hardware expansion or UI exposure requires a separate proof decision.

## Final Decision

The [SetFanLevel preflight design](set-fan-level-first-write-preflight-design.md) now makes ABI, units, fan ordering, and independently justified restore/recovery hard prerequisites. Reference behavior and successful dry-run persistence do not satisfy those gates; no first-write candidate is selected and execution remains NO-GO.

- Developer-only four-byte Max Fan Pulse/Hold: operational only under separate explicit CLI gates; the first ten-second hold completed with successful enable/restore but inconclusive FanMaxGet.
- Hold duration as physical fan-duration control: not validated.
- Normal/user-facing fan control: **NO-GO**.
- No fan UI until separate evidence exists.
- `DeviceValidatedInputLength` remains unset.
