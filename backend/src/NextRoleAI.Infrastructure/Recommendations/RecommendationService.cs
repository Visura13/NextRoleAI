using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Recommendations;

internal sealed class RecommendationService(
    ApplicationDbContext dbContext,
    IJobService jobService,
    IJobMatchScorer scorer) : IRecommendationService
{
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
            Math.Max(cv.YearsExperience, profile?.YearsOfExperience ?? 0),
            candidateSkills);

        var published = await jobService.SearchPublishedAsync(
            new JobSearchQuery(null, null, null, null, null, "newest", 1, 100),
            cancellationToken);
        var recommendations = published.Items
            .Select(job =>
            {
                var score = scorer.Score(
                    candidate,
                    new JobMatchInput(
                        job.Title,
                        job.Location,
                        job.WorkMode,
                        job.MinimumYearsExperience,
                        job.Skills));
                return new JobRecommendationResult(
                    job,
                    score.Score,
                    score.Breakdown,
                    score.MatchedSkills,
                    score.MissingRequiredSkills,
                    score.Reasons,
                    scorer.AlgorithmVersion);
            })
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Job.PublishedAtUtc)
            .ThenBy(result => result.Job.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToArray();

        return RecommendationListResult.Success(recommendations);
    }
}
