using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class AddChargeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChargeId",
                table: "Post",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeId",
                table: "Post");
        }
    }
}
