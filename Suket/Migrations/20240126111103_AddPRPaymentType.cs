using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suket.Migrations
{
    /// <inheritdoc />
    public partial class AddPRPaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "PaymentRecord",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 既存の PaymentRecord に対するデータ移行のロジックをここに記述

            migrationBuilder.Sql(@"
                UPDATE ""PaymentRecord""
                SET ""PaymentType"" = (
                    SELECT ""PaymentType""
                    FROM ""Post""
                    WHERE ""Post"".""PostId"" = ""PaymentRecord"".""PostId""
                )
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "PaymentRecord");
        }
    }
}
