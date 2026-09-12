# Persistflow Azure-to-Windows Failback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Transfer `persistd` authority from Azure to Gabriel's Windows machine without split-brain and with explicit durable evidence before Azure is deallocated.

**Architecture:** Add a host-role journal and a migration transaction around the existing per-run lease/claim protocol. Planned failback is quiesce -> verified final recovery point -> Azure relinquish -> Windows staging restore -> lease/generation verification -> Windows claim -> smoke checks; emergency recovery is a separate explicitly authorized path.

**Tech Stack:** Node.js CommonJS, `node:test`, Bash, PowerShell, existing `CONTROL.md` and lease/claim modules.

**Spec:** `docs/superpowers/specs/2026-09-12-persistflow-multichannel-backup-failback-design.md`

## Global Constraints

- At most one host may mutate a `RUN_ID` at any time.
- Backup restoration never implies activation.
- Failure before relinquish leaves Azure authoritative.
- Failure after relinquish never silently restores Azure authority.
- Emergency recovery requires explicit operator authorization and an expired prior lease.
- A restored stale generation must never overwrite a newer known generation.

---

### Task 1: Durable host-role and transfer journal

**Files:**
- Create: `persistd/src/failback/host-role.js`
- Create: `persistd/src/failback/transfer-journal.js`
- Test: `persistd/failback-journal.test.js`

**Interfaces:**
- Host roles: `ACTIVE | QUIESCING | RELINQUISHED | STANDBY | RECOVERY_PENDING`.
- `beginTransfer({ sourceHost, targetHost, runIds, recoveryPoint }) -> TransferRecord`.
- Journal writes atomically beside recovery metadata, not inside Git source trees.

- [ ] **Step 1: Write tests for legal transitions and reject `RELINQUISHED -> ACTIVE` without an explicit recovery transaction.**

```js
assert.throws(() => transition('RELINQUISHED', 'ACTIVE'), /RECOVERY_REQUIRED/);
assert.equal(transition('ACTIVE', 'QUIESCING'), 'QUIESCING');
```

- [ ] **Step 2: Run focused test; verify failure.**
- [ ] **Step 3: Implement an append-only transaction record with id, timestamps, source/target hosts, affected run IDs, recovery point id and current phase.**
- [ ] **Step 4: Use atomic temp-file rename semantics matching existing `CONTROL.md` durability patterns.**
- [ ] **Step 5: Run focused/full tests; commit `feat(failback): add durable host transfer journal`.**
### Task 2: Azure quiesce and relinquish commands

**Files:**
- Create: `persistd/src/failback/source.js`
- Create: `persistd/failback-source.test.js`
- Create: `deploy/persistflow/failback-source.sh`

**Interfaces:**
- `quiesceRuns({ runIds, continuationsRoot }) -> { ready, blockers }`
- `relinquishRuns({ runIds, transferId, now }) -> { relinquishedAt }`

- [ ] **Step 1: Write tests where ACTIVE runs move to quiescing metadata but retain authority until all blockers clear.**
- [ ] **Step 2: Add a failure test: if final recovery point is not healthy, relinquish must throw `RECOVERY_POINT_NOT_READY`.**
- [ ] **Step 3: Implement source-side gates:**

```js
if (!recovery.healthy || !recovery.MIGRATION_BUNDLE_READY) throw new Error('RECOVERY_POINT_NOT_READY');
if (inFlightMutations.length) return { ready: false, blockers: ['IN_FLIGHT_MUTATIONS'] };
```

- [ ] **Step 4: `failback-source.sh` invokes quiesce, final backup/drill, then relinquish; each phase writes to the transfer journal before proceeding.**
- [ ] **Step 5: Run focused/full tests and `bash -n`; commit `feat(failback): quiesce and relinquish azure authority`.**

### Task 3: Windows target validation and claim gate

**Files:**
- Create: `persistd/src/failback/target.js`
- Create: `persistd/failback-target.test.js`
- Create: `deploy/persistflow/failback-target.ps1`

**Interfaces:**
- `validateTarget({ transfer, restoredManifest, liveStates, now }) -> { ready, blockers }`
- `activateTarget({ transferId, runIds })` starts controllers only after validation succeeds.

- [ ] **Step 1: Write tests rejecting activation when source role is not `RELINQUISHED`, lease is still live, generation is older than known state, or restore verification failed.**

```js
assert.deepEqual(validateTarget(inputWithLiveLease).blockers, ['SOURCE_LEASE_LIVE']);
```

- [ ] **Step 2: Implement the validation gate using existing lease parsing and claim-generation helpers; do not create a parallel claim algorithm.**
- [ ] **Step 3: Implement `failback-target.ps1` to restore into staging, call shared validation, copy verified state into the final location, and then start the selected per-run services/processes.**
- [ ] **Step 4: Run focused/full tests; commit `feat(failback): gate windows activation on durable relinquish`.**
### Task 4: Explicit emergency-recovery path

**Files:**
- Create: `persistd/src/failback/emergency.js`
- Create: `persistd/failback-emergency.test.js`
- Modify: `deploy/persistflow/failback-target.ps1`

**Interfaces:**
- `assessEmergencyRecovery({ latestRecoveryPoint, sourceLease, operatorAuthorized, now }) -> { allowed, blockers, dataLossWindowMs }`.

- [ ] **Step 1: Write tests proving one failed network probe is irrelevant, operator authorization is mandatory, and a non-expired source lease blocks recovery.**
- [ ] **Step 2: Implement pure assessment logic:**

```js
if (!operatorAuthorized) blockers.push('OPERATOR_AUTH_REQUIRED');
if (sourceLease && sourceLease.expiresAt > now) blockers.push('SOURCE_LEASE_LIVE');
const dataLossWindowMs = now - latestRecoveryPoint.createdAt;
```

- [ ] **Step 3: Make the PowerShell target script require an explicit `-DisasterRecovery` switch plus a typed confirmation token derived from the transfer/recovery point id.**
- [ ] **Step 4: Print the recovery timestamp and estimated data-loss window before any claim operation.**
- [ ] **Step 5: Run focused/full tests; commit `feat(failback): add explicit disaster recovery gate`.**

### Task 5: End-to-end no-authority rehearsal

**Files:**
- Create: `deploy/persistflow/failback-rehearsal.md`
- Create: `deploy/persistflow/smoke-failback.ps1`

**Interfaces:**
- Rehearsal uses synthetic/non-production `RUN_ID`s and must never deactivate real Azure authority.

- [ ] **Step 1: Prepare four synthetic runs with distinct generations and verify the Azure supervisor owns them.**
- [ ] **Step 2: Create a healthy recovery point and restore it to Windows staging without activation.**
- [ ] **Step 3: Run target validation in dry-run mode; expected output contains `READY_FOR_TRANSFER=true` and no controller process starts.**
- [ ] **Step 4: Exercise negative fixtures for live source lease, stale generation, damaged manifest, and failed restore evidence.**
- [ ] **Step 5: Record exact commands/results in `failback-rehearsal.md`; commit `test(failback): rehearse azure to windows transfer`.**

### Task 6: Planned credit-expiry cutover runner

**Files:**
- Create: `deploy/persistflow/failback-planned.sh`
- Create: `deploy/persistflow/persistflow-failback.service`
- Create: `deploy/persistflow/persistflow-failback.timer.example`
- Modify: `deploy/persistflow/README.md`

**Interfaces:**
- Planned runner executes source phases only up to durable relinquish and emits the exact transfer id needed by Windows target tooling.
- Timer example has no hard-coded subscription, credential, or user-specific deadline; deployment writes the real schedule outside Git.

- [ ] **Step 1: Implement runner with explicit phases `preflight`, `quiesce`, `final-backup`, `verify`, `relinquish`; make reruns resume from journal state.**

```bash
case "$phase" in
  preflight|quiesce|final-backup|verify|relinquish) ;;
  *) echo "INVALID_PHASE" >&2; exit 2 ;;
esac
```

- [ ] **Step 2: Add systemd unit/timer examples and verify them with `systemd-analyze verify`.**
- [ ] **Step 3: Document the production rule: schedule cutover with safety margin before credit exhaustion; Azure deallocation is permitted only after Windows ACTIVE smoke checks pass.**
- [ ] **Step 4: Perform one full synthetic transfer where Azure relinquishes synthetic runs, Windows claims them, and assertions prove no overlap in authoritative lease intervals.**
- [ ] **Step 5: Commit `feat(failback): automate planned credit-expiry transfer`.**

## Completion Gate

This plan is complete only when a rehearsed transfer proves Azure relinquishment precedes Windows activation, stale generations and live leases block target claim, emergency takeover requires explicit authorization, the transaction is resumable/idempotent, and production Azure deallocation remains gated on verified Windows ACTIVE smoke checks.
