---
name: context-budget-manager
description: Use when a task is large, long-running, repository-heavy, multi-agent, reference-heavy, or likely to waste tokens by loading, rereading, or rediscovering unnecessary context.
---

# Context Budget Manager

Minimize token and context consumption without sacrificing correctness, safety, or required validation.

## Core principle
Treat context as a scarce working set. Load the smallest amount of information that is sufficient to make the next correct decision, while keeping authoritative execution state durable outside the model context.

Optimization must never override correctness, explicit user requirements, safety, or necessary verification.

## Context hierarchy
Prefer information in this order when it can answer the current question:
1. Current task and explicit user requirements.
2. Repo-local `AGENTS.md` and directly applicable instructions.
3. Current Git status/diff and the exact files being changed.
4. Relevant durable state from `docs/context/*`.
5. A task-specific skill.
6. Targeted external reference material.
7. Broader repository or web exploration only when the narrower sources are insufficient.

Do not preload every available skill, document, reference repository, log, or source.

## Tiered context loading
Use a three-level loading model inspired by OpenViking context engineering:
- **L0 — abstract:** one-line or very short relevance signal. Use it to decide whether a source, file, directory, skill, or reference deserves further attention.
- **L1 — overview:** structure, key points, interfaces, constraints, and likely relevance. Load this for planning when L0 indicates a match.
- **L2 — details:** full implementation, long documentation, logs, or source content. Load only when required to implement, resolve uncertainty, or validate.

Default progression is `L0 -> L1 -> L2`, not `L2 first`.

Skip directly to L2 only when the exact file/section is already known to be authoritative and necessary, such as the code being edited, a failing test, a specific configuration file, or an explicit user-provided artifact.

For directories and large repositories, first understand the highest relevant container or subsystem, then drill down into the smallest relevant child path. Preserve enough surrounding context to avoid isolated-snippet mistakes, but do not load unrelated siblings.

## Read discipline
- Read files by relevance, not by directory size.
- Prefer targeted search, exact sections, diffs, symbols, and recent relevant commits over full-file/full-repo rereads.
- Use L0/L1 summaries or metadata to reject irrelevant branches before reading full content.
- Do not reread unchanged material already established in the current working context unless verification requires it.
- When a large file is needed, inspect the relevant region first and expand only if dependencies or uncertainty demand it.
- Do not repeatedly fetch external references that have already yielded the pattern needed for the task.
- Stop research when sufficient evidence exists to choose and validate an implementation path.

## Skill and reference routing
Use routing rather than prompt accumulation:
- visual work -> `$visual-quality-director`; consult GPT Image 2 references only when relevant.
- LLM application architecture -> `$llm-app-pattern-library`; inspect only matching examples.
- cross-session or interruptible work -> `$durable-execution-memory`.
- other specialized work -> invoke only the smallest applicable skill set.

A reference library should be queried for the relevant pattern, not copied wholesale into context.

## Retrieval observability
When retrieval materially affects a decision, keep a concise operational trace of why a source was selected and what level was loaded. This does not mean narrating every read. Record enough to debug a wrong retrieval path without preserving verbose exploration.

If a result appears wrong or contradictory, inspect the retrieval path before broadening the search blindly.

## Execution strategy
1. Classify the task before exploring.
2. Identify the minimum authoritative inputs needed for the next action.
3. Start at the shallowest useful context level and deepen only where relevance is established.
4. Read/search only those inputs.
5. Form a concrete implementation hypothesis.
6. Inspect additional context only to resolve a named uncertainty or validate the hypothesis.
7. Execute the smallest coherent change.
8. Run proportionate validation.
9. Persist durable facts needed by future sessions, then allow transient exploration details to fall out of context.

## Token-waste anti-patterns
Avoid:
- reading the entire repository before a scoped change
- opening full source when an abstract/overview can first rule it out
- repeatedly summarizing the same state
- loading every skill for every task
- copying large external READMEs when one example or section is enough
- exploring many frameworks after an adequate project-compatible solution is known
- regenerating approved work instead of targeted correction
- repeating completed operations after context loss
- dumping verbose command output into durable documentation
- maintaining long narrative handoffs instead of operational checkpoints
- using multiple agents for work that is simpler and cheaper sequentially

## Multi-agent budget
Before dispatching parallel agents, verify that the work is genuinely independent and that parallelism is likely to save wall-clock time or improve quality enough to justify duplicated context/tool overhead.

Give each agent the narrowest sufficient brief and relevant files. Do not broadcast the entire project context by default.

## Durable compression
When context becomes large or a session may end:
- preserve verified state, decisions, blockers, validation results, and next safe step in durable repo context;
- omit exploratory dead ends unless they explain an important constraint or prevent repeated failure;
- use Git as the authoritative record of code changes;
- use `$durable-execution-memory` for continuity.

The goal is not to preserve every token. The goal is to preserve everything needed to continue correctly.

## External architectural reference
OpenViking (`https://github.com/volcengine/OpenViking`) is a reference for context-database ideas, especially tiered L0/L1/L2 loading, hierarchical retrieval, deterministic context organization, and observable retrieval trajectories.

Use these ideas selectively. Do not install OpenViking or introduce a context database merely because it exists. Its AGPLv3 licensing, operational complexity, privacy model, indexing cost, and project fit must be evaluated before any actual dependency or deployment decision.

## Escalation rule
Spend more context when uncertainty is material. Expand investigation when there is risk of data loss, security impact, irreversible mutation, architectural lock-in, production breakage, unclear user intent, or conflicting evidence.

Never save tokens by guessing.

## Completion check
Before finishing, ask operationally:
- Did I load anything irrelevant to the task?
- Did I escalate to L2 before establishing relevance?
- Did I repeatedly fetch or summarize information I already had?
- Could a future session resume from durable state without replaying this exploration?
- Did token optimization weaken validation or correctness?

If the last answer is yes, restore the missing validation or context before completion.
