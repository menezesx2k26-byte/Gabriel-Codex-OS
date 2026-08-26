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
