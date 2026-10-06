import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../features/auth/presentation/login_screen.dart';
import '../features/agent_workflows/presentation/agent_workflows_screen.dart';
import '../features/applications/presentation/applications_screen.dart';
import '../features/auth/presentation/register_screen.dart';
import '../features/auth/presentation/splash_screen.dart';
import '../features/auth/state/auth_view_model.dart';
import '../features/cv/presentation/cv_workspace_screen.dart';
import '../features/home/presentation/home_screen.dart';
import '../features/jobs/data/jobs_repository.dart';
import '../features/jobs/presentation/job_details_screen.dart';
import '../features/jobs/presentation/jobs_screen.dart';
import '../features/jobs/state/jobs_view_model.dart';
import '../features/profile/presentation/profile_screen.dart';
import '../features/recommendations/presentation/recommendations_screen.dart';
import '../shell/app_shell.dart';

GoRouter createAppRouter({
  required AuthViewModel auth,
  required JobsDataSource jobsRepository,
}) => GoRouter(
  initialLocation: '/splash',
  refreshListenable: auth,
  redirect: (context, state) {
    final location = state.matchedLocation;
    final isAuthRoute = location == '/login' || location == '/register';
    switch (auth.status) {
      case AuthStatus.initializing:
        return location == '/splash' ? null : '/splash';
      case AuthStatus.signedOut:
        return isAuthRoute ? null : '/login';
      case AuthStatus.signedIn:
        return isAuthRoute || location == '/splash' ? '/home' : null;
    }
  },
  routes: [
    GoRoute(path: '/splash', builder: (context, state) => const SplashScreen()),
    GoRoute(path: '/login', builder: (context, state) => const LoginScreen()),
    GoRoute(
      path: '/register',
      builder: (context, state) => const RegisterScreen(),
    ),
    ShellRoute(
      builder: (context, state, child) =>
          AppShell(location: state.uri.path, child: child),
      routes: [
        GoRoute(path: '/home', builder: (context, state) => const HomeScreen()),
        GoRoute(path: '/jobs', builder: (context, state) => const JobsScreen()),
        GoRoute(
          path: '/profile',
          builder: (context, state) => const ProfileScreen(),
        ),
      ],
    ),
    GoRoute(
      path: '/jobs/:id',
      builder: (context, state) => ChangeNotifierProvider(
        create: (_) =>
            JobDetailsViewModel(jobsRepository)
              ..load(state.pathParameters['id']!),
        child: const JobDetailsScreen(),
      ),
    ),
    GoRoute(
      path: '/cv',
      builder: (context, state) => const CvWorkspaceScreen(),
    ),
    GoRoute(
      path: '/recommendations',
      builder: (context, state) => const RecommendationsScreen(),
    ),
    GoRoute(
      path: '/agent-workflows',
      builder: (context, state) => const AgentWorkflowsScreen(),
    ),
    GoRoute(
      path: '/applications',
      builder: (context, state) => ApplicationsScreen(
        initialJobId: state.uri.queryParameters['jobId'],
        sourceWorkflowRunId: state.uri.queryParameters['workflowId'],
      ),
    ),
  ],
);
