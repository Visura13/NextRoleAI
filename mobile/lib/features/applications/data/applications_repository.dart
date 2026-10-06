import '../../../core/api/api_client.dart';
import '../models/job_application.dart';

abstract interface class ApplicationsDataSource {
  Future<List<JobApplication>> list();

  Future<JobApplication> submit({
    required String jobPostingId,
    required String coverNote,
    String? sourceWorkflowRunId,
  });

  Future<JobApplication> respond(String applicationId, String note);

  Future<JobApplication> withdraw(String applicationId, String note);
}

class ApplicationsRepository implements ApplicationsDataSource {
  const ApplicationsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<JobApplication>> list() async {
    final response = await _apiClient.getJson(
      '/api/applications',
      query: const {'pageSize': '100'},
    );
    return (response['items'] as List<dynamic>)
        .map((item) => JobApplication.fromJson(item as Map<String, dynamic>))
        .toList(growable: false);
  }

  @override
  Future<JobApplication> submit({
    required String jobPostingId,
    required String coverNote,
    String? sourceWorkflowRunId,
  }) async => JobApplication.fromJson(
    await _apiClient.postJson(
      '/api/applications',
      body: {
        'jobPostingId': jobPostingId,
        'coverNote': coverNote,
        'sourceWorkflowRunId': sourceWorkflowRunId,
      },
    ),
  );

  @override
  Future<JobApplication> respond(String applicationId, String note) async =>
      JobApplication.fromJson(
        await _apiClient.postJson(
          '/api/applications/$applicationId/respond',
          body: {'note': note},
        ),
      );

  @override
  Future<JobApplication> withdraw(String applicationId, String note) async =>
      JobApplication.fromJson(
        await _apiClient.postJson(
          '/api/applications/$applicationId/withdraw',
          body: {'note': note},
        ),
      );
}
