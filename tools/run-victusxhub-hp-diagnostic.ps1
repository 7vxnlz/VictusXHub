[CmdletBinding()]
param(
    [string]$VictusXHubExecutable = (Join-Path $PSScriptRoot "..\VictusXHub.exe")
)

$resolvedExecutable = Resolve-Path -LiteralPath $VictusXHubExecutable -ErrorAction SilentlyContinue
if ($null -eq $resolvedExecutable -or -not (Test-Path -LiteralPath $resolvedExecutable.Path -PathType Leaf)) {
    Write-Error "VictusXHub.exe was not found at '$VictusXHubExecutable'."
    exit 1
}

Start-Process -FilePath $resolvedExecutable.Path -WorkingDirectory (Split-Path -Parent $resolvedExecutable.Path) -ArgumentList "--hp-victus"
