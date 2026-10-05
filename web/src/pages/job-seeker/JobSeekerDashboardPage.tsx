import { Link } from 'react-router';
import useSWR from 'swr';
import { swrFetcher } from '../../api/client';
import type { JobSeekerProfile } from '../../api/types';
import { useAuth } from '../../auth/useAuth';

export function JobSeekerDashboardPage() {
  const { session } = useAuth();
  const { data: profile } = useSWR<JobSeekerProfile>('/api/profiles/job-seeker', swrFetcher);
  const profileStrength = profile ? Math.min(100, 30 + (profile.skills.length * 8) + (profile.summary ? 20 : 0)) : 10;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Your career workspace</p><h1>Good to see you, {session?.user.firstName}.</h1><p>Upload and confirm your CV, then compare published opportunities with a transparent score breakdown.</p></header>
      <div className="metric-grid">
        <article className="metric-card metric-card--accent"><span>Profile readiness</span><strong>{profileStrength}%</strong><div className="progress"><span style={{ width: `${profileStrength}%` }} /></div><Link to="/job-seeker/profile">Complete profile →</Link></article>
        <article className="metric-card"><span>Skills captured</span><strong>{profile?.skills.length ?? 0}</strong><p>Your confirmed profile and CV skills contribute to matching.</p></article>
        <article className="metric-card"><span>Application activity</span><strong>—</strong><p>Application tracking arrives with the shared workflow in Part 8.</p></article>
      </div>
      <section className="action-panel"><div><p className="eyebrow">CV-powered matching</p><h2>Build recommendations you can inspect.</h2><p>Review the extracted CV profile before any document data influences a score, then delegate a shortlist without giving up the final decision.</p></div><div className="button-row"><Link className="button button--secondary" to="/job-seeker/cv">Review my CV</Link><Link className="button button--secondary" to="/job-seeker/recommendations">View recommendations</Link><Link className="button" to="/job-seeker/agent-workflows">Run AI workflow</Link></div></section>
    </div>
  );
}
