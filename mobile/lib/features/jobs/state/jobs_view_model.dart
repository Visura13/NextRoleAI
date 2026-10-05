import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/jobs_repository.dart';
import '../models/job_models.dart';

class JobsViewModel extends ChangeNotifier {
  JobsViewModel(this._repository);

  final JobsDataSource _repository;
  List<Job> _jobs = const [];
  JobFilters _filters = const JobFilters();
  int _page = 1;
  int _totalPages = 0;
  bool _isLoading = false;
  bool _isLoadingMore = false;
  String? _errorMessage;

  List<Job> get jobs => _jobs;
  JobFilters get filters => _filters;
  bool get isLoading => _isLoading;
  bool get isLoadingMore => _isLoadingMore;
  bool get canLoadMore => _page < _totalPages;
  String? get errorMessage => _errorMessage;

  Future<void> load({JobFilters? filters}) async {
    if (_isLoading) return;
    _filters = filters ?? _filters;
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      final result = await _repository.search(_filters, page: 1);
      _jobs = result.items;
      _page = result.page;
      _totalPages = result.totalPages;
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'Open roles could not be loaded.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> loadMore() async {
    if (_isLoadingMore || !canLoadMore) return;
    _isLoadingMore = true;
    notifyListeners();
    try {
      final result = await _repository.search(_filters, page: _page + 1);
      _jobs = [..._jobs, ...result.items];
      _page = result.page;
      _totalPages = result.totalPages;
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } finally {
      _isLoadingMore = false;
      notifyListeners();
    }
  }
}

class JobDetailsViewModel extends ChangeNotifier {
  JobDetailsViewModel(this._repository);

  final JobsDataSource _repository;
  Job? _job;
  bool _isLoading = false;
  String? _errorMessage;

  Job? get job => _job;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  Future<void> load(String id) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _job = await _repository.getById(id);
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'This role could not be loaded.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
