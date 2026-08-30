$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$CodexDir = Join-Path $HOME ".codex"
$AgentsDir = Join-Path $HOME ".agents"
$SkillsDir = Join-Path $AgentsDir "skills"
$VendorDir = Join-Path $AgentsDir "vendor"

New-Item -ItemType Directory -Force -Path $CodexDir | Out-Null
New-Item -ItemType Directory -Force -Path $SkillsDir | Out-Null
New-Item -ItemType Directory -Force -Path $VendorDir | Out-Null

Copy-Item (Join-Path $RepoRoot "global\AGENTS.md") (Join-Path $CodexDir "AGENTS.md") -Force

$SourceSkills = Join-Path $RepoRoot "skills\*"
Copy-Item $SourceSkills $SkillsDir -Recurse -Force

$VendorRepos = @(
    @{ Name = "awesome-harness-engineering"; Url = "https://github.com/ai-boost/awesome-harness-engineering.git" },
    @{ Name = "claude-skills"; Url = "https://github.com/alirezarezvani/claude-skills.git" },
    @{ Name = "agentmemory"; Url = "https://github.com/rohitg00/agentmemory.git" },
    @{ Name = "andrej-karpathy-skills"; Url = "https://github.com/multica-ai/andrej-karpathy-skills.git" },
    @{ Name = "ponytail"; Url = "https://github.com/DietrichGebert/ponytail.git" },
    @{ Name = "tencentdb-agent-memory"; Url = "https://github.com/TencentCloud/TencentDB-Agent-Memory.git" },
    @{ Name = "ego-lite"; Url = "https://github.com/citrolabs/ego-lite.git" },
    @{ Name = "impeccable"; Url = "https://github.com/pbakaus/impeccable.git"; SparsePaths = @(".agents/skills/impeccable") },
    @{ Name = "taste-skill"; Url = "https://github.com/Leonxlnx/taste-skill.git"; SparsePaths = @("skills/taste-skill", "skills/gpt-tasteskill") },
    @{ Name = "emil-skills"; Url = "https://github.com/emilkowalski/skills.git"; SparsePaths = @("skills/emil-design-eng") },
    @{ Name = "playwright-mcp"; Url = "https://github.com/microsoft/playwright-mcp.git" },
    @{ Name = "originkit"; Url = "https://github.com/vellum-ai/originkit.git" }
)

$VendorSkills = @(
    @{ Repo = "taste-skill"; Source = "skills\taste-skill"; Target = "taste-skill" },
    @{ Repo = "taste-skill"; Source = "skills\gpt-tasteskill"; Target = "gpt-tasteskill" },
    @{ Repo = "impeccable"; Source = ".agents\skills\impeccable"; Target = "impeccable" },
    @{ Repo = "emil-skills"; Source = "skills\emil-design-eng"; Target = "emil-design-eng" }
)

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Warning "Git is not available; skipping vendor toolkit sync."
} else {
    foreach ($Repo in $VendorRepos) {
        $Destination = Join-Path $VendorDir $Repo.Name
        if (Test-Path (Join-Path $Destination ".git")) {
            Write-Host "Updating vendor reference: $($Repo.Name)"
            git -C $Destination fetch --depth 1 origin | Out-Host
            if ($Repo.ContainsKey("SparsePaths")) {
                git -C $Destination sparse-checkout init --cone | Out-Host
                git -C $Destination sparse-checkout set $Repo.SparsePaths | Out-Host
            }
            git -C $Destination reset --hard origin/HEAD | Out-Host
        } elseif (Test-Path $Destination) {
            Write-Warning "Skipping $($Repo.Name): destination exists but is not a Git repository: $Destination"
        } else {
            Write-Host "Installing vendor reference: $($Repo.Name)"
            if ($Repo.ContainsKey("SparsePaths")) {
                git clone --depth 1 --filter=blob:none --sparse $Repo.Url $Destination | Out-Host
                git -C $Destination sparse-checkout set $Repo.SparsePaths | Out-Host
            } else {
                git clone --depth 1 --filter=blob:none $Repo.Url $Destination | Out-Host
            }
        }
    }
}

foreach ($Skill in $VendorSkills) {
    $Source = Join-Path (Join-Path $VendorDir $Skill.Repo) $Skill.Source
    $Target = Join-Path $SkillsDir $Skill.Target
    if (Test-Path $Source) {
        Write-Host "Installing discoverable vendor skill: $($Skill.Target)"
        if (Test-Path $Target) { Remove-Item $Target -Recurse -Force }
        Copy-Item $Source $Target -Recurse -Force
    } else {
        Write-Warning "Vendor skill source missing: $Source"
    }
}

Write-Host "Installed global AGENTS.md to $CodexDir"
Write-Host "Installed skills to $SkillsDir"
Write-Host "Synced reuse-first vendor toolkit to $VendorDir"
Write-Host "Vendor repositories are references/candidate sources; they are not automatically loaded into every prompt."
Write-Host "Start a new Codex session to load updates."
