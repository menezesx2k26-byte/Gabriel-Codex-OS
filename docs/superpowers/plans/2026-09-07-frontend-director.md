# Frontend Director Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `frontend-director` the automatic, mandatory, end-to-end owner of every non-trivial frontend task, with authority to refactor/rebuild frontend code and dependencies while coordinating specialists and enforcing engineering plus rendered visual/UX gates.

**Architecture:** Add one user skill as the frontend task owner and wire global policy to route non-trivial frontend work into it automatically. Keep `reuse-first-router`, the 42-skill design cohort, `frontend-quality-reviewer`, `visual-quality-director`, context management, and durable execution as subordinate capabilities selected on demand. Regression tests remain lightweight string-policy tests, matching the repository's existing test style.

**Tech Stack:** Markdown skill/policy files, Python `unittest`, PowerShell installer that already copies `skills/*` to `~/.agents/skills`.

**Spec:** `docs/superpowers/specs/2026-09-07-frontend-director-design.md`

## Global Constraints

- `frontend-director` is automatic and mandatory for every non-trivial frontend task, even when the user does not name it.
- The director remains top-level owner until `APPROVED` or `BLOCKED`.
- Within frontend scope it may refactor/rebuild component structure, design systems, tokens, layouts, interactions, and install/upgrade/replace/remove frontend dependencies.
- It must preserve functional behavior and contracts and may not silently alter backend, API contracts, business rules, infrastructure, authentication/security boundaries, or externally observable product semantics.
- Specialists are selected only when materially relevant; do not bulk-install or bulk-load the 42-skill cohort.
- Routine review failures remain autonomous `NEEDS_REVISION`; do not return ordinary correction work to the user.
- `APPROVED` requires engineering evidence plus rendered visual/UX evidence when a runnable environment exists.
- Existing verification-budget policy remains in force: focused checks first and at most one full integration pass by default.

---

## File Structure

- Create `skills/frontend-director/SKILL.md` — authoritative ownership, lifecycle, specialist orchestration, authority boundaries, iteration loop, gates, states, and completion protocol.
- Create `tests/test_frontend_director.py` — regression coverage for mandatory routing, authority, specialist selection, ownership, gates, terminal states, and integrations.
- Modify `global/AGENTS.md` — small mandatory routing rule only; keep process detail in the skill.
- Modify `skills/reuse-first-router/SKILL.md` — state that under a frontend-director-owned task it is subordinate discovery infrastructure and may not seize orchestration ownership.
- Modify `skills/frontend-quality-reviewer/SKILL.md` — make its role explicitly reviewer/gate-oriented under frontend-director rather than implementation coordinator.
- Modify `README.md` — document the new central skill and the automatic frontend route.
- Do not modify `scripts/install.ps1`; it already recursively copies `skills/*` into the user skill directory.

### Task 1: Mandatory route and top-level ownership

**Files:**
- Create: `tests/test_frontend_director.py`
- Create: `skills/frontend-director/SKILL.md`
- Modify: `global/AGENTS.md` under the global routing rules

**Interfaces:**
- Consumes: existing global skill invocation convention `$skill-name` and recursive installer behavior.
- Produces: `$frontend-director` as the unique top-level frontend owner; later tasks extend this same skill file.

- [ ] **Step 1: Write the failing routing tests**

Create `tests/test_frontend_director.py` with this initial content:

```python
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
AGENTS = (ROOT / "global" / "AGENTS.md").read_text(encoding="utf-8")
DIRECTOR_PATH = ROOT / "skills" / "frontend-director" / "SKILL.md"
DIRECTOR = DIRECTOR_PATH.read_text(encoding="utf-8") if DIRECTOR_PATH.exists() else ""


class FrontendDirectorTests(unittest.TestCase):
    def test_frontend_director_skill_exists(self):
        self.assertTrue(DIRECTOR_PATH.exists())
        self.assertIn("name: frontend-director", DIRECTOR)

    def test_non_trivial_frontend_work_routes_automatically(self):
        lowered = AGENTS.lower()
        self.assertIn("$frontend-director", lowered)
        self.assertIn("non-trivial frontend", lowered)
        self.assertIn("mandatory", lowered)
        self.assertIn("even when", lowered)

    def test_director_keeps_top_level_ownership(self):
        lowered = DIRECTOR.lower()
        self.assertIn("top-level owner", lowered)
        self.assertIn("approved", lowered)
        self.assertIn("blocked", lowered)


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: failure because `skills/frontend-director/SKILL.md` does not exist and `global/AGENTS.md` does not yet contain the mandatory route.

- [ ] **Step 3: Add the minimal global route**

Insert a compact section in `global/AGENTS.md` before the generic validation section:

```markdown
## Mandatory frontend routing
- For every non-trivial frontend task involving creation or material alteration of UI, layout, components, responsive behavior, design systems, tokens, typography, motion, accessibility, frontend performance, or presentation-layer structure, invoke `$frontend-director` automatically and treat it as mandatory even when the user did not name it.
- Once `$frontend-director` owns a task, it remains the top-level owner until it reaches `APPROVED` or a genuine out-of-scope `BLOCKED` state; subordinate routers, specialists, reviewers, and libraries do not replace that ownership.
- Trivial copy-only edits and similarly narrow changes may bypass the director when they require no design judgment, structural change, or visual validation.
```

- [ ] **Step 4: Create the director skill shell with ownership and lifecycle**

Create `skills/frontend-director/SKILL.md` beginning with:

```markdown
---
name: frontend-director
description: Mandatory top-level owner for non-trivial frontend creation, redesign, refactoring, responsive UI, design-system, motion, accessibility, and frontend-performance work.
---

# Frontend Director

Own every non-trivial frontend task end-to-end. Remain the top-level owner until the task reaches `APPROVED` or a genuine out-of-scope `BLOCKED` state.

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
```

- [ ] **Step 5: Run the focused test and verify GREEN**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: all Task 1 tests pass.

- [ ] **Step 6: Commit Task 1**

```powershell
git add global/AGENTS.md skills/frontend-director/SKILL.md tests/test_frontend_director.py
git commit -m "feat: add mandatory frontend director route"
```

### Task 2: Authority, dependencies, and protected boundaries

**Files:**
- Modify: `tests/test_frontend_director.py`
- Modify: `skills/frontend-director/SKILL.md`

**Interfaces:**
- Consumes: top-level ownership from Task 1.
- Produces: explicit frontend mutation authority plus an explicit `BLOCKED` boundary for non-frontend contracts.

- [ ] **Step 1: Add failing authority tests**

Add these methods to `FrontendDirectorTests`:

```python
    def test_director_can_rebuild_and_manage_dependencies(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "refactor component structure",
            "rebuild",
            "install",
            "upgrade",
            "replace",
            "remove frontend dependencies",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_director_protects_non_frontend_contracts(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "api contracts",
            "backend",
            "business rules",
            "infrastructure",
            "authentication/security",
            "blocked",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_dependency_churn_requires_a_concrete_reason(self):
        lowered = DIRECTOR.lower()
        self.assertIn("concrete reason", lowered)
        self.assertIn("fashionable", lowered)
        self.assertIn("obsolete dependencies", lowered)
```

- [ ] **Step 2: Run focused tests and verify RED**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: the new authority tests fail because the detailed policy is not present yet.

- [ ] **Step 3: Implement the authority boundary and dependency policy**

Add these sections to `skills/frontend-director/SKILL.md`:

```markdown
## Frontend authority
Within the frontend layer, you may refactor component structure and module boundaries; extract, merge, delete, or replace components; rebuild layouts and interaction flows; create or reorganize design-system primitives and tokens; and partially or completely reconstruct the presentation layer while preserving behavior and contracts.

You may install, upgrade, replace, or remove frontend dependencies automatically when there is a concrete reason such as lower complexity, better maintainability, reduced duplication, improved accessibility, better performance, stronger project fit, or removal of a material limitation. Do not replace a library merely because another option is fashionable or aesthetically preferred. Remove obsolete dependencies and dead integration code after a safe replacement.

## Protected boundaries
Do not silently change API contracts, backend behavior, business rules, infrastructure, authentication/security boundaries, or externally observable product semantics merely to simplify frontend work. If correct progress requires one of those changes and it has not been separately authorized, enter `BLOCKED` and report the precise blocker, concrete options, and the recommended option.
```

- [ ] **Step 4: Run focused tests and verify GREEN**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: all Task 1 and Task 2 tests pass.

- [ ] **Step 5: Commit Task 2**

```powershell
git add skills/frontend-director/SKILL.md tests/test_frontend_director.py
git commit -m "feat: define frontend director authority"
```

### Task 3: Specialist orchestration and reuse-router subordination

**Files:**
- Modify: `tests/test_frontend_director.py`
- Modify: `skills/frontend-director/SKILL.md`
- Modify: `skills/reuse-first-router/SKILL.md` in `Design skill catalogue policy` and `Interaction with other skills`

**Interfaces:**
- Consumes: the preferred 42-skill cohort and on-demand catalogue already defined by `reuse-first-router`.
- Produces: Lead/Support/Review role model and a no-recursion ownership rule between director and reuse router.

- [ ] **Step 1: Extend test fixtures and write failing orchestration tests**

Add at module scope:

```python
ROUTER = (ROOT / "skills" / "reuse-first-router" / "SKILL.md").read_text(encoding="utf-8")
```

Add these test methods:

```python
    def test_specialists_are_selected_on_demand_by_concrete_need(self):
        lowered = DIRECTOR.lower()
        for marker in ("lead", "support", "review", "on demand", "concrete problem"):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)
        self.assertIn("do not bulk-load", lowered)

    def test_director_resolves_specialist_conflicts(self):
        lowered = DIRECTOR.lower()
        ordered = (
            "functional requirements",
            "project identity",
            "ux and accessibility",
            "system coherence",
            "visual quality",
            "specialist preference",
        )
        positions = [lowered.index(marker) for marker in ordered]
        self.assertEqual(positions, sorted(positions))

    def test_reuse_router_is_subordinate_inside_director_tasks(self):
        lowered = ROUTER.lower()
        self.assertIn("frontend-director", lowered)
        self.assertIn("subordinate", lowered)
        self.assertIn("does not take ownership", lowered)
```

- [ ] **Step 2: Run focused tests and verify RED**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: orchestration and router-subordination tests fail.

- [ ] **Step 3: Add specialist orchestration to the director**

Add to `skills/frontend-director/SKILL.md`:

```markdown
## Specialist orchestration
Use the preferred 42-skill design cohort as the first specialist pool, but select a specialist only when a concrete problem or hypothesis it can materially address has been identified. Resolve only the selected skill on demand. Do not bulk-load or bulk-install the cohort and do not load specialists "just in case."

Assign roles deliberately:
- `Lead`: normally one specialist that drives the main transformation.
- `Support`: zero to two specialists for focused secondary concerns.
- `Review`: auditors used after implementation reaches a reviewable state.
- `Director`: permanent owner and final decision-maker.

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
```

Also include concrete capability routing in that section for at least `frontend-design`, `impeccable`, `design-tokens-skill`, `interfaces-that-feel`, `fixing-accessibility`, `cloudflare-web-perf`, `design-auditor`, and `frontend-quality-reviewer` so the director has usable first-choice examples without loading the full catalogue.

- [ ] **Step 4: Make reuse-first-router explicitly subordinate**

In `skills/reuse-first-router/SKILL.md`, add this policy near `Design skill catalogue policy`:

```markdown
When a non-trivial frontend task is already owned by `$frontend-director`, this router is subordinate discovery/reuse infrastructure. Return reuse candidates, catalogue pointers, and adoption guidance to the director; do not take ownership of the task, create a competing orchestration loop, or require the user to coordinate the selected frontend specialists manually.
```

Also add `$frontend-director` to `Interaction with other skills` with the same ownership rule.

- [ ] **Step 5: Run focused tests and verify GREEN**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: all orchestration tests pass.

- [ ] **Step 6: Commit Task 3**

```powershell
git add skills/frontend-director/SKILL.md skills/reuse-first-router/SKILL.md tests/test_frontend_director.py
git commit -m "feat: orchestrate frontend specialists on demand"
```

### Task 4: Autonomous correction, gates, states, and reviewer integration

**Files:**
- Modify: `tests/test_frontend_director.py`
- Modify: `skills/frontend-director/SKILL.md`
- Modify: `skills/frontend-quality-reviewer/SKILL.md`

**Interfaces:**
- Consumes: director ownership and specialist roles from Tasks 1-3.
- Produces: terminal-state protocol and evidence-based Definition of Done.

- [ ] **Step 1: Extend fixtures and write failing gate/state tests**

Add at module scope:

```python
REVIEWER = (ROOT / "skills" / "frontend-quality-reviewer" / "SKILL.md").read_text(encoding="utf-8")
```

Add these methods:

```python
    def test_director_has_all_formal_states_and_autonomous_revision(self):
        for marker in ("IN_PROGRESS", "NEEDS_REVISION", "BLOCKED", "APPROVED"):
            with self.subTest(marker=marker):
                self.assertIn(marker, DIRECTOR)
        lowered = DIRECTOR.lower()
        self.assertIn("continues autonomously", lowered)
        self.assertIn("change strategy", lowered)

    def test_build_green_is_not_visual_approval(self):
        lowered = DIRECTOR.lower()
        self.assertIn("build green is not visual approval", lowered)
        self.assertIn("rendered", lowered)
        self.assertIn("desktop", lowered)
        self.assertIn("mobile", lowered)

    def test_definition_of_done_covers_required_gates(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "functional gate",
            "engineering gate",
            "visual gate",
            "ux and accessibility gate",
            "performance gate",
            "transplant test",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_frontend_quality_reviewer_is_a_review_gate(self):
        lowered = REVIEWER.lower()
        self.assertIn("review gate", lowered)
        self.assertIn("frontend-director", lowered)
        self.assertNotIn("evaluating or implementing ui", lowered)
```

- [ ] **Step 2: Run focused tests and verify RED**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: gate/state tests fail and reviewer still advertises an implementation role.

- [ ] **Step 3: Implement iteration discipline, execution record, gates, and terminal protocol**

Add explicit sections to `skills/frontend-director/SKILL.md` containing all of these requirements:

```markdown
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
```

- [ ] **Step 4: Narrow frontend-quality-reviewer to a gate role**

Change its frontmatter description to:

```yaml
description: Review gate for UI, landing pages, design systems, responsive layouts, and frontend polish; when frontend-director owns the task, return findings to it rather than taking implementation ownership.
```

Add immediately below its title:

```markdown
When `$frontend-director` owns the task, act as a review gate. Report defects and acceptance status to the director; do not become the top-level coordinator or independently redirect implementation.
```

Keep its existing review areas and workflow otherwise intact.

- [ ] **Step 5: Run focused tests and verify GREEN**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: all frontend-director tests pass.

- [ ] **Step 6: Commit Task 4**

```powershell
git add skills/frontend-director/SKILL.md skills/frontend-quality-reviewer/SKILL.md tests/test_frontend_director.py
git commit -m "feat: gate frontend director approval"
```

### Task 5: Documentation, installation-path regression, and final verification

**Files:**
- Modify: `tests/test_frontend_director.py`
- Modify: `README.md` in `Skills centrais`, `Router de design`, and `Recomendação prática`
- Verify unchanged: `scripts/install.ps1`

**Interfaces:**
- Consumes: completed director policy from Tasks 1-4.
- Produces: discoverable user-facing documentation and evidence that the existing generic installer will deploy the new skill.

- [ ] **Step 1: Add failing documentation/install tests**

Add module fixtures:

```python
README = (ROOT / "README.md").read_text(encoding="utf-8")
INSTALL = (ROOT / "scripts" / "install.ps1").read_text(encoding="utf-8")
```

Add tests:

```python
    def test_readme_documents_automatic_director_route(self):
        lowered = README.lower()
        for marker in ("frontend-director", "automático", "obrigatório", "42", "on-demand"):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_existing_installer_copies_new_skill_generically(self):
        self.assertIn('$SourceSkills = Join-Path $RepoRoot "skills\\*"', INSTALL)
        self.assertIn('Copy-Item $SourceSkills $SkillsDir -Recurse -Force', INSTALL)
```

- [ ] **Step 2: Run focused tests and verify RED only on README policy**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: installer-copy test passes immediately; README automatic-director test fails.

- [ ] **Step 3: Document the director without duplicating the full skill**

Update `README.md`:

Under `Skills centrais`, add:

```markdown
- `frontend-director`: owner automático e obrigatório de frontend não trivial; diagnostica, escolhe especialistas on-demand, pode refatorar/reconstruir a camada frontend e coordena implementação + QA até `APPROVED` ou um bloqueio real fora do escopo.
```

After `Router de design`, add a concise subsection explaining that the 42 skills are no longer something the user must coordinate manually: `frontend-director` chooses Lead/Support/Review and keeps `reuse-first-router` as subordinate discovery infrastructure.

In `Recomendação prática`, replace the direct instruction to route frontend work manually through `design-agent-skills` with a rule that non-trivial frontend goes through `frontend-director` automatically; the director then uses `design-agent-skills`, the 42-skill cohort, `motion-primitives`, `watermelon-platform`, and other references only when materially needed.

- [ ] **Step 4: Run the focused suite**

Run:

```powershell
python -m unittest tests.test_frontend_director -v
```

Expected: all tests pass.

- [ ] **Step 5: Run the repository integration suite once**

Run:

```powershell
$env:PYTHONDONTWRITEBYTECODE='1'
python -m unittest discover -s tests -v
```

Expected: all existing router/revise tests plus `test_frontend_director.py` pass.

- [ ] **Step 6: Run policy hygiene checks**

Run:

```powershell
git diff --check
@'
$errors = $null
[System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path 'scripts/install.ps1'), [ref]$null, [ref]$errors) | Out-Null
if ($errors.Count -gt 0) { $errors | Format-List; exit 1 }
Write-Host 'scripts/install.ps1 syntax OK'
'@ | powershell -NoProfile -Command -
```

Expected: `git diff --check` has no output and the parser prints `scripts/install.ps1 syntax OK`.

- [ ] **Step 7: Inspect the final policy diff against the approved spec**

Run:

```powershell
git diff HEAD~4 -- global/AGENTS.md skills/frontend-director/SKILL.md skills/reuse-first-router/SKILL.md skills/frontend-quality-reviewer/SKILL.md README.md tests/test_frontend_director.py
git status --short
```

Confirm specifically that: mandatory routing is global; process detail lives in the director skill; no bulk-loading rule remains; frontend authority is broad but non-frontend boundaries are protected; `NEEDS_REVISION` loops autonomously; rendered visual evidence is required when available; and reviewer/router ownership cannot recurse.

- [ ] **Step 8: Commit Task 5**

```powershell
git add README.md tests/test_frontend_director.py
git commit -m "docs: document automatic frontend director"
```

- [ ] **Step 9: Final verification before PR**

Run fresh after the last commit:

```powershell
$env:PYTHONDONTWRITEBYTECODE='1'
python -m unittest discover -s tests -v
git diff --check HEAD~5 HEAD
git status --short
git log -5 --oneline
```

Expected: full tests green, diff check clean, working tree clean, and the five implementation commits visible.

## Self-Review Result

- **Spec coverage:** all ten implementation acceptance criteria map to Tasks 1-5. Mandatory routing/ownership is Task 1; authority and protected boundaries Task 2; on-demand specialist selection/conflict resolution Task 3; autonomous revision, gates, states, rendered evidence, and reviewer integration Task 4; documentation, installer-path evidence, and full regression verification Task 5.
- **Placeholder scan:** no `TBD`, `TODO`, unspecified error handling, or undefined implementation step remains in this plan.
- **Interface consistency:** the skill name is consistently `frontend-director`; terminal states are consistently `IN_PROGRESS`, `NEEDS_REVISION`, `BLOCKED`, and `APPROVED`; specialist roles are consistently `Lead`, `Support`, `Review`, and `Director`; the same `tests/test_frontend_director.py` fixture names are extended sequentially across tasks.
