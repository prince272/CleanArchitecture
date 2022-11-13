using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Data.Migrations
{
    public partial class Migration_19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ready",
                table: "Payment",
                newName: "HasDetails");

            migrationBuilder.AlterColumn<int>(
                name: "Method",
                table: "Payment",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledOn",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedOn",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Payment",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiredOn",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gateway",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnUrl",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledOn",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CompletedOn",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ExpiredOn",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Gateway",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ReturnUrl",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "HasDetails",
                table: "Payment",
                newName: "Ready");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
