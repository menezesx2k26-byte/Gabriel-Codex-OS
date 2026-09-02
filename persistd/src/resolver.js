const fs = require('node:fs');
const path = require('node:path');
const { readControl } = require('./control-state');

const TERMINAL = new Set(['DONE', 'STALE', 'CANCELLED']);
const RANK = { PREPARING_TAKEOVER: 80, ROLLOVER_INCOMPLETE: 75, CONTEXT_RISK: 70, ACTIVE: 50, IDLE_INCOMPLETE: 45, WAITING_TOOL: 40, AUTH_REQUIRED: 25, WAITING_USER_APPROVAL: 20, BLOCKED: 15 };

function resolveRun(root, { includeSynthetic = false, includePendingDone = false, runId: requestedRunId = null } = {}) {
  const candidates = [];
  for (const dirent of fs.readdirSync(root, { withFileTypes: true })) {
    if (!dirent.isDirectory()) continue;
    const controlPath = path.join(root, dirent.name, 'CONTROL.md');
    if (!fs.existsSync(controlPath)) continue;
    let state;
    try { state = readControl(controlPath); } catch { continue; }
    const status = (state.STATUS || '').toUpperCase();
    if (TERMINAL.has(status)) {
      const cleanupStatus = state.BROWSER_CLEANUP_STATUS;
      const cleanupNextAt = Date.parse(state.BROWSER_CLEANUP_NEXT_AT || '');
      const cleanupDue = ['PENDING', 'FAILED'].includes(cleanupStatus)
        || (cleanupStatus === 'RETRY_SCHEDULED' && (!Number.isFinite(cleanupNextAt) || cleanupNextAt <= Date.now()));
      const pendingLifecycle = ['PENDING', 'FAILED'].includes(state.CHAT_TITLE_STATUS) || cleanupDue;
      const pendingDone = status === 'DONE' && includePendingDone
        && (state.NOTIFICATION_STATUS !== 'SENT' || pendingLifecycle);
      if (!pendingDone) continue;
    }
    const runId = state.RUN_ID || dirent.name;
    if (requestedRunId && runId !== requestedRunId) continue;
    const taskId = state.TASK_ID || '';
    const synthetic = /^(PCC-E2E|PERSIST-ALIAS)/.test(runId) || /synthetic/i.test(taskId);
    if (synthetic && !includeSynthetic) continue;
    const projectBonus = state.PROJECT_ROOT && state.PROJECT_ROOT !== 'NONE' ? 10 : 0;
    const terminalPendingBonus = status === 'DONE' ? 100 : 0;
    candidates.push({ ...state, RUN_ID: runId, CONTROL_PATH: controlPath, _rank: terminalPendingBonus + (RANK[status] || 10) + projectBonus, _mtime: fs.statSync(controlPath).mtimeMs });
  }
  if (!candidates.length) return null;
  // Within the same priority, service the stalest run first. The selected run
  // updates CONTROL.md on each tick, naturally rotating equal-priority runs
  // instead of letting the newest ACTIVE run monopolize the daemon forever.
  candidates.sort((a, b) => b._rank - a._rank || a._mtime - b._mtime);
  const winner = { ...candidates[0] };
  delete winner._rank; delete winner._mtime;
  return winner;
}

module.exports = { resolveRun };
