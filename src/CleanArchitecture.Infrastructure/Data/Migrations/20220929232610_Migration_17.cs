using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payment",
                newName: "Details_TransactionId");

            migrationBuilder.RenameColumn(
                name: "ExtensionData",
                table: "Payment",
                newName: "Details_MobileNumber");

            migrationBuilder.AddColumn<string>(
                name: "Details_Method",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_MobileIssuer_Code",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_MobileIssuer_Name",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_MobileIssuer_Pattern",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details_Method",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Details_MobileIssuer_Code",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Details_MobileIssuer_Name",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Details_MobileIssuer_Pattern",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "Details_TransactionId",
                table: "Payment",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "Details_MobileNumber",
                table: "Payment",
                newName: "ExtensionData");
        }
    }
}
