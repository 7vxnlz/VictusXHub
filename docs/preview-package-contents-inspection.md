# Preview Package Contents Inspection

Updated: 2026-09-06

This checklist defines what must be inspected in a future VictusX HP Diagnostic preview package. It is source-only release preparation. It does not create, publish, sign, checksum, or approve any artifact.

## Current Status

- No preview package artifact exists.
- Preview package publish remains **NO-GO**.
- The HP source profile is `Release` / `net10.0-windows` / `win-x64`, self-contained, single-file, explicitly untrimmed, and configured without debug symbols.
- `LICENSE`, `THIRD-PARTY-NOTICES.md`, and `tools/run-victusx-hp-diagnostic.ps1` are wired as external publish files. The profile fails before publish while the reviewed `app/Assets/VictusX.ico` is absent.
- Most rows are blocked by the missing final artifact because they require inspecting packaged files, executable metadata, and launch behavior.

## Inspection Checklist

| Item | Status | Future inspection requirement |
| --- | --- | --- |
| VictusX executable identity | Blocked by missing artifact | Confirm the packaged executable is named `VictusX.exe` and its product/version/description metadata presents VictusX HP Diagnostic preview identity without implying HP endorsement. |
| Final `VictusX.ico` presence | Blocked by missing artifact | Confirm `app/Assets/VictusX.ico` was supplied from an original or properly licensed asset and is reflected in Explorer, taskbar, tray, and window identity. |
| Expected runtime files only | Blocked by missing artifact | Compare the final contents against the selected self-contained single-file `win-x64` model; investigate any unexpected executable, DLL, extracted native library, config, or data file. |
| No test artifacts | Blocked by missing artifact | Confirm no test assemblies, testhost files, coverage files, xUnit runner assets, test result folders, or test-only dependency payloads are included. |
| No source-only/reference repo files | Blocked by missing artifact | Confirm the package excludes repository source, docs-only planning files unless intentionally included, `.git`, local reference repositories, context packs, and workspace tooling. |
| No unexpected G-Helper branding artifacts | Blocked by missing artifact | Confirm no user-facing package asset, shortcut, executable property, title, readme, or notice text presents the preview as G-Helper or ASUS control software. Review inherited non-English/Turkish resource strings; compatibility identifiers may remain internal. |
| `THIRD-PARTY-NOTICES.md` presence | Pending final publish | Source wiring is present. Promote the draft to reviewed release evidence before publishing, then confirm the external file matches final package contents. |
| License/notice files | Pending final publish | Source wiring includes external `LICENSE` and `THIRD-PARTY-NOTICES.md`. Confirm required G-Helper modified-project attribution, dependency texts, HP/OMEN trademark disclaimer, and icon attribution are complete. |
| Config/log folders not prepopulated | Blocked by missing artifact | Confirm `%APPDATA%`-style config/log/report/export directories are created at runtime only and the package does not ship local user data, FanExperiments logs, cached HP reports, machine paths, or device captures. |
| No debug symbols unless explicitly intended | Ready to inspect | Profile sets `DebugType=None`, `DebugSymbols=false`, and `CopyOutputSymbolsToPublishDirectory=false`; confirm no `.pdb`, symbol, map, dump, or build log appears. |
| Preview launcher arguments | Ready to inspect | The external launcher is source-wired and uses only `--hp-victus`; confirm it excludes read-only probe, fan experiment, dry-run, hold/pulse, and developer-only research flags. |
| Read-only safety notes | Pending final publish | Include reviewed preview safety notes that state HP Diagnostic mode is read-only and normal fan/performance control is unavailable. |
| Signing/checksum evidence files | Pending final publish | Confirm each distributed artifact has matching SHA-256 evidence and signing status evidence, generated after the artifact is final. |
| Clean-machine evidence linkage | Pending final publish | Confirm the inspected artifact is the exact artifact used for clean-machine validation and signing/checksum evidence. |

## Fail-Closed Rules

- Missing final artifact means package contents inspection cannot be completed.
- Any developer-only log, local device capture, machine path, source checkout, reference repository, or fan experiment output in the package blocks release.
- Any normal fan-control UI, fan slider, fan toggle, fan curve, pulse/run button, HP WMI write flag, EC/PawnIO fallback, or performance write claim blocks release.
- Any package contents that do not match reviewed license/notices, signing/checksum evidence, and clean-machine validation keeps preview publish **NO-GO**.

## Current Decision

The publish profile is source-configured and fail-closed, but VictusX is not source-ready for the first preview publish until the final icon is supplied, notices are promoted from draft and assembled, and localized inherited branding is dispositioned. Artifact-specific inspection, signing/checksums, and clean-machine validation follow on the exact candidate. Preview package publishing remains **NO-GO**.
