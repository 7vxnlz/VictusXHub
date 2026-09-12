[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Path,
    [ValidateRange(1, 2147483647)]
    [int]$StartLine,
    [ValidateRange(1, 2147483647)]
    [int]$EndLine,
    [ValidateRange(1, 1000)]
    [int]$MaxLines = 120,
    [ValidateRange(1024, 500000)]
    [int]$MaxCharacters = 8000,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
if ($PSBoundParameters.ContainsKey("StartLine") -xor $PSBoundParameters.ContainsKey("EndLine")) {
    throw "Specify both -StartLine and -EndLine."
}
if ($PSBoundParameters.ContainsKey("StartLine") -and $EndLine -lt $StartLine) { throw "-EndLine must not be before -StartLine." }

$file = Get-Item -LiteralPath $Path -ErrorAction Stop
if ($file.PSIsContainer) { throw "-Path must identify a file." }
$allLines = [System.IO.File]::ReadAllLines($file.FullName)
$totalLines = $allLines.Count

if ($PSBoundParameters.ContainsKey("StartLine")) {
    $from = $StartLine
    $to = [Math]::Min($EndLine, $totalLines)
}
elseif ($totalLines -le 80) {
    $from = 1
    $to = $totalLines
}
else {
    $from = 1
    $to = [Math]::Min(24, $totalLines)
}

$requestedLineCount = if ($to -ge $from) { $to - $from + 1 } else { 0 }
$lineLimitTruncated = $requestedLineCount -gt $MaxLines
$display = [System.Collections.Generic.List[string]]::new()
$displayCharacterCount = 0
$characterLimitTruncated = $false
for ($lineNumber = $from; $lineNumber -le $to -and $display.Count -lt $MaxLines; $lineNumber++) {
    $formattedLine = "{0,6}: {1}" -f $lineNumber, $allLines[$lineNumber - 1]
    if ($displayCharacterCount + $formattedLine.Length -gt $MaxCharacters) {
        $characterLimitTruncated = $true
        break
    }

    $display.Add($formattedLine)
    $displayCharacterCount += $formattedLine.Length
}
$displayCount = $display.Count
$displayTo = if ($displayCount -gt 0) { $from + $displayCount - 1 } else { 0 }
$truncated = ($lineLimitTruncated -or $characterLimitTruncated -or (-not $PSBoundParameters.ContainsKey("StartLine") -and $totalLines -gt 80))
$result = [ordered]@{
    File = $file.FullName
    TotalLines = $totalLines
    DisplayedRange = if ($displayCount -gt 0) { "${from}-${displayTo}" } else { "none" }
    DisplayedLines = $displayCount
    Truncated = $truncated
    Limits = [ordered]@{ MaxLines = $MaxLines; MaxCharacters = $MaxCharacters }
    DisplayedCharacters = $displayCharacterCount
    Lines = @($display)
}
if ($Json) { $result | ConvertTo-Json -Depth 4; return }

Write-Output ("File: {0}" -f $result.File)
Write-Output ("Lines: {0}; displayed: {1}; truncated: {2}" -f $result.TotalLines, $result.DisplayedRange, $result.Truncated)
if ($truncated -and -not $PSBoundParameters.ContainsKey("StartLine")) { Write-Output "Large file: request -StartLine and -EndLine for a specific range." }
if ($truncated -and $PSBoundParameters.ContainsKey("StartLine")) { Write-Output ("Requested range was truncated by limits: {0} lines, {1} characters. Raise -MaxLines or -MaxCharacters explicitly if needed." -f $MaxLines, $MaxCharacters) }
$result.Lines
