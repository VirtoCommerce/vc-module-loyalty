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
    public IQueryable<LoyaltyProgramUsageEntity> LoyaltyProgramUsages => DbContext.Set<LoyaltyProgramUsageEntity>();

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

    public virtual async Task<IList<LoyaltyProgramUsageEntity>> GetLoyaltyProgramUsageByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyProgramUsages.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyProgramUsages.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
