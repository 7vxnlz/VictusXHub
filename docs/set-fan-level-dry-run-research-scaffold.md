# SetFanLevel Dry-Run Research Scaffold

## Scope

Current preferred input is the [percentage-mapping dry-run](set-fan-level-percentage-mapping-dry-run.md): `--fan-percent-candidate=0..100` models default V1 integer scaling and a four-byte candidate, with explicit F.30-only evidence and endpoint warnings. The raw CLI/JSON contract below is retained as **deprecated legacy research compatibility**, not the preferred path; historical `128 -> 80-80` records remain unchanged. Neither path validates F.31 or authorizes a write.

The developer-only scaffold parses a raw candidate and persists an indented JSON research record before attempting console output. It has no transport, hardware provider, executable operation contract, device probe, or UI route. This is permission to inspect a serialization hypothesis only; SetFanLevel execution and normal/user-facing fan control remain **NO-GO**.

The [Omen comparison](omenxhub-omensuperhub-fan-implementation-comparison.md) documents command `0x20008`, command type `0x2E`, `root\wmi` / `hpqBIntM` / `hpqBIOSInt0`, and two/three-byte reference payloads. These are reference identities, not exact-device validation. No reference code was copied.

## CLI Contract

Authorized persistence verification syntax (raw serialization only):

```text
dotnet run --no-launch-profile --project app\VictusX.csproj -- --hp-victus --hp-fan-level-research-dry-run --fan-level-candidate=128
```

- Exactly one HP mode flag, one dry-run flag, and one decimal candidate are required.
- The candidate is a raw byte in `0..255`. This is a serialization bound, NOT a safe hardware range, RPM, percentage, or recommended fan target. `42` is only an illustrative byte value.
- Missing, duplicate, malformed, signed, fractional, or out-of-range values fail closed. There is no default or clamping.
- Every other argument is rejected, including HP WMI probes, SetFanMax hold/pulse/experiments, approval flags, and payload-length selection.
- The handler runs before all hardware-capable startup handlers. Recognized dry-run/candidate arguments, including malformed assignments and orphan candidate arguments, consume the invocation and exit even if validation fails.
- Successful persistence exits with code `0` for valid candidates; rejected requests save a record without a payload and exit with code `2`. A file-write failure exits with code `1`, never normal startup. Neither result authorizes execution.
- Every result is saved to `%APPDATA%\VictusX\Logs\FanExperiments\set-fan-level-research-dry-run-<UTC yyyyMMdd-HHmmss-fffffff>-<GUID>.json`. Files are UTF-8, created with `CreateNew`, and never overwrite previous records. No user-supplied path is accepted by the CLI.
- WinExe console output may be invisible. Persistence happens first and does not depend on a console. When available, console output includes the absolute saved path and JSON. No normal window, dialog, or file viewer is opened. Open the documented directory to inspect the saved result.

## Candidate And Output

The [closest-Victus deep trace](omencore-victus-16s-setfanlevel-deep-trace.md) distinguishes this raw-byte serializer from omencore's clamped percentage API: passing128 to that API would not produce80-80. ghelper-omen's128-byte input is allocated by its setter, not required by shared infrastructure. Modern +128 cleaning and separate legacy flags still do not establish target firmware meaning. Keep the dry-run inert, its historical records unchanged, and all first-write candidates unselected.

The [ABI and units evidence audit](set-fan-level-abi-units-evidence-audit.md) finds conflicting two/three-, four-, and 128-byte reference requests, mixed caller scaling, and a capability-gated +128 cleaning convention. Thus `128 -> 80-80` proves only two serialized bytes, not a safe level, percent, RPM, or supported ABI. Its firmware meaning remains uncertain; the parser's `0..255` range must not become an executable range. No record, parser, catalog, or validation field was changed by that audit.

`HpFanLevelResearchDryRunRecord` is descriptive data, deliberately separate from `HpFanResearchOperationKind`, the shared command catalog, and the SetFanMax runner. `FourByteMaxFanPulse` remains the only operation in the executable research path.

The single hypothesis repeats the supplied raw byte in two positions (`42` serializes as `2A-2A`). It does not claim validated fan mapping, device fan count, supported input length, or restore semantics. Three-byte references and the existing catalog's forbidden four-byte `FanLevelWrite` entry remain unresolved evidence; none are tried or selected as fallback. The catalog remains forbidden and unchanged.

The schema-1 JSON includes candidate namespace/class/method/command/type, raw value, hex payload, candidate input length, unknown units, validation reasons, and explicit safety fields:

- `Status`: `Not executable / not validated`
- `TimestampUtc`: record creation time in UTC
- `NoHardwareInvocation`, `NoWmiInvocation`: `true`
- `IsExecutable`, `WriteExecuted`, `WmiInvoked`, `FirstWriteGateSatisfied`: `false`
- `DeviceValidatedInputLength`: `null`
- `NormalControlValidated`, `UserFacingControlAllowed`: `false`
- `NormalFanControlDecision`: `NO-GO`

No baseline or observation is fabricated. Candidate JSON is not accepted as an execution request or imported as successful pulse/history evidence.

## Boundaries And Verification

No WMI invocation, EC/PawnIO/PwnIO/WinRing0 path, retry, fallback, SetFanMode, `0x37`, curve, loop, automatic control, slider, toggle, pulse/run button, or Settings/tray route was added. Existing SetFanMax approvals and behavior are unchanged. No admin or AC probe is needed for pure serialization.

Pure/local-file tests cover raw-byte bounds, missing/invalid/duplicate flags, mixed hardware commands, early startup interception, JSON safety fields, persistence and non-overwrite behavior, file-write failure, absence of UI/transport dependencies, and unchanged null input-length validation. The persistence task additionally authorizes the candidate-128 dry-run only; no hardware probe or fan experiment is permitted.

Persistence verification: solution build passed with 0 errors and 4 recurring NU1900 audit-source warnings; tests passed 276/276 with none skipped. The authorized candidate-128 dry-run initially exited with code 1 because the sandbox denied AppData writes; the approved retry exited with code 0 and printed the saved path. Disk inspection confirmed `128`, `80-80`, both no-invocation markers true, `WriteExecuted=false`, and null `DeviceValidatedInputLength`. No VictusX process remained. No WMI method, fan experiment, publishing, or release-artifact creation occurred.

## Next Safe Research Step

The [first-write preflight design](set-fan-level-first-write-preflight-design.md) defines required evidence, exact-device/safety gates, and recovery prerequisites. The executable candidate set remains empty; `128` / `80-80` proves serialization only. This design does not authorize or add execution to the dry-run.

Request existing exact-device ABI/units and recovery evidence to resolve the [static audit's conflicts](set-fan-level-abi-units-evidence-audit.md); no new probe, write, or candidate selection is proposed. Do not connect this record to a transport. Any executable SetFanLevel proposal needs a separate explicit decision and proof design; SetFanMax physical response and bounded pre-restore wait evidence do not authorize it.

- Developer-only four-byte SetFanMax Pulse/Hold: operational under existing explicit CLI gates only.
- SetFanLevel: serialization-only scaffold; execution blocked.
- FanMaxGet: inconclusive; FanGetLevel: raw-only.
- DeviceValidatedInputLength: null/unset.
- Normal/user-facing fan control: **NO-GO**.
