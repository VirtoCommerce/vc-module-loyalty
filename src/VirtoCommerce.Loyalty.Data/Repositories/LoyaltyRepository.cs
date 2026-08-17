using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public class LoyaltyRepository(LoyaltyDbContext dbContext, IUnitOfWork unitOfWork = null)
    : DbContextRepositoryBase<LoyaltyDbContext>(dbContext, unitOfWork),
        ILoyaltyRepository
{
    public IQueryable<LoyaltyProgramEntity> LoyaltyPrograms => DbContext.Set<LoyaltyProgramEntity>();
    public IQueryable<LoyaltyBalanceOperationLogEntity> LoyaltyBalanceOperationLogs => DbContext.Set<LoyaltyBalanceOperationLogEntity>();

    public virtual async Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyPrograms.Include(x => x.LocalizedNames).Where(x => x.Id == ids.First()).AsSplitQuery().ToListAsync()
            : await LoyaltyPrograms.Include(x => x.LocalizedNames).Where(x => ids.Contains(x.Id)).AsSplitQuery().ToListAsync();
    }

    public virtual async Task<IList<LoyaltyBalanceOperationLogEntity>> GetLoyaltyBalanceOperationLogsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyBalanceOperationLogs.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyBalanceOperationLogs.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public IQueryable<LoyaltyProgramProductFactorEntity> LoyaltyProgramProductFactors => DbContext.Set<LoyaltyProgramProductFactorEntity>();

    public virtual async Task<IList<LoyaltyProgramProductFactorEntity>> GetLoyaltyProgramProductFactorsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyProgramProductFactors.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyProgramProductFactors.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public IQueryable<LoyaltyMissionEntity> LoyaltyMissions => DbContext.Set<LoyaltyMissionEntity>();

    public virtual async Task<IList<LoyaltyMissionEntity>> GetLoyaltyMissionsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyMissions.Include(x => x.LocalizedNames).Include(x => x.LocalizedDescriptions).Where(x => x.Id == ids.First()).AsSplitQuery().ToListAsync()
            : await LoyaltyMissions.Include(x => x.LocalizedNames).Include(x => x.LocalizedDescriptions).Where(x => ids.Contains(x.Id)).AsSplitQuery().ToListAsync();
    }

    public IQueryable<LoyaltyMissionGoalItemEntity> LoyaltyMissionGoalItems => DbContext.Set<LoyaltyMissionGoalItemEntity>();

    public virtual async Task<IList<LoyaltyMissionGoalItemEntity>> GetLoyaltyMissionGoalItemsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyMissionGoalItems.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyMissionGoalItems.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public IQueryable<LoyaltyMissionProgressEntity> LoyaltyMissionProgresses => DbContext.Set<LoyaltyMissionProgressEntity>();

    public virtual async Task<IList<LoyaltyMissionProgressEntity>> GetLoyaltyMissionProgressesByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyMissionProgresses.Include(x => x.Items).Where(x => x.Id == ids.First()).AsSplitQuery().ToListAsync()
            : await LoyaltyMissionProgresses.Include(x => x.Items).Where(x => ids.Contains(x.Id)).AsSplitQuery().ToListAsync();
    }

    public IQueryable<LoyaltyMissionTransactionEntity> LoyaltyMissionTransactions => DbContext.Set<LoyaltyMissionTransactionEntity>();

    public virtual async Task<IList<LoyaltyMissionTransactionEntity>> GetLoyaltyMissionTransactionsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyMissionTransactions.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyMissionTransactions.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
