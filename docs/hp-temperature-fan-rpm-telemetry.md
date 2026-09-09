# HP Temperature And Fan RPM Telemetry

## Implementation And Discovery Decision

### CPU Source Follow-Up

The original CPU-only review found no trustworthy CPU-package source already wired into this HP runtime, so CPU temperature remained **Unavailable** until exact-device evidence existed. That evidence is now complete: two bounded elevated runs on the exact target returned 20/20 fresh `Core (Tctl/Tdie)` samples (33.750–38.375 C) through the narrow PawnIO path. The normal HP Diagnostic now shows that value only when a fresh validated sample is available; unavailable, access-denied, invalid, and stale samples fail closed.

| Candidate | Evidence and disposition |
| --- | --- |
| Windows WMI | Existing `System.Management` queries provide transport, not proof of sensor identity. `MSAcpi_ThermalZoneTemperature` describes a thermal zone; no exact-target CPU-package mapping is recorded. Rejected for CPU labeling. |
| PerformanceCounter | Inherited HardwareControl.GetCPUTemp reads Thermal Zone Information/Temperature for `\\_TZ.THRM`; the instance name and Kelvin conversion do not prove CPU-package identity. CPU utilization counters are not temperature sensors. |
| Inherited sensor code | HardwareControl.GetCPUTemp additionally uses ASUS ACPI; GetCPUTempWMI selects the Qualcomm `ACPI\\QCOM0C5A\\1_0` instance. Neither is an established source for this AMD Victus. These helpers remain outside the HP telemetry path. |
| HP BIOS selector | omencore HpWmiBios.GetTemperature (same revision listed below) reads 0x23 with `[01,00,00,00]`, accepts byte zero in 1..109, and labels it CPU. The [existing thermal investigation](hp-temperature-readonly-investigation.md) records contradictory ambient/board labels in other references. Numeric plausibility does not resolve that conflict for F.31. No new BIOS query is implemented or invoked. |
| LibreHardwareMonitor / PawnIO lineage | VictusXHub does not initialize LibreHardwareMonitor. The CLI probe and production typed provider share the official PawnIO.Modules 0.2.2 `AMDFamily17.bin` path so `RyzenSMU` PM-table initialization is impossible. The exact CPU gate is Family 19h, decoded CPUID Model 74h, Stepping 1. Identity is read before PawnIO through native RSMB where available, then the read-only BIOS registry fallback; WMI denial is not itself a failure. The provider opens only the already-installed PawnIO device, loads the verified module, calls only `ioctl_read_smn` for `THM_TCON_CUR_TMP` (`0x00059800`), and applies the current LHM `Core (Tctl/Tdie)` decode. PawnIO grants SYSTEM/elevated Administrators; normal medium-integrity sessions receive Win32 `5` and display neutral Unavailable without prompting or retrying per frame. |
| NVIDIA NVAPI | Existing dependency supplies GPU-target temperature only; it is not a CPU source. Existing optional GPU reads are unchanged. |

The reference WmiBiosMonitor also explicitly tracks frozen AMD BIOS readings. Re-reading a value cannot prove that the firmware refreshed it; neither a successful query nor an in-range number establishes CPU-package accuracy. This review does not claim every possible Windows/third-party CPU source is unavailable, only that the inspected existing sources do not meet the identity requirements.

The production provider owns a typed session for its lifetime, serializes reads, disposes it on reset/shutdown, and retries a failed initialization no faster than the five-second freshness window. A successful sample carries its own timestamp; a later read failure, invalid value, or stale sample is immediately Unavailable rather than retaining an old number. Existing hidden-window reset, stale GPU tests, and source-boundary tests continue to apply; no background worker is introduced.

This is a read-only telemetry milestone: optional NVIDIA GPU temperature and exact-device CPU temperature are implemented. Fan 1 / Fan 2 RPM remains unavailable on this V1 target. The PawnIO path adds no driver installation, HP BIOS method, fan command, EC access, UI control, background worker, or control-enabling change. DeviceValidatedInputLength remains null; normal fan control remains NO-GO.

| Value | Source class / decision |
| --- | --- |
| CPU load and battery/AC | Existing Windows-native GetSystemTimes/GetSystemPowerStatus; unchanged |
| CPU temperature | Exact-target `Core (Tctl/Tdie)` only, through the verified PawnIO AMDFamily17 module and fixed SMN read. A fresh valid elevated-session sample displays in the Diagnostic; normal non-elevated access denial, invalid data, or staleness displays Unavailable. Inherited ASUS/ACPI/WMI thermal sources remain excluded. |
| GPU temperature | Existing NvAPIWrapper.Net 0.8.1.101, through installed NVIDIA display driver. Separate HpNvidiaTemperatureSource performs only physical-device enumeration and GetThermalSettings. Exactly one device and one ThermalSettingsTarget.GPU sensor are required; ambiguous, non-NVIDIA, absent or failed sources remain unavailable. No NvidiaGpuControl instance is created. |
| Fan RPM | Unavailable. No proven V1 tachometer source was found. FanGetLevel remains raw-only; no multiplication, endian guess or conversion to RPM is adopted. |
| FanGetRpm 0x38 | Not implemented or invoked. Reference generation gating restricts this to V2; this target is V1. Plausible values would not prove correct interpretation. |

## Selected Reference Evidence

References were read-only; no code was copied. Paths below are relative to the named reference repository.

| Repository / revision | Selected path / symbol | Finding |
| --- | --- | --- |
| omencore b39b44978902606aa708cc0d78bcfd87e95fd88b | src/OmenCoreApp/Hardware/HpWmiBios.cs, GetFanRpmDirect | 0x38 described as V2 direct RPM; BIOS response parsing is not F.31/V1 validation. |
| ghelper-omen 1694844d2725e79a2b2065a0a1494fa1d143e3f4 | app/Omen/WmiFanController.cs, GetFanSpeeds | Explicit V2 gate and warning that V0/V1 can produce phantom RPM-shaped readings. |
| OmenSuperHub a6ab6988c446ee5421466097fdf60c0d521e5c81 | Program.cs, RunHardwareMonitor | Opens LibreHardwareMonitor and selects CPU Package/Tctl/Tdie and GPU Core sensors. That monitoring stack is not adopted; bundled low-level dependencies are outside this change. |
| OmenXHub ca84cb011d1d3e5850445d19e45fbea06e83a8fd | Services/HardwareApiService.cs, HandleGetTemperature/HandleGetFanRpm | Delegates temperatures to HardwareService; its RPM endpoint multiplies FanSpeedNow by 100. This endpoint is not independent proof of raw fan units on Victus F.31. |
| g-helper 5c26f5ac970dab9e26347d80976ebf1eece91b1e | app/Gpu/NVidia/NvidiaGpuControl.cs, ReadCurrentTemperature | Uses GPU-target NVAPI thermal sensor; the same getter already exists in VictusXHub. Only the narrow getter approach is reused, not the write-capable controller or cached-value behavior. |

HP WMI getters, Windows thermal-zone queries and library/driver sensors are distinct source categories. A getter name alone does not prove exact-device interpretation. See [existing RPM investigation](fan-get-rpm-readonly-investigation.md). CPU/fan driver-backed discovery or any alternative HP RPM command needs separate evidence review; none is silently probed here.

## Polling And Failure Behavior

The existing visible-window one-second UI timer continues OS status sampling. GPU reads use a single worker with a minimum two-second launch interval, no overlapping calls and no UI wait. A hung native getter occupies that one worker slot; no replacement workers accumulate. The display expires its last value after five seconds. An in-flight native call cannot be forcibly cancelled and may finish after hiding; Reset discards its result and no new reads start while the window is hidden.

GPU sample age is measured from read start, separately from OS poll time, so repeated UI refresh cannot make an old GPU result fresh. Exceptions, missing sensors, non-finite values, zero and values outside (0,125] Celsius fail closed. These are display sanity bounds, not thermal safety thresholds. Polling NVAPI may wake an otherwise idle GPU and affect battery use; no zero-power-overhead claim is made.

The main GPU label changes from Unavailable only when a valid fresh sample exists. Layout and disabled controls are unchanged; Diagnostic includes source and unavailable reasons. No two-fan numbers are shown without a real RPM source. No AMD temperature fallback or external sensor daemon is installed or assumed.

## Verification And Next Evidence

The exact target has passed two bounded physical validation runs: 20/20 fresh samples, 33.750–38.375 C, with the verified module hash, no RyzenSMU, no EC/fan/control command, and no WinRing0. Automated tests cover exact gating, access-denied/module/read failure, invalid values, freshness, disposal, fixed-address structure, neutral unavailable UI behavior, and copy/export. CPU integration does not validate or alter V1 fan RPM; RPM remains a separate gap and not permission to add a driver, probe EC registers, or invoke unsupported commands.
