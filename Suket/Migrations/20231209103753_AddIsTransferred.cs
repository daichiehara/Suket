using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTransferred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionRecord_Post_PostId",
                table: "TransactionRecord");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "TransactionRecord",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransferred",
                table: "TransactionRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionRecord_Post_PostId",
                table: "TransactionRecord",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionRecord_Post_PostId",
                table: "TransactionRecord");

            migrationBuilder.DropColumn(
                name: "IsTransferred",
                table: "TransactionRecord");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "TransactionRecord",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionRecord_Post_PostId",
                table: "TransactionRecord",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "PostId");
        }
    }
}
