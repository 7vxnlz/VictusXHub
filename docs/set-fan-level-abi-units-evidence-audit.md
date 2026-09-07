# SetFanLevel ABI And Units Evidence Audit

## 1. Executive Conclusion

Follow-up: the [closest-Victus deep trace](omencore-victus-16s-setfanlevel-deep-trace.md) reconstructs model selection through transport. It confirms caller-local128-byte allocation (not shared WMI expansion), separates modern +128 cleaning from legacy command2/type44 byte3 flags, and finds no high-bit insertion in omencore's ordinary V1 mapper. It also qualifies lifecycle evidence: FanService attempts shutdown restore/reset even though controller Dispose does not, and dual targets can become equal on maintenance. Low positive percentages can still round to raw zero. These are source facts, not F.31 validation; first-write NO-GO and an empty candidate set remain unchanged.

Static local-source review, 2026-09-05. **The reference implementations establish a common command identity and per-index byte convention, but do not prove the target device's accepted input length, units, safe range, physical fan mapping, or recovery contract. SetFanLevel first-write readiness remains NO-GO; no executable value is selected.**

The material conflicts are substantive:

- OmenXHub and OmenSuperHub send two bytes, or three with an optional third fan. Their cleaning path adds `128` per capability-enabled fan.
- omencore sends four bytes. ghelper-omen sends **128 bytes** in `SetFanLevel`, despite its four-byte comment, but four bytes in its countdown reassertion path.
- Callers variously divide nominal RPM by 100, scale percent against a model ceiling (often 55 on V1), or send percent directly. These are application policies, not a firmware specification.
- The closest local Victus 16-s0xxx / V1 / BIOS F.30 evidence warns about non-reactive fans after zero-level writes. It is not proof for SKU `7Z5Z2EA#AB8` / BIOS F.31.
- `128 -> 80-80` is two literal bytes of value 128 in VictusX's serializer. Its **firmware interpretation is C: uncertain**, including a potential special-bit ambiguity, not a recommended speed or safe midpoint.

No reference was executed, modified, or copied into product code. No WMI, dry-run command, or fan experiment was run. Existing SetFanMax pulse/hold evidence remains separate from SetFanLevel.

Verification: `dotnet build VictusX.sln` passed with zero errors and four recurring NU1900 audit-source warnings; `dotnet test VictusX.sln` passed 276/276 with none skipped. No application CLI path, publication, or release-artifact creation was performed. Passing tests do not establish hardware ABI or resolve the vulnerability-audit warning.

### Provenance And Search Scope

All paths below are relative to the named repository root under `D:\Projects\Workspace\references\`. Line anchors refer to these local revisions; symbols identify the relevant code when line numbers later move.

| Repository | Reviewed commit |
| --- | --- |
| OmenXHub | `ca84cb011d1d3e5850445d19e45fbea06e83a8fd` |
| OmenSuperHub | `a6ab6988c446ee5421466097fdf60c0d521e5c81` |
| omencore | `b39b44978902606aa708cc0d78bcfd87e95fd88b` |
| ghelper-omen | `1694844d2725e79a2b2065a0a1494fa1d143e3f4` |
| g-helper | `5c26f5ac970dab9e26347d80976ebf1eece91b1e` |
| ECC | `5eddf1a3ffd311423be2d4ba7d26f7209c91b033` |

Search covered SetFanLevel/FanLevel/FanGetLevel/GetFanLevel/SetFan, `0x2E`/`0x2D`, WMI identities, array construction, fan1/fan2/CPU/GPU, percent/percentage/PWM, clamp/range/100/255, fan count/type, curves, restore, timers, resume and driver/fallback terms. Follow-up reading was limited to matching wrappers, callers, transport, lifecycle and device evidence. The requested six-repository comparison required a wider focused source set than a routine edit. Negative findings concern these local revisions and reviewed call chains, not all HP firmware or every possible driver dependency.

The local `g-helper` C# sources contain no SetFanLevel/FanGetLevel/hpqBIntM/hpqBIOSInt implementation matching these searches; its inherited ASUS controls are not HP ABI evidence. The local **ECC is an agent-harness repository**, as identified by `README.md`, not an embedded-controller utility. HP command searches there yielded no relevant implementation. Neither supplies a competing HP payload or recovery rule.

## 2. Exact Command Identity

| Repository / transport source | Namespace / class | Set method | Command / type | Actual input bytes / Size |
| --- | --- | --- | --- | --- |
| OmenXHub `OmenHardware.cs:48`, `SendOmenBiosWmi`; `SetFanLevel:246` | `root\wmi` / `hpqBIntM` | `hpqBIOSInt0` | `0x20008` / `0x2E` | 2 or 3; `Size=data.Length` |
| OmenSuperHub `OmenHardware.cs:865`, `SendOmenBiosWmi`; `SetFanLevel:511` | `root\wmi` / `hpqBIntM` | `hpqBIOSInt0` | `0x20008` / `0x2E` | 2 or 3; `Size=data.Length` |
| omencore `src/OmenCoreApp/Hardware/HpWmiBios.cs:1052`, `SetFanLevel`; `SendBiosCommand:2455`, legacy transport:2599 | `root\wmi` / `hpqBIntM` | `hpqBIOSInt0` via CIM or legacy WMI | `BiosCmd.Default=0x20008` / `CMD_FAN_SET_LEVEL=0x2E` | 4; `Size=inData.Length` |
| ghelper-omen `app/Omen/HpWmiBios.cs:643`, `SetFanLevel`; `SendBiosCommand:1899`, legacy:2059 | `root\wmi` / `hpqBIntM` | `hpqBIOSInt0` via CIM or legacy WMI | `BiosCmd.Default=0x20008` / `CMD_FAN_SET_LEVEL=0x2E` | 128 in setter; 4 in `ExtendFanCountdown:2168`; actual length passed as Size |
| g-helper / ECC | No matching HP implementation found | Not established | Not established | Not established |
| VictusX `app/Hardware/Hp/HpFanLevelResearchDryRunCommand.cs:7`, record metadata | Same identity as a hypothesis | Name serialized only | Same metadata only | 2; no transport |

The Set method suffix `0` selects **zero output bytes**, not zero input bytes or proof of a two/four-byte input contract. The wrappers supply an `hpqBData` byte array and its length through the BIOS data structure. Distinguish this actual array/Size from a firmware buffer capacity, padding tolerance, and the requested readback output size; the latter do not resolve ABI validity. No fresh WMI schema inspection occurred here.

## 3. Payload ABI Comparison

`a` and `b` below are caller-supplied integers/bytes, not validated speeds. These are ordered byte fields, not one little-/big-endian 16-bit speed. There is no endianness conversion for the individual one-byte targets.

| Repository | Normal payload | Additional bytes / cleaning variant | Duplication and validation |
| --- | --- | --- | --- |
| OmenXHub | `[byte(a), byte(b)]` | Optional third = integer average `(a+b)/2`; cleaning adds 128 to each capability-enabled index before byte cast | Independent arguments supported; equal values are caller policy. Wrapper does not clamp. |
| OmenSuperHub | `[byte(a), byte(b)]` | Same optional third/average and capability-gated +128 encoding | Normal slider/curve callers send equal arguments; cleaning can supply distinct CPU/GPU settings. Wrapper does not clamp. |
| omencore | `[a,b,0,0]` | Two trailing zero bytes; no third target in this setter | Separate CPU/GPU arguments; byte type alone does not establish accepted range. |
| ghelper-omen | `[a,b,0,...,0]` (128 total) | 126 trailing zeros in setter; countdown instead builds `[a,b,0,0]` | Separate CPU/GPU arguments; comment still describes four bytes. Do not choose code or comment as target validation. |
| g-helper / ECC | None established | None established | Not applicable to HP SetFanLevel |
| VictusX dry-run | `[candidate,candidate]` only in JSON | No fallback or executable variants | Intentional serialization duplication, not a measured fan mapping |

Omen cleaning sources: `OmenXHub/OmenHardware.cs:246` and `OmenSuperHub/OmenHardware.cs:511`; `GetFanType` at OmenXHub:191 and OmenSuperHub:410 reads `0x2C`, interprets indexed fan types and capability bits. The enum names CPU/GPU/exhaust and comments describe reverse cleaning. This documents an implementation convention, **not** that BIOS F.31 supports cleaning or that every high-bit value requests it. Neither wrapper establishes safe behavior for overflow, arbitrary 128..255 inputs, missing capabilities, or invalid indexes.

### Evidence Ledger

Confidence refers to relevance/provenance, not approval to execute. A source statement can be precisely verified as code while its claimed firmware meaning remains only an inference.

| Repository | File / symbol | Device context | Command | Payload | Range | Meaning | Confidence | VictusX decision |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| OmenXHub | `OmenHardware.cs:246` / `SetFanLevel` | Generic HP; optional third/cleaning | `0x20008/0x2E` | 2/3 bytes | No wrapper clamp | Per-index arguments; +128 cleaning convention | Generic HP evidence | Candidate shape only; no execution |
| OmenXHub | `Services/TrayService.cs:671` / `RestoreConfig` fan-selection branches | Multiple model/config modes | Same wrapper | Usually equal pair | Direct percent, 100 max, or nominal RPM/100 | Mixed caller units | Implementation inference | Do not treat percent and level as identical |
| OmenSuperHub | `Program.Menu.cs:396` / fanTrackBar handlers; `OmenHardware.cs:511` | Generic HP | `0x20008/0x2E` | Equal 2-byte pair or 3 fans | 0..64 default slider, or platform-derived maximum | Displayed RPM = slider value *100 | Implementation inference | Not measured RPM or target range proof |
| omencore | `src/OmenCoreApp/Hardware/HpWmiBios.cs:1052` / `SetFanLevel` | HP WMI V1/V2 family | `0x20008/0x2E` | 4 bytes | Byte parameters; upstream policy varies | First targets logged CPU/GPU | Generic HP evidence | ABI candidate only |
| omencore | `src/OmenCoreApp/Hardware/WmiFanController.cs:212` / `MapFanPercentToWmiLevel` | V1/V2/model ceilings | Same setter | 4 bytes | Percent clamped 0..100, scaled except 100 special case | Model-dependent level, not uniform PWM | Implementation inference | Do not port scaling |
| omencore | `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs:1687`; `docs/3.6.3-BUG-REPORTS.md:583` | 16-s0xxx / 8BD4 / V1 / F.30 | Level writes and mode handoff | Zero-pair handoff implicated | Safe range unproven | Two zones; zero-level recovery hazard | Close-device evidence | Strengthens recovery blocker, not candidate approval |
| ghelper-omen | `app/Omen/HpWmiBios.cs:643,2168` / setter, countdown | Generic HP, V1/V2 | `0x20008/0x2E` | 128 versus 4 bytes | Model-scaled callers | Conflicting active input shapes | Generic HP evidence | Do not resolve by trial/fallback |
| g-helper | C# HP-symbol search | ASUS-oriented inherited shell | Not found | Not found | Unknown | No HP SetFanLevel evidence | Unknown | Exclude from ABI proof |
| ECC | `README.md`; HP-symbol search | Agent harness, not HP hardware | Not found | Not found | Unknown | No fan implementation evidence | Unknown | Exclude from ABI proof |
| VictusX | `HpFanLevelResearchDryRunRecord`; recorded persistence result | Target workflow, serialization only | Descriptive metadata | `80-80` | Parser 0..255 only | Two bytes128, no device acceptance | Implementation inference | First-write NO-GO |

No row qualifies as **Exact-device evidence** for executed SetFanLevel. Exact-device SetFanMax response and FanGetLevel raw captures concern different operations and cannot fill that gap.

## 4. Range And Unit Comparison

| Source | Application range/conversion found | Zero / maximum / sentinel evidence | Firmware conclusion |
| --- | --- | --- | --- |
| OmenSuperHub `Program.Menu.cs:397` | Slider min0; max64 if platform max unavailable, otherwise integer `platformMaxFanSpeed*1.1/100`; display multiplies by100 | Zero can be sent; no proof it means auto. Capability-gated +128 cleaning exists. | Nominal 100-RPM-step convention; not validated physical RPM/percent/PWM |
| OmenSuperHub `Program.cs:297,765` | Curve/config nominal RPM divided by100, integer division | Equal pair; startup can resend | No universal 0..100 accepted range established |
| OmenXHub `Services/TrayService.cs:516,671` | Curve/smart paths divide by100; fixed percent passes integer directly; max passes100 | Different modes can give different meanings to the same integer | Conflicting caller conventions, not a unit specification |
| OmenXHub `Services/EcFanService.cs:78` | Fallback clamps original arguments to0..100 | Separate EC percent targets after manual-enable register writes | Clamp applies to EC path, NOT BIOS ABI; fallback can reinterpret an RPM-derived level as percent |
| omencore `HpWmiBios.DetectMaxFanLevel:962`, `WmiFanController.MapFanPercentToWmiLevel:212` | Reviewed model/user ceilings up to100; V1 default55, V2 default100. Percent<100 scales by ceiling; 100 maps to100 regardless of ceiling in the helper | Normal `SetFanSpeed(0)` redirects to auto recovery. Raw zero still exists in conditional recovery paths. | V1 versus V2 scale distinction is app evidence only; helper100 is not a universal BIOS maximum |
| ghelper-omen `HpWmiBios.DetectMaxFanLevel:537`, `WmiFanController.SetFanSpeed:524` | Percent0..100, model/V1 default55/V2 default100; percent scaled by ceiling; SetFanMax failure uses ceiling | V2 nonzero values below20 are floored in caller; zero avoided in reset handoff; setter itself accepts bytes | No hardware minimum/maximum validated for F.31 |

The common source comment "0-255, in krpm units" in the two `HpWmiBios.SetFanLevel` methods is insufficient and dimensionally imprecise: several callers multiply/divide by **100**, not 1000. V2 percent policy conflicts with a universal RPM-unit interpretation. Report it as source terminology, not a conversion VictusX can adopt.

No deliberate normal SetFanLevel target `255`, or validated SetFanLevel `0xFF` auto/keep-current sentinel, was established in these reviewed callers. A byte parameter can represent255 and cleaning arithmetic can produce high-bit values; neither proves safe acceptance. OmenXHub's separate `0x29` power-field `0xFF` sentinel must not be transferred to `0x2E`. No accepted physical minimum, maximum, zero meaning, or raw-PWM scale is proven for the target.

## 5. Dual-Fan Mapping Evidence

Byte0 receives argument1 and byte1 argument2 in all four HP setters. omencore/ghelper-omen explicitly name/log these CPU/GPU and expose independent `SetFanSpeeds(cpuPercent,gpuPercent)` (`WmiFanController.cs:764` / `:697`). OmenXHub's curve loop computes fan indices0/1 independently, optionally synchronizing to their maximum. OmenSuperHub normally duplicates one requested value; its method and cleaning settings can differ per index.

This supports a **per-fan-index implementation hypothesis**, not a low/high byte split or an ABI requiring equal targets. No reviewed evidence maps those indexes to physical left/right on the exact Victus. CPU/GPU labels also do not prove isolated cooling zones or ordering on that device.

The Omen wrappers' optional third byte is selected by a flag, with callers consulting `IsThreeFanSupported`/fan-type metadata; it is an average, not a general fan-count-sized array. The other setters always have two target positions regardless of reported fan count. omencore's close-device 8BD4 profile has `FanZoneCount=2` but `SupportsIndependentFanCurves=false` and `UserVerified=false`. Two fans detected by VictusX therefore prove neither independent writable targets nor a particular buffer length.

## 6. SetFanLevel / FanGetLevel Relationship

| Reference source | Read path and relationship used | Evidence limit |
| --- | --- | --- |
| OmenXHub `OmenHardware.cs:160,177`; TrayService loop | `0x2D`, four zero input bytes, 128 output bytes, first three taken as levels. Separate `0x11` indexed RPM helper interprets bytes2/3 big-endian. Loop may substitute requested levels when monitoring is off. | Comment calls `0x2D` a Victus_S path, but no exact F.31 correlation/physical mapping. A displayed target substituted for telemetry is not feedback. |
| OmenSuperHub `OmenHardware.GetFanLevel:373`; `Program.cs:297,1109,1400` | Same `0x2D` read. Compares requested RPM/100 with first two values; displays each *100, or average `(a+b)*50`; may substitute requested levels if monitoring off. | A comparison/conversion in code is not an independent RPM measurement or validated round trip. Read failure defaults to zero values in wrapper. |
| omencore `HpWmiBios.GetFanLevel:1087`, `ExtendFanCountdown:2711`; ghelper-omen equivalents:677,2168 | Prefer alternate V2 `0x37` read; can derive levels from an RPM path divided by100, then use legacy `0x2D`. Countdown can replay returned pair into `0x2E`. | A derived value or replay is not evidence that raw V1 output equals a safe input. Variant selection, fallback and estimates can hide the actual source. |

No independently calibrated, same-session SetFanLevel input / raw readback / per-fan tachometer dataset for the exact device was found. No PWM duty relationship is demonstrated. `FanGetLevel` remains **raw-only**, not RPM, percentage, target, or auto-state; `FanMaxGet=false` remains **inconclusive** as latch confirmation. No new read path, including `0x11` or `0x37`, is authorized.

## 7. Recovery And Restore Behavior

- **OmenSuperHub:** `Program.Config.cs:366 / RestoreFanControl` restores configured behavior, not necessarily firmware auto. Its `auto` branch calls MaxFan off and enables a one-second software curve timer. RPM branches call off then reapply levels; max calls on. `Program.Menu.cs:383` behaves similarly. `Program.cs:1539 / Exit` stops monitoring/exits but contains no SetFanLevel-auto or guaranteed fan cleanup sequence. Cleaning restores configuration after its timer, not independently verified automatic control.
- **OmenXHub:** `TrayService.cs:671` selection branches use off plus software curves, percentage or RPM-derived levels; `Exit:1416` disposes timers and hardware monitoring without a bounded SetFanLevel `finally` restore in that routine. The EC fallback is a different manual-control path, not an approved BIOS recovery mechanism. Do not infer a fail-safe handoff from stopping timers.
- **omencore:** `WmiFanController.RestoreAutoControl:1001` and reset helpers combine MaxFan off, default SetFanMode, V1 level20 hints and conditional zero-floor clearing. `ClearV1AutoModeFloor:2077` skips zero on conservative/V2 paths. Normal zero-percent requests redirect to restore rather than imply raw zero is auto. The 8BD4 profile explicitly disables V1 zero-floor clearing after field failures.
- **ghelper-omen:** `WmiFanController.RestoreAutoControl:926` and reset logic near1019 use MaxFan off/SetFanMode(Default), V1 hint20, and explicitly skip zero; V2 skips the level handoff. Neither hint20 nor a mode transition is validated recovery for VictusX.
- Both WMI controllers' `Dispose` (omencore:2414, ghelper-omen:2075) deliberately stop countdown extension **without restoring auto**, to preserve user settings. Application-wide crash/power-loss recovery is not proven by these routines.

No safe restore packet emerges from this review. **Do not select `00-00`, echo baseline bytes, use MaxFan off as assumed recovery, or port SetFanMode prerequisites.** BIOS timeout, process exit, `finally`, or elapsed time alone cannot prove automatic control resumed. Recovery remains a hard prerequisite before any separate first-write proposal.

## 8. Background And Reapply Behavior

| Repository / symbols | Observed scheduling | VictusX boundary |
| --- | --- | --- |
| OmenXHub `TrayService.StartTimers:512`, `OptimiseSchedule:565`, `OnPowerChange:958` | Software curves/smart mode send off plus level every1s, without speed-change guard. 30s mode/config reassertion; startup restore and resume countdown reapply settings; AC events restore power config. Fixed paths also receive startup reassertion. | Not a one-shot system; no timers, curves, mode reassertion or persisted writes may be adopted. |
| OmenSuperHub `Program.cs:288,297,756,782`; `Program.Config.RestoreFanControl:366` | Curve checks every1s and writes when level differs by>1; 30s scheduler reapplies max/RPM for first five ticks. Config restore after startup/resume and AC power config handling exist. | "Auto" includes application writes. No startup/resume/curve behavior is acceptable by implication. |
| omencore `WmiFanController` interval:98, callback and `ExtendFanCountdown`; `Services/FanService.cs:675` | 5s maintenance timer, manual reassertion and read/replay countdown. `HandleSystemResume` reapplies active preset or forces curve refresh. `PowerAutomationService.cs:372` can apply configured AC/battery fan presets. | Reject background reassertion, profile automation and repeated recovery. |
| ghelper-omen `WmiFanController.cs:51,72,1633,1777` | 800ms timer, manual reapply interval2500ms, preset interval30000ms; separate CPU/GPU levels retained, countdown can replay four-byte request after128-byte setter. | Not a bounded one-write model. Inherited sleep/settings flow also exists; a complete HP-specific power-event packet trace is not established by this review, and must not be presumed safe. |

All four contain continuous-control architecture or reassertion beyond their single setter bodies. A void/simple wrapper is not proof of one total write per user action. Shutdown and resume behavior need separately reviewed traces and hardware evidence before porting anything executable.

## 9. Device-Family Relevance Ranking

1. **Closest, not exact:** omencore `ModelCapabilityDatabase.cs:1687` and `docs/3.6.3-BUG-REPORTS.md:583`: Victus16-s0xxx, board8BD4, BIOSF.30, V1, two level fields. Profile disables EC fan control and independent curves, remains `UserVerified=false`, and warns of zero-level handoff failures. Its `7Z5Z2EA` note concerns keyboard RGB, **not** exact-SKU SetFanLevel validation. Reports of max-stuck and non-reactive/zero fans are negative safety evidence; no calibrated units/ABI matrix is supplied.
2. **Generic/near family:** omencore V1 logic and ghelper-omen Victus entries/default V1 ceiling55. ghelper-omen's reviewed model database includes other Victus families but no matching16-s0/8BD4/F.31 entry found. Defaults and inferred model entries are not device confirmation.
3. **Generic HP:** OmenSuperHub/OmenXHub BIOS wrappers, indexed fan types, optional third/cleaning capability. OmenXHub's Victus_S comment is not a dated SKU/BIOS experiment. Its special Omen6 return-code handling is another model-specific assumption, not an F.31 rule.
4. **No relevant HP ABI evidence:** g-helper and ECC at the reviewed revisions.

The target remains HP Victus16-s0035nt / `7Z5Z2EA#AB8` / F.31 / ThermalPolicyVersion1. Similar names, a nearby BIOS version, and shared implementation ancestry cannot promote source confidence to exact-device validation.

## 10. PawnIO / EC / Fallback Risk

| Path | Driver/fallback involvement | Decision |
| --- | --- | --- |
| OmenSuperHub SetFanLevel wrapper | BIOS WMI; no application-level direct EC fallback or wrapper retry found. Project includes LibreHardwareMonitor, and `OmenHardware.IsPawnIOInstalled:49` checks PawnIO for broader telemetry. | BIOS-only setter does not make the app driver-free; no monitor/driver or recurring-control dependency is required by VictusX. |
| OmenXHub SetFanLevel | WMI null result invokes `Services/EcFanService.SetFanSpeed:78` when available; PawnIO `LpcAcpiEc`, manual registers, percent clamp0..100; `EcWriteByte:166` retry loop and ready polling. | Entire fallback blocked. Raw/nominal-RPM input being reused as EC percentage is especially unsafe to infer across paths. |
| OmenXHub transport | `OmenHardware.cs:103` treats command type0x2E + returnCode0x2E as success (`Array.Empty<byte>()`) under an Omen6 quirk comment. | Do not port return-code exception or interpret46 as measured RPM on VictusX. |
| omencore/ghelper-omen WMI setters | BIOS CIM/WMI bodies, not PawnIO setters; `SendBiosCommand` can fall back from CIM to legacy WMI after exceptions. `SetFanSpeed` loops up to3 attempts with500ms delays; MaxFan failure can fall back to SetFanLevel. | Even a BIOS-only wrapper may repeat a write via transport/controller fallback. None of this retry behavior is allowed in VictusX. |
| Wider omencore/ghelper-omen stack | Separate EC/driver controller implementations exist; e.g. ghelper-omen `app/Omen/FanController.cs:42,544` accepts `IEcAccess` and exposes EC immediate percent writes, alongside `PawnIOEcAccess`. | Do not port a generic controller/selector or assume selecting a BIOS facade excludes all wider fallback behavior. |
| g-helper / ECC | No HP SetFanLevel call chain identified | Their unrelated drivers, controls or similarly named concepts prove nothing about this ABI. |

No WinRing0, PwnIO spelling, or LibreHardwareMonitor fan-write call was identified in the reviewed BIOS setter bodies. PawnIO/monitor presence elsewhere must be kept distinct from the actual SetFanLevel transport. This audit does not certify every vendored library as read-only; VictusX must inherit **none** of these low-level fallbacks.

## 11. Interpretation Of `80-80`

**Answer: C (uncertain firmware meaning).** At the serialization level only, A is literally true: decimal128 is encoded independently into each of two bytes. It is not a16-bit RPM value, 128%, half duty, or a proven128-level target.

B (invalid because all references expect0..100) is not established: references use heterogeneous model ceilings, RPM-derived values, byte wrappers and cleaning variants. Nevertheless128 is above the ordinary default55/64/100 caller scales reviewed here and is **not an evidence-backed normal target**.

D is an additional unresolved possibility: the Omen cleaning branch can encode a nominal zero plus128 on supported fans. The dry-run does not enable cleaning or read capability bits, but it would serialize identical bits; whether firmware interprets that high bit specially on this Victus is unknown. Do not claim it will reverse a fan, will be rejected, or will clamp safely.

The dry-run's parser0..255 boundary is only storage representability. Its persisted `NoHardwareInvocation=true`, `NoWmiInvocation=true`, `WriteExecuted=false` and `DeviceValidatedInputLength=null` remain truthful and unchanged. Preserve the historical record; do not retroactively classify serialization as a hardware test.

## 12. Evidence Conflicts

- Actual array lengths disagree:2/3,4,128. ghelper-omen even disagrees with itself across setter/countdown and its stale four-byte comment.
- "krpm" comments, RPM/100 callers, percent-direct callers, and V1/V2 scaling do not specify one common unit.
- Two fan indexes are widespread; CPU/GPU labels and independently controllable zones are not guaranteed by the close-device profile. No left/right proof exists.
- Wrapper byte ranges and optional +128 cleaning do not support a uniform unsigned-speed interpretation.
- "Auto" can mean a software curve, SetFanMode transition, conditional zero/hint write, or stopped reassertion; none is a universal restore packet.
- Feedback can be raw, RPM-derived, or replaced by the requested value. Echo-looking telemetry is not independent confirmation.
- Shared code/comments across projects are not independent hardware replications. Read source bodies and provenance rather than count agreeing names.

## 13. Unknowns That Remain

Accepted F.31 input length/padding; physical units and safe range; high-bit/sentinel meanings; byte-to-physical-fan order and independent control; required thermal-policy/mode state; independent tachometer correlation; write rejection versus silent clamp; reliable firmware-auto handoff; crash/power/suspend/service-contention recovery; durable cooling under load; exact behavior after stopping reassertion. Local source alone has not closed these items.

## 14. First-Write Readiness

**NO-GO. Executable candidate set: empty.** No first-write value, asymmetric pair, recovery packet, input length, new gate permission, transport or UI route is selected. SetFanMax's working four-byte pair does not validate SetFanLevel's separate command.

The [preflight design](set-fan-level-first-write-preflight-design.md) still requires exact-device ABI/units/mapping and independently justified recovery before any separate implementation/execution decision. No fallback experiment can resolve these conflicts safely by assumption. `DeviceValidatedInputLength` remains null; `FanGetLevel` raw-only; `FanMaxGet` inconclusive. Normal/user-facing fan control remains NO-GO.

## 15. Recommended Next Safe Task

Create a **documentation-only exact-device ABI and recovery evidence request**: ask upstream maintainers for existing authoritative protocol descriptions or already-recorded, identity-tagged F.31/V1 traces that distinguish input Size, indexed targets, high-bit behavior, independently sourced RPM and firmware-auto recovery. Reconcile the128-versus4-byte ghelper-omen discrepancy and close-device zero-handoff report before considering any candidate. This is a request for existing evidence, not authorization for anyone to perform new writes, probes or sweeps. No new experiment, product implementation, background control or fan UI is recommended now.

- Developer-only four-byte SetFanMax Pulse/Hold: operational under existing explicit CLI gates only; physical duration/level control is not validated.
- SetFanLevel first write: **NO-GO**, no executable value selected.
- `DeviceValidatedInputLength`: null/unset.
- Normal/user-facing fan control: **NO-GO**. No fan UI until separate evidence exists.
