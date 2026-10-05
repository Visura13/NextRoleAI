namespace NextRoleAI.Api.Contracts.Health;

public sealed record ApiHealthResponse(
    string Service,
    string Status,
    DateTimeOffset UtcTimestamp);
