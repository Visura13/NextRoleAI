import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import useSWR, { mutate as mutateCache } from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { EmploymentType, Job, JobPayload, JobStatus, WorkMode } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';

interface JobForm {
  title: string;
  description: string;
  location: string;
  employmentType: EmploymentType;
  workMode: WorkMode;
  minimumYearsExperience: number;
  salaryMinimum: string;
  salaryMaximum: string;
  salaryCurrency: string;
  closesAtUtc: string;
  requiredSkills: string;
  preferredSkills: string;
}

const emptyJob: JobForm = {
  title: '', description: '', location: '', employmentType: 'FullTime', workMode: 'Hybrid', minimumYearsExperience: 0,
  salaryMinimum: '', salaryMaximum: '', salaryCurrency: '', closesAtUtc: '', requiredSkills: '', preferredSkills: '',
};

function dateTimeLocal(value: string | null) {
  if (!value) return '';
  const date = new Date(value);
  const offset = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function toForm(job: Job): JobForm {
  return {
    title: job.title, description: job.description, location: job.location, employmentType: job.employmentType,
    workMode: job.workMode, minimumYearsExperience: job.minimumYearsExperience,
    salaryMinimum: job.salaryMinimum?.toString() ?? '', salaryMaximum: job.salaryMaximum?.toString() ?? '',
    salaryCurrency: job.salaryCurrency ?? '', closesAtUtc: dateTimeLocal(job.closesAtUtc),
    requiredSkills: job.skills.filter((skill) => skill.isRequired).map((skill) => skill.name).join(', '),
    preferredSkills: job.skills.filter((skill) => !skill.isRequired).map((skill) => skill.name).join(', '),
  };
}

function skillList(value: string, isRequired: boolean) {
  return value.split(',').map((name) => name.trim()).filter(Boolean).map((name) => ({ name, isRequired }));
}

export function JobEditorPage() {
  const { jobId } = useParams();
  const editing = Boolean(jobId);
  const { data: job, error, isLoading, mutate } = useSWR<Job>(jobId ? `/api/recruiter/jobs/${jobId}` : null, swrFetcher);
  const [editedForm, setForm] = useState<JobForm | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const form = editedForm ?? (job ? toForm(job) : emptyJob);

  function payload(): JobPayload {
    return {
      title: form.title.trim(), description: form.description.trim(), location: form.location.trim(),
      employmentType: form.employmentType, workMode: form.workMode, minimumYearsExperience: form.minimumYearsExperience,
      salaryMinimum: form.salaryMinimum === '' ? null : Number(form.salaryMinimum),
      salaryMaximum: form.salaryMaximum === '' ? null : Number(form.salaryMaximum),
      salaryCurrency: form.salaryCurrency.trim().toUpperCase() || null,
      closesAtUtc: form.closesAtUtc ? new Date(form.closesAtUtc).toISOString() : null,
      skills: [...skillList(form.requiredSkills, true), ...skillList(form.preferredSkills, false)],
    };
  }

  async function save(event: FormEvent) {
    event.preventDefault();
    const request = payload();
    setFeedback(null);
    if (!request.title || !request.description || !request.location || request.skills.length === 0) {
      setFeedback({ kind: 'error', message: 'Title, description, location, and at least one skill are required.' });
      return;
    }
    if (request.salaryMinimum !== null && request.salaryMaximum !== null && request.salaryMinimum > request.salaryMaximum) {
      setFeedback({ kind: 'error', message: 'Maximum salary must be at least the minimum salary.' });
      return;
    }
    if ((request.salaryMinimum !== null || request.salaryMaximum !== null) && !request.salaryCurrency) {
      setFeedback({ kind: 'error', message: 'Add a three-letter currency when entering salary.' });
      return;
    }

    setSubmitting(true);
    try {
      const saved = await api.request<Job>(editing ? `/api/recruiter/jobs/${jobId}` : '/api/recruiter/jobs', { method: editing ? 'PUT' : 'POST', body: JSON.stringify(request) }, true);
      await mutateCache((key) => typeof key === 'string' && key.startsWith('/api/recruiter/jobs'));
      if (editing) {
        await mutate(saved, { revalidate: false });
        setFeedback({ kind: 'success', message: 'Job details saved.' });
      } else {
        navigate(`/recruiter/jobs/${saved.id}/edit`, { replace: true, state: { created: true } });
      }
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The job could not be saved.' });
    } finally {
      setSubmitting(false);
    }
  }

  async function changeStatus(status: JobStatus) {
    if (!jobId) return;
    setFeedback(null);
    try {
      const saved = await api.request<Job>(`/api/recruiter/jobs/${jobId}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }, true);
      await mutate(saved, { revalidate: false });
      await mutateCache((key) => typeof key === 'string' && key.startsWith('/api/recruiter/jobs'));
      setFeedback({ kind: 'success', message: status === 'Published' ? 'The job is now visible to job seekers.' : 'The job has been closed.' });
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The status could not be changed.' });
    }
  }

  async function deleteJob() {
    if (!jobId || !window.confirm('Delete this draft job? This cannot be undone.')) return;
    try {
      await api.request<void>(`/api/recruiter/jobs/${jobId}`, { method: 'DELETE' }, true);
      await mutateCache((key) => typeof key === 'string' && key.startsWith('/api/recruiter/jobs'));
      navigate('/recruiter/jobs');
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The job could not be deleted.' });
    }
  }

  if (editing && isLoading) return <LoadingState label="Loading job editor" />;
  if (editing && (error || !job)) return <ErrorState message="This job could not be found or does not belong to your recruiter account." />;

  return (
    <div className="page-stack">
      <header className="portal-header portal-header--action"><div><Link className="back-link" to="/recruiter/jobs">← Job postings</Link><p className="eyebrow">{editing ? `${job?.status} job` : 'New draft'}</p><h1>{editing ? 'Refine your opportunity.' : 'Create a clear opportunity.'}</h1><p>Structured fields make search—and later transparent matching—more useful.</p></div>{job?.status === 'Draft' && <button className="button button--secondary" type="button" onClick={() => changeStatus('Published')}>Publish job</button>}{job?.status === 'Published' && <button className="button button--secondary" type="button" onClick={() => changeStatus('Closed')}>Close job</button>}</header>
      <form className="surface-form" onSubmit={save}>
        {feedback && <Notice kind={feedback.kind}>{feedback.message}</Notice>}
        <div className="form-grid"><label>Job title<input maxLength={200} value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} /></label><label>Location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} /></label></div>
        <label>Job description<textarea maxLength={8000} rows={10} value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} placeholder="Describe the purpose, responsibilities, and impact of this role." /></label>
        <div className="form-grid form-grid--three"><label>Employment type<select value={form.employmentType} onChange={(event) => setForm({ ...form, employmentType: event.target.value as EmploymentType })}><option value="FullTime">Full time</option><option value="PartTime">Part time</option><option value="Contract">Contract</option><option value="Internship">Internship</option></select></label><label>Work mode<select value={form.workMode} onChange={(event) => setForm({ ...form, workMode: event.target.value as WorkMode })}><option value="OnSite">On-site</option><option value="Hybrid">Hybrid</option><option value="Remote">Remote</option></select></label><label>Minimum experience<input type="number" min={0} max={80} value={form.minimumYearsExperience} onChange={(event) => setForm({ ...form, minimumYearsExperience: Number(event.target.value) })} /></label></div>
        <fieldset><legend>Salary range <span className="optional">Optional</span></legend><div className="form-grid form-grid--three"><label>Minimum<input type="number" min={0} value={form.salaryMinimum} onChange={(event) => setForm({ ...form, salaryMinimum: event.target.value })} /></label><label>Maximum<input type="number" min={0} value={form.salaryMaximum} onChange={(event) => setForm({ ...form, salaryMaximum: event.target.value })} /></label><label>Currency<input maxLength={3} value={form.salaryCurrency} onChange={(event) => setForm({ ...form, salaryCurrency: event.target.value })} placeholder="LKR" /></label></div></fieldset>
        <div className="form-grid"><label>Required skills<input value={form.requiredSkills} onChange={(event) => setForm({ ...form, requiredSkills: event.target.value })} placeholder="C#, PostgreSQL" /><small>Comma-separated</small></label><label>Preferred skills <span className="optional">Optional</span><input value={form.preferredSkills} onChange={(event) => setForm({ ...form, preferredSkills: event.target.value })} placeholder="Azure, Docker" /><small>Comma-separated</small></label></div>
        <label>Closing date <span className="optional">Optional</span><input type="datetime-local" value={form.closesAtUtc} onChange={(event) => setForm({ ...form, closesAtUtc: event.target.value })} /></label>
        <div className="form-actions"><button className="button" disabled={submitting} type="submit">{submitting ? 'Saving…' : editing ? 'Save changes' : 'Create draft'}</button>{job?.status === 'Draft' && <button className="button button--danger" type="button" onClick={deleteJob}>Delete draft</button>}</div>
      </form>
    </div>
  );
}
