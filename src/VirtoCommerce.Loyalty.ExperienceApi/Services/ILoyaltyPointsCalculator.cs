using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.Loyalty.ExperienceApi.Services;

public interface ILoyaltyPointsCalculator
{
    Task<LoyaltyPointsContext> ResolveAsync(string storeId, string userId, string language, string currencyCode, IList<string> productIds);
}
