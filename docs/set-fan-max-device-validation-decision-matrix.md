# SetFanMax Device Validation Decision Matrix

This matrix evaluates evidence only. Its strongest result is **proceed to the next design step**; it never permits or performs a write.

| Evidence path | Required evidence | Decision |
| --- | --- | --- |
| No validated shape | Neither shape is device-validated | Stop |
| One-byte path | One-byte reference evidence, one-byte device validation, and one-byte restore/disable proof | Continue checking shared gates |
| Four-byte path | Four-byte reference evidence, four-byte device validation, and four-byte restore/disable proof | Continue checking shared gates |
| Conflicting path | Both shapes claimed as device-validated | Stop; do not choose a default |
| Missing restore proof | Selected shape lacks matching restore/disable evidence | Stop |

## Shared Gates

Both evidence paths require:

- `FanMaxGet` readback availability.
- A fresh baseline readback showing max fan disabled.
- Planned `FanMaxGet` checks after any future enable and after restore.
- Human review of the reference evidence.
- Human confirmation of the selected device-specific input length.
- Human approval of the recovery plan.

An unavailable, missing, ambiguous, or already-enabled baseline stops the process. Restore evidence and both verification readback plans must use the same selected shape.

## Why This Is Not A Write Implementation

The simulator accepts booleans and readback facts supplied by tests or manual review. It has no WMI dependency, command invocation, payload byte construction, runtime registration, or write permission. `IsWriteExecutionAllowed` is always false.

## Next Safe Task

Have a human independently review and sign off the device-specific evidence collection and manual recovery procedure. Only after one input length and its restore behavior are externally proven should a separate task reassess the guarded preflight design; do not add a runtime write path yet.
