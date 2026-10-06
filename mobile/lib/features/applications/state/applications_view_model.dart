import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/applications_repository.dart';
import '../models/job_application.dart';

class ApplicationsViewModel extends ChangeNotifier {
  ApplicationsViewModel(this._repository);

  final ApplicationsDataSource _repository;
  List<JobApplication> _items = const [];
  bool _isLoading = false;
  bool _isSaving = false;
  String? _errorMessage;
  String? _successMessage;

  List<JobApplication> get items => _items;
  bool get isLoading => _isLoading;
  bool get isSaving => _isSaving;
  String? get errorMessage => _errorMessage;
  String? get successMessage => _successMessage;

  Future<void> load() async {
    if (_isLoading) return;
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _items = await _repository.list();
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'Applications could not be loaded.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> submit({
    required String jobPostingId,
    required String coverNote,
    String? sourceWorkflowRunId,
  }) => _save(
    () => _repository.submit(
      jobPostingId: jobPostingId,
      coverNote: coverNote,
      sourceWorkflowRunId: sourceWorkflowRunId,
    ),
    'Application submitted. Recruiter notification was recorded.',
  );

  Future<bool> respond(String applicationId, String note) => _save(
    () => _repository.respond(applicationId, note),
    'Your response was sent to the recruiter.',
  );

  Future<bool> withdraw(String applicationId, String note) => _save(
    () => _repository.withdraw(applicationId, note),
    'Application withdrawn.',
  );

  Future<bool> _save(
    Future<JobApplication> Function() operation,
    String message,
  ) async {
    if (_isSaving) return false;
    _isSaving = true;
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
    try {
      final updated = await operation();
      _items = [updated, ..._items.where((item) => item.id != updated.id)];
      _successMessage = message;
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      return false;
    } on Object {
      _errorMessage = 'The application could not be updated.';
      return false;
    } finally {
      _isSaving = false;
      notifyListeners();
    }
  }
}
