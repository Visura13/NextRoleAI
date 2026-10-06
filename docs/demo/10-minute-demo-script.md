# Ten-minute demonstration script

Prepare the Recruiter company and one draft job before recording, but create/publish the demonstrated job on screen. Keep Swagger, React, an Android emulator, GitHub Actions, and the architecture diagram in separate tabs.

## 0:00-1:00 — Problem and architecture

Explain the CV-to-job-ranking problem, the Job Seeker and Recruiter roles, and why React and Flutter share one ASP.NET Core/PostgreSQL API. Show `docs/architecture/system-architecture.md`.

## 1:00-2:15 — Authentication and role authorization

Log in as each role in React. Show protected navigation. Briefly demonstrate that a Job Seeker cannot enter the Recruiter workspace. Mention short-lived JWT access tokens and rotated hashed refresh tokens.

## 2:15-3:30 — Recruiter CRUD and PostgreSQL

Create, edit, and publish a job with required/optional skills. Show search/filter/sort/pagination briefly. Open Swagger and point to REST routes, request validation, and the health URL. Explain the company ownership constraint and migration-backed schema.

## 3:30-5:00 — CV and deterministic ranking

As Job Seeker, upload the prepared non-sensitive CV, review/correct extracted fields, and confirm them. Show ranked jobs, score breakdown, reasons, and missing skills. Explain that unconfirmed extraction cannot influence ranking and files are private.

## 5:00-7:00 — Agentic workflow

Start a safe objective. Show the four distinct roles, fixed allowed tools, structured calls, rule results, durations, and `PendingApproval`. Approve it and show the published shortlist. Then submit an injection-style objective and show safe failure with no shortlist. Explain retry, timeout, and audit behavior without claiming hidden reasoning is stored.

## 7:00-8:30 — Cross-platform application lifecycle

Submit an application from Flutter or React, open it in Recruiter React, change it to In Review then Shortlisted or Rejected, and refresh both Job Seeker clients. Show identical status history and notification audit.

## 8:30-9:30 — Quality and deployment

Show the live React URL, `/health`, Swagger, a green Actions run, coverage artifact/log, and downloadable APK. Mention the real PostgreSQL integration-test service and performance JSON. State the free-tier CV storage limitation honestly.

## 9:30-10:00 — Git and conclusion

Show focused PRs and commit history. Summarize explainability, least privilege, human control, and what you personally implemented. End with the repository and evaluator-access page.

Do a timed rehearsal and keep a backup local recording. Never expose tokens, API keys, passwords, or real CV data.
