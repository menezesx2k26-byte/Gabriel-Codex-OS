$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$CodexDir = Join-Path $HOME ".codex"
$AgentsDir = Join-Path $HOME ".agents"
$SkillsDir = Join-Path $AgentsDir "skills"

New-Item -ItemType Directory -Force -Path $CodexDir | Out-Null
New-Item -ItemType Directory -Force -Path $SkillsDir | Out-Null

Copy-Item (Join-Path $RepoRoot "global\AGENTS.md") (Join-Path $CodexDir "AGENTS.md") -Force

$SourceSkills = Join-Path $RepoRoot "skills\*"
Copy-Item $SourceSkills $SkillsDir -Recurse -Force

Write-Host "Installed global AGENTS.md to $CodexDir"
Write-Host "Installed skills to $SkillsDir"
Write-Host "Start a new Codex session to load updates."
