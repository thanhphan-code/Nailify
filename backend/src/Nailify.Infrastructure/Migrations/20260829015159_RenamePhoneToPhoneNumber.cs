using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nailify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePhoneToPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Users",
                newName: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Users",
                newName: "Phone");
        }
    }
}
