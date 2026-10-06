# System context

## Actors

- Job Seeker: manages a CV and preferences, receives job recommendations, applies, and tracks status through React or Flutter.
- Recruiter: manages a company and job postings, reviews applications, and records decisions through React.

## Trust boundaries

- React and Flutter are untrusted public clients.
- ASP.NET Core is the only public application and authorization boundary.
- PostgreSQL, agent tools, private file storage, and the Resend notification provider are dependencies accessed only through the API.
- A model response is untrusted input until its schema and business rules pass deterministic validation.

## Current controlled-agent scenario

1. A Job Seeker starts a recommendation workflow from Flutter.
2. ASP.NET Core authenticates the user and persists the request in PostgreSQL.
3. Four controlled agents plan, read the confirmed profile, discover eligible jobs, and validate the proposed shortlist through allow-listed tools.
4. Publication pauses at a human approval gate.
5. The Job Seeker inspects and approves, rejects, or revises the proposal in React or Flutter.
6. ASP.NET Core records the decision and returns the shared status and audit trail to both clients.

## Application workflow

1. A Job Seeker explicitly submits to an open job from React or Flutter, optionally linking an approved agent shortlist.
2. ASP.NET Core prevents duplicates, validates ownership and job availability, and stores the submission plus its first audit event.
3. A Recruiter sees only applications for their company's jobs and records a valid decision in React.
4. Every transition appends immutable history and a Resend delivery attempt. Provider failure does not roll back the decision.
5. React polls for fresh status and Flutter refreshes the same owner-scoped API timeline.

No agent may automatically apply for a job or make the recruiter's final decision.
