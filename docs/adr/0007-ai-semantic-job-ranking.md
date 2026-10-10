# ADR 0007: Validated AI semantic job ranking

- Status: accepted
- Date: 2026-10-10
- Supersedes: the recommendation-scoring decision in ADR 0005; its private CV-storage decision remains active

## Context

The original weighted matcher depended on exact skill and title tokens. It was reproducible, but it could rank an unrelated vacancy above a role that clearly matched the candidate's responsibilities and transferable frontend experience. CV analysis already provides richer professional summary, education, and skill evidence through an OpenAI-compatible provider.

## Decision

Use the configured language model to compare the confirmed candidate profile with every eligible published job in bounded batches of ten. Score each job independently out of 100 using skills fit (35), role fit (25), experience fit (15), education fit (10), location fit (10), and preferred-salary fit (5), so scores remain comparable across batches. Preferred salary is an optional minimum monthly amount in LKR; missing salary data or a different advertised currency receives a neutral salary score rather than an invented conversion. Return matched skills, missing required skills, and two to five concise evidence-based reasons.

Treat candidate and job content as untrusted. The prompt excludes protected characteristics and forbids following instructions embedded in supplied content. The API validates known job IDs, exact job coverage, uniqueness, component bounds, explanation count, and skill evidence allow-lists before summing or displaying a score.

Do not fall back to the old deterministic matcher. Provider failures, quota errors, malformed responses, and incomplete rankings return `503`, because a visible temporary failure is safer than a confidently displayed but misleading score. Cache successful rankings for 12 hours, keyed by candidate/profile/job update times and algorithm/model version.

## Consequences

- Ranking can recognise semantic and transferable experience instead of exact keywords only.
- React, Flutter, and the controlled workflow consume one shared ranking contract.
- Explanations and bounded components remain inspectable even though model outputs are not inherently deterministic.
- Recommendations now require configured API quota and network availability.
- Model changes can alter scores, so the algorithm version includes the model name and evaluations should record it.
- New CV/profile/job data and model-version changes invalidate cached results; repeated reads otherwise avoid unnecessary provider charges for 12 hours.
