# NextRoleAI implementation plan

Each part should finish with working code, automated verification, a focused commit, and a GitHub push before the next part starts.

## Part 1 - Repository and API foundation

- Status: complete
- ASP.NET Core solution and layered project boundaries
- Health endpoints and first API test
- Repository conventions and backend CI

## Part 2 - PostgreSQL and authentication

- Status: complete and merged
- PostgreSQL and Entity Framework Core
- ASP.NET Core Identity with JobSeeker and Recruiter roles
- JWT access tokens and refresh-token rotation
- Registration, login, logout, refresh, and current-user endpoints
- Integration tests for authentication and authorization

## Part 3 - Profiles and recruiter job management

- Status: complete and merged
- Job-seeker and recruiter/company profiles
- Job-posting entities, skills, validation, ownership, and status
- CRUD, search, filtering, sorting, and pagination
- Database constraints, indexes, migrations, seed data, and tests

## Part 4 - React web application

- Status: complete and merged
- Shared authentication and protected routes
- Job-seeker profile and public job discovery screens
- Recruiter company and job-management screens
- Loading, empty, validation, success, and error states
- Reserved application screens that clearly identify the Part 8 dependency

## Part 5 - Flutter job-seeker application

- Status: complete and merged
- Shared authentication with secure token storage
- Job Seeker registration, login, logout, and protected navigation
- Native CV document picker with type and size validation
- Shared profile editing and public job browsing, filtering, and details
- Clear reserved screens for Part 6 recommendations and Part 8 applications
- Mobile validation and widget tests

## Part 6 - CV processing and deterministic ranking

- Status: complete and merged
- Secure CV upload and storage abstraction
- Structured CV profile and user correction flow
- Testable weighted job-matching engine
- Match breakdowns and explanations

## Part 7 - Controlled Agentic AI workflow

- Status: complete on `feature/part-7-agentic-workflow`
- Planning, CV Profile, Job Discovery, and Validation agents
- Allow-listed tools and structured input/output contracts
- Durable workflow state, retries, timeouts, logs, and safe failures
- Prompt-injection defenses and deterministic validation
- Human approval, rejection, and full revision cycles in React and Flutter

## Part 8 - Cross-platform approval and notifications

- Status: not started
- Application workflow from Flutter or React
- Recruiter decision through React
- Updated Job Seeker status in both clients
- Auditable decisions and third-party notification integration

## Part 9 - Quality, deployment, and assessment evidence

- Status: not started
- End-to-end, performance, security, and agent-evaluation tests
- Deployment and APK generation
- Architecture diagrams, ER diagram, ADRs, reports, and README completion
- AI usage log, demonstration script, and viva preparation
