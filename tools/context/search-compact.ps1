[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Pattern,
    [string[]]$Path = @("."),
    [ValidateRange(1, 1000)]
    [int]$MaxFiles = 12,
    [ValidateRange(1, 10000)]
    [int]$MaxMatches = 24,
    [ValidateRange(0, 20)]
    [int]$Context = 0,
    [ValidateRange(256, 200000)]
    [int]$MaxPreviewCharacters = 6000,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
if ($null -eq (Get-Command rg -ErrorAction SilentlyContinue)) { throw "rg is required but was not found on PATH." }

$countLines = @(& rg --count --no-heading --color never -- $Pattern @Path 2>$null)
if ($LASTEXITCODE -gt 1) { throw "rg failed with exit code $LASTEXITCODE." }

$files = foreach ($line in $countLines) {
    if ($line -match '^(?<Path>.*):(?<Count>\d+)$') {
        [pscustomobject]@{ Path = $matches.Path; Count = [int]$matches.Count }
    }
}
$totalMatches = [int](($files | Measure-Object -Property Count -Sum).Sum)
$topFiles = @($files | Sort-Object Path | Sort-Object Count -Descending | Select-Object -First $MaxFiles)
$previewArguments = @("--line-number", "--no-heading", "--color", "never")
if ($Context -gt 0) { $previewArguments += @("--context", $Context) }
$previewCandidates = @(& rg @previewArguments -- $Pattern @Path 2>$null | Select-Object -First $MaxMatches)
if ($LASTEXITCODE -gt 1) { throw "rg failed with exit code $LASTEXITCODE." }

$preview = [System.Collections.Generic.List[string]]::new()
$previewCharacterCount = 0
$previewCharacterTruncated = $false
foreach ($line in $previewCandidates) {
    if ($previewCharacterCount + $line.Length -gt $MaxPreviewCharacters) {
        $previewCharacterTruncated = $true
        break
    }

    $preview.Add($line)
    $previewCharacterCount += $line.Length
}
$previewMatchTruncated = $totalMatches -gt $MaxMatches

$result = [ordered]@{
    Matches = $totalMatches
    Files = $files.Count
    TopFiles = $topFiles
    Preview = @($preview)
    Truncated = ($files.Count -gt $MaxFiles -or $previewMatchTruncated -or $previewCharacterTruncated)
    PreviewLimits = [ordered]@{ MaxFiles = $MaxFiles; MaxMatches = $MaxMatches; MaxPreviewCharacters = $MaxPreviewCharacters; Context = $Context }
    PreviewCharacters = $previewCharacterCount
}
if ($Json) { $result | ConvertTo-Json -Depth 5; return }

Write-Output ("Matches: {0} across {1} files" -f $result.Matches, $result.Files)
Write-Output "Top files:"
foreach ($file in $result.TopFiles) { Write-Output ("{0,-65} {1,6}" -f $file.Path, $file.Count) }
if ($result.Truncated) { Write-Output ("Preview truncated. Limits: {0} files, {1} matches, {2} characters. Use narrower paths/patterns or raise -MaxFiles, -MaxMatches, or -MaxPreviewCharacters explicitly." -f $MaxFiles, $MaxMatches, $MaxPreviewCharacters) }
Write-Output "Preview:"
$result.Preview
