using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Application.Applications;

public static class ApplicationStatusTransitions
{
    public static bool CanRecruiterMove(ApplicationStatus current, ApplicationStatus target) =>
        target switch
        {
            ApplicationStatus.InReview => current == ApplicationStatus.Submitted,
            ApplicationStatus.MoreInformationRequested =>
                current is ApplicationStatus.Submitted or ApplicationStatus.InReview,
            ApplicationStatus.Shortlisted or ApplicationStatus.Rejected =>
                current is ApplicationStatus.Submitted or
                    ApplicationStatus.InReview or
                    ApplicationStatus.MoreInformationRequested,
            _ => false
        };

    public static bool CanJobSeekerWithdraw(ApplicationStatus current) =>
        current is ApplicationStatus.Submitted or
            ApplicationStatus.InReview or
            ApplicationStatus.MoreInformationRequested;

    public static bool CanJobSeekerRespond(ApplicationStatus current) =>
        current == ApplicationStatus.MoreInformationRequested;
}
