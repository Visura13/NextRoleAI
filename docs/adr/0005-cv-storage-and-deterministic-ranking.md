# ADR 0005: Private CV storage and deterministic ranking

- Status: partially superseded by ADR 0007 (private storage remains accepted)
- Date: 2026-10-05

## Context

CVs contain personal information and must not be placed in the repository or a publicly served web directory. The first ranking increment also needs repeatable results that can be tested, explained to users, and defended during assessment before an LLM-based agent is introduced.

## Decision

Store original PDF and DOCX files through an `ICvFileStore` abstraction. The development implementation writes generated file names to an application-data directory outside the repository and exposes no download endpoint. PostgreSQL stores the file metadata, checksum, extracted text, user-reviewed structured profile, and skills. Replacing or deleting a CV removes the superseded private file.

Validate the extension, declared content type, maximum size, and file signature before storage. Extract text locally with PdfPig or the Open XML SDK, derive a conservative draft profile, and require the Job Seeker to review and confirm it before ranking.

Rank published jobs with versioned deterministic weights:

- required skill coverage: 45 points;
- preferred skill coverage: 15 points;
- title-token overlap: 15 points;
- experience coverage: 15 points;
- location match, with remote roles accepted: 10 points.

Return the component scores, matched skills, missing required skills, human-readable reasons, and algorithm version with every recommendation.

## Consequences

- Development and marking can run without paid cloud storage or an AI provider.
- The same confirmed profile and scoring rules serve React and Flutter.
- Scores are reproducible and their origin is visible to users and tests.
- Image-only PDFs are rejected because optical character recognition is not included yet.
- The local file store is suitable for one development machine, not horizontally scaled production. A production deployment must replace it with private object storage, encryption and retention policies, and malware scanning while preserving the interface.
- Agentic AI can enrich discovery and planning in Part 7, but it must not silently replace the deterministic score or bypass confirmation.
