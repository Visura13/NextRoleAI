using NextRoleAI.Application.Recommendations;

namespace NextRoleAI.Api.Contracts.Recommendations;

public sealed record RecommendationResponse(
    IReadOnlyCollection<JobRecommendationResult> Items);
