using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountryhouseServiceAPI.Migrations
{
    public partial class adddatetorequestsandpreviewavatarsourcetousers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PreviewAvatarSource",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PreviewAvatarSource",
                table: "AspNetUsers");
        }
    }
}
