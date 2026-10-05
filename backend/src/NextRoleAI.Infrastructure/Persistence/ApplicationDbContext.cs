using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
    }
}
