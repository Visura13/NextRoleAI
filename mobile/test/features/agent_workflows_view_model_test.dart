import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/features/agent_workflows/state/agent_workflows_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('starts an auditable workflow and exposes the approval gate', () async {
    final repository = FakeAgentWorkflowsDataSource();
    final viewModel = AgentWorkflowsViewModel(repository);

    await viewModel.start(
      'Find the best 3 software engineering jobs for my profile.',
    );

    expect(viewModel.selected?.status, 'PendingApproval');
    expect(viewModel.selected?.validationResults.single.passed, isTrue);
    expect(viewModel.selected?.shortlist.single.isApproved, isFalse);
  });

  test('records approval and exposes the published shortlist', () async {
    final repository = FakeAgentWorkflowsDataSource()
      ..items = [sampleAgentWorkflow()];
    final viewModel = AgentWorkflowsViewModel(repository);

    await viewModel.load();
    await viewModel.decide(decision: 'Approve', feedback: 'Reviewed.');

    expect(repository.lastDecision, 'Approve');
    expect(viewModel.selected?.status, 'Completed');
    expect(viewModel.selected?.shortlist.single.isApproved, isTrue);
  });
}
