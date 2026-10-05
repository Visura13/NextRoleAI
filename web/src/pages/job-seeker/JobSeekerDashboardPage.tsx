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
      <header className="portal-header"><p className="eyebrow">Your career workspace</p><h1>Good to see you, {session?.user.firstName}.</h1><p>Strengthen your profile now; CV-powered ranked recommendations are the next intelligence layer.</p></header>
      <div className="metric-grid">
        <article className="metric-card metric-card--accent"><span>Profile readiness</span><strong>{profileStrength}%</strong><div className="progress"><span style={{ width: `${profileStrength}%` }} /></div><Link to="/job-seeker/profile">Complete profile →</Link></article>
        <article className="metric-card"><span>Skills captured</span><strong>{profile?.skills.length ?? 0}</strong><p>Structured skills will support transparent job matching.</p></article>
        <article className="metric-card"><span>Application activity</span><strong>—</strong><p>Application tracking arrives with the shared workflow in Part 8.</p></article>
      </div>
      <section className="action-panel"><div><p className="eyebrow">Explore today</p><h2>Published opportunities are ready.</h2><p>Search by title, location, employment type, work mode, and skills.</p></div><Link className="button" to="/jobs">Find jobs</Link></section>
    </div>
  );
}
