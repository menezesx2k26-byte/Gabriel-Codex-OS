# Persistd Two-Phase Claim Design

## Goal
Make rollover work without requiring the successor chat to run a local command. Keep CONTROL.md as the durable source of controller state and keep at most one browser tab per active run.

## Flow
Persistd prepares the next generation and creates the successor chat. The successor returns a claim request containing the run id, generation, and nonce from the baton. Persistd validates those values locally and promotes the generation using the existing claim helper.
The browser then sends a confirmation back into that same chat. Only after that confirmation may the successor continue project work. Visible chat text alone never changes controller authority.

## Failure handling
Malformed or mismatched requests do not change durable state. A failed successor before durable promotion is closed by exact chat or target identity. If durable promotion succeeds but the confirmation message fails, the promoted generation remains authoritative and persistd retries delivery instead of rolling authority back.

## Browser invariant
Each active persist task space keeps at most one live tab. Successful rollover keeps the newly promoted chat and closes siblings. Failed attempts are closed immediately. DONE still closes the whole task space.

## Scope
Change only persistd handoff, browser scripts and transport, orchestrator, controller documentation, and regression tests. Project repositories and project handoffs are not changed.