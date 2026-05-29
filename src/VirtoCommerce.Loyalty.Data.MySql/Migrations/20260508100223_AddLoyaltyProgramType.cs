using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyProgramType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgramType",
                table: "LoyaltyProgram",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramProductFactor",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoyaltyProgramId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Factor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramProductFactor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramProductFactor_LoyaltyProgram_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramProductFactor_LoyaltyProgramId",
                table: "LoyaltyProgramProductFactor",
                column: "LoyaltyProgramId");

            migrationBuilder.Sql("UPDATE `LoyaltyProgram` SET `ProgramType` = 'Default'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyProgramProductFactor");

            migrationBuilder.DropColumn(
                name: "ProgramType",
                table: "LoyaltyProgram");
        }
    }
}
