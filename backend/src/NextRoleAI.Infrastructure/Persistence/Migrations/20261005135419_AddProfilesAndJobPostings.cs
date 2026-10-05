using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextRoleAI.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddProfilesAndJobPostings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CompanyProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RecruiterUserId = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompanyProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompanyProfiles_AspNetUsers_RecruiterUserId",
                    column: x => x.RecruiterUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "JobSeekerProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                Headline = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                PreferredJobTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                YearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobSeekerProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobSeekerProfiles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "JobPostings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                EmploymentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                WorkMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                MinimumYearsExperience = table.Column<int>(type: "integer", nullable: false),
                SalaryMinimum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                SalaryMaximum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                SalaryCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ClosesAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobPostings", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobPostings_CompanyProfiles_CompanyProfileId",
                    column: x => x.CompanyProfileId,
                    principalTable: "CompanyProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "JobSeekerSkills",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobSeekerSkills", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobSeekerSkills_JobSeekerProfiles_JobSeekerProfileId",
                    column: x => x.JobSeekerProfileId,
                    principalTable: "JobSeekerProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "JobSkills",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsRequired = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobSkills", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobSkills_JobPostings_JobPostingId",
                    column: x => x.JobPostingId,
                    principalTable: "JobPostings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CompanyProfiles_RecruiterUserId",
            table: "CompanyProfiles",
            column: "RecruiterUserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_JobPostings_CompanyProfileId_UpdatedAtUtc",
            table: "JobPostings",
            columns: new[] { "CompanyProfileId", "UpdatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_JobPostings_Status_PublishedAtUtc",
            table: "JobPostings",
            columns: new[] { "Status", "PublishedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_JobSeekerProfiles_UserId",
            table: "JobSeekerProfiles",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_JobSeekerSkills_JobSeekerProfileId_Name",
            table: "JobSeekerSkills",
            columns: new[] { "JobSeekerProfileId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_JobSkills_JobPostingId_Name",
            table: "JobSkills",
            columns: new[] { "JobPostingId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_JobSkills_Name",
            table: "JobSkills",
            column: "Name");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "JobSeekerSkills");

        migrationBuilder.DropTable(
            name: "JobSkills");

        migrationBuilder.DropTable(
            name: "JobSeekerProfiles");

        migrationBuilder.DropTable(
            name: "JobPostings");

        migrationBuilder.DropTable(
            name: "CompanyProfiles");
    }
}
