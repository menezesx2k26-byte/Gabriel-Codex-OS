function decideAction({ reversible, lowRisk, scopePreserving, logicallyDetermined }) {
  const auto = reversible === true && lowRisk === true && scopePreserving === true && logicallyDetermined === true;
  return auto
    ? { decision: 'AUTO_EXECUTE', reason: 'reversible_low_risk_logical' }
    : { decision: 'REQUIRE_USER', reason: 'risk_scope_or_ambiguity' };
}

module.exports = { decideAction };
