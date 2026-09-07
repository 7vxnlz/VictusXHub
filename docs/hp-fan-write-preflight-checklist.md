# HP Fan Write Preflight Checklist

No HP fan write experiment is allowed until every item below is satisfied for one explicitly approved manual test. This checklist does not authorize implementation.

## 1. Required Proven Read-Only Checks Before Write

- `SystemDesignData` succeeds and still reports the expected HP Victus identity, `ThermalPolicyVersion=1`, and software fan support.
- `FanGetCount` succeeds and reports `FanCount=2` with protection status clear.
- `FanMaxGet` succeeds and reports the current max-fan latch state.
- `FanGetLevel` succeeds and is treated as raw bytes only.
- No read-only probe reports decode errors, unexpected byte counts, or non-clear protection status.

## 2. Required User/Human Confirmations

- Human operator is physically present at the laptop.
- AC power is connected and battery reserve is adequate.
- Independent temperature observation is available.
- The operator confirms the exact command, payload shape, expected readback, and restore path for this single run.
- The operator accepts that fan state may require manual recovery if the process fails.

## 3. Required Runtime Flags For Any Future Write Test

- `--hp-victus`
- `--hp-fan-write-experiment`
- `--hp-wmi-write-manual-test`
- `--hp-fan-write-acknowledge-risk`
- An interactive foreground confirmation repeating the selected command and bounded values.

## 4. Required Admin/Elevation Condition

- The process must be running as Administrator.
- Elevation alone is not enough; all flags, preflight checks, allowlist checks, and confirmations must also pass.

## 5. Required Backup/Readback State Before Write

- Capture device model, BIOS version, thermal policy version, adapter state, fan count, protection state, max-fan state, and raw fan level bytes immediately before the write.
- Record command name, command ID, method name, input length, expected output length, payload schema, and a non-reversible payload digest.
- Do not store raw write payloads or full binary buffers.

## 6. Required Restore/Rollback Path

- The exact return-to-auto command must be proven before the first write.
- Restore must be synchronous, foreground-only, and verified by approved read-only probes.
- No speculative restore payload, inferred default mode, EC fallback, retry loop, scheduled cleanup, or background recovery write is allowed.

## 7. Abort Conditions

- Any gate, confirmation, or read-only baseline check fails.
- Device identity, BIOS version, thermal policy, fan count, or protection state differs from the approved plan.
- Unit, fan order, bounds, watchdog behavior, or restore behavior is unknown.
- Temperature observation is unavailable or unsafe.
- The UI is closing, cancellation occurs, or a second write is requested in the same run.

## 8. Commands Still Forbidden

- `SetFanMode` / `0x1A`
- `SetFanLevel` / `0x2E`
- `SetFanMax` / `0x27`
- Ambiguous `0x37`
- Any EC access, BIOS write, hardware write, fan-control UI, or automatic fan-speed behavior.

## 9. Minimum Acceptable First Write Experiment Shape

- One command only.
- One bounded payload only.
- One foreground elevated run only.
- Pre-write read-only baseline.
- Single write attempt.
- Immediate readback verification.
- Synchronous verified restore to auto.
- Final read-only verification and redacted report.

## 10. Why UI Fan Control Must Wait

UI fan control would imply repeatable, user-facing, recoverable behavior. That is not proven yet. The current evidence proves read-only telemetry and raw fan information, not safe payload units, fan ordering, value bounds, watchdog behavior, restore behavior, or long-running control safety.
