# SetFanMax Dry-Run Report Guide

The HP capability report exposes no-write SetFanMax safety metadata:

- `SetFanMaxWriteImplemented`: always `false`; no write implementation exists.
- `SetFanMaxWriteAllowed`: always `false`; dry-run evidence cannot authorize a write.
- `SetFanMaxDryRunEvaluated`: confirms the pure evidence evaluator produced the report metadata.
- `SetFanMaxDeviceValidatedInputLength`: `null` until exactly one device-specific input length is proven; only `1` or `4` can be represented.
- `SetFanMaxDryRunBlockedReasons`: names unmet or conflicting safety evidence.
- `SetFanMaxNextRequiredProof`: identifies the next evidence requirement without suggesting execution.

## Why This Is Not A Write Path

The report uses pure data and the existing validation simulator. It does not build payload bytes, register a write command, call `hpqBIOSInt*`, invoke WMI, or grant write permission. Normal `--hp-victus` mode keeps every explicit read-only invocation unattempted unless the separate approved read-only test flag is supplied.

## Missing Proof

This Victus still lacks device-specific proof selecting exactly one SetFanMax input length (`1` or `4`), plus matching restore/disable behavior and a validated recovery procedure. Reference agreement on the state byte is not device validation.

## Interpreting The Current Blocked State

The current expected report has write implemented/allowed `false`, dry-run evaluated `true`, validated length `null`, and blockers for device-length and human recovery confirmation. This is a healthy fail-closed result: it records what is missing without selecting a payload shape or creating a write path.

## Why Fan UI Must Wait

A fan UI would imply an available, repeatable control operation. No such operation is implemented or authorized, and there is no proven payload shape or restore guarantee for this device. UI fan control must wait until a separate safety review explicitly approves an implementation after the missing evidence is resolved.
