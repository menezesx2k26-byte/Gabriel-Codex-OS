from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
INSTALL = (ROOT / "scripts" / "install.ps1").read_text(encoding="utf-8")
ROUTER = (ROOT / "skills" / "reuse-first-router" / "SKILL.md").read_text(encoding="utf-8")
README = (ROOT / "README.md").read_text(encoding="utf-8")

VENDORS = {
    "motion-primitives": "https://github.com/ibelick/motion-primitives.git",
    "watermelon-platform": "https://github.com/WatermelonCorp/watermelon-platform.git",
    "OpenManus": "https://github.com/FoundationAgents/OpenManus.git",
    "skill-manus": "https://github.com/reubenjohn/skill-manus.git",
}


class ReuseRouterVendorTests(unittest.TestCase):
    def test_installer_tracks_all_new_vendor_repositories(self):
        for name, url in VENDORS.items():
            with self.subTest(name=name):
                self.assertIn(f'Name = "{name}"', INSTALL)
                self.assertIn(f'Url = "{url}"', INSTALL)

    def test_router_exposes_visual_and_manus_routes(self):
        expected = [
            "motion-primitives",
            "watermelon-platform",
            "OpenManus",
            "skill-manus",
            "haikei.app",
        ]
        for marker in expected:
            with self.subTest(marker=marker):
                self.assertIn(marker, ROUTER)

    def test_readme_documents_new_routes_and_haikei_policy(self):
        for marker in ["motion-primitives", "watermelon-platform", "OpenManus", "skill-manus", "haikei.app"]:
            with self.subTest(marker=marker):
                self.assertIn(marker, README)
        self.assertIn("web/reference-only", README)


if __name__ == "__main__":
    unittest.main()
