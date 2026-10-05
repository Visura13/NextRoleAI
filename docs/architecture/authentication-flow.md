# Authentication flow

## Registration

1. React or Flutter submits the registration DTO to ASP.NET Core.
2. The API selects the endpoint-owned role; the client cannot supply an arbitrary role.
3. ASP.NET Core Identity validates the password, normalizes the email, hashes the password, and creates the user.
4. The API assigns either `JobSeeker` or `Recruiter`.
5. A short-lived JWT and random refresh token are generated.
6. Only the refresh-token SHA-256 hash is stored in PostgreSQL.

## Refresh rotation

1. The client submits its current refresh token.
2. The API hashes it and retrieves the matching database row.
3. An expired, unknown, or revoked token is rejected.
4. A valid token is revoked and linked to the replacement token hash.
5. A new access token and refresh token are returned.
6. Reuse of an already rotated token revokes the remaining active tokens for that user.

## Authorization

The JWT contains the stable user ID, email, and one role. Policies protect future Job Seeker and Recruiter endpoints. Authentication decisions remain in ASP.NET Core rather than React or Flutter.
