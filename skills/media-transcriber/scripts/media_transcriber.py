from __future__ import annotations

import argparse
import html
import json
import re
import shutil
import subprocess
import sys
import tempfile
import wave
from pathlib import Path
from urllib.parse import urlparse

VENDOR = Path.home() / ".agents" / "vendor" / "media-transcriber"
WHISPER_ROOT = VENDOR / "whisper.cpp-b4938"
FFMPEG_ROOT = VENDOR / "ffmpeg"
YT_DLP_EXE = VENDOR / "yt-dlp" / "yt-dlp.exe"
MODEL_ROOT = VENDOR / "models"
DEFAULT_MODEL = MODEL_ROOT / "ggml-small-q5_1.bin"
SNAPINSTA_ENDPOINT = "https://snapinsta.com.br/api/download.php"


def classify_source(source: str) -> str:
    parsed = urlparse(source)
    return "url" if parsed.scheme in {"http", "https"} else "file"


def is_instagram_url(source: str) -> bool:
    host = urlparse(source).netloc.lower().split(":", 1)[0]
    return host == "instagram.com" or host.endswith(".instagram.com")


def acquisition_order(source: str) -> list[str]:
    return ["snapinsta", "yt-dlp", "yt-dlp-firefox"] if is_instagram_url(source) else ["yt-dlp"]


def acquisition_strategy(source: str) -> str:
    return acquisition_order(source)[0]


def _first_existing(paths: list[Path]) -> str | None:
    for path in paths:
        if path.exists():
            return str(path)
    return None


def resolve_tools() -> dict[str, str | None]:
    whisper = _first_existing(
        list(WHISPER_ROOT.rglob("whisper-cli.exe")) + [VENDOR / "bin" / "whisper" / "whisper-cli.exe"]
    )
    ffmpeg = _first_existing([VENDOR / "bin" / "ffmpeg.exe"] + list(FFMPEG_ROOT.rglob("ffmpeg.exe")))
    ffprobe = _first_existing([VENDOR / "bin" / "ffprobe.exe"] + list(FFMPEG_ROOT.rglob("ffprobe.exe")))
    yt_dlp = str(YT_DLP_EXE) if YT_DLP_EXE.exists() else _first_existing([VENDOR / "bin" / "yt-dlp.exe"]) or shutil.which("yt-dlp")
    curl = shutil.which("curl.exe") or shutil.which("curl")
    return {
        "whisper_cli": whisper or shutil.which("whisper-cli"),
        "ffmpeg": ffmpeg or shutil.which("ffmpeg"),
        "ffprobe": ffprobe or shutil.which("ffprobe"),
        "yt_dlp": yt_dlp,
        "curl": curl,
        "model": str(DEFAULT_MODEL) if DEFAULT_MODEL.exists() else None,
    }


def build_whisper_command(
    whisper_cli: str, model: str, audio: str, output_prefix: str
) -> list[str]:
    return [
        whisper_cli, "-m", model, "-f", audio, "-l", "pt",
        "-otxt", "-of", output_prefix, "-nt", "-np",
    ]


def build_snapinsta_api_command(curl: str) -> list[str]:
    return [
        curl, "--silent", "--show-error", "--fail-with-body", "--location",
        "--connect-timeout", "5", "--max-time", "15",
        "-H", "Content-Type: application/json", "--data-binary", "@-", SNAPINSTA_ENDPOINT,
    ]


def build_media_download_command(curl: str, url: str, destination: str) -> list[str]:
    return [
        curl, "--silent", "--show-error", "--fail", "--location",
        "--connect-timeout", "5", "--max-time", "60", "-o", destination, url,
    ]


def _append_browser_cookies(command: list[str], cookies_from_browser: str | None) -> list[str]:
    if cookies_from_browser:
        command.extend(["--cookies-from-browser", cookies_from_browser])
    return command


def build_ytdlp_download_command(
    yt_dlp: str, source: str, template: str, cookies_from_browser: str | None = None
) -> list[str]:
    command = [yt_dlp, "--no-playlist", "-o", template]
    _append_browser_cookies(command, cookies_from_browser)
    command.append(source)
    return command


def build_subtitle_download_command(
    yt_dlp: str, source: str, template: str, cookies_from_browser: str | None = None
) -> list[str]:
    command = [
        yt_dlp, "--no-playlist", "--skip-download", "--write-subs",
        "--sub-langs", "pt.*,pt", "--sub-format", "vtt", "-o", template,
    ]
    _append_browser_cookies(command, cookies_from_browser)
    command.append(source)
    return command


def _run(
    command: list[str], *, cwd: Path | None = None, input_text: str | None = None, timeout: float | None = None
) -> subprocess.CompletedProcess[str]:
    try:
        return subprocess.run(
            command, cwd=cwd, input=input_text, capture_output=True, text=True, check=False, timeout=timeout
        )
    except subprocess.TimeoutExpired as exc:
        raise RuntimeError(f"Comando excedeu o limite de {timeout}s: {command[0]}") from exc


def select_snapinsta_media_url(payload: dict) -> str:
    media = payload.get("media") if isinstance(payload, dict) else None
    if not payload.get("status") or not isinstance(media, list) or not media:
        raise RuntimeError(payload.get("message") or "SnapInsta não retornou mídia.")
    for item in media:
        if isinstance(item, dict) and str(item.get("fileType", "")).startswith("video/") and item.get("url"):
            return str(item["url"])
    for item in media:
        if isinstance(item, dict) and item.get("url"):
            return str(item["url"])
    raise RuntimeError("SnapInsta retornou mídia sem URL utilizável.")


def download_snapinsta(source: str, target_dir: Path, curl: str) -> Path:
    body = json.dumps({"url": source}, ensure_ascii=False)
    api = _run(build_snapinsta_api_command(curl), input_text=body, timeout=20)
    if api.returncode != 0:
        raise RuntimeError(api.stderr.strip() or api.stdout.strip() or "SnapInsta API falhou.")
    try:
        payload = json.loads(api.stdout)
    except ValueError as exc:
        raise RuntimeError("SnapInsta retornou JSON inválido.") from exc

    media_url = select_snapinsta_media_url(payload)
    destination = target_dir / "source-snapinsta.mp4"
    media = _run(build_media_download_command(curl, media_url, str(destination)), timeout=65)
    if media.returncode != 0:
        raise RuntimeError(media.stderr.strip() or "Falha ao baixar mídia do SnapInsta.")
    if not destination.exists() or destination.stat().st_size == 0:
        raise RuntimeError("SnapInsta produziu um arquivo vazio.")
    return destination


def download_ytdlp(
    source: str, yt_dlp: str, target_dir: Path, cookies_from_browser: str | None = None
) -> Path:
    template = str(target_dir / "source.%(ext)s")
    result = _run(build_ytdlp_download_command(yt_dlp, source, template, cookies_from_browser))
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "Falha ao baixar a mídia com yt-dlp.")
    files = [p for p in target_dir.iterdir() if p.is_file() and p.suffix.lower() != ".vtt"]
    if not files:
        raise RuntimeError("yt-dlp terminou sem produzir um arquivo de mídia.")
    return max(files, key=lambda p: p.stat().st_mtime)


def download_url(source: str, yt_dlp: str | None, curl: str | None, target_dir: Path) -> Path:
    errors: list[str] = []
    for method in acquisition_order(source):
        try:
            if method == "snapinsta" and curl:
                return download_snapinsta(source, target_dir, curl)
            if method == "snapinsta":
                errors.append("SnapInsta indisponível: curl não encontrado")
                continue
            if method == "yt-dlp" and yt_dlp:
                return download_ytdlp(source, yt_dlp, target_dir)
            if method == "yt-dlp-firefox" and yt_dlp:
                return download_ytdlp(source, yt_dlp, target_dir, cookies_from_browser="firefox")
            errors.append(f"{method} indisponível")
        except Exception as exc:
            errors.append(f"{method}: {exc}")
    raise RuntimeError("Falha ao adquirir mídia: " + " | ".join(errors))


def vtt_to_text(vtt: str) -> str:
    lines: list[str] = []
    for raw in vtt.splitlines():
        line = raw.strip()
        if not line or line == "WEBVTT" or line.startswith(("Kind:", "Language:", "NOTE", "STYLE")):
            continue
        if "-->" in line or line.isdigit():
            continue
        line = html.unescape(re.sub(r"<[^>]+>", "", line)).strip()
        if line and (not lines or lines[-1] != line):
            lines.append(line)
    return "\n".join(lines)


def _subtitle_attempt(
    source: str, yt_dlp: str, target_dir: Path, cookies_from_browser: str | None = None
) -> tuple[str | None, bool]:
    target_dir.mkdir(parents=True, exist_ok=True)
    template = str(target_dir / "subtitle.%(ext)s")
    result = _run(build_subtitle_download_command(yt_dlp, source, template, cookies_from_browser), timeout=45)
    if result.returncode != 0:
        return None, False
    for path in sorted(target_dir.rglob("*.vtt")):
        text = vtt_to_text(path.read_text(encoding="utf-8-sig", errors="replace"))
        if text:
            return text, True
    return None, True

def try_subtitle_transcript(source: str, yt_dlp: str, target_dir: Path) -> str | None:
    text, anonymous_ok = _subtitle_attempt(source, yt_dlp, target_dir / "anonymous")
    if text or anonymous_ok:
        return text
    if is_instagram_url(source):
        text, _ = _subtitle_attempt(
            source, yt_dlp, target_dir / "firefox", cookies_from_browser="firefox"
        )
        return text
    return None


def build_chunk_windows(duration: float, chunk_seconds: float = 20.0) -> list[tuple[float, float]]:
    windows: list[tuple[float, float]] = []
    start = 0.0
    while start < duration - 1e-6:
        remaining = duration - start
        if remaining < 0.0005:
            break
        length = min(chunk_seconds, remaining)
        windows.append((round(start, 3), round(length, 3)))
        start += length
    return windows


def probe_duration(source: Path, ffprobe: str) -> float:
    result = _run([
        ffprobe, "-v", "error", "-show_entries", "format=duration",
        "-of", "default=noprint_wrappers=1:nokey=1", str(source),
    ])
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "ffprobe falhou ao medir a duração.")
    try:
        return float(result.stdout.strip())
    except ValueError as exc:
        raise RuntimeError("ffprobe retornou duração inválida.") from exc


def probe_audio_profile(source: Path, ffprobe: str) -> tuple[str, str]:
    result = _run([
        ffprobe, "-v", "error", "-select_streams", "a:0",
        "-show_entries", "stream=codec_name,profile", "-of", "json", str(source),
    ])
    if result.returncode != 0:
        return "", ""
    try:
        payload = json.loads(result.stdout)
        stream = payload.get("streams", [{}])[0]
        return str(stream.get("codec_name", "")), str(stream.get("profile", ""))
    except (ValueError, TypeError, IndexError):
        return "", ""


def needs_chunked_decode(codec_name: str, profile: str) -> bool:
    return codec_name.lower() == "aac" and profile.upper().startswith("HE-AAC")

def _ffmpeg_wav_command(ffmpeg: str, source: Path, target: Path) -> list[str]:
    return [
        ffmpeg, "-hide_banner", "-loglevel", "error", "-y", "-i", str(source),
        "-vn", "-ar", "16000", "-ac", "1", "-c:a", "pcm_s16le", str(target),
    ]


def build_audio_remux_command(ffmpeg: str, source: str, target: str) -> list[str]:
    return [
        ffmpeg, "-hide_banner", "-loglevel", "error", "-y", "-i", source,
        "-vn", "-c:a", "copy", target,
    ]


def _remux_audio(source: Path, ffmpeg: str, target: Path) -> None:
    result = _run(build_audio_remux_command(ffmpeg, str(source), str(target)))
    if result.returncode != 0 or not target.exists() or target.stat().st_size == 0:
        raise RuntimeError(result.stderr.strip() or "FFmpeg falhou ao remuxar a faixa de áudio.")


def _convert_chunk(source: Path, ffmpeg: str, target: Path, start: float, length: float) -> None:
    result = _run([
        ffmpeg, "-hide_banner", "-loglevel", "error", "-y", "-i", str(source),
        "-ss", f"{start:.3f}", "-t", f"{length:.3f}", "-vn",
        "-ar", "16000", "-ac", "1", "-c:a", "pcm_s16le", str(target),
    ])
    if result.returncode != 0 or not target.exists() or target.stat().st_size <= 44:
        raise RuntimeError(result.stderr.strip() or f"FFmpeg falhou no bloco iniciado em {start:.3f}s.")


def concatenate_wavs(parts: list[Path], target: Path) -> None:
    if not parts:
        raise RuntimeError("Nenhum bloco WAV foi produzido.")
    params = None
    with wave.open(str(target), "wb") as output:
        for part in parts:
            with wave.open(str(part), "rb") as current:
                signature = (
                    current.getnchannels(), current.getsampwidth(), current.getframerate(), current.getcomptype()
                )
                if params is None:
                    params = signature
                    output.setnchannels(signature[0])
                    output.setsampwidth(signature[1])
                    output.setframerate(signature[2])
                    output.setcomptype(signature[3], "not compressed")
                elif signature != params:
                    raise RuntimeError("Blocos WAV incompatíveis durante a recomposição.")
                output.writeframes(current.readframes(current.getnframes()))


def convert_to_wav(source: Path, ffmpeg: str, ffprobe: str, target: Path) -> None:
    codec_name, profile = probe_audio_profile(source, ffprobe)
    decode_source = source
    if needs_chunked_decode(codec_name, profile):
        remuxed = target.with_name(f"{target.stem}-source.m4a")
        _remux_audio(source, ffmpeg, remuxed)
        decode_source = remuxed

    direct = _run(_ffmpeg_wav_command(ffmpeg, decode_source, target))
    if direct.returncode == 0 and target.exists() and target.stat().st_size > 44:
        return
    target.unlink(missing_ok=True)

    duration = probe_duration(decode_source, ffprobe)
    parts: list[Path] = []
    for index, (start, length) in enumerate(build_chunk_windows(duration)):
        part = target.with_name(f"{target.stem}-part-{index:03d}.wav")
        _convert_chunk(decode_source, ffmpeg, part, start, length)
        parts.append(part)
    concatenate_wavs(parts, target)


def transcribe_wav(wav: Path, whisper_cli: str, model: str, output_prefix: Path) -> str:
    command = build_whisper_command(whisper_cli, model, str(wav), str(output_prefix))
    result = _run(command)
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "whisper.cpp falhou na transcrição.")
    transcript_path = Path(f"{output_prefix}.txt")
    if not transcript_path.exists():
        raise RuntimeError("whisper.cpp não gerou o arquivo de transcrição esperado.")
    return transcript_path.read_text(encoding="utf-8-sig", errors="replace").strip()


def _require_asr_runtime(tools: dict[str, str | None], model: Path, source: str) -> None:
    missing = [name for name in ("whisper_cli", "ffmpeg", "ffprobe") if not tools[name]]
    if classify_source(source) == "url" and acquisition_strategy(source) == "yt-dlp" and not tools["yt_dlp"]:
        missing.append("yt_dlp")
    if not model.exists():
        missing.append("model")
    if missing:
        raise RuntimeError("Dependências ausentes: " + ", ".join(missing))


def transcribe_source(source: str, output: Path | None = None, model: Path = DEFAULT_MODEL) -> str:
    tools = resolve_tools()
    source_kind = classify_source(source)

    with tempfile.TemporaryDirectory(prefix="media-transcriber-") as temp:
        work = Path(temp)
        media: Path | None = None
        text: str | None = None
        if source_kind == "url" and tools["yt_dlp"]:
            text = try_subtitle_transcript(source, str(tools["yt_dlp"]), work / "subtitles")

        if text is None:
            _require_asr_runtime(tools, model, source)
            if source_kind == "url":
                media = download_url(source, tools["yt_dlp"], tools["curl"], work)
            else:
                media = Path(source).expanduser().resolve()
                if not media.exists():
                    raise FileNotFoundError(f"Arquivo não encontrado: {media}")
            wav = work / "audio.wav"
            convert_to_wav(media, str(tools["ffmpeg"]), str(tools["ffprobe"]), wav)
            text = transcribe_wav(wav, str(tools["whisper_cli"]), str(model), work / "transcript")

    if output:
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(text + "\n", encoding="utf-8")
    return text


def main() -> int:
    parser = argparse.ArgumentParser(description="Transcreve mídia local ou URL usando whisper.cpp.")
    parser.add_argument("source", nargs="?", help="Arquivo local ou URL de mídia")
    parser.add_argument("--output", type=Path, help="Arquivo .txt opcional para salvar a transcrição")
    parser.add_argument("--model", type=Path, default=DEFAULT_MODEL, help="Modelo ggml do Whisper")
    parser.add_argument("--check", action="store_true", help="Exibe status das dependências em JSON")
    args = parser.parse_args()

    if args.check:
        status = resolve_tools()
        status["ready"] = bool(
            status["whisper_cli"] and status["ffmpeg"] and status["ffprobe"] and status["model"]
        )
        print(json.dumps(status, ensure_ascii=False))
        return 0
    if not args.source:
        parser.error("source é obrigatório, exceto com --check")

    try:
        print(transcribe_source(args.source, args.output, args.model))
        return 0
    except Exception as exc:
        print(f"erro: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
