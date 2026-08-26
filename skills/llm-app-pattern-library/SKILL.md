---
name: llm-app-pattern-library
description: Use when designing, reviewing, or implementing LLM-powered applications, agents, RAG systems, multimodal apps, generative UI, voice agents, always-on agents, or multi-agent workflows.
---

# LLM App Pattern Library

Use proven open-source application patterns to accelerate design and implementation without blindly copying examples.

## Primary reference
Use `https://github.com/Shubhamsaboo/awesome-llm-apps` as a pattern and implementation reference when the task involves LLM applications, including:
- starter AI agents
- advanced single-agent systems
- multi-agent teams
- agent skills
- RAG applications
- multimodal applications
- voice agents
- always-on/scheduled agents
- generative UI and agentic frontends
- web research and scraping agents
- evaluation, orchestration, memory, and tool-use patterns

The repository contains 100+ open-source examples and agent skills spanning multiple model providers and frameworks. Treat it as a library of patterns and examples, not as an architectural authority.

## Workflow
1. Identify the application class and the exact capability being built.
2. Search the reference repository for the closest relevant examples or patterns.
3. Inspect only the examples that materially overlap with the current requirement.
4. Extract reusable ideas at the level of:
   - architecture
   - tool boundaries
   - orchestration
   - state and memory
   - RAG/data flow
   - model routing
   - UI interaction pattern
   - evaluation and safety gates
   - deployment/runtime assumptions
5. Compare those ideas against the current repository's constraints, stack, dependencies, cost envelope, privacy needs, and existing architecture.
6. Reuse concepts selectively. Do not copy an entire example when a smaller native implementation fits the current project better.
7. Preserve existing project conventions and avoid introducing a framework solely because an example uses it.
8. Validate the resulting implementation independently with the project's own tests and acceptance criteria.

## Selection rules
- Prefer the simplest pattern that satisfies the requirement.
- Prefer examples with similar runtime, interaction model, and operational constraints over examples that merely look impressive.
- Do not treat popularity, novelty, or the word `agent` as evidence that a multi-agent architecture is necessary.
- Prefer a single agent or deterministic workflow when coordination overhead would add no real value.
- For multi-agent patterns, require a clear reason for role separation, parallelism, independent context, or evaluation boundaries.
- For RAG, verify retrieval quality and data authority instead of assuming a vector database is automatically appropriate.
- For always-on agents, define trigger, cadence, idempotency, state, failure recovery, and notification behavior explicitly.
- For generative UI, keep critical state and validation outside untrusted model-generated presentation where appropriate.
- For web or browser automation, respect site policies, user intent, authentication boundaries, and rate limits.

## Dependency discipline
- Never install a dependency just because a reference implementation uses it.
- Before adding a framework, check whether the current stack already provides the needed primitive.
- Prefer standard libraries and existing dependencies when they produce a simpler and more maintainable result.
- Treat model/provider SDKs in examples as replaceable implementation details unless the task requires that provider.

## Security and quality
- Inspect copied or adapted code before use.
- Do not copy secrets, example API keys, unsafe defaults, permissive sandbox settings, or unbounded tool permissions.
- Validate third-party package versions and maintenance status when adopting dependencies.
- Keep external tool actions scoped and auditable.
- Treat medical, financial, legal, security, or other high-stakes demo applications as examples of mechanics only; do not inherit claims of reliability or suitability from a demo repository.

## Output expectation
When this skill materially informs a task, briefly record:
- which application pattern or example family was consulted
- what concept was reused
- what was deliberately not copied
- which project-specific validation proves the implementation is acceptable
