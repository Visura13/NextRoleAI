using Microsoft.AspNetCore.Mvc.Testing;

namespace NextRoleAI.IntegrationTests.Infrastructure;

public sealed class NextRoleAIApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> originalEnvironment = [];

    public NextRoleAIApiFactory()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "NEXTROLEAI_TEST_CONNECTION_STRING");

        SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", "true");
        SetEnvironmentVariable("Jwt__Issuer", "NextRoleAI.IntegrationTests");
        SetEnvironmentVariable("Jwt__Audience", "NextRoleAI.TestClients");
        SetEnvironmentVariable(
            "Jwt__SigningKey",
            "integration-tests-only-signing-key-with-more-than-32-characters");
        SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15");
        SetEnvironmentVariable("Jwt__RefreshTokenDays", "7");
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            foreach (var (name, value) in originalEnvironment)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    private void SetEnvironmentVariable(string name, string? value)
    {
        originalEnvironment[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }
}
