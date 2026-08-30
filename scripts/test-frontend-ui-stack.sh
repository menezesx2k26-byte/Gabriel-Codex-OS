#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
failures=0

require_text() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if ! grep -Fq -- "$pattern" "$ROOT/$file"; then
    printf 'FAIL %s: missing %s in %s\n' "$label" "$pattern" "$file" >&2
    failures=$((failures + 1))
  else
    printf 'PASS %s\n' "$label"
  fi
}

require_text "global/AGENTS.md" '$frontend-quality-reviewer' "global frontend routing"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Taste Skill" "taste phase"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Impeccable" "impeccable phase"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Emil Kowalski" "emil phase"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Playwright" "playwright phase"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Three.js" "threejs implementation routing"
require_text "skills/frontend-quality-reviewer/SKILL.md" "Motion" "motion implementation routing"
require_text "skills/frontend-quality-reviewer/SKILL.md" "GSAP" "gsap implementation routing"
require_text "skills/frontend-quality-reviewer/SKILL.md" "UI_ANTI_VIBECODE.md" "local anti-vibecode authority"
require_text "skills/reuse-first-router/SKILL.md" "impeccable" "impeccable vendor routing"
require_text "skills/reuse-first-router/SKILL.md" "taste-skill" "taste vendor routing"
require_text "skills/reuse-first-router/SKILL.md" "emil-skills" "emil vendor routing"
require_text "skills/reuse-first-router/SKILL.md" "playwright-mcp" "playwright MCP vendor routing"
require_text "scripts/install.ps1" 'Name = "impeccable"' "installer impeccable vendor"
require_text "scripts/install.ps1" 'Name = "taste-skill"' "installer taste vendor"
require_text "scripts/install.ps1" 'Name = "emil-skills"' "installer emil vendor"
require_text "scripts/install.ps1" 'https://github.com/emilkowalski/skills.git' "installer emil canonical url"
require_text "scripts/install.ps1" 'Name = "playwright-mcp"' "installer playwright MCP vendor"
require_text "scripts/install.ps1" 'SparsePaths = @(".agents/skills/impeccable")' "installer sparse impeccable"
require_text "scripts/install.ps1" "sparse-checkout set" "installer sparse checkout support"

if (( failures > 0 )); then
  printf '\n%d frontend UI stack check(s) failed.\n' "$failures" >&2
  exit 1
fi

printf '\nFrontend UI stack contract PASS.\n'
