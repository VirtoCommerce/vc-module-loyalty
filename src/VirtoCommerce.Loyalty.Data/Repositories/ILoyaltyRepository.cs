using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Loyalty.Data.Models;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public interface ILoyaltyRepository : IRepository
{
    IQueryable<LoyaltyProgramEntity> LoyaltyPrograms { get; }

    Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids, string responseGroup);
}
