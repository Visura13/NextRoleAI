using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Jobs;

internal sealed class JobService(
    ApplicationDbContext dbContext,
    TimeProvider timeProvider) : IJobService
{
    public async Task<JobCommandResult> CreateAsync(
        string recruiterUserId,
        JobUpsert input,
        CancellationToken cancellationToken = default)
    {
        var company = await dbContext.CompanyProfiles.SingleOrDefaultAsync(
            profile => profile.RecruiterUserId == recruiterUserId,
            cancellationToken);

        if (company is null)
        {
            return JobCommandResult.Failure(
                JobCommandError.CompanyProfileRequired,
                "Create a company profile before creating a job posting.");
        }

        var now = timeProvider.GetUtcNow();
        var job = new JobPosting
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = company.Id,
            CompanyProfile = company,
            Status = JobStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        Apply(job, input);
        dbContext.JobPostings.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        return JobCommandResult.Success(ToResult(job));
    }

    public async Task<PagedResult<JobResult>> GetRecruiterJobsAsync(
        string recruiterUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.JobPostings
            .AsNoTracking()
            .Include(job => job.CompanyProfile)
            .Include(job => job.Skills)
            .Where(job => job.CompanyProfile.RecruiterUserId == recruiterUserId)
            .OrderByDescending(job => job.UpdatedAtUtc);

        return await ToPageAsync(query, page, pageSize, cancellationToken);
    }

    public async Task<JobResult?> GetRecruiterJobAsync(
        string recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await dbContext.JobPostings
            .AsNoTracking()
            .Include(candidate => candidate.CompanyProfile)
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == jobId &&
                    candidate.CompanyProfile.RecruiterUserId == recruiterUserId,
                cancellationToken);

        return job is null ? null : ToResult(job);
    }

    public async Task<JobCommandResult> UpdateAsync(
        string recruiterUserId,
        Guid jobId,
        JobUpsert input,
        CancellationToken cancellationToken = default)
    {
        var job = await FindOwnedAsync(recruiterUserId, jobId, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        if (job.Status == JobStatus.Closed)
        {
            return JobCommandResult.Failure(
                JobCommandError.InvalidTransition,
                "Closed job postings cannot be edited.");
        }

        job.Skills.Clear();
        Apply(job, input);
        job.UpdatedAtUtc = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);

        return JobCommandResult.Success(ToResult(job));
    }

    public async Task<JobCommandResult> ChangeStatusAsync(
        string recruiterUserId,
        Guid jobId,
        JobStatus status,
        CancellationToken cancellationToken = default)
    {
        var job = await FindOwnedAsync(recruiterUserId, jobId, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        if (job.Status == status)
        {
            return JobCommandResult.Success(ToResult(job));
        }

        var validTransition =
            (job.Status == JobStatus.Draft && status == JobStatus.Published) ||
            (job.Status == JobStatus.Published && status == JobStatus.Closed);

        if (!validTransition)
        {
            return JobCommandResult.Failure(
                JobCommandError.InvalidTransition,
                $"A {job.Status} job posting cannot transition to {status}.");
        }

        var now = timeProvider.GetUtcNow();
        if (status == JobStatus.Published && job.ClosesAtUtc <= now)
        {
            return JobCommandResult.Failure(
                JobCommandError.InvalidTransition,
                "The closing date must be in the future before publishing.");
        }

        job.Status = status;
        job.PublishedAtUtc ??= status == JobStatus.Published ? now : null;
        job.UpdatedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return JobCommandResult.Success(ToResult(job));
    }

    public async Task<JobCommandResult> DeleteAsync(
        string recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await FindOwnedAsync(recruiterUserId, jobId, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        if (job.Status != JobStatus.Draft)
        {
            return JobCommandResult.Failure(
                JobCommandError.PublishedJobCannotBeDeleted,
                "Only draft job postings can be deleted. Close published jobs instead.");
        }

        var result = ToResult(job);
        dbContext.JobPostings.Remove(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return JobCommandResult.Success(result);
    }

    public async Task<PagedResult<JobResult>> SearchPublishedAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var jobs = dbContext.JobPostings
            .AsNoTracking()
            .Include(job => job.CompanyProfile)
            .Include(job => job.Skills)
            .Where(job => job.Status == JobStatus.Published &&
                (job.ClosesAtUtc == null || job.ClosesAtUtc > now));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            jobs = jobs.Where(job =>
                EF.Functions.ILike(job.Title, pattern) ||
                EF.Functions.ILike(job.Description, pattern) ||
                EF.Functions.ILike(job.CompanyProfile.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var pattern = $"%{query.Location.Trim()}%";
            jobs = jobs.Where(job => EF.Functions.ILike(job.Location, pattern));
        }

        if (query.EmploymentType is not null)
        {
            jobs = jobs.Where(job => job.EmploymentType == query.EmploymentType);
        }

        if (query.WorkMode is not null)
        {
            jobs = jobs.Where(job => job.WorkMode == query.WorkMode);
        }

        if (!string.IsNullOrWhiteSpace(query.Skill))
        {
            var skill = query.Skill.Trim();
            jobs = jobs.Where(job => job.Skills.Any(candidate =>
                EF.Functions.ILike(candidate.Name, skill)));
        }

        var ordered = query.SortBy.Trim().ToLowerInvariant() switch
        {
            "title" => jobs.OrderBy(job => job.Title),
            "closingdate" => jobs.OrderBy(job => job.ClosesAtUtc == null)
                .ThenBy(job => job.ClosesAtUtc),
            _ => jobs.OrderByDescending(job => job.PublishedAtUtc)
        };

        return await ToPageAsync(ordered, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<JobResult?> GetPublishedAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var job = await dbContext.JobPostings
            .AsNoTracking()
            .Include(candidate => candidate.CompanyProfile)
            .Include(candidate => candidate.Skills)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == jobId &&
                candidate.Status == JobStatus.Published &&
                (candidate.ClosesAtUtc == null || candidate.ClosesAtUtc > now),
                cancellationToken);

        return job is null ? null : ToResult(job);
    }

    private async Task<JobPosting?> FindOwnedAsync(
        string recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken) =>
        await dbContext.JobPostings
            .Include(job => job.CompanyProfile)
            .Include(job => job.Skills)
            .SingleOrDefaultAsync(
                job => job.Id == jobId &&
                    job.CompanyProfile.RecruiterUserId == recruiterUserId,
                cancellationToken);

    private static void Apply(JobPosting job, JobUpsert input)
    {
        job.Title = input.Title.Trim();
        job.Description = input.Description.Trim();
        job.Location = input.Location.Trim();
        job.EmploymentType = input.EmploymentType;
        job.WorkMode = input.WorkMode;
        job.MinimumYearsExperience = input.MinimumYearsExperience;
        job.SalaryMinimum = input.SalaryMinimum;
        job.SalaryMaximum = input.SalaryMaximum;
        job.SalaryCurrency = string.IsNullOrWhiteSpace(input.SalaryCurrency)
            ? null
            : input.SalaryCurrency.Trim().ToUpperInvariant();
        job.ClosesAtUtc = input.ClosesAtUtc;

        foreach (var skill in NormaliseSkills(input.Skills))
        {
            job.Skills.Add(new JobSkill
            {
                Id = Guid.NewGuid(),
                Name = skill.Name,
                IsRequired = skill.IsRequired
            });
        }
    }

    private static IReadOnlyCollection<JobSkillInput> NormaliseSkills(
        IEnumerable<JobSkillInput> skills) =>
        skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill.Name))
            .GroupBy(skill => skill.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new JobSkillInput(
                group.Key,
                group.Any(skill => skill.IsRequired)))
            .OrderBy(skill => skill.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static async Task<PagedResult<JobResult>> ToPageAsync(
        IOrderedQueryable<JobPosting> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobResult>(
            jobs.Select(ToResult).ToArray(),
            page,
            pageSize,
            totalCount);
    }

    private static JobResult ToResult(JobPosting job) =>
        new(
            job.Id,
            job.CompanyProfileId,
            job.CompanyProfile.Name,
            job.Title,
            job.Description,
            job.Location,
            job.EmploymentType,
            job.WorkMode,
            job.MinimumYearsExperience,
            job.SalaryMinimum,
            job.SalaryMaximum,
            job.SalaryCurrency,
            job.Status,
            job.PublishedAtUtc,
            job.ClosesAtUtc,
            job.Skills
                .OrderBy(skill => skill.Name, StringComparer.OrdinalIgnoreCase)
                .Select(skill => new JobSkillInput(skill.Name, skill.IsRequired))
                .ToArray(),
            job.CreatedAtUtc,
            job.UpdatedAtUtc);

    private static JobCommandResult NotFound() =>
        JobCommandResult.Failure(
            JobCommandError.NotFound,
            "The job posting was not found.");
}
