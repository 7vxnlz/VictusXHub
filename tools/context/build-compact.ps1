[CmdletBinding()]
param([switch]$Json)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$logDirectory = Join-Path $repoRoot ".tmp\logs"
[System.IO.Directory]::CreateDirectory($logDirectory) | Out-Null
$logPath = Join-Path $logDirectory ("build-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))
$stopwatch = [Diagnostics.Stopwatch]::StartNew()
Push-Location $repoRoot
try { & dotnet build VictusXHub.sln --no-restore *> $logPath; $exitCode = $LASTEXITCODE }
finally { Pop-Location; $stopwatch.Stop() }

$logLines = @(Get-Content -LiteralPath $logPath)
$summary = $logLines | Where-Object { $_ -match '^\s*\d+\s+Warning\(s\)' } | Select-Object -Last 1
$warnings = if ($summary -match '^\s*(\d+)\s+Warning\(s\)\s*(\d+)\s+Error\(s\)') { [int]$matches[1] } else { @($logLines | Where-Object { $_ -match ': warning ' }).Count }
$errors = if ($summary -match '^\s*(\d+)\s+Warning\(s\)\s*(\d+)\s+Error\(s\)') { [int]$matches[2] } else { @($logLines | Where-Object { $_ -match ': error ' }).Count }
$failureExcerpt = @($logLines | Where-Object { $_ -match ': error |\berror \w+\d+' } | Select-Object -First 120)
$failingProject = foreach ($line in $failureExcerpt) { if ($line -match '\[(?<Project>[^\]]+\.csproj)\]') { $matches.Project; break } }
$result = [ordered]@{ Build = if ($exitCode -eq 0) { "PASS" } else { "FAIL" }; ExitCode = $exitCode; Warnings = $warnings; Errors = $errors; FailingProject = $failingProject; DurationSeconds = [Math]::Round($stopwatch.Elapsed.TotalSeconds, 3); FullLog = $logPath; FailureExcerpt = $failureExcerpt }
if ($Json) { $result | ConvertTo-Json -Depth 4 } else {
    Write-Output ("Build: {0}" -f $result.Build)
    Write-Output ("Warnings: {0}; Errors: {1}; Duration: {2:N1}s" -f $result.Warnings, $result.Errors, $result.DurationSeconds)
    Write-Output ("Full log: {0}" -f $result.FullLog)
    if ($exitCode -ne 0 -and $failingProject) { Write-Output ("Failing project: {0}" -f $failingProject) }
    if ($exitCode -ne 0 -and $failureExcerpt.Count -gt 0) { Write-Output "Failure excerpt:"; $failureExcerpt }
}
exit $exitCode
