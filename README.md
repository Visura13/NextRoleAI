# NextRoleAI

NextRoleAI is an AI-assisted recruitment platform that helps job seekers turn a CV and job preferences into explainable, ranked job recommendations. Job seekers use both the React web application and Flutter mobile application, while recruiters use the React application to manage job postings and review applications.

## Current increment

Part 3 adds Job Seeker and company profiles, normalized skills, recruiter-owned job postings, controlled publishing and closing, and a public searchable job catalog. React, Flutter, CV processing, job ranking, applications, and Agentic AI behavior remain intentionally deferred.

## Planned architecture

```text
React web app -----------\
                         -> ASP.NET Core API -> PostgreSQL
Flutter mobile app -----/           |
                                    -> Controlled Agentic AI workflow
                                    -> Notification provider
```

React and Flutter will use the same API, identity, roles, permissions, and business rules. Neither client will connect directly to PostgreSQL, an AI model, or a third-party provider.

## Solution projects

- `NextRoleAI.Api`: HTTP endpoints and application composition.
- `NextRoleAI.Application`: use cases, contracts, validation, and authorization policies.
- `NextRoleAI.Domain`: entities, value objects, enums, and business rules.
- `NextRoleAI.Infrastructure`: PostgreSQL, identity, file storage, and external providers.
- `NextRoleAI.AgenticAI`: agent orchestration, tools, structured state, and safety controls.
- `NextRoleAI.Api.Tests`: automated backend tests.
- `NextRoleAI.IntegrationTests`: real PostgreSQL API and authorization tests.

## Prerequisites

- .NET 10 SDK
- PostgreSQL 17 or Docker Desktop
- Flutter will be required in a later increment

## Configure local development

Copy `.env.example` to `.env`, choose a local PostgreSQL password, and generate a random JWT signing key with at least 32 characters. Never commit the completed `.env` file.

Start PostgreSQL with Docker:

```powershell
docker compose up -d postgres
```

Configure the API process in the current PowerShell session:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=nextroleai;Username=nextroleai;Password=YOUR_LOCAL_PASSWORD"
$env:Jwt__SigningKey = "YOUR_RANDOM_SIGNING_KEY_WITH_AT_LEAST_32_CHARACTERS"
```

Optional local demo data is disabled by default. To create a demo Recruiter, company, and published job in the Development environment, also set:

```powershell
$env:SeedData__Enabled = "true"
$env:SeedData__RecruiterEmail = "demo.recruiter@example.com"
$env:SeedData__RecruiterPassword = "YOUR_STRONG_LOCAL_ONLY_PASSWORD"
```

Restore local tools and apply migrations:

```powershell
dotnet tool restore
dotnet ef database update --project backend/src/NextRoleAI.Infrastructure --startup-project backend/src/NextRoleAI.Api
```

## Run the API

```powershell
dotnet restore NextRoleAI.sln
dotnet run --project backend/src/NextRoleAI.Api
```

The API exposes:

- `GET /health`: infrastructure-friendly health endpoint.
- `GET /api/health`: JSON service health response.
- `POST /api/auth/register/job-seeker`: create a Job Seeker account.
- `POST /api/auth/register/recruiter`: create a Recruiter account.
- `POST /api/auth/login`: authenticate an existing user.
- `POST /api/auth/refresh`: rotate a refresh token and issue a new token pair.
- `POST /api/auth/logout`: revoke a refresh token.
- `GET /api/auth/me`: return the authenticated user's shared identity and role.
- `GET|PUT /api/profiles/job-seeker`: read or update the current Job Seeker profile.
- `GET|PUT /api/profiles/company`: read or update the current Recruiter's company.
- `GET|POST /api/recruiter/jobs`: list or create the current Recruiter's jobs.
- `GET|PUT|DELETE /api/recruiter/jobs/{id}`: manage a recruiter-owned job.
- `PATCH /api/recruiter/jobs/{id}/status`: publish or close a recruiter-owned job.
- `GET /api/jobs`: search, filter, sort, and paginate open published jobs.
- `GET /api/jobs/{id}`: retrieve an open published job.

See [docs/api/part-3-profiles-and-jobs.md](docs/api/part-3-profiles-and-jobs.md) for request shapes, filters, and status rules.

## Run the tests

```powershell
dotnet test NextRoleAI.sln
```

PostgreSQL integration tests run automatically in GitHub Actions. To run them locally, set `NEXTROLEAI_TEST_CONNECTION_STRING` to a disposable PostgreSQL database before running the test command.

## Authentication security

- Passwords are hashed and verified by ASP.NET Core Identity.
- Access tokens are short-lived JWTs and contain the user's stable ID and role.
- Refresh tokens are random, stored only as SHA-256 hashes, rotated on use, and revoked on logout.
- Reuse of a rotated refresh token revokes the user's remaining active sessions.
- JWT signing keys and database credentials are supplied through environment variables and are never committed.

## Roles in the current scope

- `JobSeeker`: uses React and Flutter to manage a CV, receive recommendations, apply, and track status.
- `Recruiter`: uses React to manage job postings and review applications.

The assignment's normal minimum is three roles. The current two-role scope must be confirmed by the lecturer as part of the approved solo-group adjustment.

## Delivery increments

See [docs/implementation-plan.md](docs/implementation-plan.md) for the part-by-part delivery plan.
