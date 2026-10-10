# Part 6 CV processing and AI semantic ranking

Part 6 turns an authenticated Job Seeker's PDF or DOCX into a reviewable structured profile and ranks currently published jobs with explainable AI semantic analysis. React and Flutter use the same API and business rules.

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
- `GET /api/recommendations?limit=20` returns ranked published jobs; it returns `409` until the CV profile is confirmed and `503` if AI ranking is unavailable.

The profile update contains `candidateName`, `email`, `phone`, `location`, `currentJobTitle`, `professionalSummary`, `yearsExperience`, one to fifty `skills`, and reviewable tertiary/professional `education` entries.

## Storage and privacy

The original file is never stored in Git or a web-public directory. With no override, the API writes generated storage keys beneath the operating system's local application-data directory (`NextRoleAI/cvs`). Set `CvStorage__RootPath` to an absolute private directory to override it:

```powershell
$env:CvStorage__RootPath = "D:\private-nextroleai-cvs"
```

Configure an OpenAI-compatible chat-completions provider through `CvAi__Enabled`, `CvAi__BaseUrl`, `CvAi__ApiKey`, and `CvAi__Model`. For CV upload, the provider receives redacted text, returns structured JSON, and is constrained by deterministic evidence validation. Only tertiary or professional education such as certificates, diplomas and degrees is retained; school-level O/L and A/L entries are excluded. If CV analysis fails, upload continues with basic extraction and no quality score. Recommendations are different: they require the AI provider and deliberately return `503` instead of substituting an inaccurate keyword score.

Only metadata and extracted information are returned by the API; this increment deliberately has no file-download endpoint. The database stores a SHA-256 checksum for integrity and traceability. Local storage is for development and marking. Production should use encrypted private object storage, retention/deletion controls, malware scanning, and backups.

## Ranking contract

`GET /api/recommendations` returns an `items` collection ordered by descending score. Each item contains the public job, total `score`, component `breakdown`, `matchedSkills`, `missingRequiredSkills`, explanatory `reasons`, and `algorithmVersion`.

The `ai-semantic-ranking-v1:<model>` score is out of 100. The model evaluates all candidate jobs together and recognises semantic and transferable evidence rather than requiring exact keyword equality:

| Component | Points | Rule |
| --- | ---: | --- |
| Skills fit | 40 | Technical, domain, and transferable skill evidence |
| Role fit | 25 | Alignment with the title and described responsibilities |
| Experience fit | 15 | Relevant depth compared with the stated minimum |
| Education fit | 10 | Relevant higher/professional education, without penalising jobs that do not require it |
| Location fit | 10 | Location and work-mode compatibility |

Candidate and job text is treated as untrusted data, protected characteristics are excluded, every job ID and component bound is validated, and evidence lists are restricted to supplied candidate/job values. Jobs are processed in bounded batches of ten; every supplied job must appear exactly once within its batch with at least two reasons. The server sums the bounded components and shows no ranking if validation fails. Successful results are cached for ten minutes and automatically invalidated by CV, profile, job, or model-version changes to reduce provider cost.

## Known limits

- Scanned-image PDFs need OCR and are rejected when no useful text is found.
- Basic fallback extraction uses a bounded skill vocabulary, so AI configuration and the user correction step remain important.
- The current recommendation query ranks the newest 100 published jobs and returns at most the requested limit.
- Ranking quality and availability depend on the configured provider, model, API quota, and sufficiently descriptive job posts.
