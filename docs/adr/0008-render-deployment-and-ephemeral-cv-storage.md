# ADR 0008: Render deployment and ephemeral CV storage

- Status: accepted for assignment deployment
- Date: 2026-10-06

## Context

The marking environment needs public React, API health, Swagger, PostgreSQL, and a repeatable setup that a solo student can operate. The repository is a monorepo and the API targets .NET 10.

## Decision

Use a root `render.yaml` Blueprint. Deploy the API from `backend/Dockerfile`, PostgreSQL 17 as a private-network database, and the Vite build as a static site. The API converts Render's PostgreSQL URL into the keyword connection-string format expected by Npgsql. Migrations run on API startup. Secrets and final URLs are supplied in the Render dashboard.

The no-cost configuration stores CV files under `/tmp/nextroleai-cvs`. Files are private but ephemeral. PostgreSQL metadata remains durable. For a production deployment, attach a persistent disk at a non-public path or implement the existing CV storage abstraction with encrypted object storage.

## Consequences

- A single declarative file documents the deployed topology.
- API and database communicate over Render's private network; the database public allow-list is empty.
- The first free-tier request may be slow after inactivity.
- Uploaded CVs may need to be re-uploaded after a service restart or redeploy.
- A paid persistent disk or object store is required before treating the service as production-grade CV storage.
