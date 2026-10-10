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
            'AGENTIC JOB SEARCH',
            style: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: AppTheme.orange,
              fontWeight: FontWeight.w800,
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Delegate the search. Discover your best-fit roles.',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          const Text(
            'Specialized agents use your confirmed profile to find and rank relevant opportunities.',
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
              isExpanded: true,
              decoration: const InputDecoration(labelText: 'Workflow history'),
              items: model.items
                  .map(
                    (workflow) => DropdownMenuItem(
                      value: workflow.id,
                      child: Text(
                        workflow.objective,
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
  const _WorkflowDetails({required this.workflow});

  final AgentWorkflow workflow;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    children: [
      if (workflow.failureMessage != null) ...[
        _MessagePanel(
          message: '${workflow.failureCode}: ${workflow.failureMessage}',
          isError: true,
        ),
        const SizedBox(height: 18),
      ],
      Text(
        'Agentic search results',
        style: Theme.of(context).textTheme.titleLarge,
      ),
      const SizedBox(height: 8),
      if (workflow.shortlist.isEmpty)
        const Text('No matching jobs were found by this run.')
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
            ),
          ),
        ),
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
