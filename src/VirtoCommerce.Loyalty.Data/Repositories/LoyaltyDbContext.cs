using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Loyalty.Data.Models;

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
