using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryhouseServiceAPI.Migrations
{
    public partial class addimageorder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "UseDefaultImage",
                table: "Ads");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageSource",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Order",
                table: "AdImages",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "PreviewImageSource",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "AdImages");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseDefaultImage",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_AuthorId",
                table: "Ads",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
