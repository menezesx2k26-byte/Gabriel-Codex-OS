---
name: persistent-conversation-controller
description: Use when an explicitly authorized browser conversation must supervise a long-running workflow across context limits, session expiry, idle worker responses, or repeated controller-window rollover until verified completion.
---

# Persistent Conversation Controller

## Principle

Persist the **controller**, not one model context. Browser conversations are replaceable generations; Git, project handoffs, and observed execution state remain authoritative.

Explicit invocation authorizes routine reversible choices and controller-window rollovers within the existing task scope. It does not grant secrets access, destructive actions, safety bypasses, material scope expansion, merge/deploy absent prior authority, or consequential product choices.

**REQUIRED:** use `durable-execution-memory` and `project-handoff-manager`. For the browser surface, use `ego-browser` for an ego task space or `windows-desktop-operator` for the user's interactive Chrome/browser. Remote Desktop Commander is transport/recovery, not a parallel browser architecture.

## Loop

For one stable `RUN_ID`:

1. Reconstruct from repo/Git/handoff before trusting chat memory.
2. Observe the worker and classify: `WORKING`, `WAITING_TOOL`, `IDLE_INCOMPLETE`, `CONTEXT_RISK`, `BLOCKED`, or `DONE`.
3. `WORKING`/`WAITING_TOOL`: do not interfere; re-observe after meaningful change or a bounded interval.
4. `IDLE_INCOMPLETE`: send the unmet acceptance state and exact next reversible action; do not send a context-free "continue".
5. `CONTEXT_RISK`: perform preventive controller rollover.
6. `BLOCKED`: try materially different safe diagnostics; never blindly repeat clicks/prompts.
7. `DONE`: independently verify the Definition of Done before stopping.

An ended response, generated handoff, idle tab, or worker saying "done" is not completion evidence.

## Preventive Rollover

For a ~25-minute controller window, roll over near 20 minutes; otherwise use ~80% of the known limit. Checkpoint earlier when recovery would be expensive.

Before rollover:

1. Update the project's established durable handoff with completed, verified, blocked, current, next, branch/worktree, HEAD, and acceptance state.
2. Update `~/.agents/continuations/<RUN_ID>/CONTROL.md` using `references/controller-baton.md`.
3. Open a successor conversation through the authorized browser surface.
4. Give it `RUN_ID`, baton path, project handoff path, and instruction to load this skill, inspect actual state, claim generation `N+1`, and continue without redoing verified work.
5. Confirm takeover from durable state or another structured signal.
6. After `N+1` is claimed, generation `N` is stale and must stop issuing mutations.

If takeover is unconfirmed, the old controller retains authority and repairs the rollover while budget remains. Opening a successor tab alone is not takeover.

Controller-window rollovers are supervisory replacements, **not** PLAN/IMPLEMENT/REVIEW project handoffs; they do not consume a repository's role-handoff limit.

## Stop Rules

Continue through ordinary context rollover and reversible implementation decisions. Stop only for:

- fresh evidence that all acceptance criteria are complete;
- explicit user cancellation or scope change;
- irreversible/high-impact action lacking authority;
- material product choice that cannot be inferred safely;
- credentials/MFA/manual user control genuinely required;
- the same blocker surviving three materially different safe attempts.

The three-attempt rule applies to repeated failure, not conversation count. Many clean controller generations are acceptable; repeated identical failure is not.

## Red Flags

- stopping because the current 25-minute window is ending;
- treating a fourth controller rollover as a fourth project handoff;
- assuming an opened successor has claimed control;
- two generations mutating simultaneously;
- treating silence or a completed response as `DONE`.

When any occurs: persist state, re-observe, classify, and continue or perform verified takeover.
