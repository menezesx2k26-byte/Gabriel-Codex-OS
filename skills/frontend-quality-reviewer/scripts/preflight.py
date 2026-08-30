#!/usr/bin/env python3
import argparse
import json
from pathlib import Path

REQUIRED_SKILLS = {
    "frontend-quality-reviewer": "frontend-quality-reviewer/SKILL.md",
    "reuse-first-router": "reuse-first-router/SKILL.md",
    "taste-skill": "taste-skill/SKILL.md",
    "impeccable": "impeccable/SKILL.md",
    "emil-design-eng": "emil-design-eng/SKILL.md",
}

THREE_KEYS = ("three.js", "threejs", "3d", "webgl", "webgpu", "webxr", "r3f", "react-three-fiber")
GSAP_KEYS = ("gsap", "scrolltrigger", "pinning", "scrub", "scroll timeline", "timeline choreography")
MOTION_KEYS = ("motion", "framer motion", "spring", "gesture", "layout animation", "enter/exit", "drag animation")
SIMPLE_KEYS = ("hover", "fade", "transition", "reveal", "opacity", "microinteraction")


def read_dependencies(repo: Path):
    package = repo / "package.json"
    if not package.exists():
        return set()
    try:
        data = json.loads(package.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return set()
    deps = {}
    for key in ("dependencies", "devDependencies", "peerDependencies", "optionalDependencies"):
        value = data.get(key, {})
        if isinstance(value, dict):
            deps.update(value)
    return set(deps)


def choose_animation_route(intent: str, deps: set[str]):
    text = intent.lower()
    if any(key in text for key in THREE_KEYS):
        if "@react-three/fiber" in deps:
            return "Three.js via existing @react-three/fiber"
        return "Three.js candidate — real 3D only"
    if any(key in text for key in GSAP_KEYS):
        return "GSAP candidate — complex timeline/scroll choreography"
    if any(key in text for key in MOTION_KEYS):
        if "motion" in deps or "framer-motion" in deps:
            return "Motion — reuse incumbent dependency"
        return "Motion candidate — component/layout/gesture motion"
    if any(key in text for key in SIMPLE_KEYS):
        return "CSS/WAAPI first — keep runtime minimal"
    return "none by default — Emil decides whether motion has a job"


def main():
    parser = argparse.ArgumentParser(description="Frontend/UI tool activation preflight")
    parser.add_argument("--repo", required=True)
    parser.add_argument("--intent", default="")
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args()
    repo = Path(args.repo).expanduser().resolve()
    skills_root = Path.home() / ".agents" / "skills"

    skill_status = {}
    missing = []
    for name, rel in REQUIRED_SKILLS.items():
        path = skills_root / rel
        exists = path.is_file()
        skill_status[name] = {"path": str(path), "available": exists}
        if not exists:
            missing.append(name)

    deps = read_dependencies(repo) if repo.is_dir() else set()
    result = {
        "status": "PASS" if repo.is_dir() and not missing else "BLOCKED",
        "repo": str(repo),
        "repo_exists": repo.is_dir(),
        "skills": skill_status,
        "missing_skills": missing,
        "animation_route": choose_animation_route(args.intent, deps),
        "detected_animation_deps": sorted(deps & {"motion", "framer-motion", "gsap", "three", "@react-three/fiber"}),
        "next": "invoke specialists in staged order; preflight is not the invocation",
    }

    if args.json:
        print(json.dumps(result, indent=2, ensure_ascii=False))
    else:
        print(f"UI_TOOL_PREFLIGHT {result['status']}")
        print(f"repo: {result['repo']}")
        print(f"animation_route: {result['animation_route']}")
        if result["detected_animation_deps"]:
            print("detected_animation_deps: " + ", ".join(result["detected_animation_deps"]))
        for name, info in result["skills"].items():
            state = "READY" if info["available"] else "MISSING"
            print(f"skill[{name}]: {state} {info['path']}")
        print(result["next"])

    raise SystemExit(0 if result["status"] == "PASS" else 2)


if __name__ == "__main__":
    main()
