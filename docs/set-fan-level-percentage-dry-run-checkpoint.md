# SetFanLevel Percentage Dry-Run Checkpoint

## Current Model

The preferred inert research input is `--fan-percent-candidate=<integer 0-100>` with `--hp-victus --hp-fan-level-research-dry-run`. It records JSON only and exits before normal UI, hardware, or WMI startup.

The close-device V1 mapping is `p == 100 ? 100 : p * 55 / 100`, using integer division after accepting only `0..100`. This is F.30 close-device source evidence, not F.31 validation.

| Percent | Raw | Candidate payload |
| --- | --- | --- |
| 0 | 0 | `00-00-00-00` |
| 1 | 0 | `00-00-00-00` |
| 50 | 27 | `1B-1B-00-00` |
| 99 | 54 | `36-36-00-00` |
| 100 | 100 | `64-64-00-00` |

The candidate format is `[raw, raw, 0, 0]`. No `0x80` high-bit or cleaning behavior is added.

## Compatibility And Safety

Legacy `--fan-level-candidate=128` remains deprecated raw research only and continues to serialize as `80-80`. It is not 128%, not a cleaning request, and not a first-write candidate.

Every dry-run record declares no hardware invocation and no WMI invocation. `DeviceValidatedInputLength` remains null, SetFanLevel first-write remains **NO-GO**, and normal/user-facing fan control remains **NO-GO**.

## Next Research Topic

Endpoint and recovery risks remain the next research topic: upstream 0% and 100% take special executable paths, while low positive values can truncate to raw zero. No endpoint behavior, restore packet, or F.31 physical meaning is approved from this dry-run model.
