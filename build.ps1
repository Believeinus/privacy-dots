# Builds PrivacyDots.exe (and the installer if Inno Setup is available).
# Requires only what ships with Windows 10/11: the .NET Framework 4.x C# compiler.
param(
    [switch]$SkipInstaller
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

# 1. Generate the .ico
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root "tools\make-icon.ps1")

# 2. Compile with the framework C# compiler (present on every Windows 10/11 machine)
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) { throw ".NET Framework C# compiler not found" }

$dist = Join-Path $root "dist"
New-Item -ItemType Directory -Force $dist | Out-Null

$sources = Get-ChildItem (Join-Path $root "src") -Filter *.cs | ForEach-Object { $_.FullName }

$outExe = Join-Path $dist "PrivacyDots.exe"
$icoArg = "/win32icon:" + (Join-Path $root "assets\PrivacyDots.ico")
$manArg = "/win32manifest:" + (Join-Path $root "src\app.manifest")
& $csc /nologo /target:winexe /platform:anycpu /optimize+ `
    "/out:$outExe" $icoArg $manArg `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    $sources
if ($LASTEXITCODE -ne 0) { throw "csc failed with exit code $LASTEXITCODE" }
Write-Host "Built $dist\PrivacyDots.exe"

# 3. Build the installer if Inno Setup is installed
if (-not $SkipInstaller) {
    $iscc = $null
    foreach ($p in @("$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe",
                     "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
                     "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe")) {
        if (Test-Path $p) { $iscc = $p; break }
    }
    if ($null -eq $iscc) {
        $cmd = Get-Command iscc -ErrorAction SilentlyContinue
        if ($cmd) { $iscc = $cmd.Source }
    }
    if ($iscc) {
        & $iscc (Join-Path $root "installer\PrivacyDots.iss")
        if ($LASTEXITCODE -ne 0) { throw "ISCC failed with exit code $LASTEXITCODE" }
        Write-Host "Installer written to $root\dist"
    } else {
        Write-Warning "Inno Setup not found - skipped installer. Install it (winget install JRSoftware.InnoSetup) and re-run."
    }
}
