# ADR 0002: PostgreSQL persistence

## Status

Accepted

## Context

The assignment mandates PostgreSQL and Entity Framework Core. NextRoleAI needs relational constraints for users, roles, profiles, jobs, applications, recommendations, and durable agent-workflow state.

## Options considered

1. PostgreSQL with the Npgsql Entity Framework Core provider.
2. PostgreSQL with handwritten SQL and a lightweight data mapper.
3. A non-relational document database.

## Decision

Use PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL`. Maintain the schema with reviewed EF Core migrations and commit dependency lock files. Use ASP.NET Core Identity tables for authentication and an indexed `RefreshTokens` table for session rotation.

PostgreSQL integration tests run against a real PostgreSQL service in GitHub Actions. Local development can use the same database through Docker Compose.

## Consequences

- The implementation directly satisfies the mandatory database and EF Core requirements.
- Migrations provide reproducible schema history and deployment evidence.
- PostgreSQL-specific integration tests catch provider and constraint issues that in-memory tests cannot.
- Developers need PostgreSQL or Docker for full local integration testing.
