import { useState } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { ApplicationList, ApplicationStatus, JobApplication } from '../../api/types';
import { ApplicationStatusBadge } from '../../components/ApplicationStatusBadge';
import { EmptyState, ErrorState, LoadingState, Notice } from '../../components/States';

const statuses: Array<ApplicationStatus | ''> = ['', 'Submitted', 'InReview', 'MoreInformationRequested', 'Shortlisted', 'Rejected', 'Withdrawn'];

export function RecruiterApplicationsPage() {
  const [status, setStatus] = useState<ApplicationStatus | ''>('');
  const endpoint = `/api/recruiter/applications?pageSize=100${status ? `&status=${status}` : ''}`;
  const { data, error, isLoading, mutate } = useSWR<ApplicationList>(endpoint, swrFetcher, { refreshInterval: 15_000, shouldRetryOnError: false });
  const [notice, setNotice] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);

  async function decide(application: JobApplication, nextStatus: ApplicationStatus, note: string) {
    setNotice(null);
    try {
      await api.request<JobApplication>(`/api/recruiter/applications/${application.id}/status`, {
        method: 'PATCH',
        body: JSON.stringify({ status: nextStatus, note }),
      }, true);
      await mutate();
      setNotice({ kind: 'success', message: `Application moved to ${nextStatus}. Notification delivery was recorded.` });
    } catch (requestError) {
      setNotice({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The decision could not be recorded.' });
    }
  }

  return <div className="page-stack">
    <header className="portal-header portal-header--action"><div><p className="eyebrow">Candidate pipeline</p><h1>Review with a durable audit trail.</h1><p>Only applications for your company are visible. Each decision becomes a timestamped event and triggers a recorded email attempt.</p></div><label className="compact-filter">Status<select value={status} onChange={(event) => setStatus(event.target.value as ApplicationStatus | '')}>{statuses.map((value) => <option key={value || 'all'} value={value}>{value || 'All statuses'}</option>)}</select></label></header>
    {notice && <Notice kind={notice.kind}>{notice.message}</Notice>}
    {isLoading && <LoadingState label="Loading candidate applications" />}
    {error && <ErrorState message="Applications could not be loaded." />}
    {data?.items.length === 0 && <EmptyState title="No applications in this view" message="Applications appear after Job Seekers submit to one of your published roles." />}
    {data && data.items.length > 0 && <div className="application-list">{data.items.map((application) => <RecruiterApplicationCard application={application} key={`${application.id}-${application.status}`} onDecide={decide} />)}</div>}
  </div>;
}

function RecruiterApplicationCard({ application, onDecide }: { application: JobApplication; onDecide: (application: JobApplication, status: ApplicationStatus, note: string) => Promise<void> }) {
  const choices = decisionChoices(application.status);
  const [status, setStatus] = useState<ApplicationStatus | ''>(choices[0] ?? '');
  const [note, setNote] = useState('');
  const [busy, setBusy] = useState(false);

  async function submitDecision() {
    if (!status || note.trim().length < 2) return;
    setBusy(true);
    try {
      await onDecide(application, status, note.trim());
      setNote('');
    } finally {
      setBusy(false);
    }
  }

  return <article className="application-card">
    <div className="application-card__header"><div><p className="eyebrow">{application.jobTitle}</p><h2>{application.jobSeekerName}</h2><a href={`mailto:${application.jobSeekerEmail}`}>{application.jobSeekerEmail}</a></div><ApplicationStatusBadge status={application.status} /></div>
    <section><h3>Candidate note</h3><blockquote>{application.coverNote}</blockquote></section>
    <details><summary>Decision history ({application.statusHistory.length})</summary><ol className="application-timeline">{application.statusHistory.map((event) => <li key={event.id}><strong>{event.newStatus}</strong><span>{event.actorRole} · {new Date(event.createdAtUtc).toLocaleString()}</span><p>{event.note}</p></li>)}</ol></details>
    <p className="notification-audit">Latest notification: <strong>{application.notificationDeliveries[0]?.provider ?? 'None'} · {application.notificationDeliveries[0]?.status ?? 'Not attempted'}</strong></p>
    {choices.length > 0 && <div className="application-action"><div className="form-grid"><label>Decision<select value={status} onChange={(event) => setStatus(event.target.value as ApplicationStatus)}>{choices.map((choice) => <option key={choice} value={choice}>{choice}</option>)}</select></label><label>Decision note<textarea minLength={2} maxLength={1000} rows={3} value={note} onChange={(event) => setNote(event.target.value)} /></label></div><button className="button" disabled={busy || !status || note.trim().length < 2} onClick={submitDecision} type="button">{busy ? 'Saving…' : 'Record decision'}</button></div>}
  </article>;
}

function decisionChoices(status: ApplicationStatus): ApplicationStatus[] {
  if (status === 'Submitted') return ['InReview', 'MoreInformationRequested', 'Shortlisted', 'Rejected'];
  if (status === 'InReview') return ['MoreInformationRequested', 'Shortlisted', 'Rejected'];
  if (status === 'MoreInformationRequested') return ['Shortlisted', 'Rejected'];
  return [];
}
