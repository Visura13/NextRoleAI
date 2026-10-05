# System context

## Actors

- Job Seeker: manages a CV and preferences, receives job recommendations, applies, and tracks status through React or Flutter.
- Recruiter: manages a company and job postings, reviews applications, and records decisions through React.

## Trust boundaries

- React and Flutter are untrusted public clients.
- ASP.NET Core is the only public application and authorization boundary.
- PostgreSQL, agent tools, file storage, and future notification or model providers are private dependencies accessed through the API.
- A model response is untrusted input until its schema and business rules pass deterministic validation.

## Current controlled-agent scenario

1. A Job Seeker starts a recommendation workflow from Flutter.
2. ASP.NET Core authenticates the user and persists the request in PostgreSQL.
3. Four controlled agents plan, read the confirmed profile, discover eligible jobs, and validate the proposed shortlist through allow-listed tools.
4. Publication pauses at a human approval gate.
5. The Job Seeker inspects and approves, rejects, or revises the proposal in React or Flutter.
6. ASP.NET Core records the decision and returns the shared status and audit trail to both clients.

Part 8 extends this foundation with application submission, Recruiter review, status propagation, and notifications.

No agent may automatically apply for a job or make the recruiter's final decision.
