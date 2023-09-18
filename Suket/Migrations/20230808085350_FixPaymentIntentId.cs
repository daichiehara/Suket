using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentIntentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChargeId",
                table: "Post",
                newName: "PaymentIntentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentIntentId",
                table: "Post",
                newName: "ChargeId");
        }
    }
}
