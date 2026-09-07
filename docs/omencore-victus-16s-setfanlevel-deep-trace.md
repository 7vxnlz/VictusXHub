# omencore Victus 16-s0xxx SetFanLevel Deep Trace

## 1. Executive Conclusion

Follow-up implementation: the inert [percentage dry-run](set-fan-level-percentage-mapping-dry-run.md) now models the traced default55 arithmetic, including the100 exception and low-positive zero truncation, in a four-byte JSON candidate only. Historical raw128/80-80 remains deprecated compatibility. This does not adopt endpoint control branches, resolve F.31 units/ABI/recovery, or change first-write NO-GO.

Static source trace, 2026-09-05. **omencore's closest Victus/V1 normal request path converts an application percentage to a model-scaled byte and sends two fan arguments plus two zeros, with input Size=4. It does not set a manual/cleaning high bit.** With no override, V1 uses a software ceiling of 55; this is an implementation default, not a measured F.31 limit.

The ghelper-omen 128-byte request is allocated in its SetFanLevel method and passed through unchanged with Size=128. It is **not automatic shared-WMI padding**. Its countdown helper separately sends four bytes. The first two logical fields agree, but wire-length equivalence remains unproven.

OmenHub modern cleaning adds 128 per capability-enabled fan; legacy cleaning instead toggles bit7 in byte3 of a different command. Neither establishes what Victus F.31 does with `80-80`. That candidate is unexpected for omencore's ordinary percentage mapper and remains uncertain as firmware input.

**SetFanLevel first write: NO-GO. No first-write value, restore packet, or executable ABI is selected.** Normal fan control remains NO-GO and `DeviceValidatedInputLength` remains null. No source was copied, no product behavior changed, and no reference, WMI method, dry-run command, or experiment was executed.

Verification: solution build passed with zero errors and four recurring NU1900 audit-source warnings; solution tests passed276/276, none skipped. Only build/test were executed, not any application command. No publication, release artifacts or commit. These checks do not prove hardware semantics or resolve the audit-source warning.

### Provenance

Reference root: `D:\Projects\Workspace\references\`. Paths and line anchors below are relative to the indicated repository at these revisions. Symbols, not comments alone, were traced through callers and transports.

| Repository | Commit | Use in this trace |
| --- | --- | --- |
| omencore | `b39b44978902606aa708cc0d78bcfd87e95fd88b` | Primary model, service, controller, lifecycle and transport chain |
| ghelper-omen | `1694844d2725e79a2b2065a0a1494fa1d143e3f4` | Setter/transport/countdown and scaling comparison |
| OmenSuperHub | `a6ab6988c446ee5421466097fdf60c0d521e5c81` | Modern versus legacy cleaning and payload comparison |
| OmenXHub | `ca84cb011d1d3e5850445d19e45fbea06e83a8fd` | Cleaning and EC fallback comparison |
| g-helper | `5c26f5ac970dab9e26347d80976ebf1eece91b1e` | Prior audit's negative HP SetFanLevel search; no relevant new trace |
| ECC | `5eddf1a3ffd311423be2d4ba7d26f7209c91b033` | Agent-harness repository, not an HP EC implementation |

The [ABI audit](set-fan-level-abi-units-evidence-audit.md) provides the six-repository inventory. This follow-up expands selected omencore files because factory, service and shutdown behavior cannot be inferred from the BIOS wrapper alone. Searches included model/policy/level symbols, `0x80`, `0x7F`, 128/127, cleaning/manual/override/auto/reset, buffer construction and fan-index terms. Unrelated power, GPU and capability-bit matches were not treated as SetFanLevel semantics.

Confidence labels used here: **Exact target evidence**, **Close-device evidence**, **Generic HP evidence**, **Implementation inference**, **Unknown**. Code-proven behavior is not device validation.

## 2. Closest-Device Evidence Chain

| Link | Source / symbol | Established fact | Confidence / limit |
| --- | --- | --- | --- |
| Field context | omencore `docs/3.6.3-BUG-REPORTS.md:583`, DC-Victus16-8BD4 | Report identifies Victus16-s0xxx, board8BD4, F.30; WMI selected; zero-level handoff and stuck/non-reactive fans reported | Close-device evidence; summarized historical logs, not a captured F.31 packet trace |
| Identity collection | `src/OmenCoreApp/Hardware/CapabilityDetectionService.cs:290`, `DetectDeviceInfo` | Model from Win32_ComputerSystem, BIOS from Win32_BIOS, ProductId from Win32_BaseBoard.Product | Implementation inference; 8BD4 is a board product identifier, not SKU7Z5Z2EA#AB8 |
| Profile resolution | `CapabilityDetectionService.cs:96`, `LoadModelCapabilities`; `ModelCapabilityDatabase.cs:1912`, `GetPreferredCapabilities` | Non-ambiguous exact product lookup wins; otherwise model-name/vendor matching then product fallback | Implementation inference; no F.31-specific gate here |
| Profile | `ModelCapabilityDatabase.cs:1687` | 8BD4/16-s0 profile: WMI=true, EC fan control=false, curves=true, independent curves=false, two zones, UserVerified=false, AllowV1AutoModeFloorClear=false | Close-device evidence; profile declaration is not independent acceptance testing |
| Generation | `HpWmiBios.cs:807`, `QuerySystemData` | Reads thermal policy from SystemDesignData result[3]; OMEN MAX name branch can probe/promote to V2 | Implementation inference; a Victus name does not satisfy the OMEN+MAX condition. F.30 field report says V1 |
| Controller choice | `CapabilityDetectionService.cs:685`; `FanControllerFactory.cs:250,410` | Available BIOS plus WMI-capable model chooses WmiBios; factory constructs WmiFanController and wrapper | Implementation inference; availability check is not proof of level effectiveness |
| Conservative parameters | `FanControllerFactory.cs:414` | EC-disabled or unverified profile gives strictFanModeReadback=false; explicit floor-clear=false is passed | Implementation inference; relaxes an EC readback check, not an ABI proof |

The 8BD4 profile does not supply a MaxFanLevel override. `HardwareBringup.cs:186` passes configured user override and model metadata to the factory; `WmiFanController.cs:250` calls `DetectMaxFanLevel`. Valid user override1..100 takes priority, then valid model override, then V2=100 or V1=55 (`HpWmiBios.cs:962`). Thus55 is conditional on no override and actual V1 detection, not an unconditional property of every Victus.

The profile's SKU7Z5Z2EA note concerns keyboard RGB. It does not validate fan units on the user's F.31 device. The historic F.30 report describes failures before conservative handoff changes; the current source trace is not claimed to be the exact old executable that produced those logs.

## 3. End-To-End Call Graph

Primary fixed-percentage path, conditional on successful reference initialization:

```text
HardwareBringup -> CapabilityDetectionService.DetectCapabilities
  -> DetectDeviceInfo -> LoadModelCapabilities -> 8BD4 / 16-s0 profile
  -> DetectWmiBiosCapabilities -> DetermineFanControlMethod (WmiBios)
  -> FanControllerFactory.Create -> TryCreateWmiController
  -> WmiFanControllerWrapper -> WmiFanController (V1, ceiling55 absent override)

FanControlViewModel.ConstantFanPercent (clamp0..100)
  -> ApplyConstantSpeed -> FanService.DisableCurve
  -> FanService.ForceSetFanSpeed(percent)
  -> SetFanSpeedSerialized -> RuntimeEcOperationCoordinator.Execute
  -> WmiFanControllerWrapper.SetFanSpeed
  -> WmiFanController.SetFanSpeed
       zero -> RestoreAutoControl (not the direct level branch)
       100 -> SetFanMax; failure may fall back to SetFanLevel(100,100)
       1..99 -> MapFanPercentToWmiLevel -> SetFanLevel(level,level)
  -> HpWmiBios.SetFanLevel -> new byte[4], first two arguments, two zeros
  -> SendBiosCommand(0x20008,0x2E,data,outputSize0)
  -> hpqBData=data; Size=4; InData=hpqBDataIn
  -> root\wmi / hpqBIntM / hpqBIOSInt0 (CIM or legacy WMI)
```

Anchors: `ViewModels/FanControlViewModel.cs:372,2269`; `Services/FanService.cs:3168,1653`; `Services/RuntimeEcOperationCoordinator.cs:18`; `Hardware/FanControllerFactory.cs:838`; `Hardware/WmiFanController.cs:620`; `Hardware/HpWmiBios.cs:1052,2455,2599`. All paths are under omencore `src/OmenCoreApp/`.

The coordinator's name includes EC but its body locks and executes a delegate once; it does not convert values, perform EC I/O, or retry. Actual retry/fallback behavior is downstream. This distinction prevents treating a class name as evidence of direct EC writes in the chosen BIOS setter.

Curve entry: `FanService.ApplyCustomCurve:1244` validates points; `ApplyCurveIfNeededAsync:2530` evaluates temperatures and percentages, then uses the same serialized setter or a multi-step ramp. Independent curves use `EnableIndependentCurves:1340`, `ApplyIndependentCurvesAsync` and `SetFanSpeedsSerialized:1668`. A separate lower-level `WmiFanController.ApplyCustomCurve:562` branch chooses a discrete curve point and calls the same byte mapper directly; it is not the service's interpolating algorithm. No single raw value can be inferred from an arbitrary curve without its points, sensor sample, configuration and prior state.

## 4. Value Transformation Table

All numeric examples below are **static arithmetic illustrations, not proposed first-write values**. All rows describe implementation behavior; physical units remain unvalidated.

| Stage / omencore source | Transformation | Output / important qualification |
| --- | --- | --- |
| `FanControlViewModel.ConstantFanPercent:372` | Clamp integer to0..100 | Application percentage, not raw byte |
| `FanService.ValidateCurve:1375` | Reject fewer than2 points, decreasing temperatures, percent outside0..100; sort accepted curves on storage | Does not validate hardware support |
| `SmoothCurveTemperature:1473` | Limit sample delta to +6/-4 degrees per evaluation; bypass at75 degrees or uninitialized state | Changes curve input, not a payload bit; source policy, not endorsed safety limits |
| `InterpolateFanSpeed:1512` | Linear interpolation between points, clamp interpolated percent0..100; endpoints return endpoint percent; empty fallback50 | Scalar curve target; not RPM feedback |
| `AdjustFanPercentForGpuPowerBoost:3135` | Cast interpolated percent to int; optional addition0/2/5/8 scaled by `min(1,(gpuTemp-50)/30)` and truncated; cap100 | 8BD4 declares GPU boost unsupported, so this is conditional broader code, not confirmed active target behavior |
| `ApplySafetyBoundsClamping:1411` | If enabled: emergency threshold forces100; source floors80/60/40 at90/85/80 degrees | Can override a curve; opt-out exists. No new VictusX thermal policy approved |
| Service application | Direct single-target path casts to int; independent path rounds; hysteresis delays writes | Current/previous state affects actual request |
| `RampFanToPercentAsync:2802` | Linear integer-rounded intermediate percentages, each clamped0..100, delayed and sent; final target ensured | Multiple writes, not a harmless display animation |
| `WmiFanController.SetFanSpeed:620` | Clamp0..100; exact0 redirects to auto; 100 uses MaxFan with level100 fallback | User input128 would clamp to100, not emit raw128 through this route |
| `MapFanPercentToWmiLevel:212` | For p<100, integer `(p * clamp(ceiling,1,100))/100`; for p=100 return100 | V1/no override: p50 ->27 (`1B`); p99 ->54 (`36`). No RPM calculation on write path |
| Low positive percentages | V1/no override p1 ->0 after integer division | Zero-percent redirect occurs BEFORE scaling. Consequently raw zero is still reachable even when explicit zero-floor clearing is disabled |
| Dual setter:764 | Clamp both; both0 ->auto; both100 ->MaxFan/fallback; otherwise map separately. With ceiling>=100 only, individual zero is raised to1 | V1 does not receive that single-fan floor; both bytes still sent together |
| BIOS setter:1052 | Assign byte arguments unchanged into positions0/1, append zeros0/0 | No additional clamp, percent/RPM division, mask, cleaning flag or byte-order conversion |
| Transport:2455 | Assign array and its Length, command/type/sign, choose method by output size | No per-byte transformation or universal128-byte expansion |

Manual/override flags such as `IsManualControlActive` are in-memory scheduling state, not ORed into the targets. `MaxFanLevelOverride` changes the scale, not a high-bit flag. `-1` last-target values are internal state sentinels, not validated `FF` firmware targets. `SetFanMode`'s `FF` prefix belongs to command0x1A, not SetFanLevel.

## 5. Payload Construction Table

All four BIOS transports use `root\wmi`, `hpqBIntM`, command0x20008/type0x2E and output-size0 selecting `hpqBIOSInt0`. Table entries are actual managed input arrays and the Size field, not independently captured firmware buffers.

| Repository / file-symbol | Logical fields | Allocated input / Size | Remaining bytes | Confidence |
| --- | --- | --- | --- | --- |
| omencore `Hardware/HpWmiBios.cs:1052`, SetFanLevel | fan1, fan2 | 4 / 4 | Positions2/3 explicit zero | Implementation inference |
| omencore same file:2711, ExtendFanCountdown | returned fan1, fan2 | 4 / 4 | Two zeros | Implementation inference; readback replay is another input origin |
| ghelper-omen `app/Omen/HpWmiBios.cs:643`, SetFanLevel | fan1, fan2 | 128 / 128 | Positions2..127 zero-initialized | Implementation inference |
| ghelper-omen same file:2168, ExtendFanCountdown | returned fan1, fan2 | 4 / 4 | Two zeros | Implementation inference |
| OmenSuperHub `OmenHardware.cs:511`, SetFanLevel | fan1, fan2; optional fan3 average | 2 / 2 or3 / 3 | Third byte is meaningful, not padding | Generic HP evidence |
| OmenXHub `OmenHardware.cs:246`, SetFanLevel | Same, with optional cleaning and EC fallback | 2 / 2 or3 / 3 | No normal trailing zero padding | Generic HP evidence |
| VictusX dry-run record | repeated candidate byte | JSON hypothesis2; no method call | None | Exact target evidence limited to prior inert serialization/persistence, not hardware ABI |

omencore transport anchors: `HpWmiBios.cs:2486` assigns hpqBData, :2487 assigns Length, :2503 invokes CIM; legacy :2636 assigns uint Length, :2639 assigns data, :2643 selects method and :2647 invokes. Legacy selects the instance by BIOS_METHOD_INSTANCE. The method suffix describes requested OUTPUT size, not input length. A five-second CIM timeout is not physical stop protection and can lead to legacy fallback after failure.

## 6. Payload-Length Interpretation

**Resolved in source:** ghelper-omen's128 is a caller allocation, not imposed by its transport. `HpWmiBios.cs:653` allocates it; :1946 passes it unchanged and :1947 sets Length. The same infrastructure accepts the countdown helper's4. OmenSuperHub :879-880 and OmenXHub `SendOmenBiosWmi` similarly pass actual array length. No reviewed transport silently turns2/4 bytes into128.

**Not resolved:** whether HP provider/firmware ignores, requires, copies, or interprets trailing bytes at the target BIOS. Source does not explain the historical reason for ghelper-omen's larger setter allocation; its old four-byte comment is not evidence of infrastructure necessity. The shared first two fields give logical similarity only. The3-byte variant contains a third target, while4/128 contain padding; these are genuinely different requests and cannot be treated as ABI-equivalent or tried as fallbacks.

No fan-count-dependent allocation exists in omencore's setter. WMI class schema capacity, actual managed array, declared Size and requested output size must remain four separate facts. No runtime schema query was performed.

## 7. High-Bit / 0x80 Semantics

| Search finding | Exact path | Meaning established by code | What it does NOT prove |
| --- | --- | --- | --- |
| No +128 / OR80 / AND7F in normal level mapping | omencore WmiFanController mapper/setters and HpWmiBios.SetFanLevel; ghelper-omen corresponding setter | Ordinary application requests are unflagged mapped bytes | Firmware cannot attach special meaning to an arbitrary raw high-bit byte |
| Modern cleaning adds128 | OmenSuperHub OmenHardware.cs:513-520; OmenXHub :248-254 | With fanClean=true and indexed capability=true, add128 before cast; optional third uses average plus128 | F.31 supports this mode or interprets80 as a safe cleaning request |
| Legacy cleaning OR80 / AND7F | OmenSuperHub :494-507; OmenXHub :227-232 | Read command1/type44 (`0x2C`), toggle byte3 bit7, write command2/type44 | This is NOT command0x20008/type0x2E and does not define either byte of80-80 |
| Manual/auto flags | omencore controller state and separate SetFanMode requests | Software ownership state plus separate commands | Bit7 of a level is a manual-override indicator |
| Other127/128/7F/80 matches | SystemDesignData, GPU mode and power routines; large output buffers | Different fields/commands or buffer sizes | A universal fan sentinel, percent range or cleaning contract |

For nominal values0..127, arithmetic `value+128` is numerically equal to setting bit7. The Omen wrappers use **addition**, not a protective `value & 0x7F` followed by OR80. For arbitrary larger inputs, casts/overflow behavior cannot be interpreted as safe flag packing. Capability presence and code comments describing reverse cleaning are **Generic HP evidence** only. No high-bit flag or cleaning encoding is found on the primary omencore Victus level path.

Important exception to the ordinary mapper boundary: `ExtendFanCountdown` can replay raw `GetFanLevel` bytes without that mapper. Thus "ordinary percentage writes stay <=100" does **not** mean every possible omencore0x2E packet is bounded that way. The replay helper provides no validation of bit7 or input/output equivalence.

## 8. Dual-Fan Mapping Evidence

Position0 receives fan1 and position1 fan2; omencore names/logs them CPU/GPU. No16-bit combined target or endian conversion occurs. Left/right physical mapping is absent. Both positions always travel in one request; there is no sparse one-fan write or proven keep-other-fan sentinel.

`WmiFanController.SetFanSpeeds:764` accepts separate CPU/GPU percentages and emits both mapped targets. However, after success :851 stores `Math.Max(cpuPercent,gpuPercent)` in the single last-manual field; maintenance :1870-1871 later sends that value to **both** fans. Independent initial targets can therefore be overwritten by equal-pair reapplication. ghelper-omen instead retains separate manual CPU/GPU targets for its maintenance callback (see prior ABI audit).

The8BD4 profile declares two zones and independent curves=false. That declaration must not be confused with absence of a low-level dual setter: the method exists and has no board-specific fan-count/independent-zone gate. Service independent curves may call it where exposed; the normal shared curve uses the hotter component and equal targets. No independent physical control is proven on F.31.

## 9. Restore / Handoff Behavior

The closest profile sets `allowV1AutoModeFloorClear=false` and `strictFanModeReadback=false`. It does **not** establish a simple SetFanLevel off packet.

1. `RestoreAutoControl:1001` first stops countdown scheduling, checks availability, and runs `ResetFromMaxMode` only when software tracks manual/max state and cooldown permits.
2. `ResetFromMaxMode` near1090 conditionally sends MaxFan off, sends SetFanMode(Default), then, when ceiling<100, sends raw level20/20 (`14-14-00-00`) as a transition hint. This hint bypasses percentage scaling. `ClearV1AutoModeFloor:2077` returns without zero for the8BD4 profile.
3. Restore sends SetFanMode(Default) again, checks mode according to configured readback strictness, clears software manual state and invokes the same skipped floor-clear helper. Success returned by this sequence is not independent proof of physical auto control.
4. `ResetEcToDefaults:1169` on the WMI controller has a broader BIOS sequence: MaxFan off, default mode, V1 raw20/20, `ExtendFanCountdown` (potential readback replay), Balanced performance mode, then final default mode. Its name/logs say EC reset, but this implementation uses BIOS methods, not direct EC register writes. It is still a broader write sequence than VictusX permits.

Zero is **not eliminated globally**: other profiles can allow floor clearing; small positive V1 percentages can truncate to raw0; asymmetric V1 requests can include0; raw countdown replay can contain0. F.30 failure evidence makes these paths concerns, not recommended recovery experiments. Neither20, zero, SetFanMax off nor a SetFanMode transition is selected as VictusX recovery.

Shutdown clarification: the controller's `Dispose:2414` deliberately does not restore. But `MainViewModel.cs:5998` calls `FanService.Dispose:3254`, which, if writes are available, attempts `RestoreAutoControlSerialized`, waits100ms, then calls `ResetEcToDefaults`, before disabling curves/stopping. This **does attempt application-level recovery**, qualifying any inference based solely on controller Dispose. Failure/crash/power loss can still bypass or defeat it; no durable F.31 guarantee follows.

## 10. Background / Reapply Behavior

- Service curves interpolate, apply hysteresis, optionally ramp, and force refresh (normally30s). `DisableCurve:1315` clears software curve state; by itself it sends no firmware-auto request.
- The controller maintenance timer ticks every5s; manual writes normally have15s spacing or5s at high duty. Throttled callbacks can still call countdown extension. Multiple0x2E requests may occur through mapped reassertion and raw replay.
- Single-speed writes have up to3 attempts/500ms delays. Shared transport can try legacy WMI following CIM failure, independently of controller retries. A service comment saying single attempt does not remove downstream retries.
- `FanService.HandleSystemSuspend` near630 pauses the engine, stops maintenance first, then attempts auto restore. `HandleSystemResume:675` reapplies active preset or forces curve reevaluation. `MainViewModel:2874,2883` wires those calls; these are executable lifecycle behaviors, not just documentation.
- The previous audit traces AC/battery preset application in `PowerAutomationService`. Model/mode changes can trigger reset, level hints and other commands. No lifecycle write pattern is approved for VictusX.

## 11. Comparator Findings And Non-Port List

| Area | omencore closest path | ghelper-omen | OmenSuperHub / OmenXHub |
| --- | --- | --- | --- |
| Ordinary scale | V1 default55, percent mapper with100 exception | Model-dependent ceiling; similar percent conversion | Nominal RPM/100 callers; XHub also direct-percent callers |
| Input |4 bytes, Size4 | Setter128; replay4 |2 bytes, optional third target |
| High bit | No normal flag packing; raw replay unmasked | No normal flag packing; raw replay unmasked | Modern +128 cleaning; separate legacy byte3 bit7 protocol |
| Restore | Multi-command, no zero-floor clear for8BD4; service-level shutdown reset | Similar mode/hint assumptions; controller Dispose alone does not restore | Auto can mean a software curve, not firmware auto |
| Fallback | Controller retries, CIM-to-legacy fallback; factory has other backends | Retrying WMI and broader EC-capable stack | SuperHub BIOS setter has no application EC fallback found; XHub has PawnIO EC fallback |

**Factory caution:** `HardwareBringup.cs:160` initializes EC access for the broader app; `FanControllerFactory.CreateWithAutoDetection:310` tries WMI then EC then OGH. `TryCreateEcController:455` checks supplied access/map availability, not the model flag in that method. The8BD4 capability declaration therefore must not be quoted as proof that copying the entire factory is fail-closed against EC fallback. The traced successful WMI setter is BIOS-only; the whole application is not.

VictusX must not inherit zero/hint recovery assumptions, mode/performance writes, command0x37 power paths, raw readback replay, background resends, curves, ramps, retry loops, transport fallback, EC/PawnIO backends, or UI control routes. In particular, SetFanMode is used by reference transitions but is not a proven mandatory precondition for every0x2E call: the primary steady-state setter does not unconditionally invoke it. No prerequisite may be imported implicitly.

## 12. Interpretation Of VictusX `80-80`

| Possible reading | Static result |
| --- | --- |
| Two raw bytes128/128 | Proven serialization fact only; not ordinary output of omencore's clamped percentage mapper |
| Flag bit plus zero | Numerically possible under the modern Omen cleaning convention; firmware recognition on this Victus is Unknown |
| Cleaning/reverse mode | Generic HP implementation evidence exists only with capability/cleaning branch; target meaning unproven |
| Manual override indicator | No evidence in the traced normal level path; manual state is software/separate mode commands |
| Invalid/unexpected level | Unexpected relative to the normal V1 mapper; firmware rejection, clamp or side effect is Unknown |
| Exact speed, percent or PWM | Not established; do not convert128 to RPM,128%, or half duty |

The byte-capacity bound of the inert dry-run is not the controller's percent API. Feeding128 to that API would clamp to100 and take a MaxFan path, not serialize80-80. Conversely a direct byte-level wrapper could pass128 unchanged. These distinct entry points explain why "the reference accepts a byte" is not useful safety proof.

**Conclusion remains uncertain, not a selected first-write target.** No decoder, parser range, dry-run record or DeviceValidatedInputLength changed.

## 13. Remaining Conflicts

Two versus four versus128 declared input bytes; third-fan field versus padding; stale setter comments; V1 software default versus physical unit/range; cleaning addition versus arbitrary byte input; mapped requests versus unvalidated raw replay; independent initial pair versus equal maintenance; zero-floor prevention versus low-positive rounding; controller Dispose versus service cleanup. This trace resolves where these differences originate in source, not how F.31 firmware handles them.

## 14. Exact Facts Proven

**Implementation inference, verified in source:** profile resolution logic; V1 default55 absent overrides; percentage clamp/integer scaling; no normal high-bit flag insertion; paired byte assignments; explicit four-byte omencore allocation/Size; caller-local ghelper128 allocation; distinct legacy cleaning command; dual-setter maintenance synchronization; service-level shutdown recovery attempts. These are code facts, not hardware validation.

**Close-device evidence:** F.30/V1/8BD4 reports and conservative zero-handoff profile. **Generic HP evidence:** OmenHub cleaning and indexed target conventions. **Exact target evidence:** existing VictusX inert128/80-80 serialization and separate SetFanMax observations only; none validates executed SetFanLevel.

## 15. Facts Still Unproven

F.31 accepted input length/padding tolerance, actual units/min/max, bit7/sentinel handling, per-physical-fan ordering, independent fan response, input/readback equivalence, trustworthy RPM correlation, silent clamping, effective automatic handoff, suspend/crash/power/service-conflict recovery and thermal safety. FanGetLevel remains raw-only and FanMaxGet inconclusive. NearbyF.30 evidence does not validateF.31.

## 16. First-Write Readiness Decision

**NO-GO; executable candidate set empty.** ABI, physical units/range and recovery are not materially resolved for the target. No first-write numeric value, restore packet, normal fan UI, experiment, write implementation, new permission or fallback is authorized. `DeviceValidatedInputLength` stays null. Existing developer-only four-byte SetFanMax pulse/hold gates are unchanged; their results do not validate SetFanLevel.

## 17. Recommended Next Safe Task

Prepare one documentation-only upstream evidence request using these exact revision/symbol anchors: ask for existing protocol or recorded exact-target evidence explaining input Size/padding, V1 level units/bounds, high-bit handling, and safe auto handoff. Specifically request the rationale for ghelper-omen's128-byte setter versus four-byte replay and clarification of the8BD4 zero-handoff failure. Do not ask for new writes, sweeps, probes or UI exposure. A new runtime experiment is not the next task.

Source-only release-prep remains GO; preview publish remains NO-GO. SetFanLevel first-write and normal/user-facing fan control remain NO-GO.
