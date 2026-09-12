import importlib.util
import json
import subprocess
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MODULE = ROOT / "scripts" / "media_transcriber.py"


class MediaTranscriberTests(unittest.TestCase):
    def load_module(self):
        spec = importlib.util.spec_from_file_location("media_transcriber", MODULE)
        self.assertIsNotNone(spec)
        self.assertIsNotNone(spec.loader)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        return module

    def test_classifies_url_and_local_file(self):
        module = self.load_module()
        self.assertEqual(module.classify_source("https://instagram.com/reel/abc"), "url")
        self.assertEqual(module.classify_source(r"C:\\tmp\\clip.mp4"), "file")

    def test_instagram_prefers_snapinsta_with_ytdlp_fallback(self):
        module = self.load_module()
        self.assertTrue(module.is_instagram_url("https://www.instagram.com/reel/abc/"))
        self.assertEqual(module.acquisition_order("https://instagram.com/reel/abc"), ["snapinsta", "yt-dlp", "yt-dlp-firefox"])
        self.assertEqual(module.acquisition_order("https://youtube.com/watch?v=abc"), ["yt-dlp"])

    def test_snapinsta_curl_commands_have_hard_timeouts(self):
        module = self.load_module()
        api = module.build_snapinsta_api_command("curl.exe")
        media = module.build_media_download_command("curl.exe", "https://cdn.test/a.mp4", "out.mp4")
        self.assertIn("--connect-timeout", api)
        self.assertIn("--max-time", api)
        self.assertIn("@-", api)
        self.assertIn("--max-time", media)
        self.assertIn("out.mp4", media)

    def test_snapinsta_parser_prefers_video_media(self):
        module = self.load_module()
        payload = {
            "status": True,
            "media": [
                {"url": "https://cdn.test/image.jpg", "fileType": "image/jpeg"},
                {"url": "https://cdn.test/video.mp4", "fileType": "video/mp4"},
            ],
        }
        self.assertEqual(module.select_snapinsta_media_url(payload), "https://cdn.test/video.mp4")

    def test_chunk_windows_cover_duration_without_gaps(self):
        module = self.load_module()
        self.assertEqual(
            module.build_chunk_windows(40.19, chunk_seconds=20.0),
            [(0.0, 20.0), (20.0, 20.0), (40.0, 0.19)],
        )

    def test_chunk_windows_do_not_loop_on_sub_millisecond_remainder(self):
        code = (
            "import importlib.util,json; from pathlib import Path; "
            f"p=Path(r'{MODULE}'); s=importlib.util.spec_from_file_location('mt',p); "
            "m=importlib.util.module_from_spec(s); s.loader.exec_module(m); "
            "print(json.dumps(m.build_chunk_windows(40.192336,20.0)))"
        )
        result = subprocess.run([sys.executable, "-c", code], capture_output=True, text=True, timeout=2)
        self.assertEqual(json.loads(result.stdout), [[0.0, 20.0], [20.0, 20.0], [40.0, 0.192]])

    def test_he_aac_remux_command_copies_audio_without_decoding(self):
        module = self.load_module()
        cmd = module.build_audio_remux_command("ffmpeg.exe", "source.mp4", "audio.m4a")
        self.assertIn("-vn", cmd)
        self.assertIn("copy", cmd)
        self.assertLess(cmd.index("-c:a"), cmd.index("copy"))

    def test_he_aac_is_preemptively_chunked(self):
        module = self.load_module()
        self.assertTrue(module.needs_chunked_decode("aac", "HE-AAC"))
        self.assertTrue(module.needs_chunked_decode("aac", "HE-AACv2"))
        self.assertFalse(module.needs_chunked_decode("aac", "LC"))

    def test_quantized_small_is_default_model(self):
        module = self.load_module()
        self.assertEqual(module.DEFAULT_MODEL.name, "ggml-small-q5_1.bin")

    def test_runtime_prefers_vendor_gyan_ffmpeg_when_present(self):
        module = self.load_module()
        stable = module.VENDOR / "bin" / "ffmpeg.exe"
        if stable.exists():
            self.assertEqual(Path(module.resolve_tools()["ffmpeg"]), stable)

    def test_bootstrap_defaults_to_quantized_small(self):
        bootstrap = (ROOT / "scripts" / "bootstrap_media_transcriber.ps1").read_text(encoding="utf-8-sig")
        self.assertIn("[string]$Model = 'small-q5_1'", bootstrap)
        self.assertIn("'small-q5_1'", bootstrap)

    def test_whisper_command_forces_portuguese_transcription(self):
        module = self.load_module()
        cmd = module.build_whisper_command("whisper-cli.exe", "model.bin", "audio.wav", "out")
        self.assertIn("pt", cmd)
        self.assertNotIn("--translate", cmd)
        self.assertNotIn("-tr", cmd)

    def test_check_mode_returns_machine_readable_status(self):
        result = subprocess.run(
            [sys.executable, str(MODULE), "--check"],
            capture_output=True,
            text=True,
            check=False,
        )
        self.assertEqual(result.returncode, 0)
        payload = json.loads(result.stdout)
        self.assertIn("whisper_cli", payload)
        self.assertIn("ffmpeg", payload)
        self.assertIn("ffprobe", payload)
        self.assertIn("yt_dlp", payload)


class MediaTranscriberAuthenticatedFallbackTests(unittest.TestCase):
    def load_module(self):
        spec = importlib.util.spec_from_file_location("media_transcriber_auth", MODULE)
        self.assertIsNotNone(spec)
        self.assertIsNotNone(spec.loader)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        return module

    def test_instagram_adds_firefox_authenticated_fallback(self):
        module = self.load_module()
        self.assertEqual(
            module.acquisition_order("https://instagram.com/reel/abc"),
            ["snapinsta", "yt-dlp", "yt-dlp-firefox"],
        )

    def test_ytdlp_download_command_can_use_firefox_session(self):
        module = self.load_module()
        anonymous = module.build_ytdlp_download_command("yt-dlp.exe", "https://x.test/v", "out.%(ext)s")
        authenticated = module.build_ytdlp_download_command(
            "yt-dlp.exe", "https://x.test/v", "out.%(ext)s", cookies_from_browser="firefox"
        )
        self.assertNotIn("--cookies-from-browser", anonymous)
        self.assertIn("--cookies-from-browser", authenticated)
        self.assertIn("firefox", authenticated)

    def test_subtitle_probe_precedes_asr_and_can_reuse_firefox_session(self):
        module = self.load_module()
        cmd = module.build_subtitle_download_command(
            "yt-dlp.exe", "https://x.test/v", "subtitle.%(ext)s", cookies_from_browser="firefox"
        )
        self.assertIn("--skip-download", cmd)
        self.assertIn("--write-subs", cmd)
        self.assertNotIn("--write-auto-subs", cmd)
        self.assertIn("--cookies-from-browser", cmd)
        self.assertIn("firefox", cmd)

    def test_vtt_to_text_strips_metadata_timestamps_and_duplicate_cues(self):
        module = self.load_module()
        vtt = "WEBVTT\n\n00:00:00.000 --> 00:00:01.000\nOlá mundo\n\n00:00:01.000 --> 00:00:02.000\nOlá mundo\nTudo bem?\n"
        self.assertEqual(module.vtt_to_text(vtt), "Olá mundo\nTudo bem?")

    def test_manual_subtitles_do_not_require_asr_runtime(self):
        module = self.load_module()
        module.resolve_tools = lambda: {
            "whisper_cli": None, "ffmpeg": None, "ffprobe": None,
            "yt_dlp": "yt-dlp.exe", "curl": None, "model": None,
        }
        module.try_subtitle_transcript = lambda *args, **kwargs: "Legenda humana"
        missing_model = ROOT / "definitely-missing-model.bin"
        self.assertEqual(
            module.transcribe_source("https://youtube.com/watch?v=abc", model=missing_model),
            "Legenda humana",
        )


if __name__ == "__main__":
    unittest.main()
