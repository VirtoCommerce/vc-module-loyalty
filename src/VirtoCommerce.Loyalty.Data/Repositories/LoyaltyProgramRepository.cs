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

    public IQueryable<ConditionEntity> Conditions => DbContext.Set<ConditionEntity>();

    public IQueryable<RewardTypeEntity> RewardTypes => DbContext.Set<RewardTypeEntity>();

    public IQueryable<LoyaltyProgramUserGroupEntity> LoyaltyProgramUserGroups => DbContext.Set<LoyaltyProgramUserGroupEntity>();

    public virtual async Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids)
    {
        var result = await LoyaltyPrograms
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        if (result.Count > 0)
        {
            var programIds = result.Select(x => x.Id).ToList();

            await Conditions
                .Where(x => programIds.Contains(x.LoyaltyProgramId))
                .LoadAsync();

            await RewardTypes
                .Where(x => programIds.Contains(x.LoyaltyProgramId))
                .LoadAsync();

            var conditionIds = Conditions.Select(x => x.Id).ToList();
            await LoyaltyProgramUserGroups
                .Where(x => conditionIds.Contains(x.ConditionId))
                .LoadAsync();
        }

        return result;
    }
}
