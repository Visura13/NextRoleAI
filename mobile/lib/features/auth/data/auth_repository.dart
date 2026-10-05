import '../../../core/api/api_client.dart';
import '../../../core/storage/session_storage.dart';
import '../models/auth_models.dart';

abstract interface class AuthDataSource {
  Future<AuthSession?> restoreSession();
  Future<AuthSession> login({required String email, required String password});
  Future<AuthSession> register(RegistrationInput input);
  Future<void> logout();
}

class AuthRepository implements AuthDataSource {
  const AuthRepository(this._apiClient, this._sessionStorage);

  final ApiClient _apiClient;
  final SessionStorage _sessionStorage;

  @override
  Future<AuthSession?> restoreSession() async {
    final stored = await _sessionStorage.read();
    if (stored == null) return null;

    try {
      final user = CurrentUser.fromJson(
        await _apiClient.getJson('/api/auth/me'),
      );
      final refreshed = (await _sessionStorage.read() ?? stored).withUser(user);
      await _ensureJobSeeker(refreshed);
      await _sessionStorage.write(refreshed);
      return refreshed;
    } on Object {
      await _sessionStorage.clear();
      return null;
    }
  }

  @override
  Future<AuthSession> login({
    required String email,
    required String password,
  }) =>
      _authenticate('/api/auth/login', {'email': email, 'password': password});

  @override
  Future<AuthSession> register(RegistrationInput input) =>
      _authenticate('/api/auth/register/job-seeker', input.toJson());

  Future<AuthSession> _authenticate(
    String path,
    Map<String, dynamic> body,
  ) async {
    final session = AuthSession.fromJson(
      await _apiClient.postJson(path, body: body, authenticated: false),
    );
    await _ensureJobSeeker(session);
    await _sessionStorage.write(session);
    return session;
  }

  Future<void> _ensureJobSeeker(AuthSession session) async {
    if (session.user.role == 'JobSeeker') return;
    await _sessionStorage.clear();
    throw const ApiException(
      'Recruiter accounts use the NextRoleAI web application.',
      statusCode: 403,
    );
  }

  @override
  Future<void> logout() async {
    final session = await _sessionStorage.read();
    try {
      if (session != null) {
        await _apiClient.postEmpty(
          '/api/auth/logout',
          body: {'refreshToken': session.refreshToken},
          authenticated: false,
        );
      }
    } finally {
      await _sessionStorage.clear();
    }
  }
}
