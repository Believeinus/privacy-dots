<div align="center">

# Privacy Dots

**Know the moment any app uses your camera or microphone.**

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D4)](https://github.com/Believeinus/privacy-dots)
[![License](https://img.shields.io/badge/license-MIT-2ea44f)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.1.1-orange)](dist/)
[![Size](https://img.shields.io/badge/app%20size-~36%20KB-lightgrey)](dist/)
[![Dependencies](https://img.shields.io/badge/dependencies-none-brightgreen)](docs/DOCUMENTATION.md)

</div>

Privacy Dots puts a tiny colored dot on your screen whenever your camera or microphone is live — always on top, never in your way.

| Indicator | Meaning |
|:---------:|---------|
| 🟢 | Camera is in use |
| 🟠 | Microphone is in use |
| *(nothing)* | All quiet — no app is watching or listening |

## ✨ Features

- **Always visible** — the dots stay above every app and window, so you can never miss them
- **Never in the way** — transparent, click-through overlay; no border, no focus stealing, no Alt-Tab entry
- **Yours to adjust** — dot size (6–40 px), position (any corner or top center), and edge margin, with live preview
- **Smart tray icon** — mirrors the dots and tells you in plain words what's active
- **Set and forget** — optional start with Windows
- **Clean install, cleaner uninstall** — a standard wizard in; one click in *Settings → Apps* out, nothing left behind

## 🔒 Your privacy comes first

> [!NOTE]
> Privacy Dots is a privacy tool, so it holds itself to the same standard it monitors.

- **Never touches your camera or mic.** It reads the usage records Windows itself keeps — the same source behind Windows' own indicators — so it knows a device is busy without ever accessing it.
- **Zero network code.** No telemetry, no analytics, no update pings. It *can't* phone home.
- **Collects nothing.** The only file it writes is its own settings on your PC.
- **Fully open source.** Seven short C# files — read them in [`src/`](src/).

## 🪶 Lightweight by design

Single **~36 KB** executable · ~28 MB RAM · near-zero CPU · built on the .NET Framework already inside Windows — nothing extra to install.

## 🚀 Get started

1. Download **[PrivacyDots-Setup-1.1.1.exe](https://github.com/Believeinus/privacy-dots/releases/latest/download/PrivacyDots-Setup-1.1.1.exe)** from the [latest release](https://github.com/Believeinus/privacy-dots/releases/latest)
2. Run the wizard (no admin rights needed)
3. That's it — the dots appear whenever your camera or mic goes live

> [!TIP]
> Double-click the tray icon for settings, or right-click → *Show test dots* to preview placement. Full guide in the **[Documentation](docs/DOCUMENTATION.md)**.

## 🛠️ Build from source

No IDE, no SDK — Windows ships with everything needed:

```powershell
git clone https://github.com/Believeinus/privacy-dots.git
cd privacy-dots
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

Details in the [documentation](docs/DOCUMENTATION.md#building-from-source).

## 📄 License

Open source under the [MIT License](LICENSE) — free to use and share.

---

<div align="center">

Developed by **Hiteshwar Singh**

</div>
