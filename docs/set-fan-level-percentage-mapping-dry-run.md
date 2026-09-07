# SetFanLevel Percentage-Mapping Dry-Run

## Scope And Evidence

Preferred developer research input is now `--fan-percent-candidate=<integer 0-100>` with `--hp-victus --hp-fan-level-research-dry-run`. This path only serializes and persists JSON, then exits before hardware or UI startup. No first-write value is selected.

Reconfirmed local omencore revision `b39b44978902606aa708cc0d78bcfd87e95fd88b`: `src/OmenCoreApp/Hardware/WmiFanController.cs`, `MapFanPercentToWmiLevel` (212), and `HpWmiBios.cs`, `DetectMaxFanLevel` (962) / `SetFanLevel` (1052). See the [deep trace](omencore-victus-16s-setfanlevel-deep-trace.md). This is **Close-device evidence**, Victus 16-s0xxx / 8BD4 / V1 / F.30, not target F.31 validation. The implementation reexpresses the arithmetic only; it does not copy or connect the reference controller.

## Mapping Contract

Source formula: clamp p to 0..100; if p == 100 return 100; otherwise integer `(p * clamp(ceiling, 1, 100)) / 100`. This model fixes the reference default V1 ceiling at 55, with no user/model override. CLI input outside 0..100 is rejected, not clamped or reinterpreted.

| Requested percent | Mapped raw argument | Four-byte JSON candidate |
| --- | --- | --- |
| 0 | 0 | 00-00-00-00 |
| 1 | 0 | 00-00-00-00 |
| 2 | 1 | 01-01-00-00 |
| 25 | 13 | 0D-0D-00-00 |
| 50 | 27 | 1B-1B-00-00 |
| 75 | 41 | 29-29-00-00 |
| 99 | 54 | 36-36-00-00 |
| 100 | 100 | 64-64-00-00 |

These are arithmetic examples, not proposed hardware targets. Both fan arguments receive the same mapped value; positions 2 and 3 are zero padding. No high bit or cleaning semantics are inserted. A shared percentage avoids implying validated independent CPU/GPU control: the reference dual setter exists, but the close profile disables independent curves and maintenance can synchronize both arguments.

**Boundary hazards:** upstream exact 0% routes to auto recovery before mapping. Upstream 100% attempts MaxFan first, with raw 100 fallback. The dry-run models the mapping helper, not these executable branches. Positive 1% can truncate to raw zero, which is not a proven safe minimum or restore packet. No branch, retry, fallback, recovery, timer or write is adopted. FanGetLevel remains raw-only, not RPM or percent.

## JSON And Compatibility

Percentage records use schema 2, keep existing status/metadata field names, and add `UsesPercentageMapping`, `RequestedPercentCandidate`, `MappedRawLevelCandidate`, `MappingFormula`, `MappingSource`, `ReferenceDefaultV1Ceiling`, `EvidenceConfidence`, `TargetBios`, `TargetBiosValidated`, and `BoundaryCaution`. `RawLevelCandidate` is null on this preferred path. `PayloadHexCandidate` records all four bytes; `CandidateInputLength=4` is hypothetical, never validation.

Deprecated `--fan-level-candidate=<0-255>` remains explicitly raw research only with schema 1 and its original two-byte format. Thus legacy 128 still serializes as `80-80`, never 128%, never a cleaning request, and never a first-write choice. Old files are not rewritten. Percentage and legacy flags cannot be combined. Missing, duplicate, malformed or mixed probe/write flags fail closed with no candidate payload.

Both paths retain `NoHardwareInvocation=true`, `NoWmiInvocation=true`, `IsExecutable=false`, `FirstWriteReady=false`, `NormalFanControlReady=false`, and `DeviceValidatedInputLength=null`. WMI identifiers are descriptive strings only. Existing append-only UTF-8 timestamp/GUID files under `%APPDATA%\VictusX\Logs\FanExperiments\` remain independent of WinExe console visibility; no new UI action exists.

## Decision And Next Step

The [endpoint/recovery audit](set-fan-level-endpoint-recovery-audit.md) narrows offline research to2..99% / raw1..54 with equal arguments; it does not change this inert parser's0..100 arithmetic coverage. Raw0, the100 endpoint, and all recovery assumptions remain excluded from any prospective write envelope. Exact-target recovery evidence is needed before first-write consideration.

Mapping correctness does not prove accepted ABI, physical units, range, fan ordering or recovery on F.31. Request existing exact-target protocol/recovery evidence next, without probes or experiments. SetFanLevel first-write and normal/user-facing fan control remain **NO-GO**. Existing separately gated SetFanMax permissions are unchanged.
