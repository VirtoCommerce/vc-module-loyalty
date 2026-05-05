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

    IQueryable<LoyaltyProgramOperationLogEntity> LoyaltyProgramOperationLogs { get; }

    Task<IList<LoyaltyProgramOperationLogEntity>> GetLoyaltyProgramOperationLogsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyProgramProductFactorEntity> LoyaltyProgramProductFactors { get; }

    Task<IList<LoyaltyProgramProductFactorEntity>> GetLoyaltyProgramProductFactorsByIdsAsync(IList<string> ids, string responseGroup);
}
