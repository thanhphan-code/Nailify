using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_NailDesigns_NailDesignId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_NailDesigns_Categories_CategoryId",
                table: "NailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceNailDesigns_NailDesigns_ApplicableNailDesignsId",
                table: "ServiceNailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceNailDesigns_Services_ApplicableServicesId",
                table: "ServiceNailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffProfiles_Users_UserId",
                table: "StaffProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffSchedules_Users_StaffId",
                table: "StaffSchedules");

            migrationBuilder.DropIndex(
                name: "IX_StaffSchedules_StaffId",
                table: "StaffSchedules");

            migrationBuilder.DropIndex(
                name: "IX_NailDesigns_CategoryId",
                table: "NailDesigns");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CustomerId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffId_AppointmentDate",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "ApplicableServicesId",
                table: "ServiceNailDesigns",
                newName: "NailDesignId");

            migrationBuilder.RenameColumn(
                name: "ApplicableNailDesignsId",
                table: "ServiceNailDesigns",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceNailDesigns_ApplicableServicesId",
                table: "ServiceNailDesigns",
                newName: "IX_ServiceNailDesigns_NailDesignId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Specialty",
                table: "StaffProfiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "StaffProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "StaffProfiles",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NailDesigns",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "NailDesigns",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExtraPrice",
                table: "NailDesigns",
                type: "numeric(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffSchedules_StaffId_WorkDate_StartTime",
                table: "StaffSchedules",
                columns: new[] { "StaffId", "WorkDate", "StartTime" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StaffSchedules_EndTime_After_StartTime",
                table: "StaffSchedules",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name",
                table: "Services",
                column: "Name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_DurationMinutes_Positive",
                table: "Services",
                sql: "\"DurationMinutes\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_Price_NonNegative",
                table: "Services",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews",
                sql: "\"Rating\" BETWEEN 1 AND 5");

            migrationBuilder.CreateIndex(
                name: "IX_NailDesigns_CategoryId_Name",
                table: "NailDesigns",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NailDesigns_ExtraPrice_NonNegative",
                table: "NailDesigns",
                sql: "\"ExtraPrice\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CustomerId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "CustomerId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId_AppointmentDate_StartTime",
                table: "Appointments",
                columns: new[] { "StaffId", "AppointmentDate", "StartTime" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_EndTime_After_StartTime",
                table: "Appointments",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_TotalPrice_NonNegative",
                table: "Appointments",
                sql: "\"TotalPrice\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_NailDesigns_NailDesignId",
                table: "Appointments",
                column: "NailDesignId",
                principalTable: "NailDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NailDesigns_Categories_CategoryId",
                table: "NailDesigns",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceNailDesigns_NailDesigns_NailDesignId",
                table: "ServiceNailDesigns",
                column: "NailDesignId",
                principalTable: "NailDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceNailDesigns_Services_ServiceId",
                table: "ServiceNailDesigns",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffProfiles_Users_UserId",
                table: "StaffProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffSchedules_Users_StaffId",
                table: "StaffSchedules",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_NailDesigns_NailDesignId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_NailDesigns_Categories_CategoryId",
                table: "NailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceNailDesigns_NailDesigns_NailDesignId",
                table: "ServiceNailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceNailDesigns_Services_ServiceId",
                table: "ServiceNailDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffProfiles_Users_UserId",
                table: "StaffProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffSchedules_Users_StaffId",
                table: "StaffSchedules");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_StaffSchedules_StaffId_WorkDate_StartTime",
                table: "StaffSchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StaffSchedules_EndTime_After_StartTime",
                table: "StaffSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Services_Name",
                table: "Services");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_DurationMinutes_Positive",
                table: "Services");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_Price_NonNegative",
                table: "Services");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_NailDesigns_CategoryId_Name",
                table: "NailDesigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NailDesigns_ExtraPrice_NonNegative",
                table: "NailDesigns");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CustomerId_AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffId_AppointmentDate_StartTime",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_EndTime_After_StartTime",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_TotalPrice_NonNegative",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "NailDesignId",
                table: "ServiceNailDesigns",
                newName: "ApplicableServicesId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "ServiceNailDesigns",
                newName: "ApplicableNailDesignsId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceNailDesigns_NailDesignId",
                table: "ServiceNailDesigns",
                newName: "IX_ServiceNailDesigns_ApplicableServicesId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "Specialty",
                table: "StaffProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "StaffProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "StaffProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NailDesigns",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "NailDesigns",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExtraPrice",
                table: "NailDesigns",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffSchedules_StaffId",
                table: "StaffSchedules",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_NailDesigns_CategoryId",
                table: "NailDesigns",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CustomerId",
                table: "Appointments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "StaffId", "AppointmentDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_NailDesigns_NailDesignId",
                table: "Appointments",
                column: "NailDesignId",
                principalTable: "NailDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NailDesigns_Categories_CategoryId",
                table: "NailDesigns",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceNailDesigns_NailDesigns_ApplicableNailDesignsId",
                table: "ServiceNailDesigns",
                column: "ApplicableNailDesignsId",
                principalTable: "NailDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceNailDesigns_Services_ApplicableServicesId",
                table: "ServiceNailDesigns",
                column: "ApplicableServicesId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffProfiles_Users_UserId",
                table: "StaffProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffSchedules_Users_StaffId",
                table: "StaffSchedules",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
