from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")
INSTALL = (ROOT / "scripts" / "install.ps1").read_text(encoding="utf-8")
AGENTS = (ROOT / "global" / "AGENTS.md").read_text(encoding="utf-8")
DIRECTOR_PATH = ROOT / "skills" / "frontend-director" / "SKILL.md"
DIRECTOR = DIRECTOR_PATH.read_text(encoding="utf-8") if DIRECTOR_PATH.exists() else ""
ROUTER = (ROOT / "skills" / "reuse-first-router" / "SKILL.md").read_text(encoding="utf-8")
REVIEWER = (ROOT / "skills" / "frontend-quality-reviewer" / "SKILL.md").read_text(encoding="utf-8")


class FrontendDirectorTests(unittest.TestCase):
    def test_frontend_director_skill_exists(self):
        self.assertTrue(DIRECTOR_PATH.exists())
        self.assertIn("name: frontend-director", DIRECTOR)

    def test_non_trivial_frontend_work_routes_automatically(self):
        lowered = AGENTS.lower()
        self.assertIn("$frontend-director", lowered)
        self.assertIn("non-trivial frontend", lowered)
        self.assertIn("mandatory", lowered)
        self.assertIn("even when", lowered)

    def test_director_keeps_top_level_ownership(self):
        lowered = DIRECTOR.lower()
        self.assertIn("top-level owner", lowered)
        self.assertIn("approved", lowered)
        self.assertIn("blocked", lowered)

    def test_director_can_rebuild_and_manage_dependencies(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "refactor component structure",
            "rebuild",
            "install",
            "upgrade",
            "replace",
            "remove frontend dependencies",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_director_protects_non_frontend_contracts(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "api contracts",
            "backend",
            "business rules",
            "infrastructure",
            "authentication/security",
            "blocked",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_dependency_churn_requires_a_concrete_reason(self):
        lowered = DIRECTOR.lower()
        self.assertIn("concrete reason", lowered)
        self.assertIn("fashionable", lowered)
        self.assertIn("obsolete dependencies", lowered)

    def test_specialists_are_selected_on_demand_by_concrete_need(self):
        lowered = DIRECTOR.lower()
        for marker in ("lead", "support", "review", "on demand", "concrete problem"):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)
        self.assertIn("do not bulk-load", lowered)

    def test_director_resolves_specialist_conflicts(self):
        lowered = DIRECTOR.lower()
        ordered = (
            "functional requirements",
            "project identity",
            "ux and accessibility",
            "system coherence",
            "visual quality",
            "specialist preference",
        )
        positions = [lowered.index(marker) for marker in ordered]
        self.assertEqual(positions, sorted(positions))

    def test_reuse_router_is_subordinate_inside_director_tasks(self):
        lowered = ROUTER.lower()
        catalogue_policy = lowered.partition("## design skill catalogue policy")[2].partition(
            "## existing composed baselines"
        )[0]
        interactions = lowered.partition("## interaction with other skills")[2].partition(
            "## completion rule"
        )[0]

        for section_name, section in (
            ("design skill catalogue policy", catalogue_policy),
            ("interaction with other skills", interactions),
        ):
            for marker in (
                "frontend-director",
                "subordinate",
                "do not take ownership",
                "competing orchestration loop",
                "require the user to coordinate",
            ):
                with self.subTest(section=section_name, marker=marker):
                    self.assertIn(marker, section)

    def test_director_has_all_formal_states_and_autonomous_revision(self):
        for marker in ("IN_PROGRESS", "NEEDS_REVISION", "BLOCKED", "APPROVED"):
            with self.subTest(marker=marker):
                self.assertIn(marker, DIRECTOR)
        lowered = DIRECTOR.lower()
        self.assertIn("continues autonomously", lowered)
        self.assertIn("change strategy", lowered)

    def test_build_green_is_not_visual_approval(self):
        lowered = DIRECTOR.lower()
        self.assertIn("build green is not visual approval", lowered)
        self.assertIn("rendered", lowered)
        self.assertIn("desktop", lowered)
        self.assertIn("mobile", lowered)

    def test_definition_of_done_covers_required_gates(self):
        lowered = DIRECTOR.lower()
        for marker in (
            "functional gate",
            "engineering gate",
            "visual gate",
            "ux and accessibility gate",
            "performance gate",
            "transplant test",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_frontend_quality_reviewer_is_a_review_gate(self):
        lowered = REVIEWER.lower()
        self.assertIn("review gate", lowered)
        self.assertIn("frontend-director", lowered)
        self.assertNotIn("evaluating or implementing ui", lowered)

    def test_readme_documents_automatic_director_route(self):
        lowered = README.lower()
        for marker in ("frontend-director", "automático", "obrigatório", "42", "on-demand"):
            with self.subTest(marker=marker):
                self.assertIn(marker, lowered)

    def test_existing_installer_copies_new_skill_generically(self):
        self.assertIn('$SourceSkills = Join-Path $RepoRoot "skills\\*"', INSTALL)
        self.assertIn('Copy-Item $SourceSkills $SkillsDir -Recurse -Force', INSTALL)


if __name__ == "__main__":
    unittest.main()
