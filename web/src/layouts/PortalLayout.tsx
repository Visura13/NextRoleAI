import { NavLink, Outlet } from 'react-router';
import { useAuth } from '../auth/useAuth';

export function PortalLayout({ role }: { role: 'JobSeeker' | 'Recruiter' }) {
  const { session } = useAuth();
  const recruiter = role === 'Recruiter';

  return (
    <div className="container portal-layout">
      <aside className="portal-sidebar">
        <p className="eyebrow">{recruiter ? 'Recruiter workspace' : 'Job seeker workspace'}</p>
        <h2>{session?.user.firstName} {session?.user.lastName}</h2>
        <nav aria-label={`${recruiter ? 'Recruiter' : 'Job seeker'} navigation`}>
          <NavLink end to={recruiter ? '/recruiter' : '/job-seeker'}>Overview</NavLink>
          {recruiter ? (
            <>
              <NavLink to="/recruiter/company">Company profile</NavLink>
              <NavLink to="/recruiter/jobs">Job postings</NavLink>
              <NavLink to="/recruiter/applications">Applications</NavLink>
            </>
          ) : (
            <>
              <NavLink to="/job-seeker/profile">My profile</NavLink>
              <NavLink to="/job-seeker/recommendations">Recommendations</NavLink>
              <NavLink to="/job-seeker/agent-workflows">AI workflows</NavLink>
              <NavLink to="/jobs">Find jobs</NavLink>
              <NavLink to="/job-seeker/applications">Applications</NavLink>
            </>
          )}
        </nav>
      </aside>
      <section className="portal-content"><Outlet /></section>
    </div>
  );
}
