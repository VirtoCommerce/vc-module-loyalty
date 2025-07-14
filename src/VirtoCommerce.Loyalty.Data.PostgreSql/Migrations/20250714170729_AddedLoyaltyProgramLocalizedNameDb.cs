using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddedLoyaltyProgramLocalizedNameDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyProgramLocalizedNameEntity_LoyaltyProgram_ParentEnti~",
                table: "LoyaltyProgramLocalizedNameEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyProgramLocalizedNameEntity",
                table: "LoyaltyProgramLocalizedNameEntity");

            migrationBuilder.RenameTable(
                name: "LoyaltyProgramLocalizedNameEntity",
                newName: "LoyaltyProgramLocalizedName");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyProgramLocalizedNameEntity_ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                newName: "IX_LoyaltyProgramLocalizedName_ParentEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                type: "character varying(128)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "LoyaltyProgramLocalizedName",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyProgramLocalizedName",
                table: "LoyaltyProgramLocalizedName",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyProgramLocalizedName_LoyaltyProgram_ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                column: "ParentEntityId",
                principalTable: "LoyaltyProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyProgramLocalizedName_LoyaltyProgram_ParentEntityId",
                table: "LoyaltyProgramLocalizedName");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyProgramLocalizedName",
                table: "LoyaltyProgramLocalizedName");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyProgramLocalizedName");

            migrationBuilder.RenameTable(
                name: "LoyaltyProgramLocalizedName",
                newName: "LoyaltyProgramLocalizedNameEntity");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyProgramLocalizedName_ParentEntityId",
                table: "LoyaltyProgramLocalizedNameEntity",
                newName: "IX_LoyaltyProgramLocalizedNameEntity_ParentEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "ParentEntityId",
                table: "LoyaltyProgramLocalizedNameEntity",
                type: "character varying(128)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "LoyaltyProgramLocalizedNameEntity",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyProgramLocalizedNameEntity",
                table: "LoyaltyProgramLocalizedNameEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyProgramLocalizedNameEntity_LoyaltyProgram_ParentEnti~",
                table: "LoyaltyProgramLocalizedNameEntity",
                column: "ParentEntityId",
                principalTable: "LoyaltyProgram",
                principalColumn: "Id");
        }
    }
}
