[CmdletBinding()]
param(
    [string[]]$Path = @("."),
    [ValidateRange(1, 1000)]
    [int]$Top = 25,
    [ValidateRange(0, 1000000)]
    [int]$TokenBudget = 0,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$repoPrefix = $repoRoot.TrimEnd('\') + '\'
$allowedExtensions = @('.cs', '.csproj', '.json', '.md', '.props', '.ps1', '.resx', '.sln', '.targets', '.xml')
$excludedSegments = @('\.git\', '\bin\', '\obj\', '\artifacts\', '\TestResults\', '\references\', '\reference\')
$files = @{}

function Add-ContextFile([System.IO.FileInfo]$File) {
    $fullPath = $File.FullName
    if (-not $fullPath.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the repository: $fullPath"
    }

    $normalized = $fullPath.Replace('/', '\')
    if ($excludedSegments | Where-Object { $normalized.IndexOf($_, [StringComparison]::OrdinalIgnoreCase) -ge 0 }) { return }
    if ($allowedExtensions -notcontains $File.Extension.ToLowerInvariant()) { return }
    $files[$fullPath] = $File
}

foreach ($requestedPath in $Path) {
    $candidate = if ([System.IO.Path]::IsPathRooted($requestedPath)) { $requestedPath } else { Join-Path $repoRoot $requestedPath }
    $resolved = Resolve-Path -LiteralPath $candidate
    $isRepositoryRoot = $resolved.Path.Equals($repoRoot, [StringComparison]::OrdinalIgnoreCase)
    if (-not $isRepositoryRoot -and -not $resolved.Path.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the repository: $requestedPath"
    }

    $item = Get-Item -LiteralPath $resolved.Path
    if ($item.PSIsContainer) {
        Get-ChildItem -LiteralPath $item.FullName -File -Recurse | ForEach-Object { Add-ContextFile $_ }
    } else {
        Add-ContextFile $item
    }
}

$rows = foreach ($file in $files.Values) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    [pscustomobject]@{
        Path = $file.FullName.Substring($repoPrefix.Length).Replace('\', '/')
        Lines = if ($text.Length -eq 0) { 0 } else { ($text -split "`r?`n").Count }
        Characters = $text.Length
        ApproxTokens = [Math]::Ceiling($text.Length / 4.0)
    }
}

$ordered = @($rows | Sort-Object ApproxTokens -Descending)
$totalTokens = [int](($ordered | Measure-Object ApproxTokens -Sum).Sum)
$result = [pscustomobject]@{
    Repository = $repoRoot
    FileCount = $ordered.Count
    ApproxTokens = $totalTokens
    TokenBudget = if ($TokenBudget -gt 0) { $TokenBudget } else { $null }
    LargestFiles = @($ordered | Select-Object -First $Top)
}

if ($Json) {
    $result | ConvertTo-Json -Depth 4
} else {
    $result.LargestFiles | Format-Table -AutoSize
    "Files: {0}; approximate tokens: {1:N0}" -f $result.FileCount, $result.ApproxTokens
}

if ($TokenBudget -gt 0 -and $totalTokens -gt $TokenBudget) {
    throw "Approximate token count $totalTokens exceeds budget $TokenBudget."
}
