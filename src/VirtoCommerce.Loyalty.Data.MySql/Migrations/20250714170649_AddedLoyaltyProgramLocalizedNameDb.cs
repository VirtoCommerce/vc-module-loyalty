using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddedLoyaltyProgramLocalizedNameDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyProgramLocalizedNameEntity_LoyaltyProgram_ParentEntit~",
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

            migrationBuilder.UpdateData(
                table: "LoyaltyProgramLocalizedName",
                keyColumn: "ParentEntityId",
                keyValue: null,
                column: "ParentEntityId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ParentEntityId",
                table: "LoyaltyProgramLocalizedName",
                type: "varchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "LoyaltyProgramLocalizedName",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(95)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "LoyaltyProgramLocalizedNameEntity",
                type: "varchar(95)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyProgramLocalizedNameEntity",
                table: "LoyaltyProgramLocalizedNameEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyProgramLocalizedNameEntity_LoyaltyProgram_ParentEntit~",
                table: "LoyaltyProgramLocalizedNameEntity",
                column: "ParentEntityId",
                principalTable: "LoyaltyProgram",
                principalColumn: "Id");
        }
    }
}
