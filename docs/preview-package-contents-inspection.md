# Preview Package Contents Inspection

Updated: 2026-09-08

This checklist defines what must be inspected in a future VictusX HP Diagnostic preview package. It is source-only release preparation. It does not create, publish, sign, checksum, or approve any artifact.

## Current Status

- No final preview package has been selected. An ignored local candidate has been used only to verify the inspection workflow.
- Preview package publish remains **NO-GO**.
- The HP source profile is `Release` / `net10.0-windows` / `win-x64`, self-contained, single-file, explicitly untrimmed, and configured without debug symbols.
- `VictusX.exe` embeds the application and self-contained runtime. `NvAPIWrapper.dll` remains external and replaceable. `VictusX.dll.config`, `LICENSE`, `THIRD-PARTY-NOTICES.md`, `Assets/Licenses/**`, and `tools/run-victusx-hp-diagnostic.ps1` are external publish files.
- `tools/verify-victusx-preview-package.ps1` performs the deterministic file/layout checks below against an explicit directory. Final signing/checksum, package notice review, and clean-machine evidence remain manual blockers.

## Automated Inspection

Run from the repository root against an explicit candidate directory:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-victusx-preview-package.ps1 -PublishDirectory <candidate-directory>
```

The tool reads the candidate without modifying it. It emits `PASS`, `FAIL`, and `WARN` records, per-file SHA-256 evidence, a deterministic candidate-manifest SHA-256, and exits nonzero on any `FAIL`. `Preview package: GO` means only that its automated checks passed; it is not approval to publish and does not close any warning or manual evidence gate.

The exact allowed layout is:

- package root: `VictusX.exe`, `VictusX.dll.config`, `NvAPIWrapper.dll`, `LICENSE`, and `THIRD-PARTY-NOTICES.md`;
- `tools/run-victusx-hp-diagnostic.ps1` using only `--hp-victus`;
- the twelve reviewed files under `Assets/Licenses/` recorded by `app/Assets/Licenses/LICENSE-SOURCES.md`.

Any additional file is fail-closed until the publish contract and inspector are deliberately reviewed together.

## Inspection Checklist

| Item | Status | Future inspection requirement |
| --- | --- | --- |
| VictusX executable identity | Automated, final candidate pending | Confirm `VictusX.exe`, Windows executable header, readable VictusX product metadata, and extractable icon resource. Final visual confirmation remains part of clean-machine validation. |
| Final `VictusX.ico` source and wiring | Source complete; final candidate pending | The approved PNG source, generated ICO hashes and frames, factual provenance, `ApplicationIcon`, and single HP tray resource are verified in source. Confirm the exact candidate visually during clean-machine validation. |
| Expected runtime files only | Automated, runtime notices pending | Require the single executable, `VictusX.dll.config`, and only the external `NvAPIWrapper.dll`; reject every unexpected file/binary. Embedded runtime-pack version/notice mapping remains manual. |
| No test artifacts | Automated | Reject test directories/assemblies, testhost, xUnit, test-platform, coverage, and test-result artifacts. |
| No source-only/reference repo files | Automated | Reject source/build files, `.git`, `.github`, reference repositories, context packs, and unexpected tooling. |
| No unexpected G-Helper branding artifacts | Blocked by missing artifact | Confirm no user-facing package asset, shortcut, executable property, title, readme, or notice text presents the preview as G-Helper or ASUS control software. Review inherited non-English/Turkish resource strings; compatibility identifiers may remain internal. |
| `THIRD-PARTY-NOTICES.md` presence | Automated presence/match; review pending | Require the external file and byte-match it to the repository source. Promotion from draft remains manual. |
| License/notice files | Automated presence/match; review pending | Require `LICENSE` and every reviewed `Assets/Licenses/**` file and byte-match them to repository sources. Attribution/runtime review remains manual. |
| Config/log folders not prepopulated | Blocked by missing artifact | Confirm `%APPDATA%`-style config/log/report/export directories are created at runtime only and the package does not ship local user data, FanExperiments logs, cached HP reports, machine paths, or device captures. |
| No debug symbols unless explicitly intended | Automated | Reject `.pdb`, dump, trace, and log artifacts. |
| Preview launcher arguments | Automated | Require the source-matching launcher and exactly one application argument: `--hp-victus`. |
| Read-only safety notes | Pending final publish | Include reviewed preview safety notes that state HP Diagnostic mode is read-only and normal fan/performance control is unavailable. |
| Checksum preparation | Automated manifest; final artifact pending | Emit SHA-256 for every candidate file and a deterministic manifest digest. The final ZIP/installer checksum and signing record must be generated later from final bytes. |
| Clean-machine evidence linkage | Pending final publish | Confirm the inspected artifact is the exact artifact used for clean-machine validation and signing/checksum evidence. |

## Fail-Closed Rules

- Missing final artifact means package contents inspection cannot be completed.
- Any developer-only log, local device capture, machine path, source checkout, reference repository, or fan experiment output in the package blocks release.
- Any normal fan-control UI, fan slider, fan toggle, fan curve, pulse/run button, HP WMI write flag, EC/PawnIO fallback, or performance write claim blocks release.
- Any package contents that do not match reviewed license/notices, signing/checksum evidence, and clean-machine validation keeps preview publish **NO-GO**.

## Current Decision

The publish profile and automated directory inspector are source-ready. Preview package publishing remains **NO-GO** until the exact final candidate passes this tool and final package notice review, signing decision/final artifact checksum, and clean-machine validation are complete.
