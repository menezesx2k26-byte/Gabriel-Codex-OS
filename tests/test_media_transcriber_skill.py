from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
SKILL = ROOT / "skills" / "media-transcriber"
README = (ROOT / "README.md").read_text(encoding="utf-8")
SKILL_TEXT = (SKILL / "SKILL.md").read_text(encoding="utf-8") if (SKILL / "SKILL.md").exists() else ""


class MediaTranscriberSkillPackagingTests(unittest.TestCase):
    def test_media_transcriber_is_versioned_as_a_complete_skill(self):
        expected = [
            SKILL / "SKILL.md",
            SKILL / "scripts" / "media_transcriber.py",
            SKILL / "scripts" / "bootstrap_media_transcriber.ps1",
            SKILL / "scripts" / "transcribe_media.ps1",
            SKILL / "tests" / "test_media_transcriber.py",
        ]
        for path in expected:
            with self.subTest(path=path.name):
                self.assertTrue(path.exists(), f"missing versioned skill file: {path}")

    def test_readme_lists_media_transcriber_as_a_central_skill(self):
        self.assertIn("media-transcriber", README)

    def test_skill_documents_authenticated_fallback_and_subtitle_fast_path(self):
        for marker in (
            "yt-dlp-firefox",
            "--cookies-from-browser firefox",
            "legendas",
            "antes do ASR",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, SKILL_TEXT)

    def test_skill_description_is_trigger_only(self):
        description = SKILL_TEXT.split("description:", 1)[1].splitlines()[0].strip()
        self.assertTrue(description.startswith("Use when"))
        self.assertNotIn("Prefer", description)


if __name__ == "__main__":
    unittest.main()
