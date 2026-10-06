using System.Data.Common;
using NextRoleAI.Api.Configuration;

namespace NextRoleAI.Api.Tests.Configuration;

public sealed class PostgresUrlConnectionStringTests
{
    [Fact]
    public void Convert_DecodesCredentialsAndPreservesRequiredTls()
    {
        var result = PostgresUrlConnectionString.Convert(
            "postgresql://nextrole%40user:p%40ss%3Aword@db.internal:6432/nextroleai?sslmode=require");

        var parsed = new DbConnectionStringBuilder { ConnectionString = result };
        Assert.Equal("db.internal", parsed["host"]);
        Assert.Equal("6432", parsed["port"].ToString());
        Assert.Equal("nextroleai", parsed["database"]);
        Assert.Equal("nextrole@user", parsed["username"]);
        Assert.Equal("p@ss:word", parsed["password"]);
        Assert.Equal("Require", parsed["ssl mode"]);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("https://example.com/database")]
    [InlineData("postgresql://missing-credentials/database")]
    public void Convert_RejectsMalformedOrUnsupportedUrls(string databaseUrl)
    {
        Assert.Throws<InvalidOperationException>(() =>
            PostgresUrlConnectionString.Convert(databaseUrl));
    }
}
