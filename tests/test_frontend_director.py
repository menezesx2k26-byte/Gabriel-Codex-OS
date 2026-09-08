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


if __name__ == "__main__":
    unittest.main()
