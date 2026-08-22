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

## Visual and asset work
- The function of the asset inside the layout comes before aesthetics.
- Do not invent UI text, logos, or business claims inside raster images unless explicitly requested.
- Review composition, crop safety, anatomy, lighting, and consistency before approval.
- Use screenshots of real interfaces when the goal is to represent a real product.

## Safety rails
- Do not install unnecessary dependencies.
- Do not expose secrets.
- Do not silently mutate infrastructure or configuration outside the task scope.
