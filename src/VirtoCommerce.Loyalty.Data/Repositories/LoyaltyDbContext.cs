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

        modelBuilder.Entity<LoyaltyProgramUsageEntity>().ToTable("LoyaltyProgramUsage").HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramUsageEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyProgramUsageEntity>().HasOne(x => x.LoyaltyProgram).WithMany()
            .HasForeignKey(x => x.LoyaltyProgramId).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
        modelBuilder.Entity<LoyaltyProgramUsageEntity>().Property(x => x.Points).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyProgramUsageEntity>().Property(x => x.Balance).HasColumnType("decimal").HasPrecision(18, 4);
        modelBuilder.Entity<LoyaltyProgramUsageEntity>()
             .HasIndex(x => new { x.ObjectId, x.ObjectType, x.UsageType }).IsUnique()
             .HasDatabaseName("IX_LoyaltyProgramUsage_ObjectId_ObjectType_UsageType");

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
