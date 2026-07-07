# Changelog

All notable changes to Privacy Dots are documented here. Each version is also available on the [releases page](https://github.com/Believeinus/privacy-dots/releases) with its installer attached.

## [1.1.1] — 2026-07-07

### Fixed

- **Stuck dot on Windows 10** — the green or orange dot could stay on after an app stopped using the camera or microphone, even with the device disconnected. Windows' usage records can contain stale "still running" entries when an app crashes, the PC shuts down mid-use, or — notably on Windows 10 — the stop timestamp is never written. Detection now verifies every active entry against the list of actually running apps (desktop apps by exe name, Store apps by package family name) and ignores entries whose app is gone. When verification isn't possible, the dot is still shown — a false alarm beats a missed one.

## [1.1.0] — 2026-07-06

First public release.

### Added

- Always-on-top overlay: 🟢 green dot while the camera is in use, 🟠 orange dot while the microphone is in use, nothing when idle
- Transparent, click-through overlay with no border, taskbar entry, or Alt-Tab presence
- Settings window: dot size (6–40 px) with live preview, five screen positions, edge margin, start with Windows
- Dynamic tray icon that mirrors the dots, with plain-language status tooltip and test-dots preview
- Setup wizard (per-user, no admin rights) with optional autostart and desktop shortcut
- Clean uninstall that removes the app, autostart entry, and settings
- Detection via Windows' Capability Access Manager consent store — no camera/mic access, no network code, no data collection

[1.1.1]: https://github.com/Believeinus/privacy-dots/releases/tag/v1.1.1
[1.1.0]: https://github.com/Believeinus/privacy-dots/releases/tag/v1.1.0
