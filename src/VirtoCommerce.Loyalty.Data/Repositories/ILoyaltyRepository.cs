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

    IQueryable<LoyaltyBalanceOperationLogEntity> LoyaltyBalanceOperationLogs { get; }

    Task<IList<LoyaltyBalanceOperationLogEntity>> GetLoyaltyBalanceOperationLogsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyProgramProductFactorEntity> LoyaltyProgramProductFactors { get; }

    Task<IList<LoyaltyProgramProductFactorEntity>> GetLoyaltyProgramProductFactorsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyMissionEntity> LoyaltyMissions { get; }

    Task<IList<LoyaltyMissionEntity>> GetLoyaltyMissionsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyMissionGoalItemEntity> LoyaltyMissionGoalItems { get; }

    Task<IList<LoyaltyMissionGoalItemEntity>> GetLoyaltyMissionGoalItemsByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyMissionProgressEntity> LoyaltyMissionProgresses { get; }

    Task<IList<LoyaltyMissionProgressEntity>> GetLoyaltyMissionProgressesByIdsAsync(IList<string> ids, string responseGroup);

    IQueryable<LoyaltyMissionTransactionEntity> LoyaltyMissionTransactions { get; }

    Task<IList<LoyaltyMissionTransactionEntity>> GetLoyaltyMissionTransactionsByIdsAsync(IList<string> ids, string responseGroup);
}
