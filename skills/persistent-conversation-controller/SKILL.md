---
name: persistent-conversation-controller
description: Use when an explicitly authorized AI conversation must supervise a long-running workflow across browser/chat context limits, session expiry, idle worker responses, or repeated controller-window rollover until verified completion.
---

# Persistent Conversation Controller

## Contract

Persist the **controller**, not one model context. Git and the project's established handoff remain authoritative; browser conversations are replaceable generations.

A rollover is a **verified UI transaction**. `CONTROL.md` saved, a tab opened, or a path reported is never completion by itself.

Explicit invocation authorizes routine reversible choices and controller rollovers inside the existing task scope. It does not authorize secrets access, destructive actions, security bypasses, material scope expansion, or merge/deploy absent prior authority.

**REQUIRED SUB-SKILLS:** `durable-execution-memory`, `project-handoff-manager`, plus `ego-browser` for an authenticated agent browser or `windows-desktop-operator` for an authenticated interactive browser/app. Remote Desktop Commander is transport/recovery, not completion evidence.

The successor message must be self-contained. Never assume a fresh chat will resolve an `@skill` mention. When Remote Desktop Commander is available, instruct the successor to read the installed skill from `~/.agents/skills/persistent-conversation-controller/SKILL.md` or `%USERPROFILE%\.agents\skills\persistent-conversation-controller\SKILL.md` before mutating state.

## State Loop

| State | Action |
|---|---|
| `WORKING` / `WAITING_TOOL` | Observe; do not interfere |
| `IDLE_INCOMPLETE` | Send unmet acceptance + exact next reversible action |
| `CONTEXT_RISK` | Start preventive rollover |
| `ROLLOVER_INCOMPLETE` | Repair missing UI step; old generation retains authority |
| `AUTH_REQUIRED` | Preserve state; request only required manual authentication |
| `BLOCKED` | Try materially different safe diagnostics |
| `DONE` | Independently verify DoD |

An ended response, saved baton, idle tab, or worker saying "done" is not completion evidence.

## Surface Routing

Use the first **authenticated, controllable** surface:

1. authenticated `ego-browser` task space;
2. existing interactive browser/app via `windows-desktop-operator`;
3. if semantic UI cannot reach the composer, only the existing approved raw-input path.

Never bypass Local Executor policy. A logged-out ChatGPT/Claude page is not a valid successor and must not become an anonymous continuation without the original account/tools.

## Rollover Transaction

For a ~25-minute controller window, target rollover near 20 minutes; otherwise use ~80% of the known limit.

Rollover is complete only when **all** are observed:

1. Project handoff and `~/.agents/continuations/<RUN_ID>/CONTROL.md` are current.
2. An authenticated successor conversation is actually opened.
3. The successor composer receives the **handoff text itself** from `references/controller-baton.md`, not merely a file path.
4. The message is submitted and visibly appears in the successor chat.
5. The successor responds or begins work from that handoff. A partial or truncated response is activity, never a claim.
6. Generation `N+1` claims control by updating durable controller state to `ACTIVE` with `CLAIMED_AT` and, when chat output is available, emits the complete marker `CLAIM <RUN_ID> G<N+1>`. Generation `N` re-reads durable state and observes the complete claim; only then may `N` become stale.

If any item is missing, use `ROLLOVER_INCOMPLETE` or `AUTH_REQUIRED` and keep generation `N` authoritative. Never report “activated” merely because durable files exist.

Controller generations are supervisory replacements, not PLAN/IMPLEMENT/REVIEW project handoffs, so they do not consume project role-handoff limits.

## Stop Rules

Continue through ordinary rollover and reversible in-scope work. Stop only for verified DoD, explicit cancellation/scope change, irreversible action lacking authority, a material uninferable decision, authentication/MFA/manual control genuinely required, or the same blocker surviving three materially different safe attempts.

## Hard Failures

- baton saved but no successor message;
- handoff path reported instead of injected into chat;
- successor tab opened but message not submitted;
- successor activity not observed;
- partial/truncated successor text treated as a claim;
- two generations mutating concurrently;
- raw input used by bypassing the approved control path.

Any of these means rollover is incomplete, not successful.
