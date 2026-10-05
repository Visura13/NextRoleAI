import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { CvProfile } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';

interface CvForm {
  candidateName: string;
  email: string;
  phone: string;
  location: string;
  currentJobTitle: string;
  professionalSummary: string;
  yearsExperience: number;
  skills: string;
}

export function CvPage() {
  const { data, error, isLoading, mutate } = useSWR<CvProfile>('/api/cv', swrFetcher, { shouldRetryOnError: false });
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [editedForm, setForm] = useState<CvForm | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error' | 'info'; message: string } | null>(null);
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);

  const form = editedForm ?? (data ? {
    candidateName: data.candidateName,
    email: data.email,
    phone: data.phone,
    location: data.location,
    currentJobTitle: data.currentJobTitle,
    professionalSummary: data.professionalSummary,
    yearsExperience: data.yearsExperience,
    skills: data.skills.join(', '),
  } : null);

  async function upload(event: FormEvent) {
    event.preventDefault();
    setFeedback(null);
    if (!selectedFile) {
      setFeedback({ kind: 'error', message: 'Choose a PDF or DOCX CV first.' });
      return;
    }
    const extension = selectedFile.name.split('.').pop()?.toLowerCase();
    if (!['pdf', 'docx'].includes(extension ?? '') || selectedFile.size > 5 * 1024 * 1024) {
      setFeedback({ kind: 'error', message: 'Choose a PDF or DOCX file no larger than 5 MB.' });
      return;
    }

    const body = new FormData();
    body.append('file', selectedFile);
    setUploading(true);
    try {
      const uploaded = await api.request<CvProfile>('/api/cv', { method: 'POST', body }, true);
      await mutate(uploaded, { revalidate: false });
      setForm(null);
      setSelectedFile(null);
      setFeedback({ kind: 'info', message: 'Text extracted. Review every field, correct it if needed, then confirm the profile.' });
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'Your CV could not be uploaded.' });
    } finally {
      setUploading(false);
    }
  }

  async function confirmProfile(event: FormEvent) {
    event.preventDefault();
    if (!form) return;
    setFeedback(null);
    const skills = [...new Set(form.skills.split(',').map((skill) => skill.trim()).filter(Boolean))];
    if (!form.candidateName.trim() || !form.currentJobTitle.trim() || form.professionalSummary.trim().length < 20 || skills.length === 0) {
      setFeedback({ kind: 'error', message: 'Add your name, current title, a useful summary, and at least one skill.' });
      return;
    }
    setSaving(true);
    try {
      const saved = await api.request<CvProfile>('/api/cv/profile', {
        method: 'PUT',
        body: JSON.stringify({ ...form, candidateName: form.candidateName.trim(), email: form.email.trim(), phone: form.phone.trim(), location: form.location.trim(), currentJobTitle: form.currentJobTitle.trim(), professionalSummary: form.professionalSummary.trim(), skills }),
      }, true);
      await mutate(saved, { revalidate: false });
      setForm(null);
      setFeedback({ kind: 'success', message: 'CV profile confirmed. Your explainable recommendations are ready.' });
    } catch (requestError) {
      setFeedback({ kind: 'error', message: requestError instanceof ApiError ? requestError.message : 'The structured CV profile could not be saved.' });
    } finally {
      setSaving(false);
    }
  }

  async function removeCv() {
    if (!window.confirm('Delete this CV and its extracted profile?')) return;
    await api.request<void>('/api/cv', { method: 'DELETE' }, true);
    await mutate(undefined, { revalidate: false });
    setForm(null);
    setFeedback({ kind: 'success', message: 'The CV and extracted profile were deleted.' });
  }

  if (isLoading) return <LoadingState label="Loading your CV" />;
  if (error && !(error instanceof ApiError && error.status === 404)) return <ErrorState message="Your CV could not be loaded." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">CV intelligence</p><h1>Turn your CV into a profile you control.</h1><p>Upload a PDF or DOCX, then review the extracted information before it can influence recommendations.</p></header>
      {feedback && <Notice kind={feedback.kind}>{feedback.message}</Notice>}
      <form className="surface-form upload-panel" onSubmit={upload}>
        <div><h2>{data ? 'Replace CV' : 'Upload CV'}</h2><p className="muted">PDF or DOCX, maximum 5 MB. The original document is stored privately.</p></div>
        <label>CV document<input accept=".pdf,.docx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document" type="file" onChange={(event) => setSelectedFile(event.target.files?.[0] ?? null)} /></label>
        <div className="button-row"><button className="button" disabled={uploading} type="submit">{uploading ? 'Extracting…' : data ? 'Replace and extract' : 'Upload and extract'}</button>{data && <button className="button button--danger" type="button" onClick={removeCv}>Delete CV</button>}</div>
      </form>
      {data && form && (
        <form className="surface-form" onSubmit={confirmProfile}>
          <div className="cv-file-summary"><div><p className="eyebrow">Extracted document</p><h2>{data.originalFileName}</h2><p>{(data.sizeBytes / 1024).toFixed(1)} KB · SHA-256 {data.sha256Checksum.slice(0, 12)}…</p></div><span className={`status status--${data.status === 'Confirmed' ? 'published' : 'draft'}`}>{data.status === 'Confirmed' ? 'Confirmed' : 'Needs review'}</span></div>
          <Notice kind="info">Extraction is only a draft. You are responsible for reviewing and correcting these fields.</Notice>
          <div className="form-grid"><label>Candidate name<input maxLength={150} value={form.candidateName} onChange={(event) => setForm({ ...form, candidateName: event.target.value })} /></label><label>Current job title<input maxLength={150} value={form.currentJobTitle} onChange={(event) => setForm({ ...form, currentJobTitle: event.target.value })} /></label></div>
          <div className="form-grid"><label>Email<input maxLength={254} type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} /></label><label>Phone<input maxLength={40} value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} /></label></div>
          <div className="form-grid"><label>Location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} /></label><label>Years of experience<input min={0} max={80} type="number" value={form.yearsExperience} onChange={(event) => setForm({ ...form, yearsExperience: Number(event.target.value) })} /></label></div>
          <label>Professional summary<textarea maxLength={2000} rows={6} value={form.professionalSummary} onChange={(event) => setForm({ ...form, professionalSummary: event.target.value })} /></label>
          <label>Skills<input value={form.skills} onChange={(event) => setForm({ ...form, skills: event.target.value })} /><small>Comma-separated. Correct or add anything the extractor missed.</small></label>
          <div className="form-actions"><button className="button" disabled={saving} type="submit">{saving ? 'Confirming…' : data.status === 'Confirmed' ? 'Save corrections' : 'Confirm CV profile'}</button></div>
        </form>
      )}
    </div>
  );
}
