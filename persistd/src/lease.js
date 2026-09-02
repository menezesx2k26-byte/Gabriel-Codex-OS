const fs = require('node:fs');
const path = require('node:path');

function defaultIsProcessAlive(pid) {
  if (!Number.isInteger(pid) || pid <= 0) return false;
  try {
    process.kill(pid, 0);
    return true;
  } catch (error) {
    return error?.code === 'EPERM';
  }
}

function acquireLock(lockPath, { isProcessAlive = defaultIsProcessAlive } = {}) {
  fs.mkdirSync(path.dirname(lockPath), { recursive: true });
  let fd;
  for (let attempt = 0; attempt < 3 && fd === undefined; attempt++) {
    try {
      fd = fs.openSync(lockPath, 'wx');
    } catch (error) {
      if (!error || error.code !== 'EEXIST') throw error;
      let owner;
      try {
        owner = Number.parseInt(fs.readFileSync(lockPath, 'utf8').trim(), 10);
      } catch (readError) {
        if (readError.code === 'ENOENT') continue;
        throw readError;
      }
      if (!Number.isInteger(owner) || owner <= 0 || isProcessAlive(owner)) throw new Error('LOCKED');
      try { fs.unlinkSync(lockPath); } catch (unlinkError) { if (unlinkError.code !== 'ENOENT') throw unlinkError; }
    }
  }
  if (fd === undefined) throw new Error('LOCKED');
  fs.writeFileSync(fd, `${process.pid}\n`);
  fs.fsyncSync(fd);
  let released = false;
  return () => {
    if (released) return;
    released = true;
    fs.closeSync(fd);
    try { fs.unlinkSync(lockPath); } catch (error) { if (error.code !== 'ENOENT') throw error; }
  };
}

function claimLease(state, owner, now = new Date(), ttlMs = 60000) {
  const currentOwner = state.LEASE_OWNER;
  const expiry = state.LEASE_EXPIRES_AT ? Date.parse(state.LEASE_EXPIRES_AT) : NaN;
  if (currentOwner && currentOwner !== owner && Number.isFinite(expiry) && expiry > now.getTime()) {
    throw new Error(`LEASE_HELD:${currentOwner}`);
  }
  return {
    ...state,
    LEASE_OWNER: owner,
    LEASE_EXPIRES_AT: new Date(now.getTime() + ttlMs).toISOString(),
    LAST_HEARTBEAT: now.toISOString(),
  };
}

module.exports = { acquireLock, claimLease };
