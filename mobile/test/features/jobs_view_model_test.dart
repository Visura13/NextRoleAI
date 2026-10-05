import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/features/jobs/models/job_models.dart';
import 'package:nextroleai_mobile/features/jobs/state/jobs_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('loads a filtered first page and appends the next page', () async {
    final repository = FakeJobsDataSource()
      ..pages[1] = PagedJobs(
        items: [sampleJob()],
        page: 1,
        pageSize: 1,
        totalCount: 2,
        totalPages: 2,
      )
      ..pages[2] = PagedJobs(
        items: [sampleJob(id: 'job-2')],
        page: 2,
        pageSize: 1,
        totalCount: 2,
        totalPages: 2,
      );
    final viewModel = JobsViewModel(repository);

    await viewModel.load(
      filters: const JobFilters(search: 'Flutter', workMode: 'Hybrid'),
    );
    await viewModel.loadMore();

    expect(viewModel.filters.search, 'Flutter');
    expect(viewModel.jobs.map((job) => job.id), ['job-1', 'job-2']);
    expect(viewModel.canLoadMore, isFalse);
  });
}
