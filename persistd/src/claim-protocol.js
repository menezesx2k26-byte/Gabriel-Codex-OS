function buildClaimRequestLine(runId, generation, nonce) {
  return `CLAIM_REQUEST ${runId} G${generation} ${nonce}`;
}

function buildClaimConfirmationLine(runId, generation, nonce) {
  return `CLAIM_CONFIRMED ${runId} G${generation} ${nonce}`;
}

function parseClaimRequestLine(text) {
  const line = String(text || '').trim();
  const match = /^CLAIM_REQUEST\s+(\S+)\s+G(\d+)\s+(\S+)$/.exec(line);
  if (!match) return null;
  return {
    runId: match[1],
    generation: Number.parseInt(match[2], 10),
    nonce: match[3],
    line,
  };
}

function isExpectedClaimRequest(text, { runId, generation, nonce }) {
  const parsed = parseClaimRequestLine(text);
  return Boolean(parsed
    && parsed.runId === runId
    && parsed.generation === generation
    && parsed.nonce === nonce);
}

module.exports = {
  buildClaimRequestLine,
  buildClaimConfirmationLine,
  parseClaimRequestLine,
  isExpectedClaimRequest,
};