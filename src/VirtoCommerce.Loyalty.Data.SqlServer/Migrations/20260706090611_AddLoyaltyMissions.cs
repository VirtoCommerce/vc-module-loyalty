using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyProgramOperationLog_LoyaltyProgram_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramOperationLog_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.RenameColumn(
                name: "LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog",
                newName: "SourceType");

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "LoyaltyProgramOperationLog",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            // Backfill: the renamed SourceType column still holds the former LoyaltyProgramId values.
            migrationBuilder.Sql("UPDATE [LoyaltyProgramOperationLog] SET [SourceId] = [SourceType] WHERE [SourceType] IS NOT NULL;");
            migrationBuilder.Sql("UPDATE [LoyaltyProgramOperationLog] SET [SourceType] = 'LoyaltyProgram' WHERE [SourceId] IS NOT NULL;");

            migrationBuilder.CreateTable(
                name: "LoyaltyMission",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StoreId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Public = table.Column<bool>(type: "bit", nullable: false),
                    Periodicity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PredicateVisualTreeSerialized = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionGoalItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MissionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionGoalItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionGoalItem_LoyaltyMission_MissionId",
                        column: x => x.MissionId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionLocalizedDescription",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentEntityId = table.Column<string>(type: "nvarchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionLocalizedDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionLocalizedDescription_LoyaltyMission_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionLocalizedName",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentEntityId = table.Column<string>(type: "nvarchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionLocalizedName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionLocalizedName_LoyaltyMission_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionProgress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MissionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionProgress_LoyaltyMission_MissionId",
                        column: x => x.MissionId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MissionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MissionProgressId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ObjectId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ObjectType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ContributionValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionTransaction_LoyaltyMission_MissionId",
                        column: x => x.MissionId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionProgressItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MissionProgressId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionProgressItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionProgressItem_LoyaltyMissionProgress_MissionProgressId",
                        column: x => x.MissionProgressId,
                        principalTable: "LoyaltyMissionProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramOperationLog_SourceType_SourceId",
                table: "LoyaltyProgramOperationLog",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionGoalItem_MissionId_ProductId",
                table: "LoyaltyMissionGoalItem",
                columns: new[] { "MissionId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionLocalizedDescription_LanguageCode_ParentEntityId",
                table: "LoyaltyMissionLocalizedDescription",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionLocalizedDescription_ParentEntityId",
                table: "LoyaltyMissionLocalizedDescription",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionLocalizedName_LanguageCode_ParentEntityId",
                table: "LoyaltyMissionLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionLocalizedName_ParentEntityId",
                table: "LoyaltyMissionLocalizedName",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionProgress_MissionId_UserId_PeriodStart",
                table: "LoyaltyMissionProgress",
                columns: new[] { "MissionId", "UserId", "PeriodStart" },
                unique: true,
                filter: "[PeriodStart] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionProgressItem_MissionProgressId_ProductId",
                table: "LoyaltyMissionProgressItem",
                columns: new[] { "MissionProgressId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionTransaction_MissionId_ObjectId_UserId",
                table: "LoyaltyMissionTransaction",
                columns: new[] { "MissionId", "ObjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyMissionTransaction_MissionProgressId",
                table: "LoyaltyMissionTransaction",
                column: "MissionProgressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyMissionGoalItem");

            migrationBuilder.DropTable(
                name: "LoyaltyMissionLocalizedDescription");

            migrationBuilder.DropTable(
                name: "LoyaltyMissionLocalizedName");

            migrationBuilder.DropTable(
                name: "LoyaltyMissionProgressItem");

            migrationBuilder.DropTable(
                name: "LoyaltyMissionTransaction");

            migrationBuilder.DropTable(
                name: "LoyaltyMissionProgress");

            migrationBuilder.DropTable(
                name: "LoyaltyMission");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramOperationLog_SourceType_SourceId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.RenameColumn(
                name: "SourceType",
                table: "LoyaltyProgramOperationLog",
                newName: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramOperationLog_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog",
                column: "LoyaltyProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyProgramOperationLog_LoyaltyProgram_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog",
                column: "LoyaltyProgramId",
                principalTable: "LoyaltyProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
