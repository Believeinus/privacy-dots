<#
Releases a new version of Privacy Dots in one command.

Usage:
    .\release.ps1 -Version 1.2.0

Before running, add a "## [1.2.0]" section to CHANGELOG.md - it becomes the
GitHub release notes. Requires Inno Setup 6 and an authenticated gh CLI.

What it does, in order:
  1. Stamps the version into AssemblyInfo.cs, the About dialog, the installer
     script, README.md, and docs/DOCUMENTATION.md
  2. Rebuilds the exe and installer (previous installers are removed from dist)
  3. Commits everything as "Release vX.Y.Z" and pushes
  4. Creates GitHub release vX.Y.Z with the installer attached, using the
     CHANGELOG section as the notes

Note: the release commit includes ALL pending changes in the working tree.
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Edit-Text([string]$Path, [scriptblock]$Transform) {
    $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $new = & $Transform $text
    if ($new -ne $text) { [System.IO.File]::WriteAllText($Path, $new, $utf8NoBom) }
}

# --- sanity checks -----------------------------------------------------------
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw "gh CLI not found on PATH" }

$changelogPath = Join-Path $root "CHANGELOG.md"
$changelog = [System.IO.File]::ReadAllText($changelogPath, [System.Text.Encoding]::UTF8)
$versionPattern = [regex]::Escape($Version)
if ($changelog -notmatch "(?m)^## \[$versionPattern\]") {
    throw "CHANGELOG.md has no '## [$Version]' section - write the changelog entry first"
}

# --- extract release notes from the changelog section ------------------------
$notes = New-Object System.Collections.Generic.List[string]
$inSection = $false
foreach ($line in ($changelog -split "`r?`n")) {
    if ($line -match "^## \[$versionPattern\]") { $inSection = $true; continue }
    if ($inSection -and ($line -match '^## \[' -or $line -match '^\[[\d.]+\]:')) { break }
    if ($inSection) { $notes.Add($line) }
}
$notesText = (($notes -join "`n").Trim()) +
    "`n`n## Install`n`nDownload **PrivacyDots-Setup-$Version.exe** below and run it - it upgrades an existing install in place. Windows 10/11, no admin rights needed."
$notesFile = Join-Path $env:TEMP "privacy-dots-release-notes.md"
[System.IO.File]::WriteAllText($notesFile, $notesText, $utf8NoBom)

# --- stamp the version everywhere --------------------------------------------
Edit-Text (Join-Path $root "src\AssemblyInfo.cs") { param($t)
    ($t -replace 'AssemblyVersion\("[\d.]+"\)', "AssemblyVersion(""$Version.0"")") `
        -replace 'AssemblyFileVersion\("[\d.]+"\)', "AssemblyFileVersion(""$Version.0"")" }

Edit-Text (Join-Path $root "src\TrayContext.cs") { param($t)
    $t -replace 'Privacy Dots [\d.]+\\n', "Privacy Dots $Version\n" }

Edit-Text (Join-Path $root "installer\PrivacyDots.iss") { param($t)
    $t -replace '#define MyAppVersion "[\d.]+"', "#define MyAppVersion ""$Version""" }

Edit-Text (Join-Path $root "README.md") { param($t)
    ($t -replace 'version-[\d.]+-orange', "version-$Version-orange") `
        -replace 'PrivacyDots-Setup-[\d.]+\.exe', "PrivacyDots-Setup-$Version.exe" }

Edit-Text (Join-Path $root "docs\DOCUMENTATION.md") { param($t)
    $t -replace 'PrivacyDots-Setup-[\d.]+\.exe', "PrivacyDots-Setup-$Version.exe" }

Write-Host "Version $Version stamped into sources, installer script, and docs"

# --- rebuild ------------------------------------------------------------------
Get-ChildItem (Join-Path $root "dist") -Filter "PrivacyDots-Setup-*.exe" -ErrorAction SilentlyContinue |
    Remove-Item -Force
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root "build.ps1")
if ($LASTEXITCODE -ne 0) { throw "build failed" }

$installer = Join-Path $root "dist\PrivacyDots-Setup-$Version.exe"
if (-not (Test-Path $installer)) { throw "expected installer not found: $installer" }

# --- commit, push, release ----------------------------------------------------
git -C $root add -A
git -C $root commit -m "Release v$Version"
if ($LASTEXITCODE -ne 0) { throw "git commit failed (nothing to commit?)" }
git -C $root push
if ($LASTEXITCODE -ne 0) { throw "git push failed" }

gh release create "v$Version" $installer --title "Privacy Dots v$Version" --notes-file $notesFile
if ($LASTEXITCODE -ne 0) { throw "gh release create failed" }

Write-Host ""
Write-Host "Released v$Version : https://github.com/Believeinus/privacy-dots/releases/tag/v$Version"
