# SetFanMax Gate Test Coverage Checkpoint

## Tested Fail-Closed Scenarios

- Default dashboard input renders SetFanMax as `NO-GO`, not satisfied, not implemented, and not allowed.
- `DeviceValidatedInputLength` missing, invalid, or merely reported as `1` or `4` never selects a payload length.
- The dry-run report serializes write implemented/allowed as `false` and first-write gate status as `NO-GO`.
- Formatter/model helpers have no WMI dependency or invocation surface.

## Old/Missing Report Fallback Coverage

Old schema, missing metadata, missing gate fields, and partial gate fields render safe fallback text. Missing first-write gate fields now show an explicit cached/old-report missing-field reason and fail closed to `NO-GO` / not satisfied.

## Optimistic Cached Value Blocking Coverage

Cached `GO`, `true`, or implemented/allowed-style values are treated as unexpected optimistic data and render blocked. These values are never accepted as write authorization.

## Dashboard, Copy, And Export Coverage

The dashboard row formatter and `BuildSummary()` path are covered. Copy summary and export both use `BuildSummary()`, so their SetFanMax gate text inherits the same fail-closed wording and excludes raw binary dumps. The Markdown exporter also has pure coverage for its blank, no-write manual evidence capture template.

## Remaining Untested Areas

No integration test launches the WinForms app or clicks UI actions for this gate. No test invokes WMI or validates real hardware behavior. Those areas remain manual and safety-gated because SetFanMax is not implemented, no write path exists, and the project must not run explicit WMI probes during normal test coverage.

## Current Status

SetFanMax remains **NO-GO**. Fan writes are not implemented or allowed, `DeviceValidatedInputLength` remains unset, and no payload length has been selected.

## Recommended Next Safe Task

Keep SetFanMax work limited to documentation and cached/report-backed diagnostics until exact-device payload length, restore behavior, thermal/power-state, failure/recovery, and human approval evidence are all proven.
