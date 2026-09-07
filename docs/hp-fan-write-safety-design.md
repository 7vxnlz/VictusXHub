# HP Fan Write Safety Design

## Current Read-Only Evidence

This HP Victus has safely completed elevated, single-shot read-only probes:

- `SystemDesignData`: `ThermalPolicyVersion=1` and `DeclaresSoftwareFanControlSupport=true`.
- `FanGetCount`: two fans, protection status clear.
- `FanMaxGet`: max fan disabled.
- `FanGetLevel`: returned raw-only values; their units and control meaning remain unproven.

These results establish a useful firmware capability and baseline. They do not prove a safe write command, payload, range, restore sequence, or watchdog behavior.

## Candidate Write/Control Commands

| Command | Reference role | Why it is dangerous |
| --- | --- | --- |
| `SetFanMode` / `0x1A` | Writes a fan/performance mode payload. | Can change a coupled thermal or performance policy; mode values vary by firmware and may not mean the same thing on this V1 Victus. |
| `SetFanLevel` / `0x2E` | Writes per-fan manual values. | Fan order, units, valid range, minimum safe floor, timeout, and return-to-auto behavior are unproven. References also describe model-specific non-responsive fan behavior after a zero-level write. |
| `SetFanMax` / `0x27` | Writes the max-fan latch. | Can change a persistent operating state and requires another write to change it back. Its interaction with manual levels and thermal policy is unproven. |
| `0x37` | Read-like V2 fan-level use in some references; write-like power-limit use in others. | The command-family ambiguity makes it unsafe to classify or invoke on this V1 device. |
| EC fan paths | Fallback/direct fan control in some references. | They bypass the proven BIOS WMI read-only path and have no validated register map or recovery behavior for this Victus. |

The command roles above are reference evidence, not an implementation recipe. The reviewed sources are `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`, `omencore` `b39b44978902606aa708cc0d78bcfd87e95fd88b`, `OmenSuperHub` `a6ab6988c446ee5421466097fdf60c0d521e5c81`, and `OmenXHub` `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`.

## Required Preconditions

No future write experiment is eligible until all of the following are true:

- A separately approved, model- and firmware-specific experiment plan names one command, one exact payload format, one bounded value set, and one expected post-write readback.
- The device identity, BIOS version, `ThermalPolicyVersion`, fan count, and current max-fan state are captured immediately before the experiment and match the approved plan.
- The manual-value unit, fan ordering, accepted range, timeout/watchdog behavior, and a vendor- or device-validated return-to-auto path are known. Raw `FanGetLevel` bytes are insufficient evidence.
- A read-only baseline is healthy: both fans are reported, protection status is clear, and no unexpected WMI or decode error is present.
- The machine is on stable AC power, has adequate battery reserve, is supervised locally, and has independent temperature/fan observation available for the entire test.
- The experiment is explicitly approved by the operator for that single run. No saved setting, UI control, service, startup action, or background task may authorize it.

## Required Future Runtime Gates

Any future implementation must require all of these simultaneously, with no configuration-file or environment-variable bypass:

- `--hp-victus`
- `--hp-fan-write-experiment`
- `--hp-wmi-write-manual-test`
- `--hp-fan-write-acknowledge-risk`
- An interactive confirmation that repeats the selected command and bounded values
- A dedicated write-only allowlist entry; `SafeReadOnlyInvocation` must never authorize a write
- A single elevated Administrator process

The default application path, including normal `--hp-victus`, must remain write-free.

## Elevation And Execution Boundaries

Elevation is necessary but never sufficient. A future write path must reject non-admin processes, unknown model or firmware fingerprints, unsupported thermal-policy versions, any command outside the dedicated allowlist, and any payload that differs from the approved fixed shape. It must be single-shot, foreground-only, and unable to retry, poll, schedule, or run during startup, shutdown, suspend/resume, or crash recovery.

## Required Rollback And Restore Design

Before the first write is allowed, the exact return-to-auto command and its device-specific confirmation must be established through a separate safety review. The implementation must capture the read-only baseline, verify the post-write state, and synchronously attempt the validated restore before reporting completion. It must verify restoration with the same approved read-only probes.

There must be no speculative "restore" payload, no inferred default mode, no EC fallback, and no background or best-effort cleanup write. If the process terminates unexpectedly, the operator needs a documented manual recovery procedure; automatic recovery cannot be assumed safe.

## Required Logging And Report Fields

A future report must contain only redacted, reviewable metadata:

- Request and confirmation state, gate results, elevation state, device/BIOS/thermal-policy fingerprint, and an operation identifier.
- Command name, command ID, method name, input/output lengths, payload schema/version, and a non-reversible payload digest; never raw payloads or binary buffers.
- Read-only baseline summaries, preflight verdict, invocation result/return code, duration, post-write verification result, restore attempt/result, and final read-only state.
- Abort reason, exception category, and whether the hardware state is known restored, known not restored, or unknown.

## Abort Conditions

Abort before writing when any precondition or gate fails, baseline data is missing or stale, protection status is non-clear, the unit/range/restore behavior is not established, thermal or external observation is unavailable, AC/battery conditions are unsuitable, or a second write is requested in the same run.

Abort immediately after an attempted write when the BIOS reports an error, the call throws, the post-write readback disagrees with the approved expectation, monitoring detects an unsafe condition, the UI is closing, cancellation occurs, or restoration cannot be verified. The report must then explicitly state that final fan state is unknown or not restored; it must not retry or issue an unreviewed fallback write.

## Forbidden Now

No fan write or control implementation is authorized. `SetFanMode` / `0x1A`, `SetFanLevel` / `0x2E`, `SetFanMax` / `0x27`, and ambiguous `0x37` remain blocked. EC access, BIOS writes, hardware writes, fan-speed UI, polling, automatic retries, automatic restore, performance mode control, and any ASUS behavior change remain forbidden.

## Exact Next Safe Step

Create a documentation-only, model-specific manual-control preflight specification. It must first prove the `SetFanLevel` value unit, fan ordering, minimum/maximum safe bounds, watchdog timeout, and a readback-verified return-to-auto sequence using additional evidence; do not add a command catalog entry, runtime flag, invocation path, or write-capable code yet.
