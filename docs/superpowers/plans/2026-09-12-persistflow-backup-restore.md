# Persistflow Backup and Restore Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Produce encrypted, incremental recovery points for `persistd` critical state and prove they restore in disposable Linux and Windows staging environments.

**Architecture:** Build a repo-owned manifest/inventory layer, then use restic for encrypted deduplicated snapshots and rclone as the Google Drive transport. Upload success alone never marks a recovery point healthy.

**Tech Stack:** Node.js, Bash, PowerShell, restic, rclone, SHA-256.

**Spec:** `docs/superpowers/specs/2026-09-12-persistflow-multichannel-backup-failback-design.md`

## Global Constraints

- Google Drive is backup storage only, never a live state directory or authority source.
- Git is authoritative for clean source trees; backup records exact reconstruction metadata.
- Encryption material is stored outside the Drive backup location and outside Git.
- Reproducible caches, builds and scratch are excluded.
- Backup success requires verification and restore evidence.

---
### Task 1: Recovery manifest and repository inventory

**Files:**
- Create: `persistd/src/recovery/manifest.js`
- Create: `persistd/src/recovery/git-inventory.js`
- Test: `persistd/recovery-manifest.test.js`

**Interfaces:**
- `buildRecoveryManifest({ continuationsRoot, reposRoot, host }) -> Promise<Manifest>`
- `inspectRepository(path) -> {path,remote,branch,head,dirty,untracked}`

- [ ] **Step 1: Write a failing test with a temporary Git repo and temporary `CONTROL.md`.**

```js
assert.equal(manifest.schemaVersion, 1);
assert.equal(manifest.runs[0].generation, 3);
assert.equal(manifest.repositories[0].head, headSha);
```

- [ ] **Step 2: Run `node --test recovery-manifest.test.js`; verify failure.**
- [ ] **Step 3: Implement manifest generation with stable JSON keys, UTC timestamp, and controller parsing through existing `readControl()`.**

```js
return { schemaVersion: 1, createdAt: new Date().toISOString(), host, runs, repositories };
```

- [ ] **Step 4: Mark dirty/untracked repositories with `requiresPayloadBackup: true`; clean repositories store reconstruction metadata only.**
- [ ] **Step 5: Run focused and full tests; commit `feat(recovery): inventory durable state and git reconstruction`.**
### Task 2: Backup payload selection and verification metadata

**Files:**
- Create: `persistd/src/recovery/payload.js`
- Create: `persistd/src/recovery/health.js`
- Test: `persistd/recovery-payload.test.js`

**Interfaces:**
- `buildPayloadPlan(manifest) -> { includePaths, excludePatterns, repositoryPayloads }`
- `evaluateBackupHealth(evidence) -> { healthy, reasons }`

- [ ] **Step 1: Add tests proving continuations and required dirty-repo files are included while `node_modules`, caches, builds and scratch are excluded.**

```js
assert.ok(plan.includePaths.some((p) => p.endsWith('.agents/continuations')));
assert.ok(plan.excludePatterns.includes('**/node_modules/**'));
```

- [ ] **Step 2: Add a health-contract test where upload is true but restore is false; expected `healthy === false`.**
- [ ] **Step 3: Implement the fixed health contract:**

```js
const required = ['BACKUP_CREATED','CHECKSUMS_VERIFIED','RESTORABLE','RESTORE_DRILL_PASSED','DRIVE_UPLOAD_VERIFIED'];
return { healthy: required.every((k) => evidence[k] === true), reasons: required.filter((k) => evidence[k] !== true) };
```

- [ ] **Step 4: Run focused/full tests and commit `feat(recovery): define backup payload and health contract`.**

### Task 3: Restic and Drive transport wrappers

**Files:**
- Create: `deploy/persistflow/backup.sh`
- Create: `deploy/persistflow/backup.env.example`
- Create: `deploy/persistflow/restic-excludes.txt`
- Test: `persistd/recovery-scripts.test.js`

**Interfaces:**
- `backup.sh --manifest <path> --output <result.json>` exits nonzero on snapshot/check failure.
- Configuration is read from protected environment files; repository files contain no credentials.

- [ ] **Step 1: Add script-contract tests for `set -euo pipefail`, required env validation, exclude-file use, and JSON result output.**
- [ ] **Step 2: Implement the backup sequence:**

```bash
restic backup --exclude-file "$EXCLUDE_FILE" --json "$STATE_ROOT" >"$WORK_DIR/restic.json"
restic check --read-data-subset="$RESTIC_CHECK_SUBSET"
rclone copy "$RESTIC_REPOSITORY_LOCAL" "$RCLONE_REMOTE"
```

- [ ] **Step 3: Emit snapshot id, manifest hash and upload verification into the result JSON; never print protected environment values.**
- [ ] **Step 4: Run `bash -n`, script-contract tests and full Node tests; commit `feat(recovery): add encrypted drive backup transport`.**
### Task 4: Disposable restore drill

**Files:**
- Create: `deploy/persistflow/restore-drill.sh`
- Create: `persistd/src/recovery/verify-restore.js`
- Test: `persistd/recovery-restore.test.js`

**Interfaces:**
- `verifyRestore({ stagingRoot, manifestPath }) -> { ok, checks[] }`
- Restore drill never starts controller services and never claims authority.

- [ ] **Step 1: Add tests that reject checksum mismatch, missing `CONTROL.md`, unparsable controller state, and mismatched Git metadata.**
- [ ] **Step 2: Implement staged restore:**

```bash
mkdir -p "$STAGING_ROOT"
restic restore "$SNAPSHOT_ID" --target "$STAGING_ROOT"
node "$VERIFY_SCRIPT" --staging "$STAGING_ROOT" --manifest "$MANIFEST"
```

- [ ] **Step 3: `verify-restore.js` must use existing `readControl()` for every restored controller file and compare manifest SHA/branch metadata without activating anything.**
- [ ] **Step 4: Run synthetic corrupt/good restore fixtures; full test suite must pass.**
- [ ] **Step 5: Commit `feat(recovery): add disposable restore verification`.**

### Task 5: Scheduled backup and stale-backup readiness gate

**Files:**
- Create: `deploy/persistflow/persistflow-backup.service`
- Create: `deploy/persistflow/persistflow-backup.timer`
- Create: `persistd/src/recovery/readiness.js`
- Test: `persistd/recovery-readiness.test.js`

**Interfaces:**
- `assessRecoveryReadiness({ latestBackup, latestDrill, now, maxBackupAgeMs, maxDrillAgeMs })` returns `READY` or explicit blockers.

- [ ] **Step 1: Write tests where stale backup or stale restore drill blocks planned migration readiness.**
- [ ] **Step 2: Implement pure readiness logic; no systemd calls inside Node code.**

```js
if (now - latestBackup > maxBackupAgeMs) blockers.push('BACKUP_STALE');
if (now - latestDrill > maxDrillAgeMs) blockers.push('RESTORE_DRILL_STALE');
```

- [ ] **Step 3: Add a systemd timer that invokes the repo-owned backup script and records results under `$HOME/.agents/recovery/results/`.**
- [ ] **Step 4: Run unit tests and `systemd-analyze verify` against both unit files on Linux.**
- [ ] **Step 5: Commit `feat(recovery): schedule verified backups`.**
### Task 6: Windows-side restore rehearsal

**Files:**
- Create: `deploy/persistflow/restore-windows.ps1`
- Create: `deploy/persistflow/RESTORE.md`
- Test: `persistd/recovery-windows-script.test.js`

**Interfaces:**
- `restore-windows.ps1 -Snapshot <id> -StagingRoot <path> -Manifest <path>` restores and verifies only; it never starts persistd.

- [ ] **Step 1: Add static-contract tests that the script requires a staging directory, invokes verification, and contains no controller start/claim command.**
- [ ] **Step 2: Implement the same restore contract as Linux using native path handling and the shared Node verifier.**

```powershell
& restic restore $Snapshot --target $StagingRoot
if ($LASTEXITCODE -ne 0) { throw 'RESTORE_FAILED' }
& node $VerifyScript --staging $StagingRoot --manifest $Manifest
```

- [ ] **Step 3: Write `RESTORE.md` with exact prerequisites, environment inputs, staging-only rule, Git reconstruction step, and expected success markers.**
- [ ] **Step 4: Run a full Windows staging restore using only the Drive recovery point plus documented Git remotes; record the result JSON.**
- [ ] **Step 5: Commit `feat(recovery): prove windows staging restore`.**

## Completion Gate

This plan is complete only when a scheduled encrypted recovery point reaches all five health flags, a disposable Linux restore passes, a disposable Windows restore passes without claiming authority, dirty repository state is either captured or explicitly blocks healthy backup status, and no protected values appear in Git or plaintext Drive metadata.
