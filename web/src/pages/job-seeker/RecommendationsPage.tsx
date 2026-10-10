import { Link } from 'react-router';
import useSWR from 'swr';
import { ApiError, swrFetcher } from '../../api/client';
import type { RecommendationResponse } from '../../api/types';
import { JobCard } from '../../components/JobCard';
import { EmptyState, ErrorState, LoadingState } from '../../components/States';

export function RecommendationsPage() {
  const { data, error, isLoading } = useSWR<RecommendationResponse>('/api/recommendations', swrFetcher, {
    dedupingInterval: 300_000,
    revalidateOnFocus: false,
    shouldRetryOnError: false,
  });

  if (isLoading) return <LoadingState label="Ranking published jobs" />;
  if (error instanceof ApiError && error.status === 409) {
    return <EmptyState title="Confirm your CV first" message={error.message} action={<Link className="button" to="/job-seeker/profile">Review my profile</Link>} />;
  }
  if (error) return <ErrorState message={error instanceof ApiError ? error.message : 'AI recommendations could not be calculated.'} />;

  return (
    <div className="page-stack">
      <header className="portal-header"><p className="eyebrow">AI semantic ranking</p><h1>Jobs ranked against your complete professional profile.</h1><p>The AI compares transferable skills, responsibilities, experience, higher education, and work preferences—not only exact keyword matches.</p></header>
      {!data?.items.length ? <EmptyState title="No open jobs to rank" message="Recommendations will appear when recruiters publish matching opportunities." /> : (
        <div className="recommendation-list">
          {data.items.map((recommendation) => (
            <section className="recommendation" key={recommendation.job.id}>
              <div className="recommendation__score"><strong>{recommendation.score.toFixed(1)}</strong><span>match score</span></div>
              <JobCard job={recommendation.job} />
              <div className="score-breakdown" aria-label="Score breakdown">
                <span>Skills fit <strong>{recommendation.breakdown.skillsFit}</strong>/35</span>
                <span>Role fit <strong>{recommendation.breakdown.roleFit}</strong>/25</span>
                <span>Experience fit <strong>{recommendation.breakdown.experienceFit}</strong>/15</span>
                <span>Education fit <strong>{recommendation.breakdown.educationFit}</strong>/10</span>
                <span>Location fit <strong>{recommendation.breakdown.locationFit}</strong>/10</span>
                <span>Salary fit <strong>{recommendation.breakdown.salaryFit}</strong>/5</span>
              </div>
              <div className="recommendation__evidence"><div><h3>Why it ranked here</h3><ul>{recommendation.reasons.map((reason) => <li key={reason}>{reason}</li>)}</ul></div><div><h3>Missing required skills</h3>{recommendation.missingRequiredSkills.length ? <div className="tag-row">{recommendation.missingRequiredSkills.map((skill) => <span className="tag tag--missing" key={skill}>{skill}</span>)}</div> : <p className="muted">None identified.</p>}</div></div>
            </section>
          ))}
        </div>
      )}
    </div>
  );
}
