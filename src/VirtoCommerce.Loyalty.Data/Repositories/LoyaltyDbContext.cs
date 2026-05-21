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

        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>().ToTable("LoyaltyProgramOperationLog").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>().HasOne(x => x.LoyaltyProgram).WithMany()
            .HasForeignKey(x => x.LoyaltyProgramId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>().Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>().Property(x => x.Balance).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyProgramOperationLogEntity>()
             .HasIndex(x => new { x.ObjectId, x.ObjectType, x.OperationType }).IsUnique()
             .HasDatabaseName("IX_LoyaltyProgramOperationLog_ObjectId_ObjectType_OperationType");

        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().ToTable("LoyaltyProgramProductFactor").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().HasOne(x => x.LoyaltyProgram).WithMany()
            .HasForeignKey(x => x.LoyaltyProgramId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>().Property(x => x.Factor).HasPrecision(18, 2);
        modelBuilder.Entity<LoyaltyProgramProductFactorEntity>()
            .HasIndex(x => new { x.LoyaltyProgramId, x.ProductId }).IsUnique()
            .HasDatabaseName("IX_LoyaltyProgramProductFactor_LoyaltyProgramId_ProductId");

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
