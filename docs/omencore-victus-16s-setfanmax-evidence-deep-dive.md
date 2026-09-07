# omencore Victus 16-s0xxx SetFanMax Evidence Deep Dive

## Files Reviewed

Reference commit: `b39b44978902606aa708cc0d78bcfd87e95fd88b`.

- `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`
- `src/OmenCoreApp.Tests/Hardware/ModelCapabilityDatabaseTests.cs`
- `src/OmenCoreApp/Hardware/HpWmiBios.cs`
- `src/OmenCoreApp/Hardware/WmiFanController.cs`
- `src/OmenCoreApp/Hardware/FanControllerFactory.cs`
- `src/OmenCoreApp.Tests/Hardware/WmiFanControllerPerformanceModeMaxReleaseTests.cs`
- `src/OmenCoreApp.Tests/Hardware/WmiFanControllerMaxModeHealthTests.cs`

## Closest Cohort Evidence

omencore defines product ID `8BD4` as **HP Victus 16-s0xxx AMD**, with BIOS `F.30`, V1 WMI fan control, two fan zones, and EC fan control disabled. The profile is explicitly conservative and `UserVerified=false`. Its notes mention `7Z5Z2EA` only for keyboard RGB evidence, not for SetFanMax payload validation. No `F.31` SetFanMax result was found.

The profile also disables V1 automatic-mode floor clearing after reports of fans remaining at maximum or becoming non-reactive after `SetFanLevel(0,0)`. This supports caution; it does not validate SetFanMax.

## SetFanMax Shape

omencore's shared `HpWmiBios.SetFanMax` implementation uses:

- WMI namespace/class: `root\wmi` / `hpqBIntM`
- Input class: `hpqBDataIn`
- Command: `0x20008` (`BiosCmd.Default`)
- Command type: `0x27`
- Input size: `4`
- Enable data: `01-00-00-00`
- Disable data: `00-00-00-00`
- Output size/method: `0` / `hpqBIOSInt0`

This is a generic implementation. No code or field record ties a successful four-byte SetFanMax write specifically to `8BD4`, Victus 16-s0xxx, BIOS `F.30`, SKU `7Z5Z2EA`, or BIOS `F.31`.

## Surrounding Flow

The factory selects the WMI controller for the `8BD4` profile because WMI fan control is enabled and EC fan control is disabled. The controller can set max fan, fan mode, and fan level through the shared BIOS WMI wrapper. Performance-mode transitions release an active max-fan latch with `SetFanMax(false)`.

Restore logic is multi-step: disable max fan, restore default fan mode, and, for V1, conditionally apply/reset a manual fan floor. The conservative `8BD4` profile prevents the risky V1 zero-floor clear. Tests verify controller call ordering with fakes; their cited hardware cases are other boards and do not prove the payload on this cohort.

`FanMaxGet` provides a readback API, but the reviewed SetFanMax flows do not document an `8BD4` hardware result containing before, enabled, restored, and final readbacks. omencore supports both WMI and EC paths generally; the closest `8BD4` cohort is configured for BIOS WMI only.

## Comparison With This Victus

Similarities:

- Same Victus 16-s0xxx family naming.
- V1 thermal/fan policy.
- Two reported fans/zones.
- SKU family includes `7Z5Z2EA`.
- This device's `FanMaxGet` and `FanGetLevel` read-only queries work.

Differences and gaps:

- omencore records BIOS `F.30`; this device uses `F.31`.
- The current device's product/board ID has not been matched to omencore's `8BD4` evidence here.
- omencore marks the profile unverified and conservative.
- Its `7Z5Z2EA` evidence concerns RGB, not fan writes.
- No exact-cohort command trace proves input `Size=4`, latch behavior, or restore behavior.

## Decision

The evidence is **not enough** to choose one byte or four bytes. It makes four bytes a reference-backed implementation candidate, but not a device-validated ABI. `DeviceValidatedInputLength` remains unset and SetFanMax remains **NO-GO**.

## Current Recommendation

Keep write code absent. The next safe task is to obtain an independently reviewable, sanitized field record for the exact `8BD4`/16-s0xxx cohort on `F.30` or `F.31` that records command type `0x27`, input size, return code, and `FanMaxGet` before/after/restore results. Without that evidence, neither payload shape should be approved.
