using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class FixUserBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBalance_AspNetUsers_UserAccountId",
                table: "UserBalance");

            migrationBuilder.DropIndex(
                name: "IX_UserBalance_UserAccountId",
                table: "UserBalance");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "UserBalance");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBalance_AspNetUsers_Id",
                table: "UserBalance",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBalance_AspNetUsers_Id",
                table: "UserBalance");

            migrationBuilder.AddColumn<string>(
                name: "UserAccountId",
                table: "UserBalance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalance_UserAccountId",
                table: "UserBalance",
                column: "UserAccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBalance_AspNetUsers_UserAccountId",
                table: "UserBalance",
                column: "UserAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
