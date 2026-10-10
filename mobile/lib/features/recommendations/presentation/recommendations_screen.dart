import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/states.dart';
import '../../jobs/presentation/job_card.dart';
import '../models/recommendation.dart';
import '../state/recommendations_view_model.dart';

class RecommendationsScreen extends StatefulWidget {
  const RecommendationsScreen({super.key});

  @override
  State<RecommendationsScreen> createState() => _RecommendationsScreenState();
}

class _RecommendationsScreenState extends State<RecommendationsScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback(
      (_) => context.read<RecommendationsViewModel>().load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<RecommendationsViewModel>();
    return Scaffold(
      appBar: AppBar(title: const Text('Recommendations')),
      body: viewModel.isLoading
          ? const LoadingPanel(label: 'Ranking published jobs')
          : viewModel.errorMessage != null
          ? MessagePanel(
              icon: viewModel.cvRequired
                  ? Icons.description_outlined
                  : Icons.cloud_off_outlined,
              title: viewModel.cvRequired
                  ? 'Confirm your CV first'
                  : 'Recommendations unavailable',
              message: viewModel.errorMessage!,
              action: FilledButton(
                onPressed: viewModel.cvRequired
                    ? () => context.push('/cv')
                    : viewModel.load,
                child: Text(
                  viewModel.cvRequired ? 'Review my CV' : 'Try again',
                ),
              ),
            )
          : viewModel.items.isEmpty
          ? const MessagePanel(
              icon: Icons.work_off_outlined,
              title: 'No open jobs to rank',
              message: 'Recommendations appear when recruiters publish opportunities.',
            )
          : RefreshIndicator(
              onRefresh: viewModel.load,
              child: ListView(
                padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
                children: [
                  Text(
                    'Jobs ranked by AI semantic fit.',
                    style: Theme.of(context).textTheme.headlineSmall
                        ?.copyWith(fontWeight: FontWeight.w900),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'The AI compares transferable skills, responsibilities, experience, education, and work preferences.',
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 20),
                  ...viewModel.items.map(
                    (recommendation) =>
                        _RecommendationCard(recommendation: recommendation),
                  ),
                ],
              ),
            ),
    );
  }
}

class _RecommendationCard extends StatelessWidget {
  const _RecommendationCard({required this.recommendation});

  final JobRecommendation recommendation;

  @override
  Widget build(BuildContext context) => Card(
    margin: const EdgeInsets.only(bottom: 18),
    child: Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 66,
                height: 66,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppTheme.lime,
                  border: Border.all(color: AppTheme.forest, width: 3),
                ),
                child: Center(
                  child: Text(
                    recommendation.score.toStringAsFixed(1),
                    style: const TextStyle(fontWeight: FontWeight.w900),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  '${recommendation.algorithmVersion} · AI score',
                  style: TextStyle(
                    color: Theme.of(context).colorScheme.onSurfaceVariant,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          JobCard(
            job: recommendation.job,
            onTap: () => context.push('/jobs/${recommendation.job.id}'),
          ),
          const SizedBox(height: 8),
          _Breakdown(recommendation.breakdown),
          const SizedBox(height: 14),
          Text(
            'Why it ranked here',
            style: Theme.of(context).textTheme.titleMedium
                ?.copyWith(fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 6),
          ...recommendation.reasons.map(
            (reason) => Padding(
              padding: const EdgeInsets.only(bottom: 5),
              child: Text('• $reason'),
            ),
          ),
          if (recommendation.missingRequiredSkills.isNotEmpty) ...[
            const SizedBox(height: 10),
            Text(
              'Missing required skills',
              style: TextStyle(
                color: Theme.of(context).colorScheme.error,
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: 6),
            Wrap(
              spacing: 6,
              children: recommendation.missingRequiredSkills
                  .map((skill) => Chip(label: Text(skill)))
                  .toList(),
            ),
          ],
        ],
      ),
    ),
  );
}

class _Breakdown extends StatelessWidget {
  const _Breakdown(this.breakdown);

  final MatchBreakdown breakdown;

  @override
  Widget build(BuildContext context) => Column(
    children: [
      _row('Skills fit', breakdown.skillsFit, 40),
      _row('Role fit', breakdown.roleFit, 25),
      _row('Experience fit', breakdown.experienceFit, 15),
      _row('Education fit', breakdown.educationFit, 10),
      _row('Location fit', breakdown.locationFit, 10),
    ],
  );

  Widget _row(String label, num value, int maximum) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 3),
    child: Row(
      children: [
        Expanded(child: Text(label)),
        Text(
          '${value.toStringAsFixed(1)}/$maximum',
          style: const TextStyle(fontWeight: FontWeight.w700),
        ),
      ],
    ),
  );
}
