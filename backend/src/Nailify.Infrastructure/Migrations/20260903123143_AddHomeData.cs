using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBookable",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AdditionalDurationMinutes",
                table: "NailDesigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "NailDesigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "NailDesigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CustomerFavoriteDesigns",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    NailDesignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFavoriteDesigns", x => new { x.CustomerId, x.NailDesignId });
                    table.ForeignKey(
                        name: "FK_CustomerFavoriteDesigns_NailDesigns_NailDesignId",
                        column: x => x.NailDesignId,
                        principalTable: "NailDesigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerFavoriteDesigns_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_IsActive_IsBookable_IsFeatured_DisplayOrder",
                table: "Services",
                columns: new[] { "IsActive", "IsBookable", "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsApproved_IsVisible_IsFeatured_CreatedAt",
                table: "Reviews",
                columns: new[] { "IsApproved", "IsVisible", "IsFeatured", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NailDesigns_Status_IsFeatured_DisplayOrder",
                table: "NailDesigns",
                columns: new[] { "Status", "IsFeatured", "DisplayOrder" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_NailDesigns_AdditionalDuration_NonNegative",
                table: "NailDesigns",
                sql: "\"AdditionalDurationMinutes\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFavoriteDesigns_CustomerId_NailDesignId",
                table: "CustomerFavoriteDesigns",
                columns: new[] { "CustomerId", "NailDesignId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFavoriteDesigns_NailDesignId",
                table: "CustomerFavoriteDesigns",
                column: "NailDesignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFavoriteDesigns");

            migrationBuilder.DropIndex(
                name: "IX_Services_IsActive_IsBookable_IsFeatured_DisplayOrder",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_IsApproved_IsVisible_IsFeatured_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_NailDesigns_Status_IsFeatured_DisplayOrder",
                table: "NailDesigns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NailDesigns_AdditionalDuration_NonNegative",
                table: "NailDesigns");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsBookable",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AdditionalDurationMinutes",
                table: "NailDesigns");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "NailDesigns");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "NailDesigns");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");
        }
    }
}
