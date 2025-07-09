using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Repositories;

public interface ILoyaltyProgramRepository : IRepository
{
    IQueryable<LoyaltyProgramEntity> LoyaltyPrograms { get; }
    IQueryable<TransactionLogEntity> Transactions { get; }

    Task<IList<LoyaltyProgramEntity>> GetLoyaltyProgramsByIdsAsync(IList<string> ids);
}
