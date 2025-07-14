using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIndexFromLocalizedDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);
        }
    }
}
