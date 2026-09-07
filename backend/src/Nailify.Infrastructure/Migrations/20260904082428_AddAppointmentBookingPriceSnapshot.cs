using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentBookingPriceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DesignExtraPriceAtBooking",
                table: "Appointments",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_DesignExtraPrice_NonNegative",
                table: "Appointments",
                sql: "\"DesignExtraPriceAtBooking\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_DesignExtraPrice_NonNegative",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DesignExtraPriceAtBooking",
                table: "Appointments");
        }
    }
}
