import '../../../core/api/api_client.dart';
import '../models/recommendation.dart';

abstract interface class RecommendationsDataSource {
  Future<List<JobRecommendation>> getRecommendations();
}

class RecommendationsRepository implements RecommendationsDataSource {
  const RecommendationsRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<JobRecommendation>> getRecommendations() async {
    final response = await _apiClient.getJson('/api/recommendations');
    return (response['items'] as List<dynamic>)
        .map((item) => JobRecommendation.fromJson(item as Map<String, dynamic>))
        .toList(growable: false);
  }
}
