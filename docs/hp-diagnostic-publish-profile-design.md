# HP Diagnostic Publish Profile Design

## Artifact Contract

- Intended name: `VictusX-HP-Diagnostic-v{SemVer}-win-x64-portable.zip`.
- Example preview: `VictusX-HP-Diagnostic-v0.1.0-preview.1-win-x64-portable.zip`.
- The artifact is an HP read-only diagnostic preview, not a hardware-control release.

## Runtime Mode

The supported entry point must launch `VictusX.exe` with exactly:

```text
--hp-victus
```

`--hp-wmi-readonly-test` must not appear in the publish profile, launcher, shortcut, or release instructions. It enables elevated developer-only probes and is unnecessary for the cached, local-file-backed Diagnostic dashboard.

## Deployment Recommendation

Use a self-contained `win-x64` portable preview first. Its larger size is preferable to requiring users to install the matching .NET Windows Desktop Runtime, and it gives cleaner test results on otherwise unprepared machines. Keep single-file packaging only if startup, extraction behavior, report paths, and shutdown pass packaged smoke tests.

Start with a portable ZIP rather than an installer. An installer should wait for stable product identity, signing, upgrade/uninstall behavior, and a proven safe shortcut contract.

## Version and App Metadata

- Use a SemVer prerelease version and include it in the artifact name.
- Set consistent numeric assembly/file versions and a matching informational version with the short source commit.
- Set explicit product name, file description, publisher/project identity, copyright, and license metadata.
- Identify the product as `VictusX HP Diagnostic Preview` and state that it is read-only.
- Document compatibility between the app version and `ReportSchemaVersion`.
- Replace the inherited G-Helper icon with an original VictusX icon; do not use HP trademarks or imply HP endorsement.

## Launcher and Shortcut Contract

- Provide one clearly named HP Diagnostic launcher or shortcut that resolves `VictusX.exe` relative to itself and appends only `--hp-victus`.
- Do not promote a bare `VictusX.exe` launch as the preview entry because it preserves default G-Helper/ASUS behavior.
- Do not request administrator rights, register startup, configure an updater, or include developer/test flags.
- Keep the inherited default mode intact outside the dedicated HP entry point.

## Integrity Requirements

- Produce a SHA-256 checksum for the final ZIP and publish the same value beside the download.
- Authenticode-sign executables when a verified publisher certificate is available, using a timestamped signature.
- Verify signatures and checksums after packaging, and record the clean source revision and build environment.
- Run malware scanning and ensure symbols, logs, device captures, and machine-specific paths are excluded.

## Pre-release Safety Checklist

- Dedicated HP profile contains no WMI test or write flags.
- Packaged entry supplies exactly `--hp-victus`.
- Distinct icon, explicit metadata, version, license, and notices are present.
- Clean Windows-machine launch reaches the Diagnostic/Quit-only shell without elevation.
- All explicit invocation fields remain `Attempted=false`.
- Cached report loading, copy, reload, folder open, and sanitized export remain local-only.
- No fan, performance, GPU, battery, lighting, updater, or ASUS control surface is exposed.
- Missing/corrupt reports render safely; report and export paths are correct.
- Normal close and Quit produce no exception dialog or remaining process.
- Artifact name, embedded version, checksum, and signature agree.

## Current Blockers

Publishing remains blocked by the absence of a dedicated HP publish profile and launcher, an inherited G-Helper icon, inconsistent version metadata, unresolved packaged single-file validation, missing signing/checksum automation, incomplete package notices, and no clean-machine packaged smoke test. `SetFanMax` remains NO-GO/design-only, and no fan or performance control may be claimed.

## Next Safe Implementation Task

Add a non-publishing `HpDiagnosticPreview.pubxml` and explicit MSBuild metadata with a validation check that rejects developer/test flags. Review its evaluated properties and tests without running `dotnet publish` or creating artifacts.
