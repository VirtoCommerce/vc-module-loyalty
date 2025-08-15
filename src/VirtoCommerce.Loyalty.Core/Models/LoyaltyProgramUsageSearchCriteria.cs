using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramUsageSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }

    public string OrderId { get; set; }

    public string LoyaltyProgramId { get; set; }

    public string UsageType { get; set; }
}
