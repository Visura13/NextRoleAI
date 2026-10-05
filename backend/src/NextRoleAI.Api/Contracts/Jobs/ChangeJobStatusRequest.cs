using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Contracts.Jobs;

public sealed record ChangeJobStatusRequest(JobStatus Status);
