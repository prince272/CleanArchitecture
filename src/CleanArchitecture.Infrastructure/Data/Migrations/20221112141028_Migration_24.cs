using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_24 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image_Height",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Image_MimeType",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Image_Name",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Image_Size",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Image_Type",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Image_Width",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "RegisteredOn",
                table: "User",
                newName: "RegisteredAt");

            migrationBuilder.RenameColumn(
                name: "Image_Path",
                table: "Product",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Payment",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ExpiredOn",
                table: "Payment",
                newName: "ExpiredAt");

            migrationBuilder.RenameColumn(
                name: "DeclinedOn",
                table: "Payment",
                newName: "DeclinedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Payment",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CompletedOn",
                table: "Payment",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresOn",
                table: "BearerToken",
                newName: "RefreshTokenExpiresAt");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpiresOn",
                table: "BearerToken",
                newName: "AccessTokenExpiresAt");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Product",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Product",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "Product",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Product",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => new { x.ProductId, x.Id });
                    table.ForeignKey(
                        name: "FK_Media_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "RegisteredAt",
                table: "User",
                newName: "RegisteredOn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Product",
                newName: "Image_Path");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Payment",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "ExpiredAt",
                table: "Payment",
                newName: "ExpiredOn");

            migrationBuilder.RenameColumn(
                name: "DeclinedAt",
                table: "Payment",
                newName: "DeclinedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Payment",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "Payment",
                newName: "CompletedOn");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiresAt",
                table: "BearerToken",
                newName: "RefreshTokenExpiresOn");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpiresAt",
                table: "BearerToken",
                newName: "AccessTokenExpiresOn");

            migrationBuilder.AddColumn<int>(
                name: "Image_Height",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image_MimeType",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image_Name",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Image_Size",
                table: "Product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Image_Type",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Image_Width",
                table: "Product",
                type: "int",
                nullable: true);
        }
    }
}
