$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$CodexDir = Join-Path $HOME ".codex"
$AgentsDir = Join-Path $HOME ".agents"
$SkillsDir = Join-Path $AgentsDir "skills"
$PersistdDir = Join-Path $AgentsDir "persistd"
$VendorDir = Join-Path $AgentsDir "vendor"

New-Item -ItemType Directory -Force -Path $CodexDir | Out-Null
New-Item -ItemType Directory -Force -Path $SkillsDir | Out-Null
New-Item -ItemType Directory -Force -Path $VendorDir | Out-Null

Copy-Item (Join-Path $RepoRoot "global\AGENTS.md") (Join-Path $CodexDir "AGENTS.md") -Force

$SourceSkills = Join-Path $RepoRoot "skills\*"
Copy-Item $SourceSkills $SkillsDir -Recurse -Force

$PersistdSource = Join-Path $RepoRoot "persistd"
if (Test-Path (Join-Path $PersistdSource "package.json")) {
    New-Item -ItemType Directory -Force -Path $PersistdDir | Out-Null
    Copy-Item (Join-Path $PersistdSource "*") $PersistdDir -Recurse -Force
}

$VendorRepos = @(
    @{ Name = "awesome-harness-engineering"; Url = "https://github.com/ai-boost/awesome-harness-engineering.git" },
    @{ Name = "claude-skills"; Url = "https://github.com/alirezarezvani/claude-skills.git" },
    @{ Name = "agentmemory"; Url = "https://github.com/rohitg00/agentmemory.git" },
    @{ Name = "andrej-karpathy-skills"; Url = "https://github.com/multica-ai/andrej-karpathy-skills.git" },
    @{ Name = "ponytail"; Url = "https://github.com/DietrichGebert/ponytail.git" },
    @{ Name = "tencentdb-agent-memory"; Url = "https://github.com/TencentCloud/TencentDB-Agent-Memory.git" },
    @{ Name = "ego-lite"; Url = "https://github.com/citrolabs/ego-lite.git" },
    @{ Name = "motion-primitives"; Url = "https://github.com/ibelick/motion-primitives.git" },
    @{ Name = "watermelon-platform"; Url = "https://github.com/WatermelonCorp/watermelon-platform.git" },
    @{ Name = "fontsource"; Url = "https://github.com/fontsource/fontsource.git" },
    @{ Name = "OpenManus"; Url = "https://github.com/FoundationAgents/OpenManus.git" },
    @{ Name = "skill-manus"; Url = "https://github.com/reubenjohn/skill-manus.git" }
)

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Warning "Git is not available; skipping vendor toolkit sync."
} else {
    foreach ($Repo in $VendorRepos) {
        $Destination = Join-Path $VendorDir $Repo.Name
        if (Test-Path (Join-Path $Destination ".git")) {
            Write-Host "Updating vendor reference: $($Repo.Name)"
            git -C $Destination fetch --depth 1 origin | Out-Host
            git -C $Destination reset --hard origin/HEAD | Out-Host
        } elseif (Test-Path $Destination) {
            Write-Warning "Skipping $($Repo.Name): destination exists but is not a Git repository: $Destination"
        } else {
            Write-Host "Installing vendor reference: $($Repo.Name)"
            git clone --depth 1 --filter=blob:none $Repo.Url $Destination | Out-Host
        }
    }
}

Write-Host "Installed global AGENTS.md to $CodexDir"
Write-Host "Installed skills to $SkillsDir"
Write-Host "Installed versioned persistd runtime to $PersistdDir"
Write-Host "Synced reuse-first vendor toolkit to $VendorDir"
Write-Host "Vendor repositories are references/candidate sources; they are not automatically loaded into every prompt."
Write-Host "Haikei remains a web/reference-only router option until a verified official Git repository exists."
Write-Host "Start a new Codex session to load updates."
