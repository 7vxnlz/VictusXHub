# SystemDesignData Invocation Approval

## 1. Selected command

`SystemDesignData`

## 2. Why this is the first read-only invocation candidate

It is the lowest-risk HP BIOS WMI candidate because it is informational platform/design data, uses `hpqBIOSInt128`, and is not a fan, power, thermal, RGB, keyboard, battery, or control command.

## 3. Safety checks already passing

- `SystemDesignData` exists in the VictusX command catalog.
- It is marked `ReadOnly`.
- It is marked `SafeReadOnlyInvocation`.
- Expected method is `hpqBIOSInt128`.
- Expected output size is `128`.
- The dry-run path validates that `root\wmi`, `hpqBIntM`, `hpqBDataIn`, and `hpqBIOSInt128` are available before allowing readiness.

## 4. Required runtime flag

Only `--hp-victus` mode may attempt this command. It must remain disabled in default ASUS mode.

## 5. Log before invocation

- Command name.
- Method name.
- Expected input size.
- Expected output size.
- Safety classification.
- Confirmation that `--hp-victus` mode is active.

## 6. Log after invocation

- Whether invocation succeeded.
- Whether a return-code field was present.
- Sanitized return status.
- Output byte length.
- Sanitized error details if any.

## 7. Forbidden scope

Only `SystemDesignData` may be invoked. No fan, EC, BIOS write, power, thermal, RGB, keyboard, display, GPU, or battery commands are allowed.

## 8. Abort conditions

- `--hp-victus` is missing.
- HP Victus identity is not confirmed.
- Required HP WMI classes are unavailable.
- `hpqBIOSInt128` is not exposed.
- Command metadata changes away from `ReadOnly` and `SafeReadOnlyInvocation`.
- The command hangs, throws, returns unexpected structure, or reports suspicious errors.

## 9. Rollback plan

Revert the invocation-path commit and change `SystemDesignData` back to `ReadIntent` if the first real read fails, hangs, returns suspicious data, or creates unexpected side effects.

## 10. Exact next coding step

Implement one-shot `SystemDesignData` invocation behind `--hp-victus` only, write sanitized output length/status to the HP capability report, and leave every other HP BIOS WMI command blocked.
