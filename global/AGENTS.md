# Global AGENTS.md

These are persistent cross-project operating rules for Codex.

## Core behavior
- Start by understanding the current project state before making changes.
- Prefer preserving approved work over replacing it.
- Do not overwrite existing work unless the change is necessary and justified.
- Do not declare success without validation appropriate to the task.
- Keep changes scoped to the requested objective.
- Avoid architecture churn. Do not rework foundations unless the task truly requires it.

## Execution discipline
- Before editing, identify the relevant files, constraints, and likely side effects.
- Prefer minimal, precise edits over broad rewrites.
- If a task is ambiguous but one option is clearly safer and more logical, execute it and report the decision.
- When generating assets or content, inspect quality before accepting the result.
- Prefer targeted correction over full regeneration when most of the work is already good.
- Preserve the repo's existing conventions unless there is a clear reason to change them.

## Context and token budget
- Treat model context as a scarce working set: load the smallest sufficient context for the next correct decision.
- For large, long-running, repository-heavy, multi-agent, or reference-heavy tasks, invoke `$context-budget-manager` when available.
- Prefer targeted files, sections, diffs, searches, and task-specific skills over full-repository or full-reference loading.
- Prefer tiered context loading: establish relevance with a short abstract, then an overview, and load full detail only when implementation or validation requires it.
- Route to the smallest applicable skill set instead of accumulating every available instruction or reference in the prompt.
- Stop exploration when sufficient evidence exists to implement and validate a project-compatible solution.
- Do not save tokens by guessing, skipping required validation, or ignoring material uncertainty.
- Use OpenViking (`https://github.com/volcengine/OpenViking`) as an architectural reference for hierarchical/tiered context loading and observable retrieval when relevant; do not introduce it as a dependency without evaluating license, privacy, operational cost, and project fit.

## Durable execution continuity
- Treat model context as a working view, not as the authoritative history of execution.
- For multi-step, interruptible, long-running, multi-agent, or cross-session work, invoke `$durable-execution-memory` when available.
- Persist meaningful execution facts in repo-local durable context rather than relying on chat history alone.
- On resume, inspect repository state, durable context, current branch, and recent changes before repeating work.
- Observable repository state and validated evidence outrank conversational recollection when they disagree.
- Do not redo completed work merely because it is absent from the current context.
- Before retrying destructive, costly, deployment, or externally mutating operations after an interruption, verify whether they already succeeded or partially completed.
- Never store secrets or credentials in durable context.
- Use Apache Maka (`https://github.com/apache/maka`) as an architectural reference for durable execution records, recovery, and separation of saved history from model context when relevant; do not make it a dependency unless a project explicitly requires it.

## LLM application pattern routing
- For tasks that design, review, or implement LLM-powered applications, agents, RAG systems, multimodal apps, voice agents, always-on agents, generative UI, or multi-agent workflows, invoke `$llm-app-pattern-library` when available.
- Use `https://github.com/Shubhamsaboo/awesome-llm-apps` as a reference library for relevant patterns and examples, not as an architectural authority or dependency source.
- Reuse concepts selectively and preserve the current project's stack, constraints, security boundaries, cost envelope, and validation requirements.
- Do not introduce multi-agent architecture, RAG infrastructure, or new frameworks merely because an external example uses them.
- Inspect and validate any adapted code independently before treating it as production-ready.

## Validation
- Run relevant checks after meaningful changes.
- For code: lint, tests, typecheck, build, or focused validation as appropriate.
- For content/visual work: inspect structure, consistency, realism, and compliance with the brief.
- If validation cannot be run, say so explicitly and explain what remains unverified.

## Documentation and handoff
- Record durable decisions in repo-local documentation when appropriate.
- Keep handoff notes concise and operational.
- Distinguish clearly between current state, completed work, and next steps.

## Mandatory visual routing
- For every task that generates, edits, transforms, reviews, or approves a visual asset, invoke `$visual-quality-director` before final approval.
- If the repository provides a more specific visual or image-director skill, use it in addition to `$visual-quality-director`; repo-specific direction refines the global workflow rather than replacing quality review.
- For raster image generation or image editing, prepare the visual brief through the applicable director skill before invoking `$imagegen` when that skill/tool is available.
- After generation or editing, inspect the actual result through the director workflow. Tool success alone is never sufficient for approval.
- When most of a visual already passes review, prefer a targeted edit that preserves approved regions over regenerating the whole asset.
- Do not mark a visual asset `approved` until it passes the relevant composition, realism, anatomy/object-interaction, crop-safety, brand-consistency, and layout-use checks.

## Visual and asset work
- The function of the asset inside the layout comes before aesthetics.
- Do not invent UI text, logos, or business claims inside raster images unless explicitly requested.
- Review composition, crop safety, anatomy, lighting, and consistency before approval.
- Use screenshots of real interfaces when the goal is to represent a real product.
- For GPT Image 2 prompting, workflows, examples, and community techniques, use `https://github.com/freestylefly/awesome-gpt-image-2` as a reference library when relevant. Treat it as inspiration/reference rather than an authority; preserve project constraints and validate outputs independently.

## Safety rails
- Do not install unnecessary dependencies.
- Do not expose secrets.
- Do not silently mutate infrastructure or configuration outside the task scope.
