import { Link, useParams } from 'react-router';
import useSWR from 'swr';
import { publicFetcher } from '../../api/client';
import type { Job } from '../../api/types';
import { formatSalary, readableLabel } from '../../components/jobFormatting';
import { ErrorState, LoadingState, Notice } from '../../components/States';
import { useAuth } from '../../auth/useAuth';

export function JobDetailsPage() {
  const { jobId } = useParams();
  const { session } = useAuth();
  const { data: job, error, isLoading } = useSWR<Job>(jobId ? `/api/jobs/${jobId}` : null, publicFetcher);

  if (isLoading) return <div className="container narrow-page"><LoadingState label="Loading job details" /></div>;
  if (error || !job) return <div className="container narrow-page"><ErrorState message="This role is unavailable or is no longer published." action={<Link className="button button--secondary" to="/jobs">Back to jobs</Link>} /></div>;

  return (
    <div className="container detail-layout">
      <article className="job-detail">
        <Link className="back-link" to="/jobs">← All jobs</Link>
        <p className="eyebrow">{job.companyName}</p>
        <h1>{job.title}</h1>
        <div className="job-meta"><span>{job.location}</span><span>{readableLabel(job.workMode)}</span><span>{readableLabel(job.employmentType)}</span></div>
        <section><h2>About the role</h2><p className="preserve-lines">{job.description}</p></section>
        <section><h2>Skills we are looking for</h2><div className="tag-row">{job.skills.map((skill) => <span className={skill.isRequired ? 'tag tag--required' : 'tag'} key={skill.name}>{skill.name}{skill.isRequired ? ' · required' : ''}</span>)}</div></section>
      </article>
      <aside className="detail-card">
        <p className="eyebrow">Role details</p>
        <dl><div><dt>Salary</dt><dd>{formatSalary(job)}</dd></div><div><dt>Experience</dt><dd>{job.minimumYearsExperience}+ years</dd></div><div><dt>Closes</dt><dd>{job.closesAtUtc ? new Intl.DateTimeFormat('en', { dateStyle: 'medium' }).format(new Date(job.closesAtUtc)) : 'Open until filled'}</dd></div></dl>
        {session?.user.role === 'JobSeeker' ? <Notice kind="info">Submitting is always an explicit action. You can track every recruiter update on web or mobile.</Notice> : <Notice kind="info">Log in as a Job Seeker to submit and track an application.</Notice>}
        <Link className="button button--full" to={session?.user.role === 'JobSeeker' ? `/job-seeker/applications?jobId=${job.id}` : '/register'}>{session?.user.role === 'JobSeeker' ? 'Apply for this role' : 'Create job seeker account'}</Link>
      </aside>
    </div>
  );
}
