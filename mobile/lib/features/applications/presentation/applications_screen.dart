import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/states.dart';
import '../models/job_application.dart';
import '../state/applications_view_model.dart';

class ApplicationsScreen extends StatefulWidget {
  const ApplicationsScreen({
    this.initialJobId,
    this.sourceWorkflowRunId,
    super.key,
  });

  final String? initialJobId;
  final String? sourceWorkflowRunId;

  @override
  State<ApplicationsScreen> createState() => _ApplicationsScreenState();
}

class _ApplicationsScreenState extends State<ApplicationsScreen> {
  final _coverNoteController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback(
      (_) => context.read<ApplicationsViewModel>().load(),
    );
  }

  @override
  void dispose() {
    _coverNoteController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<ApplicationsViewModel>();
    return Scaffold(
      appBar: AppBar(title: const Text('Applications')),
      body: RefreshIndicator(
        onRefresh: viewModel.load,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
          children: [
            Text(
              'Track every opportunity.',
              style: Theme.of(context).textTheme.headlineSmall
                  ?.copyWith(fontWeight: FontWeight.w900),
            ),
            const SizedBox(height: 8),
            Text(
              'Submit on mobile or web and see the same recruiter decisions and audit history.',
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
            ),
            if (widget.initialJobId != null) ...[
              const SizedBox(height: 20),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'NEW APPLICATION',
                        style: Theme.of(context).textTheme.labelSmall?.copyWith(
                          color: AppTheme.orange,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.2,
                        ),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Add a short, evidence-based cover note.',
                        style: TextStyle(fontWeight: FontWeight.w800),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: _coverNoteController,
                        minLines: 4,
                        maxLines: 7,
                        maxLength: 2000,
                        decoration: const InputDecoration(
                          labelText: 'Cover note',
                          hintText:
                              'Explain why your experience fits this role.',
                        ),
                      ),
                      if (widget.sourceWorkflowRunId != null)
                        const Text(
                          'This role came from an approved AI shortlist. Submission still requires your explicit confirmation.',
                        ),
                      const SizedBox(height: 12),
                      FilledButton.icon(
                        onPressed: viewModel.isSaving ? null : _submit,
                        icon: const Icon(Icons.send_outlined),
                        label: Text(
                          viewModel.isSaving
                              ? 'Submitting…'
                              : 'Submit application',
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
            if (viewModel.successMessage != null) ...[
              const SizedBox(height: 16),
              FeedbackBanner(message: viewModel.successMessage!),
            ],
            if (viewModel.errorMessage != null) ...[
              const SizedBox(height: 16),
              FeedbackBanner(message: viewModel.errorMessage!, isError: true),
            ],
            const SizedBox(height: 24),
            if (viewModel.isLoading && viewModel.items.isEmpty)
              const LoadingPanel(label: 'Loading applications')
            else if (viewModel.items.isEmpty)
              const MessagePanel(
                icon: Icons.track_changes_outlined,
                title: 'No applications yet',
                message: 'Open a published role and choose Apply to start your application timeline.',
              )
            else
              ...viewModel.items.map(
                (application) => _ApplicationCard(application: application),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _submit() async {
    final note = _coverNoteController.text.trim();
    if (note.length < 20) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cover note must contain at least 20 characters.'),
        ),
      );
      return;
    }
    final saved = await context.read<ApplicationsViewModel>().submit(
      jobPostingId: widget.initialJobId!,
      coverNote: note,
      sourceWorkflowRunId: widget.sourceWorkflowRunId,
    );
    if (saved) _coverNoteController.clear();
  }
}

class _ApplicationCard extends StatelessWidget {
  const _ApplicationCard({required this.application});

  final JobApplication application;

  @override
  Widget build(BuildContext context) {
    final active = const {
      'Submitted',
      'InReview',
      'MoreInformationRequested',
    }.contains(application.status);
    return Card(
      margin: const EdgeInsets.only(bottom: 16),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        application.companyName.toUpperCase(),
                        style: Theme.of(context).textTheme.labelSmall?.copyWith(
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.1,
                        ),
                      ),
                      const SizedBox(height: 5),
                      Text(
                        application.jobTitle,
                        style: Theme.of(context).textTheme.titleLarge
                            ?.copyWith(fontWeight: FontWeight.w900),
                      ),
                      Text(application.jobLocation),
                    ],
                  ),
                ),
                _StatusChip(status: application.status),
              ],
            ),
            const SizedBox(height: 14),
            Text(application.coverNote),
            const Divider(height: 30),
            ExpansionTile(
              tilePadding: EdgeInsets.zero,
              childrenPadding: EdgeInsets.zero,
              title: Text(
                'Audit history (${application.statusHistory.length})',
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
              children: application.statusHistory
                  .map(
                    (event) => ListTile(
                      contentPadding: EdgeInsets.zero,
                      leading: const Icon(Icons.history, size: 20),
                      title: Text(_readable(event.newStatus)),
                      subtitle: Text('${event.actorRole} · ${event.note}'),
                    ),
                  )
                  .toList(),
            ),
            if (active) ...[
              const SizedBox(height: 8),
              Wrap(
                spacing: 10,
                runSpacing: 8,
                children: [
                  if (application.status == 'MoreInformationRequested')
                    FilledButton(
                      onPressed: () => _requestNote(
                        context,
                        title: 'Send requested information',
                        action: (note) => context
                            .read<ApplicationsViewModel>()
                            .respond(application.id, note),
                      ),
                      child: const Text('Respond'),
                    ),
                  OutlinedButton(
                    onPressed: () => _requestNote(
                      context,
                      title: 'Withdraw application',
                      action: (note) => context
                          .read<ApplicationsViewModel>()
                          .withdraw(application.id, note),
                    ),
                    child: const Text('Withdraw'),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }

  Future<void> _requestNote(
    BuildContext context, {
    required String title,
    required Future<bool> Function(String note) action,
  }) async {
    final controller = TextEditingController();
    final note = await showDialog<String>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text(title),
        content: TextField(
          controller: controller,
          minLines: 2,
          maxLines: 5,
          maxLength: 1000,
          decoration: const InputDecoration(labelText: 'Note'),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () {
              final value = controller.text.trim();
              if (value.length >= 2) Navigator.pop(dialogContext, value);
            },
            child: const Text('Confirm'),
          ),
        ],
      ),
    );
    controller.dispose();
    if (note != null) await action(note);
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) => Chip(
    label: Text(_readable(status)),
    backgroundColor: status == 'Shortlisted'
        ? const Color(0xFFE2F3E9)
        : status == 'Rejected' || status == 'Withdrawn'
        ? const Color(0xFFF3E8E8)
        : const Color(0xFFE7F1F5),
  );
}

String _readable(String value) =>
    value.replaceAllMapped(RegExp(r'(?<=[a-z])(?=[A-Z])'), (_) => ' ');
