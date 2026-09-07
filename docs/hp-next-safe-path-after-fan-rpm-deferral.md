# Next Safe HP Path After FanGetRpm Deferral

## Why FanGetRpm Is Deferred

`FanGetRpm` / `0x38` is deferred because this real HP Victus decoded `ThermalPolicyVersion=1`, while references describe `0x38` as a V2/OMEN Max-style direct RPM path. Reference notes also warn that V0/V1 systems can return garbage or phantom RPM-shaped values, so preparing it now would risk misleading telemetry.

## Candidate Path Comparison

| Candidate | Assessment |
| --- | --- |
| Investigate `GetFanType` / `0x2C` or `44` | Read-shaped, but command numbering and command-family usage remain ambiguous. Useful later, but should be mapped before becoming a probe. |
| Investigate `GetFanTable` / `0x2F` | Likely larger and more semantic than current status reads. Table/curve payloads are easy to over-interpret. |
| Pause fan reads and start fan write safety design only | Safer than implementation, but premature while read-only fan evidence is still scattered and incomplete. |
| Investigate performance mode read-only status commands | Valid future direction, but it widens scope away from the current fan evidence chain. |
| Create a consolidated HP fan read-only map | Best next step: no WMI, no code, no control behavior, and it clarifies which fan commands are proven, deferred, ambiguous, or forbidden. |

## Selected Next Safe Path

Create a consolidated HP fan read-only command map.

## Why It Is Selected

The project now has four successful real read-only results and one explicit deferral. A consolidated map will make the boundary easier to review before any new command is promoted to a probe candidate. It also keeps `GetFanType`, `GetFanTable`, `FanGetRpm`, `FanGetLevelV2`, and all write/control commands separated by evidence level instead of treating them as a loose backlog.

## Remaining Risks

- `GetFanType` may be read-only only in one command family and ambiguous in another.
- `GetFanTable` may expose curve/table data that should not be interpreted without stronger evidence.
- `FanGetRpm` may return plausible but false RPM values on this V1 Victus.
- Fan level raw bytes are still not proven to be RPM, percent, curve points, or control levels.
- Any future read probe still requires explicit gates, synthetic decode tests, and manual elevated verification.

## Still Forbidden

Do not invoke WMI from Codex, run `--hp-wmi-readonly-test` from Codex, implement fan control, implement fan writes, add fan speed control, interpret raw or unknown values as RPM/percent/curve/control level, add EC access, add BIOS writes, add hardware writes, add polling loops, add retries, or change ASUS behavior.

## Recommended Next Concrete Task

Create `docs/hp-fan-readonly-command-map.md` that groups fan commands into proven read-only, prepared/probed read-only, deferred read-only-looking, ambiguous, and forbidden write/control categories. Do not prepare another invocation candidate until that map is reviewed.
