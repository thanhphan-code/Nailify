using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenBookingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesignAdditionalDurationAtBooking",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_DesignAdditionalDuration_NonNegative",
                table: "Appointments",
                sql: "\"DesignAdditionalDurationAtBooking\" >= 0");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql("""
                ALTER TABLE "Appointments"
                ADD CONSTRAINT "EX_Appointments_ActiveStaffTime"
                EXCLUDE USING gist (
                    "StaffId" WITH =,
                    tsrange("AppointmentDate" + "StartTime", "AppointmentDate" + "EndTime", '[)') WITH &&
                )
                WHERE ("StaffId" IS NOT NULL AND "Status" IN (0, 1, 2));
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "Appointments"
                ADD CONSTRAINT "EX_Appointments_ActiveCustomerTime"
                EXCLUDE USING gist (
                    "CustomerId" WITH =,
                    tsrange("AppointmentDate" + "StartTime", "AppointmentDate" + "EndTime", '[)') WITH &&
                )
                WHERE ("Status" IN (0, 1, 2));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Appointments\" DROP CONSTRAINT IF EXISTS \"EX_Appointments_ActiveCustomerTime\";");
            migrationBuilder.Sql("ALTER TABLE \"Appointments\" DROP CONSTRAINT IF EXISTS \"EX_Appointments_ActiveStaffTime\";");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_DesignAdditionalDuration_NonNegative",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DesignAdditionalDurationAtBooking",
                table: "Appointments");
        }
    }
}
