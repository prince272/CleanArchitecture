using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "Payment",
                newName: "ReferenceCode");

            migrationBuilder.AddColumn<string>(
                name: "AccessCode",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessCode",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "ReferenceCode",
                table: "Payment",
                newName: "Reference");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
