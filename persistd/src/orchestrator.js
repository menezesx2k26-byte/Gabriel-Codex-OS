const path = require('node:path');
const crypto = require('node:crypto');
const { resolveRun } = require('./resolver');
const { readControl, writeControlAtomic } = require('./control-state');
const { acquireLock, claimLease } = require('./lease');
const { buildSuccessorMessage } = require('./handoff');
const { claimGeneration } = require('./claim-generation');
const { buildClaimRequestLine } = require('./claim-protocol');
const { buildTerminalMessage, buildAttentionMessage } = require('./notifier');

function buildChatTitle(displayName, generation, totalGenerations) {
  const base = String(displayName || 'Persist').trim() || 'Persist';
  return totalGenerations
    ? `${base} ${generation} de ${totalGenerations}`
    : `${base} ${generation}`;
}

function resolveDisplayName(state = {}) {
  const explicit = String(state.DISPLAY_NAME || '').trim();
  if (explicit && explicit !== 'NONE') return explicit;

  const taskFirst = String(state.TASK_ID || '').trim().split(/[\s_-]+/)[0];
  if (taskFirst && /[a-z][A-Z]/.test(taskFirst)) return taskFirst;

  const runFirst = String(state.RUN_ID || '').trim().split(/[\s_-]+/)[0];
  const token = runFirst || taskFirst;
  if (!token) return 'Persist';
  if (/^[A-Z0-9]+$/.test(token) || /^[a-z0-9]+$/.test(token)) {
    return token.charAt(0).toUpperCase() + token.slice(1).toLowerCase();
  }
  return token;
}

function appendChatHistory(state, chatId) {
  if (!chatId) return state;
  let history = [];
  try { history = JSON.parse(state.CHAT_HISTORY_JSON || '[]'); } catch {}
  if (history.some((item) => item.chatId === chatId)) return state;
  return {
    ...state,
    CHAT_HISTORY_JSON: JSON.stringify([...history, { index: history.length + 1, chatId }]),
  };
}

async function notifyBlockedOnce({ controlPath, state, notifier, now }) {
  if (state.BLOCKED_NOTIFICATION_STATUS === 'SENT' || !notifier?.attention) return state;
  const attention = await notifier.attention({
    state, kind: 'BLOCKED_FATAL', message: buildAttentionMessage(state, 'BLOCKED_FATAL'),
  });
  const updated = attention?.sent ? {
    ...state, BLOCKED_NOTIFICATION_STATUS: 'SENT', BLOCKED_NOTIFIED_AT: now.toISOString(),
    BLOCKED_NOTIFICATION_CHANNEL: attention.channel || 'unknown',
  } : { ...state, BLOCKED_NOTIFICATION_STATUS: 'FAILED' };
  writeControlAtomic(controlPath, updated);
  return updated;
}

function isRolloverDue(state, now, rolloverMinutes) {
  if (['CONTEXT_RISK', 'ROLLOVER_INCOMPLETE', 'PREPARING_TAKEOVER'].includes(state.STATUS)) return true;
  const anchorText = state.CLAIM_RESUMED_AT && state.CLAIM_RESUMED_AT !== 'NONE'
    ? state.CLAIM_RESUMED_AT
    : (state.CLAIMED_AT && state.CLAIMED_AT !== 'NONE' ? state.CLAIMED_AT : state.STARTED_AT);
  if (!anchorText) return false;
  const anchor = Date.parse(anchorText);
  return Number.isFinite(anchor) && now.getTime() - anchor >= rolloverMinutes * 60_000;
}

async function recordRolloverFailure({ controlPath, generation, now, notifier, reason }) {
  const current = readControl(controlPath);
  if (Number.parseInt(current.GENERATION || '0', 10) !== generation) {
    return { action: 'ROLLOVER_INCOMPLETE', generation };
  }
  const attempts = Number.parseInt(current.ROLLOVER_ATTEMPTS || '0', 10) + 1;
  if (attempts >= 3) {
    const blocked = {
      ...current, STATUS: 'BLOCKED', LEASE_OWNER: `G${generation}`,
      ROLLOVER_ATTEMPTS: String(attempts), BLOCKED_REASON: reason,
      BLOCKED_AT: now.toISOString(),
    };
    writeControlAtomic(controlPath, blocked);
    await notifyBlockedOnce({ controlPath, state: blocked, notifier, now });
    return { action: 'BLOCKED', generation };
  }
  writeControlAtomic(controlPath, {
    ...current, STATUS: 'ROLLOVER_INCOMPLETE', LEASE_OWNER: `G${generation}`,
    ROLLOVER_ATTEMPTS: String(attempts), BLOCKED_REASON: reason,
  });
  return { action: 'ROLLOVER_INCOMPLETE', generation };
}

function parseChatHistory(state) {
  try {
    const value = JSON.parse(state.CHAT_HISTORY_JSON || '[]');
    return Array.isArray(value) ? value : [];
  } catch { return []; }
}

function mergeDiscoveredChatHistory(state, discoveredChats = []) {
  const byId = new Map();
  for (const item of [...parseChatHistory(state), ...discoveredChats]) {
    if (!item?.chatId || byId.has(item.chatId)) continue;
    byId.set(item.chatId, { chatId: item.chatId });
  }
  return [...byId.values()]
    .sort((a, b) => a.chatId.localeCompare(b.chatId))
    .map((item, index) => ({ index: index + 1, chatId: item.chatId }));
}

async function reconcileChatPresentation({ controlPath, state, browser }) {
  if (!browser?.discoverRunChats) return state;
  let discovery;
  try { discovery = await browser.discoverRunChats({ state }); } catch { return state; }
  const fresh = readControl(controlPath);
  if (fresh.GENERATION !== state.GENERATION || fresh.STATUS !== state.STATUS) return fresh;
  const taskSpaceId = discovery?.taskSpaceId || fresh.BROWSER_TASKSPACE_ID || 'NONE';
  if (taskSpaceId === fresh.BROWSER_TASKSPACE_ID) return fresh;
  const updated = { ...fresh, BROWSER_TASKSPACE_ID: taskSpaceId };
  writeControlAtomic(controlPath, updated);
  return updated;
}

async function retryActivePrune({ controlPath, state, browser, now }) {
  const pending = ['PENDING', 'FAILED'].includes(state.BROWSER_PRUNE_STATUS);
  const keepChatId = state.BROWSER_PRUNE_CHAT_ID;
  if (!pending || !keepChatId || keepChatId === 'NONE' || !browser?.pruneRunTabs) return state;
  let updated;
  try {
    const result = await browser.pruneRunTabs({ state, keepChatId });
    updated = { ...state, BROWSER_PRUNE_STATUS: result?.ok ? 'SENT' : 'FAILED',
      BROWSER_PRUNED_AT: result?.ok ? now.toISOString() : (state.BROWSER_PRUNED_AT || 'NONE') };
  } catch {
    updated = { ...state, BROWSER_PRUNE_STATUS: 'FAILED' };
  }
  writeControlAtomic(controlPath, updated);
  return updated;
}

async function cleanupRunScratchQuietly({ browser, state }) {
  if (!browser?.cleanupRunScratchTabs) return;
  try { await browser.cleanupRunScratchTabs({ state }); } catch {}
}

async function closeFailedSuccessor({ browser, state, outcome }) {
  let closed = false;
  if (browser?.closeRunChat && outcome?.chatId) {
    try {
      const result = await browser.closeRunChat({ state, chatId: outcome.chatId });
      closed = Boolean(result?.ok === true && Number(result?.closed ?? 1) > 0);
    } catch {}
  }
  if (!closed && browser?.closeRunTarget && outcome?.targetId) {
    try { await browser.closeRunTarget({ state, targetId: outcome.targetId }); } catch {}
  }
  await cleanupRunScratchQuietly({ browser, state });
}

async function retryClaimConfirmation({ controlPath, state, browser, now }) {
  if (!['PENDING', 'FAILED'].includes(state.CLAIM_CONFIRM_STATUS)) return state;
  if (state.CLAIM_RESUMED_AT && state.CLAIM_RESUMED_AT !== 'NONE') {
    if (state.CLAIM_CONFIRM_STATUS === 'SENT') return state;
    const resumed = { ...state, CLAIM_CONFIRM_STATUS: 'SENT' };
    writeControlAtomic(controlPath, resumed);
    return resumed;
  }
  const chatId = state.CLAIM_CONFIRM_CHAT_ID;
  if (!chatId || chatId === 'NONE' || !browser?.sendClaimConfirmation) return state;
  let updated;
  try {
    const result = await browser.sendClaimConfirmation({
      state, chatId, generation: Number.parseInt(state.GENERATION || '0', 10), nonce: state.CLAIM_NONCE,
    });
    const rateLimited = Boolean(result?.rateLimited || result?.status === 'RATE_LIMITED');
    if (rateLimited) {
      const count = Number.parseInt(state.RATE_LIMIT_COUNT || '0', 10) + 1;
      const delayMs = Math.min(30 * 60_000, 2 * 60_000 * (2 ** Math.min(count - 1, 4)));
      updated = { ...state, CLAIM_CONFIRM_STATUS: 'RATE_LIMITED', RATE_LIMIT_COUNT: String(count),
        RATE_LIMIT_UNTIL: new Date(now.getTime() + delayMs).toISOString(), RATE_LIMIT_REASON: 'TOO_MANY_REQUESTS' };
    } else {
      const ok = Boolean(result?.ok);
      updated = { ...state, CLAIM_CONFIRM_STATUS: ok ? 'SENT' : 'FAILED',
        CLAIM_CONFIRMED_AT: ok ? ((state.CLAIM_CONFIRMED_AT && state.CLAIM_CONFIRMED_AT !== 'NONE') ? state.CLAIM_CONFIRMED_AT : now.toISOString()) : (state.CLAIM_CONFIRMED_AT || 'NONE'),
        CLAIM_RESUMED_AT: ok ? now.toISOString() : (state.CLAIM_RESUMED_AT || 'NONE'),
        RATE_LIMIT_COUNT: ok ? '0' : (state.RATE_LIMIT_COUNT || '0'), RATE_LIMIT_UNTIL: ok ? 'NONE' : (state.RATE_LIMIT_UNTIL || 'NONE'),
        RATE_LIMIT_REASON: ok ? 'NONE' : (state.RATE_LIMIT_REASON || 'NONE') };
    }
  } catch {
    updated = { ...state, CLAIM_CONFIRM_STATUS: 'FAILED' };
  }
  writeControlAtomic(controlPath, updated);
  return updated;
}

async function finalizeDoneLifecycle({ controlPath, state, browser, now, generation }) {
  let current = state;
  if (['PENDING', 'FAILED'].includes(current.CHAT_TITLE_STATUS)) {
    current = { ...current, CHAT_TITLE_STATUS: 'SKIPPED_SAFE', CHAT_TITLE_ATTEMPTS: '0' };
    writeControlAtomic(controlPath, current);
  }

  if (['PENDING', 'FAILED'].includes(current.BROWSER_CLEANUP_STATUS)) {
    if (!browser?.cleanupRun) return { action: 'BROWSER_CLEANUP_PENDING', generation };
    try {
      const result = await browser.cleanupRun({ state: current });
      if (!(result?.closed || result?.done)) throw new Error('BROWSER_CLEANUP_INCOMPLETE');
      current = { ...current, BROWSER_CLEANUP_STATUS: 'SENT', BROWSER_CLEANED_AT: now.toISOString(), BROWSER_CLEANUP_ATTEMPTS: '0' };
    } catch {
      const attempts = Number.parseInt(current.BROWSER_CLEANUP_ATTEMPTS || '0', 10) + 1;
      current = { ...current, BROWSER_CLEANUP_STATUS: 'FAILED', BROWSER_CLEANUP_ATTEMPTS: String(attempts) };
    }
    writeControlAtomic(controlPath, current);
    if (current.BROWSER_CLEANUP_STATUS !== 'SENT') return { action: 'BROWSER_CLEANUP_RETRY', generation };
  }
  return { action: 'FINALIZED', generation };
}

async function tick({ root, browser, notifier, clock = () => new Date(), rolloverMinutes = 20, includeSynthetic = false, runId = null }) {
  const resolved = resolveRun(root, { includeSynthetic, includePendingDone: true, runId });
  if (!resolved) return { action: 'NO_INCOMPLETE_RUN' };
  const controlPath = resolved.CONTROL_PATH;
  const release = acquireLock(path.join(path.dirname(controlPath), '.persistd.lock'));
  try {
    const now = clock();
    let state = readControl(controlPath);
    let generation = Number.parseInt(state.GENERATION || '1', 10);
    if (state.STATUS === 'BLOCKED') {
      await notifyBlockedOnce({ controlPath, state, notifier, now });
      return { action: 'BLOCKED', generation };
    }
    if (state.STATUS !== 'DONE') await cleanupRunScratchQuietly({ browser, state });
    if (state.CLAIM_CONFIRM_STATUS === 'RATE_LIMITED') {
      const until = Date.parse(state.RATE_LIMIT_UNTIL || '');
      if (Number.isFinite(until) && now.getTime() < until) {
        return { action: 'RATE_LIMIT_BACKOFF', generation, retryAt: state.RATE_LIMIT_UNTIL };
      }
      state = { ...state, CLAIM_CONFIRM_STATUS: 'FAILED', RATE_LIMIT_UNTIL: 'NONE' };
      writeControlAtomic(controlPath, state);
    }
    if (state.STATUS !== 'DONE' && ['PENDING', 'FAILED'].includes(state.CLAIM_CONFIRM_STATUS)) {
      state = await retryClaimConfirmation({ controlPath, state, browser, now });
      generation = Number.parseInt(state.GENERATION || '1', 10);
      if (state.CLAIM_CONFIRM_STATUS === 'RATE_LIMITED') return { action: 'RATE_LIMIT_BACKOFF', generation, retryAt: state.RATE_LIMIT_UNTIL };
      if (state.CLAIM_CONFIRM_STATUS !== 'SENT') return { action: 'CLAIM_CONFIRM_RETRY', generation };
    }
    if (state.STATUS !== 'DONE') state = await retryActivePrune({ controlPath, state, browser, now });
    state = await reconcileChatPresentation({
      controlPath, state, browser, activeTitles: state.STATUS !== 'DONE',
    });
    generation = Number.parseInt(state.GENERATION || '1', 10);
    if (state.STATUS === 'DONE') {
      if (state.NOTIFICATION_STATUS !== 'SENT') {
        if (!notifier?.terminal) return { action: 'NOTIFICATION_PENDING', generation };
        const notice = await notifier.terminal({ state, message: buildTerminalMessage(state) });
        if (!notice?.sent) {
          writeControlAtomic(controlPath, { ...state, FINAL_GENERATION: String(generation), NOTIFICATION_STATUS: 'FAILED' });
          return { action: 'NOTIFICATION_FAILED', generation };
        }
        state = {
          ...state, FINAL_GENERATION: String(generation), DONE_AT: state.DONE_AT || now.toISOString(),
          NOTIFICATION_STATUS: 'SENT', NOTIFIED_AT: now.toISOString(), NOTIFICATION_CHANNEL: notice.channel || 'unknown',
        };
        writeControlAtomic(controlPath, state);
      }
      const pendingLifecycle = ['PENDING', 'FAILED'].includes(state.CHAT_TITLE_STATUS)
        || ['PENDING', 'FAILED'].includes(state.BROWSER_CLEANUP_STATUS);
      if (!pendingLifecycle) return { action: 'DONE', generation };
      return finalizeDoneLifecycle({ controlPath, state, browser, now, generation });
    }

    state = claimLease(state, `G${generation}`, now, 90_000);
    writeControlAtomic(controlPath, state);
    if (!isRolloverDue(state, now, rolloverMinutes)) return { action: 'WATCHING', generation };

    if (browser?.isRunChatBusy && state.CHAT_ID && state.CHAT_ID !== 'NONE') {
      try {
        const activity = await browser.isRunChatBusy({ state, chatId: state.CHAT_ID });
        if (activity?.busy) return { action: 'WORKER_BUSY', generation };
        if (activity?.ok === false) return { action: 'WORKER_ACTIVITY_RETRY', generation };
      } catch {
        return { action: 'WORKER_ACTIVITY_RETRY', generation };
      }
    }

    const nonce = crypto.randomUUID();
    const nextGeneration = generation + 1;
    state = {
      ...state,
      STATUS: 'PREPARING_TAKEOVER',
      CLAIM_NONCE: nonce,
      NEXT_GENERATION: String(nextGeneration),
      LAST_HEARTBEAT: now.toISOString(),
    };
    writeControlAtomic(controlPath, state);
    const message = buildSuccessorMessage({ ...state, CONTROL_PATH: controlPath }, nextGeneration);
    let outcome;
    try {
      outcome = await browser.createSuccessor({ state, message, nextGeneration, controlPath });
    } catch (error) {
      await cleanupRunScratchQuietly({ browser, state });
      return recordRolloverFailure({
        controlPath, generation, now, notifier, reason: 'BROWSER_ERROR',
      });
    }

    if (outcome?.status === 'AUTH_REQUIRED') {
      const current = readControl(controlPath);
      if (Number.parseInt(current.GENERATION || '0', 10) === generation) {
        writeControlAtomic(controlPath, { ...current, STATUS: 'AUTH_REQUIRED', LEASE_OWNER: `G${generation}` });
      }
      if (notifier?.attention) {
        let authState = readControl(controlPath);
        if (authState.AUTH_NOTIFICATION_STATUS !== 'SENT') {
          const attention = await notifier.attention({ state: authState, kind: 'AUTH_REQUIRED', message: buildAttentionMessage(authState, 'AUTH_REQUIRED') });
          if (attention?.sent) {
            authState = {
              ...authState, AUTH_NOTIFICATION_STATUS: 'SENT', AUTH_NOTIFIED_AT: now.toISOString(),
              AUTH_NOTIFICATION_CHANNEL: attention.channel || 'unknown',
            };
            writeControlAtomic(controlPath, authState);
          }
        }
      }
      return { action: 'AUTH_REQUIRED', generation };
    }

    let claimed = readControl(controlPath);
    let twoPhaseClaim = false;
    const legacyValidClaim = Number.parseInt(claimed.GENERATION || '0', 10) === nextGeneration
      && claimed.STATUS === 'ACTIVE'
      && claimed.CLAIM_NONCE === nonce
      && Boolean(claimed.CLAIMED_AT);

    if (!legacyValidClaim) {
      if (outcome?.status === 'BROWSER_ERROR' || !outcome?.chatId) {
        await closeFailedSuccessor({ browser, state, outcome });
        return recordRolloverFailure({ controlPath, generation, now, notifier, reason: 'BROWSER_ERROR' });
      }
      const expectedRequest = buildClaimRequestLine(state.RUN_ID, nextGeneration, nonce);
      let requestVerified = outcome?.requestLine === expectedRequest;
      if (!requestVerified && browser?.verifyAssistantLine) {
        try {
          const verification = await browser.verifyAssistantLine({ state, chatId: outcome.chatId, line: expectedRequest });
          requestVerified = Boolean(verification?.ok);
        } catch { requestVerified = false; }
      }
      if (!requestVerified) {
        await closeFailedSuccessor({ browser, state, outcome });
        return recordRolloverFailure({ controlPath, generation, now, notifier, reason: 'CLAIM_REQUEST_NOT_VERIFIED' });
      }
      try {
        claimed = claimGeneration({ controlPath, generation: nextGeneration, nonce, now });
        twoPhaseClaim = true;
      } catch {
        await closeFailedSuccessor({ browser, state, outcome });
        return recordRolloverFailure({ controlPath, generation, now, notifier, reason: 'CLAIM_REQUEST_REJECTED' });
      }
    }

    let finalState = claimLease({
      ...claimed,
      CHAT_ID: outcome?.chatId || claimed.CHAT_ID || 'NONE',
      BROWSER_TASKSPACE_ID: outcome?.taskSpaceId || claimed.BROWSER_TASKSPACE_ID || 'NONE',
      TAKEOVER_EVIDENCE: outcome?.evidence || 'durable-claim-observed',
      ROLLOVER_ATTEMPTS: '0', BLOCKED_REASON: 'NONE', BLOCKED_AT: 'NONE',
      BLOCKED_NOTIFICATION_STATUS: 'NONE',
      DISPLAY_NAME: resolveDisplayName(claimed),
      CHAT_TITLE_STATUS: 'PENDING', BROWSER_CLEANUP_STATUS: 'PENDING',
      BROWSER_PRUNE_STATUS: outcome?.chatId ? 'PENDING' : 'SKIPPED',
      BROWSER_PRUNE_CHAT_ID: outcome?.chatId || 'NONE',
      CLAIM_CONFIRM_STATUS: twoPhaseClaim ? 'PENDING' : 'SKIPPED',
      CLAIM_CONFIRM_CHAT_ID: twoPhaseClaim ? (outcome?.chatId || 'NONE') : 'NONE',
      CLAIM_CONFIRMED_AT: 'NONE',
      CLAIM_RESUMED_AT: 'NONE',
    }, `G${nextGeneration}`, now, 90_000);
    finalState = appendChatHistory(finalState, outcome?.chatId);
    writeControlAtomic(controlPath, finalState);

    if (browser?.renameChat && outcome?.chatId) {
      const history = JSON.parse(finalState.CHAT_HISTORY_JSON || '[]');
      const title = buildChatTitle(finalState.DISPLAY_NAME, history.length);
      try { await browser.renameChat({ state: finalState, chatId: outcome.chatId, title }); } catch {}
    }
    if (outcome?.chatId) finalState = await retryActivePrune({ controlPath, state: finalState, browser, now });

    if (twoPhaseClaim) {
      finalState = await retryClaimConfirmation({ controlPath, state: finalState, browser, now });
    }
    if (twoPhaseClaim && finalState.CLAIM_CONFIRM_STATUS !== 'SENT') {
      return { action: 'CLAIM_CONFIRM_RETRY', generation: nextGeneration };
    }
    return { action: 'ROLLED_OVER', generation: nextGeneration };
  } finally {
    release();
  }
}

module.exports = { tick, isRolloverDue, buildChatTitle, resolveDisplayName, appendChatHistory };
