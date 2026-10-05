import 'package:flutter/material.dart';

import '../../../core/widgets/brand.dart';

class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) => const Scaffold(
    body: Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          NextRoleBrand(),
          SizedBox(height: 28),
          CircularProgressIndicator(),
        ],
      ),
    ),
  );
}
