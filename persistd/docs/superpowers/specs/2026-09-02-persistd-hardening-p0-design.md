# Persistd Hardening P0 Design

## Goal
Make `persistd` physically reliable as well as logically correct: cleanup must never be forgotten, failed successors must be proven absent, Desktop Commander setup must have one implementation, and the Windows Edge launcher policy must be versioned in the first-party repo.

## Authority and lifecycle
`CONTROL.md` remains the sole durable authority for generation/claim state. Browser/taskspace discovery remains metadata-only and may never add chat authority. `DONE` is semantic completion; physical browser cleanup is separate durable debt that is retried until verified clean.

Cleanup is successful only after read-after-write verification. A close operation that merely returns without throwing is insufficient; the target must be observed absent. Cosmetic rename is never part of completion.

## Cleanup debt
Replace terminal browser-cleanup abandonment with durable retry metadata: `BROWSER_CLEANUP_STATUS=RETRY_SCHEDULED`, attempts, next retry timestamp, and debt-since timestamp. A `DONE` run with cleanup debt remains resolver-eligible for janitor ticks until browser cleanup verifies success.

The janitor uses bounded exponential backoff and never re-sends the terminal user notification. If cleanup repeatedly fails, state remains retryable instead of becoming forgotten.
## Verified close
`closeFailedSuccessor()` accepts success only when the browser close result has `ok === true`, then verifies the exact chat/target is absent. If chat close is incomplete, fallback to exact `targetId`; if absence still cannot be proven, record cleanup debt and sweep scratch targets.

## Single Commander bootstrap
Extract the ChatGPT Desktop Commander attachment/capability setup into one browser-script primitive shared by successor creation and claim confirmation. It must support the current `composer-plus-btn`, both `Desktop Commander` and `Remote Desktop Commander` labels, and explicit post-attach verification.

## Versioned Windows launcher policy
The repo must carry the Windows Edge/CDP launch contract used in production: isolated profile, remote debugging, `--no-startup-window`, healthcheck via `/json/version`, and a cold-start assertion that `/json/list` has zero page targets before work begins. Installation/update scripts must deploy or verify this first-party launcher policy rather than relying on an unversioned local mutation.

## Invariants
- One authoritative generation per active run.
- Discovery never mutates chat authority.
- ACTIVE settled has at most one owned conversation target and zero scratch targets.
- Failed successor leaves no successor/scratch target after reconciliation.
- DONE notification is exactly-once; cleanup debt survives restarts until CLEAN.
- Idle browser cold-start has zero page targets.

## Scope
Implement the four P0 changes above, their regression tests, install/update wiring, and documentation. Do not merge, push, publish, or alter project runs while developing in the worktree.
