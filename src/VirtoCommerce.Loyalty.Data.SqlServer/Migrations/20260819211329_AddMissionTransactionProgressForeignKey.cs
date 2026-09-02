using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionTransactionProgressForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "MissionProgressId",
                table: "LoyaltyMissionTransaction",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProgressId",
                table: "LoyaltyMissionTransaction",
                column: "MissionProgressId",
                principalTable: "LoyaltyMissionProgress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                table: "LoyaltyMissionTransaction",
                column: "MissionId",
                principalTable: "LoyaltyMission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProgressId",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "MissionProgressId",
                table: "LoyaltyMissionTransaction",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                table: "LoyaltyMissionTransaction",
                column: "MissionId",
                principalTable: "LoyaltyMission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
