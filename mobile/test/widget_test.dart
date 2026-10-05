import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:nextroleai_mobile/features/auth/presentation/login_screen.dart';
import 'package:nextroleai_mobile/features/auth/presentation/register_screen.dart';
import 'package:nextroleai_mobile/features/auth/state/auth_view_model.dart';
import 'package:provider/provider.dart';

import 'support/fakes.dart';

void main() {
  testWidgets('login validates required fields and navigates to registration', (
    tester,
  ) async {
    final auth = AuthViewModel(FakeAuthDataSource());
    await auth.initialize();
    final router = GoRouter(
      initialLocation: '/login',
      routes: [
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginScreen(),
        ),
        GoRoute(
          path: '/register',
          builder: (context, state) => const RegisterScreen(),
        ),
      ],
    );
    addTearDown(router.dispose);

    await tester.pumpWidget(
      ChangeNotifierProvider.value(
        value: auth,
        child: MaterialApp.router(routerConfig: router),
      ),
    );
    await tester.pumpAndSettle();

    await tester.tap(find.byKey(const Key('login-submit')));
    await tester.pump();
    expect(find.text('Enter your email address.'), findsOneWidget);
    expect(find.text('Enter your password.'), findsOneWidget);

    await tester.tap(find.textContaining('Create an account'));
    await tester.pumpAndSettle();
    expect(find.text('Create Job Seeker account'), findsOneWidget);
  });
}
