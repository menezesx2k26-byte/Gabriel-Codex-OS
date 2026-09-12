param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [string]$Language = 'pt',
    [ValidateSet('small-q5_1','small','medium','large-v3-turbo-q5_0')]
    [string]$Model = 'small-q5_1',
    [switch]$DryRun,
    [switch]$KeepArtifacts
)

$ErrorActionPreference = 'Stop'
$skillRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$pythonScript = Join-Path $skillRoot 'scripts\media_transcriber.py'
$vendor = Join-Path $HOME '.agents\vendor\media-transcriber'
$modelPath = Join-Path $vendor "models\ggml-$Model.bin"

if ($Language -ne 'pt') {
    throw 'The unified media-transcriber pipeline currently supports Portuguese (pt) only.'
}
if ($DryRun) {
    python $pythonScript --check
    exit $LASTEXITCODE
}
if (-not (Test-Path $modelPath)) {
    & (Join-Path $skillRoot 'scripts\bootstrap_media_transcriber.ps1') -Model $Model
}

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
python $pythonScript $InputPath --model $modelPath
exit $LASTEXITCODE
