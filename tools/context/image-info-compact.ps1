[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Path,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
$file = Get-Item -LiteralPath $Path -ErrorAction Stop
if ($file.PSIsContainer) { throw "-Path must identify an image file." }

$image = $null
try {
    Add-Type -AssemblyName System.Drawing -ErrorAction Stop
    $image = [System.Drawing.Image]::FromFile($file.FullName)
    $readable = $true
    $width = $image.Width
    $height = $image.Height
    $format = $image.RawFormat.ToString()
}
catch {
    $readable = $false
    $width = $null
    $height = $null
    $format = $null
}
finally {
    if ($null -ne $image) { $image.Dispose() }
}

$result = [ordered]@{
    File = $file.FullName
    Extension = $file.Extension
    Bytes = $file.Length
    Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    ReadableImage = $readable
    Width = $width
    Height = $height
    Format = $format
}
if ($Json) { $result | ConvertTo-Json -Depth 3; return }

Write-Output ("File: {0}" -f $result.File)
Write-Output ("Bytes: {0}; SHA-256: {1}" -f $result.Bytes, $result.Sha256)
Write-Output ("Readable image: {0}; Size: {1}x{2}; Format: {3}" -f $result.ReadableImage, $result.Width, $result.Height, $result.Format)
