# HP Temperature And Fan RPM Telemetry

## Implementation And Discovery Decision

### CPU Source Follow-Up

The follow-up CPU-only review found no trustworthy CPU-package source already wired into this HP runtime. CPU temperature therefore remains **Unavailable**, as required when source identity is ambiguous. No speculative sensor provider, CPU polling task or dependency is added.

| Candidate | Evidence and disposition |
| --- | --- |
| Windows WMI | Existing `System.Management` queries provide transport, not proof of sensor identity. `MSAcpi_ThermalZoneTemperature` describes a thermal zone; no exact-target CPU-package mapping is recorded. Rejected for CPU labeling. |
| PerformanceCounter | Inherited HardwareControl.GetCPUTemp reads Thermal Zone Information/Temperature for `\\_TZ.THRM`; the instance name and Kelvin conversion do not prove CPU-package identity. CPU utilization counters are not temperature sensors. |
| Inherited sensor code | HardwareControl.GetCPUTemp additionally uses ASUS ACPI; GetCPUTempWMI selects the Qualcomm `ACPI\\QCOM0C5A\\1_0` instance. Neither is an established source for this AMD Victus. These helpers remain outside the HP telemetry path. |
| HP BIOS selector | omencore HpWmiBios.GetTemperature (same revision listed below) reads 0x23 with `[01,00,00,00]`, accepts byte zero in 1..109, and labels it CPU. The [existing thermal investigation](hp-temperature-readonly-investigation.md) records contradictory ambient/board labels in other references. Numeric plausibility does not resolve that conflict for F.31. No new BIOS query is implemented or invoked. |
| LibreHardwareMonitor | No package reference or source usage was found in the VictusX application search. Reference worker/library stacks are not an already available HP CPU provider; no monitor/driver is installed or opened. No external running sensor service is assumed. |
| NVIDIA NVAPI | Existing dependency supplies GPU-target temperature only; it is not a CPU source. Existing optional GPU reads are unchanged. |

The reference WmiBiosMonitor also explicitly tracks frozen AMD BIOS readings. Re-reading a value cannot prove that the firmware refreshed it; neither a successful query nor an in-range number establishes CPU-package accuracy. This review does not claim every possible Windows/third-party CPU source is unavailable, only that the inspected existing sources do not meet the identity requirements.

No valid real CPU sample exists to exercise a numeric CPU display path. Regression tests instead enforce unavailable output for fresh/stale/future OS snapshots and plausible or invalid untyped cached temperatures. Existing hidden-window stop/reset, stale GPU tests and source-boundary tests continue to apply; no additional polling is introduced. A future implementation needs exact-target sensor identity and freshness evidence or a separately reviewed driver-free sensor feed before wiring a numeric CPU field.

This is a partial read-only milestone: optional NVIDIA GPU temperature is implemented. CPU temperature and Fan 1 / Fan 2 RPM remain unavailable on this V1 target because the inspected sources do not establish safe, reliable readings. No HP BIOS method, new dependency, driver installation, fan command, UI control or control-enabling change is added. DeviceValidatedInputLength remains null; normal fan control remains NO-GO.

| Value | Source class / decision |
| --- | --- |
| CPU load and battery/AC | Existing Windows-native GetSystemTimes/GetSystemPowerStatus; unchanged |
| CPU temperature | Unavailable. Inherited HardwareControl.GetCPUTemp uses ASUS ACPI or a named thermal-zone counter; its WMI alternative targets a Qualcomm ACPI instance. Neither proves CPU-package identity on this AMD Victus. Generic thermal zones are not relabeled CPU temperature. |
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
| g-helper 5c26f5ac970dab9e26347d80976ebf1eece91b1e | app/Gpu/NVidia/NvidiaGpuControl.cs, ReadCurrentTemperature | Uses GPU-target NVAPI thermal sensor; the same getter already exists in VictusX. Only the narrow getter approach is reused, not the write-capable controller or cached-value behavior. |

HP WMI getters, Windows thermal-zone queries and library/driver sensors are distinct source categories. A getter name alone does not prove exact-device interpretation. See [existing RPM investigation](fan-get-rpm-readonly-investigation.md). CPU/fan driver-backed discovery or any alternative HP RPM command needs separate evidence review; none is silently probed here.

## Polling And Failure Behavior

The existing visible-window one-second UI timer continues OS status sampling. GPU reads use a single worker with a minimum two-second launch interval, no overlapping calls and no UI wait. A hung native getter occupies that one worker slot; no replacement workers accumulate. The display expires its last value after five seconds. An in-flight native call cannot be forcibly cancelled and may finish after hiding; Reset discards its result and no new reads start while the window is hidden.

GPU sample age is measured from read start, separately from OS poll time, so repeated UI refresh cannot make an old GPU result fresh. Exceptions, missing sensors, non-finite values, zero and values outside (0,125] Celsius fail closed. These are display sanity bounds, not thermal safety thresholds. Polling NVAPI may wake an otherwise idle GPU and affect battery use; no zero-power-overhead claim is made.

The main GPU label changes from Unavailable only when a valid fresh sample exists. Layout and disabled controls are unchanged; Diagnostic includes source and unavailable reasons. No two-fan numbers are shown without a real RPM source. No AMD temperature fallback or external sensor daemon is installed or assumed.

## Verification And Next Evidence

Build passed with four existing NU1900 warnings; tests passed 341/341 using test-double reads only. Tests cover valid/invalid temperatures, independent freshness, bounded concurrency, hidden reset, exception recovery, raw-only fan behavior and UI/source safety boundaries. No app launch, native sensor read, HP diagnostic probe or experiment was performed for this task; exact-device runtime temperature availability remains unconfirmed.

Next: observe the normal HP shell on the target to confirm the optional NVIDIA reading and its power impact. CPU-package and actual V1 fan-tachometer source identity remain separate discovery gaps, not permission to add a driver or probe unsupported commands.
