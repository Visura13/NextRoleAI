using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Cvs;

namespace NextRoleAI.Application.Recommendations;

public sealed record CandidateMatchProfile(
    string CurrentJobTitle,
    string PreferredJobTitle,
    string PreferredLocation,
    decimal? PreferredSalary,
    string ProfessionalSummary,
    int YearsExperience,
    IReadOnlyCollection<string> Skills,
    IReadOnlyCollection<CvEducationItem> Education);

public sealed record MatchBreakdown(
    decimal SkillsFit,
    decimal RoleFit,
    decimal ExperienceFit,
    decimal EducationFit,
    decimal LocationFit,
    decimal SalaryFit)
{
    public decimal Total =>
        SkillsFit + RoleFit + ExperienceFit + EducationFit + LocationFit + SalaryFit;
}

public sealed record AiJobMatchScore(
    Guid JobId,
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
    ConfirmedCvRequired,
    AiRankingUnavailable
}

public sealed record RecommendationListResult(
    IReadOnlyCollection<JobRecommendationResult> Items,
    RecommendationError Error = RecommendationError.None,
    string? Message = null)
{
    public bool Succeeded => Error == RecommendationError.None;

    public static RecommendationListResult Success(
        IReadOnlyCollection<JobRecommendationResult> items) => new(items);

    public static RecommendationListResult Failure(
        RecommendationError error,
        string message) => new([], error, message);
}
