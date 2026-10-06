using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Applications;

public sealed record SubmitApplicationRequest(
    [Required] Guid JobPostingId,
    [Required, StringLength(2000, MinimumLength = 20)] string CoverNote,
    Guid? SourceWorkflowRunId);
