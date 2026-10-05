import 'package:flutter_test/flutter_test.dart';
import 'package:nextroleai_mobile/features/profile/models/job_seeker_profile.dart';
import 'package:nextroleai_mobile/features/profile/state/profile_view_model.dart';

import '../support/fakes.dart';

void main() {
  test('saves the shared Job Seeker profile', () async {
    final repository = FakeProfileDataSource();
    final viewModel = ProfileViewModel(repository);
    const profile = JobSeekerProfile(
      headline: 'Mobile engineer',
      summary: 'Builds reliable products.',
      location: 'Colombo',
      preferredJobTitle: 'Flutter Engineer',
      yearsOfExperience: 3,
      skills: ['Flutter', 'Dart'],
    );

    final succeeded = await viewModel.save(profile);

    expect(succeeded, isTrue);
    expect(repository.profile?.skills, ['Flutter', 'Dart']);
    expect(viewModel.successMessage, contains('saved'));
  });
}
