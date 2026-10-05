import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/core/config/app_config.dart';
import 'package:nextroleai_mobile/features/cv/data/cv_file_picker.dart';
import 'package:nextroleai_mobile/features/cv/state/cv_selection_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('accepts a supported CV within the size limit', () async {
    final picker = FakeCvFilePicker()
      ..result = const SelectedCv(name: 'alex-cv.pdf', sizeInBytes: 2048);
    final viewModel = CvSelectionViewModel(picker);

    await viewModel.selectCv();

    expect(viewModel.selectedCv?.name, 'alex-cv.pdf');
    expect(viewModel.errorMessage, isNull);
  });

  test('rejects a CV over five megabytes', () async {
    final picker = FakeCvFilePicker()
      ..result = const SelectedCv(
        name: 'large-cv.pdf',
        sizeInBytes: AppConfig.maximumCvBytes + 1,
      );
    final viewModel = CvSelectionViewModel(picker);

    await viewModel.selectCv();

    expect(viewModel.selectedCv, isNull);
    expect(viewModel.errorMessage, contains('smaller than 5 MB'));
  });
}
