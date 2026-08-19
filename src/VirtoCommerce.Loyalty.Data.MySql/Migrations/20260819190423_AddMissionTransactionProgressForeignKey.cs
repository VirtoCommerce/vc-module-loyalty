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
            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProg~",
                table: "LoyaltyMissionTransaction",
                column: "MissionProgressId",
                principalTable: "LoyaltyMissionProgress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyMissionTransaction_LoyaltyMissionProgress_MissionProg~",
                table: "LoyaltyMissionTransaction");
        }
    }
}
