using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class LoyaltyProgramToStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "LoyaltyProgram");

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramStore",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LoyaltyProgramId = table.Column<string>(type: "character varying(128)", nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramStore_LoyaltyProgram_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramStore_LoyaltyProgramId",
                table: "LoyaltyProgramStore",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramStore_StoreId",
                table: "LoyaltyProgramStore",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyProgramStore");

            migrationBuilder.AddColumn<string>(
                name: "StoreId",
                table: "LoyaltyProgram",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }
    }
}
