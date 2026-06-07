using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateHandlingStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DuplicateHandlingStrategy",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Rename");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuplicateHandlingStrategy",
                table: "AppSettings");
        }
    }
}
