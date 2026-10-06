import { performance } from 'node:perf_hooks';

const baseUrl = (process.env.NEXTROLEAI_BASE_URL ?? 'http://localhost:5251').replace(/\/$/, '');
const path = process.env.NEXTROLEAI_LOAD_PATH ?? '/api/jobs?page=1&pageSize=20';
const requestCount = positiveInteger(process.env.NEXTROLEAI_REQUEST_COUNT, 100);
const concurrency = positiveInteger(process.env.NEXTROLEAI_CONCURRENCY, 10);
const token = process.env.NEXTROLEAI_ACCESS_TOKEN;
const durations = [];
let succeeded = 0;
let failed = 0;
let cursor = 0;

async function worker() {
  while (true) {
    const requestNumber = cursor++;
    if (requestNumber >= requestCount) return;

    const started = performance.now();
    try {
      const response = await fetch(`${baseUrl}${path}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      await response.arrayBuffer();
      if (response.ok) succeeded += 1;
      else failed += 1;
    } catch {
      failed += 1;
    } finally {
      durations.push(performance.now() - started);
    }
  }
}

const runStarted = new Date().toISOString();
await Promise.all(Array.from({ length: Math.min(concurrency, requestCount) }, worker));
durations.sort((left, right) => left - right);

const report = {
  runStarted,
  target: `${baseUrl}${path}`,
  requestCount,
  concurrency,
  succeeded,
  failed,
  successRatePercent: Number(((succeeded / requestCount) * 100).toFixed(2)),
  latencyMilliseconds: {
    minimum: rounded(durations[0]),
    median: rounded(percentile(0.5)),
    p95: rounded(percentile(0.95)),
    maximum: rounded(durations.at(-1)),
    average: rounded(durations.reduce((sum, value) => sum + value, 0) / durations.length),
  },
};

console.log(JSON.stringify(report, null, 2));
process.exitCode = failed === 0 ? 0 : 1;

function positiveInteger(value, fallback) {
  const parsed = Number.parseInt(value ?? '', 10);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function percentile(fraction) {
  const index = Math.min(durations.length - 1, Math.ceil(durations.length * fraction) - 1);
  return durations[index];
}

function rounded(value) {
  return Number(value.toFixed(2));
}
