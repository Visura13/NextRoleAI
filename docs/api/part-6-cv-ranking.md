# Part 6 CV processing and deterministic ranking

Part 6 turns an authenticated Job Seeker's PDF or DOCX into a reviewable structured profile and ranks currently published jobs with an explainable, deterministic algorithm. React and Flutter use the same API and business rules.

## CV workflow

1. Upload one PDF or DOCX of at most 5 MB as the `file` field in `multipart/form-data`.
2. The API validates metadata and file signatures, extracts readable text, creates a conservative draft profile, and returns status `NeedsReview`.
3. The Job Seeker corrects the draft and confirms it. Ranking is unavailable until status becomes `Confirmed`.
4. Uploading again replaces the private file and resets the profile to `NeedsReview`. Deleting removes both the database record and private file.

Supported endpoints, all restricted to the `JobSeeker` role:

- `GET /api/cv` returns the current CV profile or `404`.
- `POST /api/cv` uploads or replaces the current CV.
- `PUT /api/cv/profile` corrects and confirms the extracted profile.
- `DELETE /api/cv` deletes the current CV or returns `404`.
- `GET /api/recommendations?limit=20` returns ranked published jobs; it returns `409` until the CV profile is confirmed.

The profile update contains `candidateName`, `email`, `phone`, `location`, `currentJobTitle`, `professionalSummary`, `yearsExperience`, and one to fifty `skills`.

## Storage and privacy

The original file is never stored in Git or a web-public directory. With no override, the API writes generated storage keys beneath the operating system's local application-data directory (`NextRoleAI/cvs`). Set `CvStorage__RootPath` to an absolute private directory to override it:

```powershell
$env:CvStorage__RootPath = "D:\private-nextroleai-cvs"
```

Only metadata and extracted information are returned by the API; this increment deliberately has no file-download endpoint. The database stores a SHA-256 checksum for integrity and traceability. Local storage is for development and marking. Production should use encrypted private object storage, retention/deletion controls, malware scanning, and backups.

## Ranking contract

`GET /api/recommendations` returns an `items` collection ordered by descending score. Each item contains the public job, total `score`, component `breakdown`, `matchedSkills`, `missingRequiredSkills`, explanatory `reasons`, and `algorithmVersion`.

The `deterministic-v1` score is out of 100:

| Component | Points | Rule |
| --- | ---: | --- |
| Required skills | 45 | Proportion of required job skills present in the confirmed CV/profile skill union |
| Preferred skills | 15 | Proportion of optional job skills present |
| Title | 15 | Token overlap with the current or preferred job title |
| Experience | 15 | Candidate years divided by the required years, capped at full credit |
| Location | 10 | Preferred/CV location matches, or the job is remote |

No LLM is used for this score. Part 7 may add controlled agents around the workflow while keeping deterministic validation and visible evidence.

## Known limits

- Scanned-image PDFs need OCR and are rejected when no useful text is found.
- The draft parser uses a bounded skill vocabulary, so the user correction step is mandatory.
- The current recommendation query ranks the newest 100 published jobs and returns at most the requested limit.
