# Part 7 controlled Agentic AI workflow

Part 7 adds a durable, observable, human-controlled workflow for generating a Job Seeker shortlist. React and Flutter use the same API and PostgreSQL state, so a run started on one client can be reviewed on the other.

## Preconditions

- The caller must be authenticated as `JobSeeker`.
- A CV must have been uploaded, reviewed, and confirmed.
- Recruiter jobs must be published before the discovery tool can consider them.
- The objective must be between 20 and 500 characters and pass the instruction-injection guard.

## Endpoints

- `POST /api/agent-workflows` starts a controlled run with `{ "objective": "..." }`.
- `GET /api/agent-workflows?page=1&pageSize=10` returns the caller's runs and complete audit views.
- `GET /api/agent-workflows/{id}` returns one caller-owned run.
- `POST /api/agent-workflows/{id}/decision` accepts `Approve`, `Reject`, or `RequestRevision` plus an optional feedback note. `RequestRevision` also requires a revised objective.

Workflow access is owner-scoped in the persistence query as well as protected by role authorization at the controller boundary.

## Agents and tools

| Agent | Responsibility | Allowed tool |
| --- | --- | --- |
| Planning Agent | Build a structured, bounded execution plan | None |
| Candidate Profile Agent | Load the confirmed candidate evidence | `candidate-profile.read` |
| Job Discovery Agent | Rank published jobs and propose a bounded shortlist | `published-jobs.rank` |
| Validation & Safety Agent | Check plan, data, shortlist, and observed calls | None |
| Human Approval Gate | Pause before publication | `shortlist.publish`, only after approval |

Tool names are routed through a fixed allow-list. Inputs and outputs are serialized structured records; arbitrary code execution and client-selected tool names are not supported.

## State and safety

A successful first cycle moves through `Running` to `PendingApproval`. Approval publishes only the validated items and completes the run. Rejection completes without publication. Revision increments the revision number, discards the previous proposal, and reruns the full controlled cycle.

The validation gate records individual pass/fail evidence for prompt safety, structured planning, confirmed profile data, shortlist constraints, score bounds, and the observed tool allow-list. A failed rule prevents approval and publication. Agent steps use a ten-second timeout and one retry; exhausted operations are persisted as safe failures with a public error code and message.

The objective is always handled as untrusted data. Common instruction-injection phrases are rejected before planning. No model secrets, hidden reasoning, CV file contents, or credentials are written to the audit trail.

## Cross-platform demonstration

1. Confirm a CV and ensure at least one matching job is published.
2. Start the workflow from Flutter.
3. Open **AI workflows** in React and select the same pending run.
4. Inspect the proposed jobs, deterministic validation cards, agent timeline, and tool calls.
5. Approve in React.
6. Refresh Flutter and observe the completed status and approved shortlist.

The reverse direction also works because neither client owns workflow state.
