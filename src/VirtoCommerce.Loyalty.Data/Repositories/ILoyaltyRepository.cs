using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public interface ILoyaltyRepository : IRepository
{
    IQueryable<LoyaltyProgramEntity> LoyaltyPrograms { get; }

    Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyProgramUsageEntity> LoyaltyProgramUsages { get; }

    Task<IList<LoyaltyProgramUsageEntity>> GetLoyaltyProgramUsageByIdsAsync(IList<string> ids, string responseGroup);
}
