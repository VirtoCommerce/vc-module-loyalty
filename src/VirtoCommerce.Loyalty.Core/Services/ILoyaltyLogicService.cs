using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyLogicService
{
    IAsyncEnumerable<LoyaltyProgram> GetActiveLoyaltyProgramsAsync(string[] storeIds);

    Task<decimal> GetUserBalanceAsync(string userId);

    Task<bool> IsOrderProcessedAsync(string orderId);

    Task<List<string>> FindProcessedOrderIdsAsync(string[] orderIds);

    Task<LoyaltyProgramsEvaluationResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext context);

    Task LogLoyaltyUsageAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyProgramsEvaluationResult loyaltyResult);
}
