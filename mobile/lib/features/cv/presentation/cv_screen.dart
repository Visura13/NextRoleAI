import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../core/widgets/states.dart';
import '../state/cv_selection_view_model.dart';

class CvScreen extends StatelessWidget {
  const CvScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final viewModel = context.watch<CvSelectionViewModel>();
    final cv = viewModel.selectedCv;
    return Scaffold(
      appBar: AppBar(title: const Text('Your CV')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 36),
        children: [
          Text(
            'Choose your career document.',
            style: Theme.of(context).textTheme.headlineSmall
                ?.copyWith(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 8),
          Text(
            'PDF, DOC, and DOCX files up to 5 MB are accepted.',
            style: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 20),
          if (viewModel.errorMessage != null) ...[
            FeedbackBanner(message: viewModel.errorMessage!, isError: true),
            const SizedBox(height: 16),
          ],
          Card(
            child: Padding(
              padding: const EdgeInsets.all(22),
              child: cv == null
                  ? Column(
                      children: [
                        const Icon(Icons.upload_file_outlined, size: 52),
                        const SizedBox(height: 14),
                        Text(
                          'No CV selected',
                          style: Theme.of(context).textTheme.titleMedium
                              ?.copyWith(fontWeight: FontWeight.w800),
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Use the native document picker to choose a file from this device.',
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 18),
                        FilledButton.icon(
                          key: const Key('choose-cv'),
                          onPressed: viewModel.isSelecting
                              ? null
                              : viewModel.selectCv,
                          icon: const Icon(Icons.folder_open_outlined),
                          label: Text(
                            viewModel.isSelecting
                                ? 'Opening picker…'
                                : 'Choose CV',
                          ),
                        ),
                      ],
                    )
                  : Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const CircleAvatar(
                          child: Icon(Icons.description_outlined),
                        ),
                        const SizedBox(width: 14),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                cv.name,
                                style: const TextStyle(
                                  fontWeight: FontWeight.w800,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(_formatSize(cv.sizeInBytes)),
                              const SizedBox(height: 12),
                              Wrap(
                                spacing: 8,
                                children: [
                                  OutlinedButton(
                                    onPressed: viewModel.selectCv,
                                    child: const Text('Replace'),
                                  ),
                                  TextButton(
                                    onPressed: viewModel.removeSelection,
                                    child: const Text('Remove'),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
            ),
          ),
          const SizedBox(height: 20),
          const FeedbackBanner(
            message: 'The file stays on your device in this increment. Secure upload, extraction, and explainable ranking are delivered in Part 6.',
          ),
        ],
      ),
    );
  }

  String _formatSize(int bytes) {
    final kilobytes = bytes / 1024;
    if (kilobytes < 1024) return '${kilobytes.toStringAsFixed(1)} KB';
    return '${(kilobytes / 1024).toStringAsFixed(1)} MB';
  }
}
