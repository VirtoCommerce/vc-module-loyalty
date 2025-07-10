using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.LoyaltyProgramSearchService.Core.Models;

public class LoyaltyProgramSearchCriteria : SearchCriteriaBase
{
    public string StoreId { get; set; }

    public int Priority { get; set; }
}
