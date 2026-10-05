import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/brand.dart';
import '../models/agent_workflow.dart';
import '../state/agent_workflows_view_model.dart';

class AgentWorkflowsScreen extends StatefulWidget {
  const AgentWorkflowsScreen({super.key});

  @override
  State<AgentWorkflowsScreen> createState() => _AgentWorkflowsScreenState();
}

class _AgentWorkflowsScreenState extends State<AgentWorkflowsScreen> {
  final objectiveController = TextEditingController(
    text: 'Find the best 3 software engineering jobs for my confirmed profile.',
  );
  final feedbackController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback(
      (_) => context.read<AgentWorkflowsViewModel>().load(),
    );
  }

  @override
  void dispose() {
    objectiveController.dispose();
    feedbackController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const NextRoleBrand()),
    body: Consumer<AgentWorkflowsViewModel>(
      builder: (context, model, _) => ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 40),
        children: [
          Text(
            'CONTROLLED AGENTIC AI',
            style: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: AppTheme.orange,
              fontWeight: FontWeight.w800,
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Delegate the search. Keep the final say.',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          const Text(
            'Four agents plan, inspect your confirmed profile, discover roles, and validate the shortlist. Publication pauses for your approval.',
          ),
          const SizedBox(height: 20),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextField(
                    controller: objectiveController,
                    minLines: 2,
                    maxLines: 4,
                    maxLength: 500,
                    decoration: const InputDecoration(
                      labelText: 'Job-search objective',
                      helperText: 'Objectives are treated as data and checked for instruction injection.',
                    ),
                  ),
                  const SizedBox(height: 12),
                  FilledButton.icon(
                    onPressed: model.isLoading
                        ? null
                        : () {
                            if (objectiveController.text.trim().length < 20) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text(
                                    'Use at least 20 characters for the objective.',
                                  ),
                                ),
                              );
                              return;
                            }
                            model.start(objectiveController.text);
                          },
                    icon: const Icon(Icons.auto_awesome_outlined),
                    label: Text(
                      model.isLoading
                          ? 'Running agents…'
                          : 'Start controlled workflow',
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (model.errorMessage != null) ...[
            const SizedBox(height: 12),
            _MessagePanel(message: model.errorMessage!, isError: true),
          ],
          if (model.items.isNotEmpty) ...[
            const SizedBox(height: 18),
            DropdownButtonFormField<String>(
              initialValue: model.selected?.id,
              decoration: const InputDecoration(labelText: 'Workflow history'),
              items: model.items
                  .map(
                    (workflow) => DropdownMenuItem(
                      value: workflow.id,
                      child: Text(
                        '${workflow.status} · ${workflow.objective}',
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  )
                  .toList(),
              onChanged: (id) {
                if (id == null) return;
                final workflow = model.items.singleWhere(
                  (item) => item.id == id,
                );
                model.select(workflow);
                objectiveController.text = workflow.objective;
              },
            ),
          ],
          if (model.selected case final workflow?) ...[
            const SizedBox(height: 18),
            _WorkflowDetails(
              workflow: workflow,
              isBusy: model.isLoading,
              objectiveController: objectiveController,
              feedbackController: feedbackController,
              onDecision: (decision) => model.decide(
                decision: decision,
                feedback: feedbackController.text,
                revisedObjective: decision == 'RequestRevision'
                    ? objectiveController.text
                    : null,
              ),
            ),
          ] else if (!model.isLoading) ...[
            const SizedBox(height: 30),
            const Center(child: Text('No workflow history yet.')),
          ],
        ],
      ),
    ),
  );
}

class _WorkflowDetails extends StatelessWidget {
  const _WorkflowDetails({
    required this.workflow,
    required this.isBusy,
    required this.objectiveController,
    required this.feedbackController,
    required this.onDecision,
  });

  final AgentWorkflow workflow;
  final bool isBusy;
  final TextEditingController objectiveController;
  final TextEditingController feedbackController;
  final ValueChanged<String> onDecision;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    children: [
      Card(
        color: AppTheme.forestDeep,
        child: Padding(
          padding: const EdgeInsets.all(18),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                '${workflow.status} · ${workflow.approvalStatus}',
                style: const TextStyle(
                  color: AppTheme.lime,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                workflow.finalSummary ?? workflow.objective,
                style: const TextStyle(color: Colors.white, height: 1.45),
              ),
              if (workflow.failureMessage != null) ...[
                const SizedBox(height: 10),
                Text(
                  '${workflow.failureCode}: ${workflow.failureMessage}',
                  style: const TextStyle(color: Color(0xFFFFC6BE)),
                ),
              ],
            ],
          ),
        ),
      ),
      const SizedBox(height: 18),
      Text('Proposed shortlist', style: Theme.of(context).textTheme.titleLarge),
      const SizedBox(height: 8),
      if (workflow.shortlist.isEmpty)
        const Text('No jobs were published by this run.')
      else
        ...workflow.shortlist.map(
          (item) => Card(
            child: ListTile(
              leading: CircleAvatar(child: Text('${item.score}')),
              title: Text('#${item.rank} ${item.title}'),
              subtitle: Text(
                '${item.companyName} · ${item.location} · ${item.workMode}\n${item.reasonSummary}',
              ),
              isThreeLine: true,
              trailing: Icon(
                item.isApproved ? Icons.verified : Icons.pending_outlined,
                color: item.isApproved ? Colors.green : AppTheme.orange,
              ),
            ),
          ),
        ),
      const SizedBox(height: 14),
      ExpansionTile(
        tilePadding: EdgeInsets.zero,
        title: const Text('Deterministic validation'),
        children: workflow.validationResults
            .map(
              (result) => ListTile(
                contentPadding: EdgeInsets.zero,
                leading: Icon(
                  result.passed ? Icons.check_circle : Icons.cancel,
                  color: result.passed ? Colors.green : Colors.red,
                ),
                title: Text(result.ruleName),
                subtitle: Text(result.message),
              ),
            )
            .toList(),
      ),
      ExpansionTile(
        tilePadding: EdgeInsets.zero,
        title: const Text('Agent execution history'),
        children: workflow.steps
            .map(
              (step) => ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text('${step.agentName} · ${step.status}'),
                subtitle: Text(
                  '${step.responsibility}\nTools: ${step.allowedTools.isEmpty ? 'none' : step.allowedTools.join(', ')}\n${step.toolCalls.map((call) => '${call.toolName}: ${call.succeeded ? 'success' : 'failed'}').join(' · ')}',
                ),
                isThreeLine: true,
              ),
            )
            .toList(),
      ),
      if (workflow.status == 'PendingApproval') ...[
        const SizedBox(height: 14),
        Card(
          color: const Color(0xFFFFF4DF),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Human approval required',
                  style: Theme.of(context).textTheme.titleMedium
                      ?.copyWith(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 8),
                const Text(
                  'Approve only after reviewing the jobs and validation evidence. Revision reruns every agent and check.',
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: feedbackController,
                  maxLength: 1000,
                  maxLines: 2,
                  decoration: const InputDecoration(labelText: 'Decision note'),
                ),
                FilledButton(
                  onPressed: isBusy ? null : () => onDecision('Approve'),
                  child: const Text('Approve shortlist'),
                ),
                OutlinedButton(
                  onPressed: isBusy
                      ? null
                      : () => onDecision('RequestRevision'),
                  child: const Text('Request revision'),
                ),
                TextButton(
                  onPressed: isBusy ? null : () => onDecision('Reject'),
                  child: const Text('Reject'),
                ),
              ],
            ),
          ),
        ),
      ],
    ],
  );
}

class _MessagePanel extends StatelessWidget {
  const _MessagePanel({required this.message, required this.isError});

  final String message;
  final bool isError;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(
      color: isError ? const Color(0xFFFFE9E6) : const Color(0xFFE8F4EC),
      borderRadius: BorderRadius.circular(10),
    ),
    child: Text(message),
  );
}
