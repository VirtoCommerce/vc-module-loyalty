using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Core.Models;

public class LoyaltyProgramProductFactorSearchCriteria : SearchCriteriaBase
{
    public string LoyaltyProgramId { get; set; }

    public IList<string> ProductIds { get; set; }
}
