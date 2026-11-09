using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaceDay.API.Data;

#nullable disable

namespace RaceDay.API.Migrations;

[DbContext(typeof(RaceDayDbContext))]
[Migration("20261109000000_AddPart3ImageUrls")]
public partial class AddPart3ImageUrls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BannerImageUrl", table: "Events", type: "nvarchar(2048)",
            maxLength: 2048, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "ProfilePictureUrl", table: "UserProfiles", type: "nvarchar(2048)",
            maxLength: 2048, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BannerImageUrl", table: "Events");
        migrationBuilder.DropColumn(name: "ProfilePictureUrl", table: "UserProfiles");
    }
}
