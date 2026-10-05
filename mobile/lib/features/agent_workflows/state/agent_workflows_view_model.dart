import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/agent_workflows_repository.dart';
import '../models/agent_workflow.dart';

class AgentWorkflowsViewModel extends ChangeNotifier {
  AgentWorkflowsViewModel(this._repository);

  final AgentWorkflowsDataSource _repository;
  List<AgentWorkflow> _items = const [];
  AgentWorkflow? _selected;
  bool _isLoading = false;
  String? _errorMessage;

  List<AgentWorkflow> get items => _items;
  AgentWorkflow? get selected => _selected;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  Future<void> load() async => _run(() async {
    _items = await _repository.list();
    _selected ??= _items.firstOrNull;
  });

  Future<void> start(String objective) async => _run(() async {
    _selected = await _repository.start(objective.trim());
    _items = [_selected!, ..._items.where((item) => item.id != _selected!.id)];
  });

  Future<void> decide({
    required String decision,
    required String feedback,
    String? revisedObjective,
  }) async {
    final workflow = _selected;
    if (workflow == null) return;
    await _run(() async {
      _selected = await _repository.decide(
        workflowId: workflow.id,
        decision: decision,
        feedback: feedback.trim(),
        revisedObjective: revisedObjective?.trim(),
      );
      _items = [
        _selected!,
        ..._items.where((item) => item.id != _selected!.id),
      ];
    });
  }

  void select(AgentWorkflow workflow) {
    _selected = workflow;
    notifyListeners();
  }

  Future<void> _run(Future<void> Function() action) async {
    if (_isLoading) return;
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      await action();
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'The agent workflow could not be completed.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
