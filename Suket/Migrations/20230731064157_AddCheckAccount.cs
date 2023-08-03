using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChargesEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DetailsSubmitted",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargesEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DetailsSubmitted",
                table: "AspNetUsers");
        }
    }
}
