import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../models/job_seeker_profile.dart';
import '../state/profile_view_model.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _headline = TextEditingController();
  final _preferredTitle = TextEditingController();
  final _summary = TextEditingController();
  final _location = TextEditingController();
  final _preferredSalary = TextEditingController();
  final _experience = TextEditingController(text: '0');
  final _skills = TextEditingController();
  bool _didPopulate = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadProfile());
  }

  Future<void> _loadProfile() async {
    final viewModel = context.read<ProfileViewModel>();
    if (viewModel.profile == null) await viewModel.load();
    if (mounted && viewModel.errorMessage == null) {
      _populate(viewModel.profile ?? JobSeekerProfile.empty());
    }
  }

  void _populate(JobSeekerProfile profile) {
    if (_didPopulate) return;
    _headline.text = profile.headline;
    _preferredTitle.text = profile.preferredJobTitle;
    _summary.text = profile.summary;
    _location.text = profile.location;
    _preferredSalary.text = profile.preferredSalary?.toString() ?? '';
    _experience.text = '${profile.yearsOfExperience}';
    _skills.text = profile.skills.join(', ');
    _didPopulate = true;
    setState(() {});
  }

  @override
  void dispose() {
    _headline.dispose();
    _preferredTitle.dispose();
    _summary.dispose();
    _location.dispose();
    _preferredSalary.dispose();
    _experience.dispose();
    _skills.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    FocusScope.of(context).unfocus();
    if (!_formKey.currentState!.validate()) return;
    final skills = _skills.text
        .split(',')
        .map((skill) => skill.trim())
        .where((skill) => skill.isNotEmpty)
        .toSet()
        .toList(growable: false);
    final succeeded = await context.read<ProfileViewModel>().save(
      JobSeekerProfile(
        headline: _headline.text.trim(),
        preferredJobTitle: _preferredTitle.text.trim(),
        summary: _summary.text.trim(),
        location: _location.text.trim(),
        preferredSalary: _preferredSalary.text.trim().isEmpty
            ? null
            : num.parse(_preferredSalary.text.trim()),
        yearsOfExperience: int.parse(_experience.text),
        skills: skills,
      ),
    );
    if (succeeded && mounted) context.go('/recommendations');
  }

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<ProfileViewModel>();
    if (viewModel.isLoading && !_didPopulate) {
      return const Scaffold(body: LoadingPanel(label: 'Loading your profile'));
    }
    if (viewModel.errorMessage != null && !_didPopulate) {
      return Scaffold(
        appBar: AppBar(title: const Text('Career profile')),
        body: MessagePanel(
          icon: Icons.cloud_off_outlined,
          title: 'Profile unavailable',
          message: viewModel.errorMessage!,
          action: FilledButton(
            onPressed: _loadProfile,
            child: const Text('Try again'),
          ),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Career profile')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
          children: [
            Text(
              'Tell us where you want to go.',
              style: Theme.of(context).textTheme.headlineSmall
                  ?.copyWith(fontWeight: FontWeight.w900),
            ),
            const SizedBox(height: 8),
            Text(
              'This profile is shared with the web app and contributes to your explainable job ranking.',
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 20),
            if (viewModel.errorMessage != null) ...[
              FeedbackBanner(message: viewModel.errorMessage!, isError: true),
              const SizedBox(height: 16),
            ],
            if (viewModel.successMessage != null) ...[
              FeedbackBanner(message: viewModel.successMessage!),
              const SizedBox(height: 16),
            ],
            _requiredField(_headline, 'Professional headline'),
            const SizedBox(height: 14),
            _requiredField(_preferredTitle, 'Preferred job title'),
            const SizedBox(height: 14),
            _requiredField(_location, 'Preferred location'),
            const SizedBox(height: 14),
            TextFormField(
              controller: _preferredSalary,
              keyboardType: const TextInputType.numberWithOptions(
                decimal: true,
              ),
              decoration: const InputDecoration(
                labelText: 'Preferred minimum monthly salary (LKR)',
                helperText: 'Optional; used when a job lists comparable pay',
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
            const SizedBox(height: 14),
            TextFormField(
              controller: _experience,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(
                labelText: 'Years of experience',
              ),
              validator: (value) {
                final years = int.tryParse(value ?? '');
                if (years == null || years < 0 || years > 80) {
                  return 'Enter a number from 0 to 80.';
                }
                return null;
              },
            ),
            const SizedBox(height: 14),
            TextFormField(
              controller: _summary,
              minLines: 4,
              maxLines: 7,
              decoration: const InputDecoration(
                labelText: 'Professional summary',
                alignLabelWithHint: true,
              ),
              validator: (value) => (value == null || value.trim().isEmpty)
                  ? 'Add a short professional summary.'
                  : null,
            ),
            const SizedBox(height: 14),
            TextFormField(
              controller: _skills,
              minLines: 2,
              maxLines: 4,
              decoration: const InputDecoration(
                labelText: 'Skills',
                helperText: 'Separate skills with commas',
                alignLabelWithHint: true,
              ),
              validator: (value) => (value == null || value.trim().isEmpty)
                  ? 'Add at least one skill.'
                  : null,
            ),
            const SizedBox(height: 22),
            FilledButton.icon(
              onPressed: viewModel.isSaving ? null : _save,
              icon: viewModel.isSaving
                  ? const SizedBox.square(
                      dimension: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.save_outlined),
              label: Text(viewModel.isSaving ? 'Saving…' : 'Save profile'),
            ),
          ],
        ),
      ),
    );
  }

  TextFormField _requiredField(
    TextEditingController controller,
    String label,
  ) => TextFormField(
    controller: controller,
    textCapitalization: TextCapitalization.sentences,
    decoration: InputDecoration(labelText: label),
    validator: (value) =>
        (value == null || value.trim().isEmpty) ? '$label is required.' : null,
  );
}
