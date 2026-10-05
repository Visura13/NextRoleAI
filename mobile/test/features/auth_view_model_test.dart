import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/core/api/api_client.dart';
import 'package:nextroleai_mobile/features/auth/state/auth_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('initialize restores an existing Job Seeker session', () async {
    final repository = FakeAuthDataSource()..restored = sampleSession();
    final viewModel = AuthViewModel(repository);

    await viewModel.initialize();

    expect(viewModel.status, AuthStatus.signedIn);
    expect(viewModel.user?.email, 'alex@example.com');
  });

  test('failed login exposes the API message and stays signed out', () async {
    final repository = FakeAuthDataSource()
      ..loginError = const ApiException(
        'Email or password is incorrect.',
        statusCode: 401,
      );
    final viewModel = AuthViewModel(repository);
    await viewModel.initialize();

    final succeeded = await viewModel.login(
      email: 'alex@example.com',
      password: 'wrong',
    );

    expect(succeeded, isFalse);
    expect(viewModel.status, AuthStatus.signedOut);
    expect(viewModel.errorMessage, 'Email or password is incorrect.');
  });

  test('logout clears local authenticated state', () async {
    final repository = FakeAuthDataSource()..restored = sampleSession();
    final viewModel = AuthViewModel(repository);
    await viewModel.initialize();

    await viewModel.logout();

    expect(repository.didLogout, isTrue);
    expect(viewModel.status, AuthStatus.signedOut);
    expect(viewModel.session, isNull);
  });
}
