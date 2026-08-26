---
name: durable-execution-memory
description: Use when work spans multiple steps, sessions, interruptions, branches, agents, or long-running implementation where execution facts must survive context loss.
---

# Durable Execution Memory

Treat conversation context as a working view, not as the authoritative history of execution.

## Principle
- Durable execution facts must live in repository state, not only in chat history.
- A shorter prompt or compacted context must never imply that earlier verified work stopped existing.
- Preserve evidence of what happened even when only a summary is needed for the next step.
- Recovery should reconstruct from durable artifacts and Git state before relying on memory.
- Persist reusable experience as structured assets, not as an undifferentiated chat dump.

## What counts as a durable execution fact
Record facts that would matter after an interruption, handoff, context compaction, or new agent/session, including:
- files changed
- commands or validations run and their outcomes
- commits, branches, PRs, deploys, and relevant SHAs/URLs
- important tool failures and their diagnosed causes
- decisions that constrain future work
- unresolved blockers
- current implementation state
- exact next safe step

Do not dump transient chain-of-thought or verbose narration. Record operational evidence and decisions only.

## Memory asset discipline
When useful, separate durable knowledge by role instead of loading everything everywhere:
- **facts/preferences/constraints**: stable information needed for future decisions
- **scenario/project context**: compact working context for a specific project or workflow
- **skills/workflows**: proven reusable procedures with triggers, steps, and validation rules
- **docs/wiki**: structured domain knowledge and operating documentation
- **code graph/impact knowledge**: symbols, callers/callees, dependencies, and change-impact relationships

Give an agent only the smallest relevant loadout for its role and task. Do not turn all durable memory into a global prompt.

## Repository authority
Use repo-local durable context when available:
- `docs/context/STATE.md` for current verified state
- `docs/context/DECISIONS.md` for durable decisions and rationale
- `docs/context/HANDOFF.md` for pause/resume instructions and next step
- `docs/context/ACCEPTANCE_CRITERIA.md` for completion gates
- Git history for authoritative code-change history

If the repository uses another established convention, preserve it instead of forcing these paths.

## Start / resume workflow
1. Inspect repository status and current branch.
2. Read repo-local `AGENTS.md` and relevant durable context files.
3. Inspect recent commits or diffs when needed to verify what actually changed.
4. Reconcile discrepancies in favor of observable repository state and validated evidence.
5. Identify the last verified checkpoint and resume from the next safe step.
6. Do not redo completed work merely because it is absent from the current model context.

## During execution
- After a meaningful checkpoint, update durable state if losing the checkpoint would cause expensive reconstruction.
- Keep updates concise and factual.
- Separate `completed`, `verified`, `blocked`, and `next` states.
- Store commands/results only when they materially prove or explain state.
- Promote a repeated successful procedure into a reusable skill or documented workflow when future sessions would otherwise rediscover it.
- Never store secrets, credentials, tokens, private keys, or sensitive payloads in durable context.

## Before pausing or handing off
Ensure the next session can answer, without chat history:
- What is the current branch/state?
- What is already done?
- What was actually verified?
- What failed and why?
- What remains?
- What is the exact next safe action?

Update `HANDOFF.md` when present. Update `STATE.md` when the durable project state changed.

## Recovery rules
- Repository evidence outranks conversational recollection.
- A commit is evidence of a change, not evidence that the change is correct; preserve validation status separately.
- Do not repeat destructive or costly operations until checking whether they already succeeded.
- If an interrupted action may have partially completed, inspect state before retrying.
- If state cannot be reconstructed confidently, mark the uncertainty explicitly instead of inventing continuity.

## External architectural references
Apache Maka (`https://github.com/apache/maka`) is a reference for durable execution records, recovery, and separation between saved execution history and the reduced context presented to a model.

TencentDB Agent Memory (`https://github.com/TencentCloud/TencentDB-Agent-Memory`) is a reference for turning conversations, docs, code knowledge, and proven workflows into separately managed memory assets; for layered memory retrieval; and for assigning different memory loadouts to different agents instead of broadcasting all context globally.

Use these for architectural inspiration when designing continuity or memory workflows, not as mandatory dependencies or sources of truth. Preserve project constraints and avoid importing infrastructure the task does not require.
