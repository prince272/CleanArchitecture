using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Details_TransactionId",
                table: "Payment",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "Details_MobileNumber",
                table: "Payment",
                newName: "MobileNumber");

            migrationBuilder.RenameColumn(
                name: "Details_MobileIssuer_Pattern",
                table: "Payment",
                newName: "MobileIssuer_Pattern");

            migrationBuilder.RenameColumn(
                name: "Details_MobileIssuer_Name",
                table: "Payment",
                newName: "MobileIssuer_Name");

            migrationBuilder.RenameColumn(
                name: "Details_MobileIssuer_Code",
                table: "Payment",
                newName: "MobileIssuer_Code");

            migrationBuilder.RenameColumn(
                name: "Details_Method",
                table: "Payment",
                newName: "Method");

            migrationBuilder.AddColumn<bool>(
                name: "Ready",
                table: "Payment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ready",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payment",
                newName: "Details_TransactionId");

            migrationBuilder.RenameColumn(
                name: "MobileNumber",
                table: "Payment",
                newName: "Details_MobileNumber");

            migrationBuilder.RenameColumn(
                name: "MobileIssuer_Pattern",
                table: "Payment",
                newName: "Details_MobileIssuer_Pattern");

            migrationBuilder.RenameColumn(
                name: "MobileIssuer_Name",
                table: "Payment",
                newName: "Details_MobileIssuer_Name");

            migrationBuilder.RenameColumn(
                name: "MobileIssuer_Code",
                table: "Payment",
                newName: "Details_MobileIssuer_Code");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "Payment",
                newName: "Details_Method");
        }
    }
}
