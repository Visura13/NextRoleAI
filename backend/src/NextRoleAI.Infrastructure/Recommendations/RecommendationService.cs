using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NextRoleAI.Application.Cvs;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Recommendations;

internal sealed class RecommendationService(
    ApplicationDbContext dbContext,
    IJobService jobService,
    IAiJobRecommendationRanker ranker,
    IMemoryCache cache,
    ILogger<RecommendationService> logger) : IRecommendationService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<RecommendationListResult> GetAsync(
        string userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cv = await dbContext.CvDocuments
            .AsNoTracking()
            .Include(document => document.Skills)
            .SingleOrDefaultAsync(document => document.UserId == userId, cancellationToken);
        if (cv is null || cv.Status != CvProcessingStatus.Confirmed)
        {
            return RecommendationListResult.Failure(
                RecommendationError.ConfirmedCvRequired,
                "Upload and confirm your structured CV profile before requesting recommendations.");
        }

        var profile = await dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        var candidateSkills = cv.Skills
            .Select(skill => skill.Name)
            .Concat(profile?.Skills.Select(skill => skill.Name) ?? [])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var candidate = new CandidateMatchProfile(
            cv.CurrentJobTitle,
            profile?.PreferredJobTitle ?? string.Empty,
            string.IsNullOrWhiteSpace(profile?.Location) ? cv.Location : profile.Location,
            profile?.PreferredSalary,
            string.Join(
                " ",
                new[] { cv.ProfessionalSummary, profile?.Summary }
                    .Where(summary => !string.IsNullOrWhiteSpace(summary))
                    .Distinct(StringComparer.OrdinalIgnoreCase)),
            Math.Max(cv.YearsExperience, profile?.YearsOfExperience ?? 0),
            candidateSkills,
            ReadEducation(cv.EducationJson));

        var published = await jobService.SearchPublishedAsync(
            new JobSearchQuery(null, null, null, null, null, "newest", 1, 100),
            cancellationToken);
        if (published.Items.Count == 0)
        {
            return RecommendationListResult.Success([]);
        }

        var cacheKey = CreateCacheKey(userId, cv.UpdatedAtUtc, profile?.UpdatedAtUtc, published.Items);
        if (!cache.TryGetValue(cacheKey, out JobRecommendationResult[]? rankedRecommendations) ||
            rankedRecommendations is null)
        {
            IReadOnlyCollection<AiJobMatchScore> scores;
            try
            {
                scores = await ranker.RankAsync(candidate, published.Items, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "AI job ranking failed.");
                return RecommendationListResult.Failure(
                    RecommendationError.AiRankingUnavailable,
                    "AI job ranking is temporarily unavailable. No deterministic scores were substituted; try again after checking the AI provider.");
            }

            var scoresByJobId = scores.ToDictionary(score => score.JobId);
            rankedRecommendations = published.Items
                .Select(job =>
                {
                    var score = scoresByJobId[job.Id];
                    return new JobRecommendationResult(
                        job,
                        score.Score,
                        score.Breakdown,
                        score.MatchedSkills,
                        score.MissingRequiredSkills,
                        score.Reasons,
                        ranker.AlgorithmVersion);
                })
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.Job.PublishedAtUtc)
                .ThenBy(result => result.Job.Title, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            cache.Set(cacheKey, rankedRecommendations, TimeSpan.FromHours(12));
        }

        return RecommendationListResult.Success(rankedRecommendations.Take(limit).ToArray());
    }

    private string CreateCacheKey(
        string userId,
        DateTimeOffset cvUpdatedAtUtc,
        DateTimeOffset? profileUpdatedAtUtc,
        IReadOnlyCollection<JobResult> jobs) =>
        string.Join(
            '|',
            "ai-job-ranking",
            ranker.AlgorithmVersion,
            userId,
            cvUpdatedAtUtc.UtcTicks,
            profileUpdatedAtUtc?.UtcTicks ?? 0,
            string.Join(',', jobs.Select(job => $"{job.Id:N}:{job.UpdatedAtUtc.UtcTicks}")));

    private static IReadOnlyCollection<CvEducationItem> ReadEducation(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<CvEducationItem[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
