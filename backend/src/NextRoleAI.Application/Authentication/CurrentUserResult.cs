namespace NextRoleAI.Application.Authentication;

public sealed record CurrentUserResult(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role);
