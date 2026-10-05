# Part 3 profile and job API

All request and response enums are JSON strings. Protected endpoints require `Authorization: Bearer <access-token>`.

## Profiles

| Method | Route | Role | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/profiles/job-seeker` | JobSeeker | Read the current user's profile. |
| `PUT` | `/api/profiles/job-seeker` | JobSeeker | Create or replace profile details and skills. |
| `GET` | `/api/profiles/company` | Recruiter | Read the current user's company. |
| `PUT` | `/api/profiles/company` | Recruiter | Create or update company details. |

Profile writes are idempotent upserts. Skills are trimmed, deduplicated case-insensitively, and stored as normalized child records.

## Recruiter job management

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/recruiter/jobs?page=1&pageSize=20` | List jobs owned by the current Recruiter. |
| `POST` | `/api/recruiter/jobs` | Create a draft job; a company profile is required. |
| `GET` | `/api/recruiter/jobs/{id}` | Read an owned job. |
| `PUT` | `/api/recruiter/jobs/{id}` | Update an owned draft or published job. |
| `PATCH` | `/api/recruiter/jobs/{id}/status` | Change `Draft` to `Published` or `Published` to `Closed`. |
| `DELETE` | `/api/recruiter/jobs/{id}` | Delete an owned draft. |

Example status request:

```json
{
  "status": "Published"
}
```

## Public catalog

`GET /api/jobs` accepts these optional query parameters:

- `search`: case-insensitive title, description, or company search.
- `location`: partial case-insensitive location match.
- `employmentType`: `FullTime`, `PartTime`, `Contract`, or `Internship`.
- `workMode`: `OnSite`, `Hybrid`, or `Remote`.
- `skill`: case-insensitive exact skill match.
- `sortBy`: `newest`, `title`, or `closingDate`.
- `page`: positive page number.
- `pageSize`: 1 to 100.

The public list and `GET /api/jobs/{id}` expose only published jobs that have not passed their closing date.
