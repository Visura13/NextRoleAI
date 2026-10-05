import 'dart:typed_data';

import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/core/config/app_config.dart';
import 'package:nextroleai_mobile/features/cv/data/cv_file_picker.dart';
import 'package:nextroleai_mobile/features/cv/state/cv_selection_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('accepts a supported CV within the size limit', () async {
    final picker = FakeCvFilePicker()
      ..result = SelectedCv(
        name: 'alex-cv.pdf',
        sizeInBytes: 2048,
        bytes: Uint8List.fromList([1, 2, 3]),
      );
    final repository = FakeCvDataSource();
    final viewModel = CvSelectionViewModel(picker, repository);

    await viewModel.selectCv();

    expect(viewModel.selectedCv?.name, 'alex-cv.pdf');
    expect(viewModel.errorMessage, isNull);
  });

  test('rejects a CV over five megabytes', () async {
    final picker = FakeCvFilePicker()
      ..result = SelectedCv(
        name: 'large-cv.pdf',
        sizeInBytes: AppConfig.maximumCvBytes + 1,
        bytes: Uint8List(0),
      );
    final viewModel = CvSelectionViewModel(picker, FakeCvDataSource());

    await viewModel.selectCv();

    expect(viewModel.selectedCv, isNull);
    expect(viewModel.errorMessage, contains('smaller than 5 MB'));
  });

  test('uploads the selected CV and exposes extracted review data', () async {
    final picker = FakeCvFilePicker()
      ..result = SelectedCv(
        name: 'alex-cv.pdf',
        sizeInBytes: 2048,
        bytes: Uint8List.fromList([1, 2, 3]),
      );
    final repository = FakeCvDataSource();
    final viewModel = CvSelectionViewModel(picker, repository);

    await viewModel.selectCv();
    final succeeded = await viewModel.upload();

    expect(succeeded, isTrue);
    expect(viewModel.selectedCv, isNull);
    expect(viewModel.profile?.candidateName, 'Alex Morgan');
    expect(viewModel.profile?.isConfirmed, isFalse);
  });
}
