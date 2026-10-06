using System.Data.Common;

namespace NextRoleAI.Api.Configuration;

public static class PostgresUrlConnectionString
{
    public static string Convert(string databaseUrl)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            throw new InvalidOperationException("DATABASE_URL must be a valid PostgreSQL URL.");
        }

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException("DATABASE_URL must include a host, user, and password.");
        }

        var builder = new DbConnectionStringBuilder
        {
            ["Host"] = uri.Host,
            ["Port"] = uri.IsDefaultPort ? 5432 : uri.Port,
            ["Database"] = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            ["Username"] = Uri.UnescapeDataString(credentials[0]),
            ["Password"] = Uri.UnescapeDataString(credentials[1]),
            ["Pooling"] = true
        };

        if (uri.Query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase))
        {
            builder["SSL Mode"] = "Require";
        }

        return builder.ConnectionString;
    }
}
