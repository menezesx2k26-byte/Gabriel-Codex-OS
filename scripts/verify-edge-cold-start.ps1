param([int]$Port = 9533)
$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$StateDir = Join-Path $env:TEMP ("persistd-edge-smoke-" + $PID)
$pf86 = [Environment]::GetEnvironmentVariable('ProgramFiles(x86)')
$pf64 = [Environment]::GetEnvironmentVariable('ProgramFiles')
$drive = if ($env:SystemDrive) { $env:SystemDrive } else { 'C:' }
$Candidates = @(
  (Join-Path ($drive + '\\') 'Program Files (x86)\Microsoft\Edge\Application\msedge.exe'),
  (Join-Path ($drive + '\\') 'Program Files\Microsoft\Edge\Application\msedge.exe')
)
if ($pf86) { $Candidates = @((Join-Path $pf86 'Microsoft\Edge\Application\msedge.exe')) + $Candidates }
if ($pf64) { $Candidates = @((Join-Path $pf64 'Microsoft\Edge\Application\msedge.exe')) + $Candidates }
$Candidates = @($Candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique)
if (-not $Candidates) { throw 'Microsoft Edge not found' }
$oldPath = $env:EGO_HOST_BROWSER_PATH
$oldPort = $env:EGO_HOST_DEBUG_PORT
$oldState = $env:EGO_HOST_STATE_DIR
try {
  $env:EGO_HOST_BROWSER_PATH = $Candidates[0]
  $env:EGO_HOST_DEBUG_PORT = [string]$Port
  $env:EGO_HOST_STATE_DIR = $StateDir
  $module = Join-Path $RepoRoot 'persistd\src\browser\edge-host.js'
  $code = "require('$($module.Replace('\','\\'))').ensureEdgeBrowser({env:process.env}).then(x=>console.log(JSON.stringify(x))).catch(e=>{console.error(e);process.exit(1)})"
  node -e $code | Out-Host
  $version = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/json/version"
  if (-not $version.webSocketDebuggerUrl) { throw 'Browser-level CDP endpoint missing' }
  $targets = @(Invoke-RestMethod -Uri "http://127.0.0.1:$Port/json/list")
  $pages = @($targets | Where-Object { $_.type -eq 'page' })
  if ($pages.Count -ne 0) { throw "Cold-start leaked $($pages.Count) page target(s)" }
  [pscustomobject]@{ CdpAlive = $true; PageCount = $pages.Count; Browser = $version.Browser } | ConvertTo-Json -Compress
} finally {
  Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($StateDir) } | ForEach-Object {
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
  }
  Remove-Item $StateDir -Recurse -Force -ErrorAction SilentlyContinue
  $env:EGO_HOST_BROWSER_PATH = $oldPath
  $env:EGO_HOST_DEBUG_PORT = $oldPort
  $env:EGO_HOST_STATE_DIR = $oldState
}
