# YouCine Bridge â€” Verified State

## Objective
Build a one-click Windows bridge that runs YouCine through a persistent Android runtime, with fullscreen, free PiP, single-instance behavior, hotkeys, watchdog recovery, and no normal dependency on the Moto G75.

## Current branch
`feature/youcine-bridge`

## Completed
- C#/.NET 10 WinForms bridge scaffold.
- Settings persistence and PiP geometry.
- adb/scrcpy discovery and device resolution.
- One managed scrcpy session with scoped restart of `com.world.youcinemobile`.
- Fullscreen, windowed mode, free/resizable PiP, saved PiP bounds.
- Single-instance gate, retry policy, global hotkeys, session watchdog.
- Trusted `RuntimeEndpoint` support so a persistent Android runtime can be primary and the Moto G75 fallback.
- `BridgeRuntimeCoordinator` implemented to own open/reconnect/stop lifecycle.

## Verified evidence
- Current full suite: 36/36 tests passed after RuntimeEndpoint and coordinator changes.
- `RuntimeEndpoint` focused tests: 5/5 passed.
- `BridgeRuntimeCoordinatorTests`: 2/2 passed.
- Branch was pushed to GitHub at commit `1c40cbe` before the newest runtime/coordinator changes.

## Infrastructure findings
- Windows firmware virtualization is disabled; do not make local Android Emulator/BlueStacks the primary runtime.
- `persistflow` and `tsim-vm` have Azure kernel 6.17 with Binder/BinderFS support.
- LXD was initialized on both Linux VMs, but `lxc launch` proved unreliable; `tsim-vm` went offline during a VM launch attempt.
- ReDroid remains the preferred persistent Android runtime, but it is not operational yet.
- The Moto G75 ADB endpoint was unreachable during the latest migration probe, so no session/token data was read or copied.

## Current verification gap
Full suite is green at 36/36; commit and push this checkpoint.

