# System context

## Actors

- Job Seeker: manages a CV and preferences, receives job recommendations, applies, and tracks status through React or Flutter.
- Recruiter: manages a company and job postings, reviews applications, and records decisions through React.

## Trust boundaries

- React and Flutter are untrusted public clients.
- ASP.NET Core is the only public application and authorization boundary.
- PostgreSQL, AI providers, file storage, and notification providers are private dependencies accessed through the API.
- A model response is untrusted input until its schema and business rules pass deterministic validation.

## Core cross-platform scenario

1. A Job Seeker starts a recommendation workflow from Flutter.
2. ASP.NET Core authenticates the user and persists the request in PostgreSQL.
3. Controlled agents create a plan, extract a structured profile, retrieve eligible jobs, and explain deterministic scores.
4. The Job Seeker confirms an application.
5. A Recruiter reviews the application in React and shortlists, rejects, or requests more information.
6. ASP.NET Core records the decision and returns the updated status to React and Flutter.

No agent may automatically apply for a job or make the recruiter's final decision.
