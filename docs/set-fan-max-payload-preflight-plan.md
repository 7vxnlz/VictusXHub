# SetFanMax Payload And Preflight Plan

This is a pure-data safety design. It does not authorize or execute a hardware write.

## Reference-Backed Shape

The references agree on default command `0x20008`, command type `0x27`, zero output, and therefore `hpqBIOSInt0`. They do not agree on input length: `ghelper-omen` and `omencore` submit four bytes with byte 0 as `1` to enable or `0` to restore/disable and bytes 1-3 zero; OmenSuperHub submits only the first state byte. `HpFanMaxPayloadDescription` records both variants with no device-validated selection. It never creates a byte buffer or calls WMI.

## Required Preflight

- The requested command must be exactly `SetFanMax` / `0x27`.
- All future explicit write flags must be present and the process must be elevated as Administrator.
- An interactive human confirmation, approved device/firmware baseline, healthy full read-only baseline, stable AC power, and independent thermal observation are required.
- A successful immediate `FanMaxGet` pre-read is required. Its state must be known and disabled for this single enable-then-restore experiment.
- The only experiment target is enable max fan. Its required restore target is restore/disable max fan.
- A post-write `FanMaxGet` readback plan, a restore `FanMaxGet` readback plan, and a verified synchronous restore plan are mandatory.

## Future Flags

- `--hp-victus`
- `--hp-fan-write-experiment`
- `--hp-wmi-write-manual-test`
- `--hp-fan-write-acknowledge-risk`

## Abort Conditions

Abort before any future write for another command, a missing flag, no elevation, no human confirmation, unapproved or unhealthy baseline, unsuitable power or thermal observation, no pre-read, unknown or enabled baseline max-fan state, no target, no post-read plan, no restore plan, or more than one requested write. After a future attempt, abort without retry or fallback if the call errors, readback does not match, the UI closes, cancellation occurs, or restoration cannot be verified.

## Why No Write Is Executed Yet

The pure evaluator can mark a complete hypothetical request as preflight-approved, but the experiment plan remains `IsWriteExecutionAllowed=false` and has no runtime consumer. There is no WMI call, payload buffer, method selection, allowlist change, or write path in this work.

Reference evidence: `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`.
