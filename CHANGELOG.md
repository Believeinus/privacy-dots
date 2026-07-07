# Changelog

Notable changes per version. Every version's installer is on the [releases page](https://github.com/Believeinus/privacy-dots/releases).

## [1.1.1] — 2026-07-07

### Fixed

- Dot stuck on after the app using the camera/mic had closed (seen on Windows 10, even with the device unplugged). Cause: stale ConsentStore entries whose `LastUsedTimeStop` stays `0` after a crash, forced shutdown, or missed stop write. Detection now verifies each active entry's owner is actually running — desktop apps by the exe name encoded in the `NonPackaged` key path, Store apps by package family name via `GetPackageFamilyName` — and ignores dead entries. Entries that can't be verified still show the dot.

## [1.1.0] — 2026-07-06

First public release.

### Added

- Overlay: green dot = camera in use, orange = microphone in use, hidden when idle. Per-pixel-alpha layered window, click-through, no taskbar/Alt-Tab presence, re-asserts topmost every 2 s
- Detection: polls `CapabilityAccessManager\ConsentStore` (HKCU + HKLM) every 700 ms — no device access, no drivers, no network code
- Settings: dot size 6–40 px with live preview, five screen positions, edge margin, start with Windows; stored in `%APPDATA%\PrivacyDots\settings.ini`
- Tray icon mirroring the dots, status tooltip, 5-second test mode
- Inno Setup wizard: per-user install (no admin), optional autostart and desktop shortcut; uninstall removes app, autostart entry, and settings

[1.1.1]: https://github.com/Believeinus/privacy-dots/releases/tag/v1.1.1
[1.1.0]: https://github.com/Believeinus/privacy-dots/releases/tag/v1.1.0
