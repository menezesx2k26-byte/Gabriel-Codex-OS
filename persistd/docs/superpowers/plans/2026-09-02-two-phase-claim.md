# Persistd Two-Phase Claim Implementation Plan

**Goal:** Move generation promotion into the local persistd runtime while keeping CONTROL.md authoritative and limiting each active run to one browser tab.

**Architecture:** The successor returns a structured request. Persistd validates it, performs the existing local promotion, then sends confirmation to the same chat. Failures before promotion close the attempt; failures after promotion preserve the promoted generation.

**Tech Stack:** Node.js, node:test, ego-browser, CONTROL.md.

**Spec:** docs/superpowers/specs/2026-09-02-two-phase-claim-design.md

## Constraints
- CONTROL.md is authoritative.
- UI text alone does not promote a generation.
- Active task spaces keep at most one live tab.
- This installed runtime has no .git directory, so create backups before edits.

### Task 1: Browser request and confirmation

Files: src/handoff.js, src/browser/ego-script.js, src/browser/ego-browser.js, chat-lifecycle.test.js.

- [ ] Add failing tests for the structured request in the successor response.
- [ ] Add failing tests for exact run id, generation, nonce, chat id, and target id.
- [ ] Add failing tests for confirmation delivery to the same chat.
- [ ] Run the focused tests and confirm failure.
- [ ] Implement the minimum request parsing and confirmation transport.
- [ ] Run the focused tests and confirm success.

### Task 2: Local durable promotion

Files: src/orchestrator.js, src/claim-generation.js, chat-lifecycle.test.js.

- [ ] Add failing tests that reject mismatched request data without changing CONTROL.md.
- [ ] Add failing tests that valid data calls the existing local promotion and advances exactly one generation.
- [ ] Add a failing test that confirmation failure does not undo a completed durable promotion.
- [ ] Run the focused tests and confirm failure.
- [ ] Implement local validation, promotion, and confirmation state handling.
- [ ] Run the focused tests and confirm success.