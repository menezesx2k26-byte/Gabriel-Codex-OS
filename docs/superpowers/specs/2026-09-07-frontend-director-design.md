# Frontend Director Design

Date: 2026-09-07
Status: Approved design, pending implementation plan
Owner: Gabriel Codex OS

## Problem

Gabriel Codex OS already has strong frontend/design specialists and routing, including the preferred 42-skill design cohort, `reuse-first-router`, `frontend-quality-reviewer`, and visual review rules. The missing layer is orchestration: the user still has to decide which specialist to call, in what order, how to resolve conflicts, when to re-run review, and when the task is actually finished.

The new `frontend-director` must remove that coordination burden. For every non-trivial frontend task, one mandatory director owns the work end-to-end: diagnose the frontend, choose the smallest useful specialist set, implement, refactor or rebuild as needed, validate the result, iterate autonomously, and only stop at `APPROVED` or a genuine out-of-scope `BLOCKED` state.

## Goals

- Make `frontend-director` the automatic and mandatory entry point for non-trivial frontend work.
- Give it authority to change the frontend architecture, component structure, design system, tokens, libraries, and dependencies when justified by the product goal.
- Allow partial or complete reconstruction of the presentation layer while preserving functional behavior and contracts.
- Make the director select and coordinate design/frontend specialists automatically instead of requiring the user to name skills.
- Keep specialist loading narrow and on-demand rather than bulk-loading the 42 preferred design skills.
- Require both engineering evidence and visual/UX evidence before approval.
- Preserve backend, API, business-rule, infrastructure, and security boundaries unless the user separately authorizes changes outside frontend scope.

## Non-goals

- The director is not a replacement for backend, infrastructure, data, or product-logic ownership.
- It must not introduce multi-agent architecture merely because specialist delegation exists conceptually.
- It must not bulk-install or bulk-load the full design catalogue.
- It must not churn dependencies for taste or novelty alone.
- It must not treat build/test success as sufficient visual acceptance.
- It must not preserve a weak existing UI solely to minimize diff size when reconstruction is materially better.

## Mandatory routing

For every non-trivial task involving creation or material alteration of frontend UI, layout, components, responsive behavior, design systems, tokens, typography, motion, accessibility, frontend performance, or presentation-layer refactoring, `frontend-director` is mandatory even when the user does not invoke it explicitly.

Trivial copy-only edits and similarly narrow presentation changes may bypass the director when they do not require design judgment, structural change, or visual validation.

When `frontend-director` owns a task, it remains the top-level owner until terminal state. `reuse-first-router`, catalogue routers, implementation skills, review skills, and external libraries are subordinate capabilities. They may advise, supply implementation patterns, or review results, but they do not take ownership away from the director.

## Authority boundary

Within the frontend layer, the director may:

- refactor component structure and module boundaries;
- extract, merge, delete, or replace components;
- rebuild layouts and interaction flows;
- create, replace, or reorganize design tokens and design-system primitives;
- change typography, spacing, color systems, visual hierarchy, motion, and responsive strategy;
- install, upgrade, replace, or remove frontend dependencies;
- remove obsolete frontend code and duplicate libraries;
- replace a weak implementation with a different library when the new choice has a concrete project-fit advantage;
- partially or completely reconstruct the presentation layer while preserving behavior and contracts.

The director may not silently change API contracts, backend behavior, business rules, infrastructure, authentication/security boundaries, or externally observable product semantics merely to make the frontend easier to implement. If those changes are required, the task becomes `BLOCKED` and must be escalated with concrete options.

## Dependency policy

The director is allowed to install and remove dependencies automatically, but dependency changes require a concrete reason such as lower implementation complexity, better maintainability, reduced duplication, improved accessibility, better performance, stronger project fit, or removal of a material limitation.

A library must not be replaced only because another library is fashionable or aesthetically preferred. After replacement, obsolete dependencies and dead integration code should be removed when safe.

## Execution model

The director follows this lifecycle:

1. Reconnaissance
   - inspect the actual frontend, stack, design system, components, dependencies, approved references, screenshots, and project-specific constraints;
   - identify behavior and contracts that must be preserved.
2. Diagnosis
   - classify material issues across structure, composition, identity, typography, color, UX, motion, responsiveness, accessibility, and performance;
   - choose `refine`, `refactor`, or `rebuild` as the working strategy.
3. Visual thesis and acceptance criteria
   - establish a one-sentence project-specific visual thesis;
   - define concrete success criteria and preserve constraints;
   - retain the existing transplant-test requirement for generic-looking interfaces.
4. Specialist plan
   - select the smallest materially useful specialist set from the preferred design cohort first;
   - assign roles as Lead, Support, and Review;
   - resolve only the selected implementation skills on demand.
5. Implementation
   - edit code, dependencies, component structure, tokens, design system, and presentation architecture as necessary;
   - preserve functional contracts while exercising full authority inside frontend scope.
6. Rendered inspection
   - inspect the real rendered interface when the environment permits;
   - verify relevant desktop/mobile breakpoints and meaningful UI states rather than relying on source inspection alone.
7. Gates
   - run focused engineering checks and visual/UX review;
   - route failures to the smallest appropriate specialist.
8. Autonomous correction loop
   - diagnose failed gates, correct the cause, and re-review without returning routine revision work to the user.
9. Terminal state
   - finish only as `APPROVED` or `BLOCKED`.

## Specialist orchestration

The 42 preferred design skills remain the first specialist pool. The director chooses them by capability and concrete diagnosis, not by broad category matching alone.

Examples include:

- visual/interface transformation: `frontend-design`, `impeccable`, `make-interfaces-better`, `taste-skill`;
- color/tokens/system work: `color-expert`, `design-tokens-skill`, `design-system-governance`, `work-with-design-systems`;
- interaction/information architecture: `interfaces-that-feel`, `information-architecture-and-navigation`, `search-ux`, `neo-user-journey`;
- motion: `animate-skill`, `css-animation-skill`, `wiggle-claude-skill`, `remotion-best-practices` when relevant;
- accessibility: `fixing-accessibility`;
- performance: `cloudflare-web-perf` when the change materially affects performance;
- review: `design-auditor`, `plan-design-review`, `design-review-garrytan`, `frontend-quality-reviewer` as appropriate.

A specialist enters only when there is a concrete problem or hypothesis it can materially address. The director must not load specialists "just in case."

## Roles

Each task may assign:

- `Lead`: normally one specialist that drives the main transformation;
- `Support`: zero to two specialists for focused secondary concerns;
- `Review`: one or more auditors used after implementation reaches a reviewable state;
- `Director`: permanent task owner and final decision-maker.

The director may replace the Lead mid-task if the diagnosis changes. Tool choice is provisional; the product objective and preserve constraints are fixed.

## Conflict resolution

When specialist recommendations conflict, the director resolves them using this priority order:

1. functional requirements and preserved contracts;
2. approved project identity and user intent;
3. UX and accessibility;
4. system coherence and maintainability;
5. visual quality;
6. specialist preference.

A specialist recommendation is advisory. No specialist may override the director or force a library, aesthetic, or architecture choice solely because that choice appears in its own guidance.

## Parallelism

Execution is sequential by default because design decisions are strongly dependent on earlier structural and visual decisions.

Independent review work may run in parallel when the environment supports it safely, for example accessibility, performance, and visual QA on a stable candidate. The director then reconciles those findings into one coherent correction pass.

The workflow must not depend on multi-agent runtime support. The same roles must work in a single-agent environment by loading the selected skills one at a time.

## Iteration discipline

Routine failures remain inside the director loop. `NEEDS_REVISION` is not an escalation state.

Rules:

- focused failure -> focused correction;
- structural failure discovered during QA -> return to implementation with a revised strategy;
- the same failure appearing in two consecutive review rounds -> change strategy rather than repeating substantially identical patches;
- do not impose a fixed arbitrary retry count;
- do not iterate indefinitely without a new diagnosis;
- if progress requires backend, API, business-rule, infrastructure, security, unavailable credentials, or an unresolved product decision outside frontend authority -> `BLOCKED`.

## Execution record

For non-trivial tasks, maintain a compact execution record sufficient to preserve coordination state across iterations or sessions:

```text
objective
visual_thesis
preserve_constraints
strategy = refine | refactor | rebuild
lead
support[]
reviewers[]
dependency_changes[]
structural_changes[]
gates:
  functional
  engineering
  responsive
  visual
  accessibility
  performance
open_failures[]
state
```

This record should integrate with durable execution mechanisms already present in Gabriel Codex OS when a task is long-running, interruptible, or cross-session. It is operational state, not a verbose design diary.

## Gates and Definition of Done

### Functional gate

- preserved behavior still works;
- meaningful navigation and states work;
- no API contract is broken;
- no known functional regression was introduced by frontend reconstruction.

### Engineering gate

- relevant build, typecheck, lint, and focused tests pass as appropriate;
- dependency changes are coherent;
- obsolete integration code and obvious dead code are removed;
- duplicate libraries serving the same role require explicit justification.

Use the repository's existing verification-budget policy: focused checks first, one full integration pass only when justified by the scope.

### Visual gate

- hierarchy, composition, typography, spacing, color, and contrast are coherent;
- relevant desktop/mobile layouts are inspected when applicable;
- no unintended placeholders, fake UI, generic filler, or obvious visual defects remain;
- the result matches the project's visual thesis and domain identity;
- the transplant test passes.

### UX and accessibility gate

- interactions are understandable and provide appropriate feedback;
- keyboard/focus/semantic/contrast checks are applied where relevant;
- loading, empty, error, disabled, and similar states are handled when they exist;
- motion does not harm usability.

### Performance gate

Performance review becomes material when the change affects bundle size, rendering, hydration, images, animation cost, or similar runtime concerns. Do not run heavyweight performance audits on trivial changes without evidence of impact.

## Rendered evidence requirement

For meaningful visual work, passing source-level checks is not enough. When a runnable environment is available, the director should inspect the actual rendered result and relevant responsive states before approval.

The governing rule is:

> Build green is not visual approval. A visually attractive screenshot is not functional approval. Both forms of evidence are required when applicable.

## States

The director uses four formal states:

- `IN_PROGRESS`: active implementation or investigation;
- `NEEDS_REVISION`: a gate failed and the director continues autonomously;
- `BLOCKED`: continuation requires authority, information, access, or changes outside frontend scope;
- `APPROVED`: all material gates have passed with sufficient evidence.

`APPROVED` is terminal. `NEEDS_REVISION` must not be surfaced as a request for routine user coordination.

## BLOCKED protocol

When blocked, report:

- the exact blocker;
- why it prevents further correct progress;
- concrete available options;
- the director's recommended option.

Do not ask a generic "what should I do?" when the decision can be framed precisely.

## Completion report

The default completion report is concise and should include:

```text
APPROVED

Strategy: refine | refactor | rebuild
Lead: <skill>
Support: <skills or none>
Review: <skills>

Main changes:
- ...

Dependencies:
+ added
- removed

Gates:
✓ functional
✓ engineering
✓ responsive
✓ visual
✓ accessibility
✓ performance when material

Pending: none
```

Detailed coordination history should remain available for inspection but should not flood the normal handoff.

## Integration with existing Gabriel Codex OS rules

- `global/AGENTS.md` should route every non-trivial frontend task through `frontend-director` automatically and obligatorily.
- `reuse-first-router` remains mandatory for non-trivial reuse decisions, but when the task is owned by `frontend-director`, it acts as subordinate discovery/reuse infrastructure rather than a competing orchestrator.
- `design-agent-skills` and the 42 preferred cohort remain the primary design specialist catalogue, loaded on demand.
- `frontend-quality-reviewer` becomes a review gate rather than the top-level frontend coordinator.
- `visual-quality-director` continues to own raster/image asset quality and visual-asset approval where applicable; the frontend director coordinates it when those assets are part of a frontend task.
- `context-budget-manager` should keep specialist and reference loading narrow.
- `durable-execution-memory` should preserve execution state for long-running or interruptible frontend work.

## Acceptance criteria for the implementation

The implementation is complete when repository policy and skills make the following behavior unambiguous and testable:

1. A non-trivial frontend request automatically routes to `frontend-director` without the user naming it.
2. The director remains top-level owner for the whole frontend task.
3. The director can refactor or rebuild frontend structure and can install/remove dependencies within its authority boundary.
4. The director selects only materially relevant specialists and does not bulk-load the 42-skill cohort.
5. Specialist conflicts are resolved by the documented precedence rules.
6. Review failures cause autonomous correction rather than routine escalation to the user.
7. Approval requires engineering plus rendered visual/UX evidence when applicable.
8. Backend, API, business-rule, infrastructure, and security boundaries remain protected unless separately authorized.
9. `frontend-quality-reviewer`, `reuse-first-router`, `visual-quality-director`, context management, and durable execution integrate as subordinate capabilities without ownership loops.
10. Regression tests cover mandatory routing, authority boundaries, on-demand specialist selection, terminal states, and the anti-"passing tests = success" rule.
