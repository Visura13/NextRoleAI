import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../models/job_models.dart';
import '../state/jobs_view_model.dart';
import 'job_card.dart';

class JobsScreen extends StatefulWidget {
  const JobsScreen({super.key});

  @override
  State<JobsScreen> createState() => _JobsScreenState();
}

class _JobsScreenState extends State<JobsScreen> {
  final _searchController = TextEditingController();
  final _locationController = TextEditingController();
  String _employmentType = '';
  String _workMode = '';

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final jobs = context.read<JobsViewModel>();
      if (jobs.jobs.isEmpty && !jobs.isLoading) jobs.load();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    _locationController.dispose();
    super.dispose();
  }

  Future<void> _search() => context.read<JobsViewModel>().load(
    filters: JobFilters(
      search: _searchController.text,
      location: _locationController.text,
      employmentType: _employmentType,
      workMode: _workMode,
    ),
  );

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<JobsViewModel>();
    return Scaffold(
      appBar: AppBar(title: const Text('Explore jobs')),
      body: RefreshIndicator(
        onRefresh: _search,
        child: CustomScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          slivers: [
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(20, 10, 20, 16),
              sliver: SliverToBoxAdapter(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Find work that fits.',
                      style: Theme.of(context).textTheme.headlineSmall
                          ?.copyWith(fontWeight: FontWeight.w900),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Search the shared catalog now. Explainable ranking arrives in Part 6.',
                      style: TextStyle(
                        color: Theme.of(context).colorScheme.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 18),
                    SearchBar(
                      controller: _searchController,
                      hintText: 'Title, company, or skill',
                      leading: const Icon(Icons.search),
                      trailing: [
                        IconButton(
                          tooltip: 'Search filters',
                          onPressed: _openFilters,
                          icon: const Icon(Icons.tune),
                        ),
                      ],
                      onSubmitted: (_) => _search(),
                    ),
                  ],
                ),
              ),
            ),
            if (viewModel.isLoading)
              const SliverFillRemaining(
                child: LoadingPanel(label: 'Finding open roles'),
              )
            else if (viewModel.errorMessage != null)
              SliverFillRemaining(
                child: MessagePanel(
                  icon: Icons.cloud_off_outlined,
                  title: 'Jobs are unavailable',
                  message: viewModel.errorMessage!,
                  action: FilledButton(
                    onPressed: _search,
                    child: const Text('Try again'),
                  ),
                ),
              )
            else if (viewModel.jobs.isEmpty)
              const SliverFillRemaining(
                child: MessagePanel(
                  icon: Icons.search_off,
                  title: 'No matching roles',
                  message: 'Try a broader keyword, location, or work mode.',
                ),
              )
            else
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(20, 0, 20, 24),
                sliver: SliverList.builder(
                  itemCount:
                      viewModel.jobs.length + (viewModel.canLoadMore ? 1 : 0),
                  itemBuilder: (context, index) {
                    if (index == viewModel.jobs.length) {
                      return Padding(
                        padding: const EdgeInsets.only(top: 6),
                        child: OutlinedButton(
                          onPressed: viewModel.isLoadingMore
                              ? null
                              : viewModel.loadMore,
                          child: Text(
                            viewModel.isLoadingMore ? 'Loading…' : 'Load more',
                          ),
                        ),
                      );
                    }
                    final job = viewModel.jobs[index];
                    return JobCard(
                      job: job,
                      onTap: () => context.push('/jobs/${job.id}'),
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _openFilters() async {
    final result =
        await showModalBottomSheet<
          ({String location, String employmentType, String workMode})
        >(
          context: context,
          isScrollControlled: true,
          builder: (context) => _FilterSheet(
            location: _locationController.text,
            employmentType: _employmentType,
            workMode: _workMode,
          ),
        );
    if (result == null || !mounted) return;
    _locationController.text = result.location;
    _employmentType = result.employmentType;
    _workMode = result.workMode;
    await _search();
  }
}

class _FilterSheet extends StatefulWidget {
  const _FilterSheet({
    required this.location,
    required this.employmentType,
    required this.workMode,
  });

  final String location;
  final String employmentType;
  final String workMode;

  @override
  State<_FilterSheet> createState() => _FilterSheetState();
}

class _FilterSheetState extends State<_FilterSheet> {
  late final TextEditingController _locationController = TextEditingController(
    text: widget.location,
  );
  late String _employmentType = widget.employmentType;
  late String _workMode = widget.workMode;

  @override
  void dispose() {
    _locationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => SafeArea(
    child: Padding(
      padding: EdgeInsets.fromLTRB(
        20,
        20,
        20,
        20 + MediaQuery.viewInsetsOf(context).bottom,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Filter opportunities',
            style: Theme.of(context).textTheme.titleLarge
                ?.copyWith(fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 18),
          TextField(
            controller: _locationController,
            decoration: const InputDecoration(labelText: 'Location'),
          ),
          const SizedBox(height: 14),
          DropdownButtonFormField(
            initialValue: _employmentType,
            decoration: const InputDecoration(labelText: 'Employment type'),
            items: const [
              DropdownMenuItem(value: '', child: Text('Any type')),
              DropdownMenuItem(value: 'FullTime', child: Text('Full time')),
              DropdownMenuItem(value: 'PartTime', child: Text('Part time')),
              DropdownMenuItem(value: 'Contract', child: Text('Contract')),
              DropdownMenuItem(value: 'Internship', child: Text('Internship')),
            ],
            onChanged: (value) => setState(() => _employmentType = value ?? ''),
          ),
          const SizedBox(height: 14),
          DropdownButtonFormField(
            initialValue: _workMode,
            decoration: const InputDecoration(labelText: 'Work mode'),
            items: const [
              DropdownMenuItem(value: '', child: Text('Any mode')),
              DropdownMenuItem(value: 'OnSite', child: Text('On-site')),
              DropdownMenuItem(value: 'Hybrid', child: Text('Hybrid')),
              DropdownMenuItem(value: 'Remote', child: Text('Remote')),
            ],
            onChanged: (value) => setState(() => _workMode = value ?? ''),
          ),
          const SizedBox(height: 20),
          FilledButton(
            onPressed: () => Navigator.pop(context, (
              location: _locationController.text,
              employmentType: _employmentType,
              workMode: _workMode,
            )),
            child: const Text('Apply filters'),
          ),
        ],
      ),
    ),
  );
}
