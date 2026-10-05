# ADR 0004: Flutter state management and navigation

- Status: accepted
- Date: 2026-10-05

## Context

The Job Seeker mobile client needs authentication restoration, protected navigation, API-backed profile and job state, explicit loading/error states, and isolated device integration. The assignment also requires a justified Flutter state-management approach that can be tested and explained during the viva.

## Options considered

1. Widget-local `setState` for all state. This has no extra dependency, but mixes networking, secure storage, validation, and rendering and becomes difficult to test.
2. BLoC/Cubit. This gives strict event/state separation but adds more event and state types than this client currently needs.
3. Riverpod. This provides strong dependency injection and scalable state composition, but introduces a second provider model and more concepts to explain for the present scope.
4. `ChangeNotifier` view models exposed with Provider, plus declarative `go_router` redirects.

## Decision

Use feature-oriented MVVM boundaries. Views and reusable widgets contain rendering and simple interaction logic; `ChangeNotifier` view models own asynchronous state and commands; repositories own API mapping; the API client owns authenticated transport and refresh rotation. Provider supplies dependencies and view models. `go_router` derives protected navigation from authentication state.

## Consequences

- View-model logic can be tested without rendering widgets or invoking platform plugins.
- Views rebuild only from explicit notifications and remain straightforward to explain.
- Navigation guards have one source of truth: the authentication view model.
- Provider and `ChangeNotifier` require disciplined disposal and narrow consumers to avoid unnecessary rebuilds.
- A future move to Riverpod or BLoC would change the UI-state wiring but not the repository or API contracts.
