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

        return value;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
