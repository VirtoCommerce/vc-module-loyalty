using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Loyalty.Data.Models;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public class LoyaltyRepository(LoyaltyDbContext dbContext, IUnitOfWork unitOfWork = null)
    : DbContextRepositoryBase<LoyaltyDbContext>(dbContext, unitOfWork),
        ILoyaltyRepository
{
    public IQueryable<LoyaltyProgramEntity> LoyaltyPrograms => DbContext.Set<LoyaltyProgramEntity>();

    public virtual async Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await LoyaltyPrograms.Where(x => x.Id == ids.First()).ToListAsync()
            : await LoyaltyPrograms.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
