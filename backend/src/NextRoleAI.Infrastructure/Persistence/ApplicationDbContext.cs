using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NextRoleAI.Domain.AgentWorkflows;
using NextRoleAI.Domain.Applications;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Domain.Jobs;
using NextRoleAI.Domain.Profiles;
using NextRoleAI.Infrastructure.Identity;

namespace NextRoleAI.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();

    public DbSet<JobSeekerSkill> JobSeekerSkills => Set<JobSeekerSkill>();

    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();

    public DbSet<JobPosting> JobPostings => Set<JobPosting>();

    public DbSet<JobSkill> JobSkills => Set<JobSkill>();

    public DbSet<CvDocument> CvDocuments => Set<CvDocument>();

    public DbSet<CvSkill> CvSkills => Set<CvSkill>();

    public DbSet<AgentWorkflowRun> AgentWorkflowRuns => Set<AgentWorkflowRun>();

    public DbSet<AgentWorkflowStep> AgentWorkflowSteps => Set<AgentWorkflowStep>();

    public DbSet<AgentToolCall> AgentToolCalls => Set<AgentToolCall>();

    public DbSet<AgentValidationResult> AgentValidationResults => Set<AgentValidationResult>();

    public DbSet<AgentShortlistItem> AgentShortlistItems => Set<AgentShortlistItem>();

    public DbSet<AgentApprovalDecision> AgentApprovalDecisions => Set<AgentApprovalDecision>();

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    public DbSet<ApplicationStatusEvent> ApplicationStatusEvents => Set<ApplicationStatusEvent>();

    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.CreatedAtUtc).IsRequired();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(token => token.CreatedAtUtc).IsRequired();
            entity.Property(token => token.ExpiresAtUtc).IsRequired();
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(64);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobSeekerProfile>(entity =>
        {
            entity.ToTable("JobSeekerProfiles");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.UserId).IsRequired();
            entity.Property(profile => profile.Headline).HasMaxLength(160).IsRequired();
            entity.Property(profile => profile.Summary).HasMaxLength(2000).IsRequired();
            entity.Property(profile => profile.Location).HasMaxLength(150).IsRequired();
            entity.Property(profile => profile.PreferredJobTitle).HasMaxLength(150).IsRequired();
            entity.Property(profile => profile.PreferredSalary).HasPrecision(18, 2);
            entity.HasIndex(profile => profile.UserId).IsUnique();
            entity.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<JobSeekerProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobSeekerSkill>(entity =>
        {
            entity.ToTable("JobSeekerSkills");
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(skill => new { skill.JobSeekerProfileId, skill.Name }).IsUnique();
            entity.HasOne(skill => skill.JobSeekerProfile)
                .WithMany(profile => profile.Skills)
                .HasForeignKey(skill => skill.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CompanyProfile>(entity =>
        {
            entity.ToTable("CompanyProfiles");
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.RecruiterUserId).IsRequired();
            entity.Property(profile => profile.Name).HasMaxLength(200).IsRequired();
            entity.Property(profile => profile.Description).HasMaxLength(3000).IsRequired();
            entity.Property(profile => profile.Location).HasMaxLength(150).IsRequired();
            entity.Property(profile => profile.WebsiteUrl).HasMaxLength(500);
            entity.HasIndex(profile => profile.RecruiterUserId).IsUnique();
            entity.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<CompanyProfile>(profile => profile.RecruiterUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobPosting>(entity =>
        {
            entity.ToTable("JobPostings");
            entity.HasKey(job => job.Id);
            entity.Property(job => job.Title).HasMaxLength(200).IsRequired();
            entity.Property(job => job.Description).HasMaxLength(8000).IsRequired();
            entity.Property(job => job.Location).HasMaxLength(150).IsRequired();
            entity.Property(job => job.EmploymentType).HasConversion<string>().HasMaxLength(30);
            entity.Property(job => job.WorkMode).HasConversion<string>().HasMaxLength(30);
            entity.Property(job => job.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(job => job.SalaryMinimum).HasPrecision(18, 2);
            entity.Property(job => job.SalaryMaximum).HasPrecision(18, 2);
            entity.Property(job => job.SalaryCurrency).HasMaxLength(3);
            entity.HasIndex(job => new { job.Status, job.PublishedAtUtc });
            entity.HasIndex(job => new { job.CompanyProfileId, job.UpdatedAtUtc });
            entity.HasOne(job => job.CompanyProfile)
                .WithMany(profile => profile.JobPostings)
                .HasForeignKey(job => job.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobSkill>(entity =>
        {
            entity.ToTable("JobSkills");
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(skill => new { skill.JobPostingId, skill.Name }).IsUnique();
            entity.HasIndex(skill => skill.Name);
            entity.HasOne(skill => skill.JobPosting)
                .WithMany(job => job.Skills)
                .HasForeignKey(skill => skill.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CvDocument>(entity =>
        {
            entity.ToTable("CvDocuments");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.UserId).IsRequired();
            entity.Property(document => document.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(document => document.StorageKey).HasMaxLength(100).IsRequired();
            entity.Property(document => document.ContentType).HasMaxLength(150).IsRequired();
            entity.Property(document => document.Sha256Checksum).HasMaxLength(64).IsRequired();
            entity.Property(document => document.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(document => document.ExtractedText).HasMaxLength(50_000).IsRequired();
            entity.Property(document => document.CandidateName).HasMaxLength(150).IsRequired();
            entity.Property(document => document.Email).HasMaxLength(254).IsRequired();
            entity.Property(document => document.Phone).HasMaxLength(40).IsRequired();
            entity.Property(document => document.Location).HasMaxLength(150).IsRequired();
            entity.Property(document => document.CurrentJobTitle).HasMaxLength(150).IsRequired();
            entity.Property(document => document.ProfessionalSummary).HasMaxLength(2000).IsRequired();
            entity.Property(document => document.EducationJson).HasColumnType("jsonb").IsRequired();
            entity.Property(document => document.QualityAssessmentJson).HasColumnType("jsonb");
            entity.Property(document => document.AnalysisMethod).HasMaxLength(50).IsRequired();
            entity.Property(document => document.AnalysisModel).HasMaxLength(100);
            entity.Property(document => document.AnalysisPromptVersion).HasMaxLength(50).IsRequired();
            entity.Property(document => document.FailureReason).HasMaxLength(500);
            entity.HasIndex(document => document.UserId).IsUnique();
            entity.HasIndex(document => document.StorageKey).IsUnique();
            entity.HasIndex(document => document.Sha256Checksum);
            entity.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<CvDocument>(document => document.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CvSkill>(entity =>
        {
            entity.ToTable("CvSkills");
            entity.HasKey(skill => skill.Id);
            entity.Property(skill => skill.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(skill => new { skill.CvDocumentId, skill.Name }).IsUnique();
            entity.HasIndex(skill => skill.Name);
            entity.HasOne(skill => skill.CvDocument)
                .WithMany(document => document.Skills)
                .HasForeignKey(skill => skill.CvDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgentWorkflowRun>(entity =>
        {
            entity.ToTable("AgentWorkflowRuns");
            entity.HasKey(workflow => workflow.Id);
            entity.Property(workflow => workflow.UserId).IsRequired();
            entity.Property(workflow => workflow.Objective).HasMaxLength(500).IsRequired();
            entity.Property(workflow => workflow.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(workflow => workflow.ApprovalStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(workflow => workflow.CurrentAgent).HasMaxLength(100).IsRequired();
            entity.Property(workflow => workflow.FailureCode).HasMaxLength(100);
            entity.Property(workflow => workflow.FailureMessage).HasMaxLength(1000);
            entity.Property(workflow => workflow.FinalSummary).HasMaxLength(2000);
            entity.HasIndex(workflow => new { workflow.UserId, workflow.CreatedAtUtc });
            entity.HasIndex(workflow => new { workflow.Status, workflow.UpdatedAtUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(workflow => workflow.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgentWorkflowStep>(entity =>
        {
            entity.ToTable("AgentWorkflowSteps");
            entity.HasKey(step => step.Id);
            entity.Property(step => step.AgentName).HasMaxLength(100).IsRequired();
            entity.Property(step => step.Responsibility).HasMaxLength(500).IsRequired();
            entity.Property(step => step.AllowedTools).HasMaxLength(500).IsRequired();
            entity.Property(step => step.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(step => step.InputJson).HasColumnType("jsonb").IsRequired();
            entity.Property(step => step.OutputJson).HasColumnType("jsonb");
            entity.Property(step => step.Error).HasMaxLength(1000);
            entity.HasIndex(step => new { step.WorkflowRunId, step.Sequence }).IsUnique();
            entity.HasOne(step => step.WorkflowRun)
                .WithMany(workflow => workflow.Steps)
                .HasForeignKey(step => step.WorkflowRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgentToolCall>(entity =>
        {
            entity.ToTable("AgentToolCalls");
            entity.HasKey(call => call.Id);
            entity.Property(call => call.ToolName).HasMaxLength(100).IsRequired();
            entity.Property(call => call.InputJson).HasColumnType("jsonb").IsRequired();
            entity.Property(call => call.OutputJson).HasColumnType("jsonb");
            entity.Property(call => call.Error).HasMaxLength(1000);
            entity.HasIndex(call => new { call.WorkflowStepId, call.StartedAtUtc });
            entity.HasOne(call => call.WorkflowStep)
                .WithMany(step => step.ToolCalls)
                .HasForeignKey(call => call.WorkflowStepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgentValidationResult>(entity =>
        {
            entity.ToTable("AgentValidationResults");
            entity.HasKey(result => result.Id);
            entity.Property(result => result.RuleName).HasMaxLength(150).IsRequired();
            entity.Property(result => result.Message).HasMaxLength(1000).IsRequired();
            entity.HasIndex(result => new { result.WorkflowRunId, result.CreatedAtUtc });
            entity.HasOne(result => result.WorkflowRun)
                .WithMany(workflow => workflow.ValidationResults)
                .HasForeignKey(result => result.WorkflowRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AgentShortlistItem>(entity =>
        {
            entity.ToTable("AgentShortlistItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Score).HasPrecision(5, 1);
            entity.Property(item => item.ReasonSummary).HasMaxLength(1500).IsRequired();
            entity.HasIndex(item => new { item.WorkflowRunId, item.Rank }).IsUnique();
            entity.HasIndex(item => new { item.WorkflowRunId, item.JobPostingId }).IsUnique();
            entity.HasOne(item => item.WorkflowRun)
                .WithMany(workflow => workflow.ShortlistItems)
                .HasForeignKey(item => item.WorkflowRunId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.JobPosting)
                .WithMany()
                .HasForeignKey(item => item.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AgentApprovalDecision>(entity =>
        {
            entity.ToTable("AgentApprovalDecisions");
            entity.HasKey(decision => decision.Id);
            entity.Property(decision => decision.DecidedByUserId).IsRequired();
            entity.Property(decision => decision.Decision).HasConversion<string>().HasMaxLength(30);
            entity.Property(decision => decision.Feedback).HasMaxLength(1000).IsRequired();
            entity.HasIndex(decision => new { decision.WorkflowRunId, decision.DecidedAtUtc });
            entity.HasOne(decision => decision.WorkflowRun)
                .WithMany(workflow => workflow.ApprovalDecisions)
                .HasForeignKey(decision => decision.WorkflowRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobApplication>(entity =>
        {
            entity.ToTable("JobApplications");
            entity.HasKey(application => application.Id);
            entity.Property(application => application.JobSeekerUserId).IsRequired();
            entity.Property(application => application.CoverNote).HasMaxLength(2000).IsRequired();
            entity.Property(application => application.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasIndex(application => new
                { application.JobSeekerUserId, application.JobPostingId })
                .IsUnique();
            entity.HasIndex(application => new
                { application.JobPostingId, application.Status, application.UpdatedAtUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(application => application.JobSeekerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(application => application.JobPosting)
                .WithMany()
                .HasForeignKey(application => application.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(application => application.SourceWorkflowRun)
                .WithMany()
                .HasForeignKey(application => application.SourceWorkflowRunId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ApplicationStatusEvent>(entity =>
        {
            entity.ToTable("ApplicationStatusEvents");
            entity.HasKey(statusEvent => statusEvent.Id);
            entity.Property(statusEvent => statusEvent.PreviousStatus)
                .HasConversion<string>()
                .HasMaxLength(40);
            entity.Property(statusEvent => statusEvent.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(40);
            entity.Property(statusEvent => statusEvent.ActorUserId).IsRequired();
            entity.Property(statusEvent => statusEvent.ActorRole)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.Property(statusEvent => statusEvent.Note).HasMaxLength(1000).IsRequired();
            entity.HasIndex(statusEvent => new
                { statusEvent.JobApplicationId, statusEvent.CreatedAtUtc });
            entity.HasOne(statusEvent => statusEvent.JobApplication)
                .WithMany(application => application.StatusEvents)
                .HasForeignKey(statusEvent => statusEvent.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NotificationDelivery>(entity =>
        {
            entity.ToTable("NotificationDeliveries");
            entity.HasKey(delivery => delivery.Id);
            entity.Property(delivery => delivery.RecipientUserId).IsRequired();
            entity.Property(delivery => delivery.RecipientEmail).HasMaxLength(254).IsRequired();
            entity.Property(delivery => delivery.EventType).HasMaxLength(80).IsRequired();
            entity.Property(delivery => delivery.Provider).HasMaxLength(50).IsRequired();
            entity.Property(delivery => delivery.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(delivery => delivery.ProviderMessageId).HasMaxLength(200);
            entity.Property(delivery => delivery.FailureReason).HasMaxLength(500);
            entity.HasIndex(delivery => new
                { delivery.JobApplicationId, delivery.CreatedAtUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(delivery => delivery.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(delivery => delivery.JobApplication)
                .WithMany(application => application.NotificationDeliveries)
                .HasForeignKey(delivery => delivery.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
