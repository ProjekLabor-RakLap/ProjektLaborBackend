using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLaborBackend.Migrations
{
    /// <inheritdoc />
    public partial class added_producktid_to_emaillog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "EmailLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "EmailLogs");
        }
    }
}
