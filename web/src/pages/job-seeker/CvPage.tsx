import { useState, type FormEvent } from 'react';
import useSWR from 'swr';
import { api, ApiError, swrFetcher } from '../../api/client';
import type { CvProfile } from '../../api/types';
import { ErrorState, LoadingState, Notice } from '../../components/States';

type QualityMetricKey = 'completenessScore' | 'clarityScore' | 'skillsEvidenceScore' | 'impactScore' | 'atsReadabilityScore';

interface CvEducationForm {
  qualification: string;
  fieldOfStudy: string;
  institution: string;
  status: string;
}

interface CvForm {
  candidateName: string;
  email: string;
  phone: string;
  location: string;
  currentJobTitle: string;
  professionalSummary: string;
  yearsExperience: number;
  skills: string;
  education: CvEducationForm[];
}

const emptyEducation: CvEducationForm = {
  qualification: '',
  fieldOfStudy: '',
  institution: '',
  status: '',
};

function createForm(profile: CvProfile): CvForm {
  return {
    candidateName: profile.candidateName,
    email: profile.email,
    phone: profile.phone,
    location: profile.location,
    currentJobTitle: profile.currentJobTitle,
    professionalSummary: profile.professionalSummary,
    yearsExperience: profile.yearsExperience,
    skills: profile.skills.join(', '),
    education: (profile.education ?? []).map(({ qualification, fieldOfStudy, institution, status }) => ({
      qualification,
      fieldOfStudy,
      institution,
      status,
    })),
  };
}

function AnalysisSummary({ profile }: { profile: CvProfile }) {
  const isAiAnalysis = profile.analysisMethod === 'ai';
  const assessment = profile.qualityAssessment;

  if (!isAiAnalysis) {
    return (
      <Notice kind="info">
        Basic extraction is active. The AI provider did not run, so the skills below come from deterministic keyword matching and no CV quality score is available. Configure AI, then replace the CV to analyse it again.
      </Notice>
    );
  }

  if (!assessment) {
    return <Notice kind="info">AI profile extraction completed, but a CV quality assessment was not returned. You can still review and correct every field below.</Notice>;
  }

  const metrics: Array<[string, QualityMetricKey]> = [
    ['Completeness', 'completenessScore'],
    ['Clarity', 'clarityScore'],
    ['Skills evidence', 'skillsEvidenceScore'],
    ['Impact', 'impactScore'],
    ['ATS readability', 'atsReadabilityScore'],
  ];

  return (
    <section className="cv-analysis-panel" aria-labelledby="cv-quality-heading">
      <div className="cv-analysis-header">
        <div>
          <p className="eyebrow">AI CV assessment</p>
          <h2 id="cv-quality-heading">CV quality review</h2>
          <p>Document-quality guidance only. This is not an employability score.</p>
        </div>
        <div className="cv-score" aria-label={`Overall CV quality score ${assessment.overallScore} out of 100`}>
          <strong>{assessment.overallScore}</strong>
          <span>/ 100</span>
        </div>
      </div>
      <div className="quality-grid">
        {metrics.map(([label, property]) => (
          <div className="quality-metric" key={property}>
            <div><span>{label}</span><strong>{assessment[property]}</strong></div>
            <div className="quality-meter" role="progressbar" aria-label={label} aria-valuemin={0} aria-valuemax={100} aria-valuenow={assessment[property]}>
              <span style={{ width: `${assessment[property]}%` }} />
            </div>
          </div>
        ))}
      </div>
      <div className="quality-feedback">
        <div>
          <h3>What works well</h3>
          {assessment.strengths.length > 0 ? <ul>{assessment.strengths.map((item) => <li key={item}>{item}</li>)}</ul> : <p className="muted">No specific strengths were returned.</p>}
        </div>
        <div>
          <h3>What to improve</h3>
          {assessment.improvements.length > 0 ? <ul>{assessment.improvements.map((item) => <li key={item}>{item}</li>)}</ul> : <p className="muted">No specific improvements were returned.</p>}
        </div>
      </div>
      <p className="analysis-meta">Analysed with {profile.analysisModel || 'the configured AI model'}{profile.analyzedAtUtc ? ` on ${new Date(profile.analyzedAtUtc).toLocaleString()}` : ''}.</p>
    </section>
  );
}

export function CvPage() {
  const { data, error, isLoading, mutate } = useSWR<CvProfile>('/api/cv', swrFetcher, { shouldRetryOnError: false });
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [editedForm, setForm] = useState<CvForm | null>(null);
  const [feedback, setFeedback] = useState<{ kind: 'success' | 'error' | 'info'; message: string } | null>(null);
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);

  const form = editedForm ?? (data ? createForm(data) : null);

  function updateEducation(index: number, field: keyof CvEducationForm, value: string) {
    if (!form) return;
    setForm({
      ...form,
      education: form.education.map((item, itemIndex) => itemIndex === index ? { ...item, [field]: value } : item),
    });
  }

  function addEducation() {
    if (!form || form.education.length >= 20) return;
    setForm({ ...form, education: [...form.education, { ...emptyEducation }] });
  }

  function removeEducation(index: number) {
    if (!form) return;
    setForm({ ...form, education: form.education.filter((_, itemIndex) => itemIndex !== index) });
  }

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
      setFeedback(uploaded.analysisMethod === 'ai'
        ? { kind: 'info', message: 'AI analysis completed. Review the extracted profile and CV assessment, then confirm your corrections.' }
        : { kind: 'info', message: 'Basic extraction completed because AI analysis is not configured. Review and correct the profile before confirming it.' });
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
    const education = form.education
      .map((item) => ({
        qualification: item.qualification.trim(),
        fieldOfStudy: item.fieldOfStudy.trim(),
        institution: item.institution.trim(),
        status: item.status.trim(),
      }))
      .filter((item) => Object.values(item).some(Boolean));
    if (!form.candidateName.trim() || !form.currentJobTitle.trim() || form.professionalSummary.trim().length < 20 || skills.length === 0) {
      setFeedback({ kind: 'error', message: 'Add your name, current title, a useful summary, and at least one skill.' });
      return;
    }
    if (education.some((item) => !item.qualification)) {
      setFeedback({ kind: 'error', message: 'Add a qualification name for every education entry, or remove the incomplete entry.' });
      return;
    }
    setSaving(true);
    try {
      const saved = await api.request<CvProfile>('/api/cv/profile', {
        method: 'PUT',
        body: JSON.stringify({ ...form, candidateName: form.candidateName.trim(), email: form.email.trim(), phone: form.phone.trim(), location: form.location.trim(), currentJobTitle: form.currentJobTitle.trim(), professionalSummary: form.professionalSummary.trim(), skills, education }),
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
        <div className="button-row"><button className="button" disabled={uploading} type="submit">{uploading ? 'Processing…' : data ? 'Replace and process' : 'Upload and process'}</button>{data && <button className="button button--danger" type="button" onClick={removeCv}>Delete CV</button>}</div>
      </form>
      {data && <AnalysisSummary profile={data} />}
      {data && form && (
        <form className="surface-form" onSubmit={confirmProfile}>
          <div className="cv-file-summary"><div><p className="eyebrow">Extracted document</p><h2>{data.originalFileName}</h2><p>{(data.sizeBytes / 1024).toFixed(1)} KB · SHA-256 {data.sha256Checksum.slice(0, 12)}…</p></div><span className={`status status--${data.status === 'Confirmed' ? 'published' : 'draft'}`}>{data.status === 'Confirmed' ? 'Confirmed' : 'Needs review'}</span></div>
          <Notice kind="info">Extraction is only a draft. You are responsible for reviewing and correcting these fields.</Notice>
          <div className="form-grid"><label>Candidate name<input maxLength={150} value={form.candidateName} onChange={(event) => setForm({ ...form, candidateName: event.target.value })} /></label><label>Current job title<input maxLength={150} value={form.currentJobTitle} onChange={(event) => setForm({ ...form, currentJobTitle: event.target.value })} /></label></div>
          <div className="form-grid"><label>Email<input maxLength={254} type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} /></label><label>Phone<input maxLength={40} value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} /></label></div>
          <div className="form-grid"><label>Location<input maxLength={150} value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} /></label><label>Years of experience<input min={0} max={80} type="number" value={form.yearsExperience} onChange={(event) => setForm({ ...form, yearsExperience: Number(event.target.value) })} /></label></div>
          <label>Professional summary<textarea maxLength={2000} rows={6} value={form.professionalSummary} onChange={(event) => setForm({ ...form, professionalSummary: event.target.value })} /></label>
          <label>Skills<input value={form.skills} onChange={(event) => setForm({ ...form, skills: event.target.value })} /><small>Comma-separated. Correct or add anything the analysis missed.</small></label>

          <section className="education-section" aria-labelledby="education-heading">
            <div className="education-section__header">
              <div><h3 id="education-heading">Education</h3><p>Higher education only: degrees, diplomas, higher diplomas, and professional certificates. O/L and A/L entries are excluded.</p></div>
              <button className="button button--ghost button--small" disabled={form.education.length >= 20} type="button" onClick={addEducation}>Add education</button>
            </div>
            {form.education.length === 0 && <p className="education-empty">No higher-education qualification was extracted. Add one if your CV includes it.</p>}
            {form.education.map((item, index) => (
              <div className="education-card" key={`education-${index}`}>
                <div className="education-card__header"><strong>Qualification {index + 1}</strong><button className="text-button" type="button" onClick={() => removeEducation(index)}>Remove</button></div>
                <div className="form-grid"><label>Qualification<input maxLength={150} placeholder="e.g. BSc (Hons)" value={item.qualification} onChange={(event) => updateEducation(index, 'qualification', event.target.value)} /></label><label>Field of study<input maxLength={200} placeholder="e.g. Software Engineering" value={item.fieldOfStudy} onChange={(event) => updateEducation(index, 'fieldOfStudy', event.target.value)} /></label></div>
                <div className="form-grid"><label>Institution<input maxLength={200} value={item.institution} onChange={(event) => updateEducation(index, 'institution', event.target.value)} /></label><label>Status<input maxLength={50} placeholder="e.g. Completed or In progress" value={item.status} onChange={(event) => updateEducation(index, 'status', event.target.value)} /></label></div>
              </div>
            ))}
          </section>

          <div className="form-actions"><button className="button" disabled={saving} type="submit">{saving ? 'Confirming…' : data.status === 'Confirmed' ? 'Save corrections' : 'Confirm CV profile'}</button></div>
        </form>
      )}
    </div>
  );
}
