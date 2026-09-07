# Reference Fan Write Flow Comparison

## Scope

This comparison is research only. No reference code is copied and VictusX has no fan-write implementation.

Reviewed references and revisions:

- ghelper-omen: `1694844d2725e79a2b2065a0a1494fa1d143e3f4`
- omencore: `b39b44978902606aa708cc0d78bcfd87e95fd88b`
- OmenSuperHub: `a6ab6988c446ee5421466097fdf60c0d521e5c81`
- OmenXHub: `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`

## Reference Flows

| Reference | Fan control path | SetFanMax shape | Other fan writes | Restore/readback |
| --- | --- | --- | --- | --- |
| ghelper-omen | Direct `root\wmi` `hpqBIntM` call through `hpqBIOSInt0` for writes. | Command `0x20008`, type `0x27`, output `0`, four bytes: `{state, 0, 0, 0}`. | `SetFanMode` `0x1A` with `{0xFF, mode, 0, 0}`; `SetFanLevel` `0x2E` uses a zero-padded 128-byte buffer. `0x37` is treated as V2 fan-level read on newer OMEN Max paths and also as a power-limit command. | Has `FanMaxGet` `0x26`; command methods check WMI result, but a complete Victus-specific restore protocol is not established by this source alone. It also describes periodic fan-level reissue/heartbeat behavior on some newer models. |
| omencore | Direct WMI wrapper plus a stateful `WmiFanController`, model/capability logic, command history, and maintenance/reapply behavior. | Same `0x20008` / `0x27` / output `0` four-byte shape. | Same `0x1A` mode and `0x2E` level concepts. `0x37` is V2-read/power-command territory, not a safe generic fan-write command. | Stronger controller-level behavior: reads before/after selected operations, `SetFanMax(false)` and `SetFanMode(Default)` in restore paths, plus repeated ownership/health logic. It may fall back to EC controllers on supported systems. |
| OmenSuperHub | Direct `System.Management` WMI helper, `hpqBIOSInt{outputSize}`. | `0x20008`, type `0x27`, output `0`, one byte: `{state}`. | `SetFanMode` `0x1A` with `{0xFF, mode}`; `SetFanLevel` `0x2E` with two or three level bytes. `0x37` is used for LoadLine/IccMax/power work, not fan control. | Exposes one-byte max on/off helpers and read commands, but no comprehensive guarded restore transaction is evident in the direct helper. |
| OmenXHub | Direct WMI helper similar to OmenSuperHub, with application-level fan services. | `0x20008`, type `0x27`, output `0`, one byte: `{state}`. | `0x1A` with `{0xFF, mode}`; `0x2E` with two or three levels. If WMI level write fails, it can use an `EcFanService` fallback. `0x37` is used for non-fan power/load-line operations. | Includes max-off calls during some mode transitions and service-level handling, but EC fallback and continuous control are outside VictusX's safety boundary. |

## What The References Demonstrate

- The same HP WMI transport convention appears repeatedly: `root\wmi`, `hpqBDataIn`, `hpqBIntM`, and `hpqBIOSInt0` for zero-output writes.
- `SetFanMax` is commonly associated with command `0x20008`, type `0x27`, and first byte `0x01` to enable or `0x00` to disable.
- A one-byte payload is used by OmenSuperHub/OmenXHub, while a four-byte zero-tailed payload is used by ghelper-omen/omencore. This is reference evidence, not device validation.
- `SetFanMode` (`0x1A`) and `SetFanLevel` (`0x2E`) are broader manual-control paths, not substitutes for a minimal max-fan experiment.
- `0x37` is overloaded across references for V2 probing and power-related operations. It is ambiguous and must remain forbidden.
- Some references rely on model databases, repeated writes, capability heuristics, proprietary/third-party support layers, or EC fallback. Those assumptions are OMEN-specific and unsafe to transplant.

## Safe Concepts To Copy Later

- Explicit capability gating, elevation checks, command/result logging, and a bounded single-operation experiment.
- Pre-read and post-read using the already proven `FanMaxGet` read-only command.
- A defined restore action before any enable attempt, plus abort on missing readback or nonzero BIOS return.
- Keeping command payload description, preflight, and runtime execution as separately reviewed layers.

## What VictusX Must Avoid

- Copying an unvalidated one-byte or four-byte payload choice.
- Enabling `SetFanMode`, `SetFanLevel`, `0x37`, fan curves, periodic reassertion, or background ownership logic.
- EC access, `EcFanService`-style fallback, drivers, vendor binaries, or any automatic recovery write.
- Treating firmware's software-fan-control declaration or another project's model support as proof for this Victus.

## Why VictusX Is Still NO-GO

VictusX has proven read-only telemetry and `FanMaxGet` readback, but it has not proven this device's accepted `SetFanMax` input length, enable behavior, restore behavior, manual recovery route, thermal observation plan, or required human approval. Reference success does not establish firmware compatibility for HP Victus 16-s0035nt / 7Z5Z2EA.

## Exact Missing Evidence Before A Guarded First Write

1. Device-specific validation selecting exactly one input length: one or four bytes.
2. Human-approved, elevated pre-read showing max fan disabled.
3. A one-time enable test with bounded thermal observation and an explicit abort threshold.
4. `FanMaxGet` readback proving the requested state, followed by restore/disable and another readback proving disabled.
5. A tested manual recovery path if restore fails, including app exit, reboot, and any applicable firmware recovery guidance.
6. Report evidence for every stage; no UI control or repeated writes.

## Recommended Next Safe Step

Keep the implementation gate at NO-GO. Review and approve the existing missing-proof tracker and recovery/restore plan with a human before any guarded runtime write design is considered. No WMI call or payload test should be added until the device-specific input length and restore evidence are available.
