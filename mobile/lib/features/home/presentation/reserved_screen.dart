import 'package:flutter/material.dart';

import '../../../core/widgets/states.dart';

class ReservedScreen extends StatelessWidget {
  const ReservedScreen({
    required this.title,
    required this.message,
    required this.icon,
    super.key,
  });

  final String title;
  final String message;
  final IconData icon;

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: Text(title)),
    body: MessagePanel(icon: icon, title: title, message: message),
  );
}
