# .NET Runtime Notice Checklist

Updated: 2026-09-06

This checklist defines the artifact-specific evidence required before a self-contained VictusX HP Diagnostic preview can claim that .NET runtime license and notice obligations were reviewed. It does not authorize publishing and does not replace final package inspection.

## Current Status

- Source-side preparation: **Done**. The HP preview profile is documented as self-contained, and project/package notices are wired as external publish content.
- Runtime notice mapping: **Done for the verified baseline**. An ignored local `win-x64` candidate built with SDK 10.0.400 resolved Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 to 10.0.11. It passed the inspector's required-file, byte-match, identity, and deterministic-manifest checks for the three verbatim exact-pack files and the provenance record. Final distributable evidence remains a separate pending item.
- Preview package publish: **Blocked / NO-GO** until this checklist is completed against the actual package candidate.

## Checklist

| Item | Current status | Required evidence |
| --- | --- | --- |
| Publish mode | Ready to inspect | Confirm the actual release candidate is self-contained, RID-specific, and built from the intended HP preview profile. |
| RID and version match | Done for verified baseline | `net10.0-windows`, `win-x64`, SDK 10.0.400, and both selected runtime packs at 10.0.11 are recorded in the packaged runtime evidence. |
| Runtime components shipped | Pending artifact | Inventory every .NET runtime, Windows Desktop runtime, native host, native runtime, ICU/globalization, compression, diagnostics, and support file included in the package. |
| Runtime license text | Done for verified baseline | Verbatim exact-pack Core and Windows Desktop license files are externally packaged and byte-matched. |
| Runtime third-party notices | Done for verified baseline | The verbatim Core runtime-pack `THIRD-PARTY-NOTICES.TXT` is externally packaged and byte-matched. The exact Windows Desktop pack has no separately packaged notice file, so no substitute is inferred. |
| Native runtime files | Pending artifact | Identify native `.dll`, host, extraction, or support binaries that come from the .NET runtime packs and confirm they are covered by the matched notices. |
| Single-file extraction behavior | Pending artifact | Record whether runtime files are embedded, externally shipped, or extracted at runtime, and ensure notices remain externally available to users. |
| Notice placement | Source-ready, pending artifact | Confirm `LICENSE`, `THIRD-PARTY-NOTICES.md`, and `Assets/Licenses/**` are present outside the single-file executable in the final package. |
| Final artifact hash/reference | Pending artifact | Record final artifact SHA-256, package name, version, build commit SHA, reviewer, and review date. |

## Fail-Closed Rules

- Do not mark .NET runtime notice verification complete from source metadata alone.
- Do not assume a runtime-pack notice applies unless the artifact contains the matching runtime version/RID components.
- Do not treat package-library notices as covering .NET runtime components.
- Do not publish if runtime components are present but their matching license/notice set is missing or unidentified.

## Completion Criteria

This checklist can be marked complete only after a release candidate exists and the final artifact inspection records:

- exact .NET runtime and Windows Desktop runtime components included;
- matching license and third-party notice evidence for those components;
- external notice/license files present in the package;
- final artifact name, version, commit SHA, and SHA-256 hash;
- reviewer and date.

Runtime notice mapping is complete for the verified 10.0.11 `win-x64` baseline. The final package hash/reference, signing/checksum, icon attribution, and clean-machine validation remain independent **NO-GO** blockers.
