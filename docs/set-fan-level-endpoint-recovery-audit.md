# SetFanLevel Endpoint And Recovery Audit

The [non-executable harness](set-fan-level-first-write-harness-scaffold.md) enforces the offline 2..99% / raw1..54 equal-pair envelope. It checks recovery-plan declarations only, not recovery effectiveness; this audit's execution NO-GO remains unchanged.

Static review, 2026-09-06. A bounded **offline research envelope** can be defined: integer percentages 2..99, equal fan arguments, default V1 ceiling55, raw1..54 in `[raw, raw, 0, 0]`. This is a source-derived exclusion rule, not an adequate-cooling limit or permission to write. The executable first-write envelope remains empty; first-write and normal fan control remain **NO-GO**.

## Evidence Scope

Primary source: local omencore revision `b39b44978902606aa708cc0d78bcfd87e95fd88b`. Paths below are relative to that repository. Five selected files were reviewed: the controller, BIOS wrapper, model database, fan service, and existing bug report. No reference modification, external code copying, hardware call, or fan CLI execution occurred.

| File / symbol | Finding | Confidence |
| --- | --- | --- |
| `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs:1687`, 8BD4 profile | Victus16-s0xxx/F.30/V1; two zones, independent curves=false, zero-floor clear=false, UserVerified=false | Close-device evidence |
| `docs/3.6.3-BUG-REPORTS.md:583`, DC-Victus16-8BD4 | Historical stopped/non-reactive fans after zero handoff; high-state persistence with repeated writes | Close-device evidence; report summary, not independently reproduced |
| `src/OmenCoreApp/Hardware/WmiFanController.cs:212`, MapFanPercentToWmiLevel | Integer scale with special100 return | Implementation inference |
| Same file:620, SetFanSpeed | Exact0 redirects to restore;100 uses MaxFan then level100 fallback; intermediate values use mapper | Implementation inference |
| Same file:1001,1093,1169,2077 | Restore, max reset, broad reset, and conditional zero-floor clearing | Implementation inference |
| `src/OmenCoreApp/Hardware/HpWmiBios.cs:1012,1052,1206,2711` | Mode, level, max, and countdown replay serialization | Implementation inference |
| `src/OmenCoreApp/Services/FanService.cs:3254`, Dispose | Application service attempts restore then broader reset before stopping | Implementation inference |

F.30 is close-device evidence, not validation of the target Victus16-s0035nt/SKU7Z5Z2EA#AB8/F.31. Current source includes mitigations for historical failures; it is not claimed to be the historical failing executable. Secondary repository conflicts remain documented in the [deep trace](omencore-victus-16s-setfanlevel-deep-trace.md); no new secondary-source validation is claimed here.

## Endpoints And Range

`DetectMaxFanLevel` in `HpWmiBios.cs:962` selects55 for V1 absent user/model overrides. `MapFanPercentToWmiLevel` clamps percent to0..100, returns constant100 at100%, otherwise performs integer `percent * clamp(maxFanLevel,1,100) / 100`.

The explicit100 branch preserves a full-speed request beyond the ordinary model scale. `SetFanSpeed:650` comments describe MaxFan as bypassing BIOS power limits, with protocol-ceiling100 fallback left for firmware to clamp. Those are upstream design claims, not proof of F.31 power behavior or a validated firmware sentinel. In this public controller path100% first attempts SetFanMax(true), and calls SetFanLevel(100,100) only if that fails. The pure mapper itself can return100 directly to other callers, including curve/reapply paths.

| Integer percent | Raw with default55 | Meaning established here |
| --- | --- | --- |
| 0 | 0 in mapper only | Public setter redirects to auto recovery before mapping |
| 1 | 0 | Low positive truncation bypasses the exact0 redirect |
| 2 | 1 | Lowest nonzero mapping: ceil(100/55)=2 |
| 25 | 13 | Ordinary integer scaling |
| 50 | 27 | Ordinary integer scaling; 1B-1B-00-00 |
| 99 | 54 | Highest ordinary intermediate value |
| 100 | 100 | Special endpoint; distinct MaxFan/fallback control path |

For1..99%, mapped raw range is0..54; excluding raw0 leaves2..99% ->1..54. Raw55..99 is never produced by this ordinary default55 mapper. This statement does not cover configured ceilings, direct byte setters, transition hints or raw-readback replay. It is not a declaration that firmware rejects55..99.

### What Raw Zero Means

Source comments conflict: `ResetFromMaxMode` describes V1 zero as BIOS takeover, while `SetFanSpeed` warns manual0 can stop fans and prevent recovery, and the8BD4 profile explicitly disables zero-floor clearing after reported non-reactive/0 RPM behavior. V2 comments describe zero as manual0% duty. These comments do not establish a universal zero sentinel.

For this target, raw0 means **unvalidated and excluded**, not auto, minimum, safe off, or a restore packet. The close-device failure report is stronger reason for caution than a generic comment claiming takeover. The percent1 truncation remains a source-level hole in an otherwise exact-zero guard. FanGetLevel remains raw-only; neither zero readback nor similar numbers prove input/output equivalence.

## Recovery Order In The Closest Controller

These are descriptions of reference calls, not an approved VictusX recovery script. Under the default V1 path, write metadata uses `root\wmi`, `hpqBIntM`, command0x20008. Mode/level/max setters request output0 (`hpqBIOSInt0`).

| Stage | Order and conditions | Serialized write / limitation |
| --- | --- | --- |
| RestoreAutoControl entry | Stop countdown extension first, even before availability check; unavailable returnsfalse | Stops scheduling; does not prove firmware handoff or completion of an in-flight call |
| Optional ResetFromMaxMode | Runs if software tracks manual/max and five-second reset cooldown permits | Ordinary successful nonmax manual writes also qualify |
| Reset step1 | Only when software tracks max: MaxFan off, then25ms | type0x27, 00-00-00-00; skipped for ordinary nonmax manual state |
| Reset step2 | SetFanMode(Default), then25ms | type0x1A, FF-30-00-00 |
| Reset step3 | With ceiling<100: SetFanLevel(20,20), then ClearV1AutoModeFloor | type0x2E, 14-14-00-00; raw20 is a source transition hint, not20% or validated RPM |
| Reset finish | Wait50ms; internal failures can be logged/swallowed | Reset helper returnsvoid; its completion log is not proof all writes succeeded |
| Outer restore | SetFanMode(Default) again; ConfirmFanModeReadback; clear manual/max/last-target software state; ClearV1AutoModeFloor again | Another FF-30-00-00. Successful return is weaker than independent physical recovery |
| Zero-floor helper | Returns immediately when disabled on conservative8BD4 profile | No zero write for this profile. Other enabled V1 profiles wait50ms then write00-00-00-00 |

If cooldown or software state skips the full reset, outer restore still attempts the default mode. `ConfirmFanModeReadback:2022` accepts command results if EC access is absent or strict checking is disabled; therefore this check does not prove effective auto mode. Its EC-read branch must not be imported into VictusX.

### Shutdown And Broader Reset

`FanService.Dispose:3254`, when writes are available, calls serialized auto restore, waits100ms, then calls `ResetEcToDefaults`; afterwards it disables curves and stops the service. Controller Dispose alone does not supply this recovery. Forced termination/crash can bypass the service sequence.

`ResetEcToDefaults:1169` in the WMI controller attempts, in order:

1. MaxFan off; wait50ms.
2. Default fan mode; wait50ms.
3. For V1, raw20/20 transition hint; wait50ms.
4. ExtendFanCountdown; wait50ms.
5. SetPerformanceMode("Balanced"); wait100ms.
6. Default fan mode again, then clear software state.

The Balanced method (`WmiFanController.cs:911`) maps to SetFanMode(Default), optionally releases a still-tracked max latch, and stops countdown extension. It is not a direct0x37 power write in this traced method. Broader performance writes remain outside VictusX permission.

`ExtendFanCountdown` (`HpWmiBios.cs:2711`) reads levels and replays both raw bytes in a four-byte0x2E request; if unavailable/unsuccessful it tries SetIdleMode(false), type0x31. This is an additional fallback with no validated recovery meaning. Despite the reset method's name, this WMI implementation does not directly write EC registers; its BIOS requests and fallback are still forbidden to adopt here. Several boolean results are ignored, so a true broad-reset result is not comprehensive success proof.

## Proposed Offline Envelope And Residual Risk

The source supports studying integer **2..99%**, equal fan1/fan2 arguments, fixed reference ceiling55, and four-byte `[floor(p*55/100), floor(p*55/100), 0, 0]`. CPU/GPU labels describe the upstream arguments; physical ordering and sufficient cooling for both fans are unproven. This envelope is documentation for evidence review, not a new dry-run parser limit, sweep, recommended runtime range, or first-write selection.

Explicitly exclude percent0/1/100, raw0 and raw55..255 (including100 and128), asymmetric pairs, high-bit/cleaning behavior, alternate ceilings/lengths, automatic preconditioning, readback replay, retries, curves, background resends, and EC/PawnIO fallback. Ordinary raw1..54 can still be unsafe: excluding endpoints alone does not prove a minimum running speed, adequate cooling, or a reliable exit from manual policy.

Evidence that blocks even this prospective envelope includes the8BD4 reports of fans failing to start until emergency protection, persistent levels28..30 during repeated writes, the profile's UserVerified=false, absent F.31 ABI/units/physical fan mapping, uncertain default-policy ownership, no independent success signal, and unproven recovery during crash/sleep/power/service conflicts. The reference's claimed120-second timeout and VictusX SetFanMax hold observations do not prove a SetFanLevel fail-safe timeout.

## Decision And Next Research

Request existing exact-target evidence for a durable firmware-auto handoff and minimum stable nonzero behavior, with identity, starting state, exact command order, independent thermal/fan observations, failure results and source revision. Review that evidence before designing any implementation or execution request. No exact first-write value or restore packet is selected.

SetFanLevel first-write: **NO-GO**. Normal/user-facing fan control: **NO-GO**. `DeviceValidatedInputLength=null`; FanGetLevel raw-only; FanMaxGet inconclusive. Existing developer-only SetFanMax CLI permissions are unchanged. No product code changed.
