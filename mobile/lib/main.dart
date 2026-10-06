import 'dart:async';

import 'package:flutter/widgets.dart';

import 'app.dart';
import 'core/api/api_client.dart';
import 'core/config/app_config.dart';
import 'core/storage/session_storage.dart';
import 'features/agent_workflows/data/agent_workflows_repository.dart';
import 'features/applications/data/applications_repository.dart';
import 'features/auth/data/auth_repository.dart';
import 'features/auth/state/auth_view_model.dart';
import 'features/cv/data/cv_file_picker.dart';
import 'features/cv/data/cv_repository.dart';
import 'features/jobs/data/jobs_repository.dart';
import 'features/profile/data/profile_repository.dart';
import 'features/recommendations/data/recommendations_repository.dart';
import 'routing/app_router.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();

  final sessionStorage = SecureSessionStorage();
  final apiClient = ApiClient(
    baseUrl: AppConfig.apiBaseUrl,
    sessionStorage: sessionStorage,
  );
  final authViewModel = AuthViewModel(
    AuthRepository(apiClient, sessionStorage),
  );
  final jobsRepository = JobsRepository(apiClient);
  final router = createAppRouter(
    auth: authViewModel,
    jobsRepository: jobsRepository,
  );

  runApp(
    NextRoleApp(
      router: router,
      authViewModel: authViewModel,
      jobsRepository: jobsRepository,
      profileRepository: ProfileRepository(apiClient),
      cvFilePicker: DeviceCvFilePicker(),
      cvRepository: CvRepository(apiClient),
      recommendationsRepository: RecommendationsRepository(apiClient),
      agentWorkflowsRepository: AgentWorkflowsRepository(apiClient),
      applicationsRepository: ApplicationsRepository(apiClient),
    ),
  );
  unawaited(authViewModel.initialize());
}
