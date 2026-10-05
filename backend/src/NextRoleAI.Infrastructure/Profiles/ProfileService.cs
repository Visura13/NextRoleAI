using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.Profiles;
using NextRoleAI.Domain.Profiles;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Profiles;

internal sealed class ProfileService(
    ApplicationDbContext dbContext,
    TimeProvider timeProvider) : IProfileService
{
    public async Task<JobSeekerProfileResult?> GetJobSeekerAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);

        return profile is null ? null : ToResult(profile);
    }

    public async Task<JobSeekerProfileResult> UpsertJobSeekerAsync(
        string userId,
        JobSeekerProfileUpdate update,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var profile = await dbContext.JobSeekerProfiles
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAtUtc = now
            };
            dbContext.JobSeekerProfiles.Add(profile);
        }
        else
        {
            profile.Skills.Clear();
        }

        profile.Headline = update.Headline.Trim();
        profile.Summary = update.Summary.Trim();
        profile.Location = update.Location.Trim();
        profile.PreferredJobTitle = update.PreferredJobTitle.Trim();
        profile.YearsOfExperience = update.YearsOfExperience;
        profile.UpdatedAtUtc = now;

        foreach (var skill in NormaliseSkills(update.Skills))
        {
            profile.Skills.Add(new JobSeekerSkill
            {
                Id = Guid.NewGuid(),
                Name = skill
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResult(profile);
    }

    public async Task<CompanyProfileResult?> GetCompanyAsync(
        string recruiterUserId,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.CompanyProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.RecruiterUserId == recruiterUserId,
                cancellationToken);

        return profile is null ? null : ToResult(profile);
    }

    public async Task<CompanyProfileResult> UpsertCompanyAsync(
        string recruiterUserId,
        CompanyProfileUpdate update,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var profile = await dbContext.CompanyProfiles
            .SingleOrDefaultAsync(
                candidate => candidate.RecruiterUserId == recruiterUserId,
                cancellationToken);

        if (profile is null)
        {
            profile = new CompanyProfile
            {
                Id = Guid.NewGuid(),
                RecruiterUserId = recruiterUserId,
                CreatedAtUtc = now
            };
            dbContext.CompanyProfiles.Add(profile);
        }

        profile.Name = update.Name.Trim();
        profile.Description = update.Description.Trim();
        profile.Location = update.Location.Trim();
        profile.WebsiteUrl = string.IsNullOrWhiteSpace(update.WebsiteUrl)
            ? null
            : update.WebsiteUrl.Trim();
        profile.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResult(profile);
    }

    private static IReadOnlyCollection<string> NormaliseSkills(IEnumerable<string> skills) =>
        skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static JobSeekerProfileResult ToResult(JobSeekerProfile profile) =>
        new(
            profile.Id,
            profile.Headline,
            profile.Summary,
            profile.Location,
            profile.PreferredJobTitle,
            profile.YearsOfExperience,
            profile.Skills
                .Select(skill => skill.Name)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            profile.UpdatedAtUtc);

    private static CompanyProfileResult ToResult(CompanyProfile profile) =>
        new(
            profile.Id,
            profile.Name,
            profile.Description,
            profile.Location,
            profile.WebsiteUrl,
            profile.UpdatedAtUtc);
}
