from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
INSTALLER = (ROOT / "scripts" / "install.ps1").read_text(encoding="utf-8")
README = (ROOT / "README.md").read_text(encoding="utf-8")
REVISE = ROOT / "skills" / "revise" / "SKILL.md"


class ReviseAliasTests(unittest.TestCase):
    def test_revise_skill_exists_and_is_slash_alias(self):
        self.assertTrue(REVISE.exists())
        text = REVISE.read_text(encoding="utf-8")
        self.assertIn("name: revise", text)
        self.assertIn("/revise", text)

    def test_revise_contract_has_required_review_layers(self):
        text = REVISE.read_text(encoding="utf-8")
        for marker in ["PR-Agent", "reviewdog", "OpenReviewer"]:
            with self.subTest(marker=marker):
                self.assertIn(marker, text)
        self.assertIn("refut", text.lower())
        self.assertIn("INCONCLUSIVE", text)

    def test_installer_tracks_reviewer_vendor_repositories(self):
        expected = {
            "pr-agent": "https://github.com/The-PR-Agent/pr-agent.git",
            "reviewdog": "https://github.com/reviewdog/reviewdog.git",
            "OpenReviewer": "https://github.com/Ascent-AI-org/OpenReviewer.git",
        }
        for name, url in expected.items():
            with self.subTest(name=name):
                self.assertIn(name, INSTALLER)
                self.assertIn(url, INSTALLER)

    def test_readme_documents_revise_pipeline(self):
        self.assertIn("`/revise`", README)
        self.assertIn("PR-Agent", README)
        self.assertIn("reviewdog", README)
        self.assertIn("OpenReviewer", README)


if __name__ == "__main__":
    unittest.main()
