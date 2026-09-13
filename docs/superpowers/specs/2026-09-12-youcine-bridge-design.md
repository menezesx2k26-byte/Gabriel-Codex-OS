# YouCine Bridge Design

## Goal
Build a one-click Windows launcher that makes the Android YouCine app feel like a native desktop app while keeping the phone as the execution device.

## User experience
- Clicking `YouCine` launches or focuses the existing session.
- Normal launch opens the app directly on a dedicated 1920x1080 virtual Android display.
- Playback audio is forwarded to Windows.
- The default presentation is fullscreen and borderless.
- `Ctrl+Alt+P` toggles a free, always-on-top PiP mode.
- PiP remembers its last position and size.
- PiP can be moved anywhere and resized from its edges/corners.
- Double-clicking the PiP returns to fullscreen.
- Repeated launcher clicks never create duplicate sessions.

## Existing assets to preserve
- ADB: Google Platform Tools 37.0.1.
- scrcpy: 4.1.
- Android package: `com.world.youcinemobile`.
- Known launcher activity: `com.mobile.brasiltv.activity.SplashAty`.
- Existing PowerShell PiP scripts remain untouched as diagnostic fallback.

## Runtime architecture
`YouCineBridge.exe` is a small Windows desktop process built with .NET 10 and WinForms/Win32 APIs. It owns no video decoder: scrcpy remains responsible for video, audio, input injection, and the Android virtual display.

The bridge is split into focused units:
- `DeviceResolver` locates and reconnects the phone over ADB without hard-coding one endpoint forever.
- `ScrcpySession` creates, focuses, restarts, and terminates the one allowed YouCine scrcpy process.
- `WindowController` owns fullscreen/PiP state, positioning, topmost state, resize constraints, and focus.
- `HotkeyController` handles global shortcuts.
- `BridgeSettings` persists the preferred endpoint and PiP rectangle under `%LOCALAPPDATA%\YouCineBridge`.
- `BridgeApplicationContext` coordinates tray, lifecycle, watchdog, and single-instance behavior.

## Device resolution
Resolution order is deterministic and bounded:
1. Reuse a currently connected ADB device whose model matches the configured phone identity.
2. Try the last successful endpoint persisted in settings.
3. Ask `adb mdns services` for `_adb-tls-connect._tcp` endpoints and probe them.
4. Probe configured fallback endpoints, initially including `100.106.31.127:38177`.
5. If no device is reachable, show one actionable tray notification and retry on demand.

The launcher never depends on Remote Desktop Commander for normal operation. RDC remains an administrative recovery tool only.

## scrcpy session profile
The canonical balanced profile is:
- `--new-display=1920x1080/240`
- `--start-app=+com.world.youcinemobile`
- `--no-vd-system-decorations`
- `--audio-source=playback`
- `--video-bit-rate=12M`
- `--max-fps=60`
- `--window-title=YouCine-PC`
- fullscreen on first launch.

## PiP interaction
PiP is borderless, topmost, and freely placeable. The bridge preserves normal Android tap/swipe input by separating move gestures from ordinary left-click interaction:
- resize uses the native resizable frame hit area with the caption removed;
- `Alt+LeftDrag` anywhere on the PiP moves the window;
- a thin top-edge grab zone also moves the window without a modifier;
- ordinary left click/drag outside that grab zone continues to control Android;
- resize completion snaps the client area back to 16:9 within a small tolerance;
- the final PiP rectangle is persisted after move/resize;
- double-click toggles back to fullscreen.

Default PiP size is 480x270 with 16 px screen margins only on first use. After that, the user's last valid rectangle wins.

## Hotkeys
- `Ctrl+Alt+P`: toggle PiP/fullscreen.
- `Ctrl+Alt+Y`: focus the YouCine window.
- `Ctrl+Alt+R`: reconnect/restart the session.
- `F11` while YouCine is focused: toggle fullscreen/windowed presentation.

## Resilience
A watchdog checks the scrcpy process and ADB transport periodically while a session is expected to be alive. Recovery is conservative:
- if scrcpy exits unexpectedly, resolve the device and relaunch once;
- repeated failures use bounded exponential backoff;
- user-requested close disables automatic relaunch;
- ADB server restart is a last recovery step, not the first response;
- logs rotate under `%LOCALAPPDATA%\YouCineBridge\logs`.

## Single instance and activation
A named mutex guarantees one bridge process. A second launch signals the existing process through a named event and exits. The running bridge responds by starting YouCine if absent or focusing it if already running.

## Packaging and desktop integration
The project builds a Windows x64 executable with no console window. A setup script publishes the app, creates a Start Menu shortcut, and optionally pins nothing automatically; the shortcut is suitable for manual pinning to taskbar. The launcher locates adb and scrcpy from known WinGet paths first and supports explicit overrides in settings.

## Failure behavior
Failures must be visible but not noisy. The tray icon exposes `Open YouCine`, `Reconnect`, `Open logs`, and `Exit`. Missing adb/scrcpy or a missing Android package produces a precise notification rather than a silent loop.

## Safety boundaries
The bridge controls only the user's authorized Android device and the YouCine scrcpy session. It does not alter unrelated Android packages, globally kill arbitrary scrcpy instances, or depend on browser automation. Force-stop is scoped to `com.world.youcinemobile` only when a clean relaunch is explicitly required.

## Testing strategy
Unit tests cover endpoint parsing, resolver ordering, scrcpy argument construction, PiP rectangle validation, 16:9 normalization, retry/backoff, and settings serialization. Windows integration tests cover single-instance signaling and window-style transitions using a disposable test window. A manual smoke test verifies real ADB connection, app launch, fullscreen, PiP movement, resize, focus, and restart on the Moto G75.

## Acceptance criteria
1. One click starts or focuses YouCine without opening a PowerShell window.
2. Only one bridge and one managed YouCine scrcpy session exist at a time.
3. Initial playback is fullscreen with Android UI isolated to the virtual display.
4. PiP is freely movable/resizable, topmost, and remembers its rectangle.
5. Normal Android click/swipe remains usable in PiP.
6. The launcher can recover from a scrcpy crash and from a changed ADB endpoint when discovery can find the phone.
7. Normal use requires neither ChatGPT nor Remote Desktop Commander.
8. Existing PowerShell scripts remain available as fallback diagnostics.
