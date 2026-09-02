# Persist Remote Control Contract

This contract is mandatory for every active `/persist` generation when the workflow touches the user's authorized PC, browser, or interactive Windows session.

## Required control stack

Use the narrowest reliable surface in this order. Do not skip a healthier structured layer merely because raw input is easier.

1. **Remote Desktop Commander** — mandatory recovery and machine-access transport for filesystem, terminal, process, and read-only capability probes. A successor that needs local project access is not ready until Commander is attached and verified.
2. **Browser Bridge / Playwright** — preferred browser automation outside the controller's own isolated `ego-browser` rollover task space. Use semantic DOM, role/text locators, auto-waiting, and an already-authorized authenticated profile. Never use screen coordinates when DOM control is available.
3. **Dev-Orquestra Windows Interactive Control** — preferred native Windows UI surface. Use `desktop.windows.*`, `desktop.ui.inspect`, `desktop.ui.invoke`, `desktop.ui.set_value`, and `desktop.ui.select` with exact refs and observe-act-verify discipline.
4. **Structured screenshots plus raw input** — last-resort compatibility path only after the semantic layer is unavailable or cannot express the required action. Re-observe immediately before and after the action; never blind-repeat a click or keystroke.
5. **Human handoff** — authentication, CAPTCHA, MFA, protected UI, or genuinely non-automatable control. Preserve durable state and resume from the same checkpoint after the user finishes.

AnyDesk, RDP, Parsec, VNC, and similar human remote-desktop products are rescue/viewing channels, not the automation primitive for persistd.
## Capability gate

Before a worker performs a local/interactive mutation, it must establish which required surface is available and choose the highest applicable healthy layer above. A successful shell command is not proof of a visible UI result, and a screenshot is not proof that the intended semantic target was acted on.

If the required structured surface is unavailable, record/return `WAITING_TOOL` or the specific unavailable state and recover it. Do not silently downgrade to ad-hoc PowerShell, SendKeys, coordinate clicking, image matching, or a newly invented desktop agent.

A downgrade is permitted only when:
- the preferred layer was actually probed and is unavailable or insufficient for this exact operation;
- the fallback is already authorized by the existing control plane;
- the operation remains reversible and bounded;
- the result is independently re-observed.

## Browser ownership

The `persist:<RUN_ID>` ego-browser task space remains dedicated to controller rollover and chat lifecycle. Browser Bridge / Playwright is the preferred surface for other browser work when it can use an authorized session. Do not make the controller task space a general browsing scratchpad.

Never restart a personal browser merely to expose an unrestricted debugging endpoint, copy cookies/tokens, or bypass login. If an authenticated existing session can be attached through an approved bridge, use that path; otherwise request the minimum user authentication.

## Authentication handoff

When login, MFA, CAPTCHA, device confirmation, or another protected interaction blocks progress:
- stop the automation at the protected boundary;
- keep the current controller generation and lease authoritative;
- notify the user with the exact action that needs human input;
- avoid opening repeated tabs or retry loops while waiting;
- after the user completes it, re-observe the same application/session and continue automatically from the durable checkpoint.

## Fail-closed completion

A worker must never report a remote/desktop/browser action as successful solely because a tool invocation returned success. Completion requires post-action evidence from the appropriate structured state or fresh observation.

For persisted workflows, inability to obtain the required control evidence is an operational blocker, not permission to guess.