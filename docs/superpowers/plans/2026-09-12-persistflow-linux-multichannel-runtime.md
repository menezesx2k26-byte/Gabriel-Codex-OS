# Persistflow Linux Multi-Channel Runtime Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Run the existing `persistd` protocol safely on Ubuntu 24.04 and supervise at least four independent `RUN_ID`s concurrently with bounded heavy-worker concurrency.

**Architecture:** Preserve `CONTROL.md`, leases, per-run locks, generation claims and `persist:<RUN_ID>` browser isolation. Refactor Windows-only health/browser startup behind platform adapters, add Linux browser hosting and packaging, then add a supervisor that launches one daemon per run while keeping controller liveness separate from heavy-worker admission.

**Tech Stack:** Node.js CommonJS, `node:test`, Chromium/Edge CDP, systemd, Bash, existing `ego-browser` transport.

**Spec:** `docs/superpowers/specs/2026-09-12-persistflow-multichannel-backup-failback-design.md`

## Global Constraints

- Azure host is Ubuntu 24.04 LTS x64, 4 vCPU, 16 GiB RAM.
- Existing durable takeover protocol is authoritative and must not be replaced.
- One authoritative controller per `RUN_ID`; no active-active behavior.
- Authentication challenges remain `AUTH_REQUIRED`; no MFA/CAPTCHA bypass.
- Controller count and heavy-worker concurrency are separate resource classes.
- Linux must not require local Windows Desktop Commander services.

---
### Task 1: Platform capability abstraction

**Files:**
- Create: `persistd/src/platform/capabilities.js`
- Create: `persistd/src/platform/windows.js`
- Create: `persistd/src/platform/linux.js`
- Modify: `persistd/src/remote-health.js`
- Test: `persistd/platform-capabilities.test.js`

**Interfaces:**
- Produces: `createPlatformCapabilities({ platform, env }) -> { desktopHealth, browserHost, supportsNativeWindowsUi }`
- `desktopHealth.preflight(state) -> Promise<{ok,browser,desktop,repaired}>`

- [ ] **Step 1: Write failing tests for win32 and linux selection**

```js
assert.equal(createPlatformCapabilities({ platform: 'linux' }).supportsNativeWindowsUi, false);
assert.equal(createPlatformCapabilities({ platform: 'win32' }).supportsNativeWindowsUi, true);
```

- [ ] **Step 2: Run `npm test -- platform-capabilities.test.js` from `persistd/`; verify failure.**
- [ ] **Step 3: Move Windows desktop probe/repair behind `windows.js`; implement Linux health as browser-only/local-host health with `desktop: 'NOT_APPLICABLE'`.**
- [ ] **Step 4: Run the focused test and full `npm test`; both must pass.**
- [ ] **Step 5: Commit `refactor(persistd): isolate platform capabilities`.**

### Task 2: Cross-platform browser host

**Files:**
- Create: `persistd/src/browser/browser-host.js`
- Create: `persistd/src/browser/linux-host.js`
- Modify: `persistd/src/browser/edge-host.js`
- Modify: `persistd/src/browser/ego-browser.js`
- Test: `persistd/browser-host.test.js`

**Interfaces:**
- Produces: `ensureBrowserHost({ platform, env, ...deps }) -> Promise<{endpoint, launched, profileDir}>`
- Windows delegates to current Edge behavior; Linux resolves Chromium/Edge binary and persistent profile.

- [ ] **Step 1: Add tests proving Linux no longer silently returns `{skipped:true}` and Windows behavior remains compatible.**
- [ ] **Step 2: Run focused tests; verify expected failure.**
- [ ] **Step 3: Implement Linux CDP startup with `--remote-debugging-port`, persistent `EGO_HOST_STATE_DIR`, `--no-first-run`, and optional `--headless=new`; never invent authentication state.**
- [ ] **Step 4: Update `runEgoScript()` to call `ensureBrowserHost()` instead of Windows-only `ensureEdgeBrowser()`.**
- [ ] **Step 5: Run `node --test browser-host.test.js edge-host.test.js chat-lifecycle.test.js`; then full suite.**
- [ ] **Step 6: Commit `feat(persistd): add linux browser host`.**
### Task 3: Linux install and systemd packaging

**Files:**
- Create: `scripts/install.sh`
- Create: `deploy/persistflow/persistd@.service`
- Create: `deploy/persistflow/persistd-supervisor.service`
- Create: `deploy/persistflow/persistd.env.example`
- Test: `persistd/linux-packaging.test.js`

**Interfaces:**
- `install.sh` installs repo-owned skills/runtime under `$HOME/.agents` without secrets.
- `persistd@<escaped-run-id>.service` runs `node ~/.agents/persistd/src/daemon.js --run-id <RUN_ID>`.

- [ ] **Step 1: Add tests that parse unit files and assert `Restart=on-failure`, no plaintext credentials, explicit state root, and per-run `--run-id`.**
- [ ] **Step 2: Run tests and verify failure because Linux packaging does not exist.**
- [ ] **Step 3: Implement idempotent `install.sh` mirroring the portable parts of `install.ps1`; exclude Windows ego host installation.**
- [ ] **Step 4: Add systemd units with `WorkingDirectory`, `EnvironmentFile=-%h/.config/persistflow/persistd.env`, restart policy, and journal logging.**
- [ ] **Step 5: Run packaging tests plus `bash -n scripts/install.sh`; full persistd suite must pass.**
- [ ] **Step 6: Commit `feat(persistflow): package persistd for systemd`.**

### Task 4: Per-run supervisor

**Files:**
- Create: `persistd/src/supervisor.js`
- Create: `persistd/src/run-inventory.js`
- Create: `persistd/supervisor.test.js`
- Modify: `persistd/package.json`

**Interfaces:**
- `discoverEligibleRuns(root) -> Array<{runId,status,controlPath}>`
- `Supervisor.reconcile() -> Promise<{started,stopped,kept}>`
- Child command: `node src/daemon.js --root <root> --run-id <runId>`.

- [ ] **Step 1: Write tests with four synthetic `CONTROL.md` directories; assert one child per eligible run and no duplicate child for the same `RUN_ID`.**
- [ ] **Step 2: Add a restart test: dead child is replaced; healthy child is retained; terminal run is stopped.**
- [ ] **Step 3: Run focused tests and verify failure.**
- [ ] **Step 4: Implement inventory using existing `readControl()` semantics and a process map keyed only by `RUN_ID`.**
- [ ] **Step 5: Implement graceful reconciliation; never mutate controller state from the supervisor itself.**
- [ ] **Step 6: Add `"supervisor": "node src/supervisor.js"` to `package.json`; run focused and full tests.**
- [ ] **Step 7: Commit `feat(persistd): supervise runs independently`.**
### Task 5: Heavy-worker admission control and observability

**Files:**
- Create: `persistd/src/resource-gate.js`
- Create: `persistd/resource-gate.test.js`
- Modify: `persistd/src/supervisor.js`
- Modify: `deploy/persistflow/persistd.env.example`

**Interfaces:**
- `createResourceGate({ maxHeavyWorkers })` exposes `tryAcquire(runId)`, `release(runId)`, `snapshot()`.
- Default `PERSISTD_MAX_HEAVY_WORKERS=2`; controller processes are never counted as heavy workers.

- [ ] **Step 1: Write tests proving four live controllers may exist while only two heavy permits are granted.**
- [ ] **Step 2: Add idempotency tests: repeated acquire for the same `RUN_ID` must not consume two permits; release restores capacity.**
- [ ] **Step 3: Run focused tests and verify failure.**
- [ ] **Step 4: Implement an in-memory gate plus supervisor metrics output containing controller count, heavy permits, CPU/memory/disk observations, and per-run child status.**
- [ ] **Step 5: Run focused and full tests; commit `feat(persistd): bound heavy worker concurrency`.**

### Task 6: Persistflow bootstrap and four-run acceptance

**Files:**
- Create: `deploy/persistflow/bootstrap-ubuntu.sh`
- Create: `deploy/persistflow/smoke-multichannel.sh`
- Create: `deploy/persistflow/README.md`

**Interfaces:**
- Bootstrap installs Node.js runtime prerequisites, Chromium/Edge, Git, required browser libraries, and repo service units; secrets are supplied out-of-band.
- Smoke script creates or consumes four synthetic non-production run fixtures and reports lease/collision results without claiming real project authority.

- [ ] **Step 1: Implement `bootstrap-ubuntu.sh` as an idempotent script with `set -euo pipefail`; validate with `bash -n`.**
- [ ] **Step 2: Document SSH hardening/Tailscale as a post-bootstrap gate; do not embed Azure IPs or secrets in Git.**
- [ ] **Step 3: Implement smoke checks for: service health, CDP endpoint, four supervised runs, unique task-space names, no duplicate child per run, and clean reboot recovery.**
- [ ] **Step 4: On a disposable Linux environment, run `npm test` and the smoke script before touching the Azure host.**
- [ ] **Step 5: Deploy to `persistflow`, start one non-production run, then four; capture `systemctl --no-pager --full status` and supervisor metrics.**
- [ ] **Step 6: Reboot the VM through Azure control, verify supervisor/runs recover consistently, and record the evidence in `deploy/persistflow/README.md`.**
- [ ] **Step 7: Commit `feat(persistflow): bootstrap linux multichannel runtime`.**

## Completion Gate

This plan is complete only when the full `persistd` test suite passes on Windows and Linux, `persistflow` supervises four independent runs without lease/browser collisions, heavy-worker admission remains bounded, and an Azure reboot returns the host to a consistent supervised state.

## Concrete Implementation Contracts

Use these shapes unless tests force a narrower equivalent.

```js
// persistd/src/platform/capabilities.js
function createPlatformCapabilities({ platform = process.platform, env = process.env } = {}) {
  if (platform === 'win32') return createWindowsCapabilities({ env });
  if (platform === 'linux') return createLinuxCapabilities({ env });
  throw new Error(`UNSUPPORTED_PLATFORM:${platform}`);
}
```

```js
// persistd/src/run-inventory.js
function discoverEligibleRuns(root) {
  return listControlFiles(root)
    .map(({ runId, controlPath }) => ({ runId, controlPath, state: readControl(controlPath) }))
    .filter(({ state }) => !['DONE', 'STALE', 'CANCELLED'].includes(String(state.STATUS || '').toUpperCase()));
}
```

```js
// persistd/src/resource-gate.js
function createResourceGate({ maxHeavyWorkers = 2 } = {}) {
  const owners = new Set();
  return {
    tryAcquire(runId) { if (owners.has(runId)) return true; if (owners.size >= maxHeavyWorkers) return false; owners.add(runId); return true; },
    release(runId) { owners.delete(runId); },
    snapshot() { return { maxHeavyWorkers, active: [...owners] }; },
  };
}
```

```ini
# deploy/persistflow/persistd@.service
[Service]
Type=simple
ExecStart=%h/.local/bin/node %h/.agents/persistd/src/daemon.js --root %h/.agents/continuations --run-id %i
Restart=on-failure
EnvironmentFile=-%h/.config/persistflow/persistd.env
```
