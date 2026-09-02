#!/usr/bin/env node
const { readControl, writeControlAtomic } = require('./control-state');

function claimGeneration({ controlPath, generation, nonce, now = new Date() }) {
  if (!controlPath) throw new Error('CONTROL_PATH_REQUIRED');
  if (!Number.isInteger(generation) || generation < 2) throw new Error('GENERATION_INVALID');
  if (!nonce) throw new Error('NONCE_REQUIRED');

  const current = readControl(controlPath);
  const currentGeneration = Number.parseInt(current.GENERATION || '0', 10);
  if (currentGeneration !== generation - 1) {
    throw new Error(`GENERATION_MISMATCH:${currentGeneration}`);
  }
  if (!['PREPARING_TAKEOVER', 'ROLLOVER_INCOMPLETE'].includes(current.STATUS)) {
    throw new Error(`STATUS_INVALID:${current.STATUS || 'NONE'}`);
  }
  if (String(current.NEXT_GENERATION || '') !== String(generation)) {
    throw new Error(`NEXT_GENERATION_MISMATCH:${current.NEXT_GENERATION || 'NONE'}`);
  }
  if (current.CLAIM_NONCE !== nonce) throw new Error('NONCE_MISMATCH');

  const claimedAt = now.toISOString();
  const claimed = {
    ...current,
    GENERATION: String(generation),
    STATUS: 'ACTIVE',
    CLAIM_NONCE: nonce,
    CLAIMED_AT: claimedAt,
    LEASE_OWNER: `G${generation}`,
    LEASE_EXPIRES_AT: new Date(now.getTime() + 90_000).toISOString(),
    LAST_HEARTBEAT: claimedAt,
    ROLLOVER_ATTEMPTS: '0',
    BLOCKED_REASON: 'NONE',
    BLOCKED_AT: 'NONE',
    BLOCKED_NOTIFICATION_STATUS: 'NONE',
  };
  writeControlAtomic(controlPath, claimed);
  return claimed;
}

function parseArgs(argv) {
  const out = {};
  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === '--control') out.controlPath = argv[++index];
    else if (arg === '--generation') out.generation = Number(argv[++index]);
    else if (arg === '--nonce') out.nonce = argv[++index];
    else throw new Error(`UNKNOWN_ARG:${arg}`);
  }
  return out;
}

if (require.main === module) {
  const claimed = claimGeneration(parseArgs(process.argv.slice(2)));
  process.stdout.write(`CLAIMED G${claimed.GENERATION} ${claimed.RUN_ID || ''}\n`);
}

module.exports = { claimGeneration, parseArgs };
