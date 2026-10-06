using NextRoleAI.Application.Applications;
using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Api.Tests.Applications;

public sealed class ApplicationStatusTransitionsTests
{
    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.InReview)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.MoreInformationRequested)]
    [InlineData(ApplicationStatus.MoreInformationRequested, ApplicationStatus.Rejected)]
    public void RecruiterTransitions_AllowExpectedMoves(
        ApplicationStatus current,
        ApplicationStatus target) =>
        Assert.True(ApplicationStatusTransitions.CanRecruiterMove(current, target));

    [Theory]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.InReview)]
    [InlineData(ApplicationStatus.Withdrawn, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.Submitted)]
    public void RecruiterTransitions_RejectTerminalOrOwnerOnlyMoves(
        ApplicationStatus current,
        ApplicationStatus target) =>
        Assert.False(ApplicationStatusTransitions.CanRecruiterMove(current, target));

    [Theory]
    [InlineData(ApplicationStatus.Submitted, true)]
    [InlineData(ApplicationStatus.InReview, true)]
    [InlineData(ApplicationStatus.MoreInformationRequested, true)]
    [InlineData(ApplicationStatus.Shortlisted, false)]
    [InlineData(ApplicationStatus.Rejected, false)]
    public void Withdrawal_OnlyAllowsActiveApplications(
        ApplicationStatus current,
        bool expected) =>
        Assert.Equal(expected, ApplicationStatusTransitions.CanJobSeekerWithdraw(current));

    [Fact]
    public void Response_OnlyFollowsInformationRequest()
    {
        Assert.True(ApplicationStatusTransitions.CanJobSeekerRespond(
            ApplicationStatus.MoreInformationRequested));
        Assert.False(ApplicationStatusTransitions.CanJobSeekerRespond(
            ApplicationStatus.InReview));
    }
}
