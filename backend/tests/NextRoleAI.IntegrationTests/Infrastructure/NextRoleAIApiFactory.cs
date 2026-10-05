using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace NextRoleAI.IntegrationTests.Infrastructure;

public sealed class NextRoleAIApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "NEXTROLEAI_TEST_CONNECTION_STRING");

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,
                    ["Database:ApplyMigrationsOnStartup"] = "true",
                    ["Jwt:Issuer"] = "NextRoleAI.IntegrationTests",
                    ["Jwt:Audience"] = "NextRoleAI.TestClients",
                    ["Jwt:SigningKey"] =
                        "integration-tests-only-signing-key-with-more-than-32-characters",
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:RefreshTokenDays"] = "7"
                });
        });
    }
}
