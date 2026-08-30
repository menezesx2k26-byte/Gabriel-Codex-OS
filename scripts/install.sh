#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CODEX_DIR="$HOME/.codex"
AGENTS_DIR="$HOME/.agents"
SKILLS_DIR="$AGENTS_DIR/skills"
VENDOR_DIR="$AGENTS_DIR/vendor"

mkdir -p "$CODEX_DIR" "$SKILLS_DIR" "$VENDOR_DIR"
cp "$REPO_ROOT/global/AGENTS.md" "$CODEX_DIR/AGENTS.md"
cp -a "$REPO_ROOT/skills/." "$SKILLS_DIR/"

sync_vendor() {
  local name="$1" url="$2"
  shift 2
  local dest="$VENDOR_DIR/$name"
  if [[ -d "$dest/.git" ]]; then
    echo "Updating vendor reference: $name"
    git -C "$dest" fetch --depth 1 origin
    if (($#)); then
      git -C "$dest" sparse-checkout init --cone
      git -C "$dest" sparse-checkout set "$@"
    fi
    git -C "$dest" reset --hard origin/HEAD
  elif [[ -e "$dest" ]]; then
    echo "Skipping $name: destination exists but is not a Git repo: $dest" >&2
  else
    echo "Installing vendor reference: $name"
    if (($#)); then
      git clone --depth 1 --filter=blob:none --sparse "$url" "$dest"
      git -C "$dest" sparse-checkout set "$@"
    else
      git clone --depth 1 --filter=blob:none "$url" "$dest"
    fi
  fi
}

promote_skill() {
  local repo="$1" source="$2" target="$3"
  local src="$VENDOR_DIR/$repo/$source"
  local dest="$SKILLS_DIR/$target"
  [[ -f "$src/SKILL.md" ]] || { echo "Missing vendor skill source: $src" >&2; return 1; }
  echo "Installing discoverable vendor skill: $target"
  rm -rf "$dest"
  cp -a "$src" "$dest"
}

command -v git >/dev/null || { echo "git is required" >&2; exit 2; }

sync_vendor awesome-harness-engineering https://github.com/ai-boost/awesome-harness-engineering.git
sync_vendor claude-skills https://github.com/alirezarezvani/claude-skills.git
sync_vendor agentmemory https://github.com/rohitg00/agentmemory.git
sync_vendor andrej-karpathy-skills https://github.com/multica-ai/andrej-karpathy-skills.git
sync_vendor ponytail https://github.com/DietrichGebert/ponytail.git
sync_vendor tencentdb-agent-memory https://github.com/TencentCloud/TencentDB-Agent-Memory.git
sync_vendor ego-lite https://github.com/citrolabs/ego-lite.git
sync_vendor impeccable https://github.com/pbakaus/impeccable.git .agents/skills/impeccable
sync_vendor taste-skill https://github.com/Leonxlnx/taste-skill.git skills/taste-skill skills/gpt-tasteskill
sync_vendor emil-skills https://github.com/emilkowalski/skills.git skills/emil-design-eng
sync_vendor playwright-mcp https://github.com/microsoft/playwright-mcp.git
sync_vendor originkit https://github.com/vellum-ai/originkit.git

promote_skill taste-skill skills/taste-skill taste-skill
promote_skill taste-skill skills/gpt-tasteskill gpt-tasteskill
promote_skill impeccable .agents/skills/impeccable impeccable
promote_skill emil-skills skills/emil-design-eng emil-design-eng

echo "Installed global AGENTS.md to $CODEX_DIR"
echo "Installed project and promoted vendor skills to $SKILLS_DIR"
echo "Synced reuse-first vendor toolkit to $VENDOR_DIR"
echo "Start a new Codex session to load updates."
