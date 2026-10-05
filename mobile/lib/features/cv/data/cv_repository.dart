import '../../../core/api/api_client.dart';
import '../models/cv_profile.dart';
import 'cv_file_picker.dart';

abstract interface class CvDataSource {
  Future<CvProfile?> get();
  Future<CvProfile> upload(SelectedCv cv);
  Future<CvProfile> confirm(CvProfileUpdate update);
  Future<void> delete();
}

class CvRepository implements CvDataSource {
  const CvRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<CvProfile?> get() async {
    try {
      return CvProfile.fromJson(await _apiClient.getJson('/api/cv'));
    } on ApiException catch (error) {
      if (error.statusCode == 404) return null;
      rethrow;
    }
  }

  @override
  Future<CvProfile> upload(SelectedCv cv) async => CvProfile.fromJson(
    await _apiClient.postMultipart(
      '/api/cv',
      fieldName: 'file',
      fileName: cv.name,
      bytes: cv.bytes,
    ),
  );

  @override
  Future<CvProfile> confirm(CvProfileUpdate update) async => CvProfile.fromJson(
    await _apiClient.putJson('/api/cv/profile', body: update.toJson()),
  );

  @override
  Future<void> delete() => _apiClient.deleteEmpty('/api/cv');
}
