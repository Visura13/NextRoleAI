using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Jobs;

public sealed record JobSkillRequest(
    [property: Required, MaxLength(100)] string Name,
    bool IsRequired);
