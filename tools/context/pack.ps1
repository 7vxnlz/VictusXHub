[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9-]+$')]
    [string]$Pack,
    [string]$OutputPath,
    [ValidateRange(1, 1000000)]
    [int]$TokenBudget = 20000,
    [switch]$UseRepomix
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$repoPrefix = $repoRoot.TrimEnd('\') + '\'
$packDefinition = Join-Path $repoRoot "docs\context-packs\$Pack.md"
if (-not (Test-Path -LiteralPath $packDefinition -PathType Leaf)) {
    throw "Unknown context pack: $Pack"
}

$inFiles = $false
$relativePaths = foreach ($line in Get-Content -LiteralPath $packDefinition) {
    if ($line -eq '## Files') { $inFiles = $true; continue }
    if ($inFiles -and $line.StartsWith('## ')) { break }
    if ($inFiles -and $line -match '^- `([^`]+)`$') { $Matches[1] }
}

if (-not $relativePaths) { throw "No files were declared in $packDefinition." }

$resolvedFiles = foreach ($selector in $relativePaths) {
    if ($selector -notmatch '^(?<Path>[^#]+?)(?:#L(?<Start>\d+)-L(?<End>\d+))?$') {
        throw "Invalid pack selector: $selector"
    }
    $relativePath = $Matches.Path
    $startLine = if ($Matches.Start) { [int]$Matches.Start } else { 0 }
    $endLine = if ($Matches.End) { [int]$Matches.End } else { 0 }
    if (($startLine -eq 0) -ne ($endLine -eq 0) -or $endLine -lt $startLine) {
        throw "Invalid line range in pack selector: $selector"
    }

    $candidate = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $relativePath))
    if (-not $candidate.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Pack path escapes the repository: $relativePath"
    }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Pack file is missing: $relativePath"
    }
    $content = [System.IO.File]::ReadAllText($candidate)
    if ($startLine -gt 0) {
        $lines = @($content -split "`r?`n")
        if ($endLine -gt $lines.Count) {
            throw "Line range exceeds $relativePath ($($lines.Count) lines): $selector"
        }
        $content = [string]::Join([Environment]::NewLine, $lines[($startLine - 1)..($endLine - 1)])
    }
    [pscustomobject]@{
        Selector = $selector.Replace('\', '/')
        Relative = $relativePath.Replace('\', '/')
        Full = $candidate
        Content = $content
        HasRange = $startLine -gt 0
    }
}

$approxTokens = 0
foreach ($file in $resolvedFiles) {
    $approxTokens += [Math]::Ceiling($file.Content.Length / 4.0)
}
if ($approxTokens -gt $TokenBudget) {
    throw "Pack '$Pack' is approximately $approxTokens tokens and exceeds budget $TokenBudget."
}

if ($UseRepomix) {
    if ([string]::IsNullOrWhiteSpace($OutputPath)) { throw '-OutputPath is required with -UseRepomix.' }
    $repomix = Get-Command repomix -ErrorAction SilentlyContinue
    if ($null -eq $repomix) { throw 'Repomix is not installed or not on PATH.' }
    if ($resolvedFiles.HasRange -contains $true) {
        throw 'Repomix mode does not support line-range selectors; use the direct pack output for this pack.'
    }
    $destination = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputPath))
    if (-not $destination.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'OutputPath must remain inside the repository.'
    }
    if (Test-Path -LiteralPath $destination) { throw "OutputPath already exists: $destination" }
    $parent = Split-Path -Parent $destination
    if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent | Out-Null }
    $temporaryConfig = Join-Path $parent ('.repomix-context-' + [Guid]::NewGuid().ToString('N') + '.json')
    $config = [ordered]@{
        output = [ordered]@{
            filePath = $destination
            style = 'markdown'
            fileSummary = $false
            directoryStructure = $false
            tokenCountTree = $false
        }
        include = @($resolvedFiles.Relative | Select-Object -Unique)
        ignore = [ordered]@{ useGitignore = $true; useDotIgnore = $true; useDefaultPatterns = $true }
        security = [ordered]@{ enableSecurityCheck = $true }
        tokenCount = [ordered]@{ encoding = 'o200k_base' }
    }
    [System.IO.File]::WriteAllText($temporaryConfig, ($config | ConvertTo-Json -Depth 5), [System.Text.UTF8Encoding]::new($false))
    try {
        & $repomix.Source --config $temporaryConfig --output $destination --style markdown --quiet --token-budget $TokenBudget
        $repomixExitCode = $LASTEXITCODE
    } finally {
        if (Test-Path -LiteralPath $temporaryConfig) { Remove-Item -LiteralPath $temporaryConfig -Force }
    }
    if ($repomixExitCode -ne 0) {
        if (Test-Path -LiteralPath $destination) { Remove-Item -LiteralPath $destination -Force }
        throw "Repomix failed or exceeded budget with exit code $repomixExitCode."
    }
    $packedTokens = [Math]::Ceiling(([System.IO.File]::ReadAllText($destination)).Length / 4.0)
    if ($packedTokens -gt $TokenBudget) {
        Remove-Item -LiteralPath $destination -Force
        throw "Repomix output exceeded budget $TokenBudget and was removed."
    }
    "Created $destination (approximately $packedTokens tokens)."
    return
}

$builder = [System.Text.StringBuilder]::new()
[void]$builder.AppendLine("# Context pack: $Pack")
[void]$builder.AppendLine()
[void]$builder.AppendLine("Approximate source tokens: $approxTokens")
foreach ($file in $resolvedFiles) {
    [void]$builder.AppendLine()
    [void]$builder.AppendLine(('<file path="{0}">' -f $file.Selector))
    [void]$builder.AppendLine($file.Content)
    [void]$builder.AppendLine('</file>')
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $builder.ToString()
} else {
    $destination = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputPath))
    if (-not $destination.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'OutputPath must remain inside the repository.'
    }
    if (Test-Path -LiteralPath $destination) { throw "OutputPath already exists: $destination" }
    $parent = Split-Path -Parent $destination
    if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent | Out-Null }
    [System.IO.File]::WriteAllText($destination, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))
    "Created $destination (approximately $approxTokens source tokens)."
}
