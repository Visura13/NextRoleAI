using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.Api.Contracts.Cvs;
using NextRoleAI.Api.Contracts.Jobs;
using NextRoleAI.Api.Contracts.Profiles;
using NextRoleAI.Api.Contracts.Recommendations;
using NextRoleAI.Application.Cvs;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.IntegrationTests.Infrastructure;

namespace NextRoleAI.IntegrationTests.Cvs;

public sealed class CvAndRecommendationEndpointsTests(NextRoleAIApiFactory factory)
    : IClassFixture<NextRoleAIApiFactory>
{
    private const string ValidPassword = "StrongPassword1!";

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [PostgresFact]
    public async Task JobSeeker_CanUploadConfirmAndRankJobs()
    {
        using var seekerClient = factory.CreateClient();
        var seeker = await RegisterAsync(seekerClient, "job-seeker");
        Authorize(seekerClient, seeker.AccessToken);
        var profileResponse = await seekerClient.PutAsJsonAsync(
            "/api/profiles/job-seeker",
            new UpdateJobSeekerProfileRequest(
                "Backend Developer",
                "Builds reliable APIs.",
                "Colombo",
                "Software Engineer",
                3,
                ["C#", "PostgreSQL"]));
        profileResponse.EnsureSuccessStatusCode();

        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent(CreateDocx());
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        form.Add(file, "file", "alex-cv.docx");
        var uploadResponse = await seekerClient.PostAsync("/api/cv", form);

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<CvResult>(JsonOptions);
        Assert.NotNull(uploaded);
        Assert.Equal(CvProcessingStatus.NeedsReview, uploaded.Status);
        Assert.Contains("C#", uploaded.Skills);
        Assert.Contains("PostgreSQL", uploaded.Skills);

        using var replacementForm = new MultipartFormDataContent();
        using var replacementFile = new ByteArrayContent(CreateDocx());
        replacementFile.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        replacementForm.Add(replacementFile, "file", "alex-updated-cv.docx");
        var replacementResponse = await seekerClient.PostAsync(
            "/api/cv",
            replacementForm);
        replacementResponse.EnsureSuccessStatusCode();
        var replacement = await replacementResponse.Content.ReadFromJsonAsync<CvResult>(
            JsonOptions);
        Assert.NotNull(replacement);
        Assert.Equal(uploaded.Id, replacement.Id);
        Assert.Equal("alex-updated-cv.docx", replacement.OriginalFileName);

        var confirmResponse = await seekerClient.PutAsJsonAsync(
            "/api/cv/profile",
            new UpdateCvProfileRequest(
                "Alex Morgan",
                "alex@example.com",
                "+94 77 123 4567",
                "Colombo",
                "Backend Developer",
                "Backend engineer building reliable APIs and data services.",
                3,
                ["C#", "PostgreSQL"]));
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        using var recruiterClient = factory.CreateClient();
        var recruiter = await RegisterAsync(recruiterClient, "recruiter");
        Authorize(recruiterClient, recruiter.AccessToken);
        var companyResponse = await recruiterClient.PutAsJsonAsync(
            "/api/profiles/company",
            new UpdateCompanyProfileRequest(
                "Recommendation Labs",
                "Builds useful products.",
                "Colombo",
                null));
        companyResponse.EnsureSuccessStatusCode();
        var createJobResponse = await recruiterClient.PostAsJsonAsync(
            "/api/recruiter/jobs",
            new UpsertJobRequest(
                "Software Engineer",
                "Build reliable APIs and data platforms.",
                "Colombo",
                EmploymentType.FullTime,
                WorkMode.Hybrid,
                2,
                null,
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(30),
                [
                    new JobSkillRequest("C#", true),
                    new JobSkillRequest("PostgreSQL", false)
                ]));
        var job = await createJobResponse.Content.ReadFromJsonAsync<JobResult>(JsonOptions);
        Assert.NotNull(job);
        var publishResponse = await recruiterClient.PatchAsJsonAsync(
            $"/api/recruiter/jobs/{job.Id}/status",
            new ChangeJobStatusRequest(JobStatus.Published));
        publishResponse.EnsureSuccessStatusCode();

        var recommendations = await seekerClient.GetFromJsonAsync<RecommendationResponse>(
            "/api/recommendations",
            JsonOptions);
        Assert.NotNull(recommendations);
        var recommendation = Assert.Single(
            recommendations.Items,
            item => item.Job.Id == job.Id);
        Assert.Equal(100m, recommendation.Score);
        Assert.Equal("deterministic-v1", recommendation.AlgorithmVersion);
        Assert.Empty(recommendation.MissingRequiredSkills);

        var deleteResponse = await seekerClient.DeleteAsync("/api/cv");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await seekerClient.GetAsync("/api/cv")).StatusCode);
    }

    private static byte[] CreateDocx()
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(
            stream,
            DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
            autoSave: true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(
                new Body(
                    new Paragraph(new Run(new Text("Alex Morgan"))),
                    new Paragraph(new Run(new Text("alex@example.com"))),
                    new Paragraph(new Run(new Text("Backend Developer with 3 years experience"))),
                    new Paragraph(new Run(new Text("Skills: C#, PostgreSQL, REST")))));
        }

        return stream.ToArray();
    }

    private static async Task<AuthenticationResponse> RegisterAsync(
        HttpClient client,
        string role)
    {
        var endpoint = role == "job-seeker"
            ? "/api/auth/register/job-seeker"
            : "/api/auth/register/recruiter";
        var response = await client.PostAsJsonAsync(
            endpoint,
            new RegisterRequest(
                $"{role}-{Guid.NewGuid():N}@example.test",
                ValidPassword,
                "Test",
                "User"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthenticationResponse>(JsonOptions))!;
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
