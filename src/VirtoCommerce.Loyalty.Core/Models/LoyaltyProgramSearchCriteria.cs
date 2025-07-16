using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.LoyaltyProgramSearchService.Core.Models;

public class LoyaltyProgramSearchCriteria : SearchCriteriaBase
{
    public string[] StoreIds { get; set; }

    public bool? IsActive { get; set; }
}
