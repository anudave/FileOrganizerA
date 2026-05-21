using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileOrganizationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileOrganizationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileOrganizationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FilePattern = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DestinationFolder = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileOrganizationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileOrganizationSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduleName = table.Column<string>(type: "TEXT", nullable: false),
                    TargetFolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduleType = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "TEXT", nullable: false),
                    IntervalHours = table.Column<int>(type: "INTEGER", nullable: false),
                    RunOnce = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastRunStatus = table.Column<string>(type: "TEXT", nullable: false),
                    LastRunMessage = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileOrganizationSchedules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileOrganizationLogs");

            migrationBuilder.DropTable(
                name: "FileOrganizationRules");

            migrationBuilder.DropTable(
                name: "FileOrganizationSchedules");
        }
    }
}
