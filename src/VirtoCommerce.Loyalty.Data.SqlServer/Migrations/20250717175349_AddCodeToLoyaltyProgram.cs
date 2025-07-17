using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToLoyaltyProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "LoyaltyTransactions");

            migrationBuilder.RenameColumn(
                name: "AccruedPoints",
                table: "LoyaltyTransactions",
                newName: "Points");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LoyaltyProgram",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "LoyaltyProgram");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "LoyaltyTransactions",
                newName: "AccruedPoints");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "LoyaltyTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
