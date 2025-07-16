using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDateFromTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "LoyaltyTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ObjectType_ObjectId_OperationType",
                table: "LoyaltyTransactions",
                newName: "IX_LoyaltyTransactions_ObjectType_ObjectId_OperationType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyTransactions",
                table: "LoyaltyTransactions",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyTransactions",
                table: "LoyaltyTransactions");

            migrationBuilder.RenameTable(
                name: "LoyaltyTransactions",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyTransactions_ObjectType_ObjectId_OperationType",
                table: "Transactions",
                newName: "IX_Transactions_ObjectType_ObjectId_OperationType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");
        }
    }
}
