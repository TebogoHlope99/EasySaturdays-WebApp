using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySaturdays.Migrations
{
    /// <inheritdoc />
    public partial class AdminRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NewAdminData",
                table: "AdminRequests",
                newName: "NewAdminEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NewAdminEmail",
                table: "AdminRequests",
                newName: "NewAdminData");
        }
    }
}
