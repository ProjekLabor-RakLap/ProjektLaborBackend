using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLaborBackend.Migrations
{
    /// <inheritdoc />
    public partial class added_whentowarn_or_notify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WhenToNotify",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WhenToWarn",
                table: "Stocks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhenToNotify",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "WhenToWarn",
                table: "Stocks");
        }
    }
}
