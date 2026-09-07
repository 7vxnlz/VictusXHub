# Normal Fan Control Proof Priority Plan

## Current Final Decision

- Developer-only four-byte SetFanMax Max Fan Pulse: operational only under explicit command-line gates.
- Normal/user-facing fan control: **NO-GO**.

The [go/no-go evidence matrix](normal-fan-control-go-no-go-evidence-matrix.md) remains the broad readiness map. This plan ranks the blockers and defines safe proof order before normal fan control can be reconsidered.

## Priority 1: DeviceValidatedInputLength

Why it matters: normal control needs one exact input contract. VictusX cannot safely ship a command that may be interpreted differently depending on whether the firmware expects one byte or four bytes.

Missing evidence: `DeviceValidatedInputLength` remains `null`; both one-byte and four-byte payloads produced physical response, but neither is validated as the durable normal-control ABI.

Acceptable proof: exact-device evidence for HP Victus `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31` / ThermalPolicyVersion `1`, reviewed against logs, payload bytes, command metadata, restore behavior, and repeated observations.

Not acceptable: generic OMEN/HP references, behavioral similarity alone, successful WMI return codes alone, or choosing the shape that appears to work better in a short pulse.

## Priority 2: Readback Reliability

Why it matters: a normal UI must know whether the requested state actually happened and whether the system returned to safe automatic behavior.

Missing evidence: `FanMaxGet` stayed `false` during physical fan response, and `FanGetLevel` remains raw-only/inconclusive.

Acceptable proof: a reliable success and state criterion independent of `FanMaxGet` alone, or a documented combination of safe telemetry and manual/observable behavior that is strong enough for a product safety decision.

Not acceptable: treating `FanGetLevel` raw bytes as RPM, percent, fan speed, or target state without decoding and validation.

## Priority 3: Restore Proof

Why it matters: normal fan control must fail safe. A write feature cannot depend on hope that firmware eventually returns to automatic control.

Missing evidence: bounded pulse restores succeeded and were observed, but durable restore across failures, cancellations, app exits, restarts, sleep/resume, and service conflicts is not proven.

Acceptable proof: repeated restore/off confirmation across controlled sessions, including interrupted and adverse paths, with a documented recovery procedure.

Not acceptable: a successful restore return code alone, one observed stop after a pulse, or assuming `FanMaxGet=false` proves restore.

## Priority 4: Thermal Safety

Why it matters: fan writes affect cooling behavior and can interact with workload, temperature, firmware policy, and power state.

Missing evidence: manual pulse runs reported no overheating, but no sustained thermal envelope or abort threshold is validated.

Acceptable proof: safe-source temperature trend evidence before, during, and after bounded experiments; clear abort thresholds; no unsafe temperature spikes; documented workload assumptions.

Not acceptable: "no issue noticed" without recorded timing, temperature source, and stop criteria.

## Priority 5: Repeatability Across Sessions

Why it matters: normal controls are repeated by design. A one-shot pulse success does not prove safe repeated operation.

Missing evidence: limited repeated four-byte pulse evidence and one one-byte comparison do not cover long-running sessions, restarts, or repeated use.

Acceptable proof: controlled repeated sessions showing consistent command success, physical behavior, restore behavior, log classification, and absence of unsafe side effects.

Not acceptable: extrapolating from the current short pulses to continuous or repeated user-facing control.

## Priority 6: AC/Battery Behavior

Why it matters: power source can change firmware policy and cooling constraints.

Missing evidence: the developer pulse path is AC-gated, but battery and transition behavior are not validated for normal control.

Acceptable proof: AC online, battery, unplug/replug, unknown-state, and low-battery behavior matrix with fail-closed outcomes.

Not acceptable: allowing writes when AC state is unknown, or assuming AC-only pulse evidence applies to battery operation.

## Priority 7: Sleep/Resume Behavior

Why it matters: firmware state may persist, clear, or change during sleep/resume.

Missing evidence: no sleep, resume, lock, unlock, restart, or shutdown validation exists for normal fan control.

Acceptable proof: controlled sleep/resume and shutdown/restart observations with pre-state, post-state, restore behavior, and recovery notes.

Not acceptable: assuming process shutdown or tray Quit behavior proves firmware restore after sleep/resume.

## Priority 8: Crash/Recovery Behavior

Why it matters: a user-facing feature must be safe when the app crashes, is killed, or loses transport access.

Missing evidence: HP Diagnostic Quit is stable and pulse runs reported no crash, but forced-failure recovery is unproven.

Acceptable proof: simulated or controlled failure paths showing append-only logs, restore attempts where possible, clear user recovery instructions, and no persistent unsafe fan state.

Not acceptable: relying on the happy-path `finally` restore alone.

## Priority 9: HP/OMEN Service Conflict Behavior

Why it matters: vendor services or other tools may also write fan state, producing races or overwritten settings.

Missing evidence: no documented conflict testing with OMEN Gaming Hub, HP services, OmenXHub, OmenSuperHub, omencore, G-Helper, or similar tools.

Acceptable proof: controlled conflict observations and a fail-closed policy for detected competing control ownership.

Not acceptable: assuming exclusive firmware ownership because VictusX is the active process.

## Priority 10: Unsupported Command Isolation

Why it matters: unresolved commands must not become shortcuts around SetFanMax blockers.

Missing evidence: SetFanMode, SetFanLevel, `0x37`, EC writes, and fan curves all lack exact-device proof, safe payload contracts, readback, and restore behavior.

Acceptable proof: separate command-specific designs and evidence packages before any future experiment is even considered.

Not acceptable: using any unsupported command as a fallback, retry path, UI option, or hidden helper for normal control.

## Safe Ordering Of Future Work

1. Read-only proof design for `DeviceValidatedInputLength` and readback reliability.
2. Evidence-review tooling around existing local logs, without new hardware execution.
3. Restore and recovery proof design.
4. Thermal and power-state proof design.
5. Sleep/resume and crash/recovery proof design.
6. Service-conflict proof design.
7. Only after reviewed evidence exists, consider a separate normal-control product safety design.

## Not Next

- No normal fan UI.
- No fan sliders or toggles.
- No fan curves.
- No automatic control.
- No background writes.
- No pulse/run button in the Diagnostic dashboard or tray.
- No undocumented command expansion without a separate proof design.
- No SetFanMode, SetFanLevel, `0x37`, EC write, performance write, retry, or fallback path.

## Recommended Next Safe Task

The [DeviceValidatedInputLength and readback proof design](device-validated-input-length-readback-proof-design.md) now has a read-only local evidence analyzer. It reports the top gaps from valid local logs and cached reports without WMI, experiment execution, or controls. The next safe work is a restore/recovery proof design based on that fail-closed summary.

The [HP fan proof gap analyzer checkpoint](hp-fan-proof-gap-analyzer-checkpoint.md) captures the current local-only analyzer status and the expected HP Diagnostic close-to-tray versus explicit Quit behavior.
