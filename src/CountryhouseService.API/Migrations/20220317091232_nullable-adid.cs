using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryhouseServiceAPI.Migrations
{
    public partial class nullableadid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages");

            migrationBuilder.AlterColumn<int>(
                name: "AdId",
                table: "AdImages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages");

            migrationBuilder.AlterColumn<int>(
                name: "AdId",
                table: "AdImages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
