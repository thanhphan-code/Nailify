using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceLevelReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppointmentId",
                table: "Reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "Reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Reviews" AS r
                SET "ServiceId" = source."ServiceId"
                FROM (
                    SELECT DISTINCT ON ("AppointmentId") "AppointmentId", "ServiceId"
                    FROM "AppointmentServices"
                    ORDER BY "AppointmentId", "ServiceId"
                ) AS source
                WHERE r."AppointmentId" = source."AppointmentId";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppointmentId_ServiceId",
                table: "Reviews",
                columns: new[] { "AppointmentId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ServiceId",
                table: "Reviews",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Services_ServiceId",
                table: "Reviews",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Services_ServiceId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppointmentId_ServiceId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ServiceId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppointmentId",
                table: "Reviews",
                column: "AppointmentId",
                unique: true);
        }
    }
}
