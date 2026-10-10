using NextRoleAI.Application.Cvs;
using NextRoleAI.Infrastructure.Cvs;

namespace NextRoleAI.Api.Tests.Cvs;

public sealed class CvAnalysisValidatorTests
{
    [Fact]
    public void Validate_KeepsEvidenceBackedSkillsAndHigherEducationOnly()
    {
        const string source = """
            Visura De silva
            Full Stack Developer - Intern
            BSc (Hons) in Information Technology, Sri Lanka Institute of Information Technology
            GCE Advanced Level, Kelaniya President's College
            Tech Stack: Next.js, Prisma, PostgreSQL
            """;
        var fallback = new ExtractedCvProfile(
            "Visura De silva",
            "",
            "",
            "",
            "Full Stack Developer - Intern",
            "Software developer.",
            0,
            ["PostgreSQL"]);
        var raw = new RawCvAnalysis(
            "Full Stack Developer - Intern",
            "Full Stack Developer - Intern",
            "Full-stack developer building production applications.",
            1,
            [
                new RawCvSkill("Next.js", "Tech Stack: Next.js, Prisma, PostgreSQL", 0.98m),
                new RawCvSkill("Rust", "Built multiple Rust services", 0.90m)
            ],
            [
                new RawCvEducation(
                    "BSc (Hons)",
                    "Information Technology",
                    "Sri Lanka Institute of Information Technology",
                    "Present",
                    "BSc (Hons) in Information Technology, Sri Lanka Institute of Information Technology",
                    0.99m),
                new RawCvEducation(
                    "GCE Advanced Level",
                    "Information Technology",
                    "Kelaniya President's College",
                    "Completed",
                    "GCE Advanced Level, Kelaniya President's College",
                    0.99m)
            ],
            new RawCvQualityAssessment(
                90,
                80,
                75,
                70,
                85,
                ["Clear project descriptions"],
                ["Quantify project outcomes"]));

        var result = CvAnalysisValidator.Validate(
            raw,
            fallback,
            source,
            "test-model",
            DateTimeOffset.Parse("2026-10-10T10:00:00Z"));

        Assert.Contains("Next.js", result.Profile.Skills);
        Assert.DoesNotContain("Rust", result.Profile.Skills);
        var education = Assert.Single(result.Profile.Education!);
        Assert.Equal("BSc (Hons)", education.Qualification);
        Assert.DoesNotContain(
            result.Profile.Education!,
            item => item.Qualification.Contains("Advanced Level"));
        Assert.Equal(80, result.QualityAssessment!.OverallScore);
        Assert.Equal("ai", result.AnalysisMethod);
    }

    [Fact]
    public void Validate_FallsBackToDeterministicSkillsWhenAiEvidenceIsUnsupported()
    {
        var fallback = new ExtractedCvProfile(
            "Alex Morgan",
            "alex@example.com",
            "",
            "",
            "Backend Developer",
            "Backend developer with API experience.",
            3,
            ["C#", "PostgreSQL"]);
        var raw = new RawCvAnalysis(
            null,
            null,
            null,
            3,
            [new RawCvSkill("Kubernetes", "Kubernetes platform owner", 0.95m)],
            [],
            null);

        var result = CvAnalysisValidator.Validate(
            raw,
            fallback,
            "Alex Morgan uses C# and PostgreSQL.",
            "test-model",
            DateTimeOffset.UtcNow);

        Assert.Equal(new[] { "C#", "PostgreSQL" }, result.Profile.Skills);
        Assert.Null(result.QualityAssessment);
    }

    [Fact]
    public void Validate_RejectsUnsupportedTitleAndIncompleteQualityAssessment()
    {
        var fallback = new ExtractedCvProfile(
            "Alex Morgan",
            "",
            "",
            "",
            "Backend Developer",
            "Backend developer with API experience.",
            3,
            ["C#"]);
        var raw = new RawCvAnalysis(
            "Chief Technology Officer",
            "Chief Technology Officer",
            "Experienced technology leader.",
            null,
            [],
            [],
            new RawCvQualityAssessment(
                80,
                70,
                null,
                60,
                90,
                [],
                []));

        var result = CvAnalysisValidator.Validate(
            raw,
            fallback,
            "Alex Morgan is a Backend Developer with C# experience.",
            "test-model",
            DateTimeOffset.UtcNow);

        Assert.Equal("Backend Developer", result.Profile.CurrentJobTitle);
        Assert.Equal(3, result.Profile.YearsExperience);
        Assert.Null(result.QualityAssessment);
    }
}
