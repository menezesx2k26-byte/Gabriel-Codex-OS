const test = require('node:test');
const assert = require('node:assert/strict');
const { buildSuccessorMessage } = require('./src/handoff');

const state = {
  RUN_ID: 'remote-control-contract-test',
  CLAIM_NONCE: 'nonce-123',
  PROJECT_ROOT: 'C:\\repo',
  TASK_ID: 'task-1',
  CONTROL_PATH: 'C:\\control.md',
};

test('successor baton makes the remote-control capability gate mandatory', () => {
  const message = buildSuccessorMessage(state, 2);
  assert.match(message, /remote-control-contract\.md/);
  assert.match(message, /Remote Desktop Commander/);
  assert.match(message, /Browser Bridge \/ Playwright/);
  assert.match(message, /Windows Interactive Control/);
  assert.match(message, /highest applicable healthy structured control layer/i);
});
