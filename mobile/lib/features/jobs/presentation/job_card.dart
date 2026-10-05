import 'package:flutter/material.dart';

import '../../../core/theme/app_theme.dart';
import '../models/job_models.dart';

String readableJobValue(String value) => value
    .replaceAllMapped(
      RegExp(r'([a-z])([A-Z])'),
      (match) => '${match[1]} ${match[2]}',
    )
    .replaceFirst('On Site', 'On-site');

String jobSalary(Job job) {
  if (job.salaryMinimum == null && job.salaryMaximum == null) {
    return 'Salary not listed';
  }
  final values = [
    job.salaryMinimum,
    job.salaryMaximum,
  ].whereType<num>().map((value) => value.toStringAsFixed(0)).join(' – ');
  return '${job.salaryCurrency ?? ''} $values'.trim();
}

class JobCard extends StatelessWidget {
  const JobCard({required this.job, required this.onTap, super.key});

  final Job job;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Card(
    margin: const EdgeInsets.only(bottom: 14),
    child: InkWell(
      borderRadius: BorderRadius.circular(18),
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(
                  backgroundColor: const Color(0xFFEDF9CE),
                  foregroundColor: AppTheme.forest,
                  child: Text(job.companyName.substring(0, 1).toUpperCase()),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        job.title,
                        style: Theme.of(context).textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.w800),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        job.companyName,
                        style: TextStyle(
                          color: Theme.of(context).colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ],
                  ),
                ),
                const Icon(Icons.arrow_forward_ios, size: 16),
              ],
            ),
            const SizedBox(height: 14),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                _MetaChip(
                  icon: Icons.location_on_outlined,
                  label: job.location,
                ),
                _MetaChip(
                  icon: Icons.work_outline,
                  label: readableJobValue(job.employmentType),
                ),
                _MetaChip(
                  icon: Icons.laptop_outlined,
                  label: readableJobValue(job.workMode),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Text(
              jobSalary(job),
              style: const TextStyle(fontWeight: FontWeight.w800),
            ),
          ],
        ),
      ),
    ),
  );
}

class _MetaChip extends StatelessWidget {
  const _MetaChip({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
    decoration: BoxDecoration(
      color: const Color(0xFFF0F3EE),
      borderRadius: BorderRadius.circular(20),
    ),
    child: Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14),
        const SizedBox(width: 4),
        Text(label, style: const TextStyle(fontSize: 12)),
      ],
    ),
  );
}
