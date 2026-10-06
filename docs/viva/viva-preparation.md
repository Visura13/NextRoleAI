# Viva preparation

Use these prompts to practise explaining and modifying your own implementation. Do not memorize wording; be ready to open the referenced code.

## Backend and database

- Trace one request from controller DTO through service/interface to EF Core and PostgreSQL.
- Explain why DTOs are separate from entities and where validation occurs.
- Identify one unique constraint, foreign key delete behavior, composite index, and migration.
- Explain why PostgreSQL integration tests are not replaced with an in-memory provider.
- Be ready to add a small validation rule or query filter and its test.

## Authentication and clients

- Distinguish authentication from authorization and show both `401` and `403` paths.
- Explain access-token expiry, hashed refresh tokens, rotation, reuse detection, and logout.
- Compare React session storage with Flutter secure storage and state restoration.
- Explain how both clients use the same contracts while adapting UI/navigation per platform.

## CV, ranking, and agents

- Explain the upload size/type/signature checks and why stored names are generated.
- Recalculate a sample deterministic score and interpret every explanation field.
- Name all four agents, each responsibility, and the exact tools each can use.
- Explain why user text cannot create arbitrary tool calls.
- Show structured input/output, validation rules, safe failure, retry, timeout, persisted history, and the approval gate.
- Explain how prompt injection, duplicate jobs, invalid scores, and unconfirmed profiles are blocked.

## Applications and integration

- Draw valid application status transitions and identify who can trigger each.
- Explain ownership checks and immutable status events.
- Explain why failed Resend delivery does not roll back a valid business transition.
- Identify the third-party integration, secret location, disabled/test behavior, and audit record.

## Quality, Git, and deployment

- Differentiate unit, component/widget, integration, end-to-end, security, agent evaluation, and load tests used here.
- Explain locked dependency restores and the three CI workflows.
- Describe the Render Blueprint, Docker multi-stage build, private database networking, migrations, CORS, and environment variables.
- State why the APK is debug-signed and what production signing would require.
- State the ephemeral CV limitation and the concrete persistent-storage upgrade.
- Show one ADR and one PR where evidence changed your design.

## Five-minute modification drills

1. Change the minimum recommendation score and update its tests.
2. Add one allowed sort option to the job catalogue.
3. Add a validation message to the React or Flutter application form.
4. Add a new deterministic agent rule and expose it in both clients.
5. Diagnose a wrong CORS origin or API base URL using browser/network evidence.
