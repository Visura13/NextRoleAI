# NextRoleAI implementation plan

Each part should finish with working code, automated verification, a focused commit, and a GitHub push before the next part starts.

## Part 1 - Repository and API foundation

- ASP.NET Core solution and layered project boundaries
- Health endpoints and first API test
- Repository conventions and backend CI

## Part 2 - PostgreSQL and authentication

- PostgreSQL and Entity Framework Core
- ASP.NET Core Identity with JobSeeker and Recruiter roles
- JWT access tokens and refresh-token rotation
- Registration, login, logout, refresh, and current-user endpoints
- Integration tests for authentication and authorization

## Part 3 - Profiles and recruiter job management

- Job-seeker and recruiter/company profiles
- Job-posting entities, skills, validation, ownership, and status
- CRUD, search, filtering, sorting, and pagination
- Database constraints, indexes, migrations, seed data, and tests

## Part 4 - React web application

- Shared authentication and protected routes
- Job-seeker profile, jobs, and application screens
- Recruiter company, job-management, and application-review screens
- Loading, empty, validation, success, and error states

## Part 5 - Flutter job-seeker application

- Shared authentication with secure token storage
- CV file picker or camera capture
- Job browsing, recommendations, applications, and status tracking
- Mobile validation and widget tests

## Part 6 - CV processing and deterministic ranking

- Secure CV upload and storage abstraction
- Structured CV profile and user correction flow
- Testable weighted job-matching engine
- Match breakdowns and explanations

## Part 7 - Controlled Agentic AI workflow

- Planning, CV Profile, Job Discovery, and Validation agents
- Allow-listed tools and structured input/output contracts
- Durable workflow state, retries, timeouts, logs, and safe failures
- Prompt-injection defenses and deterministic validation

## Part 8 - Cross-platform approval and notifications

- Application workflow from Flutter or React
- Recruiter decision through React
- Updated Job Seeker status in both clients
- Auditable decisions and third-party notification integration

## Part 9 - Quality, deployment, and assessment evidence

- End-to-end, performance, security, and agent-evaluation tests
- Deployment and APK generation
- Architecture diagrams, ER diagram, ADRs, reports, and README completion
- AI usage log, demonstration script, and viva preparation
