using System.ComponentModel.DataAnnotations;
using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Api.Contracts.Applications;

public sealed record ApplicationDecisionRequest(
    ApplicationStatus Status,
    [Required, StringLength(1000, MinimumLength = 2)] string Note);
