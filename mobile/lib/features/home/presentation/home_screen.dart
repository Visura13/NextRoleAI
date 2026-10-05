import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/brand.dart';
import '../../auth/state/auth_view_model.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final user = context.select<AuthViewModel, String?>(
      (auth) => auth.user?.firstName,
    );
    return Scaffold(
      appBar: AppBar(
        title: const NextRoleBrand(),
        actions: [
          IconButton(
            tooltip: 'Log out',
            onPressed: () => context.read<AuthViewModel>().logout(),
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 32),
          children: [
            Text(
              'GOOD TO SEE YOU',
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                color: AppTheme.orange,
                fontWeight: FontWeight.w800,
                letterSpacing: 1.3,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Ready for your next role, ${user ?? 'Job Seeker'}?',
              style: Theme.of(context).textTheme.headlineMedium
                  ?.copyWith(fontWeight: FontWeight.w900, letterSpacing: -1),
            ),
            const SizedBox(height: 24),
            Container(
              padding: const EdgeInsets.all(22),
              decoration: BoxDecoration(
                color: AppTheme.forestDeep,
                borderRadius: BorderRadius.circular(22),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(
                    Icons.description_outlined,
                    color: AppTheme.lime,
                    size: 34,
                  ),
                  const SizedBox(height: 22),
                  Text(
                    'Bring your CV into the picture.',
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(
                      color: Colors.white,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Choose a document securely from your device. Processing and explainable ranking arrive in Part 6.',
                    style: TextStyle(color: Color(0xFFC4D2CC), height: 1.5),
                  ),
                  const SizedBox(height: 18),
                  FilledButton.tonal(
                    onPressed: () => context.push('/cv'),
                    style: FilledButton.styleFrom(
                      backgroundColor: AppTheme.lime,
                      foregroundColor: AppTheme.forestDeep,
                    ),
                    child: const Text('Choose my CV'),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 28),
            Text(
              'Your next steps',
              style: Theme.of(context).textTheme.titleLarge
                  ?.copyWith(fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 12),
            _ActionCard(
              icon: Icons.person_outline,
              title: 'Complete your profile',
              subtitle: 'Capture your goals, experience, and skills.',
              onTap: () => context.go('/profile'),
            ),
            _ActionCard(
              icon: Icons.search,
              title: 'Explore open roles',
              subtitle:
                  'Search published opportunities and filter the catalog.',
              onTap: () => context.go('/jobs'),
            ),
            _ActionCard(
              icon: Icons.auto_awesome_outlined,
              title: 'Ranked recommendations',
              subtitle:
                  'Reserved for the deterministic matching workflow in Part 6.',
              onTap: () => context.push('/recommendations'),
            ),
            _ActionCard(
              icon: Icons.track_changes_outlined,
              title: 'Application status',
              subtitle: 'Reserved for the cross-platform workflow in Part 8.',
              onTap: () => context.push('/applications'),
            ),
          ],
        ),
      ),
    );
  }
}

class _ActionCard extends StatelessWidget {
  const _ActionCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Card(
    margin: const EdgeInsets.only(bottom: 12),
    child: ListTile(
      onTap: onTap,
      contentPadding: const EdgeInsets.symmetric(horizontal: 18, vertical: 9),
      leading: CircleAvatar(
        backgroundColor: const Color(0xFFEDF9CE),
        foregroundColor: AppTheme.forest,
        child: Icon(icon),
      ),
      title: Text(title, style: const TextStyle(fontWeight: FontWeight.w700)),
      subtitle: Text(subtitle),
      trailing: const Icon(Icons.arrow_forward_ios, size: 16),
    ),
  );
}
