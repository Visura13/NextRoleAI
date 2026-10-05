namespace NextRoleAI.Application.Authentication;

public sealed record AuthenticationOutcome(
    AuthenticationResult? Result,
    IReadOnlyCollection<AuthenticationError> Errors)
{
    public bool Succeeded => Result is not null;

    public static AuthenticationOutcome Success(AuthenticationResult result) =>
        new(result, []);

    public static AuthenticationOutcome Failure(params AuthenticationError[] errors) =>
        new(null, errors);
}
