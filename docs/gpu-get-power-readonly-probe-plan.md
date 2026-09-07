# GpuGetPower Raw Read-Only Probe Plan

## 1. Reference Evidence

ghelper-omen, omencore, and OmenXHub each expose a getter paired with GPU power-policy writes. The getter is named `GetGpuPower` or `GetGpuPowerState` and uses command type `0x21`; its paired write is `SetGpuPower` / `SetGpuPowerState` on `0x22`.

The references use the getter to report configuration flags and, in some cases, to check a prior write. That pairing does not authorize the write path in VictusX.

## 2. Read-Only Assessment

`GpuGetPower` appears read-shaped in the reviewed references: it supplies a zero-filled input and requests a four-byte reply. No reviewed implementation passes a target state, limit, mode, or other mutation payload to `0x21`.

This is sufficient to study it as a future raw-first read-only candidate, but not to invoke it yet. No exact `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31` result is available, so the device-specific reply semantics remain unproven.

## 3. Expected Transport Shape

Expected reference shape:

| Item | Expected value |
| --- | --- |
| Command | Default `0x20008` |
| Command type | `0x21` |
| Input | `00-00-00-00` |
| Input size | `4` |
| Output method | `hpqBIOSInt4` |
| Output size | `4` bytes |

ghelper-omen and omencore expose byte 0 as a custom-TGP flag, byte 1 as PPAB enabled/level, and byte 2 as D-state. OmenXHub additionally labels byte 3 as a GPU slowdown-temperature field. These are reference interpretations only, not a VictusX device contract.

## 4. Raw-First Handling

Any future decoder must first preserve the four returned byte values and byte count without assuming that a nonzero flag has a universal meaning. It must:

- show unavailable, short, or malformed output safely;
- expose only reference-backed labels with an explicit provisional/raw status;
- retain unknown or disputed fields as summarized raw bytes;
- avoid raw binary dumps in logs or exports.

It must not calculate wattage, GPU boost, temperature, performance tier, or a hardware-control recommendation from this reply.

## 5. What It May Tell Us

If the expected reply is returned, this probe may establish that the firmware exposes a four-byte GPU power-policy configuration status block. It may allow a future diagnostic report to show raw/reference flag states and compare successive manual captures.

It does not identify the active performance mode, actual GPU wattage, delivered TGP, current power draw, fan behavior, or whether a power-policy write would be safe.

## 6. What It Must Not Be Used For

`GpuGetPower` must not be used to:

- infer or advertise performance-control support;
- choose or send the paired `0x22` GPU power write;
- set custom TGP, PPAB, D-state, limits, temperatures, fan modes, or fan levels;
- enable UI controls, polling, retry loops, EC fallback, or background actions;
- override the existing NO-GO decisions for fan writes.

## 7. Required Gates Before Future Invocation

Before any future manual read-only invocation, all of these must be present:

1. `--hp-victus` and `--hp-wmi-readonly-test`.
2. An elevated Administrator process.
3. Explicit `SafeReadOnlyInvocation` metadata for `GpuGetPower` only, with no write fallback.
4. Exact catalog metadata matching the four-zero-byte input and four-byte output shape.
5. A raw-first decoder/report design that fails safely on missing, short, or unexpected output.
6. Explicit confirmation that normal `--hp-victus` mode remains non-invoking.
7. A manual verification guide that requests only redacted status fields, not raw binary dumps.

This document does not add any of those gates to code.

## 8. Blocked Paired Write And Control Commands

The following remain blocked: `0x22` SetGpuPower, `0x29` concurrent-TDP and CPU/PL limit writes, `0x37` power/LoadLine/IccMax operations, `0x1A` SetFanMode, SetFanLevel, SetFanMax, EC access, and all hardware writes.

## 9. Separation From Fan Control

This is GPU power-policy status research, not fan telemetry or fan control. A successful `0x21` reply would not validate any fan command, fan-write payload, performance-mode write, or SetFanMax recovery path. The fan-write blocker summary and every existing fan NO-GO decision remain unchanged.

## 10. Recommended Next Safe Task

Perform a documentation-only review of existing exact-device or firmware-equivalent `0x21` field evidence and define a four-byte synthetic decoder test matrix. Do not add command-catalog metadata, invocation wiring, or a live probe until that review confirms the raw-first report contract.

Supporting investigation: [HP performance read-only command investigation](hp-performance-readonly-command-investigation.md).
