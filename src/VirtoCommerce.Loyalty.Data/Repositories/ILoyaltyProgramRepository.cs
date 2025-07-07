using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Data.Models;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public interface ILoyaltyProgramRepository
{
    IQueryable<LoyaltyProgramEntity> LoyaltyPrograms { get; }
    IQueryable<ConditionEntity> Conditions { get; }
    IQueryable<RewardTypeEntity> RewardTypes { get; }
    IQueryable<LoyaltyProgramUserGroupEntity> LoyaltyProgramUserGroups { get; }

    Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids);
}
