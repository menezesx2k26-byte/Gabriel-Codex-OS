const fs = require('node:fs');
const path = require('node:path');

function parseControl(text) {
  const state = {};
  for (const line of text.split(/\r?\n/)) {
    const match = /^([A-Z0-9_]+):\s*(.*)$/.exec(line);
    if (match) state[match[1]] = match[2].trim();
  }
  return state;
}

function serializeControl(state) {
  return Object.entries(state).map(([key, value]) => `${key}: ${value ?? ''}`).join('\n') + '\n';
}

function readControl(filePath) {
  return parseControl(fs.readFileSync(filePath, 'utf8'));
}

function sleepSync(ms) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
}

function replaceFileWithRetry(tmp, filePath, {
  rename = fs.renameSync, sleep = sleepSync, retries = 12, baseDelayMs = 25,
} = {}) {
  for (let attempt = 0; ; attempt++) {
    try {
      rename(tmp, filePath);
      return;
    } catch (error) {
      const transient = ['EPERM', 'EBUSY', 'EACCES'].includes(error?.code);
      if (!transient || attempt >= retries) throw error;
      sleep(Math.min(baseDelayMs * (attempt + 1), 250));
    }
  }
}

function writeFileInPlaceVerified(filePath, content) {
  const bytes = Buffer.from(content, 'utf8');
  const fd = fs.openSync(filePath, 'r+');
  try {
    let offset = 0;
    while (offset < bytes.length) {
      const written = fs.writeSync(fd, bytes, offset, bytes.length - offset, offset);
      if (written <= 0) throw new Error('CONTROL_INPLACE_WRITE_STALLED');
      offset += written;
    }
    fs.ftruncateSync(fd, bytes.length);
    fs.fsyncSync(fd);
  } finally {
    fs.closeSync(fd);
  }
  const verified = fs.readFileSync(filePath);
  if (!verified.equals(bytes)) throw new Error('CONTROL_INPLACE_VERIFY_FAILED');
}

function writeControlAtomic(filePath, state, options = {}) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  const content = serializeControl(state);
  const tmp = `${filePath}.${process.pid}.${Date.now()}.tmp`;
  fs.writeFileSync(tmp, content, 'utf8');
  try {
    replaceFileWithRetry(tmp, filePath, options);
    return;
  } catch (error) {
    const transient = ['EPERM', 'EBUSY', 'EACCES'].includes(error?.code);
    if (!transient) throw error;
  }
  writeFileInPlaceVerified(filePath, content);
  fs.unlinkSync(tmp);
}

module.exports = { parseControl, serializeControl, readControl, replaceFileWithRetry, writeFileInPlaceVerified, writeControlAtomic };
