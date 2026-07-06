# Generates assets\PrivacyDots.ico (two dots: green camera, orange mic) using GDI+.
# ICO container holds PNG-compressed images (supported since Windows Vista).
param(
    [string]$OutFile = (Join-Path (Split-Path -Parent $PSScriptRoot) "assets\PrivacyDots.ico")
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$outDir = Split-Path -Parent $OutFile
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Force $outDir | Out-Null }

$sizes = 16, 24, 32, 48, 64, 128, 256
$images = @()

foreach ($s in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap($s, $s)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

    $d = [float]($s * 0.46)
    $gap = [float]($s * 0.06)
    $total = 2 * $d + $gap
    $x = [float](($s - $total) / 2)
    $y = [float](($s - $d) / 2)

    $shadow = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(70, 0, 0, 0))
    $green  = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(52, 199, 89))
    $orange = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 149, 0))

    $g.FillEllipse($shadow, $x - $s * 0.02, $y, $d + $s * 0.04, $d + $s * 0.04)
    $g.FillEllipse($shadow, $x + $d + $gap - $s * 0.02, $y, $d + $s * 0.04, $d + $s * 0.04)
    $g.FillEllipse($green, $x, $y, $d, $d)
    $g.FillEllipse($orange, $x + $d + $gap, $y, $d, $d)

    $shadow.Dispose(); $green.Dispose(); $orange.Dispose(); $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $images += , @{ Size = $s; Bytes = $ms.ToArray() }
    $ms.Dispose()
}

$fs = [System.IO.File]::Create($OutFile)
$bw = New-Object System.IO.BinaryWriter($fs)
$bw.Write([UInt16]0)                 # reserved
$bw.Write([UInt16]1)                 # type: icon
$bw.Write([UInt16]$images.Count)     # image count

$offset = 6 + 16 * $images.Count
foreach ($img in $images) {
    $dim = $img.Size
    if ($dim -ge 256) { $dim = 0 }   # 0 means 256
    $bw.Write([Byte]$dim)            # width
    $bw.Write([Byte]$dim)            # height
    $bw.Write([Byte]0)               # palette colors
    $bw.Write([Byte]0)               # reserved
    $bw.Write([UInt16]1)             # color planes
    $bw.Write([UInt16]32)            # bits per pixel
    $bw.Write([UInt32]$img.Bytes.Length)
    $bw.Write([UInt32]$offset)
    $offset += $img.Bytes.Length
}
foreach ($img in $images) {
    $bw.Write($img.Bytes)
}
$bw.Close()
$fs.Close()

Write-Host "Icon written to $OutFile"
