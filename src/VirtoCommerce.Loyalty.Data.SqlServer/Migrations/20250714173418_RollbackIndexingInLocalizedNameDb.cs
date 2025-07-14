using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RollbackIndexingInLocalizedNameDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName");
        }
    }
}
