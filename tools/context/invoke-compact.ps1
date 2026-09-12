[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$FilePath,
    [string[]]$ArgumentList = @(),
    [ValidateRange(1, 100)]
    [int]$MaxLines = 40,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($FilePath)) { throw "-FilePath is required." }

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$logDirectory = Join-Path $repoRoot ".tmp\logs"
[System.IO.Directory]::CreateDirectory($logDirectory) | Out-Null
$logPath = Join-Path $logDirectory ("command-{0:yyyyMMdd-HHmmssfff}.log" -f (Get-Date))
$stopwatch = [Diagnostics.Stopwatch]::StartNew()
Push-Location $repoRoot
try { & $FilePath @ArgumentList *> $logPath; $exitCode = $LASTEXITCODE }
finally { Pop-Location; $stopwatch.Stop() }

$lineCount = [long]0
$errorLikeLineCount = [long]0
foreach ($line in [System.IO.File]::ReadLines($logPath)) {
    $lineCount++
    if ($line -match '(?i)\b(error|failed|exception)\b') { $errorLikeLineCount++ }
}
$result = [ordered]@{
    Command = if ($exitCode -eq 0) { "PASS" } else { "FAIL" }
    ExitCode = $exitCode
    DurationSeconds = [Math]::Round($stopwatch.Elapsed.TotalSeconds, 3)
    OutputLines = $lineCount
    ErrorLikeLines = $errorLikeLineCount
    TailLinesCaptured = [Math]::Min($lineCount, $MaxLines)
    FullLog = $logPath
}
if ($Json) { $result | ConvertTo-Json -Depth 3 } else {
    Write-Output ("Command: {0}; Exit code: {1}; Duration: {2:N1}s" -f $result.Command, $result.ExitCode, $result.DurationSeconds)
    Write-Output ("Output lines: {0}; error-like lines: {1}; bounded summary cap: {2}" -f $result.OutputLines, $result.ErrorLikeLines, $result.TailLinesCaptured)
    Write-Output ("Full log: {0}" -f $result.FullLog)
}
exit $exitCode
