import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/core/api/api_client.dart';
import 'package:nextroleai_mobile/features/applications/state/applications_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('loads the shared application timeline', () async {
    final repository = FakeApplicationsDataSource()
      ..items = [sampleApplication(status: 'InReview')];
    final viewModel = ApplicationsViewModel(repository);

    await viewModel.load();

    expect(viewModel.items.single.status, 'InReview');
    expect(viewModel.items.single.statusHistory, hasLength(1));
    expect(viewModel.errorMessage, isNull);
  });

  test('submits and updates the local application list', () async {
    final repository = FakeApplicationsDataSource();
    final viewModel = ApplicationsViewModel(repository);

    final saved = await viewModel.submit(
      jobPostingId: 'job-1',
      coverNote: 'I have relevant Flutter delivery experience.',
    );

    expect(saved, isTrue);
    expect(viewModel.items.single.status, 'Submitted');
    expect(repository.lastNote, contains('Flutter'));
    expect(viewModel.successMessage, contains('submitted'));
  });

  test('surfaces API errors without losing existing items', () async {
    final repository = FakeApplicationsDataSource()
      ..items = [sampleApplication()];
    final viewModel = ApplicationsViewModel(repository);
    await viewModel.load();
    repository.error = const ApiException(
      'Invalid transition.',
      statusCode: 409,
    );

    final saved = await viewModel.withdraw(
      'application-1',
      'No longer interested.',
    );

    expect(saved, isFalse);
    expect(viewModel.items, hasLength(1));
    expect(viewModel.errorMessage, 'Invalid transition.');
  });
}
