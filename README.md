# Privacy Dots for Windows

Ever wondered if an app is quietly using your camera or microphone? Privacy Dots gives you a simple answer — a tiny colored dot on your screen, visible at all times.

- 🟢 **Green dot** — your **camera** is in use
- 🟠 **Orange dot** — your **microphone** is in use
- **No dot** — nothing is watching or listening

That's it. No windows, no popups, no noise. Just dots.

![Privacy Dots](assets/PrivacyDots.ico)

## Features

- **Always visible** — the dots stay on top of every app, even fullscreen windows, so you can never miss them
- **Completely unobtrusive** — the overlay is transparent and click-through; it never blocks your mouse, steals focus, or shows up in Alt-Tab
- **Adjustable** — pick the dot size (6–40 px), screen position (any corner or top center), and distance from the edge
- **Live tray icon** — the tray icon also lights up green/orange, and its tooltip tells you what's active
- **Start with Windows** — optional, one checkbox
- **Simple install, clean uninstall** — a standard setup wizard; removing it from *Settings → Apps* deletes everything, including saved settings
- **Works on Windows 10 and 11** — nothing extra to install

## Your privacy comes first

This is a privacy tool, so it holds itself to the same standard:

- **It never touches your camera or microphone.** It only reads the usage records Windows itself keeps (the same information behind Windows' own tiny indicators) — so it can tell you a device is in use without ever accessing the device.
- **It never connects to the internet.** No telemetry, no analytics, no update checks, no accounts. It has no network code at all.
- **It collects nothing.** The only file it writes is a small settings file on your own PC (`%APPDATA%\PrivacyDots\settings.ini`).
- **It's open source.** The whole app is a few small, readable C# files — see for yourself in [`src/`](src/).

## Lightweight by design

- The app is a single **~36 KB** executable
- Uses roughly **28 MB of RAM** and near-zero CPU
- No frameworks or runtimes to install — it uses the .NET Framework already built into Windows 10 and 11

## Install

1. Download **[PrivacyDots-Setup-1.1.0.exe](dist/PrivacyDots-Setup-1.1.0.exe)**
2. Run it and follow the wizard (no admin rights needed)
3. Done — Privacy Dots sits in your system tray and the dots appear whenever your camera or mic goes live

To uninstall: *Settings → Apps → Privacy Dots → Uninstall*. Everything is removed cleanly.

## Quick start

- **Double-click the tray icon** to open Settings (dot size, position, edge margin, autostart)
- **Right-click the tray icon** → *Show test dots (5 s)* to see where your dots will appear
- Want a quick real test? Open the Windows Camera app or start a voice recording — the dots light up within a second

Full details are in the **[Documentation](docs/DOCUMENTATION.md)**.

## Build it yourself

No SDK or IDE needed — Windows already ships with everything required:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

See the [Documentation](docs/DOCUMENTATION.md#building-from-source) for details.

## License

Privacy Dots is open source under the [MIT License](LICENSE) — free to use, modify, and share.

---

Developed by **Hiteshwar Singh**
