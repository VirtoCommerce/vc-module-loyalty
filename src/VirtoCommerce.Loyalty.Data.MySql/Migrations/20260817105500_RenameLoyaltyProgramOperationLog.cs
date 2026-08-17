using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RenameLoyaltyProgramOperationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyProgramOperationLog",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.RenameTable(
                name: "LoyaltyProgramOperationLog",
                newName: "LoyaltyBalanceOperationLog");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyProgramOperationLog_ObjectId_ObjectType_OperationType",
                table: "LoyaltyBalanceOperationLog",
                newName: "IX_LoyaltyBalanceOperationLog_ObjectId_ObjectType_OperationType");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyProgramOperationLog_SourceType_SourceId",
                table: "LoyaltyBalanceOperationLog",
                newName: "IX_LoyaltyBalanceOperationLog_SourceType_SourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyBalanceOperationLog",
                table: "LoyaltyBalanceOperationLog",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyBalanceOperationLog",
                table: "LoyaltyBalanceOperationLog");

            migrationBuilder.RenameTable(
                name: "LoyaltyBalanceOperationLog",
                newName: "LoyaltyProgramOperationLog");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyBalanceOperationLog_ObjectId_ObjectType_OperationType",
                table: "LoyaltyProgramOperationLog",
                newName: "IX_LoyaltyProgramOperationLog_ObjectId_ObjectType_OperationType");

            migrationBuilder.RenameIndex(
                name: "IX_LoyaltyBalanceOperationLog_SourceType_SourceId",
                table: "LoyaltyProgramOperationLog",
                newName: "IX_LoyaltyProgramOperationLog_SourceType_SourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyProgramOperationLog",
                table: "LoyaltyProgramOperationLog",
                column: "Id");
        }
    }
}
