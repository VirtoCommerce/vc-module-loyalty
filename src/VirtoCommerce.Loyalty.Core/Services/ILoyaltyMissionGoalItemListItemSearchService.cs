using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyMissionGoalItemListItemSearchService
{
    Task<LoyaltyMissionGoalItemListItemSearchResult> SearchAsync(LoyaltyMissionGoalItemSearchCriteria criteria);
}
