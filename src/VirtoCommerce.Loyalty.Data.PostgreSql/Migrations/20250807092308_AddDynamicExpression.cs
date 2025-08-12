using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.PostgreSql.Migrations
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
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "LoyaltyProgram",
                type: "integer",
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
