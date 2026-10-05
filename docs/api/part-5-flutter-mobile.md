# Part 5 Flutter mobile client

The Flutter application in `mobile/` is the Job Seeker's mobile channel. It uses the same ASP.NET Core identity, permissions, profile, and published-job endpoints as the React client.

## Implemented journeys

- Job Seeker registration, login, session restoration, refresh rotation, and logout.
- Secure token persistence through the platform keystore/keychain abstraction.
- Protected declarative routes and a responsive bottom-navigation shell.
- Job search and filters, pagination, job details, and explicit API error/empty/loading states.
- Job Seeker profile creation and editing.
- Native CV document selection with extension and size validation.
- Reserved recommendation and application-status states that name their backend dependencies instead of presenting fake data.

## Architecture

The client follows feature-oriented MVVM boundaries:

- widgets and screens render state and forward user intent;
- `ChangeNotifier` view models manage UI state and commands;
- repositories map shared API contracts;
- one API client manages JSON, bearer authorization, and one serialized refresh retry;
- a token-storage interface keeps platform storage replaceable in unit tests.

Provider supplies dependencies and view models. `go_router` listens to authentication state and redirects unauthenticated users away from protected screens. See ADR 0004 for the decision and alternatives.

## Local API address

Android emulators use `10.0.2.2` to reach the host computer, so the default API address is:

```text
http://10.0.2.2:5251
```

Override it for a physical device or deployment:

```powershell
flutter run --dart-define=NEXTROLEAI_API_BASE_URL=http://YOUR_HOST:5251
```

The API must listen on an address reachable from the device. Never embed production credentials or signing secrets in `--dart-define` values.

## Deferred backend-dependent behavior

Part 5 proves the native file picker and retains only the selected file's display metadata in memory. File bytes are not uploaded or persisted because the validated CV storage and processing endpoint belongs to Part 6. Ranked recommendations begin in Part 6, and application submission/history begin in Part 8.
