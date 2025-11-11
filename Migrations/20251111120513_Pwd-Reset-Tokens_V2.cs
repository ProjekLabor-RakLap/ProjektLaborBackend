using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLaborBackend.Migrations
{
    /// <inheritdoc />
    public partial class PwdResetTokens_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PwdResetToken_Users_UserId",
                table: "PwdResetToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PwdResetToken",
                table: "PwdResetToken");

            migrationBuilder.RenameTable(
                name: "PwdResetToken",
                newName: "PwdResetTokens");

            migrationBuilder.RenameIndex(
                name: "IX_PwdResetToken_UserId",
                table: "PwdResetTokens",
                newName: "IX_PwdResetTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PwdResetTokens",
                table: "PwdResetTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PwdResetTokens_Users_UserId",
                table: "PwdResetTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PwdResetTokens_Users_UserId",
                table: "PwdResetTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PwdResetTokens",
                table: "PwdResetTokens");

            migrationBuilder.RenameTable(
                name: "PwdResetTokens",
                newName: "PwdResetToken");

            migrationBuilder.RenameIndex(
                name: "IX_PwdResetTokens_UserId",
                table: "PwdResetToken",
                newName: "IX_PwdResetToken_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PwdResetToken",
                table: "PwdResetToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PwdResetToken_Users_UserId",
                table: "PwdResetToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
