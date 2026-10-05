# NextRoleAI

NextRoleAI is an AI-assisted recruitment platform that helps job seekers turn a CV and job preferences into explainable, ranked job recommendations. Job seekers use both the React web application and Flutter mobile application, while recruiters use the React application to manage job postings and review applications.

## Current increment

Part 1 establishes the backend solution boundaries, a runnable ASP.NET Core API, health endpoints, test infrastructure, and the first GitHub Actions workflow. Database access, authentication, frontend clients, and Agentic AI behavior are intentionally deferred to later increments.

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

## Prerequisites

- .NET 10 SDK
- PostgreSQL and Flutter will be required in later increments

## Run the API

```powershell
dotnet restore NextRoleAI.sln
dotnet run --project backend/src/NextRoleAI.Api
```

The API exposes:

- `GET /health`: infrastructure-friendly health endpoint.
- `GET /api/health`: JSON service health response.

## Run the tests

```powershell
dotnet test NextRoleAI.sln
```

## Roles in the current scope

- `JobSeeker`: uses React and Flutter to manage a CV, receive recommendations, apply, and track status.
- `Recruiter`: uses React to manage job postings and review applications.

The assignment's normal minimum is three roles. The current two-role scope must be confirmed by the lecturer as part of the approved solo-group adjustment.

## Delivery increments

See [docs/implementation-plan.md](docs/implementation-plan.md) for the part-by-part delivery plan.
