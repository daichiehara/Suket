using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class AddRefunded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Refunded",
                table: "PaymentRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Refunded",
                table: "PaymentRecord");
        }
    }
}
