using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NextRoleAI.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261010100000_AddCvAiAnalysis")]
public partial class AddCvAiAnalysis : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AnalysisMethod",
            table: "CvDocuments",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "deterministic");

        migrationBuilder.AddColumn<string>(
            name: "AnalysisModel",
            table: "CvDocuments",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AnalysisPromptVersion",
            table: "CvDocuments",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "deterministic-v1");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "AnalyzedAtUtc",
            table: "CvDocuments",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EducationJson",
            table: "CvDocuments",
            type: "jsonb",
            nullable: false,
            defaultValueSql: "'[]'::jsonb");

        migrationBuilder.AddColumn<string>(
            name: "QualityAssessmentJson",
            table: "CvDocuments",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "QualityScore",
            table: "CvDocuments",
            type: "integer",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AnalysisMethod", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "AnalysisModel", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "AnalysisPromptVersion", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "AnalyzedAtUtc", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "EducationJson", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "QualityAssessmentJson", table: "CvDocuments");
        migrationBuilder.DropColumn(name: "QualityScore", table: "CvDocuments");
    }
}
