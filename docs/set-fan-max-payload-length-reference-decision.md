# SetFanMax Payload-Length Reference Decision

## Decision

**No payload length is selected. Confidence: insufficient.** The reviewed repositories confirm common command metadata, but do not provide an exact-device SetFanMax trace for HP Victus `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31` / V1. `DeviceValidatedInputLength` stays unset and implementation remains **NO-GO**.

## Target and Common Metadata

VictusX has read-only proof of V1, two fans, and `FanMaxGet`. It now also has bounded exact-device physical-response records for both shapes, while their write semantics remain unresolved. All payload-relevant implementations use command `0x20008`, type `0x27`, `hpqBIOSInt0` (directly or through an output-size-derived method name), enable byte `0x01`, and disable byte `0x00`.

## Priority Evidence

| Reference and revision | Relevant files | Payload | Scope and finding |
| --- | --- | --- | --- |
| OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd` | `OmenHardware.cs` | 1 byte | `SetMaxFanSpeedOn/Off` passes `{ 0x01 }` / `{ 0x00 }` to type `0x27`, default command `0x20008`, output size `0`, so its helper derives `hpqBIOSInt0`. The same file has generic V1 detection from SystemDesignData. No `16-s0xxx`, `7Z5Z2EA`, `8BD4`, or F.30/F.31 payload hit was found. |
| OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd` | `Services/HardwareApiService.cs`, `Services/TrayService.cs`, `Pages/FanPage.xaml.cs`, `Services/AutomationProcessor.cs` | 1 byte caller | These expose or repeatedly call the one-byte helper in a broader fan-control/timer flow. They do not add an exact-device match, independent `FanMaxGet` restore verification, or a payload-length validation record. `README.en.md` describes generic OMEN/VICTUS support and says compatibility is not guaranteed on all platforms. |
| OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81` | `OmenHardware.cs` | 1 byte | `SetMaxFanSpeedOn/Off` passes `{ 0x01 }` / `{ 0x00 }`; `SendOmenBiosWmi` uses default command `0x20008`, preserves input `Size`, and derives `hpqBIOSInt0` for output size `0`. The file also uses generic V1 detection from SystemDesignData. No target model/SKU/board/BIOS payload hit was found. |
| OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81` | `Program.Config.cs`, `Program.Menu.cs` | 1 byte caller | Max mode calls enable; auto/manual flows call disable. This is an application control flow, not device-specific restore/readback proof. |

OmenXHub states that it is based primarily on OmenSuperHub. Their agreement therefore is not two independent device-validation records.

## Comparison Evidence

| Reference and revision | Relevant files | Payload | Scope and finding |
| --- | --- | --- | --- |
| omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b` | `src/OmenCoreApp/Hardware/HpWmiBios.cs` | 4 bytes | `SetFanMax` constructs `{ state, 0, 0, 0 }`, uses command `0x20008`, type `0x27`, output size `0`, and an output-size-derived `hpqBIOSInt0` call. `GetFanMax` uses type `0x26` and a four-byte request buffer, but the SetFanMax code is generic rather than model-dispatched. |
| omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b` | `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs` | no payload selection | Closest cohort: board `8BD4`, Victus `16-s0xxx`, V1, two fans, BIOS F.30. The entry is `UserVerified=false` and its notes record conservative handling plus fan-handoff problems. It does not validate F.31 or a SetFanMax input length. |
| omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b` | `src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs`, `docs/3.8.0-BUG-REPORTS.md` | no payload selection | `7Z5Z2EA` / `16-s0035nt` appears for keyboard/RGB routing and reports, not a validated fan write. Its `8BD4` reports warn that V1 fan handoffs can leave fans at high or non-reactive states. |
| ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4` | `app/Omen/HpWmiBios.cs` | 4 bytes | Same generic `{ state, 0, 0, 0 }`, command/type/output method pattern as omencore. `app/Omen/WmiFanController.cs` contains broader control/retry/fallback behavior, which is not safe evidence for VictusX. |
| ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4` | `app/HardwareControl.cs` | none | Mentions Victus `16-s0xxx`, BIOS F.31 only in temperature-zone commentary, not SetFanMax input evidence. |
| g-helper `5c26f5ac970dab9e26347d80976ebf1eece91b1e` | `app/Fan/FanSensorControl.cs` | not applicable | ASUS fan-state code; no HP BIOS SetFanMax command evidence. |
| ECC `5eddf1a3ffd311423be2d4ba7d26f7209c91b033` | no relevant hit | not applicable | No `SetFanMax`, `0x20008`, `0x27`, or `hpqBIOSInt` result was found. |

## Weighted Result

- **One byte:** two priority repositories agree, but OmenXHub derives from OmenSuperHub and both are generic OMEN/VICTUS flows without target identity or BIOS evidence. Confidence: generic-family only.
- **Four bytes:** two generic implementations agree; omencore also has the closest `8BD4` / V1 / F.30 cohort. That cohort is explicitly unverified and its target-SKU records concern keyboard/RGB, not SetFanMax. Confidence: near-device context, not payload validation.
- **Exact-device:** no reference hit proves a SetFanMax input length for `16-s0035nt`, `7Z5Z2EA#AB8`, BIOS F.31, System family `103C_5335M7`, or the supplied HP VICTUS family string.

Exact-device experiments now show one observed one-byte response and two observed four-byte responses; see the [one-byte comparison result](set-fan-max-1byte-comparison-result.md) and [second four-byte result](set-fan-max-second-4byte-confirmation-result.md). FanMaxGet remained false throughout, so this is not a payload-selection consensus. The one-byte and four-byte shapes remain competing research hypotheses, not future defaults.

## Consequences

This evidence is insufficient to change `DeviceValidatedInputLength` or implement normal SetFanMax control. The repeated exact-device four-byte response supports limited controlled developer experiments for four byte. One-byte has only generic-family reference evidence plus user-reported external behavior, so it is permitted only through the separate one-time comparison approval: `--i-approve-one-time-set-fan-max-1-byte-comparison`, alongside every existing experiment, elevation, AC, identity, baseline, and restore gate. This comparison route is not validation, has no fallback, and does not expose normal fan control UI.

## Recommendation

Keep **NO-GO for normal control**. The [payload strategy decision](set-fan-max-payload-strategy-decision.md) defines bounded developer-experiment handling without selecting a default, changing `DeviceValidatedInputLength`, or authorizing user-facing control.
