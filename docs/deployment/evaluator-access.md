# Evaluator access checklist

Complete this file after deployment and before submission.

| Item | Value |
|---|---|
| Commit SHA | `TODO` |
| React URL | `TODO` |
| API health URL | `TODO` |
| Swagger URL | `TODO` |
| APK location/workflow run | `TODO` |
| Job Seeker test email | `TODO - provide privately if required` |
| Recruiter test email | `TODO - provide privately if required` |
| Test password delivery method | `TODO - never commit the password` |
| Demo video URL | `TODO` |

## Incognito verification

- Register/login on React as Job Seeker and Recruiter.
- Confirm protected routes reject the wrong role.
- Recruiter creates and publishes a job.
- Job Seeker edits a profile, uploads a test CV, reviews/corrects extraction, and confirms it.
- Ranked job includes score breakdown and evidence.
- Controlled workflow shows four roles, allowed tools, validation, and pending approval.
- Approval publishes the shortlist; an injection objective fails safely.
- Job Seeker submits from React or Flutter.
- Recruiter changes the application state in React.
- Both React and Flutter show the updated status/history.
- Swagger can exercise public and bearer-protected endpoints.
- `/health` returns success and all three GitHub Actions workflows are green.
