import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import 'core/theme/app_theme.dart';
import 'features/agent_workflows/data/agent_workflows_repository.dart';
import 'features/agent_workflows/state/agent_workflows_view_model.dart';
import 'features/applications/data/applications_repository.dart';
import 'features/applications/state/applications_view_model.dart';
import 'features/auth/state/auth_view_model.dart';
import 'features/cv/data/cv_file_picker.dart';
import 'features/cv/data/cv_repository.dart';
import 'features/cv/state/cv_selection_view_model.dart';
import 'features/jobs/data/jobs_repository.dart';
import 'features/jobs/state/jobs_view_model.dart';
import 'features/profile/data/profile_repository.dart';
import 'features/profile/state/profile_view_model.dart';
import 'features/recommendations/data/recommendations_repository.dart';
import 'features/recommendations/state/recommendations_view_model.dart';

class NextRoleApp extends StatelessWidget {
  const NextRoleApp({
    required this.router,
    required this.authViewModel,
    required this.jobsRepository,
    required this.profileRepository,
    required this.cvFilePicker,
    required this.cvRepository,
    required this.recommendationsRepository,
    required this.agentWorkflowsRepository,
    required this.applicationsRepository,
    super.key,
  });

  final GoRouter router;
  final AuthViewModel authViewModel;
  final JobsDataSource jobsRepository;
  final ProfileDataSource profileRepository;
  final CvFilePicker cvFilePicker;
  final CvDataSource cvRepository;
  final RecommendationsDataSource recommendationsRepository;
  final AgentWorkflowsDataSource agentWorkflowsRepository;
  final ApplicationsDataSource applicationsRepository;

  @override
  Widget build(BuildContext context) => MultiProvider(
    providers: [
      ChangeNotifierProvider.value(value: authViewModel),
      ChangeNotifierProvider(create: (_) => JobsViewModel(jobsRepository)),
      ChangeNotifierProvider(
        create: (_) => ProfileViewModel(profileRepository),
      ),
      ChangeNotifierProvider(
        create: (_) => CvSelectionViewModel(cvFilePicker, cvRepository),
      ),
      ChangeNotifierProvider(
        create: (_) => RecommendationsViewModel(recommendationsRepository),
      ),
      ChangeNotifierProvider(
        create: (_) => AgentWorkflowsViewModel(agentWorkflowsRepository),
      ),
      ChangeNotifierProvider(
        create: (_) => ApplicationsViewModel(applicationsRepository),
      ),
    ],
    child: MaterialApp.router(
      title: 'NextRoleAI',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      routerConfig: router,
    ),
  );
}
