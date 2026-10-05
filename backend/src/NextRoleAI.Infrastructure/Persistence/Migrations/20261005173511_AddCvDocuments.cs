using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextRoleAI.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCvDocuments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CvDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                StorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                Sha256Checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                ExtractedText = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                CandidateName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                CurrentJobTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                ProfessionalSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                YearsExperience = table.Column<int>(type: "integer", nullable: false),
                FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CvDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_CvDocuments_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CvSkills",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CvDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CvSkills", x => x.Id);
                table.ForeignKey(
                    name: "FK_CvSkills_CvDocuments_CvDocumentId",
                    column: x => x.CvDocumentId,
                    principalTable: "CvDocuments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CvDocuments_Sha256Checksum",
            table: "CvDocuments",
            column: "Sha256Checksum");

        migrationBuilder.CreateIndex(
            name: "IX_CvDocuments_StorageKey",
            table: "CvDocuments",
            column: "StorageKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CvDocuments_UserId",
            table: "CvDocuments",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CvSkills_CvDocumentId_Name",
            table: "CvSkills",
            columns: new[] { "CvDocumentId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CvSkills_Name",
            table: "CvSkills",
            column: "Name");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CvSkills");

        migrationBuilder.DropTable(
            name: "CvDocuments");
    }
}
