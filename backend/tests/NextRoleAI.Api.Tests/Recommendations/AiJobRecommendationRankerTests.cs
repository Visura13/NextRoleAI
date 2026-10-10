using Microsoft.Extensions.Options;
using NextRoleAI.Application.Cvs;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.Infrastructure.Cvs;
using NextRoleAI.Infrastructure.Recommendations;

namespace NextRoleAI.Api.Tests.Recommendations;

public sealed class AiJobRecommendationRankerTests
{
    [Fact]
    public async Task RankAsync_UsesSemanticAiScoresAndValidatesEvidenceLists()
    {
        var reactJob = CreateJob(
            "React Frontend Engineer",
            "Build accessible React interfaces with TypeScript.",
            [new JobSkillInput("React", true), new JobSkillInput("TypeScript", true)],
            180_000m,
            250_000m,
            "LKR");
        var backendJob = CreateJob(
            "Java Backend Engineer",
            "Build Java services.",
            [new JobSkillInput("Java", true)]);
        var response = $$"""
            {
              "rankings": [
                {
                  "jobId": "{{reactJob.Id}}",
                  "skillsFit": 34,
                  "roleFit": 24,
                  "experienceFit": 12,
                  "educationFit": 9,
                  "locationFit": 10,
                  "salaryFit": 4,
                  "matchedSkills": ["React", "TypeScript", "Invented skill"],
                  "missingRequiredSkills": [],
                  "reasons": [
                    "React and TypeScript directly support the frontend responsibilities.",
                    "The candidate has relevant full-stack project experience."
                  ]
                },
                {
                  "jobId": "{{backendJob.Id}}",
                  "skillsFit": 6,
                  "roleFit": 5,
                  "experienceFit": 8,
                  "educationFit": 9,
                  "locationFit": 10,
                  "salaryFit": 3,
                  "matchedSkills": [],
                  "missingRequiredSkills": ["Java", "Not a listed requirement"],
                  "reasons": [
                    "The profile does not show Java service development.",
                    "General software engineering education is transferable."
                  ]
                }
              ]
            }
            """;
        var languageModel = new FakeLanguageModel(response);
        var ranker = CreateRanker(languageModel);

        var results = await ranker.RankAsync(CreateCandidate(), [reactJob, backendJob]);

        var react = Assert.Single(results, item => item.JobId == reactJob.Id);
        var backend = Assert.Single(results, item => item.JobId == backendJob.Id);
        Assert.Equal(93m, react.Score);
        Assert.Equal(34m, react.Breakdown.SkillsFit);
        Assert.Equal(4m, react.Breakdown.SalaryFit);
        Assert.Equal(["React", "TypeScript"], react.MatchedSkills);
        Assert.True(react.Score > backend.Score);
        Assert.Equal(["Java"], backend.MissingRequiredSkills);
        Assert.Equal("ai-semantic-ranking-v1:test-model", ranker.AlgorithmVersion);
        Assert.Contains("\"preferredMonthlySalary\":200000", languageModel.LastUserPrompt);
        Assert.Contains("\"salaryMinimum\":180000", languageModel.LastUserPrompt);
        Assert.Contains("salaryFit", languageModel.LastSystemPrompt);
    }

    [Fact]
    public async Task RankAsync_RejectsResponsesThatOmitAJob()
    {
        var job = CreateJob(
            "React Frontend Engineer",
            "Build React applications.",
            [new JobSkillInput("React", true)]);
        var ranker = CreateRanker("{\"rankings\":[]}");

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => ranker.RankAsync(CreateCandidate(), [job]));
    }

    [Fact]
    public async Task RankAsync_RemovesRequiredSkillsAlreadyPresentInCandidateProfile()
    {
        var job = CreateJob(
            "React Frontend Engineer",
            "Build React interfaces with TypeScript and CSS.",
            [
                new JobSkillInput("CSS", true),
                new JobSkillInput("React", true),
                new JobSkillInput("TypeScript", true)
            ]);
        var response = $$"""
            {
              "rankings": [{
                "jobId": "{{job.Id}}",
                "skillsFit": 35,
                "roleFit": 25,
                "experienceFit": 12,
                "educationFit": 9,
                "locationFit": 10,
                "salaryFit": 3,
                "matchedSkills": ["CSS", "React", "TypeScript"],
                "missingRequiredSkills": ["css", " react ", "TypeScript"],
                "reasons": [
                  "The candidate lists all required frontend technologies.",
                  "The candidate's experience aligns with the role responsibilities."
                ]
              }]
            }
            """;
        var ranker = CreateRanker(response);
        var candidate = CreateCandidate() with
        {
            Skills = ["CSS", "React", "TypeScript", "JavaScript"]
        };

        var result = Assert.Single(await ranker.RankAsync(candidate, [job]));

        Assert.Empty(result.MissingRequiredSkills);
        Assert.Equal(["CSS", "React", "TypeScript"], result.MatchedSkills);
    }

    [Fact]
    public async Task RankAsync_ProcessesLargeJobSetsInBoundedBatches()
    {
        var jobs = Enumerable.Range(1, 11)
            .Select(index => CreateJob(
                $"Frontend Engineer {index}",
                "Build React applications.",
                [new JobSkillInput("React", true)]))
            .ToArray();
        var responses = jobs
            .Chunk(10)
            .Select(batch => System.Text.Json.JsonSerializer.Serialize(new
            {
                rankings = batch.Select(job => new
                {
                    jobId = job.Id,
                    skillsFit = 35,
                    roleFit = 22,
                    experienceFit = 12,
                    educationFit = 8,
                    locationFit = 9,
                    salaryFit = 3,
                    matchedSkills = new[] { "React" },
                    missingRequiredSkills = Array.Empty<string>(),
                    reasons = new[]
                    {
                        "React experience supports the stated responsibilities.",
                        "The candidate profile aligns with frontend delivery."
                    }
                })
            }))
            .ToArray();
        var languageModel = new FakeLanguageModel(responses);
        var ranker = CreateRanker(languageModel);

        var results = await ranker.RankAsync(CreateCandidate(), jobs);

        Assert.Equal(11, results.Count);
        Assert.Equal(2, languageModel.CallCount);
    }

    private static AiJobRecommendationRanker CreateRanker(string response) =>
        CreateRanker(new FakeLanguageModel(response));

    private static AiJobRecommendationRanker CreateRanker(
        ICvLanguageModelClient languageModel) =>
        new(
            languageModel,
            Options.Create(new CvAiOptions
            {
                Enabled = true,
                BaseUrl = "https://example.test/v1/",
                ApiKey = "test-key",
                Model = "test-model"
            }));

    private static CandidateMatchProfile CreateCandidate() =>
        new(
            "Full Stack Developer Intern",
            "Frontend Engineer",
            "Colombo",
            200_000m,
            "Builds responsive web applications with React and TypeScript.",
            1,
            ["React", "TypeScript", "JavaScript", "Node.js"],
            [new CvEducationItem(
                "BSc (Hons)",
                "Information Technology",
                "SLIIT",
                "In progress",
                "BSc (Hons) in Information Technology",
                0.98m)]);

    private static JobResult CreateJob(
        string title,
        string description,
        IReadOnlyCollection<JobSkillInput> skills,
        decimal? salaryMinimum = null,
        decimal? salaryMaximum = null,
        string? salaryCurrency = null) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Example Company",
            title,
            description,
            "Colombo",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            1,
            salaryMinimum,
            salaryMaximum,
            salaryCurrency,
            JobStatus.Published,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            skills,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

    private sealed class FakeLanguageModel(params string[] responses) : ICvLanguageModelClient
    {
        private readonly Queue<string> responses = new(responses);

        public int CallCount { get; private set; }

        public string LastSystemPrompt { get; private set; } = string.Empty;

        public string LastUserPrompt { get; private set; } = string.Empty;

        public Task<string> GenerateJsonAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastSystemPrompt = systemPrompt;
            LastUserPrompt = userPrompt;
            return Task.FromResult(this.responses.Dequeue());
        }
    }
}
