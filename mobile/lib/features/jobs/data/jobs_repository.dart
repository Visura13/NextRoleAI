import '../../../core/api/api_client.dart';
import '../models/job_models.dart';

abstract interface class JobsDataSource {
  Future<PagedJobs> search(JobFilters filters, {required int page});
  Future<Job> getById(String id);
}

class JobsRepository implements JobsDataSource {
  const JobsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<PagedJobs> search(JobFilters filters, {required int page}) async =>
      PagedJobs.fromJson(
        await _apiClient.getJson(
          '/api/jobs',
          query: filters.toQuery(page: page),
          authenticated: false,
        ),
      );

  @override
  Future<Job> getById(String id) async => Job.fromJson(
    await _apiClient.getJson('/api/jobs/$id', authenticated: false),
  );
}
