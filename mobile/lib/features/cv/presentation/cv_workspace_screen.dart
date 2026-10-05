import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../models/cv_profile.dart';
import '../state/cv_selection_view_model.dart';

class CvWorkspaceScreen extends StatefulWidget {
  const CvWorkspaceScreen({super.key});

  @override
  State<CvWorkspaceScreen> createState() => _CvWorkspaceScreenState();
}

class _CvWorkspaceScreenState extends State<CvWorkspaceScreen> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _location = TextEditingController();
  final _title = TextEditingController();
  final _summary = TextEditingController();
  final _years = TextEditingController(text: '0');
  final _skills = TextEditingController();
  DateTime? _loadedRevision;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback(
      (_) => context.read<CvSelectionViewModel>().load(),
    );
  }

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _phone.dispose();
    _location.dispose();
    _title.dispose();
    _summary.dispose();
    _years.dispose();
    _skills.dispose();
    super.dispose();
  }

  void _populate(CvProfile profile) {
    if (_loadedRevision == profile.updatedAtUtc) return;
    _name.text = profile.candidateName;
    _email.text = profile.email;
    _phone.text = profile.phone;
    _location.text = profile.location;
    _title.text = profile.currentJobTitle;
    _summary.text = profile.professionalSummary;
    _years.text = '${profile.yearsExperience}';
    _skills.text = profile.skills.join(', ');
    _loadedRevision = profile.updatedAtUtc;
  }

  Future<void> _confirm() async {
    FocusScope.of(context).unfocus();
    if (!_formKey.currentState!.validate()) return;
    final skills = _skills.text
        .split(',')
        .map((skill) => skill.trim())
        .where((skill) => skill.isNotEmpty)
        .toSet()
        .toList(growable: false);
    await context.read<CvSelectionViewModel>().confirm(
      CvProfileUpdate(
        candidateName: _name.text.trim(),
        email: _email.text.trim(),
        phone: _phone.text.trim(),
        location: _location.text.trim(),
        currentJobTitle: _title.text.trim(),
        professionalSummary: _summary.text.trim(),
        yearsExperience: int.parse(_years.text),
        skills: skills,
      ),
    );
  }

  Future<void> _delete() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete CV?'),
        content: const Text(
          'This removes the private document and its extracted profile.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (confirmed == true && mounted) {
      await context.read<CvSelectionViewModel>().deleteCv();
      _loadedRevision = null;
    }
  }

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<CvSelectionViewModel>();
    final profile = viewModel.profile;
    if (profile != null) _populate(profile);
    if (viewModel.isLoading) {
      return const Scaffold(body: LoadingPanel(label: 'Loading your CV'));
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Your CV')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
        children: [
          Text(
            'Turn your CV into a profile you control.',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          Text(
            'Upload a PDF or DOCX up to 5 MB, then review every extracted field.',
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 18),
          if (viewModel.errorMessage != null) ...[
            FeedbackBanner(message: viewModel.errorMessage!, isError: true),
            const SizedBox(height: 14),
          ],
          if (viewModel.successMessage != null) ...[
            FeedbackBanner(message: viewModel.successMessage!),
            const SizedBox(height: 14),
          ],
          _UploadCard(viewModel: viewModel, onDelete: _delete),
          if (profile != null) ...[
            const SizedBox(height: 20),
            Form(
              key: _formKey,
              child: Card(
                child: Padding(
                  padding: const EdgeInsets.all(18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  profile.originalFileName,
                                  style: Theme.of(context).textTheme.titleMedium
                                      ?.copyWith(fontWeight: FontWeight.w800),
                                ),
                                Text(
                                  '${(profile.sizeBytes / 1024).toStringAsFixed(1)} KB · ${profile.sha256Checksum.substring(0, 12)}…',
                                ),
                              ],
                            ),
                          ),
                          Chip(
                            label: Text(
                              profile.isConfirmed
                                  ? 'Confirmed'
                                  : 'Needs review',
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      const FeedbackBanner(
                        message: 'Extraction is only a draft. Correct anything that is missing or inaccurate.',
                      ),
                      const SizedBox(height: 16),
                      _required(_name, 'Candidate name'),
                      const SizedBox(height: 12),
                      _required(_title, 'Current job title'),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _email,
                        keyboardType: TextInputType.emailAddress,
                        decoration: const InputDecoration(labelText: 'Email'),
                        validator: (value) {
                          final email = value?.trim() ?? '';
                          if (email.isNotEmpty && !email.contains('@')) {
                            return 'Enter a valid email or leave it empty.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _phone,
                        keyboardType: TextInputType.phone,
                        decoration: const InputDecoration(labelText: 'Phone'),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _location,
                        decoration: const InputDecoration(
                          labelText: 'Location',
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _years,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Years of experience',
                        ),
                        validator: (value) {
                          final years = int.tryParse(value ?? '');
                          return years == null || years < 0 || years > 80
                              ? 'Enter a number from 0 to 80.'
                              : null;
                        },
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _summary,
                        minLines: 4,
                        maxLines: 7,
                        decoration: const InputDecoration(
                          labelText: 'Professional summary',
                          alignLabelWithHint: true,
                        ),
                        validator: (value) => (value?.trim().length ?? 0) < 20
                            ? 'Write at least 20 characters.'
                            : null,
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _skills,
                        minLines: 2,
                        maxLines: 4,
                        decoration: const InputDecoration(
                          labelText: 'Skills',
                          helperText: 'Separate skills with commas',
                          alignLabelWithHint: true,
                        ),
                        validator: (value) => (value?.trim().isEmpty ?? true)
                            ? 'Add at least one skill.'
                            : null,
                      ),
                      const SizedBox(height: 18),
                      FilledButton.icon(
                        onPressed: viewModel.isSaving ? null : _confirm,
                        icon: const Icon(Icons.verified_outlined),
                        label: Text(
                          viewModel.isSaving
                              ? 'Confirming…'
                              : profile.isConfirmed
                              ? 'Save corrections'
                              : 'Confirm CV profile',
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }

  TextFormField _required(TextEditingController controller, String label) =>
      TextFormField(
        controller: controller,
        decoration: InputDecoration(labelText: label),
        validator: (value) =>
            (value?.trim().isEmpty ?? true) ? '$label is required.' : null,
      );
}

class _UploadCard extends StatelessWidget {
  const _UploadCard({required this.viewModel, required this.onDelete});

  final CvSelectionViewModel viewModel;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    final selected = viewModel.selectedCv;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              selected?.name ??
                  (viewModel.profile == null
                      ? 'No CV selected'
                      : 'Choose a replacement document'),
              style: const TextStyle(fontWeight: FontWeight.w800),
            ),
            if (selected != null)
              Text('${(selected.sizeInBytes / 1024).toStringAsFixed(1)} KB'),
            const SizedBox(height: 14),
            OutlinedButton.icon(
              onPressed: viewModel.isSelecting ? null : viewModel.selectCv,
              icon: const Icon(Icons.folder_open_outlined),
              label: Text(
                viewModel.isSelecting ? 'Opening…' : 'Choose PDF or DOCX',
              ),
            ),
            if (selected != null) ...[
              const SizedBox(height: 8),
              FilledButton.icon(
                key: const Key('upload-cv'),
                onPressed: viewModel.isUploading ? null : viewModel.upload,
                icon: const Icon(Icons.upload_outlined),
                label: Text(
                  viewModel.isUploading ? 'Extracting…' : 'Upload and extract',
                ),
              ),
            ],
            if (viewModel.profile != null) ...[
              const SizedBox(height: 8),
              TextButton(
                onPressed: onDelete,
                child: const Text('Delete CV and extracted profile'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
