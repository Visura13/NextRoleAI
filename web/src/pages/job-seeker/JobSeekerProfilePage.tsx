import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { CvProfile, JobSeekerProfile } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';
import { CvWorkspace } from './CvPage';

interface ProfileForm {
  location: string;
  preferredJobTitle: string;
  preferredSalary: string;
}

const emptyForm: ProfileForm = { location: '', preferredJobTitle: '', preferredSalary: '' };

export function JobSeekerProfilePage() {
  const navigate = useNavigate();
  const { data, error, isLoading, mutate } = useSWR<JobSeekerProfile>('/api/profiles/job-seeker', swrFetcher);
  const { data: cv } = useSWR<CvProfile>('/api/cv', swrFetcher, { shouldRetryOnError: false });
  const [editedForm, setForm] = useState<ProfileForm | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const form = editedForm ?? (data ? {
    location: data.location,
    preferredJobTitle: data.preferredJobTitle,
    preferredSalary: data.preferredSalary?.toString() ?? '',
  } : { ...emptyForm, location: cv?.location ?? '' });

  async function submit(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);
    if (!cv || cv.status !== 'Confirmed') {
      setFeedback({ kind: 'error', message: 'Upload and confirm your professional profile before saving job preferences.' });
      return;
    }
    if (!form.location.trim() || !form.preferredJobTitle.trim()) {
      setFeedback({ kind: 'error', message: 'Add your preferred job title and work location before saving.' });
      return;
    }
    const preferredSalary = form.preferredSalary.trim()
      ? Number(form.preferredSalary)
      : null;
    if (preferredSalary !== null && (!Number.isFinite(preferredSalary) || preferredSalary < 1 || preferredSalary > 1_000_000_000)) {
      setFeedback({ kind: 'error', message: 'Enter a preferred monthly salary from LKR 1 to LKR 1,000,000,000, or leave it blank.' });
      return;
    }
    setSubmitting(true);
    try {
      const saved = await api.request<JobSeekerProfile>('/api/profiles/job-seeker', {
        method: 'PUT',
        body: JSON.stringify({
          headline: cv.currentJobTitle,
          summary: cv.professionalSummary,
          location: form.location.trim(),
          preferredJobTitle: form.preferredJobTitle.trim(),
          preferredSalary,
          yearsOfExperience: cv.yearsExperience,
          skills: cv.skills,
        }),
      }, true);
      await mutate(saved, { revalidate: false });
      navigate('/job-seeker/recommendations', { replace: true });
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'Your profile could not be saved.' });
    } finally {
      setSubmitting(false);
    }
  }

  if (isLoading) return <LoadingState label="Loading your profile" />;
  if (error && !(error instanceof ApiError && error.status === 404)) return <ErrorState message="Your profile could not be loaded." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">My profile</p><h1>Manage your complete career profile.</h1><p>Keep your CV evidence, AI analysis, higher education, preferences, and salary expectations together in one workspace.</p></header>
      <CvWorkspace embedded />
      <div className="section-heading"><p className="eyebrow">Job preferences</p><h2>Tell NextRoleAI what you want next.</h2><p>These preferences complement the evidence extracted from your confirmed CV and influence job ranking.</p></div>
      <form className="surface-form" onSubmit={submit}>
        {feedback && <Notice kind={feedback.kind}>{feedback.message}</Notice>}
        <div className="form-grid"><label>Preferred job title<input maxLength={150} value={form.preferredJobTitle} onChange={(event) => setForm({ ...form, preferredJobTitle: event.target.value })} placeholder="Software Engineer" /></label><label>Preferred work location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} placeholder="Colombo" /></label></div>
        <label>Preferred minimum monthly salary (LKR) <span className="optional">Optional</span><input type="number" min={1} max={1000000000} step={1} value={form.preferredSalary} onChange={(event) => setForm({ ...form, preferredSalary: event.target.value })} placeholder="200000" /><small>The AI uses this only when a job advertises a comparable salary range.</small></label>
        {(!cv || cv.status !== 'Confirmed') && <Notice kind="info">Upload and confirm your professional profile above before saving preferences.</Notice>}
        <div className="form-actions"><button className="button" disabled={submitting || !cv || cv.status !== 'Confirmed'} type="submit">{submitting ? 'Saving…' : 'Save preferences and view recommendations'}</button></div>
      </form>
    </div>
  );
}
