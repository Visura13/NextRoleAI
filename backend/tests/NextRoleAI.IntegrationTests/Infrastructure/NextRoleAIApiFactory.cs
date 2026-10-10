using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;

namespace NextRoleAI.IntegrationTests.Infrastructure;

public sealed class NextRoleAIApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> originalEnvironment = [];
    private readonly string cvStoragePath = Path.Combine(
        Path.GetTempPath(),
        "NextRoleAI-tests",
        Guid.NewGuid().ToString("N"));

    public NextRoleAIApiFactory()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "NEXTROLEAI_TEST_CONNECTION_STRING");

        SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", "true");
        SetEnvironmentVariable("Jwt__Issuer", "NextRoleAI.IntegrationTests");
        SetEnvironmentVariable("Jwt__Audience", "NextRoleAI.TestClients");
        SetEnvironmentVariable(
            "Jwt__SigningKey",
            "integration-tests-only-signing-key-with-more-than-32-characters");
        SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15");
        SetEnvironmentVariable("Jwt__RefreshTokenDays", "7");
        SetEnvironmentVariable("CvStorage__RootPath", cvStoragePath);
        SetEnvironmentVariable("Notifications__Enabled", "false");
        SetEnvironmentVariable("Notifications__ResendApiKey", null);
        SetEnvironmentVariable("Notifications__FromAddress", null);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAiJobRecommendationRanker>();
            services.AddSingleton<IAiJobRecommendationRanker, TestAiJobRecommendationRanker>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            foreach (var (name, value) in originalEnvironment)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            if (Directory.Exists(cvStoragePath))
            {
                Directory.Delete(cvStoragePath, recursive: true);
            }
        }
    }

    private void SetEnvironmentVariable(string name, string? value)
    {
        originalEnvironment[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }

    private sealed class TestAiJobRecommendationRanker : IAiJobRecommendationRanker
    {
        public string AlgorithmVersion => "ai-semantic-ranking-v1:test-model";

        public Task<IReadOnlyCollection<AiJobMatchScore>> RankAsync(
            CandidateMatchProfile candidate,
            IReadOnlyCollection<JobResult> jobs,
            CancellationToken cancellationToken = default)
        {
            var candidateSkills = candidate.Skills.ToHashSet(
                StringComparer.OrdinalIgnoreCase);
            IReadOnlyCollection<AiJobMatchScore> scores = jobs
                .Select(job =>
                {
                    var matched = job.Skills
                        .Where(skill => candidateSkills.Contains(skill.Name))
                        .Select(skill => skill.Name)
                        .ToArray();
                    var missing = job.Skills
                        .Where(skill => skill.IsRequired && !candidateSkills.Contains(skill.Name))
                        .Select(skill => skill.Name)
                        .ToArray();
                    var breakdown = new MatchBreakdown(40m, 25m, 15m, 10m, 10m);
                    return new AiJobMatchScore(
                        job.Id,
                        breakdown.Total,
                        breakdown,
                        matched,
                        missing,
                        [
                            "Test AI ranking found relevant professional evidence.",
                            "Test AI ranking evaluated the complete job profile."
                        ]);
                })
                .ToArray();
            return Task.FromResult(scores);
        }
    }
}
