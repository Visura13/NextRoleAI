import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/features/agent_workflows/presentation/agent_workflows_screen.dart';
import 'package:nextroleai_mobile/features/agent_workflows/state/agent_workflows_view_model.dart';
import 'package:provider/provider.dart';

import '../support/fakes.dart';

void main() {
  testWidgets(
    'shows only run history and agentic search result cards',
    (tester) async {
      await tester.binding.setSurfaceSize(const Size(360, 800));
      addTearDown(() => tester.binding.setSurfaceSize(null));
      final repository = FakeAgentWorkflowsDataSource()
        ..items = [sampleAgentWorkflow()];

      await tester.pumpWidget(
        ChangeNotifierProvider(
          create: (_) => AgentWorkflowsViewModel(repository),
          child: const MaterialApp(home: AgentWorkflowsScreen()),
        ),
      );
      await tester.pumpAndSettle();

      expect(find.text('Workflow history'), findsOneWidget);
      expect(find.text('Deterministic validation'), findsNothing);
      expect(find.text('Agent execution history'), findsNothing);
      expect(find.text('Human approval required'), findsNothing);
      expect(find.text('PendingApproval · Pending'), findsNothing);
      expect(
        find.text(
          '2 job(s) passed deterministic validation and await Job Seeker approval.',
        ),
        findsNothing,
      );
      expect(find.text('Decision note (optional)'), findsNothing);
      expect(find.text('Approve shortlist'), findsNothing);
      expect(find.text('Request revision'), findsNothing);
      expect(find.text('Reject'), findsNothing);
      expect(find.text('Agentic search results'), findsOneWidget);
      expect(tester.takeException(), isNull);
    },
  );
}
