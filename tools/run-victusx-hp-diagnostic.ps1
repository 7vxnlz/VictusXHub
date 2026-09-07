[CmdletBinding()]
param(
    [string]$VictusXExecutable = (Join-Path $PSScriptRoot "..\VictusX.exe")
)

$resolvedExecutable = Resolve-Path -LiteralPath $VictusXExecutable -ErrorAction SilentlyContinue
if ($null -eq $resolvedExecutable -or -not (Test-Path -LiteralPath $resolvedExecutable.Path -PathType Leaf)) {
    Write-Error "VictusX.exe was not found at '$VictusXExecutable'."
    exit 1
}

Start-Process -FilePath $resolvedExecutable.Path -WorkingDirectory (Split-Path -Parent $resolvedExecutable.Path) -ArgumentList "--hp-victus"
