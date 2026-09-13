# YouCine Bridge — Handoff

## Current phase
Tray/application lifecycle is wired; finish Windows packaging/install and bring up the persistent Android runtime that does not depend on the phone.

## Completed
- Core bridge architecture implemented through TDD.
- Free PiP, fullscreen/windowed mode, hotkeys, single-instance, watchdog, runtime endpoint preference, and coordinator exist in code.
- `BridgeApplicationContext` now owns tray commands, startup, hotkeys, PiP hook, watchdog, and shutdown.
- `Program.cs` now composes the real bridge runtime and signals the primary process on repeated launcher clicks.
- Existing scrcpy presentation state is synchronized before hotkey/window commands are used.
- Durable design and implementation plan live under `docs/superpowers/`.
- Branch `feature/youcine-bridge` exists on GitHub.

## Validated
- 46/46 full-suite tests pass with the tray/application wiring.
- RuntimeEndpoint/coordinator tests remain green.
- Window state synchronization has explicit fullscreen/PiP regression tests.

## Pending
- Publish/install the Windows executable and create the Start Menu shortcut.
- Bring up a stable persistent ReDroid host; `tsim-vm` is preferred if it recovers.
- Perform one-time authenticated-state migration only if supported; never fabricate server entitlement.
- Real-device/runtime smoke test remains required.

## Exact next action
Implement/verify packaging (`install.ps1` + Release publish), install locally, then continue the persistent ReDroid/runtime smoke path.
