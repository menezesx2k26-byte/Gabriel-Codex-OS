param(
  [Parameter(Mandatory = $true)]
  [string]$ProjectPath
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

New-Item -ItemType Directory -Force -Path $ProjectPath | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ProjectPath "docs\context") | Out-Null

Copy-Item (Join-Path $RepoRoot "templates\AGENTS.md") (Join-Path $ProjectPath "AGENTS.md") -Force
Copy-Item (Join-Path $RepoRoot "templates\docs\context\STATE.md") (Join-Path $ProjectPath "docs\context\STATE.md") -Force
Copy-Item (Join-Path $RepoRoot "templates\docs\context\DECISIONS.md") (Join-Path $ProjectPath "docs\context\DECISIONS.md") -Force
Copy-Item (Join-Path $RepoRoot "templates\docs\context\HANDOFF.md") (Join-Path $ProjectPath "docs\context\HANDOFF.md") -Force
Copy-Item (Join-Path $RepoRoot "templates\docs\context\ACCEPTANCE_CRITERIA.md") (Join-Path $ProjectPath "docs\context\ACCEPTANCE_CRITERIA.md") -Force

Write-Host "Bootstrapped project at $ProjectPath"
Write-Host "Next: fill in AGENTS.md and docs/context/*.md for the new repo."
