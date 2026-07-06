# Privacy Dots for Windows

Always-on-top privacy indicator dots for Windows 10 and 11.

- 🟢 **Green dot** — camera is in use
- 🟠 **Orange dot** — microphone is in use
- Nothing shown when both are idle

The dots are a borderless, click-through, per-pixel-transparent overlay drawn
directly on the screen — no window, no frame, nothing except the dots. The
overlay re-asserts its topmost position every 2 seconds so no other app can
cover it.

Developed by Hiteshwar Singh.

## How it works

Windows tracks every app's camera/microphone sessions in the registry under
`HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore`
(the same data behind the built-in Windows indicators). While a device is in
use, the owning app's `LastUsedTimeStop` value is `0`. Privacy Dots polls this
every 0.7 s — no drivers, no camera/mic access of its own, negligible CPU.

## Install

Run `dist\PrivacyDots-Setup-1.1.0.exe`. It is a standard wizard:

- Installs per-user by default (no admin rights needed)
- Optional **Start with Windows** and desktop-shortcut tasks
- Uninstall cleanly any time from **Settings → Apps** (removes the app, the
  autostart entry, and saved settings)

## Usage

Privacy Dots runs from the system tray (the tray icon is only the control
surface — the indicator itself floats on the desktop):

- **Double-click the tray icon** (or right-click → *Settings…*) to adjust
  **dot size** (6–40 px slider, live preview), **position** (top left / top
  center / top right / bottom left / bottom right), and **edge margin**
- Right-click → *Show test dots (5 s)* to preview both dots
- Right-click → *Start with Windows* to toggle autostart
- Right-click → *Exit* to quit

Settings are stored in `%APPDATA%\PrivacyDots\settings.ini`.

## Build from source

No SDK needed — uses the C# compiler that ships with the .NET Framework 4.x on
every Windows 10/11 machine:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This generates the icon, compiles `dist\PrivacyDots.exe`, and (if
[Inno Setup 6](https://jrsoftware.org/isinfo.php) is installed, e.g.
`winget install JRSoftware.InnoSetup`) compiles the installer into `dist\`.
Use `-SkipInstaller` to build only the exe.

## Project layout

```
src/          C# sources (WinForms, .NET Framework 4.x)
tools/        icon generator script
installer/    Inno Setup script
build.ps1     one-shot build script
dist/         build output (exe + installer)
```
