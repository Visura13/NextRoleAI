import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../data/recommendations_repository.dart';
import '../models/recommendation.dart';

class RecommendationsViewModel extends ChangeNotifier {
  RecommendationsViewModel(this._repository);

  final RecommendationsDataSource _repository;
  List<JobRecommendation> _items = const [];
  bool _isLoading = false;
  String? _errorMessage;
  bool _cvRequired = false;

  List<JobRecommendation> get items => _items;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  bool get cvRequired => _cvRequired;

  Future<void> load() async {
    if (_isLoading) return;
    _isLoading = true;
    _errorMessage = null;
    _cvRequired = false;
    notifyListeners();
    try {
      _items = await _repository.getRecommendations();
    } on ApiException catch (error) {
      _cvRequired = error.statusCode == 409;
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'Recommendations could not be calculated.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
