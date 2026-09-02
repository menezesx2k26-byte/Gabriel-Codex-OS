---
name: revise
description: Use when the user sends /revise, starts a message with /revise, or asks for a rigorous repository/code review with verified findings.
---

# Revise

`/revise` is the repository review orchestrator. It is independent from `/persist`; persistence may supervise a review, but neither alias changes the other's contract.

## Required sources

Use the installed vendor copies when available:
- `~/.agents/vendor/pr-agent` — **PR-Agent** (`The-PR-Agent/pr-agent`) for AI PR/diff review.
- `~/.agents/vendor/reviewdog` — **reviewdog** (`reviewdog/reviewdog`) for deterministic linter/analyzer findings filtered to the relevant diff.
- `~/.agents/vendor/OpenReviewer` — **OpenReviewer** (`Ascent-AI-org/OpenReviewer`) for the refute-or-drop second-pass pattern.

These vendors are tools/reference implementations, not automatic authority. Never execute third-party code blindly: inspect project instructions, available commands, configuration, credentials, and compatibility first.

## Review target

1. Identify the repository/worktree and preserve pre-existing changes.
2. Read `AGENTS.md` and relevant project instructions before reviewing.
3. Prefer the user's explicit target. Otherwise use the active PR; if none exists, review the current branch/diff against its best-established base.
4. Do not silently broaden the review to unrelated historical code.

## Pipeline

1. Inventory the diff, changed files, tests, CI configuration, and existing linters/static analyzers.
2. Run the repository's own deterministic checks first. Use reviewdog when it is available and materially improves diff-scoped reporting.
3. Run tests/build/type checks/security checks appropriate to the changed surface. Never invent a check the project cannot actually run.
4. Use PR-Agent when compatible to generate semantic findings about correctness, regressions, architecture, maintainability, tests, and security.
5. For every material AI finding, perform a second independent refutation pass modeled on OpenReviewer's refute-or-drop rule. Try to disprove the finding using repository evidence, tests, call sites, configuration, and documented behavior.
6. Drop unsupported findings. A plausible concern is not a confirmed defect.
7. If the user authorized fixes, make only safe in-scope corrections, then rerun the checks that could prove the fix and the full relevant suite.

If a vendor cannot run because the target is local-only, lacks credentials, requires an unavailable runtime, or is otherwise incompatible, do not fake execution. Reproduce that layer's review method with available tools where possible and mark the unavailable external execution explicitly.

## Finding standard

Each blocking finding must include:
- severity and affected file/location;
- concrete failure mode;
- evidence from the repository or a reproducible check;
- result of the refutation attempt;
- smallest justified remediation.

Prioritize correctness, regressions, security, data loss, concurrency/state bugs, broken contracts, and missing tests for changed behavior. Suppress style-only noise unless the repository explicitly makes it a gate.

## Verdict

Use exactly one final verdict:
- `PASS` — no material confirmed findings and required verification succeeded.
- `PASS WITH NOTES` — no blocking finding; non-blocking improvements remain.
- `FAIL` — at least one material finding survived refutation with evidence, or a required deterministic gate failed.
- `INCONCLUSIVE` — required evidence/checks could not be obtained, so approval would be unsafe.

Never return `PASS` merely because PR-Agent, reviewdog, OpenReviewer, or another agent reported success. Repository evidence and fresh verification are authoritative.

## Safety boundaries

Review is read-only by default. `/revise` does not authorize merge, deploy, destructive cleanup, secret access, or scope expansion. If fixes are explicitly requested or clearly part of the active coding task, preserve unrelated work and follow the repository's normal change/verification workflow.
