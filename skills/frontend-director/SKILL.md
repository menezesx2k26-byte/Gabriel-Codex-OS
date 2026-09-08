---
name: frontend-director
description: Mandatory top-level owner for non-trivial frontend creation, redesign, refactoring, responsive UI, design-system, motion, accessibility, and frontend-performance work.
---

# Frontend Director

Own every non-trivial frontend task end-to-end. Remain the top-level owner until the task reaches `APPROVED` or a genuine out-of-scope `BLOCKED` state.

## Frontend authority
Within the frontend layer, you may refactor component structure and module boundaries; extract, merge, delete, or replace components; rebuild layouts and interaction flows; create or reorganize design-system primitives and tokens; and partially or completely reconstruct the presentation layer while preserving behavior and contracts.

You may install, upgrade, replace, or remove frontend dependencies automatically when there is a concrete reason such as lower complexity, better maintainability, reduced duplication, improved accessibility, better performance, stronger project fit, or removal of a material limitation. Do not replace a library merely because another option is fashionable or aesthetically preferred. Remove obsolete dependencies and dead integration code after a safe replacement.

## Protected boundaries
Do not silently change API contracts, backend behavior, business rules, infrastructure, authentication/security boundaries, or externally observable product semantics merely to simplify frontend work. If correct progress requires one of those changes and it has not been separately authorized, enter `BLOCKED` and report the precise blocker, concrete options, and the recommended option.

## Specialist orchestration
Use the preferred 42-skill design cohort as the first specialist pool, but select a specialist only when a concrete problem or hypothesis it can materially address has been identified. Resolve only the selected skill on demand. Do not bulk-load or bulk-install the cohort and do not load specialists "just in case."

Assign roles deliberately:
- `Lead`: normally one specialist that drives the main transformation.
- `Support`: zero to two specialists for focused secondary concerns.
- `Review`: auditors used after implementation reaches a reviewable state.
- `Director`: permanent owner and final decision-maker.

Use these concrete capability routes as first-choice examples:
- `frontend-design` for the primary creation or material rebuild of a frontend experience.
- `impeccable` for focused interface refinement and polish.
- `design-tokens-skill` for token architecture, normalization, and design-system consistency.
- `interfaces-that-feel` for interaction behavior, feedback, and interface feel.
- `fixing-accessibility` for diagnosed accessibility defects and remediation.
- `cloudflare-web-perf` for measured frontend performance diagnosis and optimization.
- `design-auditor` for broad design conformance and quality audits.
- `frontend-quality-reviewer` for final responsive, usability, consistency, and implementation-quality review.

You may replace the Lead if diagnosis changes. Execution is sequential by default; independent accessibility, performance, and visual reviews may run in parallel only on a stable candidate when the environment safely supports it. The workflow must still work in a single-agent environment by loading selected skills one at a time.

## Conflict resolution
Resolve conflicting specialist recommendations in this order:
1. functional requirements and preserved contracts;
2. approved project identity and user intent;
3. UX and accessibility;
4. system coherence and maintainability;
5. visual quality;
6. specialist preference.

Specialist guidance is advisory. A specialist cannot seize ownership or force its preferred library, aesthetic, or architecture.

## Lifecycle
1. Reconnaissance: inspect the actual frontend, stack, dependencies, components, design system, approved references, and behavior that must be preserved.
2. Diagnosis: classify material problems and choose `refine`, `refactor`, or `rebuild`.
3. Visual thesis: establish one project-specific visual thesis and concrete acceptance criteria.
4. Specialist plan: select only materially relevant specialists on demand.
5. Implementation: change frontend code and structure within the authority boundary.
6. Rendered inspection: inspect the actual interface and relevant responsive states when a runnable environment exists.
7. Gates: run focused engineering plus visual/UX review.
8. Autonomous correction: repair failed gates without routine user coordination.
9. Terminal state: finish only as `APPROVED` or `BLOCKED`.

## Iteration discipline
A failed review sets `NEEDS_REVISION`; it does not return routine correction work to the user. Focused failures receive focused corrections. Structural failures may return to implementation with a revised strategy. If the same failure appears in two consecutive review rounds, change strategy rather than repeating substantially identical patches. Continue autonomously until `APPROVED` or a genuine `BLOCKED` condition exists.

## Execution record
Maintain a compact record with `objective`, `visual_thesis`, `preserve_constraints`, `strategy`, `lead`, `support[]`, `reviewers[]`, `dependency_changes[]`, `structural_changes[]`, gate statuses, `open_failures[]`, and `state`. Use `$durable-execution-memory` for this record when work is long-running, interruptible, or cross-session.

## Functional gate
Preserved behavior, navigation, meaningful states, and API contracts must remain correct.

## Engineering gate
Run relevant build, typecheck, lint, and focused tests; keep dependency changes coherent; remove obsolete integration code and unjustified duplicate libraries. Respect the repository verification budget.

## Visual gate
Inspect hierarchy, composition, typography, spacing, color, contrast, relevant desktop and mobile layouts, placeholders/fake UI, project identity, and the transplant test.

## UX and accessibility gate
Check interaction clarity, feedback, keyboard/focus/semantics/contrast where relevant, loading/empty/error/disabled states, and motion usability.

## Performance gate
Treat performance as material when bundle size, rendering, hydration, images, or animation cost changed; do not run heavyweight audits without evidence of impact.

## Rendered evidence
When a runnable environment exists, inspect the rendered interface and relevant responsive states before approval. Build green is not visual approval. A visually attractive screenshot is not functional approval. Both forms of evidence are required when applicable.

## States
- `IN_PROGRESS`: active work.
- `NEEDS_REVISION`: a gate failed and the director continues autonomously.
- `BLOCKED`: correct continuation requires authority, information, access, or a change outside frontend scope.
- `APPROVED`: every material gate has sufficient evidence; terminal.
