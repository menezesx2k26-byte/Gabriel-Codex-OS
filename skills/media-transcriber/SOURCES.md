# Pinned upstreams

Verified on 2026-09-11.

- whisper.cpp: `ggml-org/whisper.cpp`, release `b4938`, `whisper-bin-x64.zip`.
  - SHA-256: `c2a4b60edb11f7e11a9191ffb50929535527d4d91c9903dbe3e554583bbbc63d`
  - License: MIT.
- yt-dlp: `yt-dlp/yt-dlp`, release `2026.08.19`, `yt-dlp.exe`.
  - SHA-256: `66674953fe251b89f4d08c5f0e35e0728679bd67ab3d7d05c0562af101dd3e7a`
  - License: Unlicense.
  - Official CLI supports `--cookies-from-browser BROWSER`; this skill uses `firefox` only after anonymous Instagram access fails and never exports a cookies file.
  - Human subtitle fast path uses `--skip-download --write-subs` for Portuguese tracks; automatic captions are intentionally excluded.
- Preferred Windows FFmpeg runtime: Gyan full build installed into `vendor/bin` by the bootstrap. On the current host it completed 12/12 repeated HE-AAC decodes without failure.
- Retained FFmpeg fallback: `BtbN/FFmpeg-Builds`, release `autobuild-2026-09-08-13-11`.
  - Artifact: `ffmpeg-n9.0.1-27-g9b0578816c-win64-lgpl-9.0.zip`.
  - SHA-256: `ad7fe9c2849fab434e842b6967cb76e5f0dcf90ed7a5ec1e9990ba7c282614da`.
  - Compatibility note: on the current i3-8100 host this build intermittently exited with Windows code `0xC000001D` during HE-AAC decode, so it is no longer the preferred decoder.
- Model source: `ggerganov/whisper.cpp` model storage used by whisper.cpp's own download script.
  - Default: `ggml-small-q5_1.bin` (multilingual, quantized).
  - Quality fallback retained locally: `ggml-small.bin` (multilingual, full precision/size).

Instagram acquisition uses SnapInsta's public site endpoint `https://snapinsta.com.br/api/download.php`, discovered from the site's own frontend on 2026-09-08. It is treated as a best-effort external dependency; anonymous yt-dlp and then authenticated Firefox yt-dlp are the fallbacks.

Runtime artifacts are stored outside the skill under `~/.agents/vendor/media-transcriber` so the Router indexes only the skill metadata and orchestration code.
