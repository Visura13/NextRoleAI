using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.AgentWorkflows;

internal sealed class CandidateProfileReadTool(ApplicationDbContext dbContext) : IAgentTool
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Name => AgentToolNames.ReadCandidateProfile;

    public async Task<AgentToolExecutionResult> ExecuteAsync(
        AgentToolContext context,
        string inputJson,
        CancellationToken cancellationToken = default)
    {
        var cv = await dbContext.CvDocuments
            .AsNoTracking()
            .Include(document => document.Skills)
            .SingleOrDefaultAsync(
                document => document.UserId == context.UserId,
                cancellationToken);
        var profile = await dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(
                candidate => candidate.UserId == context.UserId,
                cancellationToken);

        var output = new CandidateProfileToolOutput(
            cv?.Status == CvProcessingStatus.Confirmed,
            cv?.CurrentJobTitle ?? string.Empty,
            profile?.PreferredJobTitle ?? string.Empty,
            string.IsNullOrWhiteSpace(profile?.Location)
                ? cv?.Location ?? string.Empty
                : profile.Location,
            profile?.PreferredSalary,
            Math.Max(cv?.YearsExperience ?? 0, profile?.YearsOfExperience ?? 0),
            (cv?.Skills.Select(skill => skill.Name) ?? [])
                .Concat(profile?.Skills.Select(skill => skill.Name) ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
                .ToArray());
        return new AgentToolExecutionResult(
            true,
            JsonSerializer.Serialize(output, JsonOptions));
    }
}
