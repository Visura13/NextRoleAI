using NextRoleAI.Application.Jobs;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Application.Recommendations;

public sealed record CandidateMatchProfile(
    string CurrentJobTitle,
    string PreferredJobTitle,
    string PreferredLocation,
    int YearsExperience,
    IReadOnlyCollection<string> Skills);

public sealed record JobMatchInput(
    string Title,
    string Location,
    WorkMode WorkMode,
    int MinimumYearsExperience,
    IReadOnlyCollection<JobSkillInput> Skills);

public sealed record MatchBreakdown(
    decimal RequiredSkills,
    decimal PreferredSkills,
    decimal Title,
    decimal Experience,
    decimal Location)
{
    public decimal Total =>
        RequiredSkills + PreferredSkills + Title + Experience + Location;
}

public sealed record MatchScore(
    decimal Score,
    MatchBreakdown Breakdown,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingRequiredSkills,
    IReadOnlyCollection<string> Reasons);

public sealed record JobRecommendationResult(
    JobResult Job,
    decimal Score,
    MatchBreakdown Breakdown,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingRequiredSkills,
    IReadOnlyCollection<string> Reasons,
    string AlgorithmVersion);

public enum RecommendationError
{
    None,
    ConfirmedCvRequired
}

public sealed record RecommendationListResult(
    IReadOnlyCollection<JobRecommendationResult> Items,
    RecommendationError Error = RecommendationError.None,
    string? Message = null)
{
    public bool Succeeded => Error == RecommendationError.None;

    public static RecommendationListResult Success(
        IReadOnlyCollection<JobRecommendationResult> items) => new(items);

    public static RecommendationListResult Failure(string message) =>
        new([], RecommendationError.ConfirmedCvRequired, message);
}
