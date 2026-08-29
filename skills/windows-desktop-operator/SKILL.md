---
name: windows-desktop-operator
description: Use when an authorized Windows PC must be observed or controlled through its interactive desktop, including screenshots, windows, visible UI, mouse, keyboard, media, clipboard, or verification of on-screen state instead of relying only on shell output.
---

# Windows Desktop Operator

## Overview

Use the interactive Windows control plane when the result depends on what is actually visible or actionable in the logged-in desktop session. Treat shell/process state as supporting evidence, not a substitute for observing the UI.

The authoritative desktop-control implementation belongs in Dev-Orquestra `WINDOWS-INTERACTIVE-CONTROL-001`; this skill defines how to operate it. Do not create a parallel desktop agent just to complete a task.

## Routing

Use the narrowest reliable surface in this order:

1. Prefer structured `desktop.*` operations exposed by the Dev-Orquestra Local Executor.
2. Prefer exact window refs and semantic UI Automation over coordinates.
3. For browser work that fits the dedicated Browser Bridge profile, use Browser Bridge rather than desktop input.
4. Use `desktop.input.*` raw mouse/keyboard only when semantic control cannot perform the requested action and the existing risk/approval policy permits it.
5. Remote Desktop Commander remains transport/recovery infrastructure. Do not turn ad-hoc PowerShell, SendKeys, or Win32 snippets into the permanent control architecture.

## Observe, Act, Verify

For every visible-state operation:

1. **Observe:** obtain a fresh screenshot, `desktop.windows.list`, or bounded `desktop.ui.inspect` result appropriate to the task.
2. **Resolve:** select the exact window/UI ref. Fail on ambiguity or stale refs; never guess between similar windows from title text alone.
3. **Act:** perform one bounded semantic operation when possible.
4. **Verify:** re-observe the relevant region/state and confirm the requested effect actually occurred.
5. **Recover:** if the observed state differs from expectation, inspect again before issuing another mutation. Do not blindly repeat clicks or keystrokes.

For multi-step deterministic flows, compose safe semantic actions when supported, but retain an observable checkpoint before consequential transitions.

## Screen Observation

A screenshot is evidence of current desktop state. Prefer a fresh capture when the user refers to words such as "this", "that window", "the video", "the button", "fullscreen", or any state that can change visually.

Do not infer success from a command returning success. A fullscreen toggle, click, focus change, or media action is complete only after the visible or structured state confirms it.

Until the structured `desktop.screenshot` runtime is deployed, Remote Desktop Commander may bootstrap observation by capturing the interactive screen to a temporary PNG and reading that image. Treat this as a temporary compatibility path, not the target implementation.

## Interaction Rules

- Use exact window refs when multiple windows of the same application exist.
- Prefer `desktop.ui.invoke`, `desktop.ui.set_value`, and `desktop.ui.select` over raw input.
- Keep raw clicks inside validated virtual-screen bounds and target only the state just observed.
- Never type secrets, extract password-field values, scrape credentials/cookies/tokens, or bypass protected controls.
- Do not silently fall back from a failed structured desktop operation to arbitrary shell automation. Surface the failure, re-observe, or use another already-authorized structured route.
- If the desktop host is unavailable, report `DESKTOP_HOST_UNAVAILABLE` rather than pretending an action occurred.

## Control-Plane Survivability

The desktop-control surface must not endanger the channel used to recover the PC. Do not use desktop operations to stop, uninstall, reconfigure, or interfere with Remote Desktop Commander, Tailscale, RDP/network configuration, firewall, DNS, the Dev-Orquestra Write Control Plane, or their persistence mechanisms.

Do not introduce generic process-kill, service, registry, arbitrary shell, installer, or network-management primitives into this skill.

## Browser Boundary

Use Browser Bridge for DOM-level automation in its dedicated profile when that satisfies the task. Treat the user's already-open personal Chrome like another interactive Windows application unless a separately designed, authorized personal-session browser adapter exists.

Do not restart personal Chrome with unrestricted remote debugging, reuse/extract its cookies, or attach an unrestricted CDP endpoint merely to gain control.

## Completion Rule

Before saying a desktop task succeeded, have evidence from a post-action screenshot or structured state read that directly confirms the requested outcome.

For implementation work on the control plane itself, continue from the existing Dev-Orquestra design and implementation plan for `WINDOWS-INTERACTIVE-CONTROL-001`; do not reinvent the bridge.