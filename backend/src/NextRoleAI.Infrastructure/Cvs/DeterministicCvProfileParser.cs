using System.Text.RegularExpressions;
using NextRoleAI.Application.Cvs;

namespace NextRoleAI.Infrastructure.Cvs;

internal sealed partial class DeterministicCvProfileParser : ICvProfileParser
{
    private const int MaximumCurrentJobTitleLength = 150;

    private static readonly string[] KnownSkills =
    [
        "ASP.NET Core", "C#", "Dart", "Docker", "Entity Framework Core",
        "Flutter", "Git", "Java", "JavaScript", "Kotlin", "Node.js",
        "PostgreSQL", "Python", "React", "REST", "SQL", "TypeScript"
    ];

    private static readonly string[] TitleWords =
    [
        "developer", "engineer", "designer", "analyst", "manager",
        "architect", "consultant", "intern", "specialist", "lead"
    ];

    public ExtractedCvProfile Parse(string text)
    {
        var normalised = WhitespaceRegex().Replace(text, " ").Trim();
        var lines = text.Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 1)
            .ToArray();
        var candidateName = lines.FirstOrDefault(IsLikelyName) ?? string.Empty;
        var email = EmailRegex().Match(text).Value;
        var phone = PhoneRegex().Match(text).Value.Trim();
        var yearsMatch = YearsRegex().Match(text);
        var years = yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out var value)
            ? Math.Clamp(value, 0, 80)
            : 0;
        var currentTitle = lines.FirstOrDefault(line =>
            line.Length <= MaximumCurrentJobTitleLength &&
            TitleWords.Any(word => line.Contains(word, StringComparison.OrdinalIgnoreCase))) ??
            string.Empty;
        var skills = KnownSkills
            .Where(skill => ContainsSkill(text, skill))
            .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var summary = normalised.Length <= 600
            ? normalised
            : normalised[..600].TrimEnd() + "…";

        return new ExtractedCvProfile(
            candidateName,
            email,
            phone,
            string.Empty,
            currentTitle,
            summary,
            years,
            skills);
    }

    private static bool IsLikelyName(string value) =>
        value.Length <= 100 &&
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length is >= 2 and <= 5 &&
        value.All(character => char.IsLetter(character) || char.IsWhiteSpace(character) ||
            character is '-' or '\'');

    private static bool ContainsSkill(string text, string skill)
    {
        var pattern = skill switch
        {
            "C#" => @"(?<![A-Za-z0-9])C#(?![A-Za-z0-9])",
            "Node.js" => @"(?<![A-Za-z0-9])Node(?:\.js|JS)?(?![A-Za-z0-9])",
            _ => $@"(?<![A-Za-z0-9]){Regex.Escape(skill)}(?![A-Za-z0-9])"
        };
        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
    }

    [GeneratedRegex(@"[\w.!#$%&'*+/=?^`{|}~-]+@[\w-]+(?:\.[\w-]+)+")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?:\+?\d[\d ()-]{7,}\d)")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"(\d{1,2})\+?\s*(?:years?|yrs?)", RegexOptions.IgnoreCase)]
    private static partial Regex YearsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
