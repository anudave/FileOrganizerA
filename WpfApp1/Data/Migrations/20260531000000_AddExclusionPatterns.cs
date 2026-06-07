using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusionPatterns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExclusionPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatternName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Pattern = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExclusionPatterns", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExclusionPatterns");
        }
    }
}
