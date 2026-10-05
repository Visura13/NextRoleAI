# Part 4 React web client

The React client is a Vite TypeScript single-page application in `web/`. It consumes the existing ASP.NET Core API and never connects directly to PostgreSQL.

## Implemented journeys

- Public landing page, searchable job catalog, filters, pagination, and job details.
- Job Seeker and Recruiter registration through their role-specific API endpoints.
- Shared login with role-aware dashboard routing and protected client routes.
- Session restoration and one automatic refresh-token rotation after an authenticated `401`.
- Job Seeker profile create/update flow.
- Recruiter company profile and owned job create, edit, publish, close, and draft-delete flows.
- Responsive layouts and explicit loading, empty, validation, success, and error states.

Application submission and recruiter application review are represented by honest reserved states. Their data model, authorization rules, audit history, and notifications belong to Part 8 and are not mocked in the client.

## Local configuration

Copy `web/.env.example` to `web/.env.local` if the API is not running at `http://localhost:5251`.

```text
VITE_API_BASE_URL=http://localhost:5251
```

The API CORS allow-list is empty by default and includes `http://localhost:5173` in Development. Configure production origins through `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, and so on.

## Session storage decision

The current API returns both tokens in JSON so the React client stores the minimum session payload in versioned `sessionStorage`. This avoids persistence after the browser session ends, but JavaScript-readable tokens still require strong XSS controls. Before production, the web channel should move refresh-token transport to a Secure, HttpOnly, SameSite cookie while the Flutter client continues using platform secure storage.
