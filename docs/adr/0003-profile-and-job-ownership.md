# ADR 0003: Profile and job ownership

## Status

Accepted

## Context

NextRoleAI must expose the same business rules to future React and Flutter clients. Job Seeker data belongs to one identity, each Recruiter manages one company profile, and job postings must not be editable by another Recruiter. Public catalog results must never leak drafts, closed jobs, or expired postings.

## Decision

- Store Job Seeker and company profiles in normalized PostgreSQL tables with unique identity foreign keys.
- Store profile and job skills in child tables instead of comma-separated text or client-only state.
- Make each job belong to a company and derive recruiter ownership through that relationship.
- Enforce recruiter ownership inside the application service query as well as with role authorization at the API boundary.
- Use the explicit lifecycle `Draft -> Published -> Closed`. Only drafts can be deleted, and closed jobs cannot be edited.
- Expose only published jobs whose closing date is absent or in the future through the public catalog.
- Keep development seed data opt-in and require credentials through configuration rather than source control.

## Consequences

- React and Flutter can share one API and cannot bypass lifecycle or ownership rules.
- Database constraints prevent duplicate profiles and duplicate skills within one profile or job.
- Public queries can use indexes on job status, publication time, company, update time, and skill name.
- Reopening a closed job requires a future product decision rather than an undocumented status change.
