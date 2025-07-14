using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public class LoyaltyProgramRepository(LoyaltyDbContext dbContext, IUnitOfWork unitOfWork = null)
    : DbContextRepositoryBase<LoyaltyDbContext>(dbContext, unitOfWork), ILoyaltyProgramRepository
{
    public IQueryable<LoyaltyProgramEntity> LoyaltyPrograms => DbContext.Set<LoyaltyProgramEntity>();

    public IQueryable<TransactionLogEntity> Transactions => DbContext.Set<TransactionLogEntity>();

    public IQueryable<LoyaltyProgramStoreEntity> LoyaltyProgramStores => DbContext.Set<LoyaltyProgramStoreEntity>();

    public virtual async Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids)
    {
        var result = await LoyaltyPrograms
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        if (result.Count > 0)
        {
            var programIds = result.Select(x => x.Id).ToList();

            await Transactions
                .Where(x => programIds.Contains(x.LoyaltyProgramId))
                .LoadAsync();

            await LoyaltyProgramStores.Where(x => ids.Contains(x.LoyaltyProgramId)).LoadAsync();
        }

        return result;
    }
}
