import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../models/auth_models.dart';
import '../state/auth_view_model.dart';
import 'auth_scaffold.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    FocusScope.of(context).unfocus();
    if (!_formKey.currentState!.validate()) return;
    await context.read<AuthViewModel>().register(
      RegistrationInput(
        firstName: _firstNameController.text.trim(),
        lastName: _lastNameController.text.trim(),
        email: _emailController.text.trim(),
        password: _passwordController.text,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthViewModel>();
    return AuthScaffold(
      eyebrow: 'Job seeker account',
      title: 'Build a profile for the work you want next.',
      subtitle: 'Recruiter registration is available in the web application. This mobile experience is focused on Job Seekers.',
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (auth.errorMessage != null) ...[
              FeedbackBanner(message: auth.errorMessage!, isError: true),
              const SizedBox(height: 18),
            ],
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(child: _nameField(_firstNameController, 'First name')),
                const SizedBox(width: 12),
                Expanded(child: _nameField(_lastNameController, 'Last name')),
              ],
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _emailController,
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.email],
              decoration: const InputDecoration(labelText: 'Email address'),
              validator: (value) {
                final email = value?.trim() ?? '';
                if (email.isEmpty || !email.contains('@')) {
                  return 'Enter a valid email address.';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _passwordController,
              obscureText: _obscurePassword,
              autofillHints: const [AutofillHints.newPassword],
              decoration: InputDecoration(
                labelText: 'Password',
                helperText:
                    '8+ characters with upper, lower, number, and symbol',
                suffixIcon: IconButton(
                  tooltip: _obscurePassword ? 'Show password' : 'Hide password',
                  onPressed: () =>
                      setState(() => _obscurePassword = !_obscurePassword),
                  icon: Icon(
                    _obscurePassword
                        ? Icons.visibility_outlined
                        : Icons.visibility_off_outlined,
                  ),
                ),
              ),
              validator: (value) {
                final password = value ?? '';
                if (password.length < 8 ||
                    !RegExp('[A-Z]').hasMatch(password) ||
                    !RegExp('[a-z]').hasMatch(password) ||
                    !RegExp('[0-9]').hasMatch(password) ||
                    !RegExp(r'[^A-Za-z0-9]').hasMatch(password)) {
                  return 'Use a stronger password matching the guidance.';
                }
                return null;
              },
            ),
            const SizedBox(height: 22),
            FilledButton(
              onPressed: auth.isSubmitting ? null : _submit,
              child: Text(
                auth.isSubmitting
                    ? 'Creating account…'
                    : 'Create Job Seeker account',
              ),
            ),
            const SizedBox(height: 12),
            TextButton(
              onPressed: auth.isSubmitting ? null : () => context.go('/login'),
              child: const Text('Already registered? Sign in'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _nameField(TextEditingController controller, String label) =>
      TextFormField(
        controller: controller,
        textCapitalization: TextCapitalization.words,
        decoration: InputDecoration(labelText: label),
        validator: (value) =>
            (value == null || value.trim().isEmpty) ? 'Required' : null,
      );
}
