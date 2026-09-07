# .NET Runtime Notice Checklist

Updated: 2026-09-06

This checklist defines the artifact-specific evidence required before a self-contained VictusX HP Diagnostic preview can claim that .NET runtime license and notice obligations were reviewed. It does not authorize publishing and does not replace final package inspection.

## Current Status

- Source-side preparation: **Done**. The HP preview profile is documented as self-contained, and project/package notices are wired as external publish content.
- Artifact-specific verification: **Pending**. No release candidate has been published or inspected, so the exact runtime components and notices shipped by the artifact are not yet known.
- Preview package publish: **Blocked / NO-GO** until this checklist is completed against the actual package candidate.

## Checklist

| Item | Current status | Required evidence |
| --- | --- | --- |
| Publish mode | Ready to inspect | Confirm the actual release candidate is self-contained, RID-specific, and built from the intended HP preview profile. |
| RID and version match | Pending artifact | Record `TargetFramework`, `RuntimeIdentifier`, .NET SDK version, runtime pack versions, app version, build commit SHA, and artifact name. |
| Runtime components shipped | Pending artifact | Inventory every .NET runtime, Windows Desktop runtime, native host, native runtime, ICU/globalization, compression, diagnostics, and support file included in the package. |
| Runtime license text | Pending artifact | Match the shipped .NET runtime components to the applicable Microsoft .NET license text from the exact runtime pack/source evidence. |
| Runtime third-party notices | Pending artifact | Include or reference the third-party notices supplied by the matching .NET runtime/runtime-pack materials. |
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

Until then, the .NET runtime notice item remains artifact-specific pending work, and preview package publishing remains **NO-GO**.
