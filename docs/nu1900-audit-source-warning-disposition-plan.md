# NU1900 Audit-Source Warning Disposition Plan

## Current Observed Warning State

Local `dotnet build VictusX.sln` and `dotnet test VictusX.sln` currently pass, but they repeatedly emit `NU1900` warnings while attempting to retrieve package vulnerability data from `https://api.nuget.org/v3/index.json`.

The recurring warnings mean the build succeeds, but the local SDK/build environment has not produced a clean vulnerability-audit signal for the current dependency graph.

## Why NU1900 Matters For Preview Readiness

The HP Diagnostic preview cannot treat a successful compile and test run as package vulnerability audit completion. Preview packaging needs evidence that package vulnerability metadata was retrieved successfully, or a documented maintainer decision that explains how vulnerability review was performed by another acceptable route.

`NU1900` does not prove that a package is vulnerable. It proves that audit data retrieval failed or was incomplete for that restore/build context.

## Build Pass Versus Audit Confidence

- Build/test pass: source compiles and tests execute successfully.
- Audit confidence: vulnerability metadata was reachable and evaluated for the package graph, or an alternate reviewed vulnerability check was completed and recorded.

These are separate release signals. The current state has the first signal, but not the second.

## Known External Verification

The user previously verified that nuget.org and the vulnerabilities endpoint returned HTTP 200 outside this build/test result. That is useful environmental evidence, but it does not by itself prove that the local SDK restore/build path successfully consumed vulnerability data during the release-candidate build.

## Possible Causes To Investigate

- SDK, network, proxy, TLS, DNS, or audit endpoint timing differences between direct HTTP checks and `dotnet` restore/build.
- NuGet cache, HTTP cache, or local configuration behavior.
- Offline or intermittent vulnerability data retrieval during restore.
- Local environment issue specific to this machine, SDK, user profile, certificate store, or NuGet configuration.
- Temporary nuget.org service behavior at build time.

## Acceptable Evidence To Mark Disposed

The warning can be marked disposed only after evidence such as:

- `dotnet restore VictusX.sln --force --no-cache` completes without `NU1900`.
- `dotnet build VictusX.sln` completes without `NU1900` for the release-candidate source.
- `dotnet test VictusX.sln` completes without `NU1900` for the release-candidate source.
- `dotnet list app\VictusX.csproj package --vulnerable --include-transitive` completes with interpretable results.
- `dotnet list tests\VictusX.Tests\VictusX.Tests.csproj package --vulnerable --include-transitive` completes with interpretable results if test/developer artifacts are in scope.
- `dotnet nuget list source` confirms the expected package sources for the release environment.
- CI or clean-machine validation captures the same clean audit behavior.
- Any vulnerability findings are recorded and separately dispositioned by the maintainer.

## Unacceptable Evidence

Do not mark `NU1900` disposed based only on:

- build/test passing while `NU1900` still appears;
- a browser or manual HTTP 200 check alone;
- package popularity or assumed safety;
- local cache contents without a clean restore;
- silent warning suppression;
- disabling package audit globally without a documented maintainer decision;
- absence of known vulnerabilities in memory or informal notes.

## Fail-Closed Release Decision

The HP Diagnostic preview release remains blocked while `NU1900` recurs in release-candidate restore/build/test output unless an explicit documented maintainer disposition records an alternate vulnerability-review path and its evidence.

Do not suppress `NU1900` or disable package audit as a release workaround without a documented decision tied to the release candidate.

## Recommended Local Verification Commands

Run these in a future network-connected local environment before packaging:

```powershell
dotnet nuget list source
dotnet restore VictusX.sln --force --no-cache
dotnet build VictusX.sln
dotnet test VictusX.sln
dotnet list app\VictusX.csproj package --vulnerable --include-transitive
dotnet list tests\VictusX.Tests\VictusX.Tests.csproj package --vulnerable --include-transitive
```

Record command output, SDK version, NuGet sources, date, reviewer, and any remaining warning or vulnerability findings.

## Recommended CI And Clean-Machine Verification

Before preview packaging, repeat vulnerability-audit verification in:

- CI or a reproducible build runner with known NuGet sources;
- a clean Windows machine or VM used for preview validation;
- the same source revision selected for the package candidate.

The clean-machine record should distinguish restore/build audit status from runtime smoke-test behavior.

## Relationship To Signing And Checksums

`NU1900` disposition must be tied to the same source revision and artifact candidate used for signing/checksum evidence. See [Signing And Checksum Evidence Plan](signing-checksum-evidence-plan.md). A clean checksum does not prove vulnerability audit completion, and a clean vulnerability-audit result does not replace artifact signing/checksum evidence.

## Current Blocker Status

`NU1900` disposition is open. Build and test currently pass with recurring audit-source warnings, so package vulnerability audit confidence is incomplete. This plan does not create a package, publish artifacts, change dependency versions, suppress warnings, or declare the warning fixed.

## Recommended Next Safe Task

Run the recommended clean restore and vulnerability-list commands in a network-connected release-prep environment, then record the evidence without publishing artifacts.
