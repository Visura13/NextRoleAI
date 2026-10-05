using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextRoleAI.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddAgentWorkflowState : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AgentWorkflowRuns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                Objective = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                ApprovalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CurrentAgent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                FailureCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FailureMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                FinalSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentWorkflowRuns", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentWorkflowRuns_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AgentApprovalDecisions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: false),
                DecidedByUserId = table.Column<string>(type: "text", nullable: false),
                Decision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                Feedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentApprovalDecisions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentApprovalDecisions_AgentWorkflowRuns_WorkflowRunId",
                    column: x => x.WorkflowRunId,
                    principalTable: "AgentWorkflowRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AgentShortlistItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: false),
                JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                Rank = table.Column<int>(type: "integer", nullable: false),
                Score = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                ReasonSummary = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                IsApproved = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentShortlistItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentShortlistItems_AgentWorkflowRuns_WorkflowRunId",
                    column: x => x.WorkflowRunId,
                    principalTable: "AgentWorkflowRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AgentShortlistItems_JobPostings_JobPostingId",
                    column: x => x.JobPostingId,
                    principalTable: "JobPostings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AgentValidationResults",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: false),
                RuleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Passed = table.Column<bool>(type: "boolean", nullable: false),
                Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentValidationResults", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentValidationResults_AgentWorkflowRuns_WorkflowRunId",
                    column: x => x.WorkflowRunId,
                    principalTable: "AgentWorkflowRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AgentWorkflowSteps",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkflowRunId = table.Column<Guid>(type: "uuid", nullable: false),
                Sequence = table.Column<int>(type: "integer", nullable: false),
                AgentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Responsibility = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                AllowedTools = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                InputJson = table.Column<string>(type: "jsonb", nullable: false),
                OutputJson = table.Column<string>(type: "jsonb", nullable: true),
                RetryCount = table.Column<int>(type: "integer", nullable: false),
                Error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentWorkflowSteps", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentWorkflowSteps_AgentWorkflowRuns_WorkflowRunId",
                    column: x => x.WorkflowRunId,
                    principalTable: "AgentWorkflowRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AgentToolCalls",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkflowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                ToolName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                InputJson = table.Column<string>(type: "jsonb", nullable: false),
                OutputJson = table.Column<string>(type: "jsonb", nullable: true),
                Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                Error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                DurationMilliseconds = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AgentToolCalls", x => x.Id);
                table.ForeignKey(
                    name: "FK_AgentToolCalls_AgentWorkflowSteps_WorkflowStepId",
                    column: x => x.WorkflowStepId,
                    principalTable: "AgentWorkflowSteps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AgentApprovalDecisions_WorkflowRunId_DecidedAtUtc",
            table: "AgentApprovalDecisions",
            columns: new[] { "WorkflowRunId", "DecidedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AgentShortlistItems_JobPostingId",
            table: "AgentShortlistItems",
            column: "JobPostingId");

        migrationBuilder.CreateIndex(
            name: "IX_AgentShortlistItems_WorkflowRunId_JobPostingId",
            table: "AgentShortlistItems",
            columns: new[] { "WorkflowRunId", "JobPostingId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AgentShortlistItems_WorkflowRunId_Rank",
            table: "AgentShortlistItems",
            columns: new[] { "WorkflowRunId", "Rank" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AgentToolCalls_WorkflowStepId_StartedAtUtc",
            table: "AgentToolCalls",
            columns: new[] { "WorkflowStepId", "StartedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AgentValidationResults_WorkflowRunId_CreatedAtUtc",
            table: "AgentValidationResults",
            columns: new[] { "WorkflowRunId", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AgentWorkflowRuns_Status_UpdatedAtUtc",
            table: "AgentWorkflowRuns",
            columns: new[] { "Status", "UpdatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AgentWorkflowRuns_UserId_CreatedAtUtc",
            table: "AgentWorkflowRuns",
            columns: new[] { "UserId", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AgentWorkflowSteps_WorkflowRunId_Sequence",
            table: "AgentWorkflowSteps",
            columns: new[] { "WorkflowRunId", "Sequence" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AgentApprovalDecisions");

        migrationBuilder.DropTable(
            name: "AgentShortlistItems");

        migrationBuilder.DropTable(
            name: "AgentToolCalls");

        migrationBuilder.DropTable(
            name: "AgentValidationResults");

        migrationBuilder.DropTable(
            name: "AgentWorkflowSteps");

        migrationBuilder.DropTable(
            name: "AgentWorkflowRuns");
    }
}
