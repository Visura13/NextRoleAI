using System.Globalization;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Application.Recommendations;

public sealed class DeterministicJobMatchScorer : IJobMatchScorer
{
    private const decimal RequiredSkillWeight = 45m;
    private const decimal PreferredSkillWeight = 15m;
    private const decimal TitleWeight = 15m;
    private const decimal ExperienceWeight = 15m;
    private const decimal LocationWeight = 10m;

    public string AlgorithmVersion => "deterministic-v1";

    public MatchScore Score(CandidateMatchProfile candidate, JobMatchInput job)
    {
        var candidateSkills = candidate.Skills
            .Select(Normalise)
            .Where(skill => skill.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var required = job.Skills
            .Where(skill => skill.IsRequired)
            .Select(skill => new { skill.Name, Normalised = Normalise(skill.Name) })
            .Where(skill => skill.Normalised.Length > 0)
            .ToArray();
        var preferred = job.Skills
            .Where(skill => !skill.IsRequired)
            .Select(skill => new { skill.Name, Normalised = Normalise(skill.Name) })
            .Where(skill => skill.Normalised.Length > 0)
            .ToArray();

        var matchedRequired = required
            .Where(skill => candidateSkills.Contains(skill.Normalised))
            .ToArray();
        var matchedPreferred = preferred
            .Where(skill => candidateSkills.Contains(skill.Normalised))
            .ToArray();
        var requiredScore = Coverage(matchedRequired.Length, required.Length) *
            RequiredSkillWeight;
        var preferredScore = Coverage(matchedPreferred.Length, preferred.Length) *
            PreferredSkillWeight;

        var titleScore = Math.Max(
            TitleCoverage(candidate.CurrentJobTitle, job.Title),
            TitleCoverage(candidate.PreferredJobTitle, job.Title)) * TitleWeight;

        var experienceCoverage = job.MinimumYearsExperience <= 0
            ? 1m
            : Math.Min(
                1m,
                (decimal)Math.Max(candidate.YearsExperience, 0) /
                    job.MinimumYearsExperience);
        var experienceScore = experienceCoverage * ExperienceWeight;

        var locationMatched = job.WorkMode == WorkMode.Remote ||
            ContainsEither(candidate.PreferredLocation, job.Location);
        var locationScore = locationMatched ? LocationWeight : 0m;

        var matchedSkills = matchedRequired
            .Select(skill => skill.Name)
            .Concat(matchedPreferred.Select(skill => skill.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingRequired = required
            .Except(matchedRequired)
            .Select(skill => skill.Name)
            .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var breakdown = new MatchBreakdown(
            Round(requiredScore),
            Round(preferredScore),
            Round(titleScore),
            Round(experienceScore),
            Round(locationScore));
        var reasons = BuildReasons(
            matchedSkills,
            missingRequired,
            titleScore,
            experienceCoverage,
            locationMatched,
            job.WorkMode == WorkMode.Remote);

        return new MatchScore(
            Round(breakdown.Total),
            breakdown,
            matchedSkills,
            missingRequired,
            reasons);
    }

    private static decimal Coverage(int matched, int total) =>
        total == 0 ? 1m : (decimal)matched / total;

    private static decimal TitleCoverage(string candidateTitle, string jobTitle)
    {
        var jobTokens = Tokens(jobTitle);
        if (jobTokens.Count == 0)
        {
            return 0m;
        }

        var candidateTokens = Tokens(candidateTitle);
        return (decimal)jobTokens.Count(candidateTokens.Contains) / jobTokens.Count;
    }

    private static HashSet<string> Tokens(string value) =>
        value.Split(
                [' ', '-', '/', ',', '.', '(', ')'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalise)
            .Where(token => token.Length > 1)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string Normalise(string value) =>
        value.Trim().ToLower(CultureInfo.InvariantCulture);

    private static bool ContainsEither(string left, string right) =>
        !string.IsNullOrWhiteSpace(left) &&
        !string.IsNullOrWhiteSpace(right) &&
        (left.Contains(right, StringComparison.OrdinalIgnoreCase) ||
            right.Contains(left, StringComparison.OrdinalIgnoreCase));

    private static decimal Round(decimal value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static IReadOnlyCollection<string> BuildReasons(
        IReadOnlyCollection<string> matchedSkills,
        IReadOnlyCollection<string> missingRequired,
        decimal titleScore,
        decimal experienceCoverage,
        bool locationMatched,
        bool remote)
    {
        var reasons = new List<string>();
        if (matchedSkills.Count > 0)
        {
            reasons.Add($"Matches {matchedSkills.Count} listed skill(s).");
        }

        if (missingRequired.Count > 0)
        {
            reasons.Add($"Missing {missingRequired.Count} required skill(s).");
        }

        if (titleScore > 0)
        {
            reasons.Add("The role title overlaps with your current or preferred title.");
        }

        reasons.Add(experienceCoverage >= 1m
            ? "Your experience meets the minimum requirement."
            : "Your experience is below the stated minimum.");

        if (remote)
        {
            reasons.Add("The role is remote.");
        }
        else if (locationMatched)
        {
            reasons.Add("The location matches your preference.");
        }

        return reasons;
    }
}
