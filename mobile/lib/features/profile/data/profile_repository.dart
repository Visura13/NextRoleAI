import '../../../core/api/api_client.dart';
import '../models/job_seeker_profile.dart';

abstract interface class ProfileDataSource {
  Future<JobSeekerProfile?> getProfile();
  Future<JobSeekerProfile> saveProfile(JobSeekerProfile profile);
}

class ProfileRepository implements ProfileDataSource {
  const ProfileRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<JobSeekerProfile?> getProfile() async {
    try {
      return JobSeekerProfile.fromJson(
        await _apiClient.getJson('/api/profiles/job-seeker'),
      );
    } on ApiException catch (error) {
      if (error.statusCode == 404) return null;
      rethrow;
    }
  }

  @override
  Future<JobSeekerProfile> saveProfile(JobSeekerProfile profile) async =>
      JobSeekerProfile.fromJson(
        await _apiClient.putJson(
          '/api/profiles/job-seeker',
          body: profile.toJson(),
        ),
      );
}
