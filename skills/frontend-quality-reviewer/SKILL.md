---
name: frontend-quality-reviewer
description: Use when creating, redesigning, implementing, reviewing, polishing, or accepting a frontend/UI, landing page, product surface, design system, responsive layout, or visual web experience.
---

# Frontend Quality Reviewer

Treat frontend quality as product quality. This skill is the persistent router for visual frontend work: specialists run in phases, never as simultaneous competing authorities.

## Authority

Use this order when instructions conflict:

1. explicit user brief and approved visual source;
2. repo-local contracts, product truth, accessibility/security/privacy requirements;
3. incumbent brand, tokens, components, assets, and working behavior;
4. this phased workflow;
5. external design skills and reference repositories.

External taste never overrides evidence. If the repo contains `docs/UI_ANTI_VIBECODE.md`, it is a binding local review rubric.

## Required workflow

### 0. Reuse and incumbent audit

Invoke `$reuse-first-router`. Inspect the current project before inventing anything: components, tokens, dependencies, styles, assets, prior approved work, and—when accessible and relevant—shared patterns in the user's other repositories.

Prefer, in order: existing local solution -> existing shared user-repo pattern -> approved reference/component source such as Originkit or Skiper UI -> smallest adaptation -> custom implementation.

### 1. Lock the visual source of truth

An explicit Figma design, supplied screenshot/mock, brand system, or approved reference is binding for visual direction. If several sources exist, use the most explicit and current one. Never invent testimonials, business claims, logos, or fake product proof to make a layout look complete.

### 2. Direction pass — Taste Skill

Consult the contextual Taste Skill at `~/.agents/vendor/taste-skill/skills/taste-skill/SKILL.md` when available. Use it to challenge generic LLM defaults and to infer a design direction from audience, brief, references, and brand.

Do **not** make `gpt-tasteskill` the global baseline. Its opinionated AIDA/Bento/GSAP defaults can conflict with the project or anti-vibecode rules. Use it only for an explicitly experimental/Awwwards-like direction, and local/user constraints still win.

### 3. Implement in the existing architecture

Build with the project's current framework, design system, and dependency strategy unless a change is materially justified. Preserve approved behavior and content. Reuse source selectively; do not import a whole library for one pattern.

#### Motion/3D implementation routing

Choose the smallest implementation tool that matches the actual interaction problem, after checking the project dependencies first:

- **CSS / Web Animations API first** for simple fades, color changes, hover feedback, and straightforward transitions.
- **Motion** (`motiondivision/motion`, `motion.dev`) for component/block enter/exit, layout animation, springs, gestures, and React-oriented motion when the stack is already compatible.
- **GSAP** (`greensock/GSAP`, `gsap.com`) for genuinely complex timelines, ScrollTrigger, pinning, scrub, and synchronized scroll choreography.
- **Three.js** (`mrdoob/three.js`, `threejs.org`) only for real 3D: scene/camera/lighting/materials/geometry/WebGL/WebGPU/WebXR. In React, inspect whether the project already uses `@react-three/fiber` before introducing raw Three.js.

Do not stack Motion and GSAP as owners of the same animation domain without a clear architectural split. Three.js may coexist with one 2D animation owner when real 3D is required. Respect `prefers-reduced-motion`, mobile/performance budgets, lazy loading for heavy 3D, and cleanup of RAF/listeners/resources.

### 4. Evaluation pass — Impeccable

Consult `~/.agents/vendor/impeccable/.agents/skills/impeccable/SKILL.md` when available. Route only to the Impeccable playbook that fits the need. For a shipping pass, prefer a bounded sequence such as `critique` -> targeted fixes -> `audit` -> `polish`; do not run every command or enter an open-ended polish loop.

Use Impeccable as a critic/editor. Its own rule that the brief wins is compatible with this workflow; local contracts remain superior.

### 5. Craft pass — Emil Kowalski

Consult `~/.agents/vendor/emil-skills/skills/emil-design-eng/SKILL.md` when available for interaction and motion polish. Add motion only when it has a purpose. Frequent interactions should be instant or restrained; avoid hover/scroll animation that exists only to look sophisticated.

### 6. Browser proof — Playwright

Verify the real UI, not just source code. Use the project's build/lint/typecheck/a11y checks plus Playwright at representative desktop and mobile sizes and exercise the meaningful user flows.

Regular Playwright is the baseline. `playwright-mcp` is conditional: use it only when stateful/interactive browser control materially helps **and** the project's MCP/security/permission gate allows it. Never enable an MCP merely because this skill mentions it.

### 7. Acceptance gate

Before accepting a public/product UI:

- run the repo's anti-vibecode linter in strict mode when available;
- complete manual anti-vibecode checks that automation cannot prove;
- check responsiveness, keyboard/focus behavior, loading/error/empty states where applicable;
- require real product evidence/demonstration where the page claims product capability;
- run the repo's final build/test gates.

## Anti-slop cluster

Treat these as accumulation signals, not isolated bans: aggressive gradients/glows, AI-purple/neon palettes, generic three-card rows, generic bento grids, radial orbs/dot grids, decorative terminal windows, sparkle icons, over-rounded surfaces, fake testimonials, default Inter/Geist/Space Grotesk with no brand reason, decorative animated arrows, and gratuitous hover motion.

A choice is acceptable when brand, content, or interaction gives it a real job.

## Completion evidence

Report the visual source used, reuse decision, major critique findings fixed, browser/device coverage, relevant build/test results, and any intentional exception. Tool success alone is never acceptance.
