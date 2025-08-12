using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicExpression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PredicateVisualTreeSerialized",
                table: "LoyaltyProgram",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "LoyaltyProgram",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredicateVisualTreeSerialized",
                table: "LoyaltyProgram");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "LoyaltyProgram");
        }
    }
}
