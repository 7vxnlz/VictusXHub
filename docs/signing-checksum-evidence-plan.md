# Signing And Checksum Evidence Plan

## Current Blocker State

Signing status for the initial VictusX HP Diagnostic preview is **unsigned — explicitly approved by maintainer**. No approved code-signing certificate is configured, and no signing was performed. The maintainer does not require repository/history restructuring for a signing provider. A future free, compatible signing path may be considered separately, but it is not a blocker for this initial preview.

The final distributable checksum is recorded below for the exact frozen ZIP. Clean-machine evidence remains open.

This plan does not authorize publishing, signing, checksum generation, or release readiness.

See [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) for the consolidated release-blocker status that signing/checksum evidence feeds into.

## Frozen Initial Preview Evidence

- Artifact: `VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip`
- Application version: `0.1.0-preview.1`
- Source/build commit: `ccd1f62865f1118bda476085f5d7144f787755bb`
- Target: `net10.0-windows`, `win-x64`, self-contained single-file application with external replaceable `NvAPIWrapper.dll`
- SDK/runtime baseline: SDK `10.0.400`; Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 `10.0.11`
- Signing status: **unsigned — explicitly approved by maintainer**; no signing was performed
- Artifact size: `49,848,534` bytes
- Final distributable SHA-256: `601B96C1934629AE8375641EA4C3E4E19DEA7FACB29C4C3349F24E4DB26DF1D8`
- Checksum evidence file: `VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip.sha256.txt`
- Checksum verification: matching results from PowerShell `Get-FileHash -Algorithm SHA256` and Windows `certutil -hashfile ... SHA256`
- Package inspection: original candidate and fresh ZIP extraction both returned `Preview package: GO`; all 22 extracted files byte-match the inspected candidate
- Candidate manifest SHA-256: `12D76C915F227D56B0DBE3221C94DD5C005D48387721E2AACACF638623E6931E` (package-content evidence, not the ZIP checksum)
- Notice review: linked to the reviewed [Third-Party Notices](../THIRD-PARTY-NOTICES.md) and exact 10.0.11 runtime evidence
- Verified: 2026-09-08; evidence recorded by Codex under the maintainer's explicit task authorization

This exact ZIP must be used unchanged for clean-machine validation. Any rebuild, repack, signing attempt, metadata change, or byte change invalidates this checksum and requires the candidate inspection, archive verification, and checksum evidence to be repeated.

## Expected Preview Artifact Types

The expected future artifact types are:

- portable ZIP, for example `VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip`;
- optional installer, if one is introduced later, for example `VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-setup.exe`;
- checksum file for each distributed artifact, using the artifact file name plus `.sha256.txt`.

The final artifact name and version must match the source revision, package metadata, user-facing notes, clean-machine validation record, and checksum/signing evidence.

## Signing Options

### Unsigned Preview

The maintainer explicitly approves the initial preview as unsigned. User-facing warnings remain required.

Risks:

- Windows SmartScreen and antivirus tooling may warn users.
- Users cannot verify publisher identity from an Authenticode signature.
- Tamper detection relies primarily on checksums and distribution-channel trust.

Required warning:

- State that the preview is unsigned, experimental, diagnostic-only, and should be verified with the published SHA-256 checksum before running.

### Self-Signed Or Internal Test Signing

Self-signed or internal test signing may be useful for private validation, but it should not be presented as trusted public publisher identity.

Risks:

- Users must install or trust a local/internal certificate to verify the signature.
- The signature may create a false sense of public trust.
- Certificate provenance and distribution must be controlled.

Required warning:

- State that the signature is for internal testing only unless a trusted public certificate is used.

### Trusted Code-Signing Certificate

A trusted code-signing certificate is the preferred public distribution path when available.

Risks:

- Certificate custody, expiration, timestamping, and revocation status must be managed.
- The signed artifact must be the same artifact that passed clean-machine validation.
- Any artifact change after signing invalidates the evidence chain and requires a new checksum.

Required warning:

- Users should still verify checksum and inspect the publisher/signature details before running the preview.

## Checksum Requirements

- Generate SHA-256 for every distributed artifact.
- Use one checksum file per artifact, named `<artifact-file-name>.sha256.txt`.
- Generate the checksum only after the artifact is final and after any signing step that changes artifact bytes.
- Record the exact checksum command/tool, operating system, tool version if available, artifact size, and generated digest.
- Verify the checksum independently before release.
- Do not update checksum text without rebuilding the evidence record.

Before creating a ZIP or installer, run `tools/verify-victusx-preview-package.ps1` against its source directory. The emitted per-file SHA-256 lines and deterministic manifest SHA-256 are package-content evidence only. They do not replace the required SHA-256 of the final ZIP/installer, which must be generated after any signing or other byte-changing step.

Example future checksum commands:

```powershell
Get-FileHash -Algorithm SHA256 .\VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip
certutil -hashfile .\VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip SHA256
```

Both checksum tools were run against the frozen initial-preview ZIP, and their matching result is recorded above.

## User Verification Guidance

Future users or testers should be told to:

1. Download the artifact and matching `.sha256.txt` from the same release.
2. Compute SHA-256 locally with a trusted tool.
3. Confirm the computed hash matches exactly.
4. If signed, inspect certificate subject, issuer, thumbprint, timestamp, and verification status.
5. Do not run the artifact if the checksum or signature evidence does not match.

## Release Evidence Required

For every released artifact, record:

- artifact name;
- artifact version;
- source/build commit SHA;
- artifact type: portable ZIP or installer;
- artifact size in bytes;
- signing status: unsigned, self-signed/internal, or trusted certificate;
- certificate subject, issuer, thumbprint, expiration, and timestamp authority if signed;
- signing command/tool and verification result if signed;
- SHA-256 hash;
- checksum command/tool and verification result;
- reviewer and review date;
- confirmation that the artifact is the same one used for clean-machine validation;
- reference to the clean-machine validation evidence record;
- confirmation that release launcher behavior uses only `--hp-victus`;
- confirmation that developer-only flags, logs, captured device data, and normal fan-control UI are absent.

## Fail-Closed Rules

- Missing SHA-256 evidence blocks release.
- Unknown signing status blocks release.
- A required signature that fails verification blocks release.
- A checksum mismatch blocks release.
- Artifact mutation after checksum/signing blocks release until evidence is regenerated.
- Evidence that does not identify the artifact name, version, source commit, reviewer, and date blocks release.
- Signing/checksum evidence for an artifact that did not pass clean-machine validation blocks release.
- Presence of developer-only flags, captured logs, or normal fan-control claims in the package blocks release.

## What Remains Blocking

- Signing is explicitly recorded as unsigned; no digital signature must be claimed.
- Clean-machine validation is not complete.
- Runtime dependency license/notice review and `NU1900` disposition are complete for the current baseline.
- Icon provenance and source wiring are recorded; retain their checkpoint linkage in final release evidence.
- Normal/user-facing fan control remains NO-GO.

## Recommended Next Safe Task

Run clean-machine validation against the exact frozen ZIP and record the artifact SHA-256 in that evidence. Do not rebuild or repack it.

See [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md) for the validation evidence that must match the signed/checksummed artifact.
