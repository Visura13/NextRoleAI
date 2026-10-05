import { Link } from 'react-router';
import useSWR from 'swr';
import { swrFetcher } from '../../api/client';
import type { Job, PagedResult } from '../../api/types';
import { JobCard } from '../../components/JobCard';
import { EmptyState, ErrorState, LoadingState } from '../../components/States';

export function RecruiterJobsPage() {
  const { data, error, isLoading } = useSWR<PagedResult<Job>>('/api/recruiter/jobs?page=1&pageSize=100', swrFetcher);

  return (
    <div className="page-stack">
      <header className="portal-header portal-header--action"><div><p className="eyebrow">Job management</p><h1>Your opportunities.</h1><p>Create as a draft, review the details, then publish when the role is ready.</p></div><Link className="button" to="/recruiter/jobs/new">Create job</Link></header>
      {isLoading && <LoadingState label="Loading your jobs" />}
      {error && <ErrorState message="Your job postings could not be loaded." />}
      {data?.items.length === 0 && <EmptyState title="No job postings yet" message="Create your first structured vacancy to start building the public catalog." action={<Link className="button" to="/recruiter/jobs/new">Create first job</Link>} />}
      {data && data.items.length > 0 && <div className="job-grid job-grid--portal">{data.items.map((job) => <JobCard job={job} key={job.id} manage />)}</div>}
    </div>
  );
}
