using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface ILoyaltyMissionGoalItemSearchService : ISearchService<LoyaltyMissionGoalItemSearchCriteria, LoyaltyMissionGoalItemSearchResult, LoyaltyMissionGoalItem>;
