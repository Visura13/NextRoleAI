using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Applications;

public sealed record ApplicationNoteRequest(
    [Required, StringLength(1000, MinimumLength = 2)] string Note);
