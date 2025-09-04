using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyLogicService
{
    Task<decimal> GetUserBalanceAsync(string userId);

    Task<LoyaltyBalanceResult> GetLoyaltyBalanceAsync(LoyaltyBalanceRequest request);

    Task<List<string>> FindProcessedObjectIdsAsync(string objectType, string[] objectIds);

    Task<LoyaltyAmountResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext);

    Task<bool> LogLoyaltyProgramOperationAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyAmountResult loyaltyResult);
}
