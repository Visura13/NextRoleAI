namespace NextRoleAI.Application.Authentication;

public static class RoleNames
{
    public const string JobSeeker = "JobSeeker";
    public const string Recruiter = "Recruiter";

    public static readonly IReadOnlyCollection<string> All = [JobSeeker, Recruiter];
}
