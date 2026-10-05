namespace NextRoleAI.Application.Recommendations;

public interface IRecommendationService
{
    Task<RecommendationListResult> GetAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default);
}
