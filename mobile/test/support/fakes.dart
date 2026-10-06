import 'package:nextroleai_mobile/core/storage/session_storage.dart';
import 'package:nextroleai_mobile/features/agent_workflows/data/agent_workflows_repository.dart';
import 'package:nextroleai_mobile/features/agent_workflows/models/agent_workflow.dart';
import 'package:nextroleai_mobile/features/applications/data/applications_repository.dart';
import 'package:nextroleai_mobile/features/applications/models/job_application.dart';
import 'package:nextroleai_mobile/features/auth/data/auth_repository.dart';
import 'package:nextroleai_mobile/features/auth/models/auth_models.dart';
import 'package:nextroleai_mobile/features/cv/data/cv_file_picker.dart';
import 'package:nextroleai_mobile/features/cv/data/cv_repository.dart';
import 'package:nextroleai_mobile/features/cv/models/cv_profile.dart';
import 'package:nextroleai_mobile/features/jobs/data/jobs_repository.dart';
import 'package:nextroleai_mobile/features/jobs/models/job_models.dart';
import 'package:nextroleai_mobile/features/profile/data/profile_repository.dart';
import 'package:nextroleai_mobile/features/profile/models/job_seeker_profile.dart';
import 'package:nextroleai_mobile/features/recommendations/data/recommendations_repository.dart';
import 'package:nextroleai_mobile/features/recommendations/models/recommendation.dart';

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

CvProfile sampleCvProfile({String status = 'NeedsReview'}) => CvProfile(
  id: 'cv-1',
  originalFileName: 'alex-cv.pdf',
  contentType: 'application/pdf',
  sizeBytes: 2048,
  sha256Checksum:
      'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
  status: status,
  candidateName: 'Alex Morgan',
  email: 'alex@example.com',
  phone: '',
  location: 'Colombo',
  currentJobTitle: 'Flutter Engineer',
  professionalSummary: 'Builds reliable mobile applications.',
  yearsExperience: 3,
  skills: const ['Dart', 'Flutter'],
  updatedAtUtc: DateTime.utc(2030),
);

class FakeCvDataSource implements CvDataSource {
  CvProfile? profile;
  Object? error;
  bool deleted = false;

  @override
  Future<CvProfile> confirm(CvProfileUpdate update) async {
    if (error != null) throw error!;
    return profile = sampleCvProfile(status: 'Confirmed');
  }

  @override
  Future<void> delete() async {
    if (error != null) throw error!;
    deleted = true;
    profile = null;
  }

  @override
  Future<CvProfile?> get() async {
    if (error != null) throw error!;
    return profile;
  }

  @override
  Future<CvProfile> upload(SelectedCv cv) async {
    if (error != null) throw error!;
    return profile = sampleCvProfile();
  }
}

JobRecommendation sampleRecommendation() => JobRecommendation(
  job: sampleJob(),
  score: 92.5,
  breakdown: const MatchBreakdown(
    requiredSkills: 45,
    preferredSkills: 12.5,
    title: 15,
    experience: 10,
    location: 10,
  ),
  matchedSkills: const ['Flutter'],
  missingRequiredSkills: const [],
  reasons: const ['Matches 1 listed skill.'],
  algorithmVersion: 'deterministic-v1',
);

class FakeRecommendationsDataSource implements RecommendationsDataSource {
  List<JobRecommendation> items = [];
  Object? error;

  @override
  Future<List<JobRecommendation>> getRecommendations() async {
    if (error != null) throw error!;
    return items;
  }
}

AgentWorkflow sampleAgentWorkflow({
  String status = 'PendingApproval',
  bool approved = false,
}) => AgentWorkflow(
  id: 'workflow-1',
  objective: 'Find the best 3 software engineering jobs for my profile.',
  status: status,
  approvalStatus: approved ? 'Approved' : 'Pending',
  currentAgent: approved ? 'Job Discovery Agent' : 'Human Approval Gate',
  failureCode: null,
  failureMessage: null,
  finalSummary: approved ? 'Published 1 approved recommendation.' : null,
  createdAtUtc: DateTime.utc(2030),
  steps: const [],
  validationResults: const [
    AgentValidation(
      ruleName: 'ToolAllowList',
      passed: true,
      message: 'Every tool was allowed.',
    ),
  ],
  shortlist: [
    AgentShortlistItem(
      jobId: 'job-1',
      companyName: 'Northstar Labs',
      title: 'Flutter Engineer',
      location: 'Colombo',
      workMode: 'Hybrid',
      score: 92.5,
      rank: 1,
      reasonSummary: 'Skills and title match.',
      isApproved: approved,
    ),
  ],
);

class FakeAgentWorkflowsDataSource implements AgentWorkflowsDataSource {
  List<AgentWorkflow> items = [];
  Object? error;
  String? lastDecision;

  @override
  Future<AgentWorkflow> decide({
    required String workflowId,
    required String decision,
    required String feedback,
    String? revisedObjective,
  }) async {
    if (error != null) throw error!;
    lastDecision = decision;
    return sampleAgentWorkflow(status: 'Completed', approved: true);
  }

  @override
  Future<List<AgentWorkflow>> list() async {
    if (error != null) throw error!;
    return items;
  }

  @override
  Future<AgentWorkflow> start(String objective) async {
    if (error != null) throw error!;
    return sampleAgentWorkflow();
  }
}

JobApplication sampleApplication({
  String id = 'application-1',
  String status = 'Submitted',
}) => JobApplication(
  id: id,
  jobPostingId: 'job-1',
  jobTitle: 'Flutter Engineer',
  companyName: 'Northstar Labs',
  jobLocation: 'Colombo',
  coverNote: 'I have relevant Flutter delivery experience.',
  status: status,
  sourceWorkflowRunId: null,
  statusHistory: [
    ApplicationStatusEvent(
      id: 'event-1',
      previousStatus: null,
      newStatus: status,
      actorRole: 'JobSeeker',
      note: 'Application submitted.',
      createdAtUtc: DateTime.utc(2030),
    ),
  ],
  notificationDeliveries: const [
    NotificationDelivery(
      eventType: 'ApplicationSubmitted',
      provider: 'Resend',
      status: 'Skipped',
      attemptCount: 1,
    ),
  ],
  createdAtUtc: DateTime.utc(2030),
  updatedAtUtc: DateTime.utc(2030),
);

class FakeApplicationsDataSource implements ApplicationsDataSource {
  List<JobApplication> items = [];
  Object? error;
  String? lastNote;

  @override
  Future<List<JobApplication>> list() async {
    if (error != null) throw error!;
    return items;
  }

  @override
  Future<JobApplication> respond(String applicationId, String note) async {
    if (error != null) throw error!;
    lastNote = note;
    return sampleApplication(id: applicationId);
  }

  @override
  Future<JobApplication> submit({
    required String jobPostingId,
    required String coverNote,
    String? sourceWorkflowRunId,
  }) async {
    if (error != null) throw error!;
    lastNote = coverNote;
    return sampleApplication();
  }

  @override
  Future<JobApplication> withdraw(String applicationId, String note) async {
    if (error != null) throw error!;
    lastNote = note;
    return sampleApplication(id: applicationId, status: 'Withdrawn');
  }
}
