using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayOsPaymentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayOsCheckoutUrl",
                table: "DepositPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PayOsOrderCode",
                table: "DepositPayments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayOsPaymentLinkId",
                table: "DepositPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "DepositPayments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositPayments_PayOsOrderCode",
                table: "DepositPayments",
                column: "PayOsOrderCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepositPayments_PayOsOrderCode",
                table: "DepositPayments");

            migrationBuilder.DropColumn(
                name: "PayOsCheckoutUrl",
                table: "DepositPayments");

            migrationBuilder.DropColumn(
                name: "PayOsOrderCode",
                table: "DepositPayments");

            migrationBuilder.DropColumn(
                name: "PayOsPaymentLinkId",
                table: "DepositPayments");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "DepositPayments");
        }
    }
}
