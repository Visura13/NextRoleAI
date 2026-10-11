using System.Text.RegularExpressions;

namespace NextRoleAI.AgenticAI;

public sealed partial class ObjectiveGuard
{
    private static readonly string[] ForbiddenPhrases =
    [
        "ignore previous",
        "ignore all instructions",
        "system prompt",
        "developer message",
        "reveal your prompt",
        "bypass safety",
        "disable validation",
        "call any tool",
        "execute sql",
        "drop table",
        "act as system"
    ];

    public string ValidateAndNormalise(string objective)
    {
        var value = WhitespaceRegex().Replace(objective, " ").Trim();
        if (value.Length is < 20 or > 500)
        {
            throw new AgentExecutionException(
                "InvalidObjective",
                "The objective must contain between 20 and 500 characters.");
        }

        if (value.Any(char.IsControl) ||
            ForbiddenPhrases.Any(phrase =>
                value.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
        {
            throw new AgentExecutionException(
                "UnsafeObjective",
                "The objective contains instruction-like content that cannot be processed safely.");
        }

        if (!JobSearchIntentRegex().IsMatch(value) ||
            !JobSearchSubjectRegex().IsMatch(value))
        {
            throw new AgentExecutionException(
                "OutOfScopeObjective",
                "This workflow only supports job-search objectives. Ask it to find, recommend, rank, or compare jobs or roles for your profile.");
        }

        return value;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(
        @"\b(find|search|recommend|suggest|show|list|rank|compare|discover|identify|match|shortlist|tell|best|top|suitable)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex JobSearchIntentRegex();

    [GeneratedRegex(
        @"\b(job|jobs|role|roles|position|positions|opportunity|opportunities|vacancy|vacancies|posting|postings|employment|career|careers|work)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex JobSearchSubjectRegex();
}
