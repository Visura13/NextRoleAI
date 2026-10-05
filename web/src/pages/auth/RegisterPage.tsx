import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router';
import { ApiError } from '../../api/client';
import type { UserRole } from '../../api/types';
import { useAuth } from '../../auth/useAuth';
import { Notice } from '../../components/States';

export function RegisterPage() {
  const { session, register } = useAuth();
  const navigate = useNavigate();
  const [role, setRole] = useState<UserRole>('JobSeeker');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  if (session) return <Navigate replace to="/dashboard" />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError('');
    if (![firstName, lastName, email, password].every((value) => value.trim())) {
      setError('Complete every field to create your account.');
      return;
    }
    if (password.length < 8) {
      setError('Use at least 8 characters for your password.');
      return;
    }

    setSubmitting(true);
    try {
      await register({ email: email.trim(), password, firstName: firstName.trim(), lastName: lastName.trim(), role });
      navigate('/dashboard', { replace: true });
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'We could not create your account. Try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="auth-page">
      <div className="auth-panel auth-panel--intro">
        <Link className="brand brand--light" to="/"><span className="brand__mark">N</span><span>NextRoleAI</span></Link>
        <div><p className="eyebrow">Start with your role</p><h1>A focused workspace for every side of hiring.</h1><p>Job Seekers can use the web app now and the Flutter app later. Recruiters get a dedicated web workspace.</p></div>
        <p className="auth-quote">Your role controls access at both the interface and API level.</p>
      </div>
      <div className="auth-panel auth-panel--form">
        <div className="form-card">
          <p className="eyebrow">Create your account</p>
          <h2>Join NextRoleAI</h2>
          <p className="muted">Already registered? <Link to="/login">Log in</Link></p>
          <div className="role-picker" role="group" aria-label="Choose account type">
            <button className={role === 'JobSeeker' ? 'is-selected' : ''} type="button" onClick={() => setRole('JobSeeker')}><strong>Job seeker</strong><span>Discover the right opportunities</span></button>
            <button className={role === 'Recruiter' ? 'is-selected' : ''} type="button" onClick={() => setRole('Recruiter')}><strong>Recruiter</strong><span>Publish and manage roles</span></button>
          </div>
          {error && <Notice kind="error">{error}</Notice>}
          <form onSubmit={handleSubmit} noValidate>
            <div className="form-grid">
              <label>First name<input autoComplete="given-name" maxLength={100} value={firstName} onChange={(event) => setFirstName(event.target.value)} /></label>
              <label>Last name<input autoComplete="family-name" maxLength={100} value={lastName} onChange={(event) => setLastName(event.target.value)} /></label>
            </div>
            <label>Email address<input autoComplete="email" type="email" maxLength={256} value={email} onChange={(event) => setEmail(event.target.value)} /></label>
            <label>Password<input autoComplete="new-password" type="password" minLength={8} maxLength={128} value={password} onChange={(event) => setPassword(event.target.value)} /><small>At least 8 characters; Identity also checks password strength.</small></label>
            <button className="button button--full" disabled={submitting} type="submit">{submitting ? 'Creating account…' : `Create ${role === 'JobSeeker' ? 'job seeker' : 'recruiter'} account`}</button>
          </form>
        </div>
      </div>
    </section>
  );
}
