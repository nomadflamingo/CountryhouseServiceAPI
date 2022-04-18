using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryhouseServiceAPI.Migrations
{
    public partial class authorandstatusforadrequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Ads_AdId",
                table: "Requests");

            migrationBuilder.AlterColumn<int>(
                name: "AdId",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Ads_AdId",
                table: "Requests",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Ads_AdId",
                table: "Requests");

            migrationBuilder.AlterColumn<int>(
                name: "AdId",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Ads_AdId",
                table: "Requests",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
