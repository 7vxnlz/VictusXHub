[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDirectory
)

$ErrorActionPreference = "Stop"
$script:FailureCount = 0

function Write-Pass([string]$Name, [string]$Message) {
    Write-Output "PASS ${Name}: $Message"
}

function Write-Fail([string]$Name, [string]$Message) {
    $script:FailureCount++
    Write-Output "FAIL ${Name}: $Message"
}

function Write-Warn([string]$Name, [string]$Message) {
    Write-Output "WARN ${Name}: $Message"
}

trap {
    Write-Fail "inspection-error" ($_.Exception.Message -replace '\s+', ' ')
    Write-Output "Preview package: NO-GO"
    exit 1
}

Add-Type -AssemblyName System.Drawing

function Get-NormalizedRelativePath([string]$Root, [string]$FullName) {
    return $FullName.Substring($Root.Length).TrimStart('\', '/').Replace('\', '/')
}

function Get-Sha256([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return [BitConverter]::ToString($sha256.ComputeHash($stream)).Replace("-", "")
    } finally {
        $sha256.Dispose()
        $stream.Dispose()
    }
}

$resolvedDirectory = Resolve-Path -LiteralPath $PublishDirectory -ErrorAction SilentlyContinue
if ($null -eq $resolvedDirectory -or -not (Test-Path -LiteralPath $resolvedDirectory.Path -PathType Container)) {
    Write-Fail "input" "publish directory '$PublishDirectory' does not exist."
    Write-Output "Preview package: NO-GO"
    exit 1
}

$packageRoot = [IO.Path]::GetFullPath($resolvedDirectory.Path).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$licenseFiles = @(
    "FftSharp-LICENSE.txt",
    "GPL-3.0.txt",
    "HidSharpCore-LICENSE.txt",
    "HidSharpCore-NOTICE.txt",
    "LICENSE-SOURCES.md",
    "Microsoft.NET-10.0.11-win-x64-RUNTIME-EVIDENCE.md",
    "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-LICENSE.TXT",
    "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-THIRD-PARTY-NOTICES.TXT",
    "Microsoft.WindowsDesktop.App.Runtime.win-x64-10.0.11-LICENSE.TXT",
    "NAudio-LICENSE.txt",
    "NvAPIWrapper-LGPL-3.0.txt",
    "NvAPIWrapper-README.txt",
    "PawnIO.Modules-0.2.2-LGPL-2.1.txt",
    "System.Management-LICENSE.txt",
    "System.Management-THIRD-PARTY-NOTICES.txt",
    "TaskScheduler-LICENSE.txt",
    "WinForms.DataVisualization-LICENSE.txt"
)
$requiredFiles = @(
    "VictusXHub.exe",
    "VictusXHub.dll.config",
    "NvAPIWrapper.dll",
    "LICENSE",
    "THIRD-PARTY-NOTICES.md",
    "tools/run-victusxhub-hp-diagnostic.ps1"
) + @($licenseFiles | ForEach-Object { "Assets/Licenses/$_" })

$entries = @(
    Get-ChildItem -LiteralPath $packageRoot -File -Recurse -Force |
        ForEach-Object {
            [pscustomobject]@{
                File = $_
                RelativePath = Get-NormalizedRelativePath $packageRoot $_.FullName
            }
        } |
        Sort-Object -Property RelativePath
)
$actualPaths = @{}
foreach ($entry in $entries) {
    $actualPaths[$entry.RelativePath] = $entry.File.FullName
}

$missing = @($requiredFiles | Where-Object { -not $actualPaths.ContainsKey($_) })
if ($missing.Count -eq 0) {
    Write-Pass "required-files" "VictusXHub executable, launcher, NvAPIWrapper sidecar, and all required notices are present."
} else {
    Write-Fail "required-files" ("missing: " + ($missing -join ", "))
}

$allowedPaths = @{}
foreach ($path in $requiredFiles) { $allowedPaths[$path] = $true }
$unexpected = @($entries.RelativePath | Where-Object { -not $allowedPaths.ContainsKey($_) })
if ($unexpected.Count -eq 0) {
    Write-Pass "package-layout" "the candidate contains only the source-configured publish files."
} else {
    Write-Fail "package-layout" ("unexpected files: " + ($unexpected -join ", "))
}

$nvApiPaths = @($entries.RelativePath | Where-Object { [IO.Path]::GetFileName($_) -ieq "NvAPIWrapper.dll" })
if ($nvApiPaths.Count -eq 1 -and $nvApiPaths[0] -ceq "NvAPIWrapper.dll") {
    Write-Pass "nvapi-sidecar" "NvAPIWrapper.dll is independently replaceable beside VictusXHub.exe."
} else {
    Write-Fail "nvapi-sidecar" "NvAPIWrapper.dll must exist exactly once beside VictusXHub.exe; found: $($nvApiPaths -join ', ')."
}

$launcherRelativePath = "tools/run-victusxhub-hp-diagnostic.ps1"
if ($actualPaths.ContainsKey($launcherRelativePath)) {
    $launcher = Get-Content -Raw -LiteralPath $actualPaths[$launcherRelativePath]
    $launcherArguments = @([regex]::Matches($launcher, "--[a-z0-9-]+", [Text.RegularExpressions.RegexOptions]::IgnoreCase) |
        ForEach-Object { $_.Value.ToLowerInvariant() } | Sort-Object -Unique)
    if ($launcherArguments.Count -eq 1 -and $launcherArguments[0] -ceq "--hp-victus") {
        Write-Pass "launcher" "the only application argument is --hp-victus."
    } else {
        Write-Fail "launcher" ("expected only --hp-victus; found: " + ($launcherArguments -join ", "))
    }
}

if ($actualPaths.ContainsKey("VictusXHub.dll.config")) {
    $sourceConfigHash = Get-Sha256 (Join-Path $repositoryRoot "app/app.config")
    $candidateConfigHash = Get-Sha256 $actualPaths["VictusXHub.dll.config"]
    if ($sourceConfigHash -ceq $candidateConfigHash) {
        Write-Pass "application-config" "VictusXHub.dll.config byte-matches app/app.config."
    } else {
        Write-Fail "application-config" "VictusXHub.dll.config does not match app/app.config."
    }
}

$sourceExtensions = @(".cs", ".csproj", ".sln", ".slnx", ".props", ".targets", ".resx", ".xaml", ".user", ".suo")
$sourceArtifacts = @($entries.RelativePath | Where-Object { $sourceExtensions -contains [IO.Path]::GetExtension($_).ToLowerInvariant() })
if ($sourceArtifacts.Count -eq 0) { Write-Pass "source-artifacts" "none found." }
else { Write-Fail "source-artifacts" ($sourceArtifacts -join ", ") }

$testArtifacts = @($entries.RelativePath | Where-Object {
    $_ -match '(^|/)(tests?|TestResults|coverage)(/|$)' -or
    $_ -match '(^|/)(testhost|xunit|Microsoft\.TestPlatform|Microsoft\.CodeCoverage)' -or
    [IO.Path]::GetExtension($_) -in @(".trx", ".coverage", ".coveragexml")
})
if ($testArtifacts.Count -eq 0) { Write-Pass "test-artifacts" "none found." }
else { Write-Fail "test-artifacts" ($testArtifacts -join ", ") }

$referenceArtifacts = @($entries.RelativePath | Where-Object { $_ -match '(^|/)(\.git|\.github|references|context-packs)(/|$)' })
if ($referenceArtifacts.Count -eq 0) { Write-Pass "reference-artifacts" "none found." }
else { Write-Fail "reference-artifacts" ($referenceArtifacts -join ", ") }

$symbols = @($entries.RelativePath | Where-Object { [IO.Path]::GetExtension($_) -ieq ".pdb" })
if ($symbols.Count -eq 0) { Write-Pass "symbols" "no PDB files found." }
else { Write-Fail "symbols" ($symbols -join ", ") }

$developerArtifacts = @($entries.RelativePath | Where-Object {
    [IO.Path]::GetExtension($_) -in @(".log", ".etl", ".dmp", ".dump", ".trace") -or
    $_ -match '(^|/)(logs?|FanExperiments)(/|$)' -or
    $_ -match '(hp-victus-capability|fanexperiment|setfanmax|setfanlevel)'
})
if ($developerArtifacts.Count -eq 0) { Write-Pass "developer-artifacts" "no logs, dumps, captures, or fan-research evidence found." }
else { Write-Fail "developer-artifacts" ($developerArtifacts -join ", ") }

$mmiArtifacts = @($entries.RelativePath | Where-Object {
    $_ -match 'Microsoft\.Management\.Infrastructure' -or $_ -match '(^|/)(libmi|mi)\.dll$'
})
if ($mmiArtifacts.Count -eq 0) { Write-Pass "mmi-artifacts" "none found." }
else { Write-Fail "mmi-artifacts" ($mmiArtifacts -join ", ") }

$noticeSources = @{
    "LICENSE" = Join-Path $repositoryRoot "LICENSE"
    "THIRD-PARTY-NOTICES.md" = Join-Path $repositoryRoot "THIRD-PARTY-NOTICES.md"
    $launcherRelativePath = Join-Path $repositoryRoot "tools/run-victusxhub-hp-diagnostic.ps1"
}
foreach ($name in $licenseFiles) {
    $noticeSources["Assets/Licenses/$name"] = Join-Path $repositoryRoot "app/Assets/Licenses/$name"
}
$noticeMismatches = @()
foreach ($relativePath in $noticeSources.Keys) {
    if ($actualPaths.ContainsKey($relativePath)) {
        $sourceHash = Get-Sha256 $noticeSources[$relativePath]
        $candidateHash = Get-Sha256 $actualPaths[$relativePath]
        if ($sourceHash -cne $candidateHash) { $noticeMismatches += $relativePath }
    }
}
if ($noticeMismatches.Count -eq 0 -and $missing.Count -eq 0) {
    Write-Pass "notice-matching" "launcher and packaged license/notice files byte-match the reviewed repository sources."
} elseif ($noticeMismatches.Count -gt 0) {
    Write-Fail "notice-matching" ("content mismatch: " + ($noticeMismatches -join ", "))
}

if ($actualPaths.ContainsKey("THIRD-PARTY-NOTICES.md")) {
    $thirdPartyNotices = Get-Content -Raw -LiteralPath $actualPaths["THIRD-PARTY-NOTICES.md"]
    $requiredNoticeEvidence = @(
        'Status: **Reviewed for the current preview baseline; exact-distributable matching pending**.',
        'VictusXHub is a modified project based on G-Helper.',
        '| FftSharp | 2.2.0 | Direct |',
        '| HidSharpCore | 1.3.0 | Direct |',
        '| NAudio.Wasapi | 2.3.0 | Direct |',
        '| NvAPIWrapper.Net | 0.8.1.101 | Direct |',
        '| PawnIO.Modules `AMDFamily17.bin`, `LpcACPIEC.bin` | 0.2.2 | Embedded diagnostic-only modules |',
        '| System.Management | 10.0.10 | Direct |',
        '| TaskScheduler | 2.12.2 | Direct |',
        '| WinForms.DataVisualization | 1.10.2 | Direct |',
        '| NAudio.Core | 2.3.0 | Transitive |',
        'Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 10.0.11',
        'No separate third-party artwork attribution requirement is identified by this factual record.'
    )
    $forbiddenNoticeEvidence = @(
        '| Microsoft.Management.Infrastructure |',
        'VictusXHub.Silent.ico',
        'VictusXHub.Balanced.ico',
        'VictusXHub.Turbo.ico',
        'Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 10.0.10'
    )
    $missingNoticeEvidence = @($requiredNoticeEvidence | Where-Object { -not $thirdPartyNotices.Contains($_) })
    $staleNoticeEvidence = @($forbiddenNoticeEvidence | Where-Object { $thirdPartyNotices.Contains($_) })
    if ($missingNoticeEvidence.Count -eq 0 -and $staleNoticeEvidence.Count -eq 0) {
        Write-Pass "notice-inventory" "project, G-Helper, eight package-library, PawnIO diagnostic modules, .NET 10.0.11, NvAPIWrapper, and icon records match the approved baseline."
    } else {
        $reasons = @()
        if ($missingNoticeEvidence.Count -gt 0) { $reasons += "missing reviewed evidence: $($missingNoticeEvidence -join '; ')" }
        if ($staleNoticeEvidence.Count -gt 0) { $reasons += "stale evidence: $($staleNoticeEvidence -join '; ')" }
        Write-Fail "notice-inventory" ($reasons -join "; ")
    }
}

$runtimeEvidenceRelativePath = "Assets/Licenses/Microsoft.NET-10.0.11-win-x64-RUNTIME-EVIDENCE.md"
$requiredRuntimeEvidence = @(
    'SDK `10.0.400`',
    '| Microsoft.NETCore.App.Runtime.win-x64 | 10.0.11 | `microsoft.netcore.app.runtime.win-x64.nuspec`; `dotnet/dotnet` commit `e2f47b0110ed922f21a1522da67279133ce28f32` |',
    '| Microsoft.WindowsDesktop.App.Runtime.win-x64 | 10.0.11 | `microsoft.windowsdesktop.app.runtime.win-x64.nuspec`; `dotnet/dotnet` commit `e2f47b0110ed922f21a1522da67279133ce28f32` |'
)
if ($actualPaths.ContainsKey($runtimeEvidenceRelativePath)) {
    $runtimeEvidence = Get-Content -Raw -LiteralPath $actualPaths[$runtimeEvidenceRelativePath]
    $missingRuntimeEvidence = @($requiredRuntimeEvidence | Where-Object { -not $runtimeEvidence.Contains($_) })
    if ($missingRuntimeEvidence.Count -eq 0) {
        Write-Pass "runtime-notices" "10.0.11 win-x64 .NET Core and Windows Desktop runtime-pack evidence matches the approved external notice files."
    } else {
        Write-Fail "runtime-notices" "expected 10.0.11 win-x64 runtime evidence is incomplete or mismatched."
    }
} else {
    Write-Fail "runtime-notices" "missing $runtimeEvidenceRelativePath."
}

$unexpectedExecutables = @($entries.RelativePath | Where-Object {
    ([IO.Path]::GetExtension($_) -in @(".exe", ".dll")) -and $_ -notin @("VictusXHub.exe", "NvAPIWrapper.dll")
})
if ($unexpectedExecutables.Count -eq 0) {
    Write-Pass "runtime-layout" "single VictusXHub executable plus the expected NvAPIWrapper sidecar; no external runtime binaries found."
} else {
    Write-Fail "runtime-layout" ("unexpected external binaries: " + ($unexpectedExecutables -join ", "))
}

if ($actualPaths.ContainsKey("VictusXHub.exe")) {
    $executableStream = [IO.File]::OpenRead($actualPaths["VictusXHub.exe"])
    try {
        $hasMzHeader = $executableStream.ReadByte() -eq 0x4D -and $executableStream.ReadByte() -eq 0x5A
    } finally {
        $executableStream.Dispose()
    }
    if ($hasMzHeader) { Write-Pass "executable-format" "VictusXHub.exe has a Windows executable header." }
    else { Write-Fail "executable-format" "VictusXHub.exe does not have a Windows executable header." }

    try {
        $versionInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($actualPaths["VictusXHub.exe"])
        if ([string]::IsNullOrWhiteSpace($versionInfo.ProductName)) {
            Write-Warn "executable-identity" "file metadata could not be read; verify product/version/description manually."
        } elseif ($versionInfo.ProductName -ceq "VictusXHub" -and $versionInfo.FileDescription -match "VictusXHub") {
            Write-Pass "executable-identity" "ProductName=$($versionInfo.ProductName); FileDescription=$($versionInfo.FileDescription); ProductVersion=$($versionInfo.ProductVersion)."
        } else {
            Write-Fail "executable-identity" "unexpected metadata ProductName='$($versionInfo.ProductName)', FileDescription='$($versionInfo.FileDescription)'."
        }
    } catch {
        Write-Warn "executable-identity" "file metadata could not be read; verify product/version/description manually."
    }

    try {
        $icon = [Drawing.Icon]::ExtractAssociatedIcon($actualPaths["VictusXHub.exe"])
        if ($null -eq $icon) { Write-Warn "executable-icon" "no extractable icon resource was found." }
        else { $icon.Dispose(); Write-Pass "executable-icon" "the approved VictusXHub icon resource is extractable; final visual review remains part of clean-machine validation." }
    } catch {
        Write-Warn "executable-icon" "icon resource could not be inspected cheaply; verify it manually."
    }
}

$manifestLines = @()
foreach ($entry in $entries) {
    $hash = Get-Sha256 $entry.File.FullName
    $manifestLines += "$hash  $($entry.File.Length)  $($entry.RelativePath)"
    Write-Output "SHA256 $hash $($entry.File.Length) $($entry.RelativePath)"
}
$manifestText = ($manifestLines -join "`n") + "`n"
$sha256 = [Security.Cryptography.SHA256]::Create()
try {
    $manifestHash = [BitConverter]::ToString(
        $sha256.ComputeHash([Text.Encoding]::UTF8.GetBytes($manifestText))).Replace("-", "")
} finally {
    $sha256.Dispose()
}
Write-Pass "checksum-evidence" "$($entries.Count) files; deterministic manifest SHA256 $manifestHash."

Write-Warn "manual-release-evidence" "artifact-level ZIP checksum and clean-machine validation must be supplied separately."

if ($script:FailureCount -eq 0) {
    Write-Output "Preview package: GO"
    exit 0
}

Write-Output "Preview package: NO-GO"
exit 1
