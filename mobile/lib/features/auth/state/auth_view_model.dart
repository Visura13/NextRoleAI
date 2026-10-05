import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/auth_repository.dart';
import '../models/auth_models.dart';

enum AuthStatus { initializing, signedOut, signedIn }

class AuthViewModel extends ChangeNotifier {
  AuthViewModel(this._repository);

  final AuthDataSource _repository;
  AuthStatus _status = AuthStatus.initializing;
  AuthSession? _session;
  bool _isSubmitting = false;
  String? _errorMessage;

  AuthStatus get status => _status;
  AuthSession? get session => _session;
  CurrentUser? get user => _session?.user;
  bool get isSubmitting => _isSubmitting;
  String? get errorMessage => _errorMessage;

  Future<void> initialize() async {
    _session = await _repository.restoreSession();
    _status = _session == null ? AuthStatus.signedOut : AuthStatus.signedIn;
    notifyListeners();
  }

  Future<bool> login({required String email, required String password}) =>
      _run(() => _repository.login(email: email, password: password));

  Future<bool> register(RegistrationInput input) =>
      _run(() => _repository.register(input));

  Future<bool> _run(Future<AuthSession> Function() action) async {
    _isSubmitting = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _session = await action();
      _status = AuthStatus.signedIn;
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      return false;
    } on Object {
      _errorMessage = 'We could not connect to NextRoleAI. Please try again.';
      return false;
    } finally {
      _isSubmitting = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    try {
      await _repository.logout();
    } finally {
      _session = null;
      _status = AuthStatus.signedOut;
      _errorMessage = null;
      notifyListeners();
    }
  }

  void clearError() {
    if (_errorMessage == null) return;
    _errorMessage = null;
    notifyListeners();
  }
}
