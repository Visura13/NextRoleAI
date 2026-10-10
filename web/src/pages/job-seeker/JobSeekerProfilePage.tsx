import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { JobSeekerProfile } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';

interface ProfileForm {
  headline: string;
  summary: string;
  location: string;
  preferredJobTitle: string;
  preferredSalary: string;
  yearsOfExperience: number;
  skills: string;
}

const emptyForm: ProfileForm = { headline: '', summary: '', location: '', preferredJobTitle: '', preferredSalary: '', yearsOfExperience: 0, skills: '' };

export function JobSeekerProfilePage() {
  const navigate = useNavigate();
  const { data, error, isLoading, mutate } = useSWR<JobSeekerProfile>('/api/profiles/job-seeker', swrFetcher);
  const [editedForm, setForm] = useState<ProfileForm | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const form = editedForm ?? (data ? {
    headline: data.headline,
    summary: data.summary,
    location: data.location,
    preferredJobTitle: data.preferredJobTitle,
    preferredSalary: data.preferredSalary?.toString() ?? '',
    yearsOfExperience: data.yearsOfExperience,
    skills: data.skills.join(', '),
  } : emptyForm);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);
    const skills = [...new Set(form.skills.split(',').map((skill) => skill.trim()).filter(Boolean))];
    if (!form.headline.trim() || !form.summary.trim() || !form.location.trim() || !form.preferredJobTitle.trim()) {
      setFeedback({ kind: 'error', message: 'Complete all profile fields before saving.' });
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
        body: JSON.stringify({ ...form, preferredSalary, headline: form.headline.trim(), summary: form.summary.trim(), location: form.location.trim(), preferredJobTitle: form.preferredJobTitle.trim(), skills }),
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
      <header className="portal-header"><p className="eyebrow">Job seeker profile</p><h1>Shape the signal recruiters and matching will see.</h1><p>Preferences here complement the skills and experience in your confirmed CV profile.</p></header>
      <form className="surface-form" onSubmit={submit}>
        {feedback && <Notice kind={feedback.kind}>{feedback.message}</Notice>}
        <div className="form-grid"><label>Professional headline<input maxLength={160} value={form.headline} onChange={(event) => setForm({ ...form, headline: event.target.value })} placeholder="Backend engineer focused on reliable systems" /></label><label>Preferred job title<input maxLength={150} value={form.preferredJobTitle} onChange={(event) => setForm({ ...form, preferredJobTitle: event.target.value })} placeholder="Software Engineer" /></label></div>
        <label>Professional summary<textarea maxLength={2000} rows={6} value={form.summary} onChange={(event) => setForm({ ...form, summary: event.target.value })} placeholder="Describe your experience, strengths, and the work you want to do." /></label>
        <div className="form-grid"><label>Location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} placeholder="Colombo" /></label><label>Years of experience<input type="number" min={0} max={80} value={form.yearsOfExperience} onChange={(event) => setForm({ ...form, yearsOfExperience: Number(event.target.value) })} /></label></div>
        <label>Preferred minimum monthly salary (LKR) <span className="optional">Optional</span><input type="number" min={1} max={1000000000} step={1} value={form.preferredSalary} onChange={(event) => setForm({ ...form, preferredSalary: event.target.value })} placeholder="200000" /><small>The AI uses this only when a job advertises a comparable salary range.</small></label>
        <label>Skills<input value={form.skills} onChange={(event) => setForm({ ...form, skills: event.target.value })} placeholder="C#, React, PostgreSQL, Azure" /><small>Separate each skill with a comma. Up to 30 skills.</small></label>
        <div className="form-actions"><button className="button" disabled={submitting} type="submit">{submitting ? 'Saving…' : 'Save profile'}</button></div>
      </form>
    </div>
  );
}
