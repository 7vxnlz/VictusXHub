[CmdletBinding()]
param(
    [string]$Target = "VictusXHub.sln",
    [string]$Filter,
    [switch]$NoBuild,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$logDirectory = Join-Path $repoRoot ".tmp\logs"
[System.IO.Directory]::CreateDirectory($logDirectory) | Out-Null
$logPath = Join-Path $logDirectory ("test-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))
$arguments = [System.Collections.Generic.List[string]]::new()
$arguments.Add("test"); $arguments.Add($Target)
if ($NoBuild) { $arguments.Add("--no-build") }
if (-not [string]::IsNullOrWhiteSpace($Filter)) { $arguments.Add("--filter"); $arguments.Add($Filter) }
$stopwatch = [Diagnostics.Stopwatch]::StartNew()
Push-Location $repoRoot
try { & dotnet @arguments *> $logPath; $exitCode = $LASTEXITCODE }
finally { Pop-Location; $stopwatch.Stop() }

$logLines = @(Get-Content -LiteralPath $logPath)
$passed = 0; $failed = 0; $skipped = 0
foreach ($line in $logLines) {
    if ($line -match '(?:Passed|Başarılı):\s*(\d+)') { $passed = [int]$matches[1] }
    if ($line -match '(?:Failed|Başarısız):\s*(\d+)') { $failed = [int]$matches[1] }
    if ($line -match '(?:Skipped|Atlanan):\s*(\d+)') { $skipped = [int]$matches[1] }
}
$failureExcerpt = @($logLines | Where-Object { $_ -match '^\s*Failed |\b(?:error|assert)\b' } | Select-Object -First 120)
$failedTests = @($logLines | Where-Object { $_ -match '^\s*Failed\s+' } | Select-Object -First 25)
$result = [ordered]@{ Tests = if ($exitCode -eq 0) { "PASS" } else { "FAIL" }; ExitCode = $exitCode; Passed = $passed; Failed = $failed; Skipped = $skipped; FailedTests = $failedTests; DurationSeconds = [Math]::Round($stopwatch.Elapsed.TotalSeconds, 3); FullLog = $logPath; FailureExcerpt = $failureExcerpt }
if ($Json) { $result | ConvertTo-Json -Depth 4 } else {
    Write-Output ("Tests: {0}" -f $result.Tests)
    Write-Output ("Passed: {0}; Failed: {1}; Skipped: {2}; Duration: {3:N1}s" -f $result.Passed, $result.Failed, $result.Skipped, $result.DurationSeconds)
    Write-Output ("Full log: {0}" -f $result.FullLog)
    if ($exitCode -ne 0 -and $failedTests.Count -gt 0) { Write-Output "Failed tests:"; $failedTests }
    if ($exitCode -ne 0 -and $failureExcerpt.Count -gt 0) { Write-Output "Failure excerpt:"; $failureExcerpt }
}
exit $exitCode
