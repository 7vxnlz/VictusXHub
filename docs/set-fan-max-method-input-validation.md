# SetFanMax Method And Input Validation

## Reference Evidence

`ghelper-omen` and `omencore` define `SetFanMax` with BIOS command `Default=0x20008`, command type `0x27`, a four-byte input, and zero expected output. Their shared send helper derives the method from output size, so zero output selects `hpqBIOSInt0`.

OmenSuperHub also uses command `0x20008`, command type `0x27`, and zero output, deriving `hpqBIOSInt0`; unlike the other two, it submits a one-byte input. OmenXHub delegates its boolean max-fan endpoint to this OmenSuperHub hardware helper and adds no independent BIOS shape evidence.

## Command And Payload Fields

| Field | Reference result |
| --- | --- |
| BIOS command | `0x20008` (`Default`) |
| Command type | `0x27` (`SetFanMax`) |
| Method | `hpqBIOSInt0` (zero output) |
| Enable state byte | `0x01` |
| Restore/disable state byte | `0x00` |
| Output size | `0` |

For the four-byte variant, bytes 1-3 are `0x00`. For the one-byte variant, there is no trailing input and the reported `Size` is one.

## Differences And Victus Risk

The operation selector and state byte agree, but the input length does not: four bytes in `ghelper-omen`/`omencore`, one byte in OmenSuperHub. This Victus has not validated either write form. Reference tests also show that max fan can behave as an independent latch that requires an explicit disable call; they do not prove this Victus will latch, clear, or recover the same way.

## Current Model And Decision

The previous pure model incorrectly presented four bytes as a single reference expectation. The stricter model now leaves `DeviceValidatedInputLength` unset by default, accepts only an explicitly selected one-byte or four-byte device-validated shape, and creates matching enable/restore metadata only for that shape. The state-byte descriptions still match every inspected reference.

`SetFanMax` must remain docs-only. Do not add a guarded dry-run runtime path: even a dry run that selects or validates a real WMI method risks becoming an invocation path without device-specific proof. What remains unproven is this device's accepted input length, return behavior, latch persistence, disable/restore behavior, and manual recovery procedure.

Reviewed references: `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`, `omencore` `b39b44978902606aa708cc0d78bcfd87e95fd88b`, `OmenSuperHub` `a6ab6988c446ee5421466097fdf60c0d521e5c81`, and `OmenXHub` `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`.
