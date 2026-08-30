# Controller Baton and Successor Message

The baton is supervisory metadata, not a second project source of truth. Project facts remain in Git and the established project handoff.

Store controller continuity metadata at:

`~/.agents/continuations/<RUN_ID>/CONTROL.md`

Recommended fields:

```text
RUN_ID: <stable workflow id>
GENERATION: <positive integer>
CONTROLLER_ID: <conversation/window identifier if available>
STATUS: ACTIVE | PREPARING_TAKEOVER | ROLLOVER_INCOMPLETE | AUTH_REQUIRED | STALE | DONE
STARTED_AT: <timestamp>
CLAIMED_AT: <timestamp>
PROJECT_ROOT: <path or NONE>
TASK_ID: <task id or NONE>
PROJECT_HANDOFF: <authoritative handoff path or NONE>
BRANCH: <branch or NONE>
HEAD: <sha or NONE>
CURRENT_STATE: <state>
NEXT_SAFE_ACTION: <one concise action>
SUCCESSOR_SURFACE: <surface or NONE>
HANDOFF_INJECTED_AT: <timestamp or NONE>
TAKEOVER_EVIDENCE: <bounded evidence or NONE>
```

Do not store secrets, cookies, credentials, tokens, chain-of-thought, or full transcripts.
## Tooling Preflight

A fresh ChatGPT conversation does **not** inherit the controller plugin automatically. Before injecting the baton, the old controller must enable the required tooling in the successor.

For the current ChatGPT UI:

1. Open the authenticated successor chat.
2. Open `Adicionar arquivos e mais`.
3. Open `Mais`.
4. Select `Remote Desktop Commander`.
5. Verify the composer shows the chip `Remote Desktop Commander, clique para remover` (or equivalent visible attachment state).
6. Only then inject and submit the successor handoff.

If the tool entry is unavailable, the chip cannot be verified, or the successor later cannot execute Commander actions, keep generation `N` authoritative and classify `ROLLOVER_INCOMPLETE` or `AUTH_REQUIRED`; do not send a bare baton and hope the new chat discovers the plugin.

If a workflow genuinely requires another connector in the successor, attach that connector before handoff as well. The minimum controller bootstrap is Remote Desktop Commander because it can read the installed skill, durable baton, and local project state.

## Successor Chat Message

The successor must receive a concise **self-contained handoff message**. A path or an `@skill` mention alone is insufficient.

```text
PERSISTENT CONVERSATION CONTROLLER TAKEOVER
RUN_ID: <RUN_ID>
GENERATION: <N+1>

Remote Desktop Commander was explicitly enabled for this chat before this message was sent.
This handoff is self-contained; do not assume an @skill mention resolves automatically.
First read the installed controller skill via Remote Desktop Commander at:
- Windows: %USERPROFILE%\.agents\skills\persistent-conversation-controller\SKILL.md
- WSL: ~/.agents/skills/persistent-conversation-controller/SKILL.md

Goal: <original workflow goal>
Project: <root or NONE>
Task: <task id or NONE>
Branch / HEAD: <branch> / <sha>
Authoritative handoff: <path or NONE>
Controller baton: ~/.agents/continuations/<RUN_ID>/CONTROL.md

Completed: <concise verified progress>
Verified: <tests/evidence>
Current state: <state>
Remaining acceptance: <what is still unmet>
Exact next action: <one reversible next action>

You are the successor controller. Reconstruct from Git/project handoff first and do not redo verified work. Claim generation <N+1> by updating CONTROL.md to STATUS: ACTIVE with CLAIMED_AT, then emit the complete line `CLAIM <RUN_ID> G<N+1>`. Continue supervising until verified DoD. Before your own context limit, perform another verified rollover using this same protocol.
```

Keep this message short enough for reliable UI injection. Prefer references to authoritative project files over copying long logs, but include enough current state that the successor can recognize the task before opening those files.

## Generation Rule

The active controller is the latest **successfully claimed** generation.

1. Generation `N` writes `STATUS: PREPARING_TAKEOVER`.
2. `N` opens an authenticated successor chat and explicitly enables required controller tooling.
3. `N` verifies the tool attachment state, then injects the self-contained successor message.
4. The successor reconstructs state and claims `N+1` by writing `STATUS: ACTIVE` and `CLAIMED_AT`; when chat output is available it also emits `CLAIM <RUN_ID> G<N+1>`.
5. `N` re-reads the baton and observes the complete claim. Partial or truncated text is activity only and must not retire `N`.
6. Only then does `N` become stale.

If the successor is logged out, lacks the controller tooling required to update durable state, the message was not visibly submitted, or takeover cannot be observed, keep `N` authoritative and classify `ROLLOVER_INCOMPLETE` or `AUTH_REQUIRED` as appropriate.

When verified DoD is reached, record `STATUS: DONE`, final project handoff, and HEAD.
