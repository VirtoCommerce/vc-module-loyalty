using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public class LoyaltyDbContext : DbContextBase
{
    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options)
        : base(options)
    {
    }

    protected LoyaltyDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoyaltyProgramEntity>().ToTable("LoyaltyProgram").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

        modelBuilder.Entity<LoyaltyProgramLocalizedNameEntity>().ToEntityTable("LoyaltyProgramLocalizedName");
        modelBuilder.Entity<LoyaltyProgramLocalizedNameEntity>().HasOne(x => x.ParentEntity).WithMany(x => x.LocalizedNames)
            .HasForeignKey(x => x.ParentEntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyProgramLocalizedNameEntity>()
            .HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId");

        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>().ToTable("LoyaltyBalanceOperationLog").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>().Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>().Property(x => x.Balance).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>()
             .HasIndex(x => new { x.ObjectId, x.ObjectType, x.OperationType }).IsUnique()
             .HasDatabaseName("IX_LoyaltyBalanceOperationLog_ObjectId_ObjectType_OperationType");
        modelBuilder.Entity<LoyaltyBalanceOperationLogEntity>()
             .HasIndex(x => new { x.SourceType, x.SourceId })
             .HasDatabaseName("IX_LoyaltyBalanceOperationLog_SourceType_SourceId");

        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().ToTable("LoyaltyProgramProductFactor").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().HasOne(x => x.LoyaltyProgram).WithMany()
            .HasForeignKey(x => x.LoyaltyProgramId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().Property(x => x.Factor).HasPrecision(18, 2);
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>()
            .HasIndex(x => new { x.LoyaltyProgramId, x.ProductId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyProgramProductFactor_LoyaltyProgramId_ProductId");

        modelBuilder.Entity<LoyaltyMissionEntity>().ToTable("LoyaltyMission").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyMissionEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

        modelBuilder.Entity<LoyaltyMissionLocalizedNameEntity>().ToEntityTable("LoyaltyMissionLocalizedName");
        modelBuilder.Entity<LoyaltyMissionLocalizedNameEntity>().HasOne(x => x.ParentEntity).WithMany(x => x.LocalizedNames)
            .HasForeignKey(x => x.ParentEntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionLocalizedNameEntity>()
            .HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionLocalizedName_LanguageCode_ParentEntityId");

        modelBuilder.Entity<LoyaltyMissionLocalizedDescriptionEntity>().ToEntityTable("LoyaltyMissionLocalizedDescription");
        modelBuilder.Entity<LoyaltyMissionLocalizedDescriptionEntity>().HasOne(x => x.ParentEntity).WithMany(x => x.LocalizedDescriptions)
            .HasForeignKey(x => x.ParentEntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionLocalizedDescriptionEntity>()
            .HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionLocalizedDescription_LanguageCode_ParentEntityId");

        modelBuilder.Entity<LoyaltyMissionGoalItemEntity>().ToTable("LoyaltyMissionGoalItem").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyMissionGoalItemEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyMissionGoalItemEntity>().HasOne(x => x.Mission).WithMany()
            .HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionGoalItemEntity>()
            .HasIndex(x => new { x.MissionId, x.ProductId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionGoalItem_MissionId_ProductId");

        modelBuilder.Entity<LoyaltyMissionProgressEntity>().ToTable("LoyaltyMissionProgress").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyMissionProgressEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyMissionProgressEntity>().HasOne(x => x.Mission).WithMany()
            .HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionProgressEntity>().Property(x => x.CurrentValue).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyMissionProgressEntity>().Property(x => x.TargetValue).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyMissionProgressEntity>().Property(x => x.Percentage).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyMissionProgressEntity>()
            .HasIndex(x => new { x.MissionId, x.UserId, x.PeriodStart }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionProgress_MissionId_UserId_PeriodStart");

        modelBuilder.Entity<LoyaltyMissionProgressItemEntity>().ToTable("LoyaltyMissionProgressItem").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyMissionProgressItemEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyMissionProgressItemEntity>().HasOne(x => x.MissionProgress).WithMany(x => x.Items)
            .HasForeignKey(x => x.MissionProgressId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionProgressItemEntity>()
            .HasIndex(x => new { x.MissionProgressId, x.ProductId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionProgressItem_MissionProgressId_ProductId");

        modelBuilder.Entity<LoyaltyMissionTransactionEntity>().ToTable("LoyaltyMissionTransaction").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyMissionTransactionEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyMissionTransactionEntity>().HasOne(x => x.Mission).WithMany()
            .HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyMissionTransactionEntity>().Property(x => x.ContributionValue).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyMissionTransactionEntity>()
            .HasIndex(x => new { x.MissionId, x.ObjectId, x.UserId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyMissionTransaction_MissionId_ObjectId_UserId");
        modelBuilder.Entity<LoyaltyMissionTransactionEntity>()
            .HasIndex(x => x.MissionProgressId)
            .HasDatabaseName("IX_LoyaltyMissionTransaction_MissionProgressId");

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.Loyalty.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.Loyalty.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.Loyalty.Data.SqlServer"));
                break;
        }
    }
}
