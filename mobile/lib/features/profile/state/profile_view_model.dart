import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/profile_repository.dart';
import '../models/job_seeker_profile.dart';

class ProfileViewModel extends ChangeNotifier {
  ProfileViewModel(this._repository);

  final ProfileDataSource _repository;
  JobSeekerProfile? _profile;
  bool _isLoading = false;
  bool _isSaving = false;
  String? _errorMessage;
  String? _successMessage;

  JobSeekerProfile? get profile => _profile;
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
      _profile = await _repository.getProfile();
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'Your profile could not be loaded.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> save(JobSeekerProfile profile) async {
    _isSaving = true;
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
    try {
      _profile = await _repository.saveProfile(profile);
      _successMessage = 'Your career profile has been saved.';
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      return false;
    } on Object {
      _errorMessage = 'Your profile could not be saved.';
      return false;
    } finally {
      _isSaving = false;
      notifyListeners();
    }
  }

  void clearMessages() {
    if (_errorMessage == null && _successMessage == null) return;
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
  }
}
