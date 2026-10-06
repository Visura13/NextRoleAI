# Performance test procedure

The repository includes a dependency-free Node load harness for a database-backed HTTP endpoint.

```powershell
$env:NEXTROLEAI_BASE_URL = "https://YOUR-API-HOST"
$env:NEXTROLEAI_REQUEST_COUNT = "100"
$env:NEXTROLEAI_CONCURRENCY = "10"
node scripts/performance/http-load-test.mjs | Tee-Object performance-results.json
```

The JSON result records target, concurrency, successes, failures, success rate, minimum, median, p95, maximum, and average latency. The default endpoint is the public paginated job catalogue. Set `NEXTROLEAI_LOAD_PATH` and `NEXTROLEAI_ACCESS_TOKEN` to test an authenticated read endpoint.

For agent latency, run five identical golden-case workflows and record each persisted step's `durationMilliseconds`, total API time, validation outcome, and whether a retry occurred. Agent creation is deliberately excluded from high-concurrency load because it writes durable audit data and requires a prepared confirmed CV per user.

## Acceptance targets for the assignment demonstration

- Health endpoint: 100% success, p95 under 500 ms after warm-up.
- Job catalogue at 10 concurrent requests: at least 99% success, p95 under 1500 ms after warm-up.
- Controlled agent workflow: five of five safe completions for the golden case, no validation bypass, and median total time under 10 seconds.
- Injection and no-result cases: safe failure with no approved shortlist.

These are project targets, not measured results. Paste the dated output from the deployed environment into the consolidated report only after running the procedure. Free-tier cold starts must be reported separately rather than removed from evidence.
