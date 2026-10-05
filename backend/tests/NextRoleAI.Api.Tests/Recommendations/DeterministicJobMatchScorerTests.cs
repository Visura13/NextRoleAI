using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Tests.Recommendations;

public sealed class DeterministicJobMatchScorerTests
{
    private readonly DeterministicJobMatchScorer scorer = new();

    [Fact]
    public void Score_ReturnsTransparentWeightedBreakdown()
    {
        var candidate = new CandidateMatchProfile(
            "Backend Developer",
            "Software Engineer",
            "Colombo",
            3,
            ["C#", "PostgreSQL"]);
        var job = new JobMatchInput(
            "Software Engineer",
            "Colombo",
            WorkMode.Hybrid,
            2,
            [
                new JobSkillInput("C#", true),
                new JobSkillInput("Docker", true),
                new JobSkillInput("PostgreSQL", false)
            ]);

        var result = scorer.Score(candidate, job);

        Assert.Equal(77.5m, result.Score);
        Assert.Equal(22.5m, result.Breakdown.RequiredSkills);
        Assert.Equal(15m, result.Breakdown.PreferredSkills);
        Assert.Equal(15m, result.Breakdown.Title);
        Assert.Equal(15m, result.Breakdown.Experience);
        Assert.Equal(10m, result.Breakdown.Location);
        Assert.Equal(["C#", "PostgreSQL"], result.MatchedSkills);
        Assert.Equal(["Docker"], result.MissingRequiredSkills);
        Assert.Contains(result.Reasons, reason => reason.Contains("Missing 1"));
    }

    [Fact]
    public void Score_IsCaseInsensitive_AndRemoteRolesMatchLocation()
    {
        var candidate = new CandidateMatchProfile(
            "Flutter Developer",
            "Mobile Engineer",
            "Kandy",
            1,
            ["flutter"]);
        var job = new JobMatchInput(
            "Mobile Engineer",
            "Remote",
            WorkMode.Remote,
            1,
            [new JobSkillInput("Flutter", true)]);

        var result = scorer.Score(candidate, job);

        Assert.Equal(100m, result.Score);
        Assert.Empty(result.MissingRequiredSkills);
        Assert.Contains("Flutter", result.MatchedSkills);
        Assert.Contains("The role is remote.", result.Reasons);
    }
}
