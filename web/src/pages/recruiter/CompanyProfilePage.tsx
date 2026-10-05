import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { CompanyProfile } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';

const emptyCompany = { name: '', description: '', location: '', websiteUrl: '' };

export function CompanyProfilePage() {
  const { data, error, isLoading, mutate } = useSWR<CompanyProfile>('/api/profiles/company', swrFetcher);
  const [editedForm, setForm] = useState<typeof emptyCompany | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error'; message: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const form = editedForm ?? (data
    ? { name: data.name, description: data.description, location: data.location, websiteUrl: data.websiteUrl ?? '' }
    : emptyCompany);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);
    if (!form.name.trim() || !form.description.trim() || !form.location.trim()) {
      setFeedback({ kind: 'error', message: 'Company name, description, and location are required.' });
      return;
    }
    setSubmitting(true);
    try {
      const saved = await api.request<CompanyProfile>('/api/profiles/company', { method: 'PUT', body: JSON.stringify({ ...form, websiteUrl: form.websiteUrl.trim() || null }) }, true);
      await mutate(saved, { revalidate: false });
      setFeedback({ kind: 'success', message: 'Company profile saved. You can now create and publish jobs.' });
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The company profile could not be saved.' });
    } finally {
      setSubmitting(false);
    }
  }

  if (isLoading) return <LoadingState label="Loading company profile" />;
  if (error && !(error instanceof ApiError && error.status === 404)) return <ErrorState message="The company profile could not be loaded." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Company profile</p><h1>Give candidates a reason to pay attention.</h1><p>This identity is attached to every job your recruiter account publishes.</p></header>
      <form className="surface-form" onSubmit={submit}>
        {feedback && <Notice kind={feedback.kind}>{feedback.message}</Notice>}
        <div className="form-grid"><label>Company name<input maxLength={200} value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} /></label><label>Location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} placeholder="Colombo, Sri Lanka" /></label></div>
        <label>Company description<textarea maxLength={3000} rows={7} value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} placeholder="What do you build, who do you serve, and what is it like to work here?" /></label>
        <label>Website URL <span className="optional">Optional</span><input type="url" maxLength={500} value={form.websiteUrl} onChange={(event) => setForm({ ...form, websiteUrl: event.target.value })} placeholder="https://example.com" /></label>
        <div className="form-actions"><button className="button" disabled={submitting} type="submit">{submitting ? 'Saving…' : 'Save company profile'}</button></div>
      </form>
    </div>
  );
}
