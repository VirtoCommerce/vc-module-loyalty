using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyProgramProductFactorUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramProductFactor_LoyaltyProgramId",
                table: "LoyaltyProgramProductFactor");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramProductFactor_LoyaltyProgramId_ProductId",
                table: "LoyaltyProgramProductFactor",
                columns: new[] { "LoyaltyProgramId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramProductFactor_LoyaltyProgramId_ProductId",
                table: "LoyaltyProgramProductFactor");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramProductFactor_LoyaltyProgramId",
                table: "LoyaltyProgramProductFactor",
                column: "LoyaltyProgramId");
        }
    }
}
