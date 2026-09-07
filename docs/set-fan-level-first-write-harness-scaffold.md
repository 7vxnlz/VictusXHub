# SetFanLevel First-Write Harness Scaffold

This internal, offline scaffold has no execution capability, CLI route, UI route, transport, hardware delegate, retry or fallback. `HpFanLevelFirstWritePreflight.Evaluate` accepts immutable supplied declarations and returns a plan; `ToJson` serializes it in memory only. It does not inspect the machine, resolve evidence references, create a log file, or verify a recovery procedure.

## Request And Gates

The request records percent and both raw arguments, exact target identity, administrator and AC declarations, successful baseline telemetry with an evidence reference, a reviewed recovery-plan reference, an append-only logging declaration and destination. Missing/false declarations and blank references fail closed. Exact identity requires HP, the documented Victus 16-s0035nt or firmware model alias, SKU 7Z5Z2EA#AB8, BIOS F.31 and thermal policy 1. Unrecognized identity strings are rejected, not guessed.

Only integer 2..99 percent and equal raw arguments matching integer `percent * 55 / 100` are admitted. Resulting raw values are 1..54. The candidate buffer is `[raw, raw, 0, 0]`; it is emitted only when all scaffold checks pass. Endpoints, raw zero, raw55..255, asymmetry, inconsistent mapping and high-bit/cleaning values are rejected. This is the close-device default-V1 offline envelope, not an approved hardware range.

Manual physical-response, restore and unsafe-abort observations remain nullable. Notes are control-character sanitized and capped at 1024 characters. Positive observations never satisfy another gate; an unsafe-abort observation blocks preflight.

## Output And Permanent Boundaries

JSON includes the request, requirements, mapping, candidate command identity `0x20008 / 0x2E`, payload, reasons and status. A complete supplied request reports **Preflight satisfied; not executable / not validated** only. This does not establish evidence authenticity, freshness, log writability, safe recovery, ABI validation or approval. No real-world recovery plan is approved by this implementation; complete test fixtures are synthetic.

`IsExecutable`, `WriteExecuted`, `FirstWriteReady` and `NormalFanControlReady` are always false. `NoHardwareInvocation` and `NoWmiInvocation` are always true; `DeviceValidatedInputLength` is always null. Existing dry-run and SetFanMax routing is untouched. There is no first-write value selected, normal fan UI, background loop or executable SetFanLevel command.

Next safe task: review existing exact-target recovery and baseline evidence against the [preflight design](set-fan-level-first-write-preflight-design.md), without executing experiments. F.30 evidence is not F.31 validation. First-write and normal fan control remain **NO-GO**.
