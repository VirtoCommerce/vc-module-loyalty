using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
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

            migrationBuilder.UpdateData(
                table: "LoyaltyMissionTransaction",
                keyColumn: "MissionProgressId",
                keyValue: null,
                column: "MissionProgressId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "MissionProgressId",
                table: "LoyaltyMissionTransaction",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProg~",
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
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProg~",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                table: "LoyaltyMissionTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "MissionProgressId",
                table: "LoyaltyMissionTransaction",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
