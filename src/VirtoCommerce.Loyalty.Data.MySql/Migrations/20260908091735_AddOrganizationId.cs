using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "LoyaltyBalanceOperationLog",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");



            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "LoyaltyMissionTransaction",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "LoyaltyMissionProgress",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "LoyaltyMissionProgress",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `LoyaltyMissionProgress` SET `OwnerId` = `UserId`;");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionProgress_MissionId_OwnerId_PeriodStart",
                table: "LoyaltyMissionProgress",
                columns: new[] { "MissionId", "OwnerId", "PeriodStart" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyMissionProgress_MissionId_UserId_PeriodStart",
                table: "LoyaltyMissionProgress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionProgress_MissionId_UserId_PeriodStart",
                table: "LoyaltyMissionProgress",
                columns: new[] { "MissionId", "UserId", "PeriodStart" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyMissionProgress_MissionId_OwnerId_PeriodStart",
                table: "LoyaltyMissionProgress");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "LoyaltyMissionProgress");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "LoyaltyMissionProgress");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "LoyaltyBalanceOperationLog");
        }
    }
}
