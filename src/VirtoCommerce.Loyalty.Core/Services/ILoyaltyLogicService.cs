using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyLogicService
{
    Task<decimal> GetUserBalanceAsync(string userId);

    Task<List<string>> FindProcessedObjectIdsAsync(string objectType, string[] objectIds);

    Task<LoyaltyProgramsEvaluationResult> EvaluateLoyaltyProgramsAsync(LoyaltyProgramEvaluationContext loyaltyContext);

    Task LogLoyaltyProgramOperationAsync(LoyaltyProgramEvaluationContext loyaltyContext, LoyaltyProgramsEvaluationResult loyaltyResult);
}
