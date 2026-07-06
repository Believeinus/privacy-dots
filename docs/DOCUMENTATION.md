# Privacy Dots — Documentation

This page covers everything in more detail: installing, using, and understanding how Privacy Dots works.

## Contents

- [What Privacy Dots does](#what-privacy-dots-does)
- [Installing](#installing)
- [Using the app](#using-the-app)
- [Settings explained](#settings-explained)
- [How detection works](#how-detection-works)
- [Privacy details](#privacy-details)
- [Performance](#performance)
- [Uninstalling](#uninstalling)
- [Building from source](#building-from-source)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)

---

## What Privacy Dots does

Privacy Dots shows a small colored dot on your screen whenever your camera or microphone is being used by any app:

| Dot | Meaning |
|-----|---------|
| 🟢 Green | Camera is in use |
| 🟠 Orange | Microphone is in use |
| Both dots | Camera **and** microphone are in use |
| No dots | Neither is in use |

The dots float on top of everything — full-screen video calls, games, browsers — so you always know when something is watching or listening. When both devices are idle, the overlay disappears completely.

The dots are not a window in the usual sense. They have no border, no background, and no presence in the taskbar or Alt-Tab. Your mouse clicks pass straight through them, so they never get in the way.

## Installing

1. Download `PrivacyDots-Setup-1.1.0.exe` from the [`dist`](../dist/) folder (or the Releases page).
2. Double-click it. The setup wizard walks you through:
   - **Install location** — installs per-user by default, so no admin prompt appears
   - **Start with Windows** — tick this if you want the dots active from the moment you log in (recommended)
   - **Desktop shortcut** — optional
3. Click through and finish. The app starts right away and appears in your system tray.

Requirements: Windows 10 or Windows 11. Nothing else — no runtimes, no drivers.

## Using the app

Privacy Dots lives in the system tray (bottom-right of your taskbar, near the clock). The tray icon mirrors the overlay: gray dots when idle, green/orange when the camera/mic is live. Hover over it for a plain-language status.

| Action | Result |
|--------|--------|
| **Double-click** the tray icon | Opens Settings |
| Right-click → **Settings…** | Opens Settings |
| Right-click → **Show test dots (5 s)** | Shows both dots for five seconds so you can check size and position |
| Right-click → **Start with Windows** | Toggles autostart |
| Right-click → **About** | Version and info |
| Right-click → **Exit** | Quits the app (dots stop appearing until you start it again) |

While the Settings window is open, both dots are shown continuously so you can see exactly what your changes look like.

## Settings explained

| Setting | What it does | Default |
|---------|--------------|---------|
| **Dot size** | Diameter of each dot, from 6 px (subtle) to 40 px (impossible to miss). Changes apply live. | 12 px |
| **Position** | Where the dots sit: top left, top center, top right, bottom left, or bottom right of your main screen. | Top right |
| **Edge margin** | How far the dots sit from the screen edge, in pixels. | 14 px |
| **Start with Windows** | Starts Privacy Dots automatically when you log in. | Off (unless chosen during install) |

Click **OK** to save, **Cancel** to discard. Settings are stored in a small plain-text file at `%APPDATA%\PrivacyDots\settings.ini`.

## How detection works

Windows itself keeps track of which apps are using the camera and microphone — that's how the small built-in indicators in the taskbar work. This bookkeeping lives in the Windows registry, under:

```
HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore
```

Each app that has ever used the camera or mic has an entry there with a "last used" start and stop time. While an app is **currently** using a device, its stop time is `0`.

Privacy Dots simply reads these entries about once every 0.7 seconds. If any app's stop time is `0` for the microphone, the orange dot appears; same for the webcam and the green dot. When the app releases the device, Windows fills in the stop time and the dot disappears.

This approach has two nice properties:

1. **It sees everything.** Any app that goes through Windows to reach the camera or mic — which is essentially all of them — is tracked, whether it's a desktop app, a Store app, or a browser tab.
2. **It requires nothing.** No drivers, no hooks, no special permissions, and crucially, no access to the devices themselves.

## Privacy details

- **No camera or microphone access.** Privacy Dots never opens, records from, or even enumerates your devices. It reads Windows' own usage log, nothing more.
- **No network activity.** The app contains no networking code whatsoever. It cannot phone home, check for updates, or send telemetry, because there is simply no code path for it.
- **No data collection.** It writes exactly one file: its own settings (`%APPDATA%\PrivacyDots\settings.ini`), which contains your dot size, position, and margin. That's all.
- **Optional autostart is transparent.** If you enable "Start with Windows", it writes one standard registry value under `HKCU\...\CurrentVersion\Run` — the same mechanism every autostart app uses — and removes it when you turn the option off or uninstall.
- **Open source.** Every line of the app is in this repository. The whole thing is seven short C# files you can read in a few minutes.

## Performance

Privacy Dots is deliberately tiny:

- Single executable, about **36 KB**
- Around **28 MB of RAM** while running
- Near-zero CPU — it does a handful of cheap registry reads per second and only redraws when something changes
- Built on the .NET Framework that ships inside Windows 10/11, so it adds nothing to your system

## Uninstalling

Open **Settings → Apps → Installed apps**, find **Privacy Dots**, and click **Uninstall**. The uninstaller:

1. Closes the running app
2. Removes the program files
3. Removes the autostart entry (even if you enabled it later from inside the app)
4. Deletes your settings folder

Nothing is left behind.

## Building from source

You don't need Visual Studio or any SDK — the build uses the C# compiler that ships with Windows.

```powershell
git clone https://github.com/Believeinus/privacy-dots.git
cd privacy-dots
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This:

1. Generates the app icon (`assets\PrivacyDots.ico`)
2. Compiles `dist\PrivacyDots.exe`
3. Builds the setup wizard `dist\PrivacyDots-Setup-<version>.exe`, if [Inno Setup 6](https://jrsoftware.org/isinfo.php) is installed (`winget install JRSoftware.InnoSetup`)

Use `.\build.ps1 -SkipInstaller` to build only the exe. The exe in `dist` is fully portable — you can run it directly without installing.

### Project layout

```
src/          C# source files (WinForms, .NET Framework 4.x)
  Program.cs        entry point, single-instance guard
  TrayContext.cs    tray icon, menu, polling loop
  OverlayForm.cs    the transparent always-on-top dot overlay
  UsageMonitor.cs   camera/mic detection (registry reads)
  AppSettings.cs    settings load/save, autostart toggle
  SettingsForm.cs   the settings window
tools/        icon generator script
installer/    Inno Setup script for the wizard
build.ps1     one-command build
dist/         build output (exe + installer)
```

## Troubleshooting

**The dots don't appear when my camera/mic is on.**
Make sure Privacy Dots is running (look for its tray icon). Try right-click → *Show test dots* to confirm the overlay itself works. Note that the dots appear on your **main** display.

**The dots appear but I want them somewhere else / bigger.**
Double-click the tray icon and adjust size, position, and margin — changes preview live.

**Something covers the dots.**
The overlay re-asserts "always on top" every 2 seconds, so at worst another aggressive always-on-top app can cover it momentarily. Exclusive-fullscreen games (as opposed to borderless-windowed) bypass all overlays by design of Windows.

**I closed the app by accident.**
Start it again from the Start Menu (search "Privacy Dots"), or log out and back in if autostart is enabled.

## FAQ

**Does it work with Zoom / Teams / Meet / OBS / Discord?**
Yes. Anything that uses the camera or microphone through Windows is detected, including browser tabs.

**Does it slow anything down?**
No. It reads a few registry values per second and draws two circles. That's the entire workload.

**Why does it need to run all the time?**
It can only show you the dots while it's running. It's designed to be so small you'll never notice it — enable "Start with Windows" and forget about it.

**Can malware hide from it?**
Privacy Dots reads Windows' own usage tracking, the same source as the built-in indicators. Malware sophisticated enough to bypass that layer could also hide from Windows itself — no userspace indicator can protect against that. For everyday apps, it's reliable.

**Windows already shows a mic icon in the taskbar. Why this?**
The built-in indicator is tiny, hides in the corner, gives no camera dot on Windows 10, and disappears in fullscreen. Privacy Dots is visible always, everywhere, at whatever size you choose.
