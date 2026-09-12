---
name: media-transcriber
description: Use when the user asks to transcribe speech from audio, video, Instagram Reels, TikTok, YouTube, social-media URLs, or local media files.
---

# Media Transcriber

Transcreva mídia com um pipeline local e reproduzível, recorrendo a sessão autenticada apenas quando a aquisição pública falhar.

## Route

1. Para arquivo local, execute `scripts/media_transcriber.py` diretamente.
2. Para URL, procure primeiro por **legendas humanas em português antes do ASR**. Legendas automáticas da plataforma não substituem o Whisper local.
3. Para Instagram, a aquisição da mídia segue: SnapInsta -> `yt-dlp` anônimo -> `yt-dlp-firefox`.
4. `yt-dlp-firefox` é último recurso e usa `--cookies-from-browser firefox`. Leia a sessão local somente após falha anônima; não exporte nem persista cookies em arquivo.
5. Para outras URLs suportadas, use `yt-dlp` anônimo diretamente.
6. Normalize áudio para PCM mono 16 kHz. Em HE-AAC, remuxe primeiro a faixa para M4A com `-c:a copy`; se a decodificação integral falhar, use chunks.
7. SnapInsta deve usar timeouts rígidos de conexão e duração total para não travar o Router.
8. Quando não houver legenda humana, transcreva com `whisper.cpp`, idioma `pt`, tradução desativada.
9. Preserve a fala original. Corrija apenas erros fonéticos óbvios do ASR e marque áudio realmente incerto como `[inaudível]`.

## Commands

Diagnóstico:

```powershell
python "$env:USERPROFILE\.agents\skills\media-transcriber\scripts\media_transcriber.py" --check
```
Transcrever arquivo ou URL:

```powershell
python "$env:USERPROFILE\.agents\skills\media-transcriber\scripts\media_transcriber.py" "<source>"
```

Salvar a transcrição:

```powershell
python "$env:USERPROFILE\.agents\skills\media-transcriber\scripts\media_transcriber.py" "<source>" --output "transcript.txt"
```

O wrapper `scripts/transcribe_media.ps1` delega ao mesmo pipeline Python; não mantenha uma segunda implementação de aquisição/transcrição.

## Runtime

- ASR: `ggml-org/whisper.cpp` pinado em `b4938`.
- Downloader: SnapInsta primeiro no Instagram; `yt-dlp/yt-dlp` `2026.08.19` anônimo e fallback autenticado via Firefox.
- Subtitle fast path: somente legendas humanas `pt*`; se existirem, não exija Whisper, FFmpeg ou modelo local.
- Conversão: prefira o build Gyan FFmpeg + `ffprobe`; mantenha o artefato BtbN pinado apenas como fallback.
- Modelo padrão: `ggml-small-q5_1.bin`; `ggml-small.bin` fica disponível como fallback de maior custo.
- Binaries/modelos ficam em `~/.agents/vendor/media-transcriber`; nunca versione cookies, mídia baixada ou modelos no Git.

## Verification

Execute após qualquer mudança:

```powershell
python -m unittest discover -s "$env:USERPROFILE\.agents\skills\media-transcriber\tests" -v
```

Para validação real de Instagram, use um Reel acessível à conta local e confirme que o caminho autenticado só aparece depois da falha pública.
