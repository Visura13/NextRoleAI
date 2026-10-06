# ADR 0007: Application lifecycle and notification delivery

- Status: accepted
- Date: 2026-10-06

## Context

Part 8 requires a Job Seeker to submit and track an application from React or Flutter, a Recruiter to record decisions in React, and both clients to observe the same durable state. Decisions must be auditable. A third-party service must notify the affected user without making a core application transition depend on provider availability.

## Decision

The API owns a strict application state machine and stores one `JobApplication` per Job Seeker and job posting. Every creation or transition appends an immutable `ApplicationStatusEvent` containing the previous status, new status, actor, role, note, and timestamp.

Recruiters may move active applications to `InReview`, `MoreInformationRequested`, `Shortlisted`, or `Rejected`. Job Seekers may answer a request for more information, returning the application to `Submitted`, or withdraw an active application. `Shortlisted`, `Rejected`, and `Withdrawn` are terminal.

Email is integrated through an `IApplicationNotificationSender` boundary and a Resend adapter. Each attempt is represented by a `NotificationDelivery` row before the provider call and is finalized as `Sent`, `Skipped`, or `Failed`. Provider failure is best-effort: the business transition stays committed and the failed attempt remains visible in the audit data. Development and CI leave delivery disabled, which records `Skipped` rather than contacting an external system.

An application may optionally reference an agent workflow. The API accepts that reference only when the workflow belongs to the Job Seeker, was explicitly approved, and contains the selected approved job. The agents never submit an application automatically.

## Consequences

- React and Flutter display one canonical server-side status and history.
- Duplicate submissions are prevented by both service validation and a database unique index.
- Recruiters cannot read or decide applications for another company.
- Notification credentials stay in environment configuration and are never returned to clients.
- A future background outbox worker can retry failed deliveries without changing API contracts or business state.
