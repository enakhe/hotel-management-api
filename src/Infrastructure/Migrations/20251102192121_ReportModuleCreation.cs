using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReportModuleCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailRecipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "UTC")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    TemplateConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GeneratedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    DownloadUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordCount = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenerationDurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_ReportSchedules_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalTable: "ReportSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReportSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuperAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnsubscribeToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportSubscriptions_ReportSchedules_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalTable: "ReportSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    RecordCount = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitiatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportExecutions_ReportSchedules_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalTable: "ReportSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportExecutions_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_InitiatedBy",
                table: "ReportExecutions",
                column: "InitiatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_IsScheduled",
                table: "ReportExecutions",
                column: "IsScheduled");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_ReportId",
                table: "ReportExecutions",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_ReportScheduleId",
                table: "ReportExecutions",
                column: "ReportScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_ReportScheduleId_StartedAt",
                table: "ReportExecutions",
                columns: new[] { "ReportScheduleId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_StartedAt",
                table: "ReportExecutions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_Status",
                table: "ReportExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_Status_StartedAt",
                table: "ReportExecutions",
                columns: new[] { "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedBy",
                table: "Reports",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedBy_GeneratedAt",
                table: "Reports",
                columns: new[] { "GeneratedBy", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportScheduleId",
                table: "Reports",
                column: "ReportScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                table: "Reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_GeneratedAt",
                table: "Reports",
                columns: new[] { "Status", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TenantId",
                table: "Reports",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Type",
                table: "Reports",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_CreatedBy",
                table: "ReportSchedules",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_IsActive",
                table: "ReportSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_IsActive_NextRunAt",
                table: "ReportSchedules",
                columns: new[] { "IsActive", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_NextRunAt",
                table: "ReportSchedules",
                column: "NextRunAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_TenantId",
                table: "ReportSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_Email",
                table: "ReportSubscriptions",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_IsActive",
                table: "ReportSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_ReportScheduleId",
                table: "ReportSubscriptions",
                column: "ReportScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_SuperAdminId",
                table: "ReportSubscriptions",
                column: "SuperAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_SuperAdminId_ReportScheduleId",
                table: "ReportSubscriptions",
                columns: new[] { "SuperAdminId", "ReportScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubscriptions_UnsubscribeToken",
                table: "ReportSubscriptions",
                column: "UnsubscribeToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_CreatedBy",
                table: "ReportTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_IsActive",
                table: "ReportTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_IsActive_IsPublic",
                table: "ReportTemplates",
                columns: new[] { "IsActive", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_IsPublic",
                table: "ReportTemplates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_ReportType",
                table: "ReportTemplates",
                column: "ReportType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportExecutions");

            migrationBuilder.DropTable(
                name: "ReportSubscriptions");

            migrationBuilder.DropTable(
                name: "ReportTemplates");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ReportSchedules");
        }
    }
}
