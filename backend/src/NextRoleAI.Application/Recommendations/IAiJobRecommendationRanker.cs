using NextRoleAI.Application.Jobs;

namespace NextRoleAI.Application.Recommendations;

public interface IAiJobRecommendationRanker
{
    Task<IReadOnlyCollection<AiJobMatchScore>> RankAsync(
        CandidateMatchProfile candidate,
        IReadOnlyCollection<JobResult> jobs,
        CancellationToken cancellationToken = default);

    string AlgorithmVersion { get; }
}
