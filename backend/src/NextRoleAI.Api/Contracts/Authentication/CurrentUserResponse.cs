using NextRoleAI.Application.Authentication;

namespace NextRoleAI.Api.Contracts.Authentication;

public sealed record CurrentUserResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role)
{
    public static CurrentUserResponse FromResult(CurrentUserResult result) =>
        new(result.UserId, result.Email, result.FirstName, result.LastName, result.Role);
}
