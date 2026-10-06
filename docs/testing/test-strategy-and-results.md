# Test strategy and evidence

## Automated coverage

| Layer | Command | Current automated scope |
|---|---|---|
| Backend unit | `dotnet test NextRoleAI.sln --configuration Release` | 32 cases covering health/controller behavior, refresh-token lifecycle, scoring, application transitions, database-URL configuration, planning, prompt safety, and deterministic agent validation |
| Backend integration | same command with `NEXTROLEAI_TEST_CONNECTION_STRING` | 10 PostgreSQL-backed journeys covering authentication, ownership, constraints, CRUD/search/filter/sort/pagination, CV replacement and confirmation, recommendations, agent approval, applications, Swagger, security headers, and role authorization |
| React | `npm test -- --run` | 9 component/client cases covering forms, protected routes, role separation, job display, workflow approval evidence, application views, and multipart requests |
| Flutter | `flutter test` | 18 API, state, validation, workflow, application, and widget cases |
| Static quality | `npm run lint`, `npm run build`, `flutter analyze`, Dart format, .NET Release build | Compilation, type checking, linting, formatting, and platform packaging |

PostgreSQL integration tests intentionally skip when the disposable test connection variable is absent. Backend CI provisions PostgreSQL 17 and therefore runs them rather than reporting a false in-memory result. CI also collects XPlat Code Coverage.

## Security cases

- Anonymous requests receive `401` on authenticated endpoints.
- A Job Seeker receives `403` on Recruiter endpoints.
- Owner-scoped resources return `404` to other authenticated users.
- Password hashing, JWT validation, refresh rotation/reuse revocation, CV signature/size/type checks, prompt-injection blocking, tool allow-lists, validation rules, and explicit human approval are automated.
- OpenAPI remains public for evaluation while protected operations retain bearer requirements.
- Standard response hardening headers are asserted.

## End-to-end trace

`CvAndRecommendationEndpointsTests.JobSeeker_CanUploadConfirmAndRankJobs` covers Job Seeker registration, profile, real DOCX upload, extraction, replacement, confirmation, Recruiter/company/job publication, deterministic recommendation, controlled agent execution, cross-owner denial, approval, prompt-injection safe failure, and CV deletion against PostgreSQL.

`ApplicationWorkflowEndpointsTests.ApplicationLifecycle_IsOwnedAuditedAndVisibleAcrossRoles` continues the shared workflow through application submission, Recruiter review/decision, owner-visible status history, and notification audit. The React and Flutter tests verify their corresponding UI state and API adapter behavior.

## Evidence procedure

1. Open the Part 9 GitHub Actions run and capture all three green jobs.
2. Download the `NextRoleAI-android-debug` artifact from Flutter CI.
3. Save backend coverage output from the run.
4. Run the manual cross-client script in `docs/deployment/evaluator-access.md` against the live services.
5. Record date, browser/device, commit SHA, observed result, and screenshot filename in the submission report.

Do not claim a live or performance result until it has actually been run against the deployed URLs.
