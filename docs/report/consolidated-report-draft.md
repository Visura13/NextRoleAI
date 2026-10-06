# NextRoleAI consolidated report — draft

> This is a technical draft, not a submission-ready personal report. Replace every `TODO`, add measured evidence, write the personal reflection yourself, and export the final document to PDF using the required university format.

## Cover and declaration

- Module: SE3090 Integrated Full-Stack and Agentic AI Application Development
- Project: NextRoleAI
- Student name: `TODO`
- Student ID: `TODO`
- Group/solo approval evidence: `TODO`
- Repository: <https://github.com/Visura13/NextRoleAI>
- Signed academic/AI declaration: `TODO — student completes`

## 1. Problem, scope, roles, and requirements

Job seekers often search many vacancies without a transparent explanation of how well each role fits their CV. NextRoleAI lets a Job Seeker upload and confirm a CV, receive deterministic explainable rankings, run a controlled multi-agent shortlist workflow, apply, and track decisions in React or Flutter. A Recruiter uses React to manage a company, publish jobs, and process applications.

The solo scope has two roles. Include written lecturer approval for the exception to the normal role/group expectation. Functional evidence includes authentication/authorization, profile and job CRUD, search/filter/sort/pagination, CV processing, ranking, agent approval, applications, reporting-style status/audit views, and Resend integration.

## 2. Technology and architecture

The system uses ASP.NET Core 10 Web API, EF Core with PostgreSQL 17, React/Vite/TypeScript, and Flutter/Dart. ASP.NET Core Identity and JWT provide shared authentication. Layered projects keep HTTP, use cases, domain rules, infrastructure, and orchestration separate. See `docs/architecture/system-architecture.md` and the ADRs for the decisions and trade-offs.

## 3. Data design

Insert the rendered `docs/architecture/er-diagram.md` diagram. Discuss PK/FK relationships, required fields, precision/length constraints, composite uniqueness, query indexes, audit timestamps, delete behavior, migrations, seed data, and transactions. Cite one concrete example for each from `ApplicationDbContext` and the migration history.

## 4. API and security

Describe REST resources, DTO validation, service DI, global problem responses, structured logging, CORS, health checks, Swagger, JWT roles, ownership, refresh rotation, secret configuration, CV isolation, and response hardening. Add the deployed health and Swagger URLs plus screenshots of anonymous `401`, wrong-role `403`, and successful bearer use.

## 5. React and Flutter clients

Show both role-aware React workspaces and the Job Seeker Flutter workflow. Explain protected navigation, state/API adapters, loading/empty/error/success states, validation, session/secure-token storage, and how both clients observe the same server-owned application history.

## 6. CV ranking and controlled Agentic AI

Explain deterministic weighted ranking and its evidence fields. Insert the agent sequence diagram. Describe planning/delegation, four specialized roles, tool allow-lists, typed JSON, persistent workflow state, retries, timeouts, rule validation, injection defense, safe failure, and human approval. Include a golden run and at least one blocked run using IDs and screenshots that contain no secrets.

## 7. Testing and evaluation

Summarize `docs/testing/test-strategy-and-results.md` and `docs/testing/agent-evaluation.md`. Insert final CI results, coverage, test counts, representative failure/fix evidence, and the manual cross-platform trace. Add real performance output using `docs/testing/performance-testing.md`; do not present acceptance targets as results.

## 8. Deployment and operation

Document the Render Blueprint, Docker build, private PostgreSQL connection, automatic migrations, environment variables, React static routing, APK build, startup order, test accounts, and incognito verification. Add live URLs/video. State the free-tier cold-start and ephemeral-CV limitations and production upgrades.

## 9. Git, contribution, challenges, and decisions

Use `docs/evidence/git-and-ci-evidence.md`, PR screenshots, commit links, and ADRs. Since the project is solo, explain your ownership of every component and how incremental PRs provided review/CI checkpoints.

## 10. Individual AI usage

Insert the completed, truthful `docs/ai/ai-usage-log.md`. Explain verification and rejected/changed suggestions. Do not claim unverified generated content as your own work.

## 11. Personal reflection

`TODO — write this section yourself without AI-generated prose. Use docs/report/personal-reflection-prompts.md only as questions.`

## 12. References and appendices

Reference official framework/provider documentation, the assignment specification, API summary, test output, performance JSON, demo script, evaluator access, screenshots, and relevant commit/PR URLs. Check every link from an incognito window before exporting the final PDF.
