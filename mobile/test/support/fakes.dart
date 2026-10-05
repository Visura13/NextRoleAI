import 'package:nextroleai_mobile/core/storage/session_storage.dart';
import 'package:nextroleai_mobile/features/auth/data/auth_repository.dart';
import 'package:nextroleai_mobile/features/auth/models/auth_models.dart';
import 'package:nextroleai_mobile/features/cv/data/cv_file_picker.dart';
import 'package:nextroleai_mobile/features/jobs/data/jobs_repository.dart';
import 'package:nextroleai_mobile/features/jobs/models/job_models.dart';
import 'package:nextroleai_mobile/features/profile/data/profile_repository.dart';
import 'package:nextroleai_mobile/features/profile/models/job_seeker_profile.dart';

AuthSession sampleSession({String accessToken = 'access-token'}) => AuthSession(
  user: const CurrentUser(
    userId: 'user-1',
    email: 'alex@example.com',
    firstName: 'Alex',
    lastName: 'Morgan',
    role: 'JobSeeker',
  ),
  accessToken: accessToken,
  accessTokenExpiresAtUtc: DateTime.utc(2030),
  refreshToken: 'refresh-token',
  refreshTokenExpiresAtUtc: DateTime.utc(2031),
);

class MemorySessionStorage implements SessionStorage {
  MemorySessionStorage([this.session]);

  AuthSession? session;
  int clearCount = 0;

  @override
  Future<void> clear() async {
    clearCount++;
    session = null;
  }

  @override
  Future<AuthSession?> read() async => session;

  @override
  Future<void> write(AuthSession session) async => this.session = session;
}

class FakeAuthDataSource implements AuthDataSource {
  AuthSession? restored;
  AuthSession loginResult = sampleSession();
  Object? loginError;
  bool didLogout = false;

  @override
  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    if (loginError != null) throw loginError!;
    return loginResult;
  }

  @override
  Future<void> logout() async => didLogout = true;

  @override
  Future<AuthSession> register(RegistrationInput input) async => loginResult;

  @override
  Future<AuthSession?> restoreSession() async => restored;
}

Job sampleJob({String id = 'job-1'}) => Job(
  id: id,
  companyName: 'Northstar Labs',
  title: 'Flutter Engineer',
  description: 'Build useful cross-platform experiences.',
  location: 'Colombo',
  employmentType: 'FullTime',
  workMode: 'Hybrid',
  minimumYearsExperience: 2,
  salaryMinimum: 200000,
  salaryMaximum: 300000,
  salaryCurrency: 'LKR',
  closesAtUtc: DateTime.utc(2030),
  skills: const [JobSkill(name: 'Flutter', isRequired: true)],
);

class FakeJobsDataSource implements JobsDataSource {
  final Map<int, PagedJobs> pages = {};
  Object? error;

  @override
  Future<Job> getById(String id) async => sampleJob(id: id);

  @override
  Future<PagedJobs> search(JobFilters filters, {required int page}) async {
    if (error != null) throw error!;
    return pages[page] ??
        const PagedJobs(
          items: [],
          page: 1,
          pageSize: 10,
          totalCount: 0,
          totalPages: 0,
        );
  }
}

class FakeProfileDataSource implements ProfileDataSource {
  JobSeekerProfile? profile;
  Object? error;

  @override
  Future<JobSeekerProfile?> getProfile() async {
    if (error != null) throw error!;
    return profile;
  }

  @override
  Future<JobSeekerProfile> saveProfile(JobSeekerProfile profile) async {
    if (error != null) throw error!;
    return this.profile = profile;
  }
}

class FakeCvFilePicker implements CvFilePicker {
  SelectedCv? result;
  Object? error;

  @override
  Future<SelectedCv?> pick() async {
    if (error != null) throw error!;
    return result;
  }
}
