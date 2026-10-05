import { Link } from 'react-router';

export function HomePage() {
  return (
    <>
      <section className="hero">
        <div className="container hero__grid">
          <div>
            <p className="eyebrow">Career discovery, with a clearer signal</p>
            <h1>Find work that fits where you are going.</h1>
            <p className="hero__lede">
              Build a focused career profile, explore quality opportunities, and soon receive explainable job matches shaped by your CV.
            </p>
            <div className="button-row">
              <Link className="button" to="/register">Create your profile</Link>
              <Link className="button button--secondary" to="/jobs">Browse open roles</Link>
            </div>
            <div className="trust-row">
              <span><strong>One account</strong> across web and mobile</span>
              <span><strong>Explainable</strong> matching, not a black box</span>
            </div>
          </div>
          <div className="hero-card" aria-label="Example job match preview">
            <div className="hero-card__header"><span>Role preview</span><span className="status status--published">Open</span></div>
            <div className="match-ring"><strong>86</strong><span>match</span></div>
            <h2>Software Engineer</h2>
            <p>Skills, preferences, and experience come together in one understandable score.</p>
            <div className="signal"><span>Skill overlap</span><strong>Strong</strong></div>
            <div className="signal"><span>Work preference</span><strong>Aligned</strong></div>
            <div className="signal"><span>Experience</span><strong>Good fit</strong></div>
          </div>
        </div>
      </section>
      <section className="section">
        <div className="container">
          <div className="section-heading"><p className="eyebrow">Built around real decisions</p><h2>Two sides of a better hiring experience</h2></div>
          <div className="feature-grid">
            <article className="feature-card"><span>01</span><h3>For job seekers</h3><p>Keep your experience and preferences in one place, then discover roles with context—not endless scrolling.</p></article>
            <article className="feature-card"><span>02</span><h3>For recruiters</h3><p>Publish structured opportunities, keep job status current, and prepare for a traceable application workflow.</p></article>
            <article className="feature-card"><span>03</span><h3>Designed for trust</h3><p>Matching explanations and human approval stay central as the AI workflow is introduced.</p></article>
          </div>
        </div>
      </section>
    </>
  );
}
