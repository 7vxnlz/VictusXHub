[CmdletBinding()]
param(
    [string]$SessionPath,
    [switch]$Latest,
    [ValidateRange(1, 100)]
    [int]$Last,
    [switch]$Json
)

$ErrorActionPreference = "Stop"

function Get-CodexSessionRoot {
    $candidateRoots = @()
    if (-not [string]::IsNullOrWhiteSpace($env:CODEX_HOME)) { $candidateRoots += $env:CODEX_HOME }
    if (-not [string]::IsNullOrWhiteSpace($env:USERPROFILE)) { $candidateRoots += (Join-Path $env:USERPROFILE ".codex") }

    foreach ($candidateRoot in $candidateRoots | Select-Object -Unique) {
        $sessions = Join-Path $candidateRoot "sessions"
        if (Test-Path -LiteralPath $sessions -PathType Container) { return (Resolve-Path -LiteralPath $sessions).Path }
    }

    throw "No Codex session directory was found under the configured local profile."
}

function Get-SessionFiles {
    param([string]$Root)

    $files = @(Get-ChildItem -LiteralPath $Root -Filter "*.jsonl" -File -Recurse -ErrorAction Stop |
        Sort-Object -Property LastWriteTimeUtc -Descending)
    if ($files.Count -eq 0) { throw "No Codex JSONL session files were found." }
    return $files
}

function Get-ShellCategory {
    param([string]$RequestData)

    if ([string]::IsNullOrWhiteSpace($RequestData)) { return "other shell" }
    $command = $RequestData
    try {
        $arguments = $RequestData | ConvertFrom-Json -ErrorAction Stop
        if ($null -ne $arguments.cmd) { $command = [string]$arguments.cmd }
    }
    catch { }

    if ([string]::IsNullOrWhiteSpace($command)) { return "other shell" }
    if ($command -match '(?i)(?:^|[^a-z])rg(?:\s|$)') { return "rg" }
    if ($command -match '(?i)Get-Content(?:\s|$)') { return "Get-Content" }
    if ($command -match '(?i)dotnet\s+test(?:\s|$)') { return "dotnet test" }
    if ($command -match '(?i)dotnet\s+build(?:\s|$)') { return "dotnet build" }
    if ($command -match '(?i)git\s+diff(?:\s|$)') { return "git diff" }
    if ($command -match '(?i)git\s+status(?:\s|$)') { return "git status" }
    if ($command -match '(?i)git\s+log(?:\s|$)') { return "git log" }
    if ($command -match '(?i)Get-ChildItem(?:\s|$)') { return "Get-ChildItem" }
    if ($command -match '(?i)Select-String(?:\s|$)') { return "Select-String" }
    if ($command -match '(?i)git\s+show(?:\s|$)') { return "git show" }
    if ($command -match '(?i)git\s+grep(?:\s|$)') { return "git grep" }
    if ($command -match '(?i)dotnet\s+restore(?:\s|$)') { return "dotnet restore" }
    if ($command -match '(?i)dotnet\s+publish(?:\s|$)') { return "dotnet publish" }
    if ($command -match '(?i)Get-(?:Process|Service|CimInstance|WinEvent|ItemProperty|Acl)(?:\s|$)') { return "Windows inspection" }
    if ($command -match '(?i)git\s+') { return "other git" }
    if ($command -match '(?i)(?:Get|Test|Resolve|Join|Split)-(?:Item|Path|Location|Command|FileHash)(?:\s|$)') { return "PowerShell filesystem" }
    if ($command -match '(?i)\b(?:powershell|pwsh)(?:\.exe)?\b') { return "PowerShell child process" }
    if ($command -match '(?i)\b(?:where(?:\.exe)?|findstr|robocopy)\b') { return "Windows command" }
    return "other shell"
}

function Get-ToolCategory {
    param($Payload)

    $name = [string]$Payload.name
    if ($name -eq "exec") {
        $requestData = if ($null -ne $Payload.arguments) { [string]$Payload.arguments } else { [string]$Payload.input }
        return (Get-ShellCategory -RequestData $requestData)
    }
    if ($name -eq "js") { return "tool: js" }
    if ($name -eq "wait") { return "tool: wait" }
    if ($name -match 'patch|edit|write') { return "tool: file edit" }
    return "other tool"
}

function Get-OutputCharacterCount {
    param($Output)

    if ($null -eq $Output) { return 0 }
    if ($Output -is [string]) { return $Output.Length }
    try { return ($Output | ConvertTo-Json -Compress -Depth 16).Length }
    catch { return 0 }
}

function Add-Count {
    param([hashtable]$Table, [string]$Name, [long]$Increment = 1)
    if (-not $Table.ContainsKey($Name)) { $Table[$Name] = [long]0 }
    $Table[$Name] += $Increment
}

function Add-TokenUsage {
    param([hashtable]$Totals, $Usage)

    if ($null -eq $Usage) { return }
    foreach ($field in @("input_tokens", "cached_input_tokens", "cache_write_input_tokens", "output_tokens", "reasoning_output_tokens", "total_tokens")) {
        $value = $Usage.$field
        if ($null -ne $value) { try { $Totals[$field] += [long]$value } catch { } }
    }
}

function Get-SafeTokenUsageObject {
    param([hashtable]$Totals)
    return [ordered]@{
        Input = $Totals.input_tokens
        CachedInput = $Totals.cached_input_tokens
        CacheWriteInput = $Totals.cache_write_input_tokens
        Output = $Totals.output_tokens
        Reasoning = $Totals.reasoning_output_tokens
        Total = $Totals.total_tokens
    }
}

if ($SessionPath -and ($Latest -or $PSBoundParameters.ContainsKey("Last"))) {
    throw "-SessionPath cannot be combined with -Latest or -Last."
}
if ($Latest -and $PSBoundParameters.ContainsKey("Last")) {
    throw "-Latest cannot be combined with -Last."
}

$sessionRoot = Get-CodexSessionRoot
if ($SessionPath) {
    $resolvedSession = Resolve-Path -LiteralPath $SessionPath -ErrorAction Stop
    if ((Get-Item -LiteralPath $resolvedSession).PSIsContainer -or $resolvedSession.Path -notmatch '\.jsonl$') {
        throw "-SessionPath must identify one JSONL session file."
    }
    $selectedFiles = @(Get-Item -LiteralPath $resolvedSession)
}
else {
    $allFiles = Get-SessionFiles -Root $sessionRoot
    $requestedCount = if ($PSBoundParameters.ContainsKey("Last")) { $Last } else { 1 }
    $selectedFiles = @($allFiles | Select-Object -First $requestedCount)
}

$tokenTotals = @{
    input_tokens = [long]0; cached_input_tokens = [long]0; cache_write_input_tokens = [long]0
    output_tokens = [long]0; reasoning_output_tokens = [long]0; total_tokens = [long]0
}
$categoryCounts = @{}
$outputCategoryTotals = @{}
$callCategories = @{}
$largestOutputs = [System.Collections.Generic.List[object]]::new()
$sessions = [System.Collections.Generic.List[object]]::new()
$eventCount = [long]0
$malformedLineCount = [long]0
$skippedFileCount = [long]0
$tokenUsageRecordCount = [long]0
$compactionCount = [long]0
$maxContextWindow = [long]0
$modelSwitchCount = [long]0
$previousModel = $null
$broadSearchOutputs = [long]0
$largeContentReads = [long]0
$outputsOver8k = [long]0

foreach ($file in $selectedFiles) {
    $fileEvents = [long]0
    $firstTimestamp = $null
    $lastTimestamp = $null
    $reader = $null
    try {
        $stream = [System.IO.File]::Open($file.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        $reader = [System.IO.StreamReader]::new($stream)
        while (($line = $reader.ReadLine()) -ne $null) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            try { $event = $line | ConvertFrom-Json -ErrorAction Stop }
            catch { $malformedLineCount++; continue }

            $eventCount++; $fileEvents++
            $timestamp = [DateTimeOffset]::MinValue
            if ([DateTimeOffset]::TryParse([string]$event.timestamp, [ref]$timestamp)) {
                if ($null -eq $firstTimestamp -or $timestamp -lt $firstTimestamp) { $firstTimestamp = $timestamp }
                if ($null -eq $lastTimestamp -or $timestamp -gt $lastTimestamp) { $lastTimestamp = $timestamp }
            }

            $eventType = [string]$event.type
            $payload = $event.payload
            if ($eventType -eq "compacted") { $compactionCount++ }
            if ($eventType -eq "token_usage_record") {
                $tokenUsageRecordCount++
                Add-TokenUsage -Totals $tokenTotals -Usage $payload.usage
            }
            if ($eventType -eq "event_msg") {
                try {
                    $contextWindow = [long]$payload.model_context_window
                    if ($contextWindow -gt $maxContextWindow) { $maxContextWindow = $contextWindow }
                }
                catch { }
            }
            if ($eventType -eq "turn_context") {
                $model = [string]$payload.model
                if (-not [string]::IsNullOrWhiteSpace($model)) {
                    if ($null -ne $previousModel -and $model -ne $previousModel) { $modelSwitchCount++ }
                    $previousModel = $model
                }
            }
            if ($eventType -ne "response_item") { continue }

            $itemType = [string]$payload.type
            if ($itemType -eq "function_call" -or $itemType -eq "custom_tool_call") {
                $category = Get-ToolCategory -Payload $payload
                Add-Count -Table $categoryCounts -Name $category
                $callId = [string]$payload.call_id
                if (-not [string]::IsNullOrWhiteSpace($callId)) { $callCategories[$callId] = $category }
                continue
            }
            if ($itemType -ne "function_call_output" -and $itemType -ne "custom_tool_call_output") { continue }

            $category = "other tool"
            $outputCallId = [string]$payload.call_id
            if (-not [string]::IsNullOrWhiteSpace($outputCallId) -and $callCategories.ContainsKey($outputCallId)) {
                $category = $callCategories[$outputCallId]
            }
            $approxTokens = [long][Math]::Ceiling((Get-OutputCharacterCount -Output $payload.output) / 4.0)
            if ($approxTokens -le 0) { continue }
            Add-Count -Table $outputCategoryTotals -Name $category -Increment $approxTokens
            $largestOutputs.Add([pscustomobject]@{ Category = $category; ApproxTokens = $approxTokens })
            if ($approxTokens -gt 8000) { $outputsOver8k++ }
            if ($category -eq "rg" -and $approxTokens -gt 2000) { $broadSearchOutputs++ }
            if ($category -eq "Get-Content" -and $approxTokens -gt 2000) { $largeContentReads++ }
        }
    }
    catch { $skippedFileCount++; continue }
    finally { if ($null -ne $reader) { $reader.Dispose() } }

    $durationSeconds = $null
    if ($null -ne $firstTimestamp -and $null -ne $lastTimestamp) {
        $durationSeconds = [Math]::Round(($lastTimestamp - $firstTimestamp).TotalSeconds, 3)
    }
    $sessions.Add([ordered]@{ File = $file.Name; Events = $fileEvents; DurationSeconds = $durationSeconds })
}

if ($sessions.Count -eq 0) { throw "No usable Codex session files could be read." }

$largestSafeOutputs = @($largestOutputs | Sort-Object -Property ApproxTokens -Descending | Select-Object -First 10 |
    ForEach-Object { [ordered]@{ Category = $_.Category; ApproxTokens = $_.ApproxTokens } })
$safeCategories = @($categoryCounts.GetEnumerator() | Sort-Object -Property Key | Sort-Object -Property Value -Descending |
    ForEach-Object { [ordered]@{ Category = $_.Key; Count = $_.Value } })
$safeOutputCategories = @($outputCategoryTotals.GetEnumerator() | Sort-Object -Property Key | Sort-Object -Property Value -Descending |
    ForEach-Object { [ordered]@{ Category = $_.Key; ApproxTokens = $_.Value } })

$report = [ordered]@{
    SessionSource = $sessionRoot
    Sessions = @($sessions)
    SessionCount = $sessions.Count
    EventCount = $eventCount
    MalformedLinesSkipped = $malformedLineCount
    SessionFilesSkipped = $skippedFileCount
    TokenUsageRecords = $tokenUsageRecordCount
    TokenUsage = Get-SafeTokenUsageObject -Totals $tokenTotals
    Context = [ordered]@{
        MaxReportedContextWindow = if ($maxContextWindow -gt 0) { $maxContextWindow } else { $null }
        Compactions = $compactionCount
        ModelSwitches = $modelSwitchCount
    }
    ToolCalls = [ordered]@{
        Total = [long](($categoryCounts.Values | Measure-Object -Sum).Sum)
        Categories = $safeCategories
    }
    ToolOutputApproxTokens = $safeOutputCategories
    LargestToolOutputs = $largestSafeOutputs
    PotentialWasteSignals = [ordered]@{
        BroadSearchOutputsOver2000ApproxTokens = $broadSearchOutputs
        LargeGetContentOutputsOver2000ApproxTokens = $largeContentReads
        OutputsOver8000ApproxTokens = $outputsOver8k
        Compactions = $compactionCount
    }
}

if ($Json) { $report | ConvertTo-Json -Depth 8; return }

Write-Output "Codex session audit"
Write-Output ""
Write-Output ("Sessions: {0}" -f $report.SessionCount)
Write-Output ("Events: {0}" -f $report.EventCount)
if ($report.MalformedLinesSkipped -gt 0) { Write-Output ("Malformed JSONL lines skipped: {0}" -f $report.MalformedLinesSkipped) }
if ($report.SessionFilesSkipped -gt 0) { Write-Output ("Session files skipped: {0}" -f $report.SessionFilesSkipped) }
Write-Output ""
Write-Output "Token usage (reported):"
foreach ($entry in $report.TokenUsage.GetEnumerator()) { Write-Output ("{0}: {1:N0}" -f $entry.Key, $entry.Value) }
Write-Output ""
Write-Output "Context:"
$contextWindowText = if ($null -eq $report.Context.MaxReportedContextWindow) { "Not reported" } else { "{0:N0}" -f $report.Context.MaxReportedContextWindow }
Write-Output ("Max reported context window: {0}" -f $contextWindowText)
Write-Output ("Compactions: {0}" -f $report.Context.Compactions)
Write-Output ("Model switches: {0}" -f $report.Context.ModelSwitches)
Write-Output ""
Write-Output "Tool calls:"
foreach ($entry in $report.ToolCalls.Categories) { Write-Output ("{0,-28} {1,6}" -f $entry.Category, $entry.Count) }
Write-Output ""
Write-Output "Tool output estimates (ApproxTokens = characters / 4):"
foreach ($entry in $report.ToolOutputApproxTokens) { Write-Output ("{0,-28} ~{1,6:N0}" -f $entry.Category, $entry.ApproxTokens) }
Write-Output ""
Write-Output "Largest tool outputs (ApproxTokens = characters / 4):"
if ($report.LargestToolOutputs.Count -eq 0) { Write-Output "None reported" }
else {
    $position = 0
    foreach ($entry in $report.LargestToolOutputs) {
        $position++
        Write-Output ("{0}. {1,-25} ~{2:N0}" -f $position, $entry.Category, $entry.ApproxTokens)
    }
}
Write-Output ""
Write-Output "Potential waste signals:"
foreach ($entry in $report.PotentialWasteSignals.GetEnumerator()) { Write-Output ("{0}: {1}" -f $entry.Key, $entry.Value) }
