using NextRoleAI.Infrastructure.Cvs;

namespace NextRoleAI.Api.Tests.Cvs;

public sealed class DeterministicCvProfileParserTests
{
    private readonly DeterministicCvProfileParser parser = new();

    [Fact]
    public void Parse_UsesShortLayoutAwareLineAsCurrentJobTitle()
    {
        const string text = """
            Visura De silva
            Full Stack Developer - Intern
            Projects
            Built a full-featured e-commerce platform with React and PostgreSQL.
            visurasurajitha@gmail.com
            0712242518
            """;

        var profile = parser.Parse(text);

        Assert.Equal("Visura De silva", profile.CandidateName);
        Assert.Equal("Full Stack Developer - Intern", profile.CurrentJobTitle);
        Assert.Equal("visurasurajitha@gmail.com", profile.Email);
    }

    [Fact]
    public void Parse_DoesNotUseAnEntireLongPdfPageAsCurrentJobTitle()
    {
        var text = "Visura De silvaFull Stack Developer - Intern" +
            new string('x', 500);

        var profile = parser.Parse(text);

        Assert.Empty(profile.CurrentJobTitle);
        Assert.True(profile.CurrentJobTitle.Length <= 150);
    }
}
