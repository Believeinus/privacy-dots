<div align="center">

# 📖 Privacy Dots — Documentation

Everything about installing, using, and understanding Privacy Dots.

</div>

## Contents

- [What Privacy Dots does](#what-privacy-dots-does)
- [Installing](#installing)
- [Using the app](#using-the-app)
- [Settings explained](#settings-explained)
- [How detection works](#how-detection-works)
- [Under the hood (for developers)](#under-the-hood-for-developers)
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
|:---:|---------|
| 🟢 | Camera is in use |
| 🟠 | Microphone is in use |
| 🟢 🟠 | Camera **and** microphone are in use |
| — | Neither is in use (overlay disappears completely) |

The dots float on top of everything — video calls, games, browsers — so you always know when something is watching or listening.

The dots are not a window in the usual sense. They have no border, no background, and no presence in the taskbar or Alt-Tab. Your mouse clicks pass straight through them, so they never get in the way.

## Installing

1. Download `PrivacyDots-Setup-1.1.0.exe` from the [`dist`](../dist/) folder
2. Double-click it. The setup wizard walks you through:
   - **Install location** — installs per-user by default, so no admin prompt appears
   - **Start with Windows** — tick this if you want the dots active from the moment you log in *(recommended)*
   - **Desktop shortcut** — optional
3. Finish. The app starts right away and appears in your system tray.

> [!NOTE]
> **Requirements:** Windows 10 or 11. Nothing else — no runtimes, no drivers.

## Using the app

Privacy Dots lives in the system tray (bottom-right of your taskbar, near the clock). The tray icon mirrors the overlay: gray dots when idle, green/orange when the camera/mic is live. Hover over it for a plain-language status.

| Action | Result |
|--------|--------|
| **Double-click** the tray icon | Opens Settings |
| Right-click → **Settings…** | Opens Settings |
| Right-click → **Show test dots (5 s)** | Shows both dots for five seconds to check size and position |
| Right-click → **Start with Windows** | Toggles autostart |
| Right-click → **About** | Version and info |
| Right-click → **Exit** | Quits the app |

> [!TIP]
> While the Settings window is open, both dots stay visible so you can see your changes live.

## Settings explained

| Setting | What it does | Default |
|---------|--------------|:-------:|
| **Dot size** | Diameter of each dot, from 6 px (subtle) to 40 px (impossible to miss). Applies live. | 12 px |
| **Position** | Top left, top center, top right, bottom left, or bottom right of your main screen | Top right |
| **Edge margin** | Distance from the screen edge, in pixels | 14 px |
| **Start with Windows** | Launches Privacy Dots automatically at login | Off |

Click **OK** to save, **Cancel** to discard. Settings live in a small plain-text file: `%APPDATA%\PrivacyDots\settings.ini`.

## How detection works

Windows itself keeps track of which apps are using the camera and microphone — that's how the built-in taskbar indicators work. This bookkeeping lives in the registry:

```
HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore
```

Under `ConsentStore\microphone` and `ConsentStore\webcam`, every app that has ever used the device has a subkey with two QWORD values: `LastUsedTimeStart` and `LastUsedTimeStop`. While an app is **currently** using the device, its `LastUsedTimeStop` is `0`. (Store apps get a subkey named after their package; classic desktop apps live one level deeper, under `NonPackaged`.)

Privacy Dots polls these keys about once every 0.7 seconds, in both `HKCU` and `HKLM`. If any subkey has `LastUsedTimeStop == 0` under `microphone`, the orange dot appears; same for `webcam` and the green dot. When the app releases the device, Windows writes the stop timestamp and the dot disappears.

This approach has two nice properties:

1. **It sees everything.** Any app that goes through Windows to reach the camera or mic — which is essentially all of them — is tracked: desktop apps, Store apps, browser tabs.
2. **It requires nothing.** No drivers, no hooks, no special permissions, and crucially, no access to the devices themselves.

## Under the hood (for developers)

The overlay is a WinForms window with the extended styles `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST`:

- `WS_EX_LAYERED` + `UpdateLayeredWindow` gives per-pixel alpha — the window *is* the dots, drawn with GDI+ anti-aliasing; there is no background to make transparent
- `WS_EX_TRANSPARENT` makes it click-through
- `WS_EX_TOOLWINDOW` + `WS_EX_NOACTIVATE` keep it out of Alt-Tab and the taskbar, and stop it from ever taking focus
- A 2-second timer re-asserts `HWND_TOPMOST` via `SetWindowPos`, so other topmost windows can't permanently cover it

Detection is plain `Microsoft.Win32.Registry` reads on a 700 ms WinForms timer — no WMI, no ETW, no device enumeration. The overlay only redraws when the mic/camera state, settings, or screen geometry actually change. The whole app is seven C# files targeting .NET Framework 4.x (in-box on Windows 10/11), compiled with the stock `csc.exe` — no NuGet packages, no external dependencies at all.

## Privacy details

- **No camera or microphone access.** Privacy Dots never opens, records from, or even enumerates your devices. It reads Windows' own usage log, nothing more.
- **No network activity.** The app contains no networking code whatsoever. It cannot phone home, check for updates, or send telemetry — there is simply no code path for it.
- **No data collection.** It writes exactly one file: its own settings (`%APPDATA%\PrivacyDots\settings.ini`) with your dot size, position, and margin.
- **Transparent autostart.** "Start with Windows" writes one standard registry value under `HKCU\...\CurrentVersion\Run` — the same mechanism every autostart app uses — and removes it when you turn the option off or uninstall.
- **Open source.** Every line is in this repository — seven short C# files you can read in a few minutes.

## Performance

| Metric | Value |
|--------|-------|
| Executable size | ~36 KB |
| Memory | ~28 MB |
| CPU | Near-zero (a few registry reads per second) |
| Dependencies | None — uses the .NET Framework inside Windows |

## Uninstalling

Open **Settings → Apps → Installed apps**, find **Privacy Dots**, click **Uninstall**. The uninstaller:

1. Closes the running app
2. Removes the program files
3. Removes the autostart entry (even if enabled later from inside the app)
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

<details>
<summary><b>The dots don't appear when my camera/mic is on</b></summary>

Make sure Privacy Dots is running (look for its tray icon). Try right-click → *Show test dots* to confirm the overlay itself works. Note that the dots appear on your **main** display.
</details>

<details>
<summary><b>The dots appear but I want them somewhere else / bigger</b></summary>

Double-click the tray icon and adjust size, position, and margin — changes preview live.
</details>

<details>
<summary><b>Something covers the dots</b></summary>

The overlay re-asserts "always on top" every 2 seconds, so at worst another aggressive always-on-top app can cover it momentarily. Exclusive-fullscreen games (as opposed to borderless-windowed) bypass all overlays by design of Windows.
</details>

<details>
<summary><b>I closed the app by accident</b></summary>

Start it again from the Start Menu (search "Privacy Dots"), or log out and back in if autostart is enabled.
</details>

## FAQ

<details>
<summary><b>Does it work with Zoom / Teams / Meet / OBS / Discord?</b></summary>

Yes. Anything that uses the camera or microphone through Windows is detected, including browser tabs.
</details>

<details>
<summary><b>Does it slow anything down?</b></summary>

No. It reads a few registry values per second and draws two circles. That's the entire workload.
</details>

<details>
<summary><b>Why does it need to run all the time?</b></summary>

It can only show you the dots while it's running. It's designed to be so small you'll never notice it — enable "Start with Windows" and forget about it.
</details>

<details>
<summary><b>Can malware hide from it?</b></summary>

Privacy Dots reads Windows' own usage tracking, the same source as the built-in indicators. Malware sophisticated enough to bypass that layer could also hide from Windows itself — no userspace indicator can protect against that. For everyday apps, it's reliable.
</details>

<details>
<summary><b>Windows already shows a mic icon in the taskbar. Why this?</b></summary>

The built-in indicator is tiny, hides in the corner, gives no camera dot on Windows 10, and disappears in fullscreen. Privacy Dots is visible always, everywhere, at whatever size you choose.
</details>

---

<div align="center">

Developed by **Hiteshwar Singh** · [MIT License](../LICENSE)

</div>
