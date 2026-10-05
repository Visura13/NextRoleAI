# NextRoleAI mobile

The Flutter client is the Job Seeker mobile experience for NextRoleAI. It uses the same ASP.NET Core API and identity as the React web application.

## Run locally

Start the API first, then run from this directory:

```powershell
flutter pub get
flutter run
```

The default API URL is `http://10.0.2.2:5251`, which maps an Android emulator to the development machine. Override it for another device or API address:

```powershell
flutter run --dart-define=NEXTROLEAI_API_BASE_URL=http://YOUR_HOST:5251
```

Physical Android devices must be able to reach that host on the local network. Production builds should use an HTTPS API and disable clear-text traffic.

## Verify

```powershell
dart format --output=none --set-exit-if-changed lib test
flutter analyze
flutter test
```

The Job Seeker can register, authenticate, manage a profile, browse jobs, upload and review a PDF/DOCX CV, inspect deterministic recommendations with score evidence, and start, inspect, approve, reject, or revise the shared controlled AI workflow. Application submission and tracking remain explicit placeholders for Part 8.
