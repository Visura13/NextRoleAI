import { Route, Routes } from 'react-router';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { AppLayout } from './layouts/AppLayout';
import { PortalLayout } from './layouts/PortalLayout';
import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { JobSeekerDashboardPage } from './pages/job-seeker/JobSeekerDashboardPage';
import { JobSeekerProfilePage } from './pages/job-seeker/JobSeekerProfilePage';
import { CvPage } from './pages/job-seeker/CvPage';
import { RecommendationsPage } from './pages/job-seeker/RecommendationsPage';
import { AgentWorkflowsPage } from './pages/job-seeker/AgentWorkflowsPage';
import { JobDetailsPage } from './pages/jobs/JobDetailsPage';
import { JobsPage } from './pages/jobs/JobsPage';
import { CompanyProfilePage } from './pages/recruiter/CompanyProfilePage';
import { JobEditorPage } from './pages/recruiter/JobEditorPage';
import { RecruiterDashboardPage } from './pages/recruiter/RecruiterDashboardPage';
import { RecruiterJobsPage } from './pages/recruiter/RecruiterJobsPage';
import { ComingSoonPage } from './pages/shared/ComingSoonPage';
import { DashboardRedirect } from './pages/shared/DashboardRedirect';
import { HomePage } from './pages/shared/HomePage';
import { NotFoundPage } from './pages/shared/NotFoundPage';

export function Application() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="jobs" element={<JobsPage />} />
        <Route path="jobs/:jobId" element={<JobDetailsPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />

        <Route element={<ProtectedRoute />}>
          <Route path="dashboard" element={<DashboardRedirect />} />
        </Route>

        <Route element={<ProtectedRoute role="JobSeeker" />}>
          <Route path="job-seeker" element={<PortalLayout role="JobSeeker" />}>
            <Route index element={<JobSeekerDashboardPage />} />
            <Route path="profile" element={<JobSeekerProfilePage />} />
            <Route path="cv" element={<CvPage />} />
            <Route path="recommendations" element={<RecommendationsPage />} />
            <Route path="agent-workflows" element={<AgentWorkflowsPage />} />
            <Route path="applications" element={<ComingSoonPage />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute role="Recruiter" />}>
          <Route path="recruiter" element={<PortalLayout role="Recruiter" />}>
            <Route index element={<RecruiterDashboardPage />} />
            <Route path="company" element={<CompanyProfilePage />} />
            <Route path="jobs" element={<RecruiterJobsPage />} />
            <Route path="jobs/new" element={<JobEditorPage />} />
            <Route path="jobs/:jobId/edit" element={<JobEditorPage />} />
            <Route path="applications" element={<ComingSoonPage />} />
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
