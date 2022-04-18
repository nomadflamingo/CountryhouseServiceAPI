using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryhouseServiceAPI.Migrations
{
    public partial class renamecodecolumntonamecolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "RequestStatuses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "AdStatuses",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "RequestStatuses",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AdStatuses",
                newName: "Code");
        }
    }
}
