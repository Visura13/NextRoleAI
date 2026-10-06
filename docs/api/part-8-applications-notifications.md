# Part 8 application and notification contracts

All endpoints require a valid bearer token. Job Seeker endpoints require the `JobSeeker` role; Recruiter endpoints require the `Recruiter` role.

## Job Seeker endpoints

### `POST /api/applications`

Submits one application to an open published job.

```json
{
  "jobPostingId": "00000000-0000-0000-0000-000000000000",
  "coverNote": "A 20 to 2,000 character evidence-based note.",
  "sourceWorkflowRunId": null
}
```

`sourceWorkflowRunId` is optional. When supplied, it must identify the caller's approved workflow and the job must be an approved shortlist item. Duplicate Job Seeker/job pairs and unavailable jobs return `409 Conflict`.

### `GET /api/applications`

Returns the current Job Seeker's applications. Optional query parameters are `status`, `page`, and `pageSize`.

### `GET /api/applications/{id}`

Returns one owner-scoped application with status history and notification-delivery audit entries.

### `POST /api/applications/{id}/respond`

Supplies additional information after `MoreInformationRequested` and returns the application to `Submitted`.

### `POST /api/applications/{id}/withdraw`

Withdraws an active application. Both commands accept:

```json
{ "note": "A short explanation." }
```

## Recruiter endpoints

### `GET /api/recruiter/applications`

Returns applications only for jobs owned by the authenticated Recruiter's company. It supports `status`, `page`, and `pageSize`.

### `GET /api/recruiter/applications/{id}`

Returns one company-owned application.

### `PATCH /api/recruiter/applications/{id}/status`

Records a valid recruiter decision and creates a notification attempt for the Job Seeker.

```json
{
  "status": "InReview",
  "note": "The application passed the initial review."
}
```

Allowed target statuses are `InReview`, `MoreInformationRequested`, `Shortlisted`, and `Rejected`, subject to the server-side transition rules.

## Shared response shape

Application responses contain job and candidate summaries, the cover note, current status, optional source workflow, timestamps, ordered `statusHistory`, and ordered `notificationDeliveries`. Provider credentials and provider error bodies are never exposed.

## Resend configuration

Resend delivery is disabled by default. To demonstrate real email delivery, set:

```powershell
$env:Notifications__Enabled = "true"
$env:Notifications__ResendApiKey = "YOUR_RESEND_API_KEY"
$env:Notifications__FromAddress = "NextRoleAI <notifications@YOUR_VERIFIED_DOMAIN>"
```

When configuration is absent, the attempt is stored as `Skipped`. A provider error is stored as `Failed`; neither outcome rolls back the application submission or decision.
