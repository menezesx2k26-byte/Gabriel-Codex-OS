const test = require('node:test');
const assert = require('node:assert/strict');
const { createRemoteHealth } = require('./src/remote-health');

test('remote health retries browser probe and repairs desktop before succeeding', async () => {
  let browserProbes = 0;
  let desktopProbes = 0;
  let repairs = 0;
  const health = createRemoteHealth({
    browser: { async healthCheck() { browserProbes++; return { ok: browserProbes > 1 }; } },
    desktopProbe: async () => ({ ok: ++desktopProbes > 1 }),
    desktopRepair: async () => { repairs++; return { ok: true }; },
    sleep: async () => {},
  });
  const result = await health.preflight({ RUN_ID: 'health' });
  assert.equal(result.ok, true);
  assert.equal(result.browser, 'HEALTHY');
  assert.equal(result.desktop, 'HEALTHY');
  assert.equal(result.repaired, true);
  assert.equal(browserProbes, 2);
  assert.equal(repairs, 1);
});

test('remote health fails closed when a required control layer stays unhealthy', async () => {
  const health = createRemoteHealth({
    browser: { async healthCheck() { return { ok: false }; } },
    desktopProbe: async () => ({ ok: false }),
    desktopRepair: async () => ({ ok: false }),
    sleep: async () => {},
  });
  const result = await health.preflight({ RUN_ID: 'health' });
  assert.equal(result.ok, false);
  assert.equal(result.browser, 'UNHEALTHY');
  assert.equal(result.desktop, 'UNHEALTHY');
});
test('ego browser transport exposes a bounded health probe', async () => {
  const { createEgoBrowserTransport } = require('./src/browser/ego-browser');
  let captured = '';
  const transport = createEgoBrowserTransport({ runner: async (script) => { captured = script; return { ok: true, status: 'HEALTHY' }; } });
  const result = await transport.healthCheck({ state: { RUN_ID: 'probe-run' } });
  assert.equal(result.ok, true);
  assert.match(captured, /taskSpaces\.useOrCreate/);
  assert.match(captured, /PERSISTD_RESULT/);
});
test('daemon wires remote health into every orchestrator tick', () => {
  const fs = require('node:fs');
  const source = fs.readFileSync(require.resolve('./src/daemon'), 'utf8');
  assert.match(source, /const remoteHealth = createRemoteHealth\(\{ browser \}\)/);
  assert.match(source, /notifier,\s*remoteHealth,\s*rolloverMinutes/);
});
test('installer pins the hardened Windows ego host and rewrites the ego-browser shim', () => {
  const fs = require('node:fs');
  const path = require('node:path');
  const source = fs.readFileSync(path.join(__dirname, '..', 'scripts', 'install.ps1'), 'utf8');
  assert.match(source, /menezesx2k26-byte\/ego-lite\.git/);
  assert.match(source, /feat\/windows-host-state-hardening/);
  assert.match(source, /ego-lite-windows-hardened/);
  assert.match(source, /ego-browser\.cmd/);
  assert.match(source, /npm --prefix .* run build/);
});