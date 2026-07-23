using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.Loyalty.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "LoyaltyProgramOperationLog",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "LoyaltyProgramOperationLog",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // fill SourceId by former LoyaltyProgramId values
            migrationBuilder.Sql("UPDATE `LoyaltyProgramOperationLog` SET `SourceId` = `LoyaltyProgramId`, `SourceType` = 'LoyaltyProgram' WHERE `LoyaltyProgramId` IS NOT NULL;");

            migrationBuilder.CreateTable(
                name: "LoyaltyMission",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoreId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Periodicity = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PredicateVisualTreeSerialized = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMission", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionGoalItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MissionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionLocalizedDescription",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentEntityId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionLocalizedDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionLocalizedDescription_LoyaltyMission_ParentEnti~",
                        column: x => x.ParentEntityId,
                        principalTable: "LoyaltyMission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionLocalizedName",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentEntityId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionProgress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MissionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MissionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MissionProgressId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContributionValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoyaltyMissionProgressItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MissionProgressId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    TargetQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyMissionProgressItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyMissionProgressItem_LoyaltyMissionProgress_MissionPro~",
                        column: x => x.MissionProgressId,
                        principalTable: "LoyaltyMissionProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                unique: true);

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

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyProgramOperationLog_LoyaltyProgram_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyProgramOperationLog_LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog");

            migrationBuilder.DropColumn(
                name: "LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoyaltyProgramId",
                table: "LoyaltyProgramOperationLog",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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
        }
    }
}
