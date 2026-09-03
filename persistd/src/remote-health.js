const { spawnSync } = require('node:child_process');
const os = require('node:os');
const path = require('node:path');

function delay(ms) { return new Promise((resolve) => setTimeout(resolve, ms)); }

function probeDesktopHost() {
  const cli = path.join(process.env.LOCALAPPDATA || path.join(os.homedir(), 'AppData', 'Local'), 'Dev-Orquestra', 'Desktop', 'orquestra-desktop-cli.exe');
  const request = JSON.stringify({ version: 1, request_id: 'persistd-health', operation: 'desktop.windows.list', arguments: {} });
  const result = spawnSync(cli, [], { input: request, encoding: 'utf8', windowsHide: true, timeout: 5000 });
  if (result.error || result.status !== 0) return { ok: false, error: result.error?.message || result.stderr || `EXIT_${result.status}` };
  try {
    const parsed = JSON.parse(String(result.stdout || '').trim());
    return { ok: Boolean(parsed?.ok), response: parsed };
  } catch (error) {
    return { ok: false, error: `INVALID_DESKTOP_RESPONSE:${error.message}` };
  }
}

function repairDesktopHost() {
  const command = "Get-ScheduledTask -TaskName 'Dev-Orquestra Desktop Host' -ErrorAction Stop | Stop-ScheduledTask -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 250; Start-ScheduledTask -TaskName 'Dev-Orquestra Desktop Host' -ErrorAction Stop";
  const result = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', command], { encoding: 'utf8', windowsHide: true, timeout: 10000 });
  return { ok: !result.error && result.status === 0, error: result.error?.message || result.stderr || null };
}
function createRemoteHealth({ browser, desktopProbe = probeDesktopHost, desktopRepair = repairDesktopHost, sleep = delay } = {}) {
  return {
    async preflight(state = {}) {
      let browserResult = { ok: false };
      for (let attempt = 0; attempt < 2; attempt++) {
        try { browserResult = await browser?.healthCheck?.({ state }); } catch { browserResult = { ok: false }; }
        if (browserResult?.ok) break;
        if (attempt === 0) await sleep(250);
      }

      let desktopResult;
      try { desktopResult = await desktopProbe(); } catch { desktopResult = { ok: false }; }
      let repaired = false;
      if (!desktopResult?.ok) {
        let repairResult = { ok: false };
        try { repairResult = await desktopRepair(); } catch { repairResult = { ok: false }; }
        repaired = Boolean(repairResult?.ok);
        if (repaired) {
          await sleep(500);
          try { desktopResult = await desktopProbe(); } catch { desktopResult = { ok: false }; }
        }
      }
      const browserHealthy = Boolean(browserResult?.ok);
      const desktopHealthy = Boolean(desktopResult?.ok);
      return { ok: browserHealthy && desktopHealthy, browser: browserHealthy ? 'HEALTHY' : 'UNHEALTHY', desktop: desktopHealthy ? 'HEALTHY' : 'UNHEALTHY', repaired };
    },
  };
}

module.exports = { createRemoteHealth, probeDesktopHost, repairDesktopHost };
