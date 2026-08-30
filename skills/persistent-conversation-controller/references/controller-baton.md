# Controller Baton

The baton is supervisory metadata, not a second project source of truth.

For repository work, project facts remain in Git and the repo's established handoff/context files. Store only controller continuity metadata at:

`~/.agents/continuations/<RUN_ID>/CONTROL.md`

Recommended fields:

```text
RUN_ID: <stable workflow id>
GENERATION: <positive integer>
CONTROLLER_ID: <conversation/window identifier if available>
STATUS: ACTIVE | PREPARING_TAKEOVER | STALE | DONE
STARTED_AT: <timestamp>
CLAIMED_AT: <timestamp>
PROJECT_ROOT: <path or NONE>
TASK_ID: <task id or NONE>
PROJECT_HANDOFF: <authoritative handoff path or NONE>
BRANCH: <branch or NONE>
HEAD: <sha or NONE>
CURRENT_STATE: <WORKING|WAITING_TOOL|IDLE_INCOMPLETE|CONTEXT_RISK|BLOCKED|DONE>
NEXT_SAFE_ACTION: <one concise action>
```

## Generation rule

The active controller is the latest **successfully claimed** generation.

Rollover from generation `N`:

1. `N` writes `STATUS: PREPARING_TAKEOVER` and the intended next generation.
2. `N` opens the successor and gives it the baton path.
3. The successor reads project state first, then claims `N+1` by writing its generation, controller id, `STATUS: ACTIVE`, and `CLAIMED_AT`.
4. `N` re-reads the baton. Only after observing the valid `N+1` claim does it become stale.
5. A controller that observes a higher claimed generation must not issue further mutations.

If two candidate successors appear, only the generation recorded in the baton is authoritative. Re-observe before any mutation when ownership is ambiguous.

Do not store secrets, cookies, tokens, credentials, chain-of-thought, or full chat transcripts in this file.

When the workflow reaches verified `DONE`, record `STATUS: DONE` and the final project handoff/HEAD. Retain or remove the local baton according to the user's normal cleanup policy; it is operational metadata, not required project history.
