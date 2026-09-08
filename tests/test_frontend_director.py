from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
AGENTS = (ROOT / "global" / "AGENTS.md").read_text(encoding="utf-8")
DIRECTOR_PATH = ROOT / "skills" / "frontend-director" / "SKILL.md"
DIRECTOR = DIRECTOR_PATH.read_text(encoding="utf-8") if DIRECTOR_PATH.exists() else ""


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


if __name__ == "__main__":
    unittest.main()
