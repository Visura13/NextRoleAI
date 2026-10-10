import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../../cv/presentation/cv_workspace_screen.dart';
import '../../cv/state/cv_selection_view_model.dart';
import '../models/job_seeker_profile.dart';
import '../state/profile_view_model.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _preferredTitle = TextEditingController();
  final _preferredLocation = TextEditingController();
  final _preferredSalary = TextEditingController();
  bool _didPopulate = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadProfile());
  }

  Future<void> _loadProfile() async {
    final profileViewModel = context.read<ProfileViewModel>();
    final cvViewModel = context.read<CvSelectionViewModel>();
    await Future.wait([
      if (profileViewModel.profile == null) profileViewModel.load(),
      if (!cvViewModel.hasLoaded) cvViewModel.load(),
    ]);
    if (mounted && profileViewModel.errorMessage == null) {
      _populate(
        profileViewModel.profile ?? JobSeekerProfile.empty(),
        cvViewModel,
      );
    }
  }

  void _populate(
    JobSeekerProfile preferences,
    CvSelectionViewModel cvViewModel,
  ) {
    if (_didPopulate) return;
    _preferredTitle.text = preferences.preferredJobTitle;
    _preferredLocation.text = preferences.location.isNotEmpty
        ? preferences.location
        : cvViewModel.profile?.location ?? '';
    _preferredSalary.text = preferences.preferredSalary?.toString() ?? '';
    _didPopulate = true;
    setState(() {});
  }

  @override
  void dispose() {
    _preferredTitle.dispose();
    _preferredLocation.dispose();
    _preferredSalary.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    FocusScope.of(context).unfocus();
    if (!_formKey.currentState!.validate()) return;
    final cv = context.read<CvSelectionViewModel>().profile;
    if (cv == null || !cv.isConfirmed) return;

    final succeeded = await context.read<ProfileViewModel>().save(
      JobSeekerProfile(
        headline: cv.currentJobTitle,
        preferredJobTitle: _preferredTitle.text.trim(),
        summary: cv.professionalSummary,
        location: _preferredLocation.text.trim(),
        preferredSalary: _preferredSalary.text.trim().isEmpty
            ? null
            : num.parse(_preferredSalary.text.trim()),
        yearsOfExperience: cv.yearsExperience,
        skills: cv.skills,
      ),
    );
    if (succeeded && mounted) context.go('/recommendations');
  }

  @override
  Widget build(BuildContext context) {
    final profileViewModel = context.watch<ProfileViewModel>();
    final cvViewModel = context.watch<CvSelectionViewModel>();

    return Scaffold(
      appBar: AppBar(title: const Text('My profile')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
        children: [
          Text(
            'Manage your complete career profile.',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          Text(
            'Your CV-derived professional evidence and job preferences now live in one place and power your recommendations.',
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 24),
          const CvWorkspaceSection(),
          const SizedBox(height: 30),
          Text(
            'Job preferences',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          Text(
            'Tell NextRoleAI what you want next. These preferences complement your confirmed professional profile when jobs are ranked.',
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 18),
          if (profileViewModel.isLoading && !_didPopulate)
            const LoadingPanel(label: 'Loading your job preferences')
          else if (profileViewModel.errorMessage != null && !_didPopulate)
            MessagePanel(
              icon: Icons.cloud_off_outlined,
              title: 'Preferences unavailable',
              message: profileViewModel.errorMessage!,
              action: FilledButton(
                onPressed: _loadProfile,
                child: const Text('Try again'),
              ),
            )
          else
            Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  if (profileViewModel.errorMessage != null) ...[
                    FeedbackBanner(
                      message: profileViewModel.errorMessage!,
                      isError: true,
                    ),
                    const SizedBox(height: 16),
                  ],
                  if (profileViewModel.successMessage != null) ...[
                    FeedbackBanner(message: profileViewModel.successMessage!),
                    const SizedBox(height: 16),
                  ],
                  TextFormField(
                    controller: _preferredTitle,
                    textCapitalization: TextCapitalization.sentences,
                    decoration: const InputDecoration(
                      labelText: 'Preferred job title',
                    ),
                    validator: (value) =>
                        (value == null || value.trim().isEmpty)
                        ? 'Preferred job title is required.'
                        : null,
                  ),
                  const SizedBox(height: 14),
                  TextFormField(
                    controller: _preferredLocation,
                    textCapitalization: TextCapitalization.words,
                    decoration: const InputDecoration(
                      labelText: 'Preferred work location',
                    ),
                    validator: (value) =>
                        (value == null || value.trim().isEmpty)
                        ? 'Preferred work location is required.'
                        : null,
                  ),
                  const SizedBox(height: 14),
                  TextFormField(
                    controller: _preferredSalary,
                    keyboardType: const TextInputType.numberWithOptions(
                      decimal: true,
                    ),
                    decoration: const InputDecoration(
                      labelText: 'Preferred minimum monthly salary (LKR)',
                      helperText:
                          'Optional; used when a job lists comparable pay',
                    ),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) return null;
                      final salary = num.tryParse(value.trim());
                      if (salary == null || salary < 1 || salary > 1000000000) {
                        return 'Enter an amount from LKR 1 to LKR 1,000,000,000.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 10),
                  if (cvViewModel.profile == null ||
                      !cvViewModel.profile!.isConfirmed)
                    const FeedbackBanner(
                      message: 'Upload and confirm your professional profile above before saving preferences.',
                    ),
                  const SizedBox(height: 18),
                  FilledButton.icon(
                    onPressed:
                        profileViewModel.isSaving ||
                            cvViewModel.profile == null ||
                            !cvViewModel.profile!.isConfirmed
                        ? null
                        : _save,
                    icon: profileViewModel.isSaving
                        ? const SizedBox.square(
                            dimension: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.save_outlined),
                    label: Text(
                      profileViewModel.isSaving
                          ? 'Saving…'
                          : 'Save preferences and view recommendations',
                    ),
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}
