const test = require('node:test');
const assert = require('node:assert/strict');
const { EventEmitter } = require('node:events');
const { ensureEdgeBrowser } = require('./src/browser/edge-host');

function unavailableFetch() {
  return Promise.resolve({ ok: false });
}

function baseOptions(child) {
  return {
    platform: 'win32',
    env: { EGO_HOST_DEBUG_PORT: '9877', LOCALAPPDATA: process.cwd() },
    browserPath: 'fake-edge.exe',
    fetchFn: unavailableFetch,
    spawnFn: () => child,
    sleep: () => new Promise((resolve) => setImmediate(resolve)),
  };
}

test('Edge spawn errors reject ensureEdgeBrowser instead of escaping as an unhandled event', async () => {
  const child = new EventEmitter();
  child.unref = () => {};
  const promise = ensureEdgeBrowser({ ...baseOptions(child), timeoutMs: 1000 });
  setImmediate(() => child.emit('error', new Error('spawn boom')));
  await assert.rejects(promise, /PERSISTD_EDGE_SPAWN_ERROR:spawn boom/);
});
test('Edge startup timeout terminates the child process created by persistd', async () => {
  const child = new EventEmitter();
  child.unref = () => {};
  let killCalls = 0;
  child.kill = () => {
    killCalls += 1;
    setImmediate(() => child.emit('close', 1));
    return true;
  };
  await assert.rejects(
    ensureEdgeBrowser({ ...baseOptions(child), timeoutMs: 5 }),
    /PERSISTD_EDGE_CDP_TIMEOUT:9877/,
  );
  assert.equal(killCalls, 1);
});