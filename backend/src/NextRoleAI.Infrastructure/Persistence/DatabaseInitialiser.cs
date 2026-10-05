using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.Domain.Profiles;
using NextRoleAI.Infrastructure.Identity;

namespace NextRoleAI.Infrastructure.Persistence;

public static class DatabaseInitialiser
{
    public static async Task InitialiseDatabaseAsync(
        this IServiceProvider services,
        bool applyMigrations,
        bool seedDevelopmentData = false,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (applyMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create role '{role}': {errors}");
                }
            }
        }

        if (seedDevelopmentData)
        {
            await SeedDevelopmentDataAsync(scope.ServiceProvider, cancellationToken);
        }
    }

    private static async Task SeedDevelopmentDataAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["SeedData:RecruiterEmail"];
        var password = configuration["SeedData:RecruiterPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SeedData recruiter email and password are required when development seeding is enabled.");
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var recruiter = await userManager.FindByEmailAsync(email);
        if (recruiter is null)
        {
            recruiter = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email.Trim(),
                UserName = email.Trim(),
                FirstName = "Demo",
                LastName = "Recruiter",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(recruiter, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create the demo recruiter: {FormatErrors(createResult)}");
            }

            var roleResult = await userManager.AddToRoleAsync(recruiter, RoleNames.Recruiter);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the demo recruiter role: {FormatErrors(roleResult)}");
            }
        }
        else if (!await userManager.IsInRoleAsync(recruiter, RoleNames.Recruiter))
        {
            var roleResult = await userManager.AddToRoleAsync(recruiter, RoleNames.Recruiter);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the demo recruiter role: {FormatErrors(roleResult)}");
            }
        }

        var company = await dbContext.CompanyProfiles
            .SingleOrDefaultAsync(
                profile => profile.RecruiterUserId == recruiter.Id,
                cancellationToken);

        if (company is not null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        company = new CompanyProfile
        {
            Id = Guid.NewGuid(),
            RecruiterUserId = recruiter.Id,
            Name = "NextRole Demo Labs",
            Description = "Demo company data for local development only.",
            Location = "Colombo",
            WebsiteUrl = "https://example.com",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var job = new JobPosting
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = company.Id,
            CompanyProfile = company,
            Title = "Junior Full-Stack Developer",
            Description = "Build web and mobile features with a collaborative product team.",
            Location = "Colombo",
            EmploymentType = EmploymentType.FullTime,
            WorkMode = WorkMode.Hybrid,
            MinimumYearsExperience = 1,
            SalaryMinimum = 100000,
            SalaryMaximum = 180000,
            SalaryCurrency = "LKR",
            Status = JobStatus.Published,
            PublishedAtUtc = now,
            ClosesAtUtc = now.AddDays(30),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        job.Skills.Add(new JobSkill
        {
            Id = Guid.NewGuid(),
            Name = "ASP.NET Core",
            IsRequired = true
        });
        job.Skills.Add(new JobSkill
        {
            Id = Guid.NewGuid(),
            Name = "React",
            IsRequired = false
        });

        company.JobPostings.Add(job);
        dbContext.CompanyProfiles.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(error => error.Description));
}
