import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

class NextRoleBrand extends StatelessWidget {
  const NextRoleBrand({super.key, this.light = false});

  final bool light;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Container(
        width: 38,
        height: 38,
        alignment: Alignment.center,
        decoration: const BoxDecoration(
          color: AppTheme.lime,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            topRight: Radius.circular(12),
            bottomRight: Radius.circular(12),
            bottomLeft: Radius.circular(3),
          ),
        ),
        child: const Text(
          'N',
          style: TextStyle(
            color: AppTheme.forestDeep,
            fontSize: 18,
            fontWeight: FontWeight.w900,
          ),
        ),
      ),
      const SizedBox(width: 10),
      Text(
        'NextRoleAI',
        style: TextStyle(
          color: light ? Colors.white : AppTheme.forest,
          fontSize: 20,
          fontWeight: FontWeight.w900,
          letterSpacing: -0.7,
        ),
      ),
    ],
  );
}
