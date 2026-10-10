namespace NextRoleAI.Application.Cvs;

public static class CvEducationPolicy
{
    private static readonly string[] HigherEducationMarkers =
    [
        "certificate", "diploma", "higher national diploma", "hnd",
        "degree", "bachelor", "bsc", "b.a", "beng", "master", "msc",
        "mba", "postgraduate", "phd", "doctorate", "undergraduate"
    ];

    private static readonly string[] SecondaryEducationMarkers =
    [
        "gce", "ordinary level", "advanced level", "o/l", "a/l",
        "high school", "secondary school", "college school"
    ];

    public static bool IsSupported(
        string? qualification,
        string? fieldOfStudy = null,
        string? institution = null)
    {
        var combined = $"{qualification} {fieldOfStudy} {institution}";
        return HigherEducationMarkers.Any(marker =>
                combined.Contains(marker, StringComparison.OrdinalIgnoreCase)) &&
            !SecondaryEducationMarkers.Any(marker =>
                combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
