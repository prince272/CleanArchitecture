using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBearerToken");

            migrationBuilder.CreateTable(
                name: "BearerToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RefreshTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BearerToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BearerToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BearerToken_UserId",
                table: "BearerToken",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BearerToken");

            migrationBuilder.CreateTable(
                name: "UserBearerToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccessTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AccessTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiresDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RefreshTokenSerialHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenSerialSourceHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBearerToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBearerToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBearerToken_UserId",
                table: "UserBearerToken",
                column: "UserId");
        }
    }
}
