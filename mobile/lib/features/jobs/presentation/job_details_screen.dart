import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../state/jobs_view_model.dart';
import 'job_card.dart';

class JobDetailsScreen extends StatelessWidget {
  const JobDetailsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<JobDetailsViewModel>();
    final job = viewModel.job;
    return Scaffold(
      appBar: AppBar(title: const Text('Role details')),
      body: viewModel.isLoading
          ? const LoadingPanel(label: 'Loading role details')
          : viewModel.errorMessage != null || job == null
          ? MessagePanel(
              icon: Icons.work_off_outlined,
              title: 'Role unavailable',
              message:
                  viewModel.errorMessage ?? 'This role is no longer available.',
            )
          : ListView(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 32),
              children: [
                Text(
                  job.companyName.toUpperCase(),
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(
                    fontWeight: FontWeight.w800,
                    letterSpacing: 1.2,
                  ),
                ),
                const SizedBox(height: 10),
                Text(
                  job.title,
                  style: Theme.of(context).textTheme.headlineMedium
                      ?.copyWith(fontWeight: FontWeight.w900),
                ),
                const SizedBox(height: 12),
                Text(
                  '${job.location} · ${readableJobValue(job.workMode)} · ${readableJobValue(job.employmentType)}',
                ),
                const SizedBox(height: 24),
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(18),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _DetailRow(label: 'Salary', value: jobSalary(job)),
                        _DetailRow(
                          label: 'Experience',
                          value: '${job.minimumYearsExperience}+ years',
                        ),
                        _DetailRow(
                          label: 'Closes',
                          value: job.closesAtUtc == null
                              ? 'Open until filled'
                              : '${job.closesAtUtc!.day}/${job.closesAtUtc!.month}/${job.closesAtUtc!.year}',
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 24),
                Text(
                  'About the role',
                  style: Theme.of(context).textTheme.titleLarge
                      ?.copyWith(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 10),
                Text(job.description, style: const TextStyle(height: 1.6)),
                const SizedBox(height: 24),
                Text(
                  'Skills',
                  style: Theme.of(context).textTheme.titleLarge
                      ?.copyWith(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 10),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: job.skills
                      .map(
                        (skill) => Chip(
                          label: Text(
                            '${skill.name}${skill.isRequired ? ' · required' : ''}',
                          ),
                        ),
                      )
                      .toList(),
                ),
                const SizedBox(height: 28),
                const FeedbackBanner(
                  message: 'Applications open in Part 8 after approval, audit history, and notification rules are implemented.',
                ),
              ],
            ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 8),
    child: Row(
      children: [
        Expanded(
          child: Text(
            label,
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
        ),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w700)),
      ],
    ),
  );
}
