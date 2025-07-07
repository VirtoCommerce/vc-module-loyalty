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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoyaltyProgramEntity>().ToAuditableEntityTable("LoyaltyProgram");

        modelBuilder.Entity<ConditionEntity>().ToAuditableEntityTable("Condition");
        modelBuilder.Entity<ConditionEntity>().HasOne(x => x.LoyaltyProgram).WithMany(x => x.Conditions)
             .HasForeignKey(x => x.LoyaltyProgramId).IsRequired().OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RewardTypeEntity>().ToAuditableEntityTable("RewardType");
        modelBuilder.Entity<RewardTypeEntity>().HasOne(x => x.LoyaltyProgram).WithMany(x => x.RewardTypes)
             .HasForeignKey(x => x.LoyaltyProgramId).IsRequired().OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoyaltyProgramUserGroupEntity>().ToEntityTable("LoyaltyProgramUserGroup");
        modelBuilder.Entity<LoyaltyProgramUserGroupEntity>().HasOne(x => x.Condition).WithMany(x => x.UserGroups)
            .HasForeignKey(x => x.ConditionId).IsRequired().OnDelete(DeleteBehavior.Cascade);

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
