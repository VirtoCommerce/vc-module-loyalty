using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyPointsCalculator
{
    Task<LoyaltyPointsContext> ResolveAsync(string storeId, string userId, string language, string currencyCode, IList<string> productIds);
}
