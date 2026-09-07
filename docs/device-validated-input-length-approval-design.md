# DeviceValidatedInputLength Manual Approval Design

## Purpose

`DeviceValidatedInputLength` identifies the only device-proven `SetFanMax` input length: `1` or `4`. It is required because the references agree on the command and state byte but disagree on the input length. It is evidence metadata for a future preflight, not a hardware-control setting.

## No Default

The value must begin unset (`null`). VictusX must never default, infer, pad, truncate, or fall back to either length:

- `1` is not implied by a successful `FanMaxGet`, fan count, thermal policy, or another project.
- `4` is not implied by zero-filled reference buffers or general HP WMI conventions.
- A missing, stale, malformed, conflicting, or revoked approval is equivalent to no approval.

## Required Evidence Before Manual Approval

An approval record must contain independently reviewable evidence for this exact Victus model, SKU, BIOS revision, and `SetFanMax` operation:

1. Exactly one selected input length, `1` or `4`, with no competing claim.
2. Command metadata matching the selected evidence: command `0x20008`, type `0x27`, zero output, and the expected `hpqBIOSInt0` transport.
3. Matched enable and restore descriptions using the same length: state byte `0x01` to enable and `0x00` to restore/disable.
4. A current successful `FanMaxGet` baseline showing max fan disabled.
5. Reviewed recovery, AC power, elevation, thermal observation, and human confirmation plans.

Reference-family evidence alone is insufficient. Manual approval must cite the supporting artifact, date, reviewer, device identity, and BIOS identity.

## Proposed Future Storage

No storage is added now. If a separately approved implementation task later needs a local record, place an explicit evidence artifact under:

`%APPDATA%\VictusX\ManualApprovals\set-fan-max-device-approval.json`

It must be separate from ordinary settings, never silently created or migrated, and never automatically trusted after a device or BIOS change. Its purpose is auditability and preflight input only; it must not create a write command, payload, flag bypass, or UI control.

## Proposed Approval Wording

> I reviewed device-specific evidence that this exact HP Victus model, SKU, and BIOS accepts SetFanMax input length [1 or 4]. The evidence includes matching enable and restore/disable descriptions, a FanMaxGet-disabled baseline, recovery procedure, AC power, thermal observation, and elevation requirements. I approve recording this evidence for preflight review only. This approval does not authorize a fan write, runtime experiment, fan UI, retry, or background action.

The record must include the reviewer name, date, selected length, evidence references, model, SKU, BIOS version, and a revocation field.

## Matching Restore Proof

Approval is invalid unless restore uses the same selected length and is supported by evidence that it returns the max-fan latch to disabled. In a future guarded experiment, this would require `FanMaxGet` before enable, after enable, and after restore. Until successful device evidence exists, this is a required plan, not proof.

## Invalidation And Revocation

Revoke the approval immediately when any of these change or become uncertain:

- BIOS/firmware version, model, SKU, board identity, or thermal-policy interpretation.
- Conflicting length evidence, malformed approval data, missing evidence source, or reviewer withdrawal.
- Failed/ambiguous restore, unexpected `FanMaxGet`, missing thermal observation, or recovery-plan failure.
- Any proposal to extend the scope to `SetFanMode`, `SetFanLevel`, `0x37`, EC access, fan curves, or UI control.

Revocation clears the effective length back to unset and leaves SetFanMax at **NO-GO**. It must never trigger a restore write automatically.

## Preflight Interaction

A future preflight may read a manually approved length only after validating the record against the current device and BIOS. It must still require every existing gate: exact SetFanMax command, future explicit flags, elevation, foreground human confirmation, healthy read-only baseline, AC power, thermal observation, disabled `FanMaxGet` pre-read, post-write readback plan, restore plan, and a single-attempt limit.

Approval contributes one evidence field. It cannot make `SetFanMaxWriteImplemented` or `SetFanMaxWriteAllowed` true, and it cannot grant write permission.

## Still Forbidden

No WMI invocation, SetFanMax execution, write payload creation, fan control, `SetFanMode`, `SetFanLevel`, ambiguous `0x37`, EC access, BIOS write, hardware write, automatic restore/retry, fan UI, or change to default ASUS behavior is permitted.
