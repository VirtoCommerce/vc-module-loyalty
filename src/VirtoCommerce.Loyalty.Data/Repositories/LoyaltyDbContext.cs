using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Extensions;

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
        modelBuilder.Entity<LoyaltyProgramEntity>().ToAuditableEntityTable("LoyaltyProgram");

        modelBuilder.Entity<TransactionLogEntity>(builder =>
        {
            builder.ToAuditableEntityTable("LoyaltyTransactions");
            builder.HasIndex(t => new { t.ObjectType, t.ObjectId, t.OperationType }).IsUnique();
        });

        modelBuilder.Entity<LoyaltyProgramStoreEntity>().ToTable("LoyaltyProgramStore");
        modelBuilder.Entity<LoyaltyProgramStoreEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<LoyaltyProgramStoreEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();
        modelBuilder.Entity<LoyaltyProgramStoreEntity>().HasOne(x => x.LoyaltyProgram)
            .WithMany(x => x.Stores).HasForeignKey(x => x.LoyaltyProgramId)
            .OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<LoyaltyProgramStoreEntity>().HasIndex(i => i.StoreId);

        modelBuilder.Entity<LoyaltyProgramLocalizedNameEntity>(builder =>
        {
            builder.ToEntityTable("LoyaltyProgramLocalizedName");

            builder.HasOne(x => x.ParentEntity)
                .WithMany(x => x.LocalizedNames)
                .HasForeignKey(x => x.ParentEntityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
                .HasDatabaseName("IX_LoyaltyProgramLocalizedName_LanguageCode_ParentEntityId");
        });

        base.OnModelCreating(modelBuilder);

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
