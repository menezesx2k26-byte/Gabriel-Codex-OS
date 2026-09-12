param(
    [ValidateSet('small-q5_1','small','medium','large-v3-turbo-q5_0')]
    [string]$Model = 'small-q5_1'
)

$ErrorActionPreference = 'Stop'
$root = Join-Path $HOME '.agents\vendor\media-transcriber'
$bin = Join-Path $root 'bin'
$models = Join-Path $root 'models'
$cache = Join-Path $root 'cache'
New-Item -ItemType Directory -Force -Path $bin,$models,$cache | Out-Null

$ytVersion = '2026.08.19'
$ytUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/$ytVersion/yt-dlp.exe"
$ytSha = '66674953fe251b89f4d08c5f0e35e0728679bd67ab3d7d05c0562af101dd3e7a'
$whisperTag = 'b4938'
$whisperUrl = "https://github.com/ggml-org/whisper.cpp/releases/download/$whisperTag/whisper-bin-x64.zip"
$whisperSha = 'c2a4b60edb11f7e11a9191ffb50929535527d4d91c9903dbe3e554583bbbc63d'

function Get-VerifiedFile([string]$Url,[string]$Destination,[string]$Sha256) {
    if (-not (Test-Path $Destination) -or ((Get-FileHash $Destination -Algorithm SHA256).Hash.ToLower() -ne $Sha256)) {
        Invoke-WebRequest -Uri $Url -OutFile $Destination -UseBasicParsing
    }
    $actual = (Get-FileHash $Destination -Algorithm SHA256).Hash.ToLower()
    if ($actual -ne $Sha256) { throw "SHA256 mismatch for $Destination" }
}

$ytExe = Join-Path $bin 'yt-dlp.exe'
Get-VerifiedFile $ytUrl $ytExe $ytSha

$whisperZip = Join-Path $cache "whisper-$whisperTag-x64.zip"
Get-VerifiedFile $whisperUrl $whisperZip $whisperSha
$whisperDir = Join-Path $bin 'whisper'
if (-not (Test-Path (Join-Path $whisperDir 'whisper-cli.exe'))) {
    $extract = Join-Path $cache "whisper-$whisperTag-extract"
    Remove-Item -Recurse -Force $extract -ErrorAction SilentlyContinue
    Expand-Archive -Path $whisperZip -DestinationPath $extract -Force
    $cli = Get-ChildItem $extract -Recurse -Filter whisper-cli.exe | Select-Object -First 1
    if (-not $cli) { throw 'whisper-cli.exe not found in pinned release archive' }
    New-Item -ItemType Directory -Force -Path $whisperDir | Out-Null
    Copy-Item -Path (Join-Path $cli.Directory.FullName '*') -Destination $whisperDir -Recurse -Force
}

$ffmpegExe = Join-Path $bin 'ffmpeg.exe'
if (-not (Test-Path $ffmpegExe)) {
    $ffmpeg = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if (-not $ffmpeg) {
        winget install --id Gyan.FFmpeg --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity | Out-Host
        $ffmpeg = Get-Command ffmpeg -ErrorAction SilentlyContinue
    }
    if ($ffmpeg) {
        Copy-Item $ffmpeg.Source $ffmpegExe -Force
    } else {
        $link = Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Links\ffmpeg.exe'
        if (Test-Path $link) {
            Copy-Item $link $ffmpegExe -Force
        } else {
            $packageRoot = Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Packages'
            $found = Get-ChildItem $packageRoot -Recurse -Filter ffmpeg.exe -ErrorAction SilentlyContinue | Select-Object -First 1
            if (-not $found) { throw 'FFmpeg installation completed but ffmpeg.exe could not be located' }
            Copy-Item $found.FullName $ffmpegExe -Force
        }
    }
}

$ffprobeExe = Join-Path $bin 'ffprobe.exe'
if (-not (Test-Path $ffprobeExe)) {
    $ffprobe = Get-Command ffprobe -ErrorAction SilentlyContinue
    if ($ffprobe) {
        Copy-Item $ffprobe.Source $ffprobeExe -Force
    } else {
        $packageRoot = Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Packages'
        $foundProbe = Get-ChildItem $packageRoot -Recurse -Filter ffprobe.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $foundProbe) { throw 'FFmpeg is present but ffprobe.exe could not be located' }
        Copy-Item $foundProbe.FullName $ffprobeExe -Force
    }
}
$vcomp = Join-Path $env:WINDIR 'System32\VCOMP140.DLL'
if (-not (Test-Path $vcomp)) {
    winget install --id Microsoft.VCRedist.2015+.x64 --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity | Out-Host
    if (-not (Test-Path $vcomp)) { throw 'Visual C++ OpenMP runtime VCOMP140.dll is still missing after VC++ Redistributable installation' }
}
$modelPath = Join-Path $models "ggml-$Model.bin"
$modelUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-$Model.bin"
if (-not (Test-Path $modelPath)) {
    $bits = Get-Command Start-BitsTransfer -ErrorAction SilentlyContinue
    if ($bits) { Start-BitsTransfer -Source $modelUrl -Destination $modelPath }
    else { Invoke-WebRequest -Uri $modelUrl -OutFile $modelPath -UseBasicParsing }
}

$manifest = [ordered]@{
    installedAt = (Get-Date).ToString('o')
    whisperCpp = [ordered]@{ tag = $whisperTag; url = $whisperUrl; sha256 = $whisperSha }
    ytDlp = [ordered]@{ version = $ytVersion; url = $ytUrl; sha256 = $ytSha }
    ffmpeg = (& $ffmpegExe -version | Select-Object -First 1)
    model = [ordered]@{ name = $Model; path = $modelPath; sha256 = (Get-FileHash $modelPath -Algorithm SHA256).Hash.ToLower() }
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 (Join-Path $root 'runtime.json')
Write-Output "MEDIA_TRANSCRIBER_READY root=$root model=$Model"
