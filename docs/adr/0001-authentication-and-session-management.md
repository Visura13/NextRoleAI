# ADR 0001: Authentication and session management

## Status

Accepted

## Context

NextRoleAI must provide one shared identity and permission model to React and Flutter. Both clients need stateless API authentication, while mobile sessions must remain usable without storing a long-lived access token. The solution must support Job Seeker and Recruiter authorization and must be explainable during the viva.

## Options considered

1. ASP.NET Core Identity with JWT access tokens and rotating refresh tokens.
2. ASP.NET Core Identity API proprietary bearer tokens.
3. A managed external identity provider.

## Decision

Use ASP.NET Core Identity for password hashing, account storage, roles, lockout settings, and user management. Issue a short-lived JWT access token and a random refresh token after successful authentication. Store only a SHA-256 hash of each refresh token, rotate it on use, and revoke active sessions when reuse of an already rotated token is detected.

React and Flutter will use the same endpoints and role claims. Secrets are supplied through environment variables rather than source-controlled settings.

## Consequences

- The API owns identity and authorization consistently across both clients.
- JWT validation is standard and works well with protected REST endpoints.
- Refresh-token rotation requires additional persistence, cleanup, and security tests.
- Client applications must store refresh tokens securely and coordinate token renewal.
- The team remains responsible for protecting the signing key and database credentials.
