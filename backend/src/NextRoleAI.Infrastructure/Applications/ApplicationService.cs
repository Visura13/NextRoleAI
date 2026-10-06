using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NextRoleAI.Application.Applications;
using NextRoleAI.Domain.AgentWorkflows;
using NextRoleAI.Domain.Applications;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Applications;

internal sealed class ApplicationService(
    ApplicationDbContext dbContext,
    IApplicationNotificationSender notificationSender,
    TimeProvider timeProvider,
    ILogger<ApplicationService> logger) : IApplicationService
{
    public async Task<ApplicationCommandResult> SubmitAsync(
        string jobSeekerUserId,
        SubmitApplicationInput input,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var job = await dbContext.JobPostings
            .Include(candidate => candidate.CompanyProfile)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == input.JobPostingId &&
                candidate.Status == JobStatus.Published &&
                (candidate.ClosesAtUtc == null || candidate.ClosesAtUtc > now),
                cancellationToken);
        if (job is null)
        {
            return Failure(
                ApplicationCommandError.JobUnavailable,
                "The job is not open for applications.");
        }

        if (await dbContext.JobApplications.AnyAsync(
            application => application.JobSeekerUserId == jobSeekerUserId &&
                application.JobPostingId == input.JobPostingId,
            cancellationToken))
        {
            return Failure(
                ApplicationCommandError.DuplicateApplication,
                "You have already applied for this job.");
        }

        if (input.SourceWorkflowRunId is not null)
        {
            var approvedSource = await dbContext.AgentWorkflowRuns.AnyAsync(
                workflow => workflow.Id == input.SourceWorkflowRunId &&
                    workflow.UserId == jobSeekerUserId &&
                    workflow.ApprovalStatus == AgentApprovalStatus.Approved &&
                    workflow.ShortlistItems.Any(item =>
                        item.JobPostingId == input.JobPostingId && item.IsApproved),
                cancellationToken);
            if (!approvedSource)
            {
                return Failure(
                    ApplicationCommandError.WorkflowNotApproved,
                    "The selected job is not part of an approved workflow shortlist.");
            }
        }

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobSeekerUserId = jobSeekerUserId,
            JobPostingId = job.Id,
            JobPosting = job,
            SourceWorkflowRunId = input.SourceWorkflowRunId,
            CoverNote = input.CoverNote.Trim(),
            Status = ApplicationStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        var statusEvent = CreateEvent(
            application,
            previousStatus: null,
            ApplicationStatus.Submitted,
            jobSeekerUserId,
            ApplicationActorRole.JobSeeker,
            "Application submitted.",
            now);
        var recruiter = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == job.CompanyProfile.RecruiterUserId, cancellationToken);
        var delivery = CreateDelivery(
            application,
            recruiter.Id,
            recruiter.Email!,
            "ApplicationSubmitted",
            now);

        dbContext.JobApplications.Add(application);
        dbContext.ApplicationStatusEvents.Add(statusEvent);
        dbContext.NotificationDeliveries.Add(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);

        await DispatchAsync(
            delivery,
            new ApplicationNotification(
                recruiter.Email!,
                $"New application for {job.Title}",
                $"A Job Seeker submitted an application for {job.Title} at {job.CompanyProfile.Name}. Sign in to NextRoleAI to review it."),
            cancellationToken);

        return ApplicationCommandResult.Success(
            await ToResultAsync(application, cancellationToken));
    }

    public Task<ApplicationListResult> GetForJobSeekerAsync(
        string jobSeekerUserId,
        ApplicationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetPageAsync(
            dbContext.JobApplications.Where(application =>
                application.JobSeekerUserId == jobSeekerUserId),
            status,
            page,
            pageSize,
            cancellationToken);

    public async Task<JobApplicationResult?> GetForJobSeekerAsync(
        string jobSeekerUserId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await BaseQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == applicationId &&
                candidate.JobSeekerUserId == jobSeekerUserId,
                cancellationToken);
        return application is null
            ? null
            : await ToResultAsync(application, cancellationToken);
    }

    public async Task<ApplicationCommandResult> WithdrawAsync(
        string jobSeekerUserId,
        Guid applicationId,
        string note,
        CancellationToken cancellationToken = default)
    {
        var application = await FindForJobSeekerAsync(
            jobSeekerUserId, applicationId, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        if (!ApplicationStatusTransitions.CanJobSeekerWithdraw(application.Status))
        {
            return InvalidTransition(application.Status, ApplicationStatus.Withdrawn);
        }

        return await ChangeStatusAsync(
            application,
            ApplicationStatus.Withdrawn,
            jobSeekerUserId,
            ApplicationActorRole.JobSeeker,
            note,
            "ApplicationWithdrawn",
            application.JobPosting.CompanyProfile.RecruiterUserId,
            $"Application withdrawn for {application.JobPosting.Title}",
            $"An application for {application.JobPosting.Title} was withdrawn. Sign in to NextRoleAI to view the audit history.",
            cancellationToken);
    }

    public async Task<ApplicationCommandResult> RespondAsync(
        string jobSeekerUserId,
        Guid applicationId,
        string note,
        CancellationToken cancellationToken = default)
    {
        var application = await FindForJobSeekerAsync(
            jobSeekerUserId, applicationId, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        if (!ApplicationStatusTransitions.CanJobSeekerRespond(application.Status))
        {
            return InvalidTransition(application.Status, ApplicationStatus.Submitted);
        }

        return await ChangeStatusAsync(
            application,
            ApplicationStatus.Submitted,
            jobSeekerUserId,
            ApplicationActorRole.JobSeeker,
            note,
            "ApplicationInformationProvided",
            application.JobPosting.CompanyProfile.RecruiterUserId,
            $"Additional information for {application.JobPosting.Title}",
            $"A candidate supplied additional information for {application.JobPosting.Title}. Sign in to NextRoleAI to review it.",
            cancellationToken);
    }

    public Task<ApplicationListResult> GetForRecruiterAsync(
        string recruiterUserId,
        ApplicationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetPageAsync(
            dbContext.JobApplications.Where(application =>
                application.JobPosting.CompanyProfile.RecruiterUserId == recruiterUserId),
            status,
            page,
            pageSize,
            cancellationToken);

    public async Task<JobApplicationResult?> GetForRecruiterAsync(
        string recruiterUserId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await BaseQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == applicationId &&
                candidate.JobPosting.CompanyProfile.RecruiterUserId == recruiterUserId,
                cancellationToken);
        return application is null
            ? null
            : await ToResultAsync(application, cancellationToken);
    }

    public async Task<ApplicationCommandResult> DecideAsync(
        string recruiterUserId,
        Guid applicationId,
        ApplicationStatus status,
        string note,
        CancellationToken cancellationToken = default)
    {
        var application = await BaseQuery()
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == applicationId &&
                candidate.JobPosting.CompanyProfile.RecruiterUserId == recruiterUserId,
                cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        if (!ApplicationStatusTransitions.CanRecruiterMove(application.Status, status))
        {
            return InvalidTransition(application.Status, status);
        }

        return await ChangeStatusAsync(
            application,
            status,
            recruiterUserId,
            ApplicationActorRole.Recruiter,
            note,
            $"Application{status}",
            application.JobSeekerUserId,
            $"Application update: {application.JobPosting.Title}",
            $"Your application for {application.JobPosting.Title} at {application.JobPosting.CompanyProfile.Name} is now {Readable(status)}. Sign in to NextRoleAI to view the recruiter's note.",
            cancellationToken);
    }

    private async Task<ApplicationCommandResult> ChangeStatusAsync(
        JobApplication application,
        ApplicationStatus target,
        string actorUserId,
        ApplicationActorRole actorRole,
        string note,
        string eventType,
        string recipientUserId,
        string subject,
        string message,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var previous = application.Status;
        application.Status = target;
        application.UpdatedAtUtc = now;
        var statusEvent = CreateEvent(
            application,
            previous,
            target,
            actorUserId,
            actorRole,
            note.Trim(),
            now);
        var recipient = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == recipientUserId, cancellationToken);
        var delivery = CreateDelivery(
            application,
            recipient.Id,
            recipient.Email!,
            eventType,
            now);

        dbContext.ApplicationStatusEvents.Add(statusEvent);
        dbContext.NotificationDeliveries.Add(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);
        await DispatchAsync(
            delivery,
            new ApplicationNotification(recipient.Email!, subject, message),
            cancellationToken);

        return ApplicationCommandResult.Success(
            await ToResultAsync(application, cancellationToken));
    }

    private async Task DispatchAsync(
        NotificationDelivery delivery,
        ApplicationNotification notification,
        CancellationToken cancellationToken)
    {
        NotificationSendResult result;
        try
        {
            result = await notificationSender.SendAsync(notification, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Application notification {EventType} could not be delivered.",
                delivery.EventType);
            result = NotificationSendResult.Failed(
                delivery.Provider,
                "Unexpected provider failure.");
        }
        delivery.Provider = result.Provider;
        delivery.ProviderMessageId = result.ProviderMessageId;
        delivery.FailureReason = result.FailureReason;
        delivery.AttemptCount++;
        delivery.AttemptedAtUtc = timeProvider.GetUtcNow();
        delivery.Status = result.Sent
            ? NotificationDeliveryStatus.Sent
            : result.Skipped
                ? NotificationDeliveryStatus.Skipped
                : NotificationDeliveryStatus.Failed;
        await dbContext.SaveChangesAsync(
            cancellationToken.IsCancellationRequested
                ? CancellationToken.None
                : cancellationToken);
    }

    private async Task<ApplicationListResult> GetPageAsync(
        IQueryable<JobApplication> source,
        ApplicationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (status is not null)
        {
            source = source.Where(application => application.Status == status);
        }

        var totalCount = await source.CountAsync(cancellationToken);
        var applications = await source
            .Include(application => application.JobPosting)
                .ThenInclude(job => job.CompanyProfile)
            .Include(application => application.StatusEvents)
            .Include(application => application.NotificationDeliveries)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(application => application.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var results = new List<JobApplicationResult>(applications.Count);
        foreach (var application in applications)
        {
            results.Add(await ToResultAsync(application, cancellationToken));
        }

        return new ApplicationListResult(results, page, pageSize, totalCount);
    }

    private IQueryable<JobApplication> BaseQuery() =>
        dbContext.JobApplications
            .Include(application => application.JobPosting)
                .ThenInclude(job => job.CompanyProfile)
            .Include(application => application.StatusEvents)
            .Include(application => application.NotificationDeliveries)
            .AsSplitQuery();

    private Task<JobApplication?> FindForJobSeekerAsync(
        string jobSeekerUserId,
        Guid applicationId,
        CancellationToken cancellationToken) =>
        BaseQuery().SingleOrDefaultAsync(candidate =>
            candidate.Id == applicationId &&
            candidate.JobSeekerUserId == jobSeekerUserId,
            cancellationToken);

    private async Task<JobApplicationResult> ToResultAsync(
        JobApplication application,
        CancellationToken cancellationToken)
    {
        var applicant = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == application.JobSeekerUserId)
            .Select(user => new
            {
                Name = user.FirstName + " " + user.LastName,
                user.Email
            })
            .SingleAsync(cancellationToken);

        return new JobApplicationResult(
            application.Id,
            application.JobPostingId,
            application.JobPosting.Title,
            application.JobPosting.CompanyProfile.Name,
            application.JobPosting.Location,
            application.JobSeekerUserId,
            applicant.Name,
            applicant.Email!,
            application.CoverNote,
            application.Status,
            application.SourceWorkflowRunId,
            application.StatusEvents
                .OrderByDescending(statusEvent => statusEvent.CreatedAtUtc)
                .Select(statusEvent => new ApplicationStatusEventResult(
                    statusEvent.Id,
                    statusEvent.PreviousStatus,
                    statusEvent.NewStatus,
                    statusEvent.ActorRole,
                    statusEvent.Note,
                    statusEvent.CreatedAtUtc))
                .ToArray(),
            application.NotificationDeliveries
                .OrderByDescending(delivery => delivery.CreatedAtUtc)
                .Select(delivery => new NotificationDeliveryResult(
                    delivery.EventType,
                    delivery.Provider,
                    delivery.Status,
                    delivery.AttemptCount,
                    delivery.CreatedAtUtc,
                    delivery.AttemptedAtUtc))
                .ToArray(),
            application.CreatedAtUtc,
            application.UpdatedAtUtc);
    }

    private static ApplicationStatusEvent CreateEvent(
        JobApplication application,
        ApplicationStatus? previousStatus,
        ApplicationStatus newStatus,
        string actorUserId,
        ApplicationActorRole actorRole,
        string note,
        DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        JobApplicationId = application.Id,
        JobApplication = application,
        PreviousStatus = previousStatus,
        NewStatus = newStatus,
        ActorUserId = actorUserId,
        ActorRole = actorRole,
        Note = note,
        CreatedAtUtc = now
    };

    private static NotificationDelivery CreateDelivery(
        JobApplication application,
        string recipientUserId,
        string recipientEmail,
        string eventType,
        DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        JobApplicationId = application.Id,
        JobApplication = application,
        RecipientUserId = recipientUserId,
        RecipientEmail = recipientEmail,
        EventType = eventType,
        Provider = "Resend",
        Status = NotificationDeliveryStatus.Pending,
        CreatedAtUtc = now
    };

    private static string Readable(ApplicationStatus status) => status switch
    {
        ApplicationStatus.InReview => "in review",
        ApplicationStatus.MoreInformationRequested => "waiting for more information",
        ApplicationStatus.Shortlisted => "shortlisted",
        ApplicationStatus.Rejected => "not selected",
        ApplicationStatus.Withdrawn => "withdrawn",
        _ => "submitted"
    };

    private static ApplicationCommandResult InvalidTransition(
        ApplicationStatus current,
        ApplicationStatus target) => Failure(
            ApplicationCommandError.InvalidTransition,
            $"An application in {current} cannot transition to {target}.");

    private static ApplicationCommandResult NotFound() => Failure(
        ApplicationCommandError.NotFound,
        "The application was not found.");

    private static ApplicationCommandResult Failure(
        ApplicationCommandError error,
        string message) => ApplicationCommandResult.Failure(error, message);
}
