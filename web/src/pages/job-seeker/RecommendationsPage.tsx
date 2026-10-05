import { Link } from 'react-router';
import useSWR from 'swr';
import { ApiError, swrFetcher } from '../../api/client';
import type { RecommendationResponse } from '../../api/types';
import { JobCard } from '../../components/JobCard';
import { EmptyState, ErrorState, LoadingState, Notice } from '../../components/States';

export function RecommendationsPage() {
  const { data, error, isLoading } = useSWR<RecommendationResponse>('/api/recommendations', swrFetcher, { shouldRetryOnError: false });

  if (isLoading) return <LoadingState label="Ranking published jobs" />;
  if (error instanceof ApiError && error.status === 409) {
    return <EmptyState title="Confirm your CV first" message={error.message} action={<Link className="button" to="/job-seeker/cv">Review my CV</Link>} />;
  }
  if (error) return <ErrorState message="Recommendations could not be calculated." />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">Explainable ranking</p><h1>Jobs ranked by evidence, not guesswork.</h1><p>Each score uses confirmed skills, title, experience, and location. The breakdown shows exactly where every point came from.</p></header>
      <Notice kind="info">Algorithm version: {data?.items[0]?.algorithmVersion ?? 'deterministic-v1'}. Scores are deterministic and do not use an LLM.</Notice>
      {!data?.items.length ? <EmptyState title="No open jobs to rank" message="Recommendations will appear when recruiters publish matching opportunities." /> : (
        <div className="recommendation-list">
          {data.items.map((recommendation) => (
            <section className="recommendation" key={recommendation.job.id}>
              <div className="recommendation__score"><strong>{recommendation.score.toFixed(1)}</strong><span>match score</span></div>
              <JobCard job={recommendation.job} />
              <div className="score-breakdown" aria-label="Score breakdown">
                <span>Required skills <strong>{recommendation.breakdown.requiredSkills}</strong>/45</span>
                <span>Preferred skills <strong>{recommendation.breakdown.preferredSkills}</strong>/15</span>
                <span>Title <strong>{recommendation.breakdown.title}</strong>/15</span>
                <span>Experience <strong>{recommendation.breakdown.experience}</strong>/15</span>
                <span>Location <strong>{recommendation.breakdown.location}</strong>/10</span>
              </div>
              <div className="recommendation__evidence"><div><h3>Why it ranked here</h3><ul>{recommendation.reasons.map((reason) => <li key={reason}>{reason}</li>)}</ul></div><div><h3>Missing required skills</h3>{recommendation.missingRequiredSkills.length ? <div className="tag-row">{recommendation.missingRequiredSkills.map((skill) => <span className="tag tag--missing" key={skill}>{skill}</span>)}</div> : <p className="muted">None identified.</p>}</div></div>
            </section>
          ))}
        </div>
      )}
    </div>
  );
}
