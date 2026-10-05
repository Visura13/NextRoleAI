namespace NextRoleAI.IntegrationTests.Infrastructure;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("NEXTROLEAI_TEST_CONNECTION_STRING")))
        {
            Skip = "Set NEXTROLEAI_TEST_CONNECTION_STRING to run PostgreSQL integration tests.";
        }
    }
}
