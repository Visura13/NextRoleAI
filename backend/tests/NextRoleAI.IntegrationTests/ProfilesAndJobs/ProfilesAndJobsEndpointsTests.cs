using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.Api.Contracts.Jobs;
using NextRoleAI.Api.Contracts.Profiles;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Profiles;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.IntegrationTests.Infrastructure;

namespace NextRoleAI.IntegrationTests.ProfilesAndJobs;

public sealed class ProfilesAndJobsEndpointsTests(NextRoleAIApiFactory factory)
    : IClassFixture<NextRoleAIApiFactory>
{
    private const string ValidPassword = "StrongPassword1!";

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [PostgresFact]
    public async Task JobSeeker_CanMaintainProfile_WithoutRecruiterAccess()
    {
        using var client = factory.CreateClient();
        var authentication = await RegisterAsync(client, "job-seeker");
        Authorize(client, authentication.AccessToken);

        var updateResponse = await client.PutAsJsonAsync(
            "/api/profiles/job-seeker",
            new UpdateJobSeekerProfileRequest(
                "Backend Developer",
                "Builds reliable APIs.",
                "Colombo",
                "Software Engineer",
                2,
                ["C#", "PostgreSQL", "c#"]));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var profile = await client.GetFromJsonAsync<JobSeekerProfileResult>(
            "/api/profiles/job-seeker",
            JsonOptions);

        Assert.NotNull(profile);
        Assert.Equal("Backend Developer", profile.Headline);
        Assert.Equal(2, profile.Skills.Count);
        Assert.Contains("C#", profile.Skills);
        Assert.Contains("PostgreSQL", profile.Skills);

        var replacementResponse = await client.PutAsJsonAsync(
            "/api/profiles/job-seeker",
            new UpdateJobSeekerProfileRequest(
                "Senior Backend Developer",
                "Builds reliable APIs and distributed systems.",
                "Kandy",
                "Senior Software Engineer",
                4,
                ["ASP.NET Core", "Docker"]));
        Assert.Equal(HttpStatusCode.OK, replacementResponse.StatusCode);

        var replacement = await replacementResponse.Content
            .ReadFromJsonAsync<JobSeekerProfileResult>(JsonOptions);
        Assert.NotNull(replacement);
        Assert.Equal(2, replacement.Skills.Count);
        Assert.DoesNotContain("C#", replacement.Skills);
        Assert.Contains("Docker", replacement.Skills);

        var recruiterProfileResponse = await client.GetAsync("/api/profiles/company");
        Assert.Equal(HttpStatusCode.Forbidden, recruiterProfileResponse.StatusCode);
    }

    [PostgresFact]
    public async Task Recruiter_CanManageJobs_AndPublicCatalogOnlyShowsOpenPublishedJobs()
    {
        using var client = factory.CreateClient();
        var authentication = await RegisterAsync(client, "recruiter");
        Authorize(client, authentication.AccessToken);

        var jobRequest = CreateJobRequest("Platform Engineer");
        var missingCompanyResponse = await client.PostAsJsonAsync(
            "/api/recruiter/jobs",
            jobRequest);
        Assert.Equal(HttpStatusCode.Conflict, missingCompanyResponse.StatusCode);

        var companyResponse = await client.PutAsJsonAsync(
            "/api/profiles/company",
            new UpdateCompanyProfileRequest(
                "NextRole Labs",
                "Builds hiring technology.",
                "Colombo",
                "https://example.test"));
        Assert.Equal(HttpStatusCode.OK, companyResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync(
            "/api/recruiter/jobs",
            jobRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(JobStatus.Draft, created.Status);

        client.DefaultRequestHeaders.Authorization = null;
        var hiddenDraftResponse = await client.GetAsync($"/api/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenDraftResponse.StatusCode);

        Authorize(client, authentication.AccessToken);
        var publishResponse = await client.PatchAsJsonAsync(
            $"/api/recruiter/jobs/{created.Id}/status",
            new ChangeJobStatusRequest(JobStatus.Published));
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var search = await client.GetFromJsonAsync<PagedResult<JobResult>>(
            "/api/jobs?search=platform&location=Colombo&employmentType=FullTime" +
            "&workMode=Hybrid&skill=C%23&sortBy=title&page=1&pageSize=10",
            JsonOptions);
        Assert.NotNull(search);
        Assert.Contains(search.Items, job => job.Id == created.Id);

        Authorize(client, authentication.AccessToken);
        var deletePublishedResponse = await client.DeleteAsync(
            $"/api/recruiter/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deletePublishedResponse.StatusCode);

        var closeResponse = await client.PatchAsJsonAsync(
            $"/api/recruiter/jobs/{created.Id}/status",
            new ChangeJobStatusRequest(JobStatus.Closed));
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var hiddenClosedResponse = await client.GetAsync($"/api/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenClosedResponse.StatusCode);

        Authorize(client, authentication.AccessToken);
        var draftResponse = await client.PostAsJsonAsync(
            "/api/recruiter/jobs",
            CreateJobRequest("API Engineer"));
        var draft = await draftResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions);
        Assert.NotNull(draft);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/recruiter/jobs/{draft.Id}",
            CreateJobRequest("Senior API Engineer"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Senior API Engineer", updated.Title);

        var deleteDraftResponse = await client.DeleteAsync($"/api/recruiter/jobs/{draft.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteDraftResponse.StatusCode);

        var deletedResponse = await client.GetAsync($"/api/recruiter/jobs/{draft.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedResponse.StatusCode);
    }

    [PostgresFact]
    public async Task Recruiter_CannotAccessAnotherRecruitersJob()
    {
        using var ownerClient = factory.CreateClient();
        var owner = await RegisterAsync(ownerClient, "recruiter");
        Authorize(ownerClient, owner.AccessToken);
        await CreateCompanyAsync(ownerClient, "Owner Company");

        var createResponse = await ownerClient.PostAsJsonAsync(
            "/api/recruiter/jobs",
            CreateJobRequest("Private Draft"));
        var job = await createResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions);
        Assert.NotNull(job);

        using var otherClient = factory.CreateClient();
        var other = await RegisterAsync(otherClient, "recruiter");
        Authorize(otherClient, other.AccessToken);
        await CreateCompanyAsync(otherClient, "Other Company");

        var getResponse = await otherClient.GetAsync($"/api/recruiter/jobs/{job.Id}");
        var updateResponse = await otherClient.PutAsJsonAsync(
            $"/api/recruiter/jobs/{job.Id}",
            CreateJobRequest("Taken Over"));

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }

    private static async Task<AuthenticationResponse> RegisterAsync(
        HttpClient client,
        string role)
    {
        var endpoint = role == "job-seeker"
            ? "/api/auth/register/job-seeker"
            : "/api/auth/register/recruiter";
        var email = $"{role}-{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync(
            endpoint,
            new RegisterRequest(email, ValidPassword, "Test", "User"));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthenticationResponse>(JsonOptions))!;
    }

    private static async Task CreateCompanyAsync(HttpClient client, string name)
    {
        var response = await client.PutAsJsonAsync(
            "/api/profiles/company",
            new UpdateCompanyProfileRequest(
                name,
                "A test company.",
                "Colombo",
                null));
        response.EnsureSuccessStatusCode();
    }

    private static UpsertJobRequest CreateJobRequest(string title) =>
        new(
            title,
            "Design and build a reliable platform for job seekers.",
            "Colombo",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            2,
            150000,
            250000,
            "LKR",
            DateTimeOffset.UtcNow.AddDays(30),
            [new JobSkillRequest("C#", true), new JobSkillRequest("PostgreSQL", false)]);

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
