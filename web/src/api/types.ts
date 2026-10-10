export type UserRole = 'JobSeeker' | 'Recruiter';

export interface CurrentUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface AuthenticationResponse extends CurrentUser {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export interface AuthSession {
  user: CurrentUser;
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export type EmploymentType = 'FullTime' | 'PartTime' | 'Contract' | 'Internship';
export type WorkMode = 'OnSite' | 'Hybrid' | 'Remote';
export type JobStatus = 'Draft' | 'Published' | 'Closed';

export interface JobSkill {
  name: string;
  isRequired: boolean;
}

export interface Job {
  id: string;
  companyId: string;
  companyName: string;
  title: string;
  description: string;
  location: string;
  employmentType: EmploymentType;
  workMode: WorkMode;
  minimumYearsExperience: number;
  salaryMinimum: number | null;
  salaryMaximum: number | null;
  salaryCurrency: string | null;
  status: JobStatus;
  publishedAtUtc: string | null;
  closesAtUtc: string | null;
  skills: JobSkill[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface JobSeekerProfile {
  id: string;
  headline: string;
  summary: string;
  location: string;
  preferredJobTitle: string;
  preferredSalary: number | null;
  yearsOfExperience: number;
  skills: string[];
  updatedAtUtc: string;
}

export interface CompanyProfile {
  id: string;
  name: string;
  description: string;
  location: string;
  websiteUrl: string | null;
  updatedAtUtc: string;
}

export interface JobPayload {
  title: string;
  description: string;
  location: string;
  employmentType: EmploymentType;
  workMode: WorkMode;
  minimumYearsExperience: number;
  salaryMinimum: number | null;
  salaryMaximum: number | null;
  salaryCurrency: string | null;
  closesAtUtc: string | null;
  skills: JobSkill[];
}

export type CvProcessingStatus = 'NeedsReview' | 'Confirmed' | 'Failed';

export interface CvEducationItem {
  qualification: string;
  fieldOfStudy: string;
  institution: string;
  status: string;
  evidence: string;
  confidence: number;
}

export interface CvQualityAssessment {
  overallScore: number;
  completenessScore: number;
  clarityScore: number;
  skillsEvidenceScore: number;
  impactScore: number;
  atsReadabilityScore: number;
  strengths: string[];
  improvements: string[];
}

export interface CvProfile {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  sha256Checksum: string;
  status: CvProcessingStatus;
  candidateName: string;
  email: string;
  phone: string;
  location: string;
  currentJobTitle: string;
  professionalSummary: string;
  yearsExperience: number;
  skills: string[];
  education: CvEducationItem[];
  qualityAssessment: CvQualityAssessment | null;
  analysisMethod: string;
  analysisModel: string | null;
  analysisPromptVersion: string;
  analyzedAtUtc: string | null;
  failureReason: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface MatchBreakdown {
  skillsFit: number;
  roleFit: number;
  experienceFit: number;
  educationFit: number;
  locationFit: number;
  salaryFit: number;
  total: number;
}

export interface JobRecommendation {
  job: Job;
  score: number;
  breakdown: MatchBreakdown;
  matchedSkills: string[];
  missingRequiredSkills: string[];
  reasons: string[];
  algorithmVersion: string;
}

export interface RecommendationResponse {
  items: JobRecommendation[];
}

export type AgentWorkflowStatus = 'Running' | 'PendingApproval' | 'Completed' | 'Rejected' | 'RevisionRequested' | 'Failed';
export type AgentApprovalStatus = 'NotRequested' | 'Pending' | 'Approved' | 'Rejected' | 'RevisionRequested';
export type AgentStepStatus = 'Pending' | 'Running' | 'Completed' | 'Failed' | 'AwaitingApproval';
export type AgentDecisionType = 'Approve' | 'Reject' | 'RequestRevision';

export interface AgentToolCall {
  id: string;
  toolName: string;
  succeeded: boolean;
  error: string | null;
  startedAtUtc: string;
  completedAtUtc: string;
  durationMilliseconds: number;
}

export interface AgentWorkflowStep {
  id: string;
  sequence: number;
  agentName: string;
  responsibility: string;
  allowedTools: string[];
  status: AgentStepStatus;
  retryCount: number;
  error: string | null;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  durationMilliseconds: number | null;
  toolCalls: AgentToolCall[];
}

export interface AgentValidationResult {
  ruleName: string;
  passed: boolean;
  message: string;
  createdAtUtc: string;
}

export interface AgentShortlistItem {
  jobId: string;
  companyName: string;
  title: string;
  location: string;
  employmentType: EmploymentType;
  workMode: WorkMode;
  score: number;
  rank: number;
  reasonSummary: string;
  isApproved: boolean;
}

export interface AgentApprovalDecision {
  decision: AgentDecisionType;
  feedback: string;
  decidedAtUtc: string;
}

export interface AgentWorkflow {
  id: string;
  objective: string;
  status: AgentWorkflowStatus;
  approvalStatus: AgentApprovalStatus;
  currentAgent: string;
  revisionNumber: number;
  failureCode: string | null;
  failureMessage: string | null;
  finalSummary: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  completedAtUtc: string | null;
  steps: AgentWorkflowStep[];
  validationResults: AgentValidationResult[];
  shortlist: AgentShortlistItem[];
  approvalDecisions: AgentApprovalDecision[];
}

export interface AgentWorkflowList {
  items: AgentWorkflow[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type ApplicationStatus = 'Submitted' | 'InReview' | 'MoreInformationRequested' | 'Shortlisted' | 'Rejected' | 'Withdrawn';
export type ApplicationActorRole = 'JobSeeker' | 'Recruiter';
export type NotificationDeliveryStatus = 'Pending' | 'Sent' | 'Skipped' | 'Failed';

export interface ApplicationStatusEvent {
  id: string;
  previousStatus: ApplicationStatus | null;
  newStatus: ApplicationStatus;
  actorRole: ApplicationActorRole;
  note: string;
  createdAtUtc: string;
}

export interface NotificationDelivery {
  eventType: string;
  provider: string;
  status: NotificationDeliveryStatus;
  attemptCount: number;
  createdAtUtc: string;
  attemptedAtUtc: string | null;
}

export interface JobApplication {
  id: string;
  jobPostingId: string;
  jobTitle: string;
  companyName: string;
  jobLocation: string;
  jobSeekerUserId: string;
  jobSeekerName: string;
  jobSeekerEmail: string;
  coverNote: string;
  status: ApplicationStatus;
  sourceWorkflowRunId: string | null;
  statusHistory: ApplicationStatusEvent[];
  notificationDeliveries: NotificationDelivery[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ApplicationList {
  items: JobApplication[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
