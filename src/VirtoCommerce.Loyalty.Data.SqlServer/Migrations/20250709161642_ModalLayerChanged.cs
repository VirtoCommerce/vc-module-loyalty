using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ModalLayerChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyProgramUserGroup");

            migrationBuilder.DropTable(
                name: "RewardType");

            migrationBuilder.DropTable(
                name: "Condition");

            migrationBuilder.DropColumn(
                name: "LocalizedName",
                table: "LoyaltyProgram");

            migrationBuilder.AlterColumn<string>(
                name: "StoreId",
                table: "LoyaltyProgram",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LoyaltyProgram",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conditions",
                table: "LoyaltyProgram",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramLocalizedNameEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentEntityId = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramLocalizedNameEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramLocalizedNameEntity_LoyaltyProgram_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "LoyaltyProgram",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoyaltyProgramId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    AccruedPoints = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ObjectId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramLocalizedNameEntity_ParentEntityId",
                table: "LoyaltyProgramLocalizedNameEntity",
                column: "ParentEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyProgramLocalizedNameEntity");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropColumn(
                name: "Conditions",
                table: "LoyaltyProgram");

            migrationBuilder.AlterColumn<string>(
                name: "StoreId",
                table: "LoyaltyProgram",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LoyaltyProgram",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "LocalizedName",
                table: "LoyaltyProgram",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Condition",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoyaltyProgramId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFirstOrder = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Condition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Condition_LoyaltyProgram_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoyaltyProgramId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AmountType = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FixedPoints = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelativePoints = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardType_LoyaltyProgram_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramUserGroup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ConditionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramUserGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramUserGroup_Condition_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Condition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Condition_LoyaltyProgramId",
                table: "Condition",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramUserGroup_ConditionId",
                table: "LoyaltyProgramUserGroup",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardType_LoyaltyProgramId",
                table: "RewardType",
                column: "LoyaltyProgramId");
        }
    }
}
