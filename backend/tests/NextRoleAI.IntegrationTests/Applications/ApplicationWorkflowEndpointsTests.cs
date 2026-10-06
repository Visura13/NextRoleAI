using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NextRoleAI.Api.Contracts.Applications;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.Api.Contracts.Jobs;
using NextRoleAI.Api.Contracts.Profiles;
using NextRoleAI.Application.Applications;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Domain.Applications;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.IntegrationTests.Infrastructure;

namespace NextRoleAI.IntegrationTests.Applications;

public sealed class ApplicationWorkflowEndpointsTests(NextRoleAIApiFactory factory)
    : IClassFixture<NextRoleAIApiFactory>
{
    private const string ValidPassword = "StrongPassword1!";
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [PostgresFact]
    public async Task ApplicationLifecycle_IsOwnedAuditedAndVisibleAcrossRoles()
    {
        using var recruiterClient = factory.CreateClient();
        var recruiter = await RegisterAsync(recruiterClient, "recruiter");
        Authorize(recruiterClient, recruiter.AccessToken);
        await CreateCompanyAsync(recruiterClient, "Application Labs");
        var job = await CreatePublishedJobAsync(recruiterClient);

        using var seekerClient = factory.CreateClient();
        var seeker = await RegisterAsync(seekerClient, "job-seeker");
        Authorize(seekerClient, seeker.AccessToken);
        var submitResponse = await seekerClient.PostAsJsonAsync(
            "/api/applications",
            new SubmitApplicationRequest(
                job.Id,
                "I have relevant platform engineering experience and would welcome an interview.",
                null));
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var submitted = await submitResponse.Content
            .ReadFromJsonAsync<JobApplicationResult>(JsonOptions);
        Assert.NotNull(submitted);
        Assert.Equal(ApplicationStatus.Submitted, submitted.Status);
        Assert.Single(submitted.StatusHistory);
        Assert.Equal(NotificationDeliveryStatus.Skipped, Assert.Single(
            submitted.NotificationDeliveries).Status);

        var duplicateResponse = await seekerClient.PostAsJsonAsync(
            "/api/applications",
            new SubmitApplicationRequest(
                job.Id,
                "This second submission must be rejected by the unique application rule.",
                null));
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        using var otherSeekerClient = factory.CreateClient();
        var otherSeeker = await RegisterAsync(otherSeekerClient, "job-seeker");
        Authorize(otherSeekerClient, otherSeeker.AccessToken);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await otherSeekerClient.GetAsync($"/api/applications/{submitted.Id}"))
                .StatusCode);

        using var otherRecruiterClient = factory.CreateClient();
        var otherRecruiter = await RegisterAsync(otherRecruiterClient, "recruiter");
        Authorize(otherRecruiterClient, otherRecruiter.AccessToken);
        await CreateCompanyAsync(otherRecruiterClient, "Other Recruiter");
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await otherRecruiterClient.GetAsync(
                $"/api/recruiter/applications/{submitted.Id}")).StatusCode);

        var inReview = await DecideAsync(
            recruiterClient,
            submitted.Id,
            ApplicationStatus.InReview,
            "The application passed the initial review.");
        Assert.Equal(ApplicationStatus.InReview, inReview.Status);

        var informationRequested = await DecideAsync(
            recruiterClient,
            submitted.Id,
            ApplicationStatus.MoreInformationRequested,
            "Please describe one production incident you resolved.");
        Assert.Equal(ApplicationStatus.MoreInformationRequested, informationRequested.Status);

        var response = await seekerClient.PostAsJsonAsync(
            $"/api/applications/{submitted.Id}/respond",
            new ApplicationNoteRequest(
                "I restored a failed deployment by rolling back and then correcting its migration."));
        response.EnsureSuccessStatusCode();
        var resubmitted = await response.Content
            .ReadFromJsonAsync<JobApplicationResult>(JsonOptions);
        Assert.NotNull(resubmitted);
        Assert.Equal(ApplicationStatus.Submitted, resubmitted.Status);

        var shortlisted = await DecideAsync(
            recruiterClient,
            submitted.Id,
            ApplicationStatus.Shortlisted,
            "Your experience matches the role. We will contact you about an interview.");
        Assert.Equal(ApplicationStatus.Shortlisted, shortlisted.Status);
        Assert.Equal(5, shortlisted.StatusHistory.Count);
        Assert.All(shortlisted.NotificationDeliveries, delivery =>
            Assert.Equal(NotificationDeliveryStatus.Skipped, delivery.Status));

        var seekerList = await seekerClient.GetFromJsonAsync<ApplicationListResult>(
            "/api/applications?pageSize=20",
            JsonOptions);
        Assert.NotNull(seekerList);
        Assert.Equal(ApplicationStatus.Shortlisted, Assert.Single(seekerList.Items).Status);

        var invalidResponse = await recruiterClient.PatchAsJsonAsync(
            $"/api/recruiter/applications/{submitted.Id}/status",
            new ApplicationDecisionRequest(
                ApplicationStatus.Rejected,
                "A terminal shortlist cannot be reversed."));
        Assert.Equal(HttpStatusCode.Conflict, invalidResponse.StatusCode);
    }

    private static async Task<JobApplicationResult> DecideAsync(
        HttpClient client,
        Guid applicationId,
        ApplicationStatus status,
        string note)
    {
        var response = await client.PatchAsJsonAsync(
            $"/api/recruiter/applications/{applicationId}/status",
            new ApplicationDecisionRequest(status, note));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobApplicationResult>(JsonOptions))!;
    }

    private static async Task<JobResult> CreatePublishedJobAsync(HttpClient client)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/recruiter/jobs",
            new UpsertJobRequest(
                "Platform Engineer",
                "Build and operate a reliable platform for job seekers and recruiters.",
                "Colombo",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                2,
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(30),
                [new JobSkillRequest("C#", true)]));
        createResponse.EnsureSuccessStatusCode();
        var job = (await createResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions))!;
        var publishResponse = await client.PatchAsJsonAsync(
            $"/api/recruiter/jobs/{job.Id}/status",
            new ChangeJobStatusRequest(JobStatus.Published));
        publishResponse.EnsureSuccessStatusCode();
        return job;
    }

    private static async Task<AuthenticationResponse> RegisterAsync(
        HttpClient client,
        string role)
    {
        var response = await client.PostAsJsonAsync(
            role == "job-seeker"
                ? "/api/auth/register/job-seeker"
                : "/api/auth/register/recruiter",
            new RegisterRequest(
                $"{role}-{Guid.NewGuid():N}@example.test",
                ValidPassword,
                "Test",
                "User"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthenticationResponse>(JsonOptions))!;
    }

    private static async Task CreateCompanyAsync(HttpClient client, string name)
    {
        var response = await client.PutAsJsonAsync(
            "/api/profiles/company",
            new UpdateCompanyProfileRequest(
                name,
                "A company used by the application workflow tests.",
                "Colombo",
                null));
        response.EnsureSuccessStatusCode();
    }

    private static void Authorize(HttpClient client, string accessToken) =>
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
