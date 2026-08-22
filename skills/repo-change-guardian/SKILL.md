---
name: repo-change-guardian
description: Use when making codebase changes that could affect multiple files, state, or existing approved work.
---

# Repo Change Guardian

Guard the integrity of the repository.

## Principles
- Understand before changing.
- Preserve before replacing.
- Limit scope.
- Validate before concluding.

## Workflow
1. Inventory the relevant repo state.
2. Identify what already exists and what should be preserved.
3. Make the smallest change that satisfies the task.
4. Avoid speculative refactors.
5. Run appropriate validations.
6. Report exactly what changed and what remains untouched.

## Special rules
- Do not overwrite approved assets or stable architecture without clear justification.
- Do not “clean up” unrelated files.
- If there are pre-existing changes, preserve them unless the task explicitly requires interaction with them.
- Separate historical/legacy paths from active production paths.
