using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDetails",
                table: "Payment");

            migrationBuilder.AddColumn<string>(
                name: "CheckoutId",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtensionData",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ExtensionData",
                table: "Payment");

            migrationBuilder.AddColumn<bool>(
                name: "HasDetails",
                table: "Payment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
