---
name: research-verifier
description: Use when documentation, specs, APIs, or external references influence implementation decisions.
---

# Research Verifier

Use external information carefully and only when it materially improves correctness.

## Rules
- Prefer official documentation for product or API behavior.
- Distinguish clearly between current, verified facts and assumptions.
- Do not over-research when the repository already contains the answer.
- If the answer is time-sensitive, verify freshness.

## Workflow
1. Identify what must be verified.
2. Check official sources first.
3. Extract only the information relevant to the implementation decision.
4. Cite or summarize the source in a way that is useful to the repo/user.
5. Convert research into concrete implementation guidance.

## Output
Report:
- what was verified
- source quality
- what changed in the plan because of verification
