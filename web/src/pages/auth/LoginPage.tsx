import { useState, type FormEvent } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router';
import { ApiError } from '../../api/client';
import { useAuth } from '../../auth/useAuth';
import { FieldError, Notice } from '../../components/States';

export function LoginPage() {
  const { session, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();

  if (session) return <Navigate replace to="/dashboard" />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError('');
    if (!email.trim() || !password) {
      setError('Enter both your email and password.');
      return;
    }

    setSubmitting(true);
    try {
      await login(email.trim(), password);
      const requestedPath = (location.state as { from?: string } | null)?.from;
      const safePath = requestedPath?.startsWith('/') && !requestedPath.startsWith('//')
        ? requestedPath
        : '/dashboard';
      navigate(safePath, { replace: true });
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'We could not log you in. Try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="auth-page">
      <div className="auth-panel auth-panel--intro">
        <Link className="brand brand--light" to="/"><span className="brand__mark">N</span><span>NextRoleAI</span></Link>
        <div><p className="eyebrow">Welcome back</p><h1>Your next move starts with a clear view.</h1><p>Sign in as a Job Seeker or Recruiter. We will take you to the right workspace automatically.</p></div>
        <p className="auth-quote">One shared identity for the web experience today and the Flutter app in the next increment.</p>
      </div>
      <div className="auth-panel auth-panel--form">
        <div className="form-card">
          <p className="eyebrow">Secure account access</p>
          <h2>Log in</h2>
          <p className="muted">New to NextRoleAI? <Link to="/register">Create an account</Link></p>
          {error && <Notice kind="error">{error}</Notice>}
          <form onSubmit={handleSubmit} noValidate>
            <label>Email address<input autoComplete="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} /></label>
            <label>Password<input autoComplete="current-password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} /></label>
            <FieldError />
            <button className="button button--full" disabled={submitting} type="submit">{submitting ? 'Logging in…' : 'Log in'}</button>
          </form>
        </div>
      </div>
    </section>
  );
}
