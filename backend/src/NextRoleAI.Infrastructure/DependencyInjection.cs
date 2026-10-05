using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NextRoleAI.AgenticAI;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Application.Cvs;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Application.Profiles;
using NextRoleAI.Application.Recommendations;
using NextRoleAI.Infrastructure.AgentWorkflows;
using NextRoleAI.Infrastructure.Authentication;
using NextRoleAI.Infrastructure.Cvs;
using NextRoleAI.Infrastructure.Identity;
using NextRoleAI.Infrastructure.Jobs;
using NextRoleAI.Infrastructure.Persistence;
using NextRoleAI.Infrastructure.Profiles;
using NextRoleAI.Infrastructure.Recommendations;

namespace NextRoleAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be configured.");
        }

        var jwtOptions = configuration
            .GetRequiredSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is required.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
            jwtOptions.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT issuer, audience, and a signing key of at least 32 characters must be configured.");
        }

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CvStorageOptions>()
            .Bind(configuration.GetSection(CvStorageOptions.SectionName));

        services.AddSingleton(TimeProvider.System);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.JobSeekerOnly,
                policy => policy.RequireRole(RoleNames.JobSeeker))
            .AddPolicy(
                AuthorizationPolicies.RecruiterOnly,
                policy => policy.RequireRole(RoleNames.Recruiter));

        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IJobService, JobService>();
        services.AddSingleton<ICvFileStore, LocalCvFileStore>();
        services.AddSingleton<ICvTextExtractor, CvTextExtractor>();
        services.AddSingleton<ICvProfileParser, DeterministicCvProfileParser>();
        services.AddScoped<ICvService, CvService>();
        services.AddSingleton<IJobMatchScorer, DeterministicJobMatchScorer>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IAgentWorkflowStore, AgentWorkflowStore>();
        services.AddScoped<IAgentTool, CandidateProfileReadTool>();
        services.AddScoped<IAgentTool, RankPublishedJobsTool>();
        services.AddScoped<IAgentTool, PublishShortlistTool>();
        services.AddSingleton<ObjectiveGuard>();
        services.AddSingleton<PlanningAgent>();
        services.AddSingleton<CandidateProfileAgent>();
        services.AddSingleton<JobDiscoveryAgent>();
        services.AddSingleton<ValidationSafetyAgent>();
        services.AddScoped<IAgentWorkflowService, AgentWorkflowOrchestrator>();

        return services;
    }
}
