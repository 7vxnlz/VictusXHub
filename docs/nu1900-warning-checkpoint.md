# NU1900 Warning Checkpoint

## Current Disposition

Status: **Cleared for the current source verification on 2026-09-06; repeat for the final release candidate.**

The warning was reproduced with a forced no-cache diagnostic restore inside the restricted coding sandbox. DNS resolved `api.nuget.org`, but TCP 443 and `Invoke-WebRequest` failed with Windows socket error `10013` (access denied by execution-environment policy). No proxy variables or WinHTTP proxy were configured. The only enabled NuGet source in the user configuration was the canonical `https://api.nuget.org/v3/index.json`; the repository has no `NuGet.Config`, audit suppression, or audit-disable setting.

The same host outside that socket restriction returned HTTP 200 for the NuGet service index and `v3/vulnerabilities/index.json`. A forced no-cache restore with .NET SDK 10.0.302 fetched the vulnerability index plus its base/update pages successfully and completed with zero warnings. Subsequent required restore/build/test commands also completed without `NU1900`. The failure was therefore environmental network denial, not a package-specific finding, TLS failure, bad source, or repository configuration defect.

## Vulnerability Evidence

- `dotnet list app\VictusX.csproj package --vulnerable --include-transitive`: no vulnerable package reported from nuget.org.
- `dotnet list tests\VictusX.Tests\VictusX.Tests.csproj package --vulnerable --include-transitive`: no vulnerable package reported from nuget.org.
- Audit remained enabled (`NuGetAudit=true`, effective audit mode `all`); no warning suppression or package-version change was made.

These results mean no vulnerability known to the retrieved NuGet advisory snapshot matched the current direct/transitive graphs. They are time-bound evidence, not a permanent claim that packages can never become vulnerable.

## Preview Acceptance Criteria

The final preview candidate is accepted only when its exact source revision records:

- a network-capable `dotnet restore VictusX.sln --force --no-cache` without `NU1900`;
- clean build and test output without `NU1900`;
- interpretable direct/transitive `--vulnerable --include-transitive` results for both projects;
- documented resolution of every reported advisory, if any; and
- command output, SDK version, source revision, reviewer, and date retained with release evidence.

If network policy blocks NuGet audit retrieval, move the audit step to a trusted network-capable CI/clean-machine environment. Do not use a warm cache alone as release evidence. If `NU1900` appears or vulnerability output is indeterminate for the release candidate, preview publishing returns to **NO-GO** until the audit succeeds or an explicit maintainer alternate review records authoritative evidence.

No repository fix was appropriate: adding a duplicate source, disabling audit, or suppressing `NU1900` would hide an environment boundary rather than repair it. See the full [NU1900 Audit-Source Warning Disposition Plan](nu1900-audit-source-warning-disposition-plan.md).
