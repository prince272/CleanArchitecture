using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenSourceSerialHash",
                table: "UserBearerToken",
                newName: "RefreshTokenSerialSourceHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenSerialSourceHash",
                table: "UserBearerToken",
                newName: "RefreshTokenSourceSerialHash");
        }
    }
}
