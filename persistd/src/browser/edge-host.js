const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { spawn } = require('node:child_process');

const DEFAULT_PORT = 9522;
const DEFAULT_WINDOW = { width: 1440, height: 960 };

function buildEdgeLaunchArgs({ port = DEFAULT_PORT, userDataDir, windowSize = DEFAULT_WINDOW, headless = false }) {
  if (!userDataDir) throw new Error('EDGE_USER_DATA_DIR_REQUIRED');
  return [
    `--remote-debugging-port=${port}`,
    `--user-data-dir=${userDataDir}`,
    '--no-first-run',
    '--no-default-browser-check',
    `--window-size=${windowSize.width},${windowSize.height}`,
    ...(headless ? ['--headless=new'] : []),
    '--no-startup-window',
  ];
}

async function browserEndpoint(port = DEFAULT_PORT, fetchFn = fetch) {
  try {
    const response = await fetchFn(`http://127.0.0.1:${port}/json/version`);
    if (!response.ok) return null;
    const info = await response.json();
    return typeof info?.webSocketDebuggerUrl === 'string' ? info : null;
  } catch { return null; }
}
function resolveStateDir(env = process.env) {
  if (env.EGO_HOST_STATE_DIR) return env.EGO_HOST_STATE_DIR;
  const local = env.LOCALAPPDATA || path.join(os.homedir(), '.local', 'share');
  return path.join(local, 'ego-windows-host');
}

async function ensureEdgeBrowser(options = {}) {
  const platform = options.platform || process.platform;
  if (platform !== 'win32') return { launched: false, skipped: true };
  const env = options.env || process.env;
  const port = Number(env.EGO_HOST_DEBUG_PORT) || DEFAULT_PORT;
  const fetchFn = options.fetchFn || fetch;
  const existing = await browserEndpoint(port, fetchFn);
  if (existing) return { endpoint: existing, launched: false };
  const browserPath = options.browserPath || env.EGO_HOST_BROWSER_PATH;
  if (!browserPath) throw new Error('PERSISTD_EDGE_BROWSER_PATH_MISSING');
  const userDataDir = path.join(resolveStateDir(env), 'profile');
  fs.mkdirSync(userDataDir, { recursive: true });
  const args = buildEdgeLaunchArgs({ port, userDataDir, headless: env.EGO_HOST_HEADLESS === '1' || env.EGO_HOST_HEADLESS === 'true' });
  const spawnFn = options.spawnFn || spawn;
  const child = spawnFn(browserPath, args, { detached: true, stdio: 'ignore' });
  if (child?.unref) child.unref();
  const sleep = options.sleep || ((ms) => new Promise((resolve) => setTimeout(resolve, ms)));
  const deadline = Date.now() + (options.timeoutMs || 20000);
  while (Date.now() < deadline) {
    const endpoint = await browserEndpoint(port, fetchFn);
    if (endpoint) return { endpoint, launched: true, args, userDataDir };
    await sleep(250);
  }
  throw new Error(`PERSISTD_EDGE_CDP_TIMEOUT:${port}`);
}

module.exports = {
  DEFAULT_PORT,
  buildEdgeLaunchArgs,
  browserEndpoint,
  resolveStateDir,
  ensureEdgeBrowser,
};
