using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresDateTime",
                table: "BearerToken",
                newName: "RefreshTokenExpires");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpiresDateTime",
                table: "BearerToken",
                newName: "AccessTokenExpires");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpires",
                table: "BearerToken",
                newName: "RefreshTokenExpiresDateTime");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpires",
                table: "BearerToken",
                newName: "AccessTokenExpiresDateTime");
        }
    }
}
