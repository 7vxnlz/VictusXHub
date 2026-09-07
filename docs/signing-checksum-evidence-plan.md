# Signing And Checksum Evidence Plan

## Current Blocker State

Signing and checksum evidence remains open for the future VictusX HP Diagnostic preview. No preview package exists, no release artifact has been selected, nothing has been signed, and no final checksums have been generated.

This plan does not authorize publishing, signing, checksum generation, or release readiness.

See [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) for the consolidated release-blocker status that signing/checksum evidence feeds into.

## Expected Preview Artifact Types

The expected future artifact types are:

- portable ZIP, for example `VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-portable.zip`;
- optional installer, if one is introduced later, for example `VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-setup.exe`;
- checksum file for each distributed artifact, using the artifact file name plus `.sha256.txt`.

The final artifact name and version must match the source revision, package metadata, user-facing notes, clean-machine validation record, and checksum/signing evidence.

## Signing Options

### Unsigned Preview

An unsigned preview may be acceptable only if the maintainer explicitly approves it and user-facing warnings are clear.

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

Example future checksum commands:

```powershell
Get-FileHash -Algorithm SHA256 .\VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-portable.zip
certutil -hashfile .\VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-portable.zip SHA256
```

These commands are examples for a future release candidate only. They were not run as part of this source-only plan.

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

- No final package artifact exists.
- No signing decision has been made.
- No signing evidence exists.
- No final SHA-256 checksum exists.
- Final package contents are unavailable.
- Clean-machine validation is not complete.
- Runtime dependency license/notice review and `NU1900` disposition remain open.
- Icon attribution remains pending a future approved VictusX icon asset.
- Normal/user-facing fan control remains NO-GO.

## Recommended Next Safe Task

Define a release-candidate evidence record template that combines [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md), package contents, `THIRD-PARTY-NOTICES.md` review status, `NU1900` disposition, signing/checksum evidence, and clean-machine validation without publishing artifacts.

See [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md) for the validation evidence that must match the signed/checksummed artifact.
