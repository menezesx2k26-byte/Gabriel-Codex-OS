# YouCine Bridge Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a one-click Windows launcher that opens/focuses YouCine through scrcpy, provides fullscreen/free PiP, and recovers ADB/scrcpy failures without Remote Desktop Commander.

**Architecture:** A .NET 10 WinForms process coordinates device resolution, one managed scrcpy session, Win32 window control, global hotkeys, settings, and watchdog recovery. scrcpy remains the media/input transport; the bridge only owns orchestration and window behavior.

**Tech Stack:** .NET 10, C# 14, WinForms, xUnit, Win32 P/Invoke, adb, scrcpy 4.1.

**Spec:** `docs/superpowers/specs/2026-09-12-youcine-bridge-design.md`

## Global Constraints
- Windows x64 only for v1.
- Android package is exactly `com.world.youcinemobile`.
- Managed scrcpy window title is exactly `YouCine-PC`.
- Canonical display profile is 1920x1080/240, 60 fps, 12 Mbps, audio playback.
- Existing PowerShell PiP scripts are not modified.
- Normal operation must not depend on ChatGPT or Remote Desktop Commander.
- Production code follows RED -> GREEN -> REFACTOR; every behavior test is observed failing before implementation.

---

## File Structure
- `apps/youcine-bridge/src/YouCineBridge/` — Windows executable.
- `apps/youcine-bridge/tests/YouCineBridge.Tests/` — unit/integration tests.
- `apps/youcine-bridge/scripts/install.ps1` — publish + shortcut installation.
- `apps/youcine-bridge/README.md` — operation and recovery commands.

### Task 1: Scaffold solution and geometry/settings core

**Files:**
- Create: `apps/youcine-bridge/YouCineBridge.sln`
- Create: `apps/youcine-bridge/src/YouCineBridge/YouCineBridge.csproj`
- Create: `apps/youcine-bridge/tests/YouCineBridge.Tests/YouCineBridge.Tests.csproj`
- Create: `apps/youcine-bridge/src/YouCineBridge/PipGeometry.cs`
- Create: `apps/youcine-bridge/src/YouCineBridge/BridgeSettings.cs`
- Test: `apps/youcine-bridge/tests/YouCineBridge.Tests/PipGeometryTests.cs`
- Test: `apps/youcine-bridge/tests/YouCineBridge.Tests/BridgeSettingsTests.cs`

**Interfaces:**
- Produces: `PipGeometry.Normalize16By9(Rectangle, Rectangle)`, `PipGeometry.DefaultFor(Rectangle)`.
- Produces: `BridgeSettings.Load(string)`, `BridgeSettings.Save(string)` with `LastEndpoint`, `PipBounds`, and tool overrides.

- [ ] **Step 1: Scaffold solution/projects only.** Run `dotnet new sln`, `dotnet new winforms`, `dotnet new xunit`, add references, then `dotnet restore`.
- [ ] **Step 2: Write failing geometry tests.** Assert a 480x300 input normalizes to 480x270 inside the working area and first-use default is bottom-right with 16 px margin.
- [ ] **Step 3: Run `dotnet test --filter PipGeometryTests` and observe missing-type failures.**
- [ ] **Step 4: Implement `PipGeometry` minimally with 16:9 normalization and screen clamping.**
- [ ] **Step 5: Write failing settings round-trip test** using a temp JSON file with endpoint and PiP rectangle.
- [ ] **Step 6: Run the settings test and observe failure, then implement JSON load/save with safe defaults.**
- [ ] **Step 7: Run all tests and commit:** `feat(youcine): add geometry and settings core`.

### Task 2: Resolve adb/scrcpy tools and Android endpoint

**Files:**
- Create: `src/YouCineBridge/ToolLocator.cs`
- Create: `src/YouCineBridge/ProcessRunner.cs`
- Create: `src/YouCineBridge/DeviceResolver.cs`
- Test: `tests/YouCineBridge.Tests/ToolLocatorTests.cs`
- Test: `tests/YouCineBridge.Tests/DeviceResolverTests.cs`

**Interfaces:**
- Produces: `ToolLocator.FindAdb(BridgeSettings)`, `ToolLocator.FindScrcpy(BridgeSettings)`.
- Produces: `DeviceResolver.ResolveAsync(CancellationToken)` returning endpoint + model.
- Consumes: `BridgeSettings.LastEndpoint` and fallback endpoint list.

- [ ] **Step 1: Write failing ToolLocator tests** for explicit override precedence and known WinGet paths.
- [ ] **Step 2: Run the tests and observe failure; implement deterministic path lookup.**
- [ ] **Step 3: Write failing DeviceResolver parser/order tests** for `adb devices -l`, persisted endpoint, `adb mdns services`, and fallback ordering.
- [ ] **Step 4: Run resolver tests and observe failure; implement resolver using an injectable `IProcessRunner`.**
- [ ] **Step 5: Add a test that rejects an unauthorized/offline transport and accepts a `device` transport with the expected model.**
- [ ] **Step 6: Run all tests and commit:** `feat(youcine): add device and tool resolution`.

### Task 3: Build and own one scrcpy session

**Files:**
- Create: `src/YouCineBridge/ScrcpyProfile.cs`
- Create: `src/YouCineBridge/ScrcpySession.cs`
- Test: `tests/YouCineBridge.Tests/ScrcpyProfileTests.cs`
- Test: `tests/YouCineBridge.Tests/ScrcpySessionTests.cs`

**Interfaces:**
- Produces: `ScrcpyProfile.Balanced.BuildArguments(endpoint)`.
- Produces: `ScrcpySession.EnsureRunningAsync(endpoint, ct)`, `Focus()`, `RestartAsync(ct)`, `Stop(userRequested)`.

- [ ] **Step 1: Write failing argument-construction test** requiring `--serial`, `--new-display=1920x1080/240`, `--start-app=+com.world.youcinemobile`, playback audio, 12M bitrate, 60 fps, and title `YouCine-PC`.
- [ ] **Step 2: Run and observe failure; implement immutable `ScrcpyProfile`.**
- [ ] **Step 3: Write failing single-session tests** proving an existing managed process is focused instead of duplicated.
- [ ] **Step 4: Run and observe failure; implement `ScrcpySession` with process discovery scoped by exact window title/owned PID.**
- [ ] **Step 5: Add restart test proving only `com.world.youcinemobile` is force-stopped when a clean relaunch is requested.**
- [ ] **Step 6: Run all tests and commit:** `feat(youcine): manage scrcpy session`.

### Task 4: Fullscreen and free PiP window control

**Files:**
- Create: `src/YouCineBridge/NativeMethods.cs`
- Create: `src/YouCineBridge/WindowController.cs`
- Create: `src/YouCineBridge/PipInteractionHook.cs`
- Test: `tests/YouCineBridge.Tests/WindowControllerTests.cs`
- Test: `tests/YouCineBridge.Tests/PipInteractionTests.cs`

**Interfaces:**
- Produces: `WindowController.EnterFullscreen()`, `EnterPip(Rectangle)`, `TogglePip()`, `Focus()`, `CaptureCurrentPipBounds()`.
- Produces: `PipInteractionHook` that supports top-edge drag and `Alt+LeftDrag` move without swallowing ordinary app clicks.

- [ ] **Step 1: Write failing style-transition tests** against a disposable WinForms test window: PiP removes caption, stays resizable/topmost, fullscreen fills monitor and is not topmost.
- [ ] **Step 2: Run and observe failure; implement Win32 style/state transitions with saved pre-PiP state.**
- [ ] **Step 3: Write failing PiP interaction tests** for hit-zone classification and move gesture activation only in top strip or while Alt is held.
- [ ] **Step 4: Run and observe failure; implement mouse hook/drag coordinator and move with `SetWindowPos`.
- [ ] **Step 5: Add failing resize-normalization test** proving move/resize end persists a clamped 16:9 rectangle; implement persistence call.
- [ ] **Step 6: Run all tests and commit:** `feat(youcine): add fullscreen and free pip controls`.

### Task 5: Application lifecycle, hotkeys, single instance, watchdog

**Files:**
- Create: `src/YouCineBridge/Program.cs`
- Create: `src/YouCineBridge/BridgeApplicationContext.cs`
- Create: `src/YouCineBridge/HotkeyController.cs`
- Create: `src/YouCineBridge/SingleInstanceGate.cs`
- Create: `src/YouCineBridge/SessionWatchdog.cs`
- Test: `tests/YouCineBridge.Tests/SingleInstanceGateTests.cs`
- Test: `tests/YouCineBridge.Tests/RetryPolicyTests.cs`

**Interfaces:**
- `SingleInstanceGate` exposes `IsPrimary` and activation event signaling.
- `BridgeApplicationContext` starts/focuses YouCine, owns tray commands, and coordinates shutdown.
- `SessionWatchdog` relaunches only when the session was not intentionally closed.

- [ ] **Step 1: Write failing single-instance test** proving a second gate signals the primary instead of becoming primary.
- [ ] **Step 2: Run and observe failure; implement mutex + named event gate.**
- [ ] **Step 3: Write failing retry-policy tests** for bounded exponential delays and reset after success.
- [ ] **Step 4: Run and observe failure; implement watchdog policy and cancellation-safe loop.**
- [ ] **Step 5: Add hotkey registration and tray commands** for Open, PiP, Reconnect, Logs, Exit, wired through `BridgeApplicationContext`.
- [ ] **Step 6: Run all tests and commit:** `feat(youcine): add bridge lifecycle and recovery`.

### Task 6: Package, install, and real-device smoke test

**Files:**
- Create: `apps/youcine-bridge/scripts/install.ps1`
- Create: `apps/youcine-bridge/README.md`
- Modify: `apps/youcine-bridge/src/YouCineBridge/YouCineBridge.csproj`

**Interfaces:**
- `install.ps1` publishes `win-x64` and creates a Start Menu shortcut named `YouCine`.

- [ ] **Step 1: Publish:** `dotnet publish src/YouCineBridge/YouCineBridge.csproj -c Release -r win-x64 --self-contained false` and require exit code 0.
- [ ] **Step 2: Write install script** that copies publish output under `%LOCALAPPDATA%\Programs\YouCineBridge` and creates the Start Menu shortcut without modifying taskbar pins.
- [ ] **Step 3: Run full automated suite:** `dotnet test -c Release`; require zero failures.
- [ ] **Step 4: Install locally and launch once.** Verify one `YouCineBridge` process and one managed `scrcpy` window.
- [ ] **Step 5: Real-device smoke:** verify the Moto G75 connects, package launches on virtual display, audio plays, fullscreen toggles, PiP moves/resizes, normal Android clicks work, and `Ctrl+Alt+R` recovers the session.
- [ ] **Step 6: Kill the managed scrcpy process once** and verify watchdog relaunches it without creating duplicates.
- [ ] **Step 7: Commit:** `feat(youcine): package one-click bridge`.

## Plan self-review
- Spec coverage: all eight acceptance criteria map to Tasks 1-6.
- No production feature is implemented before its behavior test fails.
- External-process tests use explicit interfaces so unit tests never require a live phone.
- Real ADB/scrcpy behavior is reserved for the final smoke test.
