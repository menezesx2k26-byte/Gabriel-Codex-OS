# Global AGENTS.md

These are persistent cross-project operating rules for Codex.

## Core behavior
- Start by understanding the current project state before making changes.
- Prefer preserving approved work over replacing it.
- Do not overwrite existing work unless the change is necessary and justified.
- Do not declare success without validation appropriate to the task.
- Keep changes scoped to the requested objective.
- Avoid architecture churn. Do not rework foundations unless the task truly requires it.

## Reuse-first strategy
- Before designing or implementing a non-trivial solution from scratch, invoke `$reuse-first-router` when available.
- Check whether the problem is already solved well enough by an existing library, framework, CLI, service, template, skill, protocol, integration, open-source project, or repository-local utility.
- Prefer adopting or adapting a mature "Swiss-army knife" that solves most of the requirement over rebuilding the same capabilities piecemeal.
- Search progressively and cheaply: existing project code/utilities -> installed/user skills -> known curated references -> mature ecosystem packages/tools -> broad discovery -> custom implementation.
- Stop discovery as soon as a clearly adequate candidate satisfies the material constraints; do not exhaustively compare tools without a high-impact reason.
- Evaluate reuse candidates for project fit, maintenance activity, license, security, privacy, operational cost, lock-in, performance, integration complexity, and how much of the requirement they actually cover.
- Do not reject an existing solution merely because it is not architecturally perfect. Prefer the smallest safe adaptation when it substantially reduces code, tokens, maintenance, or failure surface.
- Prefer reuse in this order when practical: use as-is -> configuration -> adapter/wrapper -> selective module/pattern -> fork -> custom build.
- Do not adopt a dependency merely because it exists. Build custom only when existing options fail material constraints, create unacceptable risk/complexity, or the custom solution is genuinely simpler.
- When choosing custom implementation despite a plausible existing solution, record the concrete reason rather than silently reinventing it.
- Reuse concepts and patterns even when the dependency itself is unsuitable.

## Browser automation efficiency
- For browser automation, prefer tools and patterns that reduce repeated observe/act round-trips by composing deterministic multi-step actions when safe.
- Prefer semantic snapshots, scoped page state, and targeted extraction over repeatedly loading full-page representations.
- When a browser task can be expressed as one bounded script or batch of actions, prefer that over many small tool calls, provided intermediate inspection is not required for safety or correctness.
- Use `ego-lite` (`https://github.com/citrolabs/ego-lite`) as a reference/candidate for low-round-trip browser automation and isolated agent browser spaces when relevant; do not assume runtime compatibility or install it automatically.
- Do not sacrifice observability, confirmation boundaries, or safety merely to reduce tool calls.

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

## Verification budget
- Treat full-repository verification as a costly integration gate, not as a default exploration step.
- Do not run the entire test/lint/typecheck/build matrix at the start of a session merely to "confirm state" when a recent green result exists for unchanged relevant code.
- Reuse fresh, durable green evidence when the corresponding code has not changed.
- After a focused change, run the narrowest checks that directly cover the changed behavior first.
- Documentation-only, handoff-only, metadata-only, or other non-functional edits do not justify a full suite unless the repository explicitly proves otherwise.
- If a repository provides a selective verification command, changed-file test router, verification ledger, or cached evidence mechanism, use it before any full-suite command.
- Unknown or cross-cutting changes should escalate to a full gate rather than be guessed safe, but reserve that gate for the end of the implementation window whenever practical.
- Default budget: at most one full verification pass per execution window. A second full pass is justified only after a concrete failure and its corrective change, or when an explicit release/integration rule requires it.
- CI may independently run full integration checks; do not duplicate the same full proof locally without a concrete reason.
- When a full suite consumes a material share of the available agent window, record/reuse its result as durable evidence so the next session does not pay for the same proof again.
- Token optimization must never weaken required production, safety, security, or irreversible-action gates.

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

## Mandatory frontend/UI routing
- **NO FRONTEND EDIT BEFORE TOOL PREFLIGHT.** Before the first frontend/UI code edit, invoke `$frontend-quality-reviewer` and run its `scripts/preflight.py` against the target repo. A missing required specialist is a blocker, not permission to silently skip it.
- For meaningful visual frontend work, direct specialist use must be observable: `$taste-skill` before visual direction/implementation, `$impeccable` after a coherent implementation, and `$emil-design-eng` for the craft/motion decision. Keep them staged; do not load them as competing authorities.
- If the current harness cannot invoke a discoverable skill by name, load that installed `SKILL.md` directly and record `manual-load`; never claim `invoked` when it was not.
- Every frontend/UI completion or handoff must include a `UI_TOOL_RECEIPT` with preflight, reuse, visual source, Taste, animation route, Impeccable, Emil, Playwright/browser proof, anti-vibecode, and final gates. `N/A` requires a concrete reason.
- For every task that creates, redesigns, implements, reviews, polishes, or accepts a meaningful frontend/UI, invoke `$frontend-quality-reviewer` when available.
- The frontend router is staged, not additive: repo/reuse audit -> approved design source -> contextual Taste direction -> implementation in the incumbent stack -> Impeccable critique/audit/polish -> Emil Kowalski interaction craft -> Playwright/browser proof -> repo hard gates.
- Project/user truth outranks external design taste. When `docs/UI_ANTI_VIBECODE.md` exists, treat it as a binding local rubric and run its automated/manual gates before acceptance.
- Do not use `gpt-tasteskill` as the global default; its AIDA/Bento/GSAP opinions are opt-in for an explicitly experimental direction and never override the brief or local anti-vibecode rules.
- Regular Playwright is the baseline browser verifier. `playwright-mcp` is conditional and may be used only when interactive/stateful control materially improves verification and the project's MCP/security gate permits it.
- Inspect the current project and relevant existing user repositories before reaching for external UI sources. Originkit/Skiper UI and other references are fallback material, not permission to replace a working design system.
- For implementation motion, route by problem: CSS/WAAPI for simple transitions; Motion for component/block/layout/gesture motion; GSAP for complex timeline/scroll/pinning/scrub; Three.js only for real 3D. Never add these as automatic premium-site decoration, and do not let two animation runtimes own the same domain without justification.

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
