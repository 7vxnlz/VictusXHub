# First HP WMI Read-Only Invocation Candidate

## Selected command

`SystemDesignData`

## Why it is the lowest-risk first candidate

- It is informational platform/design data, not a fan, power, GPU, battery, display, or lighting control.
- It uses the exposed `hpqBIOSInt128` method and expects a data response rather than a zero-output setter path.
- It is the best first proof that read-only HP BIOS WMI plumbing works before any hardware-adjacent status query.

## Why other candidates are deferred

- Fan count, fan level, fan RPM, and fan max state are too close to future fan-control behavior.
- Temperature is low risk, but less useful than system design data as the first capability proof.
- Keyboard, RGB, light bar, display, GPU, power, and battery commands are deferred because their nearby write paths need separate safety review.

## Required safety checks before invocation

- `--hp-victus` must be explicitly passed.
- HP Victus identity must still be detected.
- `root\wmi`, `hpqBIntM`, and `hpqBDataIn` must be available.
- `hpqBIOSInt128` must be exposed by `hpqBIntM`.
- The command definition must remain read-only and `SafeReadOnlyInvocation`.
- Invocation must be one-shot, report-only, logged before and after, and sanitized.

## Exact next coding step

Add a one-shot `SystemDesignData` invocation path behind `--hp-victus` that writes only sanitized output length, return status, and non-sensitive parsed capability hints to the HP capability report.

## Rollback plan

If the read fails, hangs, returns unexpected data, or logs any suspicious error, revert the single invocation-path commit and change `SystemDesignData` back to `ReadIntent` until the command shape is reviewed again.
