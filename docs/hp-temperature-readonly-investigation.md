# HP Temperature Read-Only Investigation

## Scope

This is a documentation-only review. It does not prepare a temperature command for invocation, call WMI, add telemetry code, or authorize any fan, performance, power, EC, BIOS, or hardware write.

Reference commits reviewed: ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4`, omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b`, OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81`, and OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`.

## Reference Evidence

The reviewed projects use command type `0x23` as a temperature/sensor getter. ghelper-omen and omencore call it with `[01, 00, 00, 00]` for a CPU-labelled value and `[02, 00, 00, 00]` for a GPU-labelled value. OmenSuperHub and OmenXHub use the same command as a generic sensor selector; OmenSuperHub labels selectors `0` through `3` as IR/internal board, ambient/internal air, PCH, and VR respectively.

All reviewed uses are getter-shaped: default command `0x20008`, four-byte selector input, four-byte output through `hpqBIOSInt4`, and inspection of output byte zero. No `0x23` write/control use was found in the reviewed files.

## Sensor Meaning And Ambiguity

The transport is plausibly read-only, but selector meaning is not stable across references. The CPU/GPU labels from ghelper-omen and omencore conflict with the board-sensor labels from OmenSuperHub. OmenXHub also derives an estimated CPU temperature from selector `1`, rather than treating that value as an unambiguous CPU reading.

No exact `HP Victus Gaming Laptop 16-s0035nt`, BIOS `F.31`, ThermalPolicyVersion `1` result was found. A value from `0x23` must therefore not be labelled CPU, GPU, ambient, PCH, VR, RPM, or a thermal-control state for this device. Bytes one through three are also not sufficiently documented and would need unknown-byte handling.

## Risks

- A plausible-looking raw value could be a board or ambient sensor and be falsely presented as CPU or GPU temperature.
- Reference projects mix BIOS WMI sensor reads with other telemetry paths; their UI labels are not device-specific proof.
- Temperature values can be stale, filtered, or sourced from a different sensor than a modern system telemetry tool.
- A read-shaped command is not a reason to add polling, control behavior, fan automation, or performance policy changes.

## Decision

**Defer preparation.** `0x23` is a strong documentation candidate for future raw-first investigation, but it is not safe to prepare for invocation until this Victus has selector-specific evidence. No command-catalog metadata, decoder, report wiring, or test-path changes are authorized now.

## Required Evidence Before Future Preparation

Collect documentation or manually recorded evidence for this exact model/BIOS that identifies the selector, returned byte count, all four raw output bytes, and an independently observed reference temperature under a stable non-critical condition. The evidence must distinguish CPU/GPU labels from ambient, PCH, VR, and internal-board alternatives before any user-facing sensor name is used.

## Still Forbidden

Temperature-command invocation, `--hp-wmi-readonly-test`, temperature polling, performance or GPU power control, fan control, fan writes, power-limit writes, `SetFanMax`, `SetFanMode`, `SetFanLevel`, `0x37`, EC access, BIOS writes, hardware writes, and control UI remain forbidden.

## Recommended Next Safe Task

Create a documentation-only `0x23` selector evidence matrix for the exact Victus model and BIOS. It should define the evidence required to distinguish raw selector values without adding an invocation path.
