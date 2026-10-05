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
