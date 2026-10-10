using System.Text.RegularExpressions;
using NextRoleAI.Application.Cvs;

namespace NextRoleAI.Infrastructure.Cvs;

internal static partial class CvAnalysisValidator
{
    internal static CvAnalysis Validate(
        RawCvAnalysis raw,
        ExtractedCvProfile fallback,
        string sourceText,
        string model,
        DateTimeOffset analyzedAt)
    {
        var skills = (raw.Skills ?? [])
            .Where(skill => HasSupportedEvidence(sourceText, skill.Evidence))
            .Select(skill => Clean(skill.Name, 100))
            .Where(skill => skill.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();
        if (skills.Length == 0)
        {
            skills = fallback.Skills.ToArray();
        }

        var education = (raw.Education ?? [])
            .Where(item => IsHigherEducation(item) &&
                HasSupportedEvidence(sourceText, item.Evidence))
            .Select(item => new CvEducationItem(
                Clean(item.Qualification, 150),
                Clean(item.FieldOfStudy, 200),
                Clean(item.Institution, 200),
                Clean(item.Status, 50),
                Clean(item.Evidence, 500),
                Math.Clamp(item.Confidence, 0m, 1m)))
            .Where(item => item.Qualification.Length > 0)
            .DistinctBy(
                item => $"{item.Qualification}|{item.Institution}",
                StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        var assessment = ValidateAssessment(raw.QualityAssessment);
        return new CvAnalysis(
            new ExtractedCvProfile(
                fallback.CandidateName,
                fallback.Email,
                fallback.Phone,
                fallback.Location,
                HasSupportedEvidence(sourceText, raw.CurrentJobTitleEvidence)
                    ? Choose(raw.CurrentJobTitle, fallback.CurrentJobTitle, 150)
                    : Clean(fallback.CurrentJobTitle, 150),
                Choose(raw.ProfessionalSummary, fallback.ProfessionalSummary, 2000),
                Math.Clamp(raw.YearsExperience ?? fallback.YearsExperience, 0, 80),
                skills,
                education),
            assessment,
            "ai",
            Clean(model, 100),
            AiCvAnalyzer.PromptVersion,
            analyzedAt);
    }

    private static CvQualityAssessment? ValidateAssessment(RawCvQualityAssessment? raw)
    {
        if (raw is null)
        {
            return null;
        }

        if (raw.CompletenessScore is null ||
            raw.ClarityScore is null ||
            raw.SkillsEvidenceScore is null ||
            raw.ImpactScore is null ||
            raw.AtsReadabilityScore is null)
        {
            return null;
        }

        var completeness = Math.Clamp(raw.CompletenessScore.Value, 0, 100);
        var clarity = Math.Clamp(raw.ClarityScore.Value, 0, 100);
        var skillsEvidence = Math.Clamp(raw.SkillsEvidenceScore.Value, 0, 100);
        var impact = Math.Clamp(raw.ImpactScore.Value, 0, 100);
        var atsReadability = Math.Clamp(raw.AtsReadabilityScore.Value, 0, 100);
        var overall = (int)Math.Round(
            (completeness + clarity + skillsEvidence + impact + atsReadability) / 5m,
            MidpointRounding.AwayFromZero);
        return new CvQualityAssessment(
            overall,
            completeness,
            clarity,
            skillsEvidence,
            impact,
            atsReadability,
            CleanFeedback(raw.Strengths),
            CleanFeedback(raw.Improvements));
    }

    private static IReadOnlyCollection<string> CleanFeedback(
        IReadOnlyCollection<string>? values) =>
        (values ?? [])
            .Select(value => Clean(value, 300))
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

    private static bool IsHigherEducation(RawCvEducation item)
        => CvEducationPolicy.IsSupported(
            item.Qualification,
            item.FieldOfStudy,
            item.Institution);

    private static bool HasSupportedEvidence(string sourceText, string? evidence)
    {
        var cleanedEvidence = NormaliseWhitespace(evidence);
        return cleanedEvidence.Length >= 2 &&
            NormaliseWhitespace(sourceText).Contains(
                cleanedEvidence,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string Choose(string? preferred, string fallback, int maximumLength)
    {
        var cleaned = Clean(preferred, maximumLength);
        return cleaned.Length > 0 ? cleaned : Clean(fallback, maximumLength);
    }

    private static string Clean(string? value, int maximumLength)
    {
        var cleaned = NormaliseWhitespace(value);
        return cleaned.Length <= maximumLength
            ? cleaned
            : cleaned[..maximumLength].TrimEnd();
    }

    private static string NormaliseWhitespace(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : WhitespaceRegex().Replace(value, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
