# SetFanMax Device Validation Plan

## Proven Baseline

On this Victus, elevated explicit read-only tests have successfully decoded `SystemDesignData`, `FanGetCount` (`2` fans), `FanMaxGet` (max fan disabled), and raw-only `FanGetLevel`. These results prove the guarded read path, not write safety.

## Reference Evidence And Ambiguity

The references agree on `hpqBIOSInt0`, command `0x20008`, command type `0x27`, output size `0`, enable state byte `0x01`, and restore/disable state byte `0x00`. They disagree on input length: `ghelper-omen` and `omencore` use `{ state, 0, 0, 0 }` with size `4`; OmenSuperHub uses `{ state }` with size `1`, while OmenXHub delegates to it.

Neither shape is device-validated for this Victus. Choosing one by convention could produce an unsupported BIOS write, and a successful return would still not prove that the max-fan latch can be read back or reliably cleared. This blocks runtime implementation.

## Validation Requirements

Before implementation, a human-reviewed device procedure must:

1. Establish healthy AC power, thermal monitoring, and successful `FanMaxGet=false` immediately before the experiment.
2. Select exactly one input length from device-specific evidence: `1` or `4`; inference from another HP model is insufficient.
3. Define matching enable and restore metadata for that same length, including a zero tail only for length `4`.
4. Define post-enable and post-restore `FanMaxGet` readbacks and strict timeouts.
5. Validate that restore/disable returns the latch to the original disabled state.
6. Record firmware, model, method, input size, results, and all aborts without logging unrelated raw firmware data.

## Manual Recovery

The approved procedure must name the operator, provide an immediate restore/disable action independent of any UI, require shutdown and AC removal if restore cannot be confirmed, and identify the OEM BIOS/default recovery path. The experiment must not begin unless recovery can be performed locally and temperatures can be observed independently.

## Decision Tree

1. If the exact device input length is not independently established as `1` or `4`, stop.
2. If pre-read is missing, fails, is ambiguous, or reports max fan already enabled, stop.
3. If enable and restore descriptions do not use the same validated length, stop.
4. If a tested manual recovery and restore plan is absent, stop.
5. If power, cooling, elevation, required flags, or interactive confirmation is missing, stop.
6. Only after every gate is documented may a separate review consider a guarded implementation.
7. After any future enable attempt, if return status or readback is unexpected, issue only the approved restore action and stop.
8. If restore readback does not confirm the original disabled state, follow manual recovery and prohibit further writes.

## Still Forbidden

No HP WMI write invocation, payload builder, runtime dry-run path, fan UI/control, `SetFanMode`, `SetFanLevel`, `SetFanMax`, ambiguous `0x37`, EC access, BIOS write, or other hardware write is permitted now.

Reviewed references: `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`, `omencore` `b39b44978902606aa708cc0d78bcfd87e95fd88b`, OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81`, and OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`.
