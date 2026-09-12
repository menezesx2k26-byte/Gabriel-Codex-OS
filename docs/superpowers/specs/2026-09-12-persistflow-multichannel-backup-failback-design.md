# Persistflow Multi-Channel Runtime, Backup and Failback Design

## Goal

Turn the Azure VM `persistflow` into the primary Linux host for multiple independent `persistd` workflows during the Azure credit window, while preserving durable recoverability through encrypted Google Drive backups and a verified failback path to Gabriel's Windows machine.

## Context

The current `persistd` implementation already provides per-`RUN_ID` durable state, `CONTROL.md`, generation/lease arbitration, nonce-bound takeover, and one browser task space per run. It currently multiplexes multiple runs through a single daemon tick loop and still contains Windows-specific browser/desktop health assumptions.

The Azure VM is Ubuntu 24.04 LTS, x64, `Standard_F4ads_v7` with 4 vCPUs and 16 GiB RAM. The design should use this machine as an agent host rather than merely as a remote shell.

The existing Conthabil Hydra design already defines a useful backup contract: portable bundles, manifests, SHA256 verification, restore drills, Google Drive upload verification, and restore-before-trust semantics. This design generalizes that pattern for `Gabriel-Codex-OS` and `persistd`.

## Core Decisions

1. Azure `persistflow` is the primary ACTIVE node during the credit period.
2. Gabriel's Windows machine is a prepared STANDBY node, never an equal active peer.
3. Google Drive is backup storage only; it is not a live filesystem, lease store, or consensus system.
4. Git remains authoritative for source code. Backup stores non-reproducible runtime state plus exact Git reconstruction metadata.
5. Planned failback is preferred over automatic distributed failover.
6. Split-brain prevention is mandatory: only one host may own mutation authority for a given `RUN_ID`.
7. Restore verification is part of backup success; successful upload alone is insufficient.

## Runtime Architecture

`persistflow` runs a Linux-native `persistd` runtime under `systemd`. A lightweight supervisor discovers eligible durable runs and manages one bounded worker process per active `RUN_ID` instead of relying exclusively on one global 15-second multiplexing loop.

Each run retains its existing isolation primitives:

- `~/.agents/continuations/<RUN_ID>/CONTROL.md`;
- per-run `.persistd.lock`;
- generation and lease state;
- nonce-bound claim protocol;
- dedicated `persist:<RUN_ID>` browser task space;
- independent lifecycle and terminal notification state.

The supervisor enforces global concurrency limits so many workflows may remain alive while only a bounded number perform CPU-heavy work simultaneously. Controller liveness and worker execution are separate resource classes.

The Linux runtime must preserve the existing durable controller protocol rather than replacing it with a new consensus model. Platform-specific operations are exposed through capability adapters: Linux-local operations on Azure and optional Remote Desktop Commander / Dev-Orquestra calls to the Windows machine when a workflow genuinely requires Windows-native UI or files.

## Browser Runtime

The current Windows-specific Edge launcher is refactored behind a browser-host interface. Linux uses Chromium/Edge with a persistent profile and structured DOM automation. A virtual display may be used when an authenticated interactive browser session is required.

Authentication is bootstrapped explicitly and never bypassed. CAPTCHA, MFA, expired login, or account challenges become `AUTH_REQUIRED` exactly as they do today.

Browser state that is both portable and safe to restore may be included in encrypted operational backups. OS-bound credential material is never assumed portable across Linux and Windows; the fallback procedure may require re-authentication for services whose session encryption cannot be safely migrated.

## Backup Model

Backups are divided into three classes.

### Critical state

Back up frequently:

- `~/.agents/continuations/**` including every `CONTROL.md`;
- durable handoffs and controller metadata;
- installed `persistd` configuration and version metadata;
- run inventory and host-role metadata;
- exact repository reconstruction metadata: remote URL, branch, HEAD SHA, worktree path, and dirty-state warning;
- manifests required to reproduce worker/tool configuration.

### Operational state

Back up less frequently:

- portable browser/session state where supported;
- tool and worker configuration;
- runtime manifests;
- service definitions and generated host configuration;
- non-secret local data that materially shortens restore time.

### Disposable state

Do not back up:

- `node_modules`, package caches, compiler caches, and build output;
- temporary browser/download files;
- disposable containers and images that can be rebuilt;
- NVMe scratch data;
- clean Git working trees already recoverable from their remote repositories.

## Backup Transport and Encryption

The preferred implementation is incremental, deduplicated, encrypted backup using `restic`, with `rclone` providing Google Drive transport where needed. The repository password/key must never be stored in the same Google Drive location as the encrypted backup repository.

Secrets are not committed to Git. A restore bundle may contain a `secrets-manifest.json` describing required secret names and sources, but not plaintext secret values unless they are inside an explicitly encrypted secret payload protected by a key stored outside Google Drive.

Each backup operation produces machine-readable verification metadata including:

- snapshot identifier and creation time;
- source host identity and role;
- schema/version number;
- `persistd` version or Git SHA;
- included `RUN_ID`s and their generations;
- repository reconstruction metadata;
- content/checksum verification result;
- restore-drill status where applicable.

The backup health contract is:

```text
BACKUP_CREATED=true
CHECKSUMS_VERIFIED=true
RESTORABLE=true
RESTORE_DRILL_PASSED=true
DRIVE_UPLOAD_VERIFIED=true
```

A planned migration additionally requires `MIGRATION_BUNDLE_READY=true`.

## Planned Failback Protocol

The Azure-to-Windows transition is a controlled authority transfer, not a file sync.

1. Mark the Azure host as `QUIESCING` and stop accepting new mutating work.
2. Allow in-flight reversible work to finish or checkpoint safely.
3. Produce the final migration snapshot and bundle.
4. Verify checksums and perform a disposable restore drill.
5. Verify the uploaded Drive object and record its immutable snapshot/bundle identifier.
6. Mark the Azure host `RELINQUISHED` for the affected `RUN_ID`s and stop their controller processes.
7. Restore the bundle into a staging area on Windows.
8. Verify schema, checksums, Git SHAs, run generations, and required tooling.
9. Promote the Windows host from `STANDBY` to `ACTIVE` and start controllers only after the previous lease is provably inactive.
10. Run smoke checks before Azure is considered safe to deallocate/delete.

The procedure must be idempotent up to the authority-transfer boundary. A failure before relinquish keeps Azure authoritative. A failure after relinquish never silently restarts Azure authority; recovery proceeds from durable transfer evidence.

## Emergency Recovery

Emergency recovery is available but is intentionally not an automatic election system. Windows may perform disaster-recovery takeover only when the latest durable backup is available, the prior lease has expired, and the operator explicitly authorizes disaster recovery.

Emergency recovery must display the recovery point timestamp and possible data-loss window before claim. It must not infer that Azure is dead merely from one failed network probe.

## Split-Brain Invariants

- At most one host may mutate a `RUN_ID` at a time.
- Google Drive never grants controller authority.
- Backup restoration never implies activation.
- Host activation requires durable claim evidence after previous authority is relinquished or expired under disaster-recovery rules.
- Restored stale `CONTROL.md` state must not overwrite a newer known generation.

## Observability and Resource Control

The host supervisor records controller liveness, worker state, backup age, restore-drill age, CPU, memory, disk pressure, browser health, and per-run lifecycle state. Resource pressure may pause new heavy workers but must not corrupt controller state.

Default concurrency should favor many live controllers with few simultaneous CPU-heavy jobs. Exact limits are configuration, not protocol, and may be tuned from observed VM behavior.

Backup failure is surfaced prominently but does not by itself stop unrelated controller work. A stale backup beyond the configured recovery objective blocks planned failback readiness.

## Restore Drills

A restore drill must run into a disposable staging directory without claiming controller authority. It verifies archive/repository readability, manifest schema, checksums, required files, Git reconstruction metadata, and parseability of every restored `CONTROL.md`.

Before the Azure credit period ends, at least one full Windows-side restore drill must pass using only the documented restore inputs plus Git remotes. The drill must prove that no undocumented Azure-local file is required for recovery.

## Security Boundaries

SSH administration should move away from world-open access after bootstrap, preferably through Tailscale or source-IP-restricted rules. Long-lived credentials must not be embedded in repository files, shell history, or unit files when a protected environment/credential store is available.

Google Drive contains only encrypted backup payloads plus non-sensitive verification metadata. Backup encryption keys remain outside Drive and must have at least one independent recovery copy controlled by Gabriel.

## Non-Goals

- Building a general distributed consensus system.
- Active-active `persistd` across Azure and Windows.
- Backing up reproducible caches or build products.
- Treating Google Drive as a mounted live state directory.
- Guaranteeing cross-OS portability of every third-party browser session.
- Automatic bypass of MFA, CAPTCHA, login challenges, or platform safeguards.

## Rollout Phases

Phase 1 makes the current controller platform-neutral enough to run safely on Ubuntu: isolate Windows-only health/browser behavior behind capability adapters, add Linux service packaging, and prove one persisted run end-to-end.

Phase 2 adds multi-channel supervision with bounded concurrency while preserving the existing per-run durable protocol. Scale tests increase active `RUN_ID` count gradually rather than assuming a fixed maximum.

Phase 3 adds encrypted incremental Drive backup, manifests, verification, and disposable restore drills.

Phase 4 prepares Windows standby restore tooling and performs a full no-authority restore rehearsal.

Phase 5 executes the planned Azure-to-Windows failback before credit exhaustion, verifies Windows ACTIVE operation, and only then permits Azure deallocation.

## Acceptance Criteria

The design is complete only when all of the following are demonstrated:

1. `persistd` can run on `persistflow` without requiring Windows-only local services.
2. At least four independent persisted runs can remain supervised concurrently without lease corruption or cross-run browser/state collisions.
3. Heavy worker concurrency is bounded independently from controller count.
4. Rebooting the Azure VM returns the supervisor and eligible runs to a consistent recoverable state.
5. Critical state is backed up encrypted to Google Drive on schedule.
6. A backup is never marked healthy solely because upload succeeded.
7. A disposable Linux restore drill passes from a Drive-backed recovery point.
8. A disposable Windows restore drill passes before planned failback.
9. A planned failback proves Azure relinquishment before Windows activation.
10. No test produces two simultaneous authoritative controllers for the same `RUN_ID`.
11. Git metadata is sufficient to reconstruct repository/worktree state or explicitly flags uncommitted data that requires backup.
12. Secrets are absent from Git and plaintext Drive payloads.
13. The final Windows host can continue existing persisted workflows after Azure is deallocated.

## Decision Summary

Use Azure as the temporary primary agent host, Google Drive as encrypted recovery storage, Git as source-code authority, and Gabriel's Windows machine as a tested standby destination. Extend the existing `persistd` protocol rather than replacing it, and make authority transfer explicit, verifiable, and one-way at the failback boundary.
