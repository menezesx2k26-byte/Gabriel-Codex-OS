$ErrorActionPreference = "Stop"
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
& (Join-Path $ScriptRoot "install.ps1")
