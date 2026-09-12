[CmdletBinding()]
param(
    [string[]]$Path,
    [switch]$Patch,
    [ValidateRange(1, 500)]
    [int]$MaxPatchLines = 160
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$pathArguments = if ($Path.Count -gt 0) { @("--") + $Path } else { @() }
Push-Location $repoRoot
try {
    Write-Output "Status:"
    & git status --short --branch
    Write-Output "Stat:"
    & git diff --stat @pathArguments
    Write-Output "Name status:"
    & git diff --name-status @pathArguments
    Write-Output "Whitespace check:"
    $check = @(& git diff --check @pathArguments)
    if ($check.Count -eq 0) { Write-Output "PASS" } else { $check }

    if ($Patch) {
        $logDirectory = Join-Path $repoRoot ".tmp\logs"
        [System.IO.Directory]::CreateDirectory($logDirectory) | Out-Null
        $patchPath = Join-Path $logDirectory ("diff-{0:yyyyMMdd-HHmmss}.patch" -f (Get-Date))
        & git diff @pathArguments *> $patchPath
        $patchLines = @(Get-Content -LiteralPath $patchPath)
        Write-Output ("Patch preview ({0} of {1} lines):" -f [Math]::Min($MaxPatchLines, $patchLines.Count), $patchLines.Count)
        $patchLines | Select-Object -First $MaxPatchLines
        if ($patchLines.Count -gt $MaxPatchLines) { Write-Output ("Patch truncated. Full patch: {0}" -f $patchPath) }
    }
}
finally { Pop-Location }
