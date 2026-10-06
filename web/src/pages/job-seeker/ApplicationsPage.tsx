import { useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router';
import useSWR from 'swr';
import { api, ApiError, publicFetcher, swrFetcher } from '../../api/client';
import type { ApplicationList, Job, JobApplication } from '../../api/types';
import { ApplicationStatusBadge } from '../../components/ApplicationStatusBadge';
import { EmptyState, ErrorState, LoadingState, Notice } from '../../components/States';

export function ApplicationsPage() {
  const [searchParams] = useSearchParams();
  const jobId = searchParams.get('jobId');
  const sourceWorkflowRunId = searchParams.get('workflowId');
  const { data, error, isLoading, mutate } = useSWR<ApplicationList>('/api/applications?pageSize=100', swrFetcher, { refreshInterval: 15_000, shouldRetryOnError: false });
  const { data: selectedJob } = useSWR<Job>(jobId ? `/api/jobs/${jobId}` : null, publicFetcher);
  const [coverNote, setCoverNote] = useState('');
  const [notice, setNotice] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!jobId) return;
    setBusy(true);
    setNotice(null);
    try {
      await api.request<JobApplication>('/api/applications', {
        method: 'POST',
        body: JSON.stringify({ jobPostingId: jobId, coverNote: coverNote.trim(), sourceWorkflowRunId }),
      }, true);
      setCoverNote('');
      await mutate();
      setNotice({ kind: 'success', message: 'Application submitted. The recruiter has been notified and every update will appear here.' });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The application could not be submitted.' });
    } finally {
      setBusy(false);
    }
  }

  async function update(application: JobApplication, action: 'withdraw' | 'respond', note: string) {
    setNotice(null);
    try {
      await api.request<JobApplication>(`/api/applications/${application.id}/${action}`, {
        method: 'POST',
        body: JSON.stringify({ note }),
      }, true);
      await mutate();
      setNotice({ kind: 'success', message: action === 'withdraw' ? 'Application withdrawn.' : 'Your response was sent to the recruiter.' });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The application could not be updated.' });
    }
  }

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Application tracker</p><h1>One timeline for every opportunity.</h1><p>Apply from web or mobile, follow recruiter decisions, and inspect the complete audit history.</p></header>
      {notice && <Notice kind={notice.kind}>{notice.message}</Notice>}
      {jobId && <form className="surface-form application-form" onSubmit={submit}>
        <div><p className="eyebrow">New application</p><h2>{selectedJob?.title ?? 'Selected role'}</h2><p className="muted">{selectedJob ? `${selectedJob.companyName} · ${selectedJob.location}` : 'Loading role details…'}</p></div>
        <label>Cover note<textarea minLength={20} maxLength={2000} rows={5} required value={coverNote} onChange={(event) => setCoverNote(event.target.value)} placeholder="Explain the evidence that makes you a strong match." /></label>
        {sourceWorkflowRunId && <small>Linked to approved AI workflow {sourceWorkflowRunId.slice(0, 8)}. The final submission still requires this explicit action.</small>}
        <div className="form-actions"><button className="button" disabled={busy || coverNote.trim().length < 20} type="submit">{busy ? 'Submitting…' : 'Submit application'}</button><Link className="button button--secondary" to={`/jobs/${jobId}`}>Review role</Link></div>
      </form>}
      {!jobId && <Notice kind="info">Choose an open role from <Link to="/jobs">Find jobs</Link> to start a new application.</Notice>}
      {isLoading && <LoadingState label="Loading applications" />}
      {error && <ErrorState message="Your applications could not be loaded." />}
      {data?.items.length === 0 && <EmptyState title="No applications yet" message="Open a published role and select Apply to begin." action={<Link className="button" to="/jobs">Find jobs</Link>} />}
      {data && data.items.length > 0 && <div className="application-list">{data.items.map((application) => <JobSeekerApplicationCard application={application} key={application.id} onUpdate={update} />)}</div>}
    </div>
  );
}

function JobSeekerApplicationCard({ application, onUpdate }: { application: JobApplication; onUpdate: (application: JobApplication, action: 'withdraw' | 'respond', note: string) => Promise<void> }) {
  const [note, setNote] = useState('');
  const [busy, setBusy] = useState(false);
  const needsResponse = application.status === 'MoreInformationRequested';
  const canWithdraw = ['Submitted', 'InReview', 'MoreInformationRequested'].includes(application.status);

  async function act(action: 'withdraw' | 'respond') {
    if (note.trim().length < 2) return;
    setBusy(true);
    try {
      await onUpdate(application, action, note.trim());
      setNote('');
    } finally {
      setBusy(false);
    }
  }

  return <article className="application-card">
    <div className="application-card__header"><div><p className="eyebrow">{application.companyName}</p><h2>{application.jobTitle}</h2><p>{application.jobLocation} · applied {new Date(application.createdAtUtc).toLocaleDateString()}</p></div><ApplicationStatusBadge status={application.status} /></div>
    <blockquote>{application.coverNote}</blockquote>
    <details><summary>Audit history ({application.statusHistory.length})</summary><ol className="application-timeline">{application.statusHistory.map((event) => <li key={event.id}><strong>{event.newStatus}</strong><span>{event.actorRole} · {new Date(event.createdAtUtc).toLocaleString()}</span><p>{event.note}</p></li>)}</ol></details>
    {(needsResponse || canWithdraw) && <div className="application-action"><label>{needsResponse ? 'Your response' : 'Action note'}<textarea maxLength={1000} rows={2} value={note} onChange={(event) => setNote(event.target.value)} /></label><div className="button-row">{needsResponse && <button className="button" disabled={busy || note.trim().length < 2} onClick={() => act('respond')} type="button">Send requested information</button>}{canWithdraw && <button className="button button--danger" disabled={busy || note.trim().length < 2} onClick={() => act('withdraw')} type="button">Withdraw application</button>}</div></div>}
  </article>;
}
