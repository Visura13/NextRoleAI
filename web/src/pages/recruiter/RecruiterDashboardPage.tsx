import { Link } from 'react-router';
import useSWR from 'swr';
import { ApiError, swrFetcher } from '../../api/client';
import type { CompanyProfile, Job, PagedResult } from '../../api/types';
import { useAuth } from '../../auth/useAuth';

export function RecruiterDashboardPage() {
  const { session } = useAuth();
  const { data: company, error: companyError } = useSWR<CompanyProfile>('/api/profiles/company', swrFetcher);
  const { data: jobs } = useSWR<PagedResult<Job>>('/api/recruiter/jobs?page=1&pageSize=100', swrFetcher);
  const published = jobs?.items.filter((job) => job.status === 'Published').length ?? 0;
  const drafts = jobs?.items.filter((job) => job.status === 'Draft').length ?? 0;
  const companyMissing = companyError instanceof ApiError && companyError.status === 404;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Recruiter workspace</p><h1>Welcome, {session?.user.firstName}.</h1><p>Keep your company credible and every vacancy current from one focused workspace.</p></header>
      <div className="metric-grid">
        <article className="metric-card metric-card--accent"><span>Published jobs</span><strong>{published}</strong><p>Visible in the public opportunity catalog.</p></article>
        <article className="metric-card"><span>Draft jobs</span><strong>{drafts}</strong><p>Ready for review before publishing.</p></article>
        <article className="metric-card"><span>Applications</span><strong>—</strong><p>Review and decision workflows arrive in Part 8.</p></article>
      </div>
      {(companyMissing || company) && <section className="action-panel"><div><p className="eyebrow">{companyMissing ? 'First step' : company?.name}</p><h2>{companyMissing ? 'Create your company profile.' : 'Your recruiter foundation is ready.'}</h2><p>{companyMissing ? 'A company profile is required before a job can be created.' : 'Create a structured vacancy or keep your company details up to date.'}</p></div><div className="button-row"><Link className="button button--secondary" to="/recruiter/company">{companyMissing ? 'Create company' : 'Edit company'}</Link>{!companyMissing && <Link className="button" to="/recruiter/jobs/new">Create job</Link>}</div></section>}
    </div>
  );
}
