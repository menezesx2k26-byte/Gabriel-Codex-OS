---
name: reuse-first-router
description: Use before designing or implementing non-trivial functionality when an existing library, tool, CLI, service, template, skill, protocol, open-source project, or repository utility may already solve most of the requirement.
---

# Reuse-First Router

Avoid spending tokens, time, and maintenance effort reinventing solved problems.

## Prime directive
Before custom implementation, perform the cheapest useful search for an existing solution. Reuse or adapt when a mature option satisfies the material requirements with lower total complexity than building from scratch.

Do not browse broadly by default. Search progressively and stop as soon as a clearly adequate candidate is found.

## Local vendor cache
The Gabriel Codex OS installer maintains a shallow local cache under `~/.agents/vendor/` for high-value reuse references. Prefer targeted search there before remote discovery when relevant.

Current vendor references:
- `awesome-harness-engineering` — harness architecture, memory, MCP, permissions, observability, evals, orchestration.
- `claude-skills` — large cross-agent skill catalog with Codex-compatible material.
- `agentmemory` — persistent-memory implementation/reference for coding agents.
- `andrej-karpathy-skills` — concise coding-agent behavior rules and failure-avoidance guidance.
- `ponytail` — YAGNI/reuse/minimal-code patterns for coding agents.
- `tencentdb-agent-memory` — layered memory assets, skills, wiki/code graph, and per-agent context loadouts.
- `ego-lite` — token-efficient browser automation patterns and an optional agent browser integration.

These repositories are reference/candidate sources. Do not load them wholesale into context and do not activate third-party runtime code automatically.

## Search ladder
Use this order unless the task gives a stronger source of truth:

### Tier 0 — Already here
Check only what is cheap and local:
- existing project code and utilities
- current dependencies
- repo-local scripts and CLIs
- repo-local `AGENTS.md` and skills
- durable decisions documenting previously chosen tools

If an adequate solution exists here, use it and stop.

### Tier 1 — Known toolbox
Check the smallest relevant known reference, preferably in `~/.agents/vendor/`, not all of them:
- LLM apps / agents / RAG / multi-agent -> `awesome-llm-apps`
- GPT Image 2 / image generation -> `awesome-gpt-image-2`
- context engineering / hierarchical loading -> OpenViking patterns
- durable execution / recovery -> Apache Maka patterns
- harness / orchestration / agent runtime -> `awesome-harness-engineering`
- reusable agent skills -> `claude-skills`
- coding-agent memory -> `agentmemory`
- team memory assets / code knowledge / per-agent loadouts -> `tencentdb-agent-memory`
- coding-behavior pitfalls -> `andrej-karpathy-skills`
- YAGNI / smallest-code solution -> `ponytail`
- browser automation / web workflows -> `ego-lite` when the environment and constraints fit

Inspect indexes, README sections, names, or targeted search results first. Do not ingest the whole repository.

### Tier 2 — Mature ecosystem
Only if Tier 0–1 do not solve the problem, search the relevant ecosystem for established packages, CLIs, SDKs, protocols, or services.

Prefer candidates with:
- active maintenance
- clear documentation
- compatible license
- reasonable security posture
- strong fit with the existing stack
- low integration and operational overhead

### Tier 3 — Broad discovery
Use broad GitHub/web discovery only when a meaningful capability gap remains. Search by the capability needed, not vague technology buzzwords.

Stop after finding a small shortlist of credible candidates. Do not spend tokens exhaustively ranking dozens of near-equivalent tools unless the choice is high-impact.

### Tier 4 — Custom build
Build custom only when:
- no existing option covers the material requirement;
- available options violate license, security, privacy, deployment, performance, or cost constraints;
- integration complexity exceeds the custom implementation;
- the needed behavior is genuinely project-specific;
- or the custom solution is materially smaller and easier to maintain.

When custom-building despite a plausible reusable candidate, record the concrete reason.

## Browser automation discipline
For non-trivial browser workflows, prefer a solution that reduces observation/action round-trips. If a browser tool supports composing several deterministic page actions into one script or bounded execution unit, prefer that over a long sequence of tiny tool calls when it remains observable and safe.

Evaluate browser reuse candidates for:
- platform support
- authentication/session handling
- isolation between user and agent browsing
- snapshot/DOM quality
- token/tool-call overhead
- ability to compose multi-step actions
- privacy and local-data handling
- recoverability when a step fails

`ego-lite` is a useful reference/candidate for this pattern, but its browser runtime is currently macOS-focused. Do not assume it can be activated on unsupported platforms; reuse the execution pattern or select another browser tool when necessary.

## Candidate triage
For each serious reuse candidate, answer only what matters:
- What percentage of the requirement does it cover?
- What adaptation is still needed?
- License compatible?
- Maintained enough?
- Security/privacy acceptable?
- Fits current stack/deployment?
- New operational burden?
- Does it reduce code, tokens, maintenance, or failure surface?

Reject quickly when a hard constraint fails. Do not perform deep analysis of obviously unsuitable candidates.

## Token discipline
- Search indexes before full docs.
- Read README/overview before source.
- Read source only for the integration path or uncertainty that affects adoption.
- Search the local vendor cache before repeating remote fetches.
- Do not inspect entire repositories merely to learn what they do.
- Do not compare more than a few credible options unless the decision has high switching cost.
- Cache durable decisions in `docs/context/DECISIONS.md` when future sessions would otherwise repeat the same comparison.
- Reuse prior evaluations if the tool and requirements have not materially changed.

## Composition over adoption
A "Swiss-army knife" does not have to be adopted wholesale. Prefer the cheapest safe reuse level:
1. existing feature as-is
2. configuration
3. adapter/wrapper
4. selective module or pattern
5. fork only when justified
6. custom implementation as last resort

Avoid adding a large framework to solve a tiny isolated problem.

## Interaction with other skills
- Use `$context-budget-manager` to keep discovery narrow.
- Use `$llm-app-pattern-library` for LLM application patterns.
- Use `$visual-quality-director` for visual tasks.
- Use `$durable-execution-memory` to persist important adoption decisions and avoid repeating research across sessions.

## Completion rule
Before implementing a non-trivial custom subsystem, be able to state one of:
- `reuse_selected`: an existing solution is being used or adapted;
- `reuse_not_needed`: the repo already solves it or the task is trivial;
- `custom_justified`: reusable candidates were checked and failed named material constraints.

Never use `custom_justified` merely because writing code feels faster than looking for an existing solution.
