# Signing and Checksum Workflow

## Signing Scope

This workflow applies only to a future VictusX HP diagnostic preview package after a release candidate artifact exists. It does not authorize publishing, signing, or artifact creation from the current source tree.

Sign only the final executable/package selected for release. Do not sign intermediate build output, local debug builds, logs, reports, or developer-only packages.

## Checksum Scope

Generate checksums only for the final preview artifact that testers and users will download, such as the portable ZIP or installer. The checksum must match the artifact that passed clean-machine validation.

## Expected Future Artifact Names

- Portable ZIP: `VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-portable.zip`
- Optional installer, if introduced later: `VictusX-HP-Diagnostic-0.1.0-preview.1-win-x64-setup.exe`
- Checksum file: same artifact name with `.sha256.txt`
- Signature evidence: record whether the executable, installer, or ZIP is signed and which certificate was used.

## Recommended Checksum Algorithm

Use SHA-256. Record the exact command/tool used, the generated digest, the artifact file name, artifact size, and review date.

## Signing Evidence to Record

- signing target file name and hash before signing;
- certificate subject, issuer, thumbprint, and expiration date;
- timestamp authority, if used;
- signing command/tool and exit result;
- verification command/tool and result;
- reviewer and date;
- whether unsigned preview distribution is intentionally blocked or explicitly approved.

## Checksum Evidence to Record

- artifact file name and version;
- SHA-256 digest;
- artifact size in bytes;
- checksum command/tool;
- source commit or release candidate identifier;
- confirmation that the artifact is the same one used for clean-machine validation.

## User and Tester Verification Steps

1. Download the preview artifact and `.sha256.txt` from the same release.
2. Compute SHA-256 locally with a trusted system tool.
3. Confirm the digest matches exactly.
4. If signed, inspect the signature publisher, certificate chain, timestamp, and verification status.
5. Do not run the artifact if the checksum or signature evidence does not match.

## Failure Handling

If signing fails, checksum verification fails, certificate details are unexpected, timestamps are missing when required, or the artifact changes after validation, discard the candidate and restart the release validation workflow from a fresh artifact.

Do not patch checksums or signature notes after the fact without rebuilding the evidence trail.

## Release Blocking Criteria

Release remains blocked if:

- no SHA-256 checksum exists for the final artifact;
- checksum evidence does not match the validated artifact;
- signing status is unknown or undocumented;
- a required signature is missing or fails verification;
- artifact names, source version, or validation records do not align;
- developer-only flags or hardware-control claims appear in the package or release notes.

## Recommended Next Safe Task

Create a source-only `THIRD-PARTY-NOTICES.md` draft from the dependency inventory and package license review workflow, leaving legal conclusions marked pending review.
