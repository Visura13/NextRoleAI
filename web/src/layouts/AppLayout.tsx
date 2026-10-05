import { useState } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router';
import { useAuth } from '../auth/useAuth';

export function AppLayout() {
  const { session, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();
  const dashboard = session?.user.role === 'Recruiter' ? '/recruiter' : '/job-seeker';

  async function handleLogout() {
    await logout();
    navigate('/');
  }

  return (
    <div className="app-shell">
      <header className="site-header">
        <div className="container header-inner">
          <Link className="brand" to="/" aria-label="NextRoleAI home">
            <span className="brand__mark">N</span>
            <span>NextRole<span>AI</span></span>
          </Link>
          <button
            className="menu-button"
            type="button"
            aria-expanded={menuOpen}
            aria-controls="main-navigation"
            onClick={() => setMenuOpen((current) => !current)}
          >
            Menu
          </button>
          <nav id="main-navigation" className={menuOpen ? 'main-nav is-open' : 'main-nav'} aria-label="Main navigation">
            <NavLink to="/jobs">Explore jobs</NavLink>
            {session ? (
              <>
                <NavLink to={dashboard}>Dashboard</NavLink>
                <span className="user-chip">{session.user.firstName} · {session.user.role === 'JobSeeker' ? 'Job seeker' : 'Recruiter'}</span>
                <button className="button button--ghost button--small" type="button" onClick={handleLogout}>Log out</button>
              </>
            ) : (
              <>
                <NavLink to="/login">Log in</NavLink>
                <Link className="button button--small" to="/register">Get started</Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main><Outlet /></main>
      <footer className="site-footer">
        <div className="container footer-inner">
          <div><strong>NextRoleAI</strong><p>Clearer signals. Better next roles.</p></div>
          <p>Built for job seekers and growing teams.</p>
        </div>
      </footer>
    </div>
  );
}
