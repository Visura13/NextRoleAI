import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/core/api/api_client.dart';
import 'package:nextroleai_mobile/features/recommendations/state/recommendations_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('loads ranked recommendations from the shared API', () async {
    final repository = FakeRecommendationsDataSource()
      ..items = [sampleRecommendation()];
    final viewModel = RecommendationsViewModel(repository);

    await viewModel.load();

    expect(viewModel.items.single.score, 92.5);
    expect(viewModel.items.single.algorithmVersion, 'deterministic-v1');
    expect(viewModel.errorMessage, isNull);
  });

  test('identifies when a confirmed CV is required', () async {
    final repository = FakeRecommendationsDataSource()
      ..error = const ApiException('Confirm your CV first.', statusCode: 409);
    final viewModel = RecommendationsViewModel(repository);

    await viewModel.load();

    expect(viewModel.cvRequired, isTrue);
    expect(viewModel.errorMessage, 'Confirm your CV first.');
  });
}
