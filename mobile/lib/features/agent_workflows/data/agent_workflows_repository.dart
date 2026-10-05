import '../../../core/api/api_client.dart';
import '../models/agent_workflow.dart';

abstract interface class AgentWorkflowsDataSource {
  Future<List<AgentWorkflow>> list();
  Future<AgentWorkflow> start(String objective);
  Future<AgentWorkflow> decide({
    required String workflowId,
    required String decision,
    required String feedback,
    String? revisedObjective,
  });
}

class AgentWorkflowsRepository implements AgentWorkflowsDataSource {
  AgentWorkflowsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<AgentWorkflow>> list() async {
    final json = await _apiClient.getJson(
      '/api/agent-workflows',
      authenticated: true,
      query: {'pageSize': '10'},
    );
    return (json['items'] as List<dynamic>)
        .map((item) => AgentWorkflow.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<AgentWorkflow> start(String objective) async => AgentWorkflow.fromJson(
    await _apiClient.postJson(
      '/api/agent-workflows',
      body: {'objective': objective},
      authenticated: true,
    ),
  );

  @override
  Future<AgentWorkflow> decide({
    required String workflowId,
    required String decision,
    required String feedback,
    String? revisedObjective,
  }) async => AgentWorkflow.fromJson(
    await _apiClient.postJson(
      '/api/agent-workflows/$workflowId/decision',
      body: {
        'decision': decision,
        'feedback': feedback,
        'revisedObjective': revisedObjective,
      },
      authenticated: true,
    ),
  );
}
