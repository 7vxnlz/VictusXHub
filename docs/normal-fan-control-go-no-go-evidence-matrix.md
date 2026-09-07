# Normal Fan Control Go/No-Go Evidence Matrix

## Current Decision

Developer-only four-byte SetFanMax Max Fan Pulse is operational under explicit command-line gates. It remains a bounded research operation only.

Normal/user-facing fan control remains **NO-GO**. No fan sliders, toggles, automatic writes, fan curves, background control, or normal UI route may be added from the current evidence.

## Evidence Matrix

| Area | Current evidence | Current status | Blocker / missing proof | Required next proof | Current decision |
| --- | --- | --- | --- | --- | --- |
| SetFanMax developer pulse | Four-byte Max Fan Pulse has been manually verified after the contract refactor. Enable and restore succeeded, physical response and restore were observed, and the path stayed command-line-only. | Operational for developer research under explicit gates. | This proves only a bounded pulse path, not durable fan control. | Keep append-only logs and repeat only through separate approved developer commands when needed. | Developer-only GO; normal-control NO-GO. |
| Payload shape | Four-byte has repeated exact-device physical-response evidence. One-byte has one comparison result and OmenXHub/OmenSuperHub reference support. | Experimental only. | Behavioral similarity does not prove ABI equivalence or normal-control safety. | A separate reviewed decision selecting one validated normal-control ABI, with evidence beyond short pulse behavior. | No normal payload selected. |
| Restore behavior | Matching restore/off payloads returned success and operator observed restore in bounded pulse runs. | Partial experimental evidence. | Durable restore is not proven across repeated sessions, failures, cancellation, sleep/resume, battery, or competing services. | Restore proof across controlled sessions and adverse conditions, with clear recovery steps. | Insufficient for normal control. |
| FanMaxGet readback | FanMaxGet stayed `false` during all observed physical fan responses. | Inconclusive/unreliable for pulse success. | Cannot be the sole latch or state contract for normal UI. | A reliable success/state criterion independent of FanMaxGet alone. | Blocks normal control. |
| FanGetLevel readback | Raw values changed across experiments, but remain raw-only. | Observational only. | Scale, fan mapping, units, and control meaning are unknown. | Decoded meaning or a documented raw interpretation validated against external observation. | Not usable as a control target. |
| DeviceValidatedInputLength | Remains `null` / unset. | Not validated. | Neither one-byte nor four-byte is proven as the durable device ABI for normal control. | Explicit reviewed evidence that selects and scopes a device-specific input length. | Must remain unset. |
| Repeatability | Four-byte response is repeated; one-byte response has one comparison record. | Narrow experimental repeatability. | No long-run, restart, battery, sleep/resume, crash, or service-conflict coverage. | Repeated controlled sessions with consistent enable, restore, readback, and manual observation. | Insufficient for normal control. |
| Thermal safety | Manual pulse runs reported no overheating. | Short-duration evidence only. | No thermal envelope, sustained behavior, workload variation, or abort threshold evidence. | Temperature trend evidence from safe sources, pre/post limits, abort criteria, and recovery validation. | Blocks normal control. |
| AC/battery behavior | Developer runner requires AC gate; current experiments were AC-gated. | AC-only experimental path. | Battery behavior and power-state transitions are not validated. | AC/battery state matrix, including offline/unknown fail-closed behavior and no unsafe transitions. | Normal control blocked. |
| Sleep/resume behavior | No normal-control sleep/resume validation exists. | Unknown. | Persistent firmware state after sleep/resume is not proven. | Sleep, resume, lock, unlock, and shutdown/restart observations with restore confirmation. | Blocks normal control. |
| Crash/recovery behavior | Manual pulse runs reported no crash; HP Diagnostic Quit terminates cleanly. | Limited positive evidence. | Recovery from failed write, app crash, forced termination, or restore exception is not proven for normal control. | Forced-failure and recovery protocol with proof that firmware returns to a safe state. | Blocks normal control. |
| OMEN/HP service conflict behavior | No documented conflict testing with vendor services or other fan tools exists. | Unknown. | External service overwrites, races, or conflicting firmware ownership are untested. | Controlled service-conflict observations and fail-closed policy. | Blocks normal control. |
| Unsupported command: SetFanMode | Reference evidence is ambiguous and mode-state-changing. | Blocked. | No exact-device mode baseline, payload contract, readback, or restore. | Separate command-specific proof package before any experiment design. | NO-GO. |
| Unsupported command: SetFanLevel | References disagree on input length and semantics; FanGetLevel is raw-only. | Blocked. | No safe target scale, fan mapping, restore, or automatic-control handoff. | Separate exact-device evidence and recovery design. | NO-GO. |
| Unsupported command: `0x37` | Ambiguous across references and thermal-policy generations. | Blocked. | Direction, payload, and meaning are unknown for this V1 device. | Separate read/write classification and exact-device proof. | NO-GO. |
| Unsupported command: EC writes | No EC write path is part of VictusX HP research. | Blocked. | EC access would add a new hardware risk surface. | Explicit separate design and proof, if ever considered. | NO-GO. |
| Unsupported command: fan curves | No fan curve command, scale, scheduler, or restore behavior exists. | Blocked. | Curves imply repeated background writes and normal product control. | Full normal-control safety design after command validation. | NO-GO. |
| UI readiness | HP Diagnostic dashboard is read-only and local/report-backed. Pulse history is local-file-only. | Safe diagnostic UI only. | User-facing controls would imply validated state, restore, safety, and recovery that do not exist. | Product-level normal-control design after all evidence rows become GO/ready. | No normal fan UI. |

## Biggest Remaining Blockers

- `DeviceValidatedInputLength` is still unset.
- No payload shape is fully validated for normal control.
- FanMaxGet is inconclusive and cannot prove fan state alone.
- The [DeviceValidatedInputLength and readback proof design](device-validated-input-length-readback-proof-design.md) keeps physical fan response separate from payload validation and readback reliability.
- Restore behavior is not proven across failures, power transitions, service conflicts, or long-running sessions.
- Thermal, AC/battery, sleep/resume, and crash/recovery evidence is incomplete.
- SetFanMode, SetFanLevel, `0x37`, EC writes, and fan curves remain unsupported and must not be used as fallback paths.

## Recommended Next Safe Task

Keep work in internal research or read-only diagnostic evidence review. The [normal fan control proof priority plan](normal-fan-control-proof-priority-plan.md) ranks the blocker evidence and recommends a read-only proof design for `DeviceValidatedInputLength` and readback reliability before any further experiment or UI work.
