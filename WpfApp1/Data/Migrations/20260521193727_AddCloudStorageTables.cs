using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudStorageTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudOrganizationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CloudStorageAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    SourcePath = table.Column<string>(type: "TEXT", nullable: false),
                    DestinationPath = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudOrganizationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CloudStorageAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    TokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConnectedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalFilesSynced = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudStorageAccounts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudOrganizationLogs");

            migrationBuilder.DropTable(
                name: "CloudStorageAccounts");
        }
    }
}
