# Persistflow Canonical Design — Working Spec

Status: **WORKING / BRAINSTORMING IN PROGRESS**

This document is the canonical incremental design record for Persistflow. It is intentionally updated in checkpoints during brainstorming. A checkpoint records only decisions explicitly approved by the user. It is not an implementation plan and does not authorize implementation.

## Process contract

- Decisions are numbered monotonically as `D001`, `D002`, ...
- One design decision is discussed at a time.
- Every 10 approved decisions, this spec is updated and committed to Git.
- Existing Persistflow specs remain supporting material until reconciled into this canonical document.
- No implementation work is authorized by this document while brainstorming remains open.

## Checkpoint 1 — D001–D010

### D001 — Canonical scope

Persistflow is specified as the complete persistence/control platform, not only the `persistd` daemon.

The canonical scope includes:

- persistent conversation controller / `persistd` lifecycle;
- durable Azure/runtime execution;
- control and recovery channels;
- browser and authenticated-session persistence;
- backup, restore, failover and failback;
- host isolation and run ownership;
- observability and health state;
- multi-chat and multi-run operation.

### D002 — Source of authority

The durable local state on the currently authorized host is sovereign.

`CONTROL.md` plus the structured durable state derived from the authoritative journal decide generation, lease, claim, ownership and run authority.

GitHub, ChatGPT, browser state, Remote Desktop Commander and other external channels are transports or observers. None may independently promote a generation, transfer authority or override durable controller state.

### D003 — Run mobility model

Each `RUN_ID` is **single-writer**: exactly one host may be authoritative for that run at a time.

A run may move between hosts through an explicit failover/failback protocol using fencing, epoch and nonce semantics. Active-active authority for the same `RUN_ID` is prohibited.

### D004 — Automatic failover rule

Automatic failover is permitted only when the previous writer is provably fenced and cannot continue mutating authoritative state.

Examples of acceptable fencing evidence include VM deallocation, storage-level write fencing, or another mechanism that makes continued writes impossible.

A timeout alone is insufficient. If fencing cannot be proven, the system enters `FAILOVER_PENDING` and requires intervention rather than risking split-brain.

### D005 — Authoritative persistence model

Persistflow uses an **append-only journal plus materialized state**.

Every critical authoritative transition is appended to an immutable ordered event journal first. Human-readable `CONTROL.md` and machine-readable JSON/state files are materialized projections of that journal.

If a materialized view is lost or corrupted, it must be reconstructible from the journal plus a valid checkpoint.

### D006 — Journal integrity

The authoritative event journal uses a cryptographic hash chain and periodic checkpoints.

Each event contains, at minimum:

- monotonic sequence number;
- unique event identifier;
- timestamp;
- `RUN_ID`;
- authority epoch;
- generation;
- event type;
- event payload;
- previous-event hash;
- current-event hash.

Periodic checkpoints record the latest valid sequence/hash and a materialized snapshot so recovery does not require replaying the entire lifetime of the system.

### D007 — Corruption behavior

Detected corruption of authoritative state is fail-closed.

If journal integrity fails in a critical region, the run/system enters `STATE_INTEGRITY_FAULT`, freezes authoritative mutations, preserves forensic evidence and attempts reconstruction only from a validated checkpoint and trusted backup.

The system may resume authoritative writes only after integrity has been re-established.

### D008 — Backup architecture

Authoritative state uses three backup layers:

1. fast local checkpoint/snapshot;
2. independent copy on a separate disk or volume;
3. encrypted, versioned off-host backup.

Backup is part of the runtime design, not an optional administrative task. A backup is not considered trustworthy until automated restore validation has succeeded.

### D009 — Automatic restore rule

Automatic restore is allowed only when provenance is unambiguous.

The system may restore automatically when the selected checkpoint/backup is authenticated, its hash chain is valid to the restore point, and there is no possibility of another live writer holding authority.

If any of those conditions are ambiguous, the system enters `RESTORE_REVIEW_REQUIRED` rather than choosing a truth silently.

### D010 — Recovery SLOs

Persistflow uses failure-class-specific recovery objectives:

- Process/service crash: **RPO 0**, automatic recovery target **≤ 60 seconds**.
- Host reboot/failure with durable disk intact: **RPO 0**, authoritative service target **≤ 5 minutes**.
- Total host loss with storage/backup intact: **RPO ≤ 5 minutes**, recovery target **≤ 15 minutes**.
- Disaster including primary storage loss: restore from the latest validated off-host backup with **RPO ≤ 15 minutes** and **RTO ≤ 30 minutes**.
- Cases requiring human fencing or external authentication are outside the automatic recovery timer, but must transition immediately to an explicit intervention state.

## Open design areas

The following areas are intentionally unresolved and will be defined by subsequent numbered decisions: authority epoch/fencing protocol details, journal/checkpoint implementation boundaries, backup media/retention, browser/session strategy, control-channel hierarchy, multi-run scheduling and isolation, observability, self-healing policy, secrets/authentication, upgrade strategy, disaster drills, compatibility with existing Persistflow documents, and final acceptance criteria.
