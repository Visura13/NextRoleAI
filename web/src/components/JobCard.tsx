import { Link } from 'react-router';
import type { Job } from '../api/types';
import { formatSalary, readableLabel } from './jobFormatting';

export function JobCard({ job, manage = false }: { job: Job; manage?: boolean }) {
  return (
    <article className="job-card">
      <div className="job-card__topline">
        <span className="company-mark" aria-hidden="true">{job.companyName.slice(0, 1).toUpperCase()}</span>
        <div>
          <h3>{job.title}</h3>
          <p>{job.companyName}</p>
        </div>
        {manage && <span className={`status status--${job.status.toLowerCase()}`}>{job.status}</span>}
      </div>
      <div className="job-meta">
        <span>{job.location}</span>
        <span>{readableLabel(job.workMode)}</span>
        <span>{readableLabel(job.employmentType)}</span>
      </div>
      <p className="job-card__description">{job.description}</p>
      <div className="tag-row">
        {job.skills.slice(0, 4).map((skill) => (
          <span className="tag" key={skill.name}>{skill.name}</span>
        ))}
      </div>
      <div className="job-card__footer">
        <strong>{formatSalary(job)}</strong>
        <Link className="text-link" to={manage ? `/recruiter/jobs/${job.id}/edit` : `/jobs/${job.id}`}>
          {manage ? 'Manage job' : 'View role'} <span aria-hidden="true">→</span>
        </Link>
      </div>
    </article>
  );
}
