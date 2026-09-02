# Persistd Hardening P0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate forgotten browser cleanup, unverified successor closes, duplicated Commander setup, and unversioned Windows Edge launch behavior.

**Architecture:** Keep `CONTROL.md` authoritative. Treat browser cleanup as durable janitor work with read-after-write verification. Share one Commander bootstrap primitive across browser flows, and put the Windows Edge/CDP launch policy under first-party version control with cold-start tests.

**Tech Stack:** Node.js CommonJS, Node test runner, PowerShell installer/update scripts, Microsoft Edge CDP.

**Spec:** `docs/superpowers/specs/2026-09-02-persistd-hardening-p0-design.md`

## Global Constraints
- Discovery stays metadata-only.
- DONE notification remains exactly-once and independent of cosmetic rename.
- Cleanup never has a terminal forgotten state.
- No push/merge/publish during implementation.
- All behavior changes follow RED → GREEN → full-suite verification.

---

### Task 1: Durable cleanup debt

**Files:** `src/orchestrator.js`, `src/resolver.js`, `chat-lifecycle.test.js`

**Produces:** retryable cleanup state using `RETRY_SCHEDULED`, `BROWSER_CLEANUP_NEXT_AT`, and `CLEANUP_DEBT_SINCE`.

- [ ] Add failing tests proving 3+ cleanup failures never become forgotten and a DONE cleanup debt remains resolver-eligible.
- [ ] Run only those tests and confirm RED against current `GAVE_UP` behavior.
- [ ] Replace terminal `GAVE_UP` with bounded exponential backoff and durable retry scheduling.
- [ ] Ensure terminal notification is not resent while janitor retries cleanup.
- [ ] Run focused tests, then full `npm test`, then commit.
### Task 2: Verified failed-successor close

**Files:** `src/orchestrator.js`, `src/browser/ego-script.js`, `src/browser/ego-browser.js`, `chat-lifecycle.test.js`

**Produces:** failed-successor cleanup that trusts only `ok === true` plus read-after-write absence verification.

- [ ] Add failing test where `closeRunChat()` returns `{ok:false}` and prove fallback by exact `targetId` is required.
- [ ] Add failing test where close reports success but verification still sees the target; require cleanup debt/scratch reconciliation.
- [ ] Implement result checking and an exact target-presence verifier in the browser transport.
- [ ] Run focused tests, then full `npm test`, then commit.

### Task 3: Single Desktop Commander bootstrap

**Files:** `src/browser/conversation-script.js`, `src/browser/ego-script.js`, new `src/browser/commander-setup.js`, `chat-lifecycle.test.js`

**Produces:** one script builder used by successor creation and claim confirmation.

- [ ] Add failing tests requiring both flows to contain the same current selectors and accepted labels.
- [ ] Extract `buildCommanderSetupScript()` into `commander-setup.js` and import it from both call sites.
- [ ] Remove duplicated attachment logic from `ego-script.js` and preserve capability verification.
- [ ] Compile generated async scripts in tests, run focused tests, then full `npm test`, then commit.

### Task 4: Versioned Windows Edge/CDP host policy

**Files:** new `windows-host/` package or first-party launcher module, `scripts/install.ps1`, `scripts/update.ps1`, tests/docs as required.

**Produces:** repository-owned launcher policy with isolated profile, CDP endpoint healthcheck, `--no-startup-window`, and zero-page cold-start contract.

- [ ] Inspect the installed `ego-windows-host` package/license and choose a clean-room first-party wrapper unless copying is clearly licensed and necessary.
- [ ] Add RED tests asserting launch args contain `--no-startup-window`, never startup `about:blank`, and expose browser-level CDP readiness independently of page targets.
- [ ] Implement the minimal first-party host/launcher contract and wire install/update so runtime deployments receive the versioned behavior.
- [ ] Add a Windows cold-start smoke script that asserts `/json/version` succeeds and `/json/list` has zero `type=page` targets.
- [ ] Run package tests and smoke test in an isolated port/profile, then commit.

### Task 5: Final branch verification

**Files:** no new production behavior unless verification finds a defect.

- [ ] Run full `persistd` suite from a clean worktree.
- [ ] Run Windows host cold-start smoke on an isolated port/profile.
- [ ] Inspect `git diff`, grep for `BROWSER_CLEANUP_STATUS.*GAVE_UP`, duplicated Commander setup, and unsafe discovery-history merge usage.
- [ ] Verify no installed runtime or active project continuation was changed by worktree development.
- [ ] Record final commit list and review findings; do not push or merge without explicit user request.
