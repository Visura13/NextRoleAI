# System architecture

NextRoleAI uses one HTTP API as the trust boundary for both clients. Browser and mobile code never connect directly to PostgreSQL, CV storage, the AI/notification providers, or agent tools.

```mermaid
flowchart LR
    JS[Job Seeker] --> WEB[React + Vite web]
    JS --> MOBILE[Flutter mobile]
    REC[Recruiter] --> WEB

    WEB -->|HTTPS + JWT| API[ASP.NET Core Web API]
    MOBILE -->|HTTPS + JWT| API

    API --> AUTH[ASP.NET Core Identity]
    API --> APP[Application services]
    API --> AGENT[Controlled agent orchestrator]
    AUTH --> DB[(PostgreSQL 17)]
    APP --> DB
    AGENT --> DB
    APP --> FILES[(Private CV file store)]
    APP -->|server-side API key| AI[OpenAI-compatible model API]
    APP -->|server-side API key| RESEND[Resend email API]

    AGENT --> PLAN[Planning Agent]
    AGENT --> PROFILE[Candidate Profile Agent]
    AGENT --> DISCOVERY[Job Discovery Agent]
    AGENT --> VALIDATE[Validation & Safety Agent]
    VALIDATE --> APPROVAL{Human approval}
    APPROVAL -->|approve only| PUBLISH[Publish shortlist]
```

## Backend boundaries

- `NextRoleAI.Api` owns HTTP concerns, Swagger, authentication middleware, CORS, and composition.
- `NextRoleAI.Application` owns use-case contracts and service interfaces.
- `NextRoleAI.Domain` owns entities, statuses, and business rules.
- `NextRoleAI.Infrastructure` owns EF Core/PostgreSQL, Identity, private file storage, AI-provider integration, and Resend.
- `NextRoleAI.AgenticAI` owns bounded orchestration, allow-listed tools, validation, retries, timeouts, and approval gates.

## Deployment topology

The supplied Render Blueprint creates a private PostgreSQL database, a Docker-hosted API, and a static React site in the Singapore region. Flutter uses the same public API URL. Secrets are dashboard environment variables; no credential is committed. The free deployment stores CVs in an ephemeral private directory, so a redeploy can remove uploaded files. A persistent Render disk or an object-storage implementation is the production upgrade path.
