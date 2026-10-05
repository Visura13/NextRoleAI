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

Part 5 includes Job Seeker registration, login/logout, secure session storage, profile editing, public job search and filtering, job details, and native CV document selection. CV upload/ranking and applications remain explicit placeholders for Parts 6 and 8.
